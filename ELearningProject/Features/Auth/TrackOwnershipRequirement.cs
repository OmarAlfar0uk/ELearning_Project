using Microsoft.AspNetCore.Authorization;

namespace ELearningProject.Features.Auth
{
    /// <summary>
    /// Requirement for checking whether the current user has ownership (is assigned to) a specific track.
    /// </summary>
    public class TrackOwnershipRequirement : IAuthorizationRequirement
    {
    }
}
