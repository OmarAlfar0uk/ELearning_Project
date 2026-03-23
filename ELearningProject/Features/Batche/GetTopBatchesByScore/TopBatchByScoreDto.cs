namespace ELearningProject.Features.Batche.GetTopBatchesByScore
{
    public record TopBatchByScoreDto(
        Guid BatchId,
        string BatchName,
        double AverageScore,
        int StudentCount
    );
}
