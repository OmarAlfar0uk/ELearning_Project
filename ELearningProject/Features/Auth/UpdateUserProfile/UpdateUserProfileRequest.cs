namespace ELearningProject.Features.Auth.UpdateUserProfile
{
    public class UpdateUserProfileRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public IFormFile? ProfileImage { get; set; }
    }
}
