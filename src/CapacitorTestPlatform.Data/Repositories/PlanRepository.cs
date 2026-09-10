using System.Collections.Generic;
using System.Linq;
using CapacitorTestPlatform.Core.Models;
using CapacitorTestPlatform.Data.Contexts;
using Dapper;

namespace CapacitorTestPlatform.Data.Repositories;

/// <summary>
/// 本地计划缓存仓储，读写 PlanCache 表。
/// 远程计划通过 RemotePlanRepository 拉取后，经由此仓储保存到本地 SQLite。
/// </summary>
public class PlanRepository : CapacitorTestPlatform.Core.Interfaces.IPlanRepository
{
    private readonly SQLiteContext _context;

    public PlanRepository(SQLiteContext context)
    {
        _context = context;
    }

    /// <summary>获取所有本地缓存的计划，按创建时间倒序。</summary>
    public List<PlanInfo> GetAll()
    {
        using var conn = _context.CreateConnection();
        return conn.Query<PlanInfo>("SELECT * FROM PlanCache ORDER BY CreateTime DESC").ToList();
    }

    /// <summary>按批次号查询计划。</summary>
    public List<PlanInfo> GetByLot(string lot)
    {
        using var conn = _context.CreateConnection();
        return conn.Query<PlanInfo>("SELECT * FROM PlanCache WHERE Lot = @Lot", new { Lot = lot }).ToList();
    }

    /// <summary>批量插入计划到本地缓存。</summary>
    public void InsertBatch(IEnumerable<PlanInfo> plans)
    {
        using var conn = _context.CreateConnection();
        const string sql = @"INSERT INTO PlanCache
            (ContractNumber, SampleType, TestItems, InstrumentNumber, StatusName, Lot, DeviceId, ItemNo, SpecName, SpecValue, SpecMin, SpecMax, SpecUnit)
            VALUES (@ContractNumber, @SampleType, @TestItems, @InstrumentNumber, @StatusName, @Lot, @DeviceId, @ItemNo, @SpecName, @SpecValue, @SpecMin, @SpecMax, @SpecUnit)";
        conn.Execute(sql, plans);
    }

    /// <summary>按批次号删除计划缓存。</summary>
    public void DeleteByLot(string lot)
    {
        using var conn = _context.CreateConnection();
        conn.Execute("DELETE FROM PlanCache WHERE Lot = @Lot", new { Lot = lot });
    }

    /// <summary>获取所有不重复的批次号列表。</summary>
    public List<string> GetDistinctLots()
    {
        using var conn = _context.CreateConnection();
        return conn.Query<string>("SELECT DISTINCT Lot FROM PlanCache ORDER BY Lot").ToList();
    }

    /// <summary>按关键字模糊搜索计划（匹配申请单号、批次号、规格名称）。</summary>
    public List<PlanInfo> Search(string keyword)
    {
        using var conn = _context.CreateConnection();
        return conn.Query<PlanInfo>(
            "SELECT * FROM PlanCache WHERE ContractNumber LIKE @K OR Lot LIKE @K OR SpecName LIKE @K",
            new { K = $"%{keyword}%" }).ToList();
    }
}
