namespace SaasACC.Model.Servicios.Login;

public class RegisterResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public int? ComercioId { get; set; }
    public int? ClienteId { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
