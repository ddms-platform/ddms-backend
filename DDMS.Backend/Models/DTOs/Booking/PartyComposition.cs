namespace DDMS.Backend.Models.DTOs.Booking;

/// <summary>
/// Thành phần đoàn khách của một booking, đã chuẩn hoá.
///
/// Đây là chỗ DUY NHẤT quyết định "đơn này có bao nhiêu người, thuộc hạng nào".
/// Client cũ (chỉ gửi NumPeople) lẫn client mới (gửi từng hạng) đều đi qua đây,
/// nên phần còn lại của hệ thống không phải biết có hai kiểu request.
/// </summary>
public sealed record PartyComposition(int Adults, int Children, int Infants)
{
    /// <summary>Tổng số người trên tàu — kể cả em bé, vì em bé vẫn chiếm chỗ khi tính an toàn.</summary>
    public int Total => Adults + Children + Infants;

    /// <summary>
    /// Client cũ không gửi hạng vé nào thì coi toàn bộ NumPeople là người lớn.
    /// Client mới có gửi thì bỏ qua NumPeople hoàn toàn — server tự cộng lại,
    /// không tin con số client tự khai.
    /// </summary>
    public static PartyComposition FromRequest(CreateBookingRequest request) =>
        FromCounts(request.NumPeople, request.NumAdults, request.NumChildren, request.NumInfants);

    /// <summary>
    /// Dùng chung cho request từ client và cho booking đã lưu trong DB. Đơn đặt
    /// trước khi có tính năng này có num_adults/children/infants = 0 nhưng
    /// num_people > 0 — đọc lại phải ra đúng số khách cũ, không được ra 0.
    /// </summary>
    public static PartyComposition FromCounts(int numPeople, int adults, int children, int infants)
    {
        adults = Math.Max(adults, 0);
        children = Math.Max(children, 0);
        infants = Math.Max(infants, 0);

        if (adults + children + infants == 0)
            return new PartyComposition(Math.Max(numPeople, 0), 0, 0);

        return new PartyComposition(adults, children, infants);
    }
}
