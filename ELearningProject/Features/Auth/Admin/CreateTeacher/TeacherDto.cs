namespace ELearningProject.Features.Auth.Admin.CreateTeacher
{
    /// <summary>
    /// Data Transfer Object representing the details of the created teacher.
    /// </summary>
    public class TeacherDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public List<string> AssignedTrackNames { get; set; } = new();
    }
}
