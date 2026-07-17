using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Coins
{
    /// <summary>
    /// Query to retrieve a student's current coin balance and paginated transaction history.
    /// Authorization is enforced inside the handler (not via a route policy):
    /// Students may only query their own coins; Admin/Instructor may query any student.
    /// </summary>
    /// <param name="StudentId">The student whose coin data is requested (from the route).</param>
    /// <param name="Page">1-based page number for the transaction history list.</param>
    /// <param name="PageSize">Number of transactions per page.</param>
    public record GetStudentCoinsQuery(
        Guid StudentId,
        int Page = 1,
        int PageSize = 20
    ) : IRequest<EndpointResponse<StudentCoinsDto>>;
}
