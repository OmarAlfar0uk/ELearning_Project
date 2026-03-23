namespace ELearningProject.Contracts
{
    public interface IAuditLogger
    {
        Task LogAsync(
            string action,
            string? userId = null,
            string? targetId = null,
            string? description = null);
    }
}
