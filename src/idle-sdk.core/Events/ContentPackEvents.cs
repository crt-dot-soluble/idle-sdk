namespace IdleSdk.Core.Events;

public sealed record ContentPackLoaded(string PackId, bool Enabled);
public sealed record ContentPackEnabled(string PackId);
public sealed record ContentPackDisabled(string PackId);
public sealed record ContentPackHotReloaded(string PackId);
public sealed record ContentPackHotReloadFailed(string PackRoot);
