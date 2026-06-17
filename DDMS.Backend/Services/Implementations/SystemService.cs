using DDMS.Backend.Models.DTOs.System;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Services.Implementations;

public class SystemService : ISystemService
{
    private readonly ISystemRepository _repo;
    public SystemService(ISystemRepository repo) => _repo = repo;

    public Task<List<BoatTypeResponse>> GetBoatTypesAsync(CancellationToken ct) =>
        _repo.GetBoatTypesAsync(ct);
}
