using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.OwnerBoats;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Services.Implementations;

public class BoatMaintenanceService : IBoatMaintenanceService
{
    private readonly IBoatMaintenanceRepository _repo;
    private readonly IBoatService _boats;

    public BoatMaintenanceService(IBoatMaintenanceRepository repo, IBoatService boats)
    {
        _repo = repo;
        _boats = boats;
    }

    public async Task<int> RegisterAsync(Guid boatId, Guid ownerId,
        IReadOnlyList<MaintenanceRegistrationRequest> registrations, CancellationToken ct)
    {
        await EnsureOwnedBoatAsync(boatId, ownerId);

        if (registrations == null || registrations.Count == 0)
            throw new AppException(ErrorCode.UncategorizedError, "Vui lòng chọn ít nhất một dịch vụ bảo trì.");

        var now = DateTime.UtcNow;
        foreach (var reg in registrations)
        {
            var service = await _repo.FindServiceAsync(reg.serviceId, ct)
                ?? throw new AppException(ErrorCode.UncategorizedError,
                    $"Không tìm thấy dịch vụ bảo trì với ID: {reg.serviceId}");

            _repo.AddMaintenance(new boat_maintenance
            {
                id = Guid.NewGuid(),
                boat_id = boatId,
                start_time = reg.scheduledDate,
                end_time = reg.scheduledDate.Add(BoatMaintenanceStatuses.DefaultDuration),
                reason = service.name,
                port_maintenance_service_id = reg.serviceId,
                status = BoatMaintenanceStatuses.Pending,
                created_at = now
            });
        }

        await _repo.SaveChangesAsync(ct);
        return registrations.Count;
    }

    public async Task DeleteAsync(Guid boatId, Guid maintenanceId, Guid ownerId, CancellationToken ct)
    {
        await EnsureOwnedBoatAsync(boatId, ownerId);

        var maintenance = await _repo.FindMaintenanceForBoatAsync(maintenanceId, boatId, ct)
            ?? throw new NotFoundException(ErrorCode.ResourceNotFound, "Không tìm thấy lịch bảo trì.");

        maintenance.is_deleted = true;
        await _repo.SaveChangesAsync(ct);
    }

    private async Task EnsureOwnedBoatAsync(Guid boatId, Guid ownerId)
    {
        var boat = await _boats.GetByIdByOwnerAsync(boatId, ownerId);
        if (boat == null) throw new NotFoundException(ErrorCode.ResourceNotFound, "Không tìm thấy tàu của bạn.");
    }
}
