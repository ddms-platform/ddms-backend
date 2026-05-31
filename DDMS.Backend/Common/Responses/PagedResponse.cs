namespace DDMS.Backend.Common.Responses;

public class PagedResponse<T>
{
    public List<T> items { get; init; } = [];
    public int page { get; init; }
    public int pageSize { get; init; }
    public int totalItems { get; init; }
    public int totalPages { get; init; }
}
