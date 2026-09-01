namespace CapacitorTestPlatform.Core.Models;

public class TestHistory
{
    public int Id { get; set; }
    public int TestRecordId { get; set; }
    public string SnapshotJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
