using AtomicLmsCore.WebApi.Common;
using FluentAssertions;

namespace AtomicLmsCore.WebApi.Tests.Common;

public class FeatureBucketPathsTests
{
    [Fact]
    public void FeatureBucketNames_AreCorrect()
    {
        // Assert
        FeatureBucketPaths.Solution.Should().Be("solution");
        FeatureBucketPaths.Administration.Should().Be("administration");
        FeatureBucketPaths.Learning.Should().Be("learning");
        FeatureBucketPaths.Learners.Should().Be("learners");
        FeatureBucketPaths.Engagement.Should().Be("engagement");
    }

    [Fact]
    public void RouteTemplates_AreCorrect()
    {
        // Assert
        FeatureBucketPaths.SolutionRoute.Should().Be("api/v{version:apiVersion}/solution/[controller]");
        FeatureBucketPaths.AdministrationRoute.Should().Be("api/v{version:apiVersion}/administration/[controller]");
        FeatureBucketPaths.LearningRoute.Should().Be("api/v{version:apiVersion}/learning/[controller]");
        FeatureBucketPaths.LearnersRoute.Should().Be("api/v{version:apiVersion}/learners/[controller]");
        FeatureBucketPaths.EngagementRoute.Should().Be("api/v{version:apiVersion}/engagement/[controller]");
    }


    [Theory]
    [InlineData("solution")]
    [InlineData("administration")]
    [InlineData("learning")]
    [InlineData("learners")]
    [InlineData("engagement")]
    public void FeatureBucketNames_AreConsistent(string expectedName)
    {
        // Act & Assert - Verify the constant matches the expected naming convention
        var property = typeof(FeatureBucketPaths).GetField(char.ToUpper(expectedName[0]) + expectedName[1..]);
        property.Should().NotBeNull($"Feature bucket '{expectedName}' should have a corresponding constant");

        var value = property!.GetValue(null)?.ToString();
        value.Should().Be(expectedName);
    }

    [Fact]
    public void RouteTemplates_FollowConsistentPattern()
    {
        // Arrange
        var routes = new[]
        {
            FeatureBucketPaths.SolutionRoute,
            FeatureBucketPaths.AdministrationRoute,
            FeatureBucketPaths.LearningRoute,
            FeatureBucketPaths.LearnersRoute,
            FeatureBucketPaths.EngagementRoute
        };

        // Assert
        foreach (var route in routes)
        {
            route.Should().StartWith("api/v{version:apiVersion}/");
            route.Should().EndWith("/[controller]");
            route.Should().MatchRegex(@"^api/v\{version:apiVersion\}/\w+/\[controller\]$");
        }
    }


    [Theory]
    [InlineData("SolutionRoute", "solution")]
    [InlineData("AdministrationRoute", "administration")]
    [InlineData("LearningRoute", "learning")]
    [InlineData("LearnersRoute", "learners")]
    [InlineData("EngagementRoute", "engagement")]
    public void RouteTemplates_ContainCorrectFeatureBucketName(string routePropertyName, string expectedFeatureBucket)
    {
        // Arrange
        var routeProperty = typeof(FeatureBucketPaths).GetField(routePropertyName);
        var routeValue = routeProperty!.GetValue(null)?.ToString();

        // Assert
        routeValue.Should().Contain($"/{expectedFeatureBucket}/");
    }

    [Fact]
    public void AllConstants_AreNotNullOrEmpty()
    {
        // Arrange
        var stringFields = typeof(FeatureBucketPaths).GetFields()
            .Where(f => f.FieldType == typeof(string) && f.IsStatic && f.IsLiteral);

        // Act & Assert
        foreach (var field in stringFields)
        {
            var value = field.GetValue(null)?.ToString();
            value.Should().NotBeNullOrEmpty($"Constant {field.Name} should not be null or empty");
        }
    }

    [Fact]
    public void FeatureBucketNames_AreLowercase()
    {
        // Arrange
        var featureBuckets = new[]
        {
            FeatureBucketPaths.Solution,
            FeatureBucketPaths.Administration,
            FeatureBucketPaths.Learning,
            FeatureBucketPaths.Learners,
            FeatureBucketPaths.Engagement
        };

        // Assert
        foreach (var bucket in featureBuckets)
        {
            bucket.Should().Be(bucket.ToLowerInvariant(),
                $"Feature bucket names should be lowercase for URL consistency");
        }
    }
}
