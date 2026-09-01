using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;
using CapacitorTestPlatform.Data.Repositories;

namespace CapacitorTestPlatform.Services;

public class ReportService : IReportService
{
    private readonly RemoteReportRepository _remoteReportRepo;
    private readonly TestRecordRepository _testRecordRepo;

    public ReportService(RemoteReportRepository remoteReportRepo, TestRecordRepository testRecordRepo)
    {
        _remoteReportRepo = remoteReportRepo;
        _testRecordRepo = testRecordRepo;
    }

    public async Task UploadTestRecordsAsync(string planNo, string specimenNumber)
    {
        var records = _testRecordRepo.GetUnSynced();
        if (!records.Any()) return;

        var ids = records.Select(r => r.Id).ToList();
        _testRecordRepo.MarkSynced(ids);
    }
}
