using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Data;
using DDMS.Backend.Hubs;
using DDMS.Backend.Models.DTOs.Sos;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Services.Implementations;

public class SosService : ISosService
{
    private readonly AppDbContext _db;
    private readonly IHubContext<SosHub> _sosHub;

    public SosService(AppDbContext db, IHubContext<SosHub> sosHub)
    {
        _db = db;
        _sosHub = sosHub;
    }

    public async Task<SosAlertResponse> TriggerSosAsync(Guid userId, TriggerSosRequest request, CancellationToken ct)
    {
        var newAlert = new sos_alert
        {
            id = Guid.NewGuid(),
            user_id = userId,
            boat_id = request.boat_id,
            latitude = request.latitude,
            longitude = request.longitude,
            status = "ACTIVE",
            note = request.note,
            created_at = DateTime.UtcNow
        };

        _db.sos_alerts.Add(newAlert);
        await _db.SaveChangesAsync(ct);

        var user = await _db.users.FirstOrDefaultAsync(u => u.id == userId, ct);
        boat? boatObj = null;
        if (request.boat_id.HasValue)
        {
            boatObj = await _db.boats.FirstOrDefaultAsync(b => b.id == request.boat_id.Value, ct);
        }

        var response = new SosAlertResponse
        {
            id = newAlert.id,
            user_id = userId,
            user_name = user?.full_name ?? user?.email ?? "Thuyền trưởng",
            user_phone = user?.phone,
            boat_id = request.boat_id,
            boat_name = boatObj?.name,
            registration_number = boatObj?.registration_number,
            latitude = newAlert.latitude,
            longitude = newAlert.longitude,
            status = newAlert.status,
            note = newAlert.note,
            created_at = newAlert.created_at
        };

        // Broadcast real-time event to Admins group over SignalR
        await _sosHub.Clients.Group(SosHub.AdminGroup).SendAsync("ReceiveSosAlert", response, cancellationToken: ct);
        await _sosHub.Clients.All.SendAsync("ReceiveSosAlert", response, cancellationToken: ct);

        return response;
    }

    public async Task<List<SosAlertResponse>> GetActiveAlertsAsync(CancellationToken ct)
    {
        var paged = await GetPagedAlertsAsync(1, 100, "ACTIVE", ct);
        return paged.items;
    }

    public async Task<PagedResponse<SosAlertResponse>> GetPagedAlertsAsync(int page, int pageSize, string? status, CancellationToken ct)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        var query = _db.sos_alerts
            .Include(a => a.user)
            .Include(a => a.boat)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && status != "ALL")
        {
            query = query.Where(a => a.status == status);
        }

        var totalItems = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var alerts = await query
            .OrderByDescending(a => a.created_at)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = alerts.Select(a => new SosAlertResponse
        {
            id = a.id,
            user_id = a.user_id,
            user_name = a.user?.full_name ?? a.user?.email ?? "Thuyền trưởng",
            user_phone = a.user?.phone,
            boat_id = a.boat_id,
            boat_name = a.boat?.name,
            registration_number = a.boat?.registration_number,
            latitude = a.latitude,
            longitude = a.longitude,
            status = a.status,
            note = a.note,
            created_at = a.created_at,
            resolved_at = a.resolved_at
        }).ToList();

        return new PagedResponse<SosAlertResponse>
        {
            items = items,
            page = page,
            pageSize = pageSize,
            totalItems = totalItems,
            totalPages = totalPages
        };
    }

    public async Task<SosAlertResponse> ResolveSosAsync(Guid sosId, Guid resolvedByUserId, ResolveSosRequest request, CancellationToken ct)
    {
        var alert = await _db.sos_alerts
            .Include(a => a.user)
            .Include(a => a.boat)
            .FirstOrDefaultAsync(a => a.id == sosId, ct)
            ?? throw new NotFoundException("Không tìm thấy bản ghi cảnh báo SOS");

        alert.status = "RESOLVED";
        alert.resolved_at = DateTime.UtcNow;
        alert.resolved_by = resolvedByUserId;
        if (!string.IsNullOrWhiteSpace(request.note))
        {
            alert.note = (alert.note != null ? alert.note + " | " : "") + "Xử lý: " + request.note;
        }

        await _db.SaveChangesAsync(ct);

        // Broadcast resolution to SignalR clients
        await _sosHub.Clients.All.SendAsync("SosAlertResolved", sosId, cancellationToken: ct);

        return new SosAlertResponse
        {
            id = alert.id,
            user_id = alert.user_id,
            user_name = alert.user?.full_name ?? alert.user?.email,
            user_phone = alert.user?.phone,
            boat_id = alert.boat_id,
            boat_name = alert.boat?.name,
            registration_number = alert.boat?.registration_number,
            latitude = alert.latitude,
            longitude = alert.longitude,
            status = alert.status,
            note = alert.note,
            created_at = alert.created_at,
            resolved_at = alert.resolved_at
        };
    }

    public async Task DeleteSosAsync(Guid id, CancellationToken ct)
    {
        var alert = await _db.sos_alerts.FirstOrDefaultAsync(a => a.id == id, ct)
            ?? throw new NotFoundException("Không tìm thấy bản ghi cảnh báo SOS");

        _db.sos_alerts.Remove(alert);
        await _db.SaveChangesAsync(ct);
    }
}
