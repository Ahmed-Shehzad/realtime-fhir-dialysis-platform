using MeasurementAcquisition.Application.Commands.IngestMeasurement;

using Shouldly;

using Xunit;

namespace MeasurementAcquisition.UnitTests;

public sealed class ApplicationAssemblySmokeTests
{
    [Fact]
    public void Ingest_measurement_command_resides_in_application_layer()
    {
        typeof(IngestMeasurementPayloadCommand).Assembly.GetName().Name.ShouldBe("MeasurementAcquisition.Application");
    }
}
