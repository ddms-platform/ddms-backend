using DDMS.Backend.Data;
using DDMS.Backend.Models.DTOs.System;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

public class SystemRepository : ISystemRepository
{
    private readonly AppDbContext _db;
    public SystemRepository(AppDbContext db) => _db = db;

    public Task<List<BoatTypeResponse>> GetBoatTypesAsync(CancellationToken ct) =>
        _db.boat_types
            .Select(t => new BoatTypeResponse { Code = t.code, NameVi = t.name_vi, NameEn = t.name_en })
            .ToListAsync(ct);
}
