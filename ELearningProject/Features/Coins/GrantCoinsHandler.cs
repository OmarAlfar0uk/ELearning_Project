using Auth.Models;
using ELearningProject.Contarcts;
using ELearningProject.Extensions;
using ELearningProject.Features.Coins.Services;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace ELearningProject.Features.Coins
{
    /// <summary>
    /// Handler for <see cref="GrantCoinsCommand"/>.
    /// Enforces the following authorization rules before creating the ledger entry:
    /// <list type="bullet">
    ///   <item>Admin / SuperAdmin: may grant coins to any student.</item>
    ///   <item>Instructor: may only grant coins to students who share at least one
    ///     batch with one of the instructor's assigned tracks
    ///     (resolved via <see cref="IStudentAccessService"/>). Returns 403 otherwise.</item>
    /// </list>
    /// When access is permitted, the handler atomically inserts a <see cref="CoinTransaction"/>
    /// and increments <see cref="ApplicationUser.CoinBalance"/> in a single
    /// <c>SaveChangesAsync</c> call so the two writes are always consistent.
    /// </summary>
    public class GrantCoinsHandler : IRequestHandler<GrantCoinsCommand, EndpointResponse<CoinTransactionDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStudentAccessService _studentAccessService;

        public GrantCoinsHandler(
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager,
            IStudentAccessService studentAccessService)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _studentAccessService = studentAccessService;
        }

        public async Task<EndpointResponse<CoinTransactionDto>> Handle(
            GrantCoinsCommand request,
            CancellationToken cancellationToken)
        {
            // ── 1. Resolve the calling admin/instructor's identity ─────────────────
            var callerPrincipal = _httpContextAccessor.HttpContext?.User;
            var grantedByAdminId = callerPrincipal?.GetUserId() ?? Guid.Empty;
            var callerRole = callerPrincipal?.GetUserRole() ?? string.Empty;

            if (grantedByAdminId == Guid.Empty)
            {
                return EndpointResponse<CoinTransactionDto>.UnauthorizedResponse();
            }

            // ── 2. Verify the target student exists ───────────────────────────────
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

            // ── 4. Track-scoped access check for Instructors ───────────────────────
            // Admin and SuperAdmin: IStudentAccessService returns true without a DB hit.
            // Instructor: returns true only if the student shares a batch with one of
            //   the instructor's assigned tracks (InstructorTracks → Track.BatchId → BatchStudents).
            var hasAccess = await _studentAccessService.CanAccessStudentAsync(
                callerRole,
                grantedByAdminId,
                request.StudentId,
                cancellationToken);

            if (!hasAccess)
            {
                return EndpointResponse<CoinTransactionDto>.ErrorResponse(
                    "Instructors may only grant coins to students within their assigned tracks.",
                    403);
            }

            // ── 5. Build the CoinTransaction ──────────────────────────────────────
            var transaction = new CoinTransaction
            {
                StudentId        = request.StudentId,
                Amount           = request.Amount,
                Reason           = request.Reason,
                Source           = CoinSource.AdminGrant,
                GrantedByAdminId = grantedByAdminId
            };

            // ── 6. Increment the student's running balance ────────────────────────
            // ATOMICITY: Both the CoinTransaction insert and the CoinBalance increment
            // are staged on the same DbContext change-tracker and flushed together in
            // the single SaveChangesAsync below.  They are committed in one implicit
            // database transaction and can never diverge.
            student.CoinBalance += request.Amount;

            // Stage the new transaction row.
            var transactionRepo = _unitOfWork.GetRepository<CoinTransaction>();
            await transactionRepo.CreateAsync(transaction);

            // Stage the updated balance.  UserManager.UpdateAsync would open its own
            // separate transaction, so we use the shared EF Update path instead.
            var userRepo = _unitOfWork.GetRepository<ApplicationUser>();
            userRepo.Update(student);

            // ── 7. Single SaveChangesAsync — both writes are committed atomically ──
            await _unitOfWork.SaveChangesAsync();

            // ── 8. Resolve granter name for the DTO (best-effort, non-blocking) ───
            var granter = await _userManager.FindByIdAsync(grantedByAdminId.ToString());
            var granterName = granter is not null
                ? $"{granter.FirstName} {granter.LastName}"
                : null;

            // ── 9. Build and return the response DTO ─────────────────────────────
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
