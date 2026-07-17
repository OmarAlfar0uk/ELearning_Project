using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Coins
{
    /// <summary>
    /// Query to retrieve a system-wide coins distribution snapshot.
    /// Restricted to Admin, SuperAdmin, and Instructor roles (enforced at the route level).
    /// Returns total coins distributed, the top 10 students by balance, and the 20 most recent transactions.
    /// </summary>
    public record GetCoinsDistributionQuery() : IRequest<EndpointResponse<CoinsDistributionDto>>;
}
