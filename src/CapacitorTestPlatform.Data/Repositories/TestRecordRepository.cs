using System;
using System.Collections.Generic;
using System.Linq;
using CapacitorTestPlatform.Core.Models;
using CapacitorTestPlatform.Data.Contexts;
using Dapper;

namespace CapacitorTestPlatform.Data.Repositories;

/// <summary>
/// 检测数据仓储，读写 CheckData 表。
/// 负责设备采集数据的持久化、查询、同步状态管理和批量删除。
/// </summary>
public class TestRecordRepository
{
    private readonly SQLiteContext _context;

    public TestRecordRepository(SQLiteContext context)
    {
        _context = context;
    }

    /// <summary>批量插入检测记录。</summary>
    public void InsertRecords(IEnumerable<TestRecord> records)
    {
        using var conn = _context.CreateConnection();
        const string sql = @"
            INSERT INTO CheckData
                (PlanNo, Lot, DeviceId, DeviceType, ItemNo, SpecName, CheckName, CheckValue, Result, TestTime, Operator, Remark, ExcelPath)
            VALUES
                (@PlanNo, @Lot, @DeviceId, @DeviceType, @ItemNo, @SpecName, @CheckName, @CheckValue, @Result, @TestTime, @Operator, @Remark, @ExcelPath)";

        conn.Execute(sql, records);
    }

    /// <summary>按批次号查询检测记录。</summary>
    public List<TestRecord> GetByLot(string lot)
    {
        using var conn = _context.CreateConnection();
        return conn.Query<TestRecord>(
            "SELECT * FROM CheckData WHERE Lot = @Lot ORDER BY Id", new { Lot = lot }).ToList();
    }

    /// <summary>获取所有未同步到远程的检测记录。</summary>
    public List<TestRecord> GetUnSynced()
    {
        using var conn = _context.CreateConnection();
        return conn.Query<TestRecord>(
            "SELECT * FROM CheckData WHERE SyncStatus = 0 ORDER BY Id").ToList();
    }

    /// <summary>获取最近 1000 条检测记录，用于历史页面展示。</summary>
    public List<TestRecord> GetAll()
    {
        using var conn = _context.CreateConnection();
        return conn.Query<TestRecord>("SELECT * FROM CheckData ORDER BY Id DESC LIMIT 1000").ToList();
    }

    /// <summary>获取检测记录总数。</summary>
    public int GetTotalCount()
    {
        using var conn = _context.CreateConnection();
        return conn.ExecuteScalar<int>("SELECT COUNT(*) FROM CheckData");
    }

    /// <summary>将指定记录标记为已同步。</summary>
    public void MarkSynced(List<int> ids)
    {
        if (!ids.Any()) return;
        using var conn = _context.CreateConnection();
        conn.Execute("UPDATE CheckData SET SyncStatus = 1 WHERE Id IN @Ids", new { Ids = ids });
    }

    /// <summary>按批次号删除检测记录。</summary>
    public void DeleteByLot(string lot)
    {
        using var conn = _context.CreateConnection();
        conn.Execute("DELETE FROM CheckData WHERE Lot = @Lot", new { Lot = lot });
    }
}
