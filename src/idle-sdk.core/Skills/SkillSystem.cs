namespace IdleSdk.Core.Skills;

public sealed class SkillSystem
{
    private readonly SkillRegistry _registry;
    private readonly IXpCurve _xpCurve;
    private readonly Dictionary<string, SkillProgress> _progress = new(StringComparer.OrdinalIgnoreCase);

    public SkillSystem(SkillRegistry registry, IXpCurve xpCurve)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _xpCurve = xpCurve ?? throw new ArgumentNullException(nameof(xpCurve));
    }

    public SkillProgress GetOrCreateProgress(string skillId)
    {
        if (_progress.TryGetValue(skillId, out var progress))
        {
            return progress;
        }

        var definition = _registry.Get(skillId);
        progress = new SkillProgress(definition, _xpCurve);
        _progress[skillId] = progress;
        return progress;
    }

    public void AddXp(string skillId, long amount)
    {
        var progress = GetOrCreateProgress(skillId);
        progress.AddXp(amount);
    }
}
