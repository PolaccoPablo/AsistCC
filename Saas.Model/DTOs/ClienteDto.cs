namespace SaasACC.Shared.DTOs;

public class ClienteDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Saldo { get; set; }
    public string SaldoFormateado => $"${Saldo:N2}";
    public string EstadoSaldo => Saldo >= 0 ? "A Favor" : "Debe";
}
