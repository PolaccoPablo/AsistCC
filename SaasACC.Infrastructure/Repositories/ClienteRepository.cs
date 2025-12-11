using Microsoft.EntityFrameworkCore;
using SaasACC.Application.Interfaces;
using SaasACC.Infrastructure;
using SaasACC.Model.Entities;

namespace SaasACC.Infrastructure.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly ApplicationDbContext _context;

    public ClienteRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Cliente>> GetAllAsync(int comercioId, int? estadoId = null)
    {
        var query = _context.Clientes
            .Include(c => c.CuentaCorriente)
            .Include(c => c.Estado)
            .Include(c => c.Usuario)
            .Where(c => c.ComercioId == comercioId && c.Activo);

        if (estadoId.HasValue)
        {
            query = query.Where(c => c.EstadoId == estadoId.Value);
        }

        return await query
            .OrderBy(c => c.Nombre)
            .ThenBy(c => c.Apellido)
            .ToListAsync();
    }

    public async Task<Cliente?> GetByIdAsync(int id)
    {
        return await _context.Clientes
            .Include(c => c.CuentaCorriente)
            .Include(c => c.Comercio)
            .Include(c => c.Estado)
            .Include(c => c.Usuario)
            .Include(c => c.AprobadoPor)
            .FirstOrDefaultAsync(c => c.Id == id && c.Activo);
    }

    public async Task<Cliente?> GetByEmailAsync(string email, int comercioId)
    {
        return await _context.Clientes
            .Include(c => c.CuentaCorriente)
            .FirstOrDefaultAsync(c => c.Email == email && c.ComercioId == comercioId && c.Activo);
    }

    public async Task<Cliente> CreateAsync(Cliente cliente)
    {
        // NombreCompleto se calcula automáticamente desde Nombre y Apellido

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        // NOTA: Ya NO se crea la CuentaCorriente automáticamente
        // Se creará solo cuando el cliente sea aprobado (EstadoId = 2)

        // Recargar el cliente con las navegaciones
        return await GetByIdAsync(cliente.Id) ?? cliente;
    }

    public async Task<Cliente> UpdateAsync(Cliente cliente)
    {
        // NombreCompleto se calcula automáticamente desde Nombre y Apellido
        
        _context.Clientes.Update(cliente);
        await _context.SaveChangesAsync();
        
        return cliente;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null) return false;

        // Soft delete
        cliente.Activo = false;
        cliente.FechaModificacion = DateTime.UtcNow;
        
        // También soft delete de la cuenta corriente
        var cuentaCorriente = await _context.CuentasCorrientes
            .FirstOrDefaultAsync(cc => cc.ClienteId == id);
        if (cuentaCorriente != null)
        {
            cuentaCorriente.Activo = false;
            cuentaCorriente.FechaModificacion = DateTime.UtcNow;
        }
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Clientes
            .AnyAsync(c => c.Id == id && c.Activo);
    }

    public async Task<bool> EmailExistsAsync(string email, int comercioId, int? excludeId = null)
    {
        var query = _context.Clientes
            .Where(c => c.Email == email && c.ComercioId == comercioId && c.Activo);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<CuentaCorriente> CrearCuentaCorrienteAsync(int clienteId)
    {
        // Verificar que el cliente no tenga ya una cuenta corriente
        var cuentaExistente = await _context.CuentasCorrientes
            .FirstOrDefaultAsync(cc => cc.ClienteId == clienteId);

        if (cuentaExistente != null)
        {
            return cuentaExistente;
        }

        // Crear nueva cuenta corriente
        var cuentaCorriente = new CuentaCorriente
        {
            ClienteId = clienteId,
            LimiteCredito = 0,
            FechaCreacion = DateTime.UtcNow,
            Activo = true
        };

        _context.CuentasCorrientes.Add(cuentaCorriente);
        await _context.SaveChangesAsync();

        return cuentaCorriente;
    }

    // Nuevos métodos para modelo multicomercio

    public async Task<IEnumerable<Cliente>> GetByUsuarioIdAsync(int usuarioId)
    {
        return await _context.Clientes
            .Include(c => c.Comercio)
            .Include(c => c.CuentaCorriente)
            .Include(c => c.Estado)
            .Where(c => c.UsuarioId == usuarioId && c.Activo)
            .OrderBy(c => c.Comercio.Nombre)
            .ToListAsync();
    }

    public async Task<bool> ExisteVinculoAsync(int usuarioId, int comercioId)
    {
        return await _context.Clientes
            .AnyAsync(c => c.UsuarioId == usuarioId && c.ComercioId == comercioId && c.Activo);
    }

    public async Task<Cliente?> GetVinculoAsync(int usuarioId, int comercioId)
    {
        return await _context.Clientes
            .Include(c => c.Comercio)
            .Include(c => c.CuentaCorriente)
            .Include(c => c.Estado)
            .Include(c => c.Usuario)
            .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId && c.ComercioId == comercioId && c.Activo);
    }
}

