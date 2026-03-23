namespace ELearningProject.Features.Auth.GetCurrentUser
{
    public class CurrentUserResponse
    {
        public Guid UserId { get; set; } = default!;
        public string UserName { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? PhoneNumber { get; set; }
        public string? ProfileImageUrl { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
