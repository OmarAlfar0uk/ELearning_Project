namespace ELearningProject.Features.Auth.UpdateUserProfile
{
    public record UpdateUserProfileResponse
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public Guid UserId { get; init; }
        public string UserName { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string lastName { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public string? ProfileImageUrl { get; init; }
        public IEnumerable<string>? Roles { get; init; }
        public string Token { get; init; } = string.Empty;
        public string RefreshToken { get; init; } = string.Empty;
    }
}
