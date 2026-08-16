namespace DDMS.Backend.Repositories.Interfaces;

public interface IOwnerProfileRepository
{
    /// <summary>
    /// Trạng thái hồ sơ chủ thuyền của một tài khoản, hoặc null nếu chưa nộp
    /// hồ sơ nào. Xem <see cref="Common.Constants.OwnerProfileStatuses"/>.
    /// </summary>
    Task<string?> FindStatusByUserAsync(Guid userId, CancellationToken ct);
}
