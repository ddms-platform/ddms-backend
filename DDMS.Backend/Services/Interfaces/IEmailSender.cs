namespace DDMS.Backend.Services.Interfaces;

public interface IEmailSender
{
    Task SendVerificationLinkEmailAsync(string toEmail, string verificationLink, int expiryMinutes);
    Task SendPasswordResetEmailAsync(string toEmail, string resetLink, int expiryMinutes);
    Task SendOwnerRegistrationSuccessEmailAsync(string toEmail, string ownerName, Models.DTOs.Auth.OwnerRegistrationRequest request, string language);
    Task SendBookingStatusEmailAsync(
        string toEmail, 
        string customerName, 
        string bookingId, 
        string tourName, 
        string boatName, 
        DateTime tourTime, 
        decimal totalPrice, 
        string status, 
        string? cancelReason);
    Task SendServiceRegistrationSuccessEmailAsync(
        string toEmail, 
        string ownerName, 
        string serviceName, 
        string boatName, 
        decimal basePrice);
    Task SendOwnerVerificationApprovedEmailAsync(string toEmail, string ownerName);
    Task SendBoatDockAssignmentEmailAsync(
        string toEmail, 
        string ownerName, 
        string boatName, 
        string dockName, 
        string slipCode, 
        DateTime startTime, 
        DateTime endTime);
    Task SendMaintenanceStatusEmailAsync(
        string toEmail, 
        string ownerName, 
        string boatName, 
        string serviceName, 
        string status, 
        decimal price);
    Task SendWithdrawalStatusEmailAsync(
        string toEmail, 
        string userName, 
        decimal amount, 
        string bankName, 
        string accountNumber, 
        string status);
    Task SendNewChatMessageEmailAsync(
        string toEmail,
        string recipientName,
        string senderName,
        string messageBody,
        string viewChatLink);
    Task SendScheduleChangeEmailAsync(
        string toEmail,
        string customerName,
        string bookingId,
        string tourName,
        DateTime oldTime,
        DateTime newTime);
}
