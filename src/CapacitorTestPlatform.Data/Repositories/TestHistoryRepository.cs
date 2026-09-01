using System.Collections.Generic;
using System.Linq;
using CapacitorTestPlatform.Core.Models;
using CapacitorTestPlatform.Data.Contexts;
using Dapper;

namespace CapacitorTestPlatform.Data.Repositories;

public class TestHistoryRepository : CapacitorTestPlatform.Core.Interfaces.ITestHistoryRepository
{
    private readonly SQLiteContext _context;

    public TestHistoryRepository(SQLiteContext context)
    {
        _context = context;
    }

    public List<TestRecord> GetByPlanNo(string planNo, string? specimenNumber = null)
    {
        using var conn = _context.CreateConnection();
        if (string.IsNullOrEmpty(specimenNumber))
            return conn.Query<TestRecord>("SELECT * FROM TBL_CHECKDATA WHERE PLAN_CODE = @PlanNo ORDER BY ID",
                new { PlanNo = planNo }).ToList();
        else
            return conn.Query<TestRecord>("SELECT * FROM TBL_CHECKDATA WHERE PLAN_CODE = @PlanNo AND REMARK = @Specimen ORDER BY ID",
                new { PlanNo = planNo, Specimen = specimenNumber }).ToList();
    }

    public List<TestRecord> GetByDateRange(string startDate, string endDate)
    {
        using var conn = _context.CreateConnection();
        return conn.Query<TestRecord>(
            "SELECT * FROM TBL_CHECKDATA WHERE TEST_DT BETWEEN @Start AND @End ORDER BY ID",
            new { Start = startDate, End = endDate }).ToList();
    }

    public List<TestRecord> Search(string keyword)
    {
        using var conn = _context.CreateConnection();
        return conn.Query<TestRecord>(
            "SELECT * FROM TBL_CHECKDATA WHERE PLAN_CODE LIKE @K OR LOT LIKE @K OR CHECK_VALUE LIKE @K",
            new { K = $"%{keyword}%" }).ToList();
    }

    public int GetNextSequence(string planNo, string specimenNumber, string testItem)
    {
        using var conn = _context.CreateConnection();
        var max = conn.ExecuteScalar<int?>(
            "SELECT MAX(CAST(SPEC_NAME AS INTEGER)) FROM TBL_CHECKDATA WHERE PLAN_CODE = @PlanNo AND REMARK = @Specimen",
            new { PlanNo = planNo, Specimen = specimenNumber });
        return (max ?? 0) + 1;
    }

    public List<string> GetDistinctPlanNos()
    {
        using var conn = _context.CreateConnection();
        return conn.Query<string>("SELECT DISTINCT PLAN_CODE FROM TBL_CHECKDATA ORDER BY PLAN_CODE").ToList();
    }
}
