namespace ELearningProject.Features.Auth.Admin.CreateStudent
{
    public class CreateStudentResponse
    {
        public Guid StudentId { get; set; }
        public string Email { get; set; } = default!;
        public string ActivationCode { get; set; } = default!;
    }
}
