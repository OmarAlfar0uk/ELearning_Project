
namespace ELearningProject.Contracts
{
    public interface IBaseEntity
    {
        Guid Id { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime? UpdatedAt { get; set; }
        bool IsDeleted { get; set; }
    }
}
