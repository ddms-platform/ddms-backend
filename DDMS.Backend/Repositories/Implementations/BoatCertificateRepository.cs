using DDMS.Backend.Common.Constants;
using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

public class BoatCertificateRepository : IBoatCertificateRepository
{
    private readonly AppDbContext _db;

    public BoatCertificateRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<boat_certificate>> GetByBoatIdAsync(Guid boatId, CancellationToken ct = default) =>
        _db.boat_certificates
            .Where(c => c.boat_id == boatId)
            .OrderBy(c => c.certificate_type)
            .ToListAsync(ct);

    public Task<boat_certificate?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.boat_certificates
            .Include(c => c.boat)
            .ThenInclude(b => b.owner)
            .FirstOrDefaultAsync(c => c.id == id, ct);

    public Task<boat_certificate?> GetByBoatAndTypeAsync(Guid boatId, string certificateType, CancellationToken ct = default) =>
        _db.boat_certificates
            .Where(c => c.boat_id == boatId
                        && c.certificate_type == certificateType
                        && (c.status == BoatCertificateStatuses.Pending || c.status == BoatCertificateStatuses.Approved))
            .FirstOrDefaultAsync(ct);

    public async Task<boat_certificate> AddAsync(boat_certificate entity, CancellationToken ct = default)
    {
        _db.boat_certificates.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(boat_certificate entity, CancellationToken ct = default)
    {
        _db.boat_certificates.Update(entity);
        await _db.SaveChangesAsync(ct);
    }

    public Task<List<boat_certificate>> GetPendingForAdminAsync(CancellationToken ct = default) =>
        _db.boat_certificates
            .Include(c => c.boat)
            .ThenInclude(b => b.owner)
            .Where(c => c.status == BoatCertificateStatuses.Pending)
            .OrderBy(c => c.created_at)
            .ToListAsync(ct);

    public Task<List<boat_certificate>> GetExpiringAsync(DateOnly expiryThreshold, CancellationToken ct = default) =>
        _db.boat_certificates
            .Include(c => c.boat)
            .ThenInclude(b => b.owner)
            .Where(c => c.status == BoatCertificateStatuses.Approved
                        && c.expiry_date <= expiryThreshold
                        && c.expiry_date >= DateOnly.FromDateTime(DateTime.UtcNow))
            .OrderBy(c => c.expiry_date)
            .ToListAsync(ct);

    public Task<int> MarkExpiredAsync(DateOnly today, CancellationToken ct = default) =>
        _db.boat_certificates
            .Where(c => c.status == BoatCertificateStatuses.Approved && c.expiry_date < today)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.status, BoatCertificateStatuses.Expired)
                .SetProperty(c => c.updated_at, DateTime.UtcNow), ct);

    public async Task SetBoatComplianceStatusAsync(Guid boatId, string complianceStatus, CancellationToken ct = default)
    {
        await _db.boats
            .Where(b => b.id == boatId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.compliance_status, complianceStatus)
                .SetProperty(b => b.updated_at, DateTime.UtcNow), ct);
    }

    public Task<int> DeactivateBoatServicesAsync(Guid boatId, CancellationToken ct = default) =>
        _db.boat_services
            .Where(s => s.boat_id == boatId && s.is_active == true)
            .ExecuteUpdateAsync(s => s
                .SetProperty(bs => bs.is_active, false)
                .SetProperty(bs => bs.updated_at, DateTime.UtcNow), ct);

    public Task<boat?> GetBoatByIdAsync(Guid boatId, CancellationToken ct = default) =>
        _db.boats.FirstOrDefaultAsync(b => b.id == boatId && !b.is_deleted, ct);
}
