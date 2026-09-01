using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;
using CapacitorTestPlatform.Data.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapacitorTestPlatform.Services;

/// <summary>
/// 计划业务服务，协调远程拉取和本地缓存。
/// </summary>
public class PlanService : IPlanService
{
    private readonly IPlanRepository _localRepo;
    private readonly RemotePlanRepository? _remoteRepo;
    private readonly string _station;

    /// <summary>
    /// 初始化计划服务。
    /// </summary>
    /// <param name="localRepo">本地 SQLite 计划仓储</param>
    /// <param name="remoteRepo">远程 SQL Server 计划仓储（可为 null，表示未配置远程连接）</param>
    /// <param name="station">当前工站编号，用于远程拉取时筛选计划</param>
    public PlanService(IPlanRepository localRepo, RemotePlanRepository? remoteRepo, string station)
    {
        _localRepo = localRepo;
        _remoteRepo = remoteRepo;
        _station = station;
    }

    /// <summary>
    /// 获取计划列表（从本地 SQLite 查询）。
    /// </summary>
    /// <returns>所有本地计划信息列表</returns>
    public async Task<List<PlanInfo>> GetPlansAsync()
    {
        return await Task.Run(() => _localRepo.GetAll());
    }

    /// <summary>
    /// 从远程数据库拉取当前工站的计划并保存到本地。
    /// </summary>
    /// <param name="lotNo">批次号，用于清除本地旧数据后重新导入</param>
    /// <returns>导入的计划数量</returns>
    /// <exception cref="InvalidOperationException">远程数据库未配置时抛出</exception>
    public async Task<int> ImportFromRemoteAsync(string lotNo)
    {
        if (_remoteRepo == null)
            throw new InvalidOperationException("远程数据库未配置");

        var plans = await _remoteRepo.FetchPlansAsync(_station);
        if (!plans.Any()) return 0;

        _localRepo.DeleteByLot(lotNo);
        _localRepo.InsertBatch(plans.ToList());
        return plans.Count();
    }

    /// <summary>
    /// 将本地测试结果同步到远程数据库（预留接口，暂未实现）。
    /// </summary>
    public async Task SyncToRemoteAsync()
    {
        await Task.CompletedTask;
    }

    /// <summary>
    /// 获取本地所有不重复的批次号列表。
    /// </summary>
    /// <returns>批次号字符串列表</returns>
    public List<string> GetLots()
    {
        return _localRepo.GetDistinctLots();
    }

    /// <summary>
    /// 按关键词搜索本地计划（模糊匹配计划编号或产品名称）。
    /// </summary>
    /// <param name="keyword">搜索关键词</param>
    /// <returns>匹配的计划信息列表</returns>
    public List<PlanInfo> Search(string keyword)
    {
        return _localRepo.Search(keyword);
    }
}
