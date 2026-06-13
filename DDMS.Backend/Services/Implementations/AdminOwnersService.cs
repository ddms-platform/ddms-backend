using System.Text.Json;
using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.AdminOwners;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Services.Implementations;

public class AdminOwnersService : IAdminOwnersService
{
    private readonly IAdminOwnersRepository _repo;
    private readonly IEmailSender _email;

    public AdminOwnersService(IAdminOwnersRepository repo, IEmailSender email)
    {
        _repo = repo;
        _email = email;
    }

    public async Task<List<VerificationItem>> GetVerificationsAsync(CancellationToken ct)
    {
        var profiles = await _repo.GetAllProfilesWithUserAsync(ct);
        var items = new List<VerificationItem>();
        foreach (var op in profiles)
        {
            var boatCount = await _repo.CountActiveBoatsForOwnerAsync(op.user_id, ct);
            var ownerBoats = await _repo.GetActiveBoatsWithImagesAsync(op.user_id, ct);
            items.Add(MapVerification(op, boatCount, ownerBoats));
        }
        return items;
    }

    public async Task<string> ApproveVerificationAsync(Guid profileId, CancellationToken ct)
    {
        var profile = await _repo.FindProfileWithUserAsync(profileId, ct)
            ?? throw new NotFoundException(ErrorCode.ResourceNotFound, "Không tìm thấy yêu cầu xác thực.");

        await using var tx = await _repo.BeginTransactionAsync(ct);

        var now = DateTime.UtcNow;
        profile.status = OwnerProfileStatuses.Verified;
        profile.is_verified = true;
        profile.verified_at = now;
        profile.updated_at = now;

        var ownerRole = await _repo.FindRoleByNameAsync(RoleNames.Owner, ct);
        if (ownerRole == null)
        {
            ownerRole = new role { name = RoleNames.Owner, description = RoleNames.OwnerDescription };
            _repo.AddRole(ownerRole);
            await _repo.SaveChangesAsync(ct);
        }

        if (!await _repo.UserHasRoleAsync(profile.user_id, ownerRole.id, ct))
        {
            _repo.AddUserRole(new user_role
            {
                user_id = profile.user_id,
                role_id = ownerRole.id,
                assigned_at = now
            });
        }

        await UpdatePendingBoatsAsync(profile.user_id, BoatStatuses.Idle, ct);

        await _repo.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        await TrySendApprovalEmailAsync(profile);
        return "Xác thực chủ thuyền thành công.";
    }

    public async Task<string> RejectVerificationAsync(Guid profileId, CancellationToken ct)
    {
        var profile = await _repo.FindProfileAsync(profileId, ct)
            ?? throw new NotFoundException(ErrorCode.ResourceNotFound, "Không tìm thấy yêu cầu xác thực.");

        var now = DateTime.UtcNow;
        profile.status = OwnerProfileStatuses.Rejected;
        profile.is_verified = false;
        profile.updated_at = now;

        await UpdatePendingBoatsAsync(profile.user_id, BoatStatuses.Rejected, ct);

        await _repo.SaveChangesAsync(ct);
        return "Đã từ chối yêu cầu xác thực.";
    }

    private async Task UpdatePendingBoatsAsync(Guid ownerId, string newStatus, CancellationToken ct)
    {
        var boats = await _repo.GetBoatsByStatusForOwnerAsync(ownerId, BoatStatuses.Pending, ct);
        var now = DateTime.UtcNow;
        foreach (var b in boats)
        {
            b.status = newStatus;
            b.updated_at = now;
        }
    }

    private async Task TrySendApprovalEmailAsync(owner_profile profile)
    {
        if (profile.user == null || string.IsNullOrEmpty(profile.user.email)) return;
        try
        {
            await _email.SendOwnerVerificationApprovedEmailAsync(
                profile.user.email,
                profile.business_name ?? profile.user.full_name ?? "Chủ thuyền");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending verification approval email: {ex.Message}");
        }
    }

    private static VerificationItem MapVerification(owner_profile op, int boatCount, List<boat> boats) => new()
    {
        Id = op.id,
        Name = op.business_name ?? op.user?.full_name ?? "Chủ thuyền",
        Owner = op.user?.full_name ?? "N/A",
        Email = op.user?.email ?? "N/A",
        Phone = op.phone_business ?? op.user?.phone ?? "N/A",
        Address = op.address ?? "N/A",
        License = op.license_number ?? "N/A",
        Submitted = op.created_at.ToString("dd/MM/yyyy"),
        Status = (op.status ?? OwnerProfileStatuses.Pending).ToLower(),
        Boats = boatCount,
        Vessels = boats.Select(MapVessel).ToList()
    };

    private static VesselItem MapVessel(boat b) => new()
    {
        Id = b.id,
        Name = b.name,
        Type = b.type ?? "N/A",
        Length = b.length,
        Beam = b.beam,
        RegistrationNumber = b.registration_number ?? "N/A",
        MooringType = b.mooring_type ?? "N/A",
        ExpectedDockingDate = b.expected_docking_date?.ToString("dd/MM/yyyy") ?? "N/A",
        RequiredServices = ParseJsonList(b.required_services),
        DocumentUrls = ParseJsonList(b.document_url),
        ImageUrls = b.boat_images.OrderBy(img => img.sort_order).Select(img => img.image_url).ToList(),
        Status = b.status
    };

    private static List<string> ParseJsonList(string? raw)
    {
        if (string.IsNullOrEmpty(raw)) return new();
        try { return JsonSerializer.Deserialize<List<string>>(raw) ?? new(); }
        catch { return new(); }
    }
}
