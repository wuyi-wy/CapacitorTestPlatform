using CapacitorTestPlatform.Core.Models;

namespace CapacitorTestPlatform.Core.Interfaces;

public interface IPlanService
{
    Task<List<PlanInfo>> GetPlansAsync();
    Task<int> ImportFromRemoteAsync(string lotNo);
    Task SyncToRemoteAsync();
    List<string> GetLots();
    List<PlanInfo> Search(string keyword);
}
