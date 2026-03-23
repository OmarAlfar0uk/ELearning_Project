using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Auth.Activate
{
    public class ActivateCommand : IRequest<EndpointResponse<ActivateResponse>>
    {
        public string Code { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}
