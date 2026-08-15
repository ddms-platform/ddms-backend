using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Blog;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

/// <summary>Mục "Cẩm nang & Tin tức" — phần công khai.</summary>
[ApiController]
[Route("api/public/blog")]
public class BlogController : ControllerBase
{
    private readonly IBlogService _blog;

    public BlogController(IBlogService blog) => _blog = blog;

    [HttpGet]
    public async Task<IActionResult> GetPublished(
        [FromQuery] string? category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 9,
        CancellationToken ct = default)
    {
        var result = await _blog.GetPublishedAsync(category, page, pageSize, ct);
        return Ok(ApiResponse<PagedResponse<BlogPostListItem>>.Ok(result));
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
    {
        var result = await _blog.GetBySlugAsync(slug, ct);
        return Ok(ApiResponse<BlogPostDetail>.Ok(result));
    }
}

/// <summary>Quản trị bài viết: cào tin, duyệt, xuất bản.</summary>
[ApiController]
[Route("api/admin/blog")]
[Authorize(Roles = "admin")]
public class AdminBlogController : ControllerBase
{
    private readonly IBlogService _blog;
    private readonly IBlogCrawlerService _crawler;

    public AdminBlogController(IBlogService blog, IBlogCrawlerService crawler)
    {
        _blog = blog;
        _crawler = crawler;
    }

    /// <summary>Chạy một lượt cào tin. Bài tạo ra ở trạng thái draft, chờ duyệt.</summary>
    [HttpPost("crawl")]
    public async Task<IActionResult> Crawl(CancellationToken ct)
    {
        var result = await _crawler.CrawlAsync(ct);
        return Ok(ApiResponse<CrawlResult>.Ok(result));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _blog.GetForAdminAsync(status, page, pageSize, ct);
        return Ok(ApiResponse<PagedResponse<BlogPostListItem>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _blog.GetByIdAsync(id, ct);
        return Ok(ApiResponse<BlogPostDetail>.Ok(result));
    }

    [HttpPut("{id:guid}/publish")]
    public async Task<IActionResult> SetPublished(
        Guid id, [FromBody] PublishRequest req, CancellationToken ct)
    {
        var message = await _blog.SetPublishedAsync(id, req.Published, ct);
        return Ok(ApiResponse<object>.Ok(new { message }));
    }
}
