namespace SaasACC.Shared.DTOs;

public class ClienteDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
	public string NombreCompleto { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string DNI { get; set; } = string.Empty;
	public string Telefono { get; set; } = string.Empty;
	public decimal Saldo { get; set; }
    public string SaldoFormateado => $"${Saldo:N2}";
    public string EstadoSaldo => Saldo >= 0 ? "A Favor" : "Debe";
    public CuentaCorrienteDto? CuentaCorriente { get; set; }
}

public class CuentaCorrienteDto
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public decimal Saldo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaUltimaActualizacion { get; set; }
}
