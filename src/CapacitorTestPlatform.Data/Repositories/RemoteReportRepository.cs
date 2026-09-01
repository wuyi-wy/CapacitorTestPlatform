using CapacitorTestPlatform.Core.Models;
using CapacitorTestPlatform.Data.Contexts;
using Dapper;

namespace CapacitorTestPlatform.Data.Repositories;

public class RemoteReportRepository
{
    private readonly SqlServerContext _context;

    public RemoteReportRepository(SqlServerContext context)
    {
        _context = context;
    }

    public bool IsAvailable => _context.IsWriteAvailable;

    public async Task<int> InsertTestDataAsync(string contractNumber, string specimenNumber,
        string inspectionName, string testDataJson, string deviceModel)
    {
        var conn = _context.WriteConnection;
        if (conn == null)
            throw new InvalidOperationException("SQL Server 写入连接不可用");

        var sql = @"
            INSERT INTO TestData (ContractNumber, SpecimenNumber, InspectionName, TestData, DeviceModel, CreateTime)
            VALUES (@ContractNumber, @SpecimenNumber, @InspectionName, @TestData, @DeviceModel, GETDATE());
            SELECT SCOPE_IDENTITY();";

        return await conn.ExecuteScalarAsync<int>(sql, new
        {
            ContractNumber = contractNumber,
            SpecimenNumber = specimenNumber,
            InspectionName = inspectionName,
            TestData = testDataJson,
            DeviceModel = deviceModel
        });
    }

    public async Task<bool> CheckPlanStatusAsync(string contractNumber)
    {
        var conn = _context.ReadConnection;
        if (conn == null)
            return false;

        var status = await conn.QueryFirstOrDefaultAsync<string>(@"
            SELECT statusName FROM apl_contract_plan WHERE contractNumber = @ContractNumber",
            new { ContractNumber = contractNumber });

        return status == "测试中" || status == "录入中";
    }

    public async Task UpdatePlanStatusAsync(string contractNumber)
    {
        var conn = _context.WriteConnection;
        if (conn == null) return;

        await conn.ExecuteAsync(@"
            UPDATE apl_contract_plan SET statusName = '已完成' WHERE contractNumber = @ContractNumber",
            new { ContractNumber = contractNumber });
    }
}
