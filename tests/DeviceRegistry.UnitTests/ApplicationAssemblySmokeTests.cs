using DeviceRegistry.Application.Commands.RegisterDevice;

using Shouldly;

using Xunit;

namespace DeviceRegistry.UnitTests;

public sealed class ApplicationAssemblySmokeTests
{
    [Fact]
    public void RegisterDevice_command_resides_in_application_layer()
    {
        typeof(RegisterDeviceCommand).Assembly.GetName().Name.ShouldBe("DeviceRegistry.Application");
    }
}
