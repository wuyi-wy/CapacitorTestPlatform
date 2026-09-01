using CapacitorTestPlatform.Core.Models;

namespace CapacitorTestPlatform.Core.Interfaces;

/// <summary>
/// 计划业务服务接口，协调远程拉取和本地缓存。
/// </summary>
public interface IPlanService
{
    /// <summary>获取计划列表（优先本地，本地为空时从远程拉取）</summary>
    Task<List<PlanInfo>> GetPlansAsync();

    /// <summary>从远程导入指定批次号的计划到本地</summary>
    Task<int> ImportFromRemoteAsync(string lotNo);

    /// <summary>将本地待同步数据推送到远程</summary>
    Task SyncToRemoteAsync();

    /// <summary>获取本地所有不重复的批次号</summary>
    List<string> GetLots();

    /// <summary>按关键字搜索计划</summary>
    List<PlanInfo> Search(string keyword);
}
