using Microsoft.EntityFrameworkCore;
using SaasACC.Application.Interfaces;
using SaasACC.Domain.Entities;
using SaasACC.Application.Services;

namespace SaasACC.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly ApplicationDbContext _context;

    public UsuarioRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        return await _context.Usuarios
            .Include(u => u.Comercio)
            .FirstOrDefaultAsync(u => u.Email == email && u.Activo);
    }

    public async Task<Usuario?> GetByIdAsync(int id)
    {
        return await _context.Usuarios
            .Include(u => u.Comercio)
            .FirstOrDefaultAsync(u => u.Id == id && u.Activo);
    }

    public async Task<Usuario> CreateAsync(Usuario usuario)
    {
        usuario.FechaCreacion = DateTime.UtcNow;
        usuario.Activo = true;

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return usuario;
    }

    public async Task<Usuario> UpdateAsync(Usuario usuario)
    {
        usuario.FechaModificacion = DateTime.UtcNow;

        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();

        return usuario;
    }

    public async Task<bool> ValidatePasswordAsync(string email, string password)
    {
        var usuario = await GetByEmailAsync(email);
        if (usuario == null) return false;

        return PasswordHasher.Verify(password, usuario.PasswordHash);
    }

    public async Task UpdateLastAccessAsync(int userId)
    {
        var usuario = await _context.Usuarios.FindAsync(userId);
        if (usuario != null)
        {
            usuario.UltimoAcceso = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

}
