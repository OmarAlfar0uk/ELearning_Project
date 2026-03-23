using MediatR;

namespace ELearningProject.Features.Auth.UpdateUserProfile
{
    public record UpdateUserProfileCommand(
     Guid UserId,
     string? FirstName,
     string? LastName,
     string? PhoneNumber,
     string? ProfileImage
 ) : IRequest<UpdateUserProfileResponse>;
}
