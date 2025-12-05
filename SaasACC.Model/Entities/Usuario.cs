namespace SaasACC.Model.Entities;

public class Usuario : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string? Apellido { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Rol { get; set; } = "Usuario"; // Admin, UsuarioComercio, Cliente

    // ComercioId ahora es nullable
    // NULL para Rol="Cliente" (se relaciona vía tabla Cliente)
    // NOT NULL para Rol="Admin" o "UsuarioComercio"
    public int? ComercioId { get; set; }

    public DateTime? UltimoAcceso { get; set; }

    // Navegación
    public virtual Comercio? Comercio { get; set; }
    public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
    public virtual ICollection<Cliente> ClientesAprobados { get; set; } = new List<Cliente>();
}
