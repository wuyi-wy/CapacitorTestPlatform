namespace CapacitorTestPlatform.Core.Models;

/// <summary>
/// 测试历史快照，用于记录检测数据的变更历史。
/// </summary>
public class TestHistory
{
    /// <summary>主键自增ID</summary>
    public int Id { get; set; }

    /// <summary>关联的检测记录ID</summary>
    public int TestRecordId { get; set; }

    /// <summary>快照数据 JSON</summary>
    public string SnapshotJson { get; set; } = string.Empty;

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
