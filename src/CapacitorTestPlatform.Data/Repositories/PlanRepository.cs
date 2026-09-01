using System.Collections.Generic;
using System.Linq;
using CapacitorTestPlatform.Core.Models;
using CapacitorTestPlatform.Data.Contexts;
using Dapper;

namespace CapacitorTestPlatform.Data.Repositories;

public class PlanRepository : CapacitorTestPlatform.Core.Interfaces.IPlanRepository
{
    private readonly SQLiteContext _context;

    public PlanRepository(SQLiteContext context)
    {
        _context = context;
    }

    public List<PlanInfo> GetAll()
    {
        using var conn = _context.CreateConnection();
        return conn.Query<PlanInfo>("SELECT * FROM TBL_PLANCACHE ORDER BY CREATE_DT DESC").ToList();
    }

    public List<PlanInfo> GetByLot(string lot)
    {
        using var conn = _context.CreateConnection();
        return conn.Query<PlanInfo>("SELECT * FROM TBL_PLANCACHE WHERE LOT = @Lot", new { Lot = lot }).ToList();
    }

    public void InsertBatch(IEnumerable<PlanInfo> plans)
    {
        using var conn = _context.CreateConnection();
        const string sql = @"INSERT INTO TBL_PLANCACHE
            (PLAN_CODE, LOT, STATION, DEVICE_ID, ITEM_NO, SPEC_NAME, SPEC_VALUE, SPEC_MIN, SPEC_MAX, SPEC_UNIT)
            VALUES (@PlanNo, @LotNo, @Station, @DeviceId, @ItemNo, @SpecName, @SpecValue, @SpecMin, @SpecMax, @SpecUnit)";
        conn.Execute(sql, plans);
    }

    public void DeleteByLot(string lot)
    {
        using var conn = _context.CreateConnection();
        conn.Execute("DELETE FROM TBL_PLANCACHE WHERE LOT = @Lot", new { Lot = lot });
    }

    public List<string> GetDistinctLots()
    {
        using var conn = _context.CreateConnection();
        return conn.Query<string>("SELECT DISTINCT LOT FROM TBL_PLANCACHE ORDER BY LOT").ToList();
    }

    public List<PlanInfo> Search(string keyword)
    {
        using var conn = _context.CreateConnection();
        return conn.Query<PlanInfo>(
            "SELECT * FROM TBL_PLANCACHE WHERE PLAN_CODE LIKE @K OR LOT LIKE @K OR SPEC_NAME LIKE @K",
            new { K = $"%{keyword}%" }).ToList();
    }
}
