namespace CapacitorTestPlatform.Core.Enums;

/// <summary>
/// 测试项枚举，对应产品需要执行的检测项目。
/// </summary>
public enum TestItem
{
    /// <summary>电容量</summary>
    Capacitance,

    /// <summary>损耗角正切（tgδ）</summary>
    LossTangent,

    /// <summary>等效串联电阻</summary>
    ESR,

    /// <summary>阻抗</summary>
    Impedance,

    /// <summary>绝缘电阻</summary>
    InsulationResistance,

    /// <summary>漏电流</summary>
    LeakCurrent,

    /// <summary>可靠性前测试</summary>
    PreReliability,

    /// <summary>可靠性后测试</summary>
    PostReliability
}
