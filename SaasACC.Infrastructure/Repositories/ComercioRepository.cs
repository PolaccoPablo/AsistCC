using Microsoft.EntityFrameworkCore;
using SaasACC.Application.Interfaces;
using SaasACC.Infrastructure;
using SaasACC.Model.Entities;

namespace SaasACC.Infrastructure.Repositories;

public class ComercioRepository : IComercioRepository
{
    private readonly ApplicationDbContext _context;

    public ComercioRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Comercio?> GetByIdAsync(int id)
    {
        return await _context.Comercios
            .Include(c => c.Usuarios)
            .Include(c => c.Clientes)
            .FirstOrDefaultAsync(c => c.Id == id && c.Activo);
    }

    public async Task<Comercio?> GetByEmailAsync(string email)
    {
        return await _context.Comercios
            .FirstOrDefaultAsync(c => c.Email == email && c.Activo);
    }

    public async Task<IEnumerable<Comercio>> GetAllAsync()
    {
        return await _context.Comercios
            .Where(c => c.Activo)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    public async Task<Comercio> CreateAsync(Comercio comercio)
    {
        comercio.FechaCreacion = DateTime.UtcNow;
        comercio.Activo = true;

        _context.Comercios.Add(comercio);
        await _context.SaveChangesAsync();

        return comercio;
    }

    public async Task<Comercio> UpdateAsync(Comercio comercio)
    {
        comercio.FechaModificacion = DateTime.UtcNow;

        _context.Comercios.Update(comercio);
        await _context.SaveChangesAsync();

        return comercio;
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
    {
        var query = _context.Comercios
            .Where(c => c.Email == email && c.Activo);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Comercios
            .AnyAsync(c => c.Id == id && c.Activo);
    }
}
