using System;
using System.Text.Json;
using System.Threading.Tasks;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Common.Constants;
using DDMS.Backend.Models.DTOs.Auth;
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

    public OwnerRegistrationService(
        AppDbContext dbContext,
        ICloudinaryService cloudinaryService,
        IEmailSender emailSender)
    {
        _dbContext = dbContext;
        _cloudinaryService = cloudinaryService;
        _emailSender = emailSender;
    }

    public async Task<MessageResponse> RegisterOwnerAsync(Guid userId, OwnerRegistrationRequest request, string language = "vi")
    {
        var user = await _dbContext.users.FirstOrDefaultAsync(u => u.id == userId);
        if (user == null)
            throw new UnauthorizedException();

        var existingProfile = await _dbContext.owner_profiles.FirstOrDefaultAsync(p => p.user_id == userId);
        if (existingProfile != null)
            throw new AppException(ErrorCode.AuthValidationFailed, "Bạn đã gửi yêu cầu đăng ký chủ thuyền hoặc đã là chủ thuyền.");

        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            // 1. Create Owner Profile
            var profile = new owner_profile
            {
                id = Guid.NewGuid(),
                user_id = userId,
                business_name = request.FullName,
                license_number = request.LicenseNumber,
                phone_business = request.Phone,
                address = request.Address,
                is_verified = false,
                status = "Pending",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };
            _dbContext.owner_profiles.Add(profile);

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
                    name = vessel.Name,
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

                        using var certStream = cert.File.OpenReadStream();
                        var certUpload = await _cloudinaryService.UploadImageAsync(certStream, cert.File.FileName);
                        documentUrls.Add(certUpload.ImageUrl);

                        _dbContext.boat_certificates.Add(new boat_certificate
                        {
                            id = Guid.NewGuid(),
                            boat_id = boatId,
                            certificate_type = cert.CertificateType.Trim(),
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
            await transaction.CommitAsync();

            // 3. Send Email
            await _emailSender.SendOwnerRegistrationSuccessEmailAsync(user.email, request.FullName, request, language);

            return new MessageResponse { message = "Gửi yêu cầu đăng ký chủ thuyền thành công. Vui lòng chờ Admin duyệt." };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
