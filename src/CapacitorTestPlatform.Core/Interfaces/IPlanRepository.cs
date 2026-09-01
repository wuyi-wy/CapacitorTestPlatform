using CapacitorTestPlatform.Core.Models;

namespace CapacitorTestPlatform.Core.Interfaces;

public interface IPlanRepository
{
    List<PlanInfo> GetAll();
    List<PlanInfo> GetByLot(string lot);
    void InsertBatch(IEnumerable<PlanInfo> plans);
    void DeleteByLot(string lot);
    List<string> GetDistinctLots();
    List<PlanInfo> Search(string keyword);
}
