namespace SaasACC.Model.Entities;

public class Cliente : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? DNI { get; set; }
    public int ComercioId { get; set; }

    // Nuevos campos para autogestión y aprobación
    public int EstadoId { get; set; } // FK a EstadoCliente
    public int? UsuarioId { get; set; } // FK nullable a Usuario
    public int OrigenRegistro { get; set; } // 1: Administracion, 2: Autogestion
    public DateTime? FechaAprobacion { get; set; }
    public int? AprobadoPorUsuarioId { get; set; } // FK a Usuario

    // Propiedades calculadas
    public string NombreCompleto => $"{Nombre} {Apellido}".Trim();

    // Navegación
    public virtual Comercio Comercio { get; set; } = null!;
    public virtual CuentaCorriente? CuentaCorriente { get; set; }
    public virtual EstadoCliente Estado { get; set; } = null!;
    public virtual Usuario? Usuario { get; set; }
    public virtual Usuario? AprobadoPor { get; set; }
}
