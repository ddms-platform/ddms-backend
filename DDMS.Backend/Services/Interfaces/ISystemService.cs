using DDMS.Backend.Models.DTOs.System;

namespace DDMS.Backend.Services.Interfaces;

public interface ISystemService
{
    Task<List<BoatTypeResponse>> GetBoatTypesAsync(CancellationToken ct);
}
