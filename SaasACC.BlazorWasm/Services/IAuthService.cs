using SaasACC.Model.Servicios.Login;

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
}