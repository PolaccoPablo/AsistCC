namespace SaasACC.Model.Servicios.Login;

public class LoginResponse
{
    public bool Success { get; set; }
    public string Token { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? ComercioId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public bool UserNotFound { get; set; } = false; // Nuevo campo para manejar usuario no encontrado
    public bool RequiereCambioPassword { get; set; } = false; // Indica si debe cambiar la contraseña (primer login de cliente creado por admin)
}
