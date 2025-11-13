using SaasACC.Application.Interfaces;
using SaasACC.Model.DTOs;
using SaasACC.Model.Entities;

namespace SaasACC.Application.Services;

public interface IClienteService
{
    Task<IEnumerable<ClienteDto>> GetAllClientesAsync(int comercioId);
    Task<ClienteDto?> GetClienteByIdAsync(int id);
    Task<ClienteDto> CreateClienteAsync(CreateClienteRequest request);
    Task<ClienteDto> UpdateClienteAsync(UpdateClienteRequest request);
    Task<bool> DeleteClienteAsync(int id);
    Task<bool> EmailExistsAsync(string email, int comercioId, int? excludeId = null);
}

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _clienteRepository;

    public ClienteService(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    public async Task<IEnumerable<ClienteDto>> GetAllClientesAsync(int comercioId)
    {
        var clientes = await _clienteRepository.GetAllAsync(comercioId);
        return clientes.Select(MapToDto);
    }

    public async Task<ClienteDto?> GetClienteByIdAsync(int id)
    {
        var cliente = await _clienteRepository.GetByIdAsync(id);
        return cliente != null ? MapToDto(cliente) : null;
    }

    public async Task<ClienteDto> CreateClienteAsync(CreateClienteRequest request)
    {
        // Validar que el email no exista
        if (await _clienteRepository.EmailExistsAsync(request.Email, request.ComercioId))
        {
            throw new InvalidOperationException($"Ya existe un cliente con el email {request.Email}");
        }

        var cliente = new Cliente
        {
            Nombre = request.Nombre,
            Apellido = request.Apellido,
            Email = request.Email,
            Telefono = request.Telefono ?? string.Empty,
            DNI = request.DNI ?? string.Empty,
            ComercioId = request.ComercioId,
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        var clienteCreado = await _clienteRepository.CreateAsync(cliente);
        return MapToDto(clienteCreado);
    }

    public async Task<ClienteDto> UpdateClienteAsync(UpdateClienteRequest request)
    {
        var cliente = await _clienteRepository.GetByIdAsync(request.Id);
        if (cliente == null)
        {
            throw new InvalidOperationException($"Cliente con ID {request.Id} no encontrado");
        }

        // Validar que el email no exista en otro cliente
        if (await _clienteRepository.EmailExistsAsync(request.Email, cliente.ComercioId, request.Id))
        {
            throw new InvalidOperationException($"Ya existe otro cliente con el email {request.Email}");
        }

        cliente.Nombre = request.Nombre;
        cliente.Apellido = request.Apellido;
        cliente.Email = request.Email;
        cliente.Telefono = request.Telefono ?? string.Empty;
        cliente.DNI = request.DNI ?? string.Empty;
        cliente.FechaModificacion = DateTime.UtcNow;

        var clienteActualizado = await _clienteRepository.UpdateAsync(cliente);
        return MapToDto(clienteActualizado);
    }

    public async Task<bool> DeleteClienteAsync(int id)
    {
        return await _clienteRepository.DeleteAsync(id);
    }

    public async Task<bool> EmailExistsAsync(string email, int comercioId, int? excludeId = null)
    {
        return await _clienteRepository.EmailExistsAsync(email, comercioId, excludeId);
    }

    private static ClienteDto MapToDto(Cliente cliente)
    {
        return new ClienteDto
        {
            Id = cliente.Id,
            Nombre = cliente.Nombre,
            Apellido = cliente.Apellido,
            NombreCompleto = cliente.NombreCompleto,
            Email = cliente.Email,
            DNI = cliente.DNI ?? string.Empty,
            Telefono = cliente.Telefono,
            Saldo = cliente.CuentaCorriente?.Saldo ?? 0,
            CuentaCorriente = cliente.CuentaCorriente != null ? new CuentaCorrienteDto
            {
                Id = cliente.CuentaCorriente.Id,
                ClienteId = cliente.CuentaCorriente.ClienteId,
                Saldo = cliente.CuentaCorriente.Saldo,
                FechaCreacion = cliente.CuentaCorriente.FechaCreacion,
                FechaUltimaActualizacion = cliente.CuentaCorriente.FechaModificacion
            } : null
        };
    }
}

// DTOs para requests
public class CreateClienteRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? DNI { get; set; }
    public int ComercioId { get; set; }
}

public class UpdateClienteRequest
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? DNI { get; set; }
}

