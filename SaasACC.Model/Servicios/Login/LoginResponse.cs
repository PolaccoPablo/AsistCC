namespace SaasACC.Model.Servicios.Login;

public class LoginResponse
{
    public bool Success { get; set; }
    public string Token { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? ComercioId { get; set; } // Para Admin/UsuarioComercio
    public string UserName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public bool UserNotFound { get; set; } = false;
    public bool RequiereCambioPassword { get; set; } = false;

    // Para clientes con múltiples comercios
    public List<ComercioInfo>? Comercios { get; set; }
}

public class ComercioInfo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}
