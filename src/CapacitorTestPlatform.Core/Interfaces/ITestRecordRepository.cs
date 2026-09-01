using CapacitorTestPlatform.Core.Models;

namespace CapacitorTestPlatform.Core.Interfaces;

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
