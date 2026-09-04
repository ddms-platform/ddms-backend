using System;

namespace DDMS.Backend.Models.Entities;

/// <summary>
/// Phiếu chỉnh sửa dịch vụ của tour đã được duyệt. Tour live giữ nguyên;
/// admin duyệt phiếu này mới ghi đè nội dung lên đúng tour_id cũ.
/// </summary>
public class service_change_request
{
    public Guid id { get; set; }

    public Guid tour_id { get; set; }

    public Guid boat_id { get; set; }

    public Guid owner_id { get; set; }

    /// <summary>JSON của DynamicServiceRequest — bản đề xuất, chưa áp lên tour.</summary>
    public string payload_json { get; set; } = string.Empty;

    /// <summary>pending | approved | rejected</summary>
    public string status { get; set; } = "pending";

    public string? rejection_reason { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual tour? tour { get; set; }

    public virtual boat? boat { get; set; }
}
