namespace ELearningProject.Features.Auth.Activate
{
    public class ActivateResponse
    {
        public string AccessToken { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
        public string Role { get; set; } = default!;
    }
}
