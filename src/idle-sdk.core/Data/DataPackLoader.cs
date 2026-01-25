namespace IdleSdk.Core.Data;

public sealed class DataPackLoader
{
    private readonly DataPackValidator _validator;

    public DataPackLoader(DataPackValidator validator)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    public DataPackLoadResult LoadFromJson(string json)
    {
        var result = _validator.ValidateJson(json);
        return result.IsValid ? DataPackLoadResult.Success(json) : DataPackLoadResult.Failure(result.Errors);
    }
}
