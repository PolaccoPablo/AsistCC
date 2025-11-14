using System.ComponentModel.DataAnnotations;

namespace SaasACC.Model.Servicios.Login;

public class RegisterComercioRequest
{
    // Datos del Comercio
    [Required(ErrorMessage = "El nombre del comercio es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string NombreComercio { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email del comercio es requerido")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string EmailComercio { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es requerido")]
    [Phone(ErrorMessage = "Teléfono inválido")]
    public string TelefonoComercio { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección es requerida")]
    [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
    public string DireccionComercio { get; set; } = string.Empty;

    // Datos del Usuario Administrador
    [Required(ErrorMessage = "El nombre del administrador es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string NombreAdmin { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email del administrador es requerido")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string EmailAdmin { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe confirmar la contraseña")]
    [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
