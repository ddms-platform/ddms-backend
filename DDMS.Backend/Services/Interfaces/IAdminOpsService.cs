using DDMS.Backend.Models.DTOs.AdminOps;

namespace DDMS.Backend.Services.Interfaces;

public interface IAdminOpsService
{
    Task<OpsBriefingResponse> GetMorningBriefingAsync(CancellationToken ct);
    Task<AdminOpsChatResponse> AskAsync(Guid adminUserId, AdminOpsChatRequest request, CancellationToken ct);
    IAsyncEnumerable<string> AskStreamAsync(Guid adminUserId, AdminOpsChatRequest request, CancellationToken ct);
    Task<WhatIfSimResponse> SimulateAsync(WhatIfSimRequest request, CancellationToken ct);
}
