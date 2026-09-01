using CapacitorTestPlatform.Core.Models;

namespace CapacitorTestPlatform.Core.Interfaces;

/// <summary>
/// 检测数据仓储接口（异步版本，当前未被使用，实际仓储 TestRecordRepository 为同步实现）。
/// </summary>
public interface ITestRecordRepository
{
    Task<IEnumerable<TestRecord>> GetAllAsync();
    Task<TestRecord?> GetByIdAsync(int id);
    Task<IEnumerable<TestRecord>> GetByPlanNoAsync(string planNo);
    Task<IEnumerable<TestRecord>> GetByDateRangeAsync(DateTime start, DateTime end);
    Task<int> InsertAsync(TestRecord record);
    Task UpdateAsync(TestRecord record);
    Task DeleteAsync(int id);
    Task<IEnumerable<TestRecord>> GetUnuploadedAsync();
}
