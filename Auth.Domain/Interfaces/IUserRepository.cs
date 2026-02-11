using Auth.Domain.Entities;

namespace Auth.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdWithRolesAsync(Guid id);
    Task<User?> GetByEmailWithRolesAsync(string email);
    Task<IEnumerable<User>> GetAllAsync(int page, int pageSize);
    Task<int> GetTotalCountAsync();
    Task<User> CreateAsync(User user);
    Task UpdateAsync(User user);
    Task<bool> ExistsAsync(string email);
}
