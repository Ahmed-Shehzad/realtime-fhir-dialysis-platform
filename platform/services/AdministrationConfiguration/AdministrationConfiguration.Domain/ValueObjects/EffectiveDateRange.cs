namespace AdministrationConfiguration.Domain.ValueObjects;

public readonly record struct EffectiveDateRange(DateTimeOffset? EffectiveFromUtc, DateTimeOffset? EffectiveToUtc)
{
    public void ThrowIfInvalid()
    {
        if (EffectiveFromUtc.HasValue
            && EffectiveToUtc.HasValue
            && EffectiveToUtc.Value < EffectiveFromUtc.Value) throw new ArgumentException("Effective end must not be before effective start.");
    }
}
