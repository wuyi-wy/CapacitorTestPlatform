using System.Text.Json;

namespace CapacitorTestPlatform.Core.Models;

public class ProductInfo
{
    public string? NominalValue { get; set; }
    public string? CapacitancePositiveTolerance { get; set; }
    public string? UpperLimit { get; set; }
    public string? LowerLimit { get; set; }
    public string? LossStandard { get; set; }
    public string? ImpedanceStandard { get; set; }

    public string ToJson() => JsonSerializer.Serialize(this);

    public static ProductInfo? FromJson(string? json) =>
        string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<ProductInfo>(json);
}
