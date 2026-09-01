using CapacitorTestPlatform.Core.Models;

namespace CapacitorTestPlatform.Core.Interfaces;

/// <summary>
/// 测试历史仓储接口，对应 CheckData 表的查询操作。
/// </summary>
public interface ITestHistoryRepository
{
    /// <summary>按计划号查询，可选按试件编号过滤</summary>
    List<TestRecord> GetByPlanNo(string planNo, string? specimenNumber = null);

    /// <summary>按日期范围查询</summary>
    List<TestRecord> GetByDateRange(string startDate, string endDate);

    /// <summary>按关键字模糊搜索</summary>
    List<TestRecord> Search(string keyword);

    /// <summary>获取下一个序号</summary>
    int GetNextSequence(string planNo, string specimenNumber, string testItem);

    /// <summary>获取所有不重复的计划编号</summary>
    List<string> GetDistinctPlanNos();
}
