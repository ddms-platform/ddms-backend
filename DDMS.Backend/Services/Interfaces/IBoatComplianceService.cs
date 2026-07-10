namespace DDMS.Backend.Services.Interfaces;

public interface IBoatComplianceService
{
    Task RunComplianceCheckAsync(CancellationToken ct = default);
}
