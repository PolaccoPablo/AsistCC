namespace SaasACC.Model.Entities;

public class EstadoCliente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    // Navegación
    public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
}
