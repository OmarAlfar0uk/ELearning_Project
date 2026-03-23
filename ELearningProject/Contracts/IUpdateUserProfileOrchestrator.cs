using ELearningProject.Features.Auth.UpdateUserProfile;

namespace ELearningProject.Contracts
{
    public interface IUpdateUserProfileOrchestrator
    {
        Task<UpdateUserProfileResponse> UpdateUserProfileAsync(
            Guid userId,
            UpdateUserProfileRequest request
        );
    }
}
