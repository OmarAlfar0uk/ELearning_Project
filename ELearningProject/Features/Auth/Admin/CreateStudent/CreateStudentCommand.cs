using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Auth.Admin.CreateStudent
{
    public record CreateStudentCommand(
         string Email,
         string FirstName,
         string LastName,
         string Gender,
         Guid BatchId
     ) : IRequest<EndpointResponse<CreateStudentResponse>>;
}
