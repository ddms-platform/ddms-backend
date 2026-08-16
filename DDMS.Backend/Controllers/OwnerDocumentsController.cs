using DDMS.Backend.Common.Identity;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.OwnerDocument;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[Authorize]
[ApiController]
[Route("api/owner/documents")]
public class OwnerDocumentsController : ControllerBase
{
    private readonly IOwnerDocumentService _documents;
    private readonly ICurrentUser _user;

    public OwnerDocumentsController(IOwnerDocumentService documents, ICurrentUser user)
    {
        _documents = documents;
        _user = user;
    }

    [HttpGet]
    public async Task<IActionResult> GetOverview(CancellationToken ct) =>
        Ok(ApiResponse<OwnerDocumentsOverviewResponse>.Ok(await _documents.GetOverviewByUserIdAsync(_user.Id, ct)));

    [HttpGet("list")]
    public async Task<IActionResult> List(CancellationToken ct) =>
        Ok(ApiResponse<List<OwnerDocumentListItem>>.Ok(await _documents.ListByUserIdAsync(_user.Id, ct)));

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadOrReplace(
        [FromForm] UploadOwnerDocumentRequest request, CancellationToken ct) =>
        Ok(ApiResponse<OwnerDocumentListItem>.Ok(
            await _documents.UploadOrReplaceAsync(_user.Id, request, ct)));
}
