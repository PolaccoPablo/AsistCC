using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaasACC.Application.Services;
using SaasACC.Model.DTOs;
using System.Security.Claims;

namespace SaasACCAPI.api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _clienteService;
    private readonly ILogger<ClientesController> _logger;

    public ClientesController(IClienteService clienteService, ILogger<ClientesController> logger)
    {
        _clienteService = clienteService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los clientes del comercio del usuario autenticado
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClienteDto>>> GetClientes()
    {
        try
        {
            var comercioId = GetComercioIdFromToken();
            var clientes = await _clienteService.GetAllClientesAsync(comercioId);
            return Ok(clientes);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "No se pudo obtener el ComercioId del token");
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener clientes");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene un cliente por su ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ClienteDto>> GetCliente(int id)
    {
        try
        {
            var cliente = await _clienteService.GetClienteByIdAsync(id);
            if (cliente == null)
            {
                return NotFound($"Cliente con ID {id} no encontrado");
            }

            // Verificar que el cliente pertenece al comercio del usuario
            var comercioId = GetComercioIdFromToken();
            // Nota: Aquí deberías agregar validación adicional si es necesario

            return Ok(cliente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cliente {ClienteId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Crea un nuevo cliente
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ClienteDto>> CreateCliente([FromBody] ClienteDto clienteDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var comercioId = GetComercioIdFromToken();
            
            var request = new CreateClienteRequest
            {
                Nombre = clienteDto.Nombre,
                Apellido = clienteDto.Apellido,
                Email = clienteDto.Email,
                Telefono = clienteDto.Telefono,
                DNI = clienteDto.DNI,
                ComercioId = comercioId
            };

            var clienteCreado = await _clienteService.CreateClienteAsync(request);
            return CreatedAtAction(nameof(GetCliente), new { id = clienteCreado.Id }, clienteCreado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear cliente");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Actualiza un cliente existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ClienteDto>> UpdateCliente(int id, [FromBody] ClienteDto clienteDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != clienteDto.Id)
            {
                return BadRequest("El ID del cliente no coincide");
            }

            var request = new UpdateClienteRequest
            {
                Id = clienteDto.Id,
                Nombre = clienteDto.Nombre,
                Apellido = clienteDto.Apellido,
                Email = clienteDto.Email,
                Telefono = clienteDto.Telefono,
                DNI = clienteDto.DNI
            };

            var clienteActualizado = await _clienteService.UpdateClienteAsync(request);
            return Ok(clienteActualizado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar cliente {ClienteId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Elimina un cliente (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCliente(int id)
    {
        try
        {
            var resultado = await _clienteService.DeleteClienteAsync(id);
            if (!resultado)
            {
                return NotFound($"Cliente con ID {id} no encontrado");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar cliente {ClienteId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Verifica si un email ya existe para un cliente
    /// </summary>
    [HttpGet("check-email")]
    public async Task<ActionResult<bool>> CheckEmailExists([FromQuery] string email, [FromQuery] int? excludeId = null)
    {
        try
        {
            var comercioId = GetComercioIdFromToken();
            var exists = await _clienteService.EmailExistsAsync(email, comercioId, excludeId);
            return Ok(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar email {Email}", email);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    private int GetComercioIdFromToken()
    {
        // Intentar obtener ComercioId del claim (varios nombres posibles)
        var comercioIdClaim = User.FindFirst("ComercioId")?.Value 
            ?? User.FindFirst(c => c.Type == "ComercioId")?.Value
            ?? User.FindFirst(c => c.Type.EndsWith("/ComercioId", StringComparison.OrdinalIgnoreCase))?.Value;

        if (!string.IsNullOrEmpty(comercioIdClaim) && int.TryParse(comercioIdClaim, out var comercioId))
        {
            return comercioId;
        }

        // Log para debug - ver todos los claims disponibles
        var allClaims = string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}"));
        _logger.LogWarning("No se encontró ComercioId en los claims. Claims disponibles: {Claims}", allClaims);

        throw new UnauthorizedAccessException($"No se pudo obtener el ID del comercio del token. Claims disponibles: {allClaims}");
    }
}

