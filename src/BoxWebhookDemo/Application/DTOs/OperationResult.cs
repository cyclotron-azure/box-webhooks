namespace BoxWebhookDemo.Application.DTOs;

/// <summary>
/// Generic result wrapper for operations.
/// Follows the Result pattern for explicit error handling.
/// </summary>
public class OperationResult<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }

    private OperationResult(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static OperationResult<T> Success(T value) =>
        new(true, value, null);

    public static OperationResult<T> Failure(string error) =>
        new(false, default, error);
}

/// <summary>
/// Non-generic result for void operations.
/// </summary>
public class OperationResult
{
    public bool IsSuccess { get; }
    public string? Error { get; }

    private OperationResult(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static OperationResult Success() =>
        new(true, null);

    public static OperationResult Failure(string error) =>
        new(false, error);
}
