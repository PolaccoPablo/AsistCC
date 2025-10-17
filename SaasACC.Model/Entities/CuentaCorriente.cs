namespace SaasACC.Model.Entities;

public class CuentaCorriente : BaseEntity
{
    public int ClienteId { get; set; }
    public decimal LimiteCredito { get; set; } = 0;
    public bool Bloqueada { get; set; } = false;
    public string? Observaciones { get; set; }

    // Propiedades calculadas
    public decimal Saldo => Movimientos?.Sum(m =>
        m.TipoMovimiento == TipoMovimiento.Haber ? m.Importe : -m.Importe) ?? 0;

    public decimal SaldoDisponible => LimiteCredito + Saldo;

    // Navegación
    public virtual Cliente Cliente { get; set; } = null!;
    public virtual ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
}
