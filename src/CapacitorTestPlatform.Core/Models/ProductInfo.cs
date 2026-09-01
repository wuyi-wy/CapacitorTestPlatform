using System.Text.Json;

namespace CapacitorTestPlatform.Core.Models;

/// <summary>
/// 产品信息，包含电容器的标称值和各规格上下限。
/// 通过 JSON 序列化存储到远程数据库的 ProductInfoJson 字段。
/// </summary>
public class ProductInfo
{
    /// <summary>标称值</summary>
    public string? NominalValue { get; set; }

    /// <summary>电容正公差</summary>
    public string? CapacitancePositiveTolerance { get; set; }

    /// <summary>上限值</summary>
    public string? UpperLimit { get; set; }

    /// <summary>下限值</summary>
    public string? LowerLimit { get; set; }

    /// <summary>损耗标准</summary>
    public string? LossStandard { get; set; }

    /// <summary>阻抗标准</summary>
    public string? ImpedanceStandard { get; set; }

    public string ToJson() => JsonSerializer.Serialize(this);

    public static ProductInfo? FromJson(string? json) =>
        string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<ProductInfo>(json);
}
