using CapacitorTestPlatform.Core.Models;
using CapacitorTestPlatform.Data.Contexts;
using Dapper;

namespace CapacitorTestPlatform.Data.Repositories;

/// <summary>
/// 远程计划仓储，从 SQL Server 的 apl_contract_plan 表拉取检测计划。
/// 显式列出需要的字段并通过 AS 别名映射到 PlanInfo 属性。
/// 远程表字段很多，只查询业务需要的列以减少数据传输。
/// </summary>
public class RemotePlanRepository
{
    private readonly SqlServerContext _context;

    public RemotePlanRepository(SqlServerContext context)
    {
        _context = context;
    }

    public bool IsAvailable => _context.IsReadAvailable;

    /// <summary>
    /// 从远程拉取当前工站的计划列表，仅返回测试中的单据。
    /// </summary>
    public async Task<IEnumerable<PlanInfo>> FetchPlansAsync(string? stationFilter = null)
    {
        var conn = _context.ReadConnection;
        if (conn == null)
            throw new InvalidOperationException("SQL Server 读取连接不可用");

        if (!string.IsNullOrEmpty(stationFilter))
        {
            return await conn.QueryAsync<PlanInfo>(@"
                SELECT
                    Id,
                    contractNumber AS ContractNumber,
                    sampleType AS SampleType,
                    testItems AS TestItems,
                    instrumentNumber AS InstrumentNumber,
                    statusName AS StatusName
                FROM apl_contract_plan
                WHERE instrumentNumber = @Station AND statusName = '测试中'",
                new { Station = stationFilter });
        }

        return await conn.QueryAsync<PlanInfo>(@"
            SELECT
                Id,
                contractNumber AS ContractNumber,
                sampleType AS SampleType,
                testItems AS TestItems,
                instrumentNumber AS InstrumentNumber,
                statusName AS StatusName
            FROM apl_contract_plan");
    }

    /// <summary>按合同号查询单条计划。</summary>
    public async Task<PlanInfo?> GetPlanByContractNumberAsync(string contractNumber)
    {
        var conn = _context.ReadConnection;
        if (conn == null)
            return null;

        return await conn.QueryFirstOrDefaultAsync<PlanInfo>(@"
            SELECT
                Id,
                contractNumber AS ContractNumber,
                sampleType AS SampleType,
                testItems AS TestItems,
                instrumentNumber AS InstrumentNumber,
                statusName AS StatusName
            FROM apl_contract_plan
            WHERE contractNumber = @ContractNumber",
            new { ContractNumber = contractNumber });
    }
}
