using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Sos;

namespace DDMS.Backend.Services.Interfaces;

public interface ISosService
{
    Task<SosAlertResponse> TriggerSosAsync(Guid userId, TriggerSosRequest request, CancellationToken ct);
    Task<List<SosAlertResponse>> GetActiveAlertsAsync(CancellationToken ct);
    Task<PagedResponse<SosAlertResponse>> GetPagedAlertsAsync(int page, int pageSize, string? status, CancellationToken ct);
    Task<SosAlertResponse> ResolveSosAsync(Guid sosId, Guid resolvedByUserId, ResolveSosRequest request, CancellationToken ct);
    Task DeleteSosAsync(Guid id, CancellationToken ct);
}
