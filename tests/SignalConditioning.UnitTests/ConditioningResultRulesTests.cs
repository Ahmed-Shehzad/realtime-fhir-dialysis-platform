using RealtimePlatform.IntegrationEventCatalog;

using SignalConditioning.Domain;

using Shouldly;

using Xunit;

namespace SignalConditioning.UnitTests;

public sealed class ConditioningResultRulesTests
{
    [Fact]
    public void Null_sample_is_dropout_only()
    {
        Ulid c = Ulid.NewUlid();
        ConditioningResult r = ConditioningResult.Run(c, "m-1", "ch-a", null, null, null);
        r.IsDropout.ShouldBeTrue();
        r.IntegrationEvents.OfType<SignalDropoutDetectedIntegrationEvent>().ShouldNotBeEmpty();
        r.IntegrationEvents.OfType<SignalConditionedIntegrationEvent>().ShouldBeEmpty();
    }

    [Fact]
    public void Large_step_from_previous_emits_drift_and_conditions()
    {
        Ulid c = Ulid.NewUlid();
        ConditioningResult r = ConditioningResult.Run(c, "m-2", "ch-a", 100d, 400d, null);
        r.IsDropout.ShouldBeFalse();
        r.DriftDetected.ShouldBeTrue();
        r.IntegrationEvents.OfType<SignalDriftDetectedIntegrationEvent>().ShouldNotBeEmpty();
        r.IntegrationEvents.OfType<SignalQualityCalculatedIntegrationEvent>().ShouldNotBeEmpty();
        r.IntegrationEvents.OfType<SignalConditionedIntegrationEvent>().ShouldNotBeEmpty();
    }

    [Fact]
    public void Small_step_no_drift_still_conditions()
    {
        Ulid c = Ulid.NewUlid();
        ConditioningResult r = ConditioningResult.Run(c, "m-3", "ch-a", 100d, 105d, null);
        r.DriftDetected.ShouldBeFalse();
        r.IntegrationEvents.OfType<SignalDriftDetectedIntegrationEvent>().ShouldBeEmpty();
        r.IntegrationEvents.OfType<SignalConditionedIntegrationEvent>().ShouldNotBeEmpty();
    }
}
