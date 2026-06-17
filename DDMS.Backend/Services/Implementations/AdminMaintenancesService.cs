using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.AdminMaintenances;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Services.Implementations;

public class AdminMaintenancesService : IAdminMaintenancesService
{
    private readonly IAdminMaintenancesRepository _repo;
    private readonly IEmailSender _email;

    public AdminMaintenancesService(IAdminMaintenancesRepository repo, IEmailSender email)
    {
        _repo = repo;
        _email = email;
    }

    public Task<List<MaintenanceItem>> GetPendingAsync(CancellationToken ct) =>
        _repo.GetByStatusAsync(BoatMaintenanceStatuses.Pending, ct);

    public Task<List<MaintenanceItem>> GetAllAsync(CancellationToken ct) =>
        _repo.GetByStatusAsync(null, ct);

    public Task ApproveAsync(Guid id, CancellationToken ct) =>
        UpdateStatusAsync(id, BoatMaintenanceStatuses.Approved, ct);

    public Task RejectAsync(Guid id, CancellationToken ct) =>
        UpdateStatusAsync(id, BoatMaintenanceStatuses.Rejected, ct);

    private async Task UpdateStatusAsync(Guid id, string newStatus, CancellationToken ct)
    {
        var m = await _repo.FindWithBoatOwnerAsync(id, ct)
            ?? throw new NotFoundException(ErrorCode.MaintenanceNotFound, ErrorCode.Messages.MaintenanceNotFound);

        m.status = newStatus;
        await _repo.SaveChangesAsync(ct);
        await TrySendNotificationEmailAsync(m, newStatus);
    }

    private async Task TrySendNotificationEmailAsync(boat_maintenance m, string status)
    {
        if (m.boat?.owner == null) return;
        try
        {
            var price = m.port_maintenance_service?.price ?? 0;
            var serviceName = m.port_maintenance_service?.name ?? m.reason ?? BoatMaintenanceStatuses.DefaultServiceName;
            await _email.SendMaintenanceStatusEmailAsync(
                m.boat.owner.email,
                m.boat.owner.full_name,
                m.boat.name,
                serviceName,
                status,
                price);
        }
        catch
        {
            // best-effort: nuốt lỗi gửi mail
        }
    }
}
