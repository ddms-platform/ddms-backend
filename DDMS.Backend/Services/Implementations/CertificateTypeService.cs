using System.Text.RegularExpressions;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.BoatCertificate;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Services.Implementations;

public class CertificateTypeService : ICertificateTypeService
{
    private static readonly Regex CodePattern = new(@"^[a-z][a-z0-9_]*$", RegexOptions.Compiled);

    private readonly ICertificateTypeRepository _repo;

    public CertificateTypeService(ICertificateTypeRepository repo) => _repo = repo;

    public async Task<List<CertificateTypeItem>> GetActiveAsync(CancellationToken ct = default)
    {
        var items = await _repo.GetActiveAsync(ct);
        return items.Select(Map).ToList();
    }

    public async Task<List<CertificateTypeItem>> GetAllForAdminAsync(CancellationToken ct = default)
    {
        var items = await _repo.GetAllAsync(ct);
        return items.Select(Map).ToList();
    }

    public async Task<CertificateTypeItem> CreateAsync(
        CreateCertificateTypeRequest request, CancellationToken ct = default)
    {
        var code = NormalizeCode(request.code);
        var nameVi = RequireName(request.nameVi, "nameVi");
        var nameEn = RequireName(request.nameEn, "nameEn");

        if (await _repo.GetByCodeAsync(code, ct) is not null)
        {
            throw new AppException(ErrorCode.CertificateTypeCodeExists, ErrorCode.Messages.CertificateTypeCodeExists);
        }

        var sortOrder = request.sortOrder ?? (await _repo.GetMaxSortOrderAsync(ct) + 1);
        var entity = new certificate_type
        {
            code = code,
            name_vi = nameVi,
            name_en = nameEn,
            sort_order = sortOrder,
            is_active = request.isActive
        };

        await _repo.AddAsync(entity, ct);
        return Map(entity);
    }

    public async Task<CertificateTypeItem> UpdateAsync(
        int id, UpdateCertificateTypeRequest request, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(ErrorCode.CertificateTypeNotFound, ErrorCode.Messages.CertificateTypeNotFound);

        entity.name_vi = RequireName(request.nameVi, "nameVi");
        entity.name_en = RequireName(request.nameEn, "nameEn");
        entity.sort_order = request.sortOrder;
        entity.is_active = request.isActive;

        await _repo.UpdateAsync(entity, ct);
        return Map(entity);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(ErrorCode.CertificateTypeNotFound, ErrorCode.Messages.CertificateTypeNotFound);

        if (await _repo.IsCodeInUseAsync(entity.code, ct))
        {
            // Soft-disable when already used by boat certificates
            entity.is_active = false;
            await _repo.UpdateAsync(entity, ct);
            return;
        }

        await _repo.DeleteAsync(entity, ct);
    }

    public async Task EnsureActiveCodeAsync(string code, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new AppException(ErrorCode.CertificateTypeRequired, ErrorCode.Messages.CertificateTypeRequired);
        }

        if (!await _repo.ExistsActiveCodeAsync(code.Trim(), ct))
        {
            throw new AppException(ErrorCode.CertificateTypeRequired, ErrorCode.Messages.CertificateTypeRequired);
        }
    }

    private static string NormalizeCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new AppException(ErrorCode.CertificateTypeRequired, ErrorCode.Messages.CertificateTypeRequired);
        }

        var normalized = code.Trim().ToLowerInvariant();
        if (!CodePattern.IsMatch(normalized) || normalized.Length > 50)
        {
            throw new AppException(ErrorCode.AuthValidationFailed, ErrorCode.Messages.ValidationFailed,
                new Dictionary<string, List<string>>
                {
                    ["code"] = ["Mã loại giấy tờ chỉ gồm a-z, 0-9, gạch dưới; bắt đầu bằng chữ cái."]
                });
        }

        return normalized;
    }

    private static string RequireName(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new AppException(ErrorCode.AuthValidationFailed, ErrorCode.Messages.ValidationFailed,
                new Dictionary<string, List<string>>
                {
                    [field] = ["Tên loại giấy tờ là bắt buộc."]
                });
        }

        return value.Trim();
    }

    private static CertificateTypeItem Map(certificate_type t) => new()
    {
        id = t.id,
        code = t.code,
        nameVi = t.name_vi,
        nameEn = t.name_en,
        sortOrder = t.sort_order,
        isActive = t.is_active
    };
}
