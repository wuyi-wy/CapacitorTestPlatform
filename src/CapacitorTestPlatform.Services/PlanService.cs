using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;
using CapacitorTestPlatform.Data.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapacitorTestPlatform.Services;

public class PlanService : IPlanService
{
    private readonly IPlanRepository _localRepo;
    private readonly RemotePlanRepository? _remoteRepo;
    private readonly string _station;

    public PlanService(IPlanRepository localRepo, RemotePlanRepository? remoteRepo, string station)
    {
        _localRepo = localRepo;
        _remoteRepo = remoteRepo;
        _station = station;
    }

    public async Task<List<PlanInfo>> GetPlansAsync()
    {
        return await Task.Run(() => _localRepo.GetAll());
    }

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

    public async Task SyncToRemoteAsync()
    {
        await Task.CompletedTask;
    }

    public List<string> GetLots()
    {
        return _localRepo.GetDistinctLots();
    }

    public List<PlanInfo> Search(string keyword)
    {
        return _localRepo.Search(keyword);
    }
}
