namespace AtomicLmsCore.WebApi.Common;

public static class FeatureBucketPaths
{
    public const string Solution = "solution";
    public const string Administration = "administration";
    public const string Learning = "learning";
    public const string Learners = "learners";
    public const string Engagement = "engagement";

    public const string SolutionRoute = $"api/v{{version:apiVersion}}/{Solution}/[controller]";
    public const string AdministrationRoute = $"api/v{{version:apiVersion}}/{Administration}/[controller]";
    public const string LearningRoute = $"api/v{{version:apiVersion}}/{Learning}/[controller]";
    public const string LearnersRoute = $"api/v{{version:apiVersion}}/{Learners}/[controller]";
    public const string EngagementRoute = $"api/v{{version:apiVersion}}/{Engagement}/[controller]";

    internal const string SolutionPathPattern = "/solution/";
}
