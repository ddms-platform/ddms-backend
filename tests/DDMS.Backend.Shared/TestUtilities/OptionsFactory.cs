using Microsoft.Extensions.Options;

namespace DDMS.Backend.Shared.TestUtilities;

/// <summary>
/// Helper để dựng <see cref="IOptions{T}"/> khi mock constructor nhận IOptions&lt;T&gt;
/// (ví dụ BookingHoldOptions trong BookingService), tránh phải new Options.Create() lặp lại ở mọi test.
/// </summary>
public static class OptionsFactory
{
    public static IOptions<T> Create<T>(T value) where T : class => Options.Create(value);

    public static IOptions<T> CreateDefault<T>() where T : class, new() => Options.Create(new T());
}
