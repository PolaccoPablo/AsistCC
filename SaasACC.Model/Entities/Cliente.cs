namespace SaasACC.Model.Entities;

public class Cliente : BaseEntity
{
    // Datos propios del cliente (mantenidos por ahora)
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? DNI { get; set; }

    // Relaciones (Cliente como tabla intermedia Usuario-Comercio)
    public int UsuarioId { get; set; } // REQUIRED (no nullable) - FK a Usuario
    public int ComercioId { get; set; } // FK a Comercio

    // Estado y aprobación de esta vinculación
    public int EstadoId { get; set; } // FK a EstadoCliente
    public int OrigenRegistro { get; set; } // 1: Administracion, 2: Autogestion
    public DateTime? FechaAprobacion { get; set; }
    public int? AprobadoPorUsuarioId { get; set; } // FK a Usuario

    // Datos específicos de esta vinculación (opcional)
    public string? Alias { get; set; } // Ej: "Juan el del taller"
    public string? NotasComercio { get; set; } // Notas privadas del comercio

    // Propiedades calculadas
    public string NombreCompleto => $"{Nombre} {Apellido}".Trim();

    // Navegación
    public virtual Usuario Usuario { get; set; } = null!;
    public virtual Comercio Comercio { get; set; } = null!;
    public virtual CuentaCorriente? CuentaCorriente { get; set; }
    public virtual EstadoCliente Estado { get; set; } = null!;
    public virtual Usuario? AprobadoPor { get; set; }
}
