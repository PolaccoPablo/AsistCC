namespace SaasACC.Model.Entities;

public class Comercio : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string? Logo { get; set; }
    public bool NotificacionesEmail { get; set; } = true;
    public bool NotificacionesWhatsApp { get; set; } = false;

    // Navegación
    public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
