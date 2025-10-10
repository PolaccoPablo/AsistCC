namespace SaacACC.BlazorWasm.Services
{
    public interface IAuthService
    {
        Task<LoginResponse> Login(LoginRequest loginRequest);
        Task Logout();
        Task<bool> IsUserAuthenticated();
        Task<string> GetUserRole();
        Task<int?> GetComercioId();
        string? GetToken();
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? ComercioId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}