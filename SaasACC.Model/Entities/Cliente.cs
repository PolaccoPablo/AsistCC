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

    // Propiedades calculadas
    public string NombreCompleto => $"{Nombre} {Apellido}".Trim();

    // Navegación
    public virtual Comercio Comercio { get; set; } = null!;
    public virtual CuentaCorriente? CuentaCorriente { get; set; }
}
