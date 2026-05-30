namespace DDMS.Backend.Models.Services.Interfaces;

public interface IPasswordResetService
{
    /// <summary>
    /// Sends a reset link when the account exists and supports password login.
    /// Returns the raw link (for dev logging) or null when no email is sent.
    /// </summary>
    Task<string?> SendResetLinkAsync(string email);

    Task ResetPasswordByTokenAsync(string token, string password, string confirmPassword);
}
