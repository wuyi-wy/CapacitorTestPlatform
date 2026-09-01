namespace CapacitorTestPlatform.Core.Models;

/// <summary>
/// 测试列定义，描述 DataGrid 中一列的表头、绑定属性、单位和规格范围。
/// 用于动态生成测试数据表格的列。
/// </summary>
public class TestColumnDefinition
{
    /// <summary>列标题（如 "C"）</summary>
    public string Header { get; set; } = string.Empty;

    /// <summary>绑定属性名（如 "C(μF)"）</summary>
    public string PropertyName { get; set; } = string.Empty;

    /// <summary>单位（如 "μF"）</summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>列宽度</summary>
    public int Width { get; set; } = 120;

    /// <summary>是否有规格范围</summary>
    public bool HasRange { get; set; }

    /// <summary>范围描述文本</summary>
    public string? RangeDescription { get; set; }

    /// <summary>规格下限</summary>
    public string? SpecMin { get; set; }

    /// <summary>规格上限</summary>
    public string? SpecMax { get; set; }

    /// <summary>带单位的显示表头（如 "C(μF)"）</summary>
    public string DisplayHeader => string.IsNullOrEmpty(Unit) ? Header : $"{Header}({Unit})";
}
