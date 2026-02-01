namespace IdleSdk.Core.Modules;

public sealed record ModuleDependency(string Id, Version MinimumVersion)
{
	[Obsolete("Use Id instead.")]
	public string Name => Id;
}
