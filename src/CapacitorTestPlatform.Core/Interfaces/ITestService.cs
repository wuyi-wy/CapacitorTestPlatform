using CapacitorTestPlatform.Core.Models;

namespace CapacitorTestPlatform.Core.Interfaces;

public interface ITestService
{
    void SaveTestRecords(IEnumerable<TestRecord> records);
    List<TestRecord> GetTestHistory(string planNo, string? specimenNumber = null);
    List<PlanInfo> GetSpecs(string planNo, string testItem);
    int GetNextSequence(string planNo, string specimenNumber, string testItem);
    List<TestRecord> GetUnSyncedRecords();
    void MarkSynced(List<int> ids);
}
