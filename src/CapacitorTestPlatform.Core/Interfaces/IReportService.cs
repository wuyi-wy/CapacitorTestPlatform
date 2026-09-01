namespace CapacitorTestPlatform.Core.Interfaces;

public interface IReportService
{
    Task UploadTestRecordsAsync(string planNo, string specimenNumber);
}
