namespace CapacitorTestPlatform.Core.Models;

/// <summary>
/// 设备测量结果，由驱动层返回给业务层。
/// Data 字典的 Key 为测量项名称（如 "C(μF)"），Value 为测量值。
/// </summary>
public class DeviceTestResult
{
    /// <summary>测量是否成功</summary>
    public bool Success { get; set; }

    /// <summary>测量数据，Key=测量项名称, Value=测量值</summary>
    public Dictionary<string, object?> Data { get; set; } = new();

    /// <summary>失败时的错误信息</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>创建成功结果</summary>
    public static DeviceTestResult Ok(Dictionary<string, object?> data) => new() { Success = true, Data = data };

    /// <summary>创建失败结果</summary>
    public static DeviceTestResult Fail(string error) => new() { Success = false, ErrorMessage = error };
}
