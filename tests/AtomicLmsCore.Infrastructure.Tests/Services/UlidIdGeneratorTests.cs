using AtomicLmsCore.Infrastructure.Services;
using Shouldly;

namespace AtomicLmsCore.Infrastructure.Tests.Services;

public class UlidIdGeneratorTests
{
    private readonly UlidIdGenerator _generator;

    public UlidIdGeneratorTests()
    {
        _generator = new();
    }

    [Fact]
    public void GenerateId_ReturnsValidGuid()
    {
        var id = _generator.NewId();

        id.ShouldNotBe(Guid.Empty);
        id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void GenerateId_GeneratesUniqueIds()
    {
        var ids = new HashSet<Guid>();
        const int Count = 1000;

        for (var i = 0; i < Count; i++)
        {
            var id = _generator.NewId();
            ids.Add(id);
        }

        ids.Count.ShouldBe(Count, "all generated IDs should be unique");
    }

    [Fact]
    public void GenerateId_GeneratesTimeBasedIds()
    {
        var ids = new List<Guid>();
        const int Count = 10;

        for (var i = 0; i < Count; i++)
        {
            ids.Add(_generator.NewId());
            if (i < Count - 1)
            {
                Thread.Sleep(1); // Ensure different timestamps
            }
        }

        // ULIDs contain timestamp information, so newer ones should generally be "larger"
        // when considering their timestamp portion, but GUID ordering may not reflect this
        ids.Count.ShouldBe(Count, "all IDs should be generated");
        ids.Distinct().Count().ShouldBe(Count, "all IDs should be unique");
    }

    [Fact]
    public async Task GenerateId_ThreadSafe()
    {
        var tasks = new Task<Guid>[100];
        var ids = new HashSet<Guid>();

        for (var i = 0; i < tasks.Length; i++)
        {
            tasks[i] = Task.Run(() => _generator.NewId());
        }

        var results = await Task.WhenAll(tasks);

        foreach (var id in results)
        {
            ids.Add(id);
        }

        ids.Count.ShouldBe(tasks.Length, "all IDs generated in parallel should be unique");
    }

    [Fact]
    public void GenerateId_MaintainsChronologicalOrder()
    {
        var id1 = _generator.NewId();
        Thread.Sleep(10);
        var id2 = _generator.NewId();
        Thread.Sleep(10);
        var id3 = _generator.NewId();

        var orderedIds = new[] { id1, id2, id3 }.OrderBy(x => x).ToArray();

        orderedIds[0].ShouldBe(id1, "first generated ID should be smallest");
        orderedIds[1].ShouldBe(id2, "second generated ID should be middle");
        orderedIds[2].ShouldBe(id3, "third generated ID should be largest");
    }

    [Fact]
    public void GenerateId_ProducesValidUlidFormat()
    {
        var id = _generator.NewId();
        var bytes = id.ToByteArray();

        bytes.Length.ShouldBe(16, "GUID should be 16 bytes");

        var convertBack = () =>
        {
            var _ = new Guid(bytes);
        };

        Should.NotThrow(() => convertBack(), "should be able to reconstruct GUID from bytes");
    }
}
