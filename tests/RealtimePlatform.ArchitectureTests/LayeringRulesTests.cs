using System.Reflection;

using NetArchTest.Rules;

using Shouldly;

using Xunit;

namespace RealtimePlatform.ArchitectureTests;

/// <summary>
/// Layering rules for shared platform libraries (Iteration 1 baseline).
/// </summary>
public sealed class LayeringRulesTests
{
    private static readonly Assembly Messaging = typeof(RealtimePlatform.Messaging.IEvent).Assembly;
    private static readonly Assembly MassTransitShared = typeof(RealtimePlatform.MassTransit.CatalogIntegrationEventTransport).Assembly;
    private static readonly Assembly Workflow = typeof(RealtimePlatform.Workflow.SagaLifecycleState).Assembly;
    private static readonly Assembly Redis = typeof(RealtimePlatform.Redis.RedisPlatformOptions).Assembly;

    private static string FormatFailures(TestResult result)
    {
        if (result.IsSuccessful) return string.Empty;
        return result.FailingTypes is null
            ? "Rule failed (no type list returned)."
            : string.Join(Environment.NewLine, result.FailingTypes.Select(t => t.FullName));
    }

    [Fact]
    public void Messaging_must_not_reference_ef_core()
    {
        TestResult result = Types.InAssembly(Messaging)
            .Should()
            .NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(FormatFailures(result));
    }

    [Fact]
    public void Workflow_must_not_reference_ef_core()
    {
        TestResult result = Types.InAssembly(Workflow)
            .Should()
            .NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(FormatFailures(result));
    }

    [Fact]
    public void Workflow_must_not_reference_observability_or_aspnet()
    {
        TestResult result = Types.InAssembly(Workflow)
            .Should()
            .NotHaveDependencyOn("Microsoft.AspNetCore")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(FormatFailures(result));
    }

    [Fact]
    public void Redis_must_not_reference_ef_core()
    {
        TestResult result = Types.InAssembly(Redis)
            .Should()
            .NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(FormatFailures(result));
    }

    [Fact]
    public void RealtimePlatform_MassTransit_shared_must_not_reference_aspnet()
    {
        TestResult result = Types.InAssembly(MassTransitShared)
            .Should()
            .NotHaveDependencyOn("Microsoft.AspNetCore")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(FormatFailures(result));
    }
}
