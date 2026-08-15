using System;
using System.Text.Json;
using System.Threading.Tasks;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Common.Constants;
using DDMS.Backend.Models.DTOs.Auth;
using DDMS.Backend.Models.DTOs.OwnerDocument;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Services.Interfaces;
using DDMS.Backend.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DDMS.Backend.Services.Implementations;

public class OwnerRegistrationService : IOwnerRegistrationService
{
    private readonly AppDbContext _dbContext;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IEmailSender _emailSender;
    private readonly ICertificateTypeService _certificateTypes;
    private readonly IOwnerDocumentService _ownerDocuments;
    private readonly INotificationService _notificationService;

    public OwnerRegistrationService(
        AppDbContext dbContext,
        ICloudinaryService cloudinaryService,
        IEmailSender emailSender,
        ICertificateTypeService certificateTypes,
        IOwnerDocumentService ownerDocuments,
        INotificationService notificationService)
    {
        _dbContext = dbContext;
        _cloudinaryService = cloudinaryService;
        _emailSender = emailSender;
        _certificateTypes = certificateTypes;
        _ownerDocuments = ownerDocuments;
        _notificationService = notificationService;
    }

    public async Task<MessageResponse> RegisterOwnerAsync(Guid userId, OwnerRegistrationRequest request, string language = "vi")
    {
        var user = await _dbContext.users.FirstOrDefaultAsync(u => u.id == userId);
        if (user == null)
            throw new UnauthorizedException();

        var existingProfile = await _dbContext.owner_profiles.FirstOrDefaultAsync(p => p.user_id == userId);
        if (existingProfile != null)
            throw new AppException(ErrorCode.AuthValidationFailed, "Bạn đã gửi yêu cầu đăng ký chủ thuyền hoặc đã là chủ thuyền.");

        var entityType = string.IsNullOrWhiteSpace(request.EntityType)
            ? OwnerEntityTypes.Individual
            : request.EntityType.Trim().ToLowerInvariant();

        if (!OwnerEntityTypes.IsValid(entityType))
            throw new AppException(ErrorCode.InvalidOwnerEntityType, ErrorCode.Messages.InvalidOwnerEntityType);

        // Owner documents are optional at registration; upload later via Owner Documents page.
        request.OwnerDocuments ??= new List<OwnerDocumentUploadDto>();

        // Ca luong chi co dung mot SaveChangesAsync o cuoi (OwnerDocumentService
        // chi Add vao change tracker, khong tu luu), ma EF Core von da boc
        // SaveChanges trong transaction cua no. Nen transaction thu cong o day
        // thua ngay tu dau.
        //
        // Tu khi Program.cs bat EnableRetryOnFailure, no con lam request 500:
        //   The configured execution strategy 'MySqlRetryingExecutionStrategy'
        //   does not support user-initiated transactions.
        // 1. Create Owner Profile
        var profile = new owner_profile
        {
            id = Guid.NewGuid(),
            user_id = userId,
            business_name = request.FullName,
            license_number = request.LicenseNumber,
            phone_business = request.Phone,
            address = request.Address,
            entity_type = entityType,
            is_verified = false,
            status = "Pending",
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };
        _dbContext.owner_profiles.Add(profile);

        var nationalIdUrl = await _ownerDocuments.AddDocumentsOnRegisterAsync(
            profile.id, request.OwnerDocuments);
        if (!string.IsNullOrWhiteSpace(nationalIdUrl))
            profile.license_image = nationalIdUrl;

        // 2. Create Boats
        foreach (var vessel in request.Vessels)
        {
            var boatId = Guid.NewGuid();
            var imageUrls = new List<string>();
            if (vessel.ImageFiles != null && vessel.ImageFiles.Any())
            {
                foreach (var file in vessel.ImageFiles)
                {
                    using var stream = file.OpenReadStream();
                    var uploadResult = await _cloudinaryService.UploadImageAsync(stream, file.FileName);
                    imageUrls.Add(uploadResult.ImageUrl);
                }
            }

            var documentUrls = new List<string>();
            if (vessel.DocumentFiles != null && vessel.DocumentFiles.Any())
            {
                foreach (var file in vessel.DocumentFiles)
                {
                    using var stream = file.OpenReadStream();
                    var uploadResult = await _cloudinaryService.UploadImageAsync(stream, file.FileName);
                    documentUrls.Add(uploadResult.ImageUrl);
                }
            }

            var boat = new boat
            {
                id = boatId,
                owner_id = userId,
                name = NormalizeVesselName(vessel.Name),
                type = vessel.Type,
                length = vessel.Length,
                beam = vessel.Beam,
                registration_number = vessel.RegistrationNumber,
                mooring_type = vessel.MooringType,
                expected_docking_date = vessel.ExpectedDockingDate,
                required_services = JsonSerializer.Serialize(vessel.RequiredServices ?? new List<string>()),
                document_url = documentUrls.Any() ? JsonSerializer.Serialize(documentUrls) : null,
                max_passengers = 1, // Default value, will be updated by admin later
                status = "Pending",
                compliance_status = BoatComplianceStatuses.Valid,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            _dbContext.boats.Add(boat);

            if (vessel.Certificates != null && vessel.Certificates.Count > 0)
            {
                var certNow = DateTime.UtcNow;
                foreach (var cert in vessel.Certificates)
                {
                    if (cert.File is null || cert.File.Length == 0
                        || string.IsNullOrWhiteSpace(cert.CertificateType))
                    {
                        continue;
                    }

                    var certType = cert.CertificateType.Trim();
                    if (BoatCertificateTypes.IsDeprecated(certType))
                    {
                        throw new AppException(ErrorCode.CertificateTypeRequired,
                            "Giấy phép KD vận tải thủy thuộc hồ sơ chủ thuyền (transport_license), không upload trên tàu.");
                    }

                    await _certificateTypes.EnsureActiveCodeAsync(certType, CertificateScopes.Boat);

                    using var certStream = cert.File.OpenReadStream();
                    var certUpload = await _cloudinaryService.UploadImageAsync(certStream, cert.File.FileName);
                    documentUrls.Add(certUpload.ImageUrl);

                    _dbContext.boat_certificates.Add(new boat_certificate
                    {
                        id = Guid.NewGuid(),
                        boat_id = boatId,
                        certificate_type = certType,
                        document_url = certUpload.ImageUrl,
                        public_id = certUpload.PublicId,
                        expiry_date = cert.ExpiryDate,
                        status = BoatCertificateStatuses.Pending,
                        created_at = certNow,
                        updated_at = certNow
                    });
                }

                if (documentUrls.Any())
                {
                    boat.document_url = JsonSerializer.Serialize(documentUrls);
                }
            }

            if (imageUrls.Any())
            {
                int sortOrder = 0;
                foreach (var url in imageUrls)
                {
                    _dbContext.boat_images.Add(new boat_image
                    {
                        id = Guid.NewGuid(),
                        boat_id = boatId,
                        image_url = url,
                        sort_order = sortOrder++,
                        created_at = DateTime.UtcNow
                    });
                }
            }
        }

        await _dbContext.SaveChangesAsync();

        // 3. Send In-App Notification to Admins
        try
        {
            await _notificationService.CreateNotificationForAdminsAsync(
                senderId: userId,
                type: "admin",
                title: "Đăng ký đối tác mới 👤",
                body: $"Người dùng {request.FullName} vừa gửi hồ sơ đăng ký làm Chủ tàu mới trên hệ thống. Vui lòng kiểm duyệt.",
                data: null,
                ct: default
            );
        }
        catch { /* best-effort */ }

        // 4. Send Email
        //
        // Hồ sơ đã lưu ở trên rồi. Nếu để lỗi gửi mail ném ra ngoài thì client
        // nhận 500 dù đăng ký đã thành công, khách bấm lại sẽ dính "Bạn đã gửi
        // yêu cầu đăng ký chủ thuyền hoặc đã là chủ thuyền" và tưởng hệ thống hỏng.
        // Mail chỉ là thông báo, không phải một phần của giao dịch.
        try
        {
            await _emailSender.SendOwnerRegistrationSuccessEmailAsync(
                user.email, request.FullName, request, language);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Gửi email xác nhận đăng ký chủ thuyền thất bại: {ex.Message}");
        }

        return new MessageResponse { message = "Gửi yêu cầu đăng ký chủ thuyền thành công. Vui lòng chờ Admin duyệt." };
    }

    private static string NormalizeVesselName(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "VESSEL";
        var clean = System.Text.RegularExpressions.Regex.Replace(raw.Trim(), @"\s+", " ");
        clean = System.Text.RegularExpressions.Regex.Replace(clean, @"[^\p{L}\p{N}\s\-_]", "");
        return string.IsNullOrWhiteSpace(clean) ? "VESSEL" : clean.ToUpperInvariant();
    }
}
