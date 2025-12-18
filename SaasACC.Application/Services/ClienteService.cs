using SaasACC.Application.Interfaces;
using SaasACC.Model.DTOs;
using SaasACC.Model.Entities;
using System.Security.Cryptography;
using System.Text;

namespace SaasACC.Application.Services;

public interface IClienteService
{
    Task<IEnumerable<ClienteDto>> GetAllClientesAsync(int comercioId, int? estadoId = null);
    Task<ClienteDto?> GetClienteByIdAsync(int id);
    Task<ClienteDto> CreateClienteAsync(CreateClienteRequest request);
    Task<ClienteDto> UpdateClienteAsync(UpdateClienteRequest request);
    Task<bool> DeleteClienteAsync(int id);
    Task<bool> EmailExistsAsync(string email, int comercioId, int? excludeId = null);
    Task<ClienteDto> AprobarClienteAsync(int clienteId, int aprobadoPorUsuarioId);
    Task<ClienteDto> RechazarClienteAsync(int clienteId, int rechazadoPorUsuarioId);
    Task<IEnumerable<ClienteDto>> GetClientesPendientesAsync(int comercioId);

    // Nuevos métodos para modelo multicomercio
    Task<IEnumerable<MiComercioDto>> GetComerciosDeUsuarioAsync(int usuarioId);
    Task<ClienteDto> VincularUsuarioAComercioAsync(int usuarioId, int comercioId, bool requiereAprobacion = true);
}

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IComercioRepository _comercioRepository;

    public ClienteService(
        IClienteRepository clienteRepository,
        IUsuarioRepository usuarioRepository,
        IComercioRepository comercioRepository)
    {
        _clienteRepository = clienteRepository;
        _usuarioRepository = usuarioRepository;
        _comercioRepository = comercioRepository;
    }

    public async Task<IEnumerable<ClienteDto>> GetAllClientesAsync(int comercioId, int? estadoId = null)
    {
        var clientes = await _clienteRepository.GetAllAsync(comercioId, estadoId);
        return clientes.Select(MapToDto);
    }

    public async Task<ClienteDto?> GetClienteByIdAsync(int id)
    {
        var cliente = await _clienteRepository.GetByIdAsync(id);
        return cliente != null ? MapToDto(cliente) : null;
    }

    public async Task<ClienteDto> CreateClienteAsync(CreateClienteRequest request)
    {
        Usuario usuario;

        // Verificar si el usuario ya existe (por email)
        var usuarioExistente = await _usuarioRepository.GetByEmailAsync(request.Email);

        if (usuarioExistente != null)
        {
            // Usuario existe: verificar que no esté ya vinculado a este comercio
            var yaEsCliente = await _clienteRepository.ExisteVinculoAsync(
                usuarioExistente.Id, request.ComercioId);

            if (yaEsCliente)
            {
                throw new InvalidOperationException(
                    "Este usuario ya es cliente de este comercio");
            }

            usuario = usuarioExistente;
        }
        else
        {
            // Usuario NO existe: validar DNI y crear nuevo usuario
            if (string.IsNullOrWhiteSpace(request.DNI))
            {
                throw new InvalidOperationException(
                    "El DNI es requerido para crear un cliente nuevo desde la administración");
            }

            // Crear el usuario con contraseña temporal = DNI
            usuario = new Usuario
            {
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Email = request.Email,
                PasswordHash = HashPassword(request.DNI),
                Rol = "Cliente",
                ComercioId = null, // NULL para clientes (se relacionan vía Cliente)
                FechaCreacion = DateTime.UtcNow,
                Activo = true
                // UltimoAcceso queda null, indicando que nunca ha iniciado sesión
            };

            usuario = await _usuarioRepository.CreateAsync(usuario);
        }

        // Crear la vinculación Cliente (tabla intermedia Usuario-Comercio)
        var cliente = new Cliente
        {
            Nombre = request.Nombre,
            Apellido = request.Apellido,
            Email = request.Email,
            Telefono = request.Telefono ?? string.Empty,
            DNI = request.DNI ?? string.Empty,
            UsuarioId = usuario.Id, // FK a Usuario (REQUIRED)
            ComercioId = request.ComercioId,
            EstadoId = 2, // Activo (creado por admin)
            OrigenRegistro = 1, // Administración
            Activo = true,
            FechaCreacion = DateTime.UtcNow,
            FechaAprobacion = DateTime.UtcNow // Se aprueba inmediatamente
        };

        var clienteCreado = await _clienteRepository.CreateAsync(cliente);

        // Crear cuenta corriente inmediatamente para clientes creados por admin
        await _clienteRepository.CrearCuentaCorrienteAsync(clienteCreado.Id);

        // Recargar cliente con cuenta corriente
        var clienteConCuenta = await _clienteRepository.GetByIdAsync(clienteCreado.Id);
        return MapToDto(clienteConCuenta!);
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

    public async Task<ClienteDto> AprobarClienteAsync(int clienteId, int aprobadoPorUsuarioId)
    {
        var cliente = await _clienteRepository.GetByIdAsync(clienteId);
        if (cliente == null)
        {
            throw new InvalidOperationException($"Cliente con ID {clienteId} no encontrado");
        }

        if (cliente.EstadoId == 2) // Ya está activo
        {
            throw new InvalidOperationException("El cliente ya está aprobado");
        }

        // Cambiar estado a Activo
        cliente.EstadoId = 2;
        cliente.FechaAprobacion = DateTime.UtcNow;
        cliente.AprobadoPorUsuarioId = aprobadoPorUsuarioId;
        cliente.FechaModificacion = DateTime.UtcNow;

        await _clienteRepository.UpdateAsync(cliente);

        // Crear cuenta corriente al aprobar
        await _clienteRepository.CrearCuentaCorrienteAsync(clienteId);

        // Recargar cliente con cuenta corriente
        var clienteAprobado = await _clienteRepository.GetByIdAsync(clienteId);
        return MapToDto(clienteAprobado!);
    }

    public async Task<ClienteDto> RechazarClienteAsync(int clienteId, int rechazadoPorUsuarioId)
    {
        var cliente = await _clienteRepository.GetByIdAsync(clienteId);
        if (cliente == null)
        {
            throw new InvalidOperationException($"Cliente con ID {clienteId} no encontrado");
        }

        // Cambiar estado a Inactivo
        cliente.EstadoId = 3;
        cliente.FechaModificacion = DateTime.UtcNow;
        cliente.AprobadoPorUsuarioId = rechazadoPorUsuarioId; // Guardamos quién rechazó

        await _clienteRepository.UpdateAsync(cliente);

        var clienteRechazado = await _clienteRepository.GetByIdAsync(clienteId);
        return MapToDto(clienteRechazado!);
    }

    public async Task<IEnumerable<ClienteDto>> GetClientesPendientesAsync(int comercioId)
    {
        var clientes = await _clienteRepository.GetAllAsync(comercioId, estadoId: 1); // EstadoId = 1 (Pendiente)
        return clientes.Select(MapToDto);
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
            EstadoId = cliente.EstadoId,
            EstadoNombre = cliente.Estado?.Nombre ?? string.Empty,
            OrigenRegistro = cliente.OrigenRegistro,
            TieneUsuario = cliente.UsuarioId > 0,
            FechaAprobacion = cliente.FechaAprobacion,
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

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    // Nuevos métodos para modelo multicomercio

    public async Task<IEnumerable<MiComercioDto>> GetComerciosDeUsuarioAsync(int usuarioId)
    {
        var clientes = await _clienteRepository.GetByUsuarioIdAsync(usuarioId);

        return clientes.Select(c => new MiComercioDto
        {
            ComercioId = c.ComercioId,
            NombreComercio = c.Comercio.Nombre,
            EstadoId = c.EstadoId,
            EstadoNombre = c.Estado.Nombre,
            FechaVinculacion = c.FechaCreacion,
            FechaAprobacion = c.FechaAprobacion,
            Saldo = c.CuentaCorriente?.Saldo ?? 0
        });
    }

    public async Task<ClienteDto> VincularUsuarioAComercioAsync(int usuarioId, int comercioId, bool requiereAprobacion = true)
    {
        // Validar que usuario existe
        var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
        if (usuario == null)
        {
            throw new InvalidOperationException("Usuario no encontrado");
        }

        // Validar que comercio existe
        if (!await _comercioRepository.ExistsAsync(comercioId))
        {
            throw new InvalidOperationException("Comercio no encontrado");
        }

        // Validar que no esté ya vinculado
        if (await _clienteRepository.ExisteVinculoAsync(usuarioId, comercioId))
        {
            throw new InvalidOperationException("El usuario ya está vinculado a este comercio");
        }

        // Crear vinculación
        var cliente = new Cliente
        {
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido ?? string.Empty,
            Email = usuario.Email,
            Telefono = string.Empty,
            UsuarioId = usuarioId,
            ComercioId = comercioId,
            EstadoId = requiereAprobacion ? 1 : 2, // Pendiente o Activo
            OrigenRegistro = 2, // Autogestión (usuario se vincula por sí mismo)
            Activo = true,
            FechaCreacion = DateTime.UtcNow,
            FechaAprobacion = requiereAprobacion ? null : DateTime.UtcNow
        };

        var clienteCreado = await _clienteRepository.CreateAsync(cliente);

        // Si no requiere aprobación, crear cuenta corriente inmediatamente
        if (!requiereAprobacion)
        {
            await _clienteRepository.CrearCuentaCorrienteAsync(clienteCreado.Id);
        }

        var clienteConCuenta = await _clienteRepository.GetByIdAsync(clienteCreado.Id);
        return MapToDto(clienteConCuenta!);
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

