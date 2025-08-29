using Microsoft.AspNetCore.Mvc;

namespace SaasACC.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            Message = "¡API funcionando correctamente!",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0"
        });
    }

    [HttpGet("clientes")]
    public IActionResult GetClientes()
    {
        var clientes = new[]
        {
            new { Id = 1, Nombre = "Juan Pérez", Saldo = 1500.50m },
            new { Id = 2, Nombre = "María García", Saldo = -250.75m },
            new { Id = 3, Nombre = "Carlos López", Saldo = 0m }
        };

        return Ok(clientes);
    }
}
