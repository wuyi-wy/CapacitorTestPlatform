using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;
using CapacitorTestPlatform.Data.Repositories;

namespace CapacitorTestPlatform.Services;

public class TestService : ITestService
{
    private readonly TestRecordRepository _testRecordRepo;
    private readonly ITestHistoryRepository _historyRepo;

    public TestService(TestRecordRepository testRecordRepo, ITestHistoryRepository historyRepo)
    {
        _testRecordRepo = testRecordRepo;
        _historyRepo = historyRepo;
    }

    public void SaveTestRecords(IEnumerable<TestRecord> records)
    {
        _testRecordRepo.InsertRecords(records);
    }

    public List<TestRecord> GetTestHistory(string planNo, string? specimenNumber = null)
    {
        return _historyRepo.GetByPlanNo(planNo, specimenNumber);
    }

    public List<PlanInfo> GetSpecs(string planNo, string testItem)
    {
        return new List<PlanInfo>();
    }

    public int GetNextSequence(string planNo, string specimenNumber, string testItem)
    {
        return _historyRepo.GetNextSequence(planNo, specimenNumber, testItem);
    }

    public List<TestRecord> GetUnSyncedRecords()
    {
        return _testRecordRepo.GetUnSynced();
    }

    public void MarkSynced(List<int> ids)
    {
        _testRecordRepo.MarkSynced(ids);
    }
}
