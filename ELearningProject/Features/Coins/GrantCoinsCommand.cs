using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Coins
{
    /// <summary>
    /// Command to manually grant coins to a student. Issued by an Admin, SuperAdmin, or Instructor.
    /// </summary>
    /// <param name="StudentId">The target student's identifier (set from the route parameter).</param>
    /// <param name="Amount">Number of coins to credit; must be greater than zero.</param>
    /// <param name="Reason">Human-readable explanation for the grant.</param>
    public record GrantCoinsCommand(
        Guid StudentId,
        int Amount,
        string Reason
    ) : IRequest<EndpointResponse<CoinTransactionDto>>;
}
