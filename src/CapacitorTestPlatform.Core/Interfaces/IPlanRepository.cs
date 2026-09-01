using CapacitorTestPlatform.Core.Models;

namespace CapacitorTestPlatform.Core.Interfaces;

/// <summary>
/// 本地计划缓存仓储接口，对应 PlanCache 表。
/// </summary>
public interface IPlanRepository
{
    /// <summary>获取所有本地缓存的计划</summary>
    List<PlanInfo> GetAll();

    /// <summary>按批次号查询计划</summary>
    List<PlanInfo> GetByLot(string lot);

    /// <summary>批量插入计划到本地缓存</summary>
    void InsertBatch(IEnumerable<PlanInfo> plans);

    /// <summary>按批次号删除计划缓存</summary>
    void DeleteByLot(string lot);

    /// <summary>获取所有不重复的批次号列表</summary>
    List<string> GetDistinctLots();

    /// <summary>按关键字模糊搜索计划</summary>
    List<PlanInfo> Search(string keyword);
}
