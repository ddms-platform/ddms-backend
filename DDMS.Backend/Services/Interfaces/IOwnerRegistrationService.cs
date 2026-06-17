using System;
using System.Threading.Tasks;
using DDMS.Backend.Models.DTOs.Auth;

namespace DDMS.Backend.Services.Interfaces;

public interface IOwnerRegistrationService
{
    Task<MessageResponse> RegisterOwnerAsync(Guid userId, OwnerRegistrationRequest request, string language = "vi");
}
