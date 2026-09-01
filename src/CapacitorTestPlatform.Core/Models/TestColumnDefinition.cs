namespace CapacitorTestPlatform.Core.Models;

public class TestColumnDefinition
{
    public string Header { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public int Width { get; set; } = 120;
    public bool HasRange { get; set; }
    public string? RangeDescription { get; set; }
    public string? SpecMin { get; set; }
    public string? SpecMax { get; set; }

    public string DisplayHeader => string.IsNullOrEmpty(Unit) ? Header : $"{Header}({Unit})";
}
