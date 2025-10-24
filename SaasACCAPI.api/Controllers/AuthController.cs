using Microsoft.AspNetCore.Mvc;
using SaasACC.Application.Services;
using SaasACC.Model.Servicios.Login;

namespace SaasACCAPI.api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    ErrorMessage = "Datos de entrada inválidos"
                });
            }

            _logger.LogInformation("Intento de login para email: {Email}", request.Email);

            var result = await _authService.LoginAsync(request);

            if (result.Success)
            {
                _logger.LogInformation("Login exitoso para usuario: {Email}", request.Email);
                return Ok(result);
            }

            if (result.UserNotFound)
            {
                _logger.LogWarning("Usuario no encontrado: {Email}", request.Email);
                return NotFound(result);
            }

            _logger.LogWarning("Credenciales inválidas para usuario: {Email}", request.Email);
            return Unauthorized(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el proceso de login para email: {Email}", request.Email);
            return StatusCode(500, new LoginResponse
            {
                Success = false,
                ErrorMessage = "Error interno del servidor"
            });
        }
    }
}
