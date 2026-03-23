namespace ELearningProject.Features.Auth.Admin.Create
{
    public class CreateAdminResponse
    {
        public Guid AdminId { get; set; }
        public string Email { get; set; } = default!;
        public string Role { get; set; } = "Admin";
    }
}
