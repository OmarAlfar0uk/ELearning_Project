using Auth.Models;

namespace ELearningProject.Models
{
    public class Notification : BaseEntity
    {
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = default!;

        public string Title { get; set; } = default!;
        public string Message { get; set; } = default!;

        public string? ActionUrl { get; set; }   // لينك يودّي على صفحة
        public bool IsRead { get; set; } = false;

        public NotificationType Type { get; set; }
    }
}
