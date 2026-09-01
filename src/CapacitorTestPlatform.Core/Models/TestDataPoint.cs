namespace CapacitorTestPlatform.Core.Models;

/// <summary>
/// 测试数据点，包含序号和一组测量值。
/// 用于单次设备采集的结构化数据传输。
/// </summary>
public class TestDataPoint
{
    /// <summary>数据点序号</summary>
    public int Sequence { get; set; }

    /// <summary>测量值集合，Key=测量项名称, Value=测量值</summary>
    public Dictionary<string, object?> Values { get; set; } = new();
}
