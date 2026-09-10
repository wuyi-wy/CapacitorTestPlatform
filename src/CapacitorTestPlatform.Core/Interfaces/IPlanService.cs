using CapacitorTestPlatform.Core.Models;

namespace CapacitorTestPlatform.Core.Interfaces;

/// <summary>
/// 计划业务服务接口，协调远程拉取和本地缓存。
/// </summary>
public interface IPlanService
{
    /// <summary>获取计划列表（本地数据）</summary>
    Task<List<PlanInfo>> GetPlansAsync();

    /// <summary>从远程数据库拉取当前工站的计划并同步保存到本地，远程不可用时返回 null</summary>
    Task<List<PlanInfo>?> FetchFromRemoteAsync();

    /// <summary>从远程导入指定批次号的计划到本地</summary>
    Task<int> ImportFromRemoteAsync(string lotNo);

    /// <summary>将本地待同步数据推送到远程</summary>
    Task SyncToRemoteAsync();

    /// <summary>获取本地所有不重复的批次号</summary>
    List<string> GetLots();

    /// <summary>按关键字搜索计划</summary>
    List<PlanInfo> Search(string keyword);
}
