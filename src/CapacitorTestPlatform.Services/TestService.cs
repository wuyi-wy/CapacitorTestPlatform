using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;
using CapacitorTestPlatform.Data.Repositories;

namespace CapacitorTestPlatform.Services;

/// <summary>
/// 测试业务服务，负责测试记录的保存、查询和同步状态管理。
/// </summary>
public class TestService : ITestService
{
    private readonly TestRecordRepository _testRecordRepo;
    private readonly ITestHistoryRepository _historyRepo;

    /// <summary>
    /// 初始化测试服务。
    /// </summary>
    /// <param name="testRecordRepo">测试记录仓储（本地 TBL_CHECKDATA）</param>
    /// <param name="historyRepo">测试历史仓储，用于查询历史记录和序号</param>
    public TestService(TestRecordRepository testRecordRepo, ITestHistoryRepository historyRepo)
    {
        _testRecordRepo = testRecordRepo;
        _historyRepo = historyRepo;
    }

    /// <summary>
    /// 批量保存测试记录到本地数据库。
    /// </summary>
    /// <param name="records">待保存的测试记录集合</param>
    public void SaveTestRecords(IEnumerable<TestRecord> records)
    {
        _testRecordRepo.InsertRecords(records);
    }

    /// <summary>
    /// 按计划编号查询测试历史记录。
    /// </summary>
    /// <param name="planNo">计划编号</param>
    /// <param name="specimenNumber">样品编号（可选，为空时返回该计划下所有记录）</param>
    /// <returns>匹配的测试记录列表</returns>
    public List<TestRecord> GetTestHistory(string planNo, string? specimenNumber = null)
    {
        return _historyRepo.GetByPlanNo(planNo, specimenNumber);
    }

    /// <summary>
    /// 获取指定计划和测试项的规格信息（预留接口，暂返回空列表）。
    /// </summary>
    /// <param name="planNo">计划编号</param>
    /// <param name="testItem">测试项名称</param>
    /// <returns>规格信息列表</returns>
    public List<PlanInfo> GetSpecs(string planNo, string testItem)
    {
        return new List<PlanInfo>();
    }

    /// <summary>
    /// 获取下一个测试序号，用于自动递增编号。
    /// </summary>
    /// <param name="planNo">计划编号</param>
    /// <param name="specimenNumber">样品编号</param>
    /// <param name="testItem">测试项名称</param>
    /// <returns>下一个可用的序号</returns>
    public int GetNextSequence(string planNo, string specimenNumber, string testItem)
    {
        return _historyRepo.GetNextSequence(planNo, specimenNumber, testItem);
    }

    /// <summary>
    /// 获取所有未同步到远程数据库的测试记录。
    /// </summary>
    /// <returns>未同步的测试记录列表</returns>
    public List<TestRecord> GetUnSyncedRecords()
    {
        return _testRecordRepo.GetUnSynced();
    }

    /// <summary>
    /// 将指定记录标记为已同步。
    /// </summary>
    /// <param name="ids">需要标记为已同步的记录 ID 列表</param>
    public void MarkSynced(List<int> ids)
    {
        _testRecordRepo.MarkSynced(ids);
    }
}
