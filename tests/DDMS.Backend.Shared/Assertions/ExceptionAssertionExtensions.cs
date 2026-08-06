using DDMS.Backend.Common.Exceptions;
using FluentAssertions;

namespace DDMS.Backend.Shared.Assertions;

/// <summary>
/// Assertion helper riêng cho AppException/NotFoundException — gần như mọi equivalence class
/// "input không hợp lệ" trong Services đều throw 1 trong 2 loại này kèm theo mã lỗi (Code)
/// từ ErrorCode, nên cần assert cả type lẫn Code thay vì chỉ message.
/// </summary>
public static class ExceptionAssertionExtensions
{
    public static AppException ShouldBeAppException(this Exception exception, int expectedCode)
    {
        exception.Should().BeOfType<AppException>();
        var appEx = (AppException)exception;
        appEx.Code.Should().Be(expectedCode);
        return appEx;
    }

    public static NotFoundException ShouldBeNotFoundException(this Exception exception, int expectedCode = ErrorCode.ResourceNotFound)
    {
        exception.Should().BeOfType<NotFoundException>();
        var notFoundEx = (NotFoundException)exception;
        notFoundEx.Code.Should().Be(expectedCode);
        return notFoundEx;
    }

    public static ValidationException ShouldBeValidationExceptionWithField(this Exception exception, string fieldName)
    {
        exception.Should().BeOfType<ValidationException>();
        var validationEx = (ValidationException)exception;
        validationEx.FieldErrors.Should().NotBeNull();
        validationEx.FieldErrors!.Should().ContainKey(fieldName);
        return validationEx;
    }
}
