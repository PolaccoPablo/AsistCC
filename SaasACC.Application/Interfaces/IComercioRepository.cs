using SaasACC.Model.Entities;

namespace SaasACC.Application.Interfaces;

public interface IComercioRepository
{
    Task<Comercio?> GetByIdAsync(int id);
    Task<Comercio?> GetByEmailAsync(string email);
    Task<IEnumerable<Comercio>> GetAllAsync();
    Task<Comercio> CreateAsync(Comercio comercio);
    Task<Comercio> UpdateAsync(Comercio comercio);
    Task<bool> EmailExistsAsync(string email, int? excludeId = null);
    Task<bool> ExistsAsync(int id);
}
