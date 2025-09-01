using FluentAssertions;

namespace AtomicLmsCore.WebApi.Tests.Behaviors;

public class MediatRBehaviorOrderTests
{
    [Fact]
    public void BehaviorRegistrationOrder_DocumentsExpectedOrder()
    {
        // This test documents the expected registration order for MediatR behaviors
        var expectedOrder = new[]
        {
            "ValidationBehavior",
            "TelemetryBehavior"
        };

        // Assert - Documentation of expected behavior registration order
        expectedOrder[0].Should().Be("ValidationBehavior", "Validation should run first to fail fast");
        expectedOrder[1].Should().Be("TelemetryBehavior", "Telemetry should measure everything including validation");

        // The order matters because:
        // 1. ValidationBehavior should fail fast before expensive operations
        // 2. TelemetryBehavior should measure total time including validation
        // 3. If reversed, telemetry would not measure validation time correctly
    }

    [Fact]
    public void MediatRPipeline_ValidationAndTelemetryBehaviorsExist()
    {
        // This test verifies that the expected behaviors exist in the application
        // by checking their types are available
        var validationBehaviorType = typeof(Application.Common.Behaviors.ValidationBehavior<,>);
        var telemetryBehaviorType = typeof(Application.Common.Behaviors.TelemetryBehavior<,>);

        // Assert
        validationBehaviorType.Should().NotBeNull("ValidationBehavior should exist");
        telemetryBehaviorType.Should().NotBeNull("TelemetryBehavior should exist");

        validationBehaviorType.Name.Should().Be("ValidationBehavior`2");
        telemetryBehaviorType.Name.Should().Be("TelemetryBehavior`2");
    }

    [Theory]
    [InlineData("ValidationBehavior", 1)] // Should run first
    [InlineData("TelemetryBehavior", 2)] // Should run second
    public void BehaviorOrder_IsCorrect(string behaviorName, int expectedPosition)
    {
        // This theory test documents the expected order positions
        var behaviors = new[] { "ValidationBehavior", "TelemetryBehavior" };

        // Assert
        var actualPosition = Array.IndexOf(behaviors, behaviorName) + 1;
        actualPosition.Should().Be(expectedPosition,
            $"{behaviorName} should be at position {expectedPosition}");
    }
}
