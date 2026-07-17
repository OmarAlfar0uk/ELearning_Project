using Auth.Models;
using ELearningProject.Contarcts;
using ELearningProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace ELearningProject.Features.Coins.Services
{
    /// <summary>
    /// Handles automatic coin-award triggers for Stage 2.
    /// Each method is intentionally fire-and-forget: callers wrap the invocation in
    /// try/catch so that a coin-award failure never rolls back the originating action
    /// (exam submission, manual grading, assignment submission).
    /// </summary>
    public interface ICoinAwardService
    {
        /// <summary>
        /// Awards coins to the student who owns <paramref name="attemptId"/> if:
        /// <list type="bullet">
        ///   <item>The attempt's <c>Status == Graded</c> and <c>Score</c> is not null.</item>
        ///   <item><c>Score / TotalExamPoints &gt;= 0.60</c> (passing threshold).</item>
        ///   <item>No <see cref="CoinTransaction"/> already references this attempt
        ///     (idempotency guard via <see cref="CoinTransaction.RelatedExamAttemptId"/>).</item>
        /// </list>
        /// Coin amount = <c>round(ratio × 20)</c>, <see cref="CoinSource.ExamPassed"/>.
        /// </summary>
        Task AwardExamCoinsIfEligibleAsync(Guid attemptId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Awards a flat 10 coins to the student who owns <paramref name="submissionId"/> if:
        /// <list type="bullet">
        ///   <item>The submission was created on or before the parent assignment's <c>DueDate</c>
        ///     (or <c>DueDate</c> is null — no deadline, always on-time).</item>
        ///   <item>No <see cref="CoinTransaction"/> already references this submission
        ///     (idempotency guard via <see cref="CoinTransaction.RelatedSubmissionId"/>).</item>
        /// </list>
        /// Coin amount = 10, <see cref="CoinSource.AssignmentOnTime"/>.
        /// </summary>
        Task AwardAssignmentCoinsIfEligibleAsync(Guid submissionId, CancellationToken cancellationToken = default);
    }

    /// <inheritdoc />
    public class CoinAwardService : ICoinAwardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public CoinAwardService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        // ────────────────────────────────────────────────────────────────────────
        // EXAM PASSED
        // ────────────────────────────────────────────────────────────────────────

        /// <inheritdoc />
        public async Task AwardExamCoinsIfEligibleAsync(
            Guid attemptId,
            CancellationToken cancellationToken = default)
        {
            // ── 1. Load the attempt with Exam→Questions (needed for TotalExamPoints) ─
            var attemptRepo = _unitOfWork.GetRepository<ExamAttempt>();
            var attempt = await attemptRepo
                .FindByCondition(a => a.Id == attemptId)
                .Include(a => a.Exam)
                    .ThenInclude(e => e.Questions)
                .FirstOrDefaultAsync(cancellationToken);

            if (attempt is null) return;

            // ── 2. Must be fully graded with a score ─────────────────────────────
            if (attempt.Status != AttemptStatus.Graded || !attempt.Score.HasValue) return;

            // ── 3. Compute ratio and enforce 60 % threshold ──────────────────────
            var totalPoints = attempt.Exam.Questions.Sum(q => q.Points);
            if (totalPoints <= 0) return;

            var ratio = (double)attempt.Score.Value / totalPoints;
            if (ratio < 0.60) return;

            // ── 4. Idempotency guard — reject if a transaction already exists ────
            var txRepo = _unitOfWork.GetRepository<CoinTransaction>();
            var alreadyAwarded = await txRepo
                .FindByCondition(tx => tx.RelatedExamAttemptId == attemptId)
                .AnyAsync(cancellationToken);
            if (alreadyAwarded) return;

            // ── 5. Calculate coin amount: round(ratio × 20) ──────────────────────
            var coinAmount = (int)Math.Round(ratio * 20, MidpointRounding.AwayFromZero);
            if (coinAmount <= 0) return;

            // ── 6. Load student for balance update (UserManager gives tracked entity)
            var student = await _userManager.FindByIdAsync(attempt.StudentId.ToString());
            if (student is null || student.IsDeleted) return;

            // ── 7. Stage both writes on the shared DbContext ─────────────────────
            // ATOMICITY: the CoinTransaction INSERT and the CoinBalance UPDATE share
            // the same SaveChangesAsync call — committed as one implicit transaction.
            var transaction = new CoinTransaction
            {
                StudentId            = attempt.StudentId,
                Amount               = coinAmount,
                Reason               = $"Passed exam '{attempt.Exam.Title}' " +
                                       $"({attempt.Score}/{totalPoints} pts, {ratio:P0}).",
                Source               = CoinSource.ExamPassed,
                RelatedExamAttemptId = attemptId
                // GrantedByAdminId intentionally null — system-generated
            };

            await txRepo.CreateAsync(transaction);

            student.CoinBalance += coinAmount;
            var userRepo = _unitOfWork.GetRepository<ApplicationUser>();
            userRepo.Update(student);

            // ── 8. Single SaveChangesAsync ────────────────────────────────────────
            await _unitOfWork.SaveChangesAsync();

            Log.Information(
                "CoinAward | ExamPassed | AttemptId={AttemptId} | StudentId={StudentId} | " +
                "Coins={CoinAmount} | Score={Score}/{TotalPoints}",
                attemptId, attempt.StudentId, coinAmount, attempt.Score, totalPoints);
        }

        // ────────────────────────────────────────────────────────────────────────
        // ASSIGNMENT ON-TIME
        // ────────────────────────────────────────────────────────────────────────

        /// <inheritdoc />
        public async Task AwardAssignmentCoinsIfEligibleAsync(
            Guid submissionId,
            CancellationToken cancellationToken = default)
        {
            // ── 1. Load submission with Assignment (needed for DueDate) ──────────
            var submissionRepo = _unitOfWork.GetRepository<Submission>();
            var submission = await submissionRepo
                .FindByCondition(s => s.Id == submissionId)
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(cancellationToken);

            if (submission is null) return;

            // ── 2. On-time check: CreatedAt must be <= DueDate ───────────────────
            // If DueDate is null there is no deadline — always treated as on-time.
            // NOTE: SubmitAssignmentHandler already blocks late submissions (DueDate < UtcNow),
            // so this guard primarily protects against future non-handler call sites.
            if (submission.Assignment.DueDate.HasValue &&
                submission.CreatedAt > submission.Assignment.DueDate.Value)
                return;

            // ── 3. Idempotency guard ─────────────────────────────────────────────
            var txRepo = _unitOfWork.GetRepository<CoinTransaction>();
            var alreadyAwarded = await txRepo
                .FindByCondition(tx => tx.RelatedSubmissionId == submissionId)
                .AnyAsync(cancellationToken);
            if (alreadyAwarded) return;

            // ── 4. Load student for balance update ───────────────────────────────
            var student = await _userManager.FindByIdAsync(submission.StudentId.ToString());
            if (student is null || student.IsDeleted) return;

            // ── 5. Stage both writes atomically ──────────────────────────────────
            const int OnTimeBonus = 10;

            var transaction = new CoinTransaction
            {
                StudentId           = submission.StudentId,
                Amount              = OnTimeBonus,
                Reason              = $"Assignment '{submission.Assignment.Title}' submitted on time.",
                Source              = CoinSource.AssignmentOnTime,
                RelatedSubmissionId = submissionId
            };

            await txRepo.CreateAsync(transaction);

            student.CoinBalance += OnTimeBonus;
            var userRepo = _unitOfWork.GetRepository<ApplicationUser>();
            userRepo.Update(student);

            // ── 6. Single SaveChangesAsync ────────────────────────────────────────
            await _unitOfWork.SaveChangesAsync();

            Log.Information(
                "CoinAward | AssignmentOnTime | SubmissionId={SubmissionId} | " +
                "StudentId={StudentId} | Coins={CoinAmount}",
                submissionId, submission.StudentId, OnTimeBonus);
        }
    }
}
