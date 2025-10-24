using SaasACC.Model.Entities;

namespace SaasACC.Application.Interfaces;

public interface IClienteRepository
{
    Task<IEnumerable<Cliente>> GetAllAsync(int comercioId);
    Task<Cliente?> GetByIdAsync(int id);
    Task<Cliente?> GetByEmailAsync(string email, int comercioId);
    Task<Cliente> CreateAsync(Cliente cliente);
    Task<Cliente> UpdateAsync(Cliente cliente);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> EmailExistsAsync(string email, int comercioId, int? excludeId = null);
}

