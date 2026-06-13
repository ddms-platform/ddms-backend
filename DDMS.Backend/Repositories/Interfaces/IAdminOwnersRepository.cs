using DDMS.Backend.Models.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace DDMS.Backend.Repositories.Interfaces;

public interface IAdminOwnersRepository
{
    Task<List<owner_profile>> GetAllProfilesWithUserAsync(CancellationToken ct);
    Task<int> CountActiveBoatsForOwnerAsync(Guid ownerId, CancellationToken ct);
    Task<List<boat>> GetActiveBoatsWithImagesAsync(Guid ownerId, CancellationToken ct);

    Task<owner_profile?> FindProfileWithUserAsync(Guid profileId, CancellationToken ct);
    Task<owner_profile?> FindProfileAsync(Guid profileId, CancellationToken ct);

    Task<role?> FindRoleByNameAsync(string name, CancellationToken ct);
    void AddRole(role entity);
    Task<bool> UserHasRoleAsync(Guid userId, int roleId, CancellationToken ct);
    void AddUserRole(user_role entity);

    Task<List<boat>> GetBoatsByStatusForOwnerAsync(Guid ownerId, string status, CancellationToken ct);

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
