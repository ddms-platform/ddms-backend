using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DDMS.Backend.Models.Entities;

/// <summary>
/// Bài viết trên mục "Cẩm nang & Tin tức". Nguồn là RSS của các báo, nội dung
/// do AI viết lại. Luôn giữ source_name và source_url để dẫn nguồn — bài gốc
/// vẫn thuộc bản quyền của báo, viết lại không xoá được điều đó.
/// </summary>
[Table("blog_posts")]
public partial class blog_post
{
    [Key]
    public Guid id { get; set; } = Guid.NewGuid();

    [Required, StringLength(300)]
    public string title { get; set; } = null!;

    /// <summary>Định danh trên URL, duy nhất.</summary>
    [Required, StringLength(320)]
    public string slug { get; set; } = null!;

    [StringLength(600)]
    public string? summary { get; set; }

    /// <summary>Nội dung đã viết lại, định dạng Markdown.</summary>
    public string? content { get; set; }

    [StringLength(1000)]
    public string? cover_image_url { get; set; }

    /// <summary>cam_nang | kinh_nghiem | tin_tuc</summary>
    [Required, StringLength(40)]
    public string category { get; set; } = "tin_tuc";

    /// <summary>draft | published</summary>
    [Required, StringLength(20)]
    public string status { get; set; } = "draft";

    // ----- Dẫn nguồn: bắt buộc có, không được để trống khi xuất bản -----
    [StringLength(150)]
    public string? source_name { get; set; }

    [StringLength(1000)]
    public string? source_url { get; set; }

    public DateTime? source_published_at { get; set; }

    /// <summary>Băm của source_url, dùng để chặn cào trùng bài.</summary>
    [StringLength(64)]
    public string? source_hash { get; set; }

    /// <summary>
    /// Kịch bản video dạng JSON: danh sách cảnh gồm lời đọc và ảnh.
    /// Trình duyệt tự dựng slideshow từ đây, không cần render MP4 trên server.
    /// </summary>
    public string? video_script { get; set; }

    public DateTime? published_at { get; set; }

    public int view_count { get; set; }

    public DateTime created_at { get; set; } = DateTime.UtcNow;

    public DateTime updated_at { get; set; } = DateTime.UtcNow;
}
