using SaasACC.Domain.Enums;

namespace SaasACC.Domain.Entities;

public class Movimiento : BaseEntity
{
    public int CuentaCorrienteId { get; set; }
    public TipoMovimiento TipoMovimiento { get; set; }
    public decimal Importe { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string? Comprobante { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public bool Pagado { get; set; } = false;
    public DateTime? FechaPago { get; set; }
    public string? ObservacionesPago { get; set; }

    // Navegación
    public virtual CuentaCorriente CuentaCorriente { get; set; } = null!;
}