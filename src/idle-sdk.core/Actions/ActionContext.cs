namespace IdleSdk.Core.Actions;

public sealed class ActionContext
{
    public ActionContext(Guid profileId, DateTimeOffset startedAt)
    {
        ProfileId = profileId;
        StartedAt = startedAt;
    }

    public Guid ProfileId { get; }
    public DateTimeOffset StartedAt { get; }
}
