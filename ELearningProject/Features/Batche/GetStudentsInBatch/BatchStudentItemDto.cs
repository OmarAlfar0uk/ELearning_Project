
namespace ELearningProject.Features.Batche.GetStudentsInBatch
{
    public record BatchStudentItemDto(
        Guid StudentId,
        string FullName,
        string Email,
        int Rank,
        double AverageScore
    );
}
