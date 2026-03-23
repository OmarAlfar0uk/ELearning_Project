namespace ELearningProject.Features.Auth.ChangeUserRole
{
    public class ChangeUserRoleRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
