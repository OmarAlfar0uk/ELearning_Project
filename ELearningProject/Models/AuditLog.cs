namespace ELearningProject.Models
{
    public class AuditLog
    {

        public Guid Id { get; set; }
        public string Action { get; set; } = default!;
        public string? UserId { get; set; }
        public string? TargetId { get; set; }
        public string? Description { get; set; }
        public string? IpAddress { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
