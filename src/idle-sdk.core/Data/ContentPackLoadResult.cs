namespace IdleSdk.Core.Data;

public sealed record ContentPackLoadResult(bool IsValid, ContentPackState? Pack, IReadOnlyList<ValidationError> Errors)
{
    public static ContentPackLoadResult Success(ContentPackState pack) => new(true, pack, Array.Empty<ValidationError>());
    public static ContentPackLoadResult Failure(IEnumerable<ValidationError> errors) => new(false, null, errors.ToList());
}
