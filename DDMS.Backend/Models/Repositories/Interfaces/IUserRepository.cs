using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Models.Repositories.Interfaces;

public interface IUserRepository
{
    Task<user?> GetByEmailAsync(string email);
    Task<user?> GetByGoogleIdAsync(string googleId);
    Task<user?> GetByIdWithRolesAsync(Guid id);
    Task<bool> EmailExistsAsync(string email);
    Task<user> AddAsync(user entity);
    Task UpdateAsync(user entity);
    Task MarkEmailVerifiedAsync(Guid userId);
    Task AssignRoleAsync(Guid userId, string roleName);
}
