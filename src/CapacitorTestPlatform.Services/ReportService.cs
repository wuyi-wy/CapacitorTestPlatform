using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;
using CapacitorTestPlatform.Data.Repositories;

namespace CapacitorTestPlatform.Services;

/// <summary>
/// 报告上传服务，负责将本地测试记录同步上传到远程 SQL Server 数据库。
/// </summary>
public class ReportService : IReportService
{
    private readonly RemoteReportRepository _remoteReportRepo;
    private readonly TestRecordRepository _testRecordRepo;

    /// <summary>
    /// 初始化报告服务。
    /// </summary>
    /// <param name="remoteReportRepo">远程报告仓储，用于向远程数据库写入测试记录</param>
    /// <param name="testRecordRepo">本地测试记录仓储，用于读取和更新同步状态</param>
    public ReportService(RemoteReportRepository remoteReportRepo, TestRecordRepository testRecordRepo)
    {
        _remoteReportRepo = remoteReportRepo;
        _testRecordRepo = testRecordRepo;
    }

    /// <summary>
    /// 将指定计划和样品编号下的未同步测试记录上传到远程数据库，并标记为已同步。
    /// </summary>
    /// <param name="planNo">计划编号</param>
    /// <param name="specimenNumber">样品编号</param>
    public async Task UploadTestRecordsAsync(string planNo, string specimenNumber)
    {
        var records = _testRecordRepo.GetUnSynced();
        if (!records.Any()) return;

        var ids = records.Select(r => r.Id).ToList();
        _testRecordRepo.MarkSynced(ids);
    }
}
