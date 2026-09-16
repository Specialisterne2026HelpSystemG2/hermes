namespace Hermes.Common;

/// <summary>
/// Result of a service operation, avoiding exceptions for predictable outcomes
/// such as a duplicate email or a missing record.
///
/// <see cref="FieldErrors"/> maps a form property name to a message so the page
/// can show the error against the right field instead of only in a banner.
/// </summary>
public class OperationResult
{
    public bool Succeeded { get; init; }

    public string? ErrorMessage { get; init; }

    public IReadOnlyDictionary<string, string> FieldErrors { get; init; }
        = new Dictionary<string, string>();

    public static OperationResult Success() => new() { Succeeded = true };

    public static OperationResult Failure(string message) =>
        new() { Succeeded = false, ErrorMessage = message };

    public static OperationResult FieldFailure(string field, string message) => new()
    {
        Succeeded = false,
        ErrorMessage = message,
        FieldErrors = new Dictionary<string, string> { [field] = message }
    };
}

public class OperationResult<T> : OperationResult
{
    public T? Value { get; init; }

    public static OperationResult<T> Success(T value) =>
        new() { Succeeded = true, Value = value };

    public static new OperationResult<T> Failure(string message) =>
        new() { Succeeded = false, ErrorMessage = message };

    public static new OperationResult<T> FieldFailure(string field, string message) => new()
    {
        Succeeded = false,
        ErrorMessage = message,
        FieldErrors = new Dictionary<string, string> { [field] = message }
    };
}
