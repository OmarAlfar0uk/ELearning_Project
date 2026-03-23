namespace ELearningProject.Features.Batche.GetAllBatches
{
    public record BatchDto(Guid Id, string Name, DateTime StartDate, int StudentCount, int TrackCount);
}
