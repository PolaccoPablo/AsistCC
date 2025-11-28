namespace SaasACC.Model.DTOs;

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

    // Nuevos campos para sistema de aprobación
    public int EstadoId { get; set; }
    public string EstadoNombre { get; set; } = string.Empty;
    public int OrigenRegistro { get; set; } // 1: Administracion, 2: Autogestion
    public string OrigenRegistroNombre => OrigenRegistro == 1 ? "Administración" : "Autogestión";
    public bool TieneUsuario { get; set; }
    public DateTime? FechaAprobacion { get; set; }

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
