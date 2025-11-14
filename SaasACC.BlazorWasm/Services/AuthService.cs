using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SaasACC.Model.Servicios.Login;

namespace SaacACC.BlazorWasm.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authStateProvider;
        private const string TOKEN_KEY = "authToken";
        private const string ROLE_KEY = "userRole";
        private const string COMERCIO_KEY = "comercioId";

        public AuthService(HttpClient httpClient,
                          ILocalStorageService localStorage,
                          AuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;
        }

        public async Task<LoginResponse> Login(LoginRequest loginRequest)
        {
            try
            {
                var json = JsonSerializer.Serialize(loginRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/auth/login", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (loginResponse?.Success == true)
                {
                    await _localStorage.SetItemAsync(TOKEN_KEY, loginResponse.Token);
                    await _localStorage.SetItemAsync(ROLE_KEY, loginResponse.Role);
                    if (loginResponse.ComercioId.HasValue)
                        await _localStorage.SetItemAsync(COMERCIO_KEY, loginResponse.ComercioId.Value);

                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", loginResponse.Token);

                    ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(loginResponse.Token);
                }

                return loginResponse ?? new LoginResponse { Success = false, ErrorMessage = "Error de conexión" };
            }
            catch (Exception ex)
            {
                return new LoginResponse { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<RegisterResponse> RegisterComercio(RegisterComercioRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/auth/register/comercio", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                var registerResponse = JsonSerializer.Deserialize<RegisterResponse>(responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (registerResponse?.Success == true && !string.IsNullOrEmpty(registerResponse.Token))
                {
                    await _localStorage.SetItemAsync(TOKEN_KEY, registerResponse.Token);
                    await _localStorage.SetItemAsync(ROLE_KEY, "Admin");
                    if (registerResponse.ComercioId.HasValue)
                        await _localStorage.SetItemAsync(COMERCIO_KEY, registerResponse.ComercioId.Value);

                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", registerResponse.Token);

                    ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(registerResponse.Token);
                }

                return registerResponse ?? new RegisterResponse { Success = false, ErrorMessage = "Error de conexión" };
            }
            catch (Exception ex)
            {
                return new RegisterResponse { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<RegisterResponse> RegisterCliente(RegisterClienteRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/auth/register/cliente", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                var registerResponse = JsonSerializer.Deserialize<RegisterResponse>(responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return registerResponse ?? new RegisterResponse { Success = false, ErrorMessage = "Error de conexión" };
            }
            catch (Exception ex)
            {
                return new RegisterResponse { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync(TOKEN_KEY);
            await _localStorage.RemoveItemAsync(ROLE_KEY);
            await _localStorage.RemoveItemAsync(COMERCIO_KEY);

            _httpClient.DefaultRequestHeaders.Authorization = null;
            ((CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();
        }

        public async Task<bool> IsUserAuthenticated()
        {
            var token = await _localStorage.GetItemAsync<string>(TOKEN_KEY);
            return !string.IsNullOrEmpty(token);
        }

        public async Task<string> GetUserRole()
        {
            return await _localStorage.GetItemAsync<string>(ROLE_KEY) ?? "";
        }

        public async Task<int?> GetComercioId()
        {
            return await _localStorage.GetItemAsync<int?>(COMERCIO_KEY);
        }

        public string? GetToken()
        {
            return _localStorage.GetItemAsync<string>(TOKEN_KEY).Result;
        }
    }
}