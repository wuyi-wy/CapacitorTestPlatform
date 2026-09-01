namespace CapacitorTestPlatform.Core.Interfaces;

/// <summary>
/// 报告服务接口，负责将检测数据上传到远程 SQL Server。
/// </summary>
public interface IReportService
{
    /// <summary>上传指定计划和试件的检测记录到远程数据库</summary>
    Task UploadTestRecordsAsync(string planNo, string specimenNumber);
}
