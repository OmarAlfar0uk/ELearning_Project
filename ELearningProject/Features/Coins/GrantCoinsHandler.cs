using Auth.Models;
using ELearningProject.Contarcts;
using ELearningProject.Extensions;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Coins
{
    /// <summary>
    /// Handler for <see cref="GrantCoinsCommand"/>.
    /// Verifies the target student exists and is in the Student role, then atomically
    /// inserts a <see cref="CoinTransaction"/> and increments the student's
    /// <see cref="ApplicationUser.CoinBalance"/> in a single <c>SaveChangesAsync</c> call.
    /// </summary>
    public class GrantCoinsHandler : IRequestHandler<GrantCoinsCommand, EndpointResponse<CoinTransactionDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public GrantCoinsHandler(
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public async Task<EndpointResponse<CoinTransactionDto>> Handle(
            GrantCoinsCommand request,
            CancellationToken cancellationToken)
        {
            // ── 1. Resolve the calling admin/instructor's identity ─────────────────
            var callerPrincipal = _httpContextAccessor.HttpContext?.User;
            var grantedByAdminId = callerPrincipal?.GetUserId() ?? Guid.Empty;

            if (grantedByAdminId == Guid.Empty)
            {
                return EndpointResponse<CoinTransactionDto>.UnauthorizedResponse();
            }

            // ── 2. Verify the target student exists (with tracking so we can mutate) ─
            // We use the UserManager here because ApplicationUser is managed by Identity
            // and we need both existence check and role verification in one shot.
            var student = await _userManager.FindByIdAsync(request.StudentId.ToString());

            if (student == null || student.IsDeleted)
            {
                return EndpointResponse<CoinTransactionDto>.NotFoundResponse(
                    "Student not found.");
            }

            // ── 3. Verify the target user is actually in the Student role ──────────
            var isStudent = await _userManager.IsInRoleAsync(student, "Student");
            if (!isStudent)
            {
                return EndpointResponse<CoinTransactionDto>.ErrorResponse(
                    "The specified user is not a Student.",
                    400);
            }

            // ── 4. Build the CoinTransaction ──────────────────────────────────────
            var transaction = new CoinTransaction
            {
                StudentId      = request.StudentId,
                Amount         = request.Amount,
                Reason         = request.Reason,
                Source         = CoinSource.AdminGrant,
                GrantedByAdminId = grantedByAdminId
            };

            // ── 5. Increment the student's running balance ────────────────────────
            // ATOMICITY NOTE: Both the CoinTransaction insert and the CoinBalance
            // increment are staged against the same DbContext change-tracker and
            // flushed together in the single SaveChangesAsync call below.
            // This guarantees the two writes are committed in one database round-trip
            // (single transaction) — they cannot diverge.
            student.CoinBalance += request.Amount;

            // Stage the new transaction row.
            var transactionRepo = _unitOfWork.GetRepository<CoinTransaction>();
            await transactionRepo.CreateAsync(transaction);

            // Stage the updated balance.  UserManager.UpdateAsync would open a
            // separate transaction, so we use the EF Update path instead so both
            // changes share the same SaveChangesAsync call.
            var userRepo = _unitOfWork.GetRepository<ApplicationUser>();
            userRepo.Update(student);

            // ── 6. Single SaveChangesAsync — both writes are committed atomically ──
            await _unitOfWork.SaveChangesAsync();

            // ── 7. Resolve granter name for the DTO (best-effort, non-blocking) ───
            var granter = await _userManager.FindByIdAsync(grantedByAdminId.ToString());
            var granterName = granter is not null
                ? $"{granter.FirstName} {granter.LastName}"
                : null;

            // ── 8. Build and return the response DTO ─────────────────────────────
            var dto = new CoinTransactionDto(
                transaction.Id,
                transaction.Amount,
                transaction.Reason,
                transaction.Source,
                granterName,
                transaction.CreatedAt);

            return EndpointResponse<CoinTransactionDto>.SuccessResponse(
                dto,
                "Coins granted successfully.",
                201);
        }
    }
}
