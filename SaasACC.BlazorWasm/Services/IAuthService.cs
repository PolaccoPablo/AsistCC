using SaasACC.Model.Servicios.Login;

namespace SaacACC.BlazorWasm.Services
{
    public interface IAuthService
    {
        Task<LoginResponse> Login(LoginRequest loginRequest);
        Task<RegisterResponse> RegisterComercio(RegisterComercioRequest request);
        Task<RegisterResponse> RegisterCliente(RegisterClienteRequest request);
        Task<ChangePasswordResponse> ChangePassword(ChangePasswordRequest request);
        Task Logout(bool sessionExpired = false);
        Task CheckSessionAsync();
        Task<bool> IsUserAuthenticated();
        Task<string> GetUserRole();
        Task<int?> GetComercioId();
        string? GetToken();
    }
}