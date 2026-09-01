using CapacitorTestPlatform.Core.Models;

namespace CapacitorTestPlatform.Core.Interfaces;

public interface ITestHistoryRepository
{
    List<TestRecord> GetByPlanNo(string planNo, string? specimenNumber = null);
    List<TestRecord> GetByDateRange(string startDate, string endDate);
    List<TestRecord> Search(string keyword);
    int GetNextSequence(string planNo, string specimenNumber, string testItem);
    List<string> GetDistinctPlanNos();
}
