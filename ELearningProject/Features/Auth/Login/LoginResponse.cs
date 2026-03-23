namespace ELearningProject.Features.Auth.Login
{
    public class LoginResponse
    {
        public string AccessToken { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
        public List<string> Roles { get; set; } = new();
    }
}
