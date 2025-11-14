using Microsoft.AspNetCore.Mvc;
using SaasACC.Application.Interfaces;
using SaasACC.Model.Entities;

namespace SaasACCAPI.api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ComerciosController : ControllerBase
{
    private readonly IComercioRepository _comercioRepository;
    private readonly ILogger<ComerciosController> _logger;

    public ComerciosController(IComercioRepository comercioRepository, ILogger<ComerciosController> logger)
    {
        _comercioRepository = comercioRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ComercioDto>>> GetAll()
    {
        try
        {
            var comercios = await _comercioRepository.GetAllAsync();
            var comerciosDto = comercios.Select(c => new ComercioDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Email = c.Email,
                Telefono = c.Telefono
            });

            return Ok(comerciosDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener lista de comercios");
            return StatusCode(500, "Error interno del servidor");
        }
    }
}

// DTO simple para listar comercios
public class ComercioDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
}
