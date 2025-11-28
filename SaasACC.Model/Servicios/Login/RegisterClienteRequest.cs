using System.ComponentModel.DataAnnotations;

namespace SaasACC.Model.Servicios.Login;

public class RegisterClienteRequest
{
    // Datos del Cliente
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es requerido")]
    [StringLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es requerido")]
    [Phone(ErrorMessage = "Teléfono inválido")]
    public string Telefono { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
    public string? Direccion { get; set; }

    [StringLength(20, ErrorMessage = "El DNI no puede exceder 20 caracteres")]
    public string? DNI { get; set; }

    // Referencia al Comercio
    [Required(ErrorMessage = "Debe seleccionar un comercio")]
    public int ComercioId { get; set; }

    // Credenciales de acceso (opcional - para autogestión)
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
    public string? Password { get; set; }

    [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
    public string? ConfirmPassword { get; set; }
}
