using System;
using System.Threading.Tasks;
using DDMS.Backend.Models.DTOs.Auth;
using DDMS.Backend.Models.DTOs.BoatCertificate;
using DDMS.Backend.Models.DTOs.OwnerDocument;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Collections.Generic;

namespace DDMS.Backend.Controllers;

[ApiController]
[Route("api/owner")]
public class OwnerRegistrationController : ControllerBase
{
    private readonly IOwnerRegistrationService _ownerRegistrationService;

    public OwnerRegistrationController(IOwnerRegistrationService ownerRegistrationService)
    {
        _ownerRegistrationService = ownerRegistrationService;
    }

    [HttpPost("register")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Register([FromForm] IFormCollection form)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }

        var request = new OwnerRegistrationRequest
        {
            FullName = form["FullName"].ToString(),
            Email = form["Email"].ToString(),
            Phone = form["Phone"].ToString(),
            LicenseNumber = form["LicenseNumber"].ToString(),
            Address = form["Address"].ToString(),
            EntityType = form.ContainsKey("EntityType")
                ? form["EntityType"].ToString()
                : "individual",
            OwnerDocuments = new List<OwnerDocumentUploadDto>(),
            Vessels = new List<VesselRegistrationItem>()
        };

        int docIndex = 0;
        while (form.ContainsKey($"OwnerDocuments[{docIndex}].DocumentType"))
        {
            var docPrefix = $"OwnerDocuments[{docIndex}]";
            var expiryStr = form[$"{docPrefix}.ExpiryDate"].ToString();
            var docDto = new OwnerDocumentUploadDto
            {
                DocumentType = form[$"{docPrefix}.DocumentType"].ToString(),
                ExpiryDate = string.IsNullOrEmpty(expiryStr) ? null : DateOnly.Parse(expiryStr)
            };

            var docFiles = form.Files.GetFiles($"{docPrefix}.File");
            if (docFiles is { Count: > 0 })
            {
                docDto.File = docFiles[0];
            }

            request.OwnerDocuments.Add(docDto);
            docIndex++;
        }

        // Parse dynamic vessels. Assuming keys like Vessels[0].Name, Vessels[0].Type
        int i = 0;
        while (form.ContainsKey($"Vessels[{i}].Name"))
        {
            var prefix = $"Vessels[{i}]";
            
            var lengthStr = form[$"{prefix}.Length"].ToString();
            var beamStr = form[$"{prefix}.Beam"].ToString();
            var expectedDateStr = form[$"{prefix}.ExpectedDockingDate"].ToString();
            
            var vessel = new VesselRegistrationItem
            {
                Name = form[$"{prefix}.Name"].ToString(),
                Type = form[$"{prefix}.Type"].ToString(),
                RegistrationNumber = form[$"{prefix}.RegistrationNumber"].ToString(),
                MooringType = form[$"{prefix}.MooringType"].ToString(),
                Length = string.IsNullOrEmpty(lengthStr) ? null : decimal.Parse(lengthStr),
                Beam = string.IsNullOrEmpty(beamStr) ? null : decimal.Parse(beamStr),
                ExpectedDockingDate = string.IsNullOrEmpty(expectedDateStr) ? null : DateTime.Parse(expectedDateStr),
                RequiredServices = new List<string>()
            };

            // Parse required services array
            int j = 0;
            while (form.ContainsKey($"{prefix}.RequiredServices[{j}]"))
            {
                vessel.RequiredServices.Add(form[$"{prefix}.RequiredServices[{j}]"].ToString());
                j++;
            }
            
            // Single string RequiredServices mapping fallback (if frontend sends JSON or comma separated string)
            if (form.ContainsKey($"{prefix}.RequiredServices") && vessel.RequiredServices.Count == 0)
            {
                var servicesRaw = form[$"{prefix}.RequiredServices"].ToString();
                if (!string.IsNullOrEmpty(servicesRaw))
                {
                    try {
                        var parsed = System.Text.Json.JsonSerializer.Deserialize<List<string>>(servicesRaw);
                        if (parsed != null) vessel.RequiredServices = parsed;
                    } catch {
                        vessel.RequiredServices = new List<string>(servicesRaw.Split(','));
                    }
                }
            }

            var imgFiles = form.Files.GetFiles($"{prefix}.ImageFiles");
            if (imgFiles != null && imgFiles.Count > 0)
            {
                vessel.ImageFiles.AddRange(imgFiles);
            }

            var docFiles = form.Files.GetFiles($"{prefix}.DocumentFiles");
            if (docFiles != null && docFiles.Count > 0)
            {
                vessel.DocumentFiles.AddRange(docFiles);
            }

            int certIndex = 0;
            while (form.ContainsKey($"{prefix}.Certificates[{certIndex}].CertificateType"))
            {
                var certPrefix = $"{prefix}.Certificates[{certIndex}]";
                var expiryStr = form[$"{certPrefix}.ExpiryDate"].ToString();
                var certDto = new CertificateUploadDto
                {
                    CertificateType = form[$"{certPrefix}.CertificateType"].ToString(),
                    ExpiryDate = string.IsNullOrEmpty(expiryStr)
                        ? default
                        : DateOnly.Parse(expiryStr)
                };

                var certFiles = form.Files.GetFiles($"{certPrefix}.File");
                if (certFiles is { Count: > 0 })
                {
                    certDto.File = certFiles[0];
                }

                vessel.Certificates.Add(certDto);
                certIndex++;
            }

            request.Vessels.Add(vessel);
            i++;
        }

        string lang = Request.Headers.AcceptLanguage.ToString().StartsWith("en", StringComparison.OrdinalIgnoreCase) ? "en" : "vi";

        var response = await _ownerRegistrationService.RegisterOwnerAsync(userId, request, lang);
        return Ok(response);
    }
}
