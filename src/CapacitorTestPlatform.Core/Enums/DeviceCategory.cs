namespace CapacitorTestPlatform.Core.Enums;

/// <summary>
/// 设备分类枚举，用于 UI 设备选择下拉和 MockDriver 结果模拟。
/// </summary>
public enum DeviceCategory
{
    /// <summary>LCR 数字电桥（TH2817A/TH2832/TH2810B）</summary>
    LcrBridge,

    /// <summary>漏电流测试仪（TH2683A）</summary>
    LeakCurrentTester,

    /// <summary>绝缘电阻测试仪（TH2689）</summary>
    InsulationResistanceTester,

    /// <summary>安全测试仪 / 耐压测试仪（TH9201）</summary>
    SafetyTester
}
