namespace IdleSdk.Core.Data;

public sealed record DataPackLoadResult(bool IsValid, string? Payload, IReadOnlyList<ValidationError> Errors)
{
    public static DataPackLoadResult Success(string payload) => new(true, payload, Array.Empty<ValidationError>());
    public static DataPackLoadResult Failure(IEnumerable<ValidationError> errors) => new(false, null, errors.ToList());
}
