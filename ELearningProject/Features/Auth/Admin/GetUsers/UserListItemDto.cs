namespace ELearningProject.Features.Auth.Admin.GetUsers
{
    public class UserListItemDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public bool IsActivated { get; set; }
        public List<string> Roles { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }
}
