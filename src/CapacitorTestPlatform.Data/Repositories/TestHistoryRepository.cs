using System.Collections.Generic;
using System.Linq;
using CapacitorTestPlatform.Core.Models;
using CapacitorTestPlatform.Data.Contexts;
using Dapper;

namespace CapacitorTestPlatform.Data.Repositories;

/// <summary>
/// 测试历史仓储，提供 CheckData 表的多维度查询能力。
/// 支持按计划号、日期范围、关键字搜索，以及序号自增和去重查询。
/// </summary>
public class TestHistoryRepository : CapacitorTestPlatform.Core.Interfaces.ITestHistoryRepository
{
    private readonly SQLiteContext _context;

    public TestHistoryRepository(SQLiteContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 按计划号查询检测记录，可选按试件编号过滤。
    /// </summary>
    public List<TestRecord> GetByPlanNo(string planNo, string? specimenNumber = null)
    {
        using var conn = _context.CreateConnection();
        if (string.IsNullOrEmpty(planNo))
        {
            if (string.IsNullOrEmpty(specimenNumber))
                return conn.Query<TestRecord>("SELECT * FROM CheckData ORDER BY Id DESC LIMIT 1000").ToList();
            else
                return conn.Query<TestRecord>("SELECT * FROM CheckData WHERE Remark = @Specimen ORDER BY Id",
                    new { Specimen = specimenNumber }).ToList();
        }
        else
        {
            if (string.IsNullOrEmpty(specimenNumber))
                return conn.Query<TestRecord>("SELECT * FROM CheckData WHERE PlanNo = @PlanNo ORDER BY Id",
                    new { PlanNo = planNo }).ToList();
            else
                return conn.Query<TestRecord>("SELECT * FROM CheckData WHERE PlanNo = @PlanNo AND Remark = @Specimen ORDER BY Id",
                    new { PlanNo = planNo, Specimen = specimenNumber }).ToList();
        }
    }

    /// <summary>按日期范围查询检测记录。</summary>
    public List<TestRecord> GetByDateRange(string startDate, string endDate)
    {
        using var conn = _context.CreateConnection();
        return conn.Query<TestRecord>(
            "SELECT * FROM CheckData WHERE TestTime BETWEEN @Start AND @End ORDER BY Id",
            new { Start = startDate, End = endDate }).ToList();
    }

    /// <summary>按关键字模糊搜索（匹配计划号、批次号、检测值）。</summary>
    public List<TestRecord> Search(string keyword)
    {
        using var conn = _context.CreateConnection();
        return conn.Query<TestRecord>(
            "SELECT * FROM CheckData WHERE PlanNo LIKE @K OR Lot LIKE @K OR CheckValue LIKE @K",
            new { K = $"%{keyword}%" }).ToList();
    }

    /// <summary>
    /// 获取下一个序号：查询指定计划+试件下 SpecName 的最大整数值，加1返回。
    /// </summary>
    public int GetNextSequence(string planNo, string specimenNumber, string testItem)
    {
        using var conn = _context.CreateConnection();
        var max = conn.ExecuteScalar<int?>(
            "SELECT MAX(CAST(SpecName AS INTEGER)) FROM CheckData WHERE PlanNo = @PlanNo AND Remark = @Specimen",
            new { PlanNo = planNo, Specimen = specimenNumber });
        return (max ?? 0) + 1;
    }

    /// <summary>获取所有不重复的计划编号列表。</summary>
    public List<string> GetDistinctPlanNos()
    {
        using var conn = _context.CreateConnection();
        return conn.Query<string>("SELECT DISTINCT PlanNo FROM CheckData ORDER BY PlanNo").ToList();
    }
}
