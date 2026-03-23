namespace ELearningProject.Contracts
{
    public interface IAuthCreated
    {
        Guid UserId { get; }
        string Email { get; }
        string UserName { get; }
        string Role { get; }
        DateTime CreatedAt { get; }
    }
}
