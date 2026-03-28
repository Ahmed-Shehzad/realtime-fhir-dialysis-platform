using MeasurementValidation.Domain;

using RealtimePlatform.IntegrationEventCatalog;

using Shouldly;

using Xunit;

namespace MeasurementValidation.UnitTests;

public sealed class ValidatedMeasurementRulesTests
{
    [Fact]
    public void Null_sample_quarantines_and_emits_quarantine_event()
    {
        Ulid c = Ulid.NewUlid();
        ValidatedMeasurement v = ValidatedMeasurement.Run(c, "m-1", "profile-a", null, "t1");
        v.Outcome.ShouldBe(MeasurementValidationOutcome.Quarantined);
        v.IntegrationEvents.OfType<MeasurementValidationQuarantinedIntegrationEvent>().ShouldNotBeEmpty();
    }

    [Fact]
    public void Negative_value_fails()
    {
        Ulid c = Ulid.NewUlid();
        ValidatedMeasurement v = ValidatedMeasurement.Run(c, "m-2", "profile-a", -1d, null);
        v.Outcome.ShouldBe(MeasurementValidationOutcome.Failed);
        v.IntegrationEvents.OfType<MeasurementValidationFailedIntegrationEvent>().ShouldNotBeEmpty();
    }

    [Fact]
    public void Value_above_1000_fails()
    {
        Ulid c = Ulid.NewUlid();
        ValidatedMeasurement v = ValidatedMeasurement.Run(c, "m-3", "profile-a", 1001d, null);
        v.Outcome.ShouldBe(MeasurementValidationOutcome.Failed);
    }

    [Fact]
    public void Value_in_elevated_band_flags_and_validates()
    {
        Ulid c = Ulid.NewUlid();
        ValidatedMeasurement v = ValidatedMeasurement.Run(c, "m-4", "profile-a", 501d, null);
        v.Outcome.ShouldBe(MeasurementValidationOutcome.Flagged);
        v.IntegrationEvents.OfType<MeasurementFlaggedIntegrationEvent>().ShouldNotBeEmpty();
        v.IntegrationEvents.OfType<MeasurementValidatedIntegrationEvent>().ShouldNotBeEmpty();
    }

    [Fact]
    public void Normal_value_passes()
    {
        Ulid c = Ulid.NewUlid();
        ValidatedMeasurement v = ValidatedMeasurement.Run(c, "m-5", "profile-a", 100d, null);
        v.Outcome.ShouldBe(MeasurementValidationOutcome.Passed);
        v.IntegrationEvents.OfType<MeasurementValidatedIntegrationEvent>().ShouldNotBeEmpty();
    }
}
