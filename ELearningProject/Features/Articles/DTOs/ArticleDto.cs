namespace ELearningProject.Features.Articles.DTOs
{
    public record ArticleDto(
        Guid Id,
        string Title,
        string Content,
        string? ImageUrl,
        string AuthorName,
        DateTime CreatedAt
    );
}
