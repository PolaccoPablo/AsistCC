namespace SaasACC.Model.Entities;

public enum TipoMovimiento
{
    Debe = 1,  // Cliente debe dinero (compra, cargo)
    Haber = 2  // Cliente tiene a favor (pago, abono)
}
