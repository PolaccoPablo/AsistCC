using Microsoft.EntityFrameworkCore;
using SaasACC.Application.Interfaces;
using SaasACC.Infrastructure;
using SaasACC.Model.Entities;
using System.Security.Cryptography;
using System.Text;

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

    public async Task<bool> ValidatePasswordAsync(string email, string password)
    {
        var usuario = await GetByEmailAsync(email);
        if (usuario == null) return false;

        return VerifyPassword(password, usuario.PasswordHash);
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

    // Métodos auxiliares para hash de contraseñas
    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        var hashedPassword = HashPassword(password);
        return hashedPassword == hash;
    }
}
