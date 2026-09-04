namespace DDMS.Backend.Common.Helpers;

/// <summary>
/// Sức chứa hiệu dụng của một chuyến (tour_schedule).
///
/// Có hai nguồn khai báo độc lập nhau:
///  - <c>boat.max_passengers</c>: giới hạn vật lý của con thuyền.
///  - <c>tour.max_guests</c>: số khách tối đa chủ thuyền khai cho tour.
/// Ràng buộc thực tế là giá trị NHỎ HƠN trong hai cái, chỉ tính những giá trị
/// đã khai (> 0). Chưa khai cả hai (dữ liệu cũ) thì trả <c>null</c> = không biết,
/// bên gọi tự quyết định — booking thì bỏ qua check thay vì chặn hết mọi đơn.
///
/// Dùng chung cho cả luồng chặn overbooking và luồng hiển thị chỗ trống để hai
/// bên không bao giờ lệch nhau (trước đây search tính theo thuyền, booking cũng
/// theo thuyền, nên chuyến chưa gán thuyền vừa bị ẩn khỏi search vừa không bị
/// chặn overbooking).
/// </summary>
public static class ScheduleCapacity
{
    public static int? Effective(int? boatMaxPassengers, int? tourMaxGuests)
    {
        return new[] { boatMaxPassengers ?? 0, tourMaxGuests ?? 0 }
            .Where(c => c > 0)
            .Select(c => (int?)c)
            .Min();
    }
}
