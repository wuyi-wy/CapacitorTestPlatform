using CapacitorTestPlatform.Core.Models;

namespace CapacitorTestPlatform.Core.Interfaces;

/// <summary>
/// 测试业务服务接口，封装检测数据的存储和查询逻辑。
/// </summary>
public interface ITestService
{
    /// <summary>保存检测记录到本地数据库</summary>
    void SaveTestRecords(IEnumerable<TestRecord> records);

    /// <summary>获取测试历史（按计划号，可选按试件编号过滤）</summary>
    List<TestRecord> GetTestHistory(string planNo, string? specimenNumber = null);

    /// <summary>获取计划对应的规格信息</summary>
    List<PlanInfo> GetSpecs(string planNo, string testItem);

    /// <summary>获取下一个序号</summary>
    int GetNextSequence(string planNo, string specimenNumber, string testItem);

    /// <summary>获取未同步的检测记录</summary>
    List<TestRecord> GetUnSyncedRecords();

    /// <summary>将指定记录标记为已同步</summary>
    void MarkSynced(List<int> ids);
}
