namespace CapacitorTestPlatform.Core.Models;

public class PlanInfo
{
    public int Id { get; set; }
    public string PlanNo { get; set; } = string.Empty;
    public string? ProductModel { get; set; }
    public string? TestItems { get; set; }
    public string? Station { get; set; }
    public string? Status { get; set; }
    public string? ProductInfoJson { get; set; }
    public DateTime? SyncTime { get; set; }

    public List<string> TestItemList => string.IsNullOrEmpty(TestItems)
        ? new List<string>()
        : TestItems.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList();
}
