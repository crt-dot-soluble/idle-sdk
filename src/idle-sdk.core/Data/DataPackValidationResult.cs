namespace IdleSdk.Core.Data;

public sealed record DataPackValidationResult(bool IsValid, IReadOnlyList<ValidationError> Errors)
{
    public static DataPackValidationResult Success() => new(true, Array.Empty<ValidationError>());

    public static DataPackValidationResult Failure(IEnumerable<ValidationError> errors)
        => new(false, errors.ToList());
}
