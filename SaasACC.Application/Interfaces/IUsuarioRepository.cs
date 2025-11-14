using SaasACC.Model.Entities;

namespace SaasACC.Application.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByEmailAsync(string email);
    Task<Usuario?> GetByIdAsync(int id);
    Task<Usuario> CreateAsync(Usuario usuario);
    Task<bool> ValidatePasswordAsync(string email, string password);
    Task UpdateLastAccessAsync(int userId);
}
