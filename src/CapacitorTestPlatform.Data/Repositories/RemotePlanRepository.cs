using CapacitorTestPlatform.Core.Models;
using CapacitorTestPlatform.Data.Contexts;
using Dapper;

namespace CapacitorTestPlatform.Data.Repositories;

public class RemotePlanRepository
{
    private readonly SqlServerContext _context;

    public RemotePlanRepository(SqlServerContext context)
    {
        _context = context;
    }

    public bool IsAvailable => _context.IsReadAvailable;

    public async Task<IEnumerable<PlanInfo>> FetchPlansAsync(string? stationFilter = null)
    {
        var conn = _context.ReadConnection;
        if (conn == null)
            throw new InvalidOperationException("SQL Server 读取连接不可用");

        var sql = @"SELECT
                Id,
                contractNumber AS PlanNo,
                productModel AS ProductModel,
                testItems AS TestItems,
                instrumentNumber AS Station,
                statusName AS Status
            FROM apl_contract_plan";

        if (!string.IsNullOrEmpty(stationFilter))
        {
            sql += " WHERE instrumentNumber = @Station";
            return await conn.QueryAsync<PlanInfo>(sql, new { Station = stationFilter });
        }

        return await conn.QueryAsync<PlanInfo>(sql);
    }

    public async Task<PlanInfo?> GetPlanByContractNumberAsync(string contractNumber)
    {
        var conn = _context.ReadConnection;
        if (conn == null)
            return null;

        return await conn.QueryFirstOrDefaultAsync<PlanInfo>(@"
            SELECT
                Id,
                contractNumber AS PlanNo,
                productModel AS ProductModel,
                testItems AS TestItems,
                instrumentNumber AS Station,
                statusName AS Status
            FROM apl_contract_plan
            WHERE contractNumber = @ContractNumber",
            new { ContractNumber = contractNumber });
    }
}
