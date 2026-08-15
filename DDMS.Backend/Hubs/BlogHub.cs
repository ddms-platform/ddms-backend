using Microsoft.AspNetCore.SignalR;

namespace DDMS.Backend.Hubs;

/// <summary>
/// Đẩy bài viết mới tới mọi khách đang mở trang tin tức.
///
/// Không yêu cầu đăng nhập: đây là nội dung công khai, và mục đích là người
/// đang đọc thấy bài mới xuất hiện mà không phải tải lại trang.
/// </summary>
public class BlogHub : Hub
{
    public const string GroupName = "blog-readers";

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName);
        await base.OnDisconnectedAsync(exception);
    }
}
