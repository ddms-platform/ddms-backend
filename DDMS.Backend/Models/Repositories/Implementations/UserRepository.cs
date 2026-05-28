using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Models.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Models.Repositories.Implementations;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;

    public UserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<user?> GetByEmailAsync(string email)
    {
        return _dbContext.users
            .Include(x => x.user_roles)
            .ThenInclude(x => x.role)
            .FirstOrDefaultAsync(x => x.email == email);
    }

    public Task<user?> GetByGoogleIdAsync(string googleId)
    {
        return _dbContext.users
            .Include(x => x.user_roles)
            .ThenInclude(x => x.role)
            .FirstOrDefaultAsync(x => x.google_id == googleId);
    }

    public Task<user?> GetByIdWithRolesAsync(Guid id)
    {
        return _dbContext.users
            .Include(x => x.user_roles)
            .ThenInclude(x => x.role)
            .FirstOrDefaultAsync(x => x.id == id);
    }

    public Task<bool> EmailExistsAsync(string email)
    {
        return _dbContext.users.AnyAsync(x => x.email == email);
    }

    public async Task<user> AddAsync(user entity)
    {
        _dbContext.users.Add(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(user entity)
    {
        entity.updated_at = DateTime.UtcNow;
        _dbContext.users.Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task MarkEmailVerifiedAsync(Guid userId)
    {
        var entity = await _dbContext.users.FirstOrDefaultAsync(x => x.id == userId);
        if (entity is null)
        {
            return;
        }

        entity.email_verified_at = DateTime.UtcNow;
        entity.updated_at = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
    }

    public async Task AssignRoleAsync(Guid userId, string roleName)
    {
        var role = await _dbContext.roles.FirstOrDefaultAsync(x => x.name == roleName);
        if (role is null)
        {
            return;
        }

        var exists = await _dbContext.user_roles.AnyAsync(x => x.user_id == userId && x.role_id == role.id);
        if (exists)
        {
            return;
        }

        _dbContext.user_roles.Add(new user_role
        {
            user_id = userId,
            role_id = role.id,
            assigned_at = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();
    }
}
