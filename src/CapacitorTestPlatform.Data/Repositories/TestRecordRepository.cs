using System;
using System.Collections.Generic;
using System.Linq;
using CapacitorTestPlatform.Core.Models;
using CapacitorTestPlatform.Data.Contexts;
using Dapper;

namespace CapacitorTestPlatform.Data.Repositories;

public class TestRecordRepository
{
    private readonly SQLiteContext _context;

    public TestRecordRepository(SQLiteContext context)
    {
        _context = context;
    }

    public void InsertRecords(IEnumerable<TestRecord> records)
    {
        using var conn = _context.CreateConnection();
        const string sql = @"
            INSERT INTO TBL_CHECKDATA
                (PLAN_CODE, LOT, DEVICE_ID, DEVICE_TYPE, ITEM_NO, SPEC_NAME, CHECK_NAME, CHECK_VALUE, RESULT, TEST_DT, OPERATOR, REMARK)
            VALUES
                (@PlanNo, @Lot, @DeviceId, @DeviceType, @ItemNo, @SpecName, @CheckName, @CheckValue, @Result, @TestTime, @Operator, @Remark)";

        conn.Execute(sql, records);
    }

    public List<TestRecord> GetByLot(string lot)
    {
        using var conn = _context.CreateConnection();
        return conn.Query<TestRecord>(
            "SELECT * FROM TBL_CHECKDATA WHERE LOT = @Lot ORDER BY ID", new { Lot = lot }).ToList();
    }

    public List<TestRecord> GetUnSynced()
    {
        using var conn = _context.CreateConnection();
        return conn.Query<TestRecord>(
            "SELECT * FROM TBL_CHECKDATA WHERE SYNC_STATUS = 0 ORDER BY ID").ToList();
    }

    public List<TestRecord> GetAll()
    {
        using var conn = _context.CreateConnection();
        return conn.Query<TestRecord>("SELECT * FROM TBL_CHECKDATA ORDER BY ID DESC LIMIT 1000").ToList();
    }

    public int GetTotalCount()
    {
        using var conn = _context.CreateConnection();
        return conn.ExecuteScalar<int>("SELECT COUNT(*) FROM TBL_CHECKDATA");
    }

    public void MarkSynced(List<int> ids)
    {
        if (!ids.Any()) return;
        using var conn = _context.CreateConnection();
        conn.Execute("UPDATE TBL_CHECKDATA SET SYNC_STATUS = 1 WHERE ID IN @Ids", new { Ids = ids });
    }

    public void DeleteByLot(string lot)
    {
        using var conn = _context.CreateConnection();
        conn.Execute("DELETE FROM TBL_CHECKDATA WHERE LOT = @Lot", new { Lot = lot });
    }
}
