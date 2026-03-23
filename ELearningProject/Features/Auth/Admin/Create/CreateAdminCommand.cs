using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Auth.Admin.Create
{
    public class CreateAdminCommand : IRequest<EndpointResponse<CreateAdminResponse>>
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
    }
}
