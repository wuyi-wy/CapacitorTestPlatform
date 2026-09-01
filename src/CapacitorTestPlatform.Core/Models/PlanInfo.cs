namespace CapacitorTestPlatform.Core.Models;

/// <summary>
/// 检测计划信息，对应本地 PlanCache 表和远程 apl_contract_plan 视图。
/// 远程拉取时通过 SQL AS 别名映射到此模型的属性。
/// </summary>
public class PlanInfo
{
    /// <summary>主键（远程表自增ID，本地无实际用途）</summary>
    public int Id { get; set; }

    /// <summary>计划编号 / 合同号，对应 PlanCache.PlanNo</summary>
    public string PlanNo { get; set; } = string.Empty;

    /// <summary>产品型号</summary>
    public string? ProductModel { get; set; }

    /// <summary>测试项目，分号分隔（如 "电容量;漏电流;绝缘电阻"）</summary>
    public string? TestItems { get; set; }

    /// <summary>工站名称</summary>
    public string? Station { get; set; }

    /// <summary>计划状态（测试中/已完成/录入中）</summary>
    public string? Status { get; set; }

    /// <summary>产品信息 JSON（远程同步用）</summary>
    public string? ProductInfoJson { get; set; }

    /// <summary>同步时间</summary>
    public DateTime? SyncTime { get; set; }

    // ----- 以下属性对应 PlanCache 表的本地字段 -----

    /// <summary>批次号，对应 PlanCache.Lot</summary>
    public string? Lot { get; set; }

    /// <summary>设备编号，对应 PlanCache.DeviceId</summary>
    public string? DeviceId { get; set; }

    /// <summary>检测项序号，对应 PlanCache.ItemNo</summary>
    public string? ItemNo { get; set; }

    /// <summary>规格名称，对应 PlanCache.SpecName</summary>
    public string? SpecName { get; set; }

    /// <summary>规格值，对应 PlanCache.SpecValue</summary>
    public string? SpecValue { get; set; }

    /// <summary>规格下限，对应 PlanCache.SpecMin</summary>
    public string? SpecMin { get; set; }

    /// <summary>规格上限，对应 PlanCache.SpecMax</summary>
    public string? SpecMax { get; set; }

    /// <summary>规格单位，对应 PlanCache.SpecUnit</summary>
    public string? SpecUnit { get; set; }

    /// <summary>创建时间，对应 PlanCache.CreateTime</summary>
    public string? CreateTime { get; set; }

    /// <summary>
    /// 将 TestItems 按分号拆分为列表，用于 UI 绑定测试项选择。
    /// </summary>
    public List<string> TestItemList => string.IsNullOrEmpty(TestItems)
        ? new List<string>()
        : TestItems.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList();
}
