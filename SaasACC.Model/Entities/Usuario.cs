namespace SaasACC.Model.Entities;

public class Usuario : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Rol { get; set; } = "Usuario"; // Admin, Usuario
    public int ComercioId { get; set; }
    public DateTime? UltimoAcceso { get; set; }

    // Navegación
    public virtual Comercio Comercio { get; set; } = null!;
}
