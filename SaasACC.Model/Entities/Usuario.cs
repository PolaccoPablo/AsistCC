namespace SaasACC.Model.Entities;

public class Usuario : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string? Apellido { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Rol { get; set; } = "Usuario"; // Admin, UsuarioComercio, Cliente
    public int ComercioId { get; set; }
    public DateTime? UltimoAcceso { get; set; }

    // Navegación
    public virtual Comercio Comercio { get; set; } = null!;
    public virtual ICollection<Cliente> ClientesAprobados { get; set; } = new List<Cliente>();
}
