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
    /// Handler for <see cref="GetStudentCoinsQuery"/>.
    /// Enforces per-handler authorization:
    /// <list type="bullet">
    ///   <item>Student role → may only retrieve their own coins (403 if StudentId ≠ caller).</item>
    ///   <item>Admin / SuperAdmin role → may retrieve any student's coins.</item>
    ///   <item>Instructor role → may retrieve any student's coins for now.
    ///     <para>
    ///       TODO (Stage 2): Tighten this to "only students enrolled in one of this instructor's
    ///       tracks". This requires resolving "does instructor X have student Y in any of their
    ///       tracks?", which is a different shape from the existing <c>TrackOwnership</c> policy
    ///       (that policy resolves a <c>trackId</c>/<c>lectureId</c>/<c>examId</c> from the route,
    ///       but this route only carries <c>studentId</c>). Open question for product review before
    ///       Stage 2 implementation.
    ///     </para>
    ///   </item>
    /// </list>
    /// </summary>
    public class GetStudentCoinsHandler : IRequestHandler<GetStudentCoinsQuery, EndpointResponse<StudentCoinsDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public GetStudentCoinsHandler(
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public async Task<EndpointResponse<StudentCoinsDto>> Handle(
            GetStudentCoinsQuery request,
            CancellationToken cancellationToken)
        {
            // ── 1. Resolve caller identity ────────────────────────────────────────
            var callerPrincipal = _httpContextAccessor.HttpContext?.User;
            var callerId = callerPrincipal?.GetUserId() ?? Guid.Empty;
            var callerRole = callerPrincipal?.GetUserRole() ?? string.Empty;

            if (callerId == Guid.Empty)
            {
                return EndpointResponse<StudentCoinsDto>.UnauthorizedResponse();
            }

            // ── 2. Per-handler authorization ──────────────────────────────────────
            // Students may only see their own coin history.
            if (callerRole == "Student" && callerId != request.StudentId)
            {
                return EndpointResponse<StudentCoinsDto>.ErrorResponse(
                    "Students may only view their own coin history.",
                    403);
            }

            // ── 3. Verify the target student exists ───────────────────────────────
            var student = await _userManager.FindByIdAsync(request.StudentId.ToString());
            if (student == null || student.IsDeleted)
            {
                return EndpointResponse<StudentCoinsDto>.NotFoundResponse("Student not found.");
            }

            // ── 4. Fetch paginated transaction history (newest first) ──────────────
            var txRepo = _unitOfWork.GetRepository<CoinTransaction>();

            var page = Math.Max(1, request.Page);
            var pageSize = Math.Max(1, Math.Min(100, request.PageSize));

            var history = await txRepo
                .FindByCondition(tx => tx.StudentId == request.StudentId)
                .Include(tx => tx.GrantedByAdmin)
                .OrderByDescending(tx => tx.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(tx => new CoinTransactionDto(
                    tx.Id,
                    tx.Amount,
                    tx.Reason,
                    tx.Source,
                    tx.GrantedByAdmin != null
                        ? tx.GrantedByAdmin.FirstName + " " + tx.GrantedByAdmin.LastName
                        : null,
                    tx.CreatedAt))
                .ToListAsync(cancellationToken);

            // ── 5. Build the response DTO ─────────────────────────────────────────
            var dto = new StudentCoinsDto(
                student.Id,
                student.FullName,
                student.CoinBalance,
                history);

            return EndpointResponse<StudentCoinsDto>.SuccessResponse(
                dto,
                "Student coins retrieved successfully.");
        }
    }
}
