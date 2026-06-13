using DDMS.Backend.Models.DTOs.System;

namespace DDMS.Backend.Repositories.Interfaces;

public interface ISystemRepository
{
    Task<List<BoatTypeResponse>> GetBoatTypesAsync(CancellationToken ct);
}
