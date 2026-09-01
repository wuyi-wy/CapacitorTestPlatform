namespace CapacitorTestPlatform.Core.Models;

public class TestDataPoint
{
    public int Sequence { get; set; }
    public Dictionary<string, object?> Values { get; set; } = new();
}
