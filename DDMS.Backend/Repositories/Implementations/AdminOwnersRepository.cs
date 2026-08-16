using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DDMS.Backend.Repositories.Implementations;

public class AdminOwnersRepository : IAdminOwnersRepository
{
    private readonly AppDbContext _db;
    public AdminOwnersRepository(AppDbContext db) => _db = db;

    public Task<List<owner_profile>> GetAllProfilesWithUserAsync(CancellationToken ct) =>
        _db.owner_profiles
            .Include(op => op.user)
            .Include(op => op.owner_documents)
            .OrderByDescending(op => op.created_at)
            .ToListAsync(ct);

    public Task<int> CountActiveBoatsForOwnerAsync(Guid ownerId, CancellationToken ct) =>
        _db.boats.CountAsync(b => b.owner_id == ownerId && !b.is_deleted, ct);

    public Task<List<boat>> GetActiveBoatsWithImagesAsync(Guid ownerId, CancellationToken ct) =>
        _db.boats.Include(b => b.boat_images).Include(b => b.boat_certificates).Where(b => b.owner_id == ownerId && !b.is_deleted).ToListAsync(ct);

    public Task<owner_profile?> FindProfileWithUserAsync(Guid profileId, CancellationToken ct) =>
        _db.owner_profiles
            .Include(op => op.user)
            .Include(op => op.owner_documents)
            .FirstOrDefaultAsync(op => op.id == profileId, ct);

    public Task<owner_profile?> FindProfileAsync(Guid profileId, CancellationToken ct) =>
        _db.owner_profiles
            .Include(op => op.owner_documents)
            .FirstOrDefaultAsync(op => op.id == profileId, ct);

    public Task<role?> FindRoleByNameAsync(string name, CancellationToken ct) =>
        _db.roles.FirstOrDefaultAsync(r => r.name == name, ct);

    public void AddRole(role entity) => _db.roles.Add(entity);

    public Task<bool> UserHasRoleAsync(Guid userId, int roleId, CancellationToken ct) =>
        _db.user_roles.AnyAsync(ur => ur.user_id == userId && ur.role_id == roleId, ct);

    public void AddUserRole(user_role entity) => _db.user_roles.Add(entity);

    public Task<List<boat>> GetBoatsByStatusForOwnerAsync(Guid ownerId, string status, CancellationToken ct) =>
        _db.boats.Where(b => b.owner_id == ownerId && b.status == status && !b.is_deleted).ToListAsync(ct);

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct) =>
        _db.Database.BeginTransactionAsync(ct);

    public IExecutionStrategy CreateExecutionStrategy() =>
        _db.Database.CreateExecutionStrategy();

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
