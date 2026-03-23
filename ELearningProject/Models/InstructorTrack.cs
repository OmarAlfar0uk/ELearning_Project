
using Auth.Models;

namespace ELearningProject.Models
{
    public class InstructorTrack : Auth.Models.BaseEntity
    {
        public Guid InstructorId { get; set; }
        public ApplicationUser Instructor { get; set; } = default!;

        public Guid TrackId { get; set; }
        public Track Track { get; set; } = default!;
    }
}
