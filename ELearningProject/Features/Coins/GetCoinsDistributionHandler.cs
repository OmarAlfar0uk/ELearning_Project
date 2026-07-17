using Auth.Models;
using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Coins
{
    /// <summary>
    /// Handler for <see cref="GetCoinsDistributionQuery"/>.
    /// Aggregates three metrics in one database round-trip per query:
    /// total coins distributed, the top 10 students by balance, and the 20 most recent transactions.
    /// Role-gating (Admin / SuperAdmin / Instructor) is enforced at the endpoint level.
    /// </summary>
    public class GetCoinsDistributionHandler : IRequestHandler<GetCoinsDistributionQuery, EndpointResponse<CoinsDistributionDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public GetCoinsDistributionHandler(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<EndpointResponse<CoinsDistributionDto>> Handle(
            GetCoinsDistributionQuery request,
            CancellationToken cancellationToken)
        {
            var txRepo = _unitOfWork.GetRepository<CoinTransaction>();

            // ── 1. Total coins distributed across all students ─────────────────────
            var totalDistributed = await txRepo
                .GetAll()
                .SumAsync(tx => tx.Amount, cancellationToken);

            // ── 2. Top 10 students by CoinBalance ─────────────────────────────────
            // We query the Users table via UserManager to respect the soft-delete
            // filter already in place on ApplicationUser, then order in-process on
            // the small result set (at most all students, typically manageable).
            // A direct EF query on the Users DbSet via IUnitOfWork is used here
            // because UserManager doesn't expose ordering/projection natively.
            var userRepo = _unitOfWork.GetRepository<ApplicationUser>();
            var topStudents = await userRepo
                .GetAll()
                .OrderByDescending(u => u.CoinBalance)
                .Take(10)
                .Select(u => new CoinTopStudentDto(
                    u.Id,
                    u.FirstName + " " + u.LastName,
                    u.CoinBalance))
                .ToListAsync(cancellationToken);

            // ── 3. Most recent 20 transactions system-wide ────────────────────────
            var recentTransactions = await txRepo
                .GetAll()
                .Include(tx => tx.GrantedByAdmin)
                .OrderByDescending(tx => tx.CreatedAt)
                .Take(20)
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

            // ── 4. Compose and return ─────────────────────────────────────────────
            var dto = new CoinsDistributionDto(
                totalDistributed,
                topStudents,
                recentTransactions);

            return EndpointResponse<CoinsDistributionDto>.SuccessResponse(
                dto,
                "Coins distribution retrieved successfully.");
        }
    }
}
