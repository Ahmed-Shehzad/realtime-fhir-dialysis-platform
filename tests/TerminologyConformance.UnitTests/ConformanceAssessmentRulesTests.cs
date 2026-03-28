using RealtimePlatform.IntegrationEventCatalog;

using TerminologyConformance.Domain;

using Shouldly;

using Xunit;

namespace TerminologyConformance.UnitTests;

public sealed class ConformanceAssessmentRulesTests
{
    [Fact]
    public void No_terminology_or_profile_slices_skips_both_without_catalog_events()
    {
        Ulid c = Ulid.NewUlid();
        ConformanceAssessment a = ConformanceAssessment.Run(c, "res-1", null, null, null, null, null);
        a.TerminologySliceOutcome.ShouldBe(ConformanceSliceOutcome.Skipped);
        a.ProfileSliceOutcome.ShouldBe(ConformanceSliceOutcome.Skipped);
        a.IntegrationEvents.ShouldBeEmpty();
    }

    [Fact]
    public void Valid_code_and_system_emits_terminology_validated()
    {
        Ulid c = Ulid.NewUlid();
        ConformanceAssessment a = ConformanceAssessment.Run(
            c,
            "res-2",
            "http://loinc.org",
            "8480-6",
            null,
            null,
            "t1");
        a.TerminologySliceOutcome.ShouldBe(ConformanceSliceOutcome.Passed);
        a.IntegrationEvents.OfType<TerminologyValidatedIntegrationEvent>().ShouldNotBeEmpty();
    }

    [Fact]
    public void Invalid_code_emits_terminology_failed()
    {
        Ulid c = Ulid.NewUlid();
        ConformanceAssessment a = ConformanceAssessment.Run(
            c,
            "res-3",
            "http://loinc.org",
            "INVALID-CODE",
            null,
            null,
            null);
        a.TerminologySliceOutcome.ShouldBe(ConformanceSliceOutcome.Failed);
        a.IntegrationEvents.OfType<TerminologyValidationFailedIntegrationEvent>().ShouldNotBeEmpty();
    }

    [Fact]
    public void Profile_non_conformant_emits_profile_failed()
    {
        Ulid c = Ulid.NewUlid();
        const string profile = "http://example.org/fhir/StructureDefinition/non-conformant-observation";
        ConformanceAssessment a = ConformanceAssessment.Run(c, "res-4", null, null, null, profile, null);
        a.ProfileSliceOutcome.ShouldBe(ConformanceSliceOutcome.Failed);
        a.IntegrationEvents.OfType<ProfileConformanceFailedIntegrationEvent>().ShouldNotBeEmpty();
    }

    [Fact]
    public void Valid_profile_emits_profile_validated()
    {
        Ulid c = Ulid.NewUlid();
        const string profile = "http://hl7.org/fhir/StructureDefinition/Observation";
        ConformanceAssessment a = ConformanceAssessment.Run(c, "res-5", "http://loinc.org", "8480-6", null, profile, null);
        a.ProfileSliceOutcome.ShouldBe(ConformanceSliceOutcome.Passed);
        a.IntegrationEvents.OfType<ProfileConformanceValidatedIntegrationEvent>().ShouldNotBeEmpty();
    }
}
