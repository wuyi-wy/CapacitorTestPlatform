using System;

namespace CapacitorTestPlatform.Core.Models;

public class TestRecord
{
    public int Id { get; set; }
    public string PlanNo { get; set; } = string.Empty;
    public string Lot { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string ItemNo { get; set; } = string.Empty;
    public string SpecName { get; set; } = string.Empty;
    public string CheckName { get; set; } = string.Empty;
    public string CheckValue { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string TestTime { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    public string Operator { get; set; } = string.Empty;
    public string Remark { get; set; } = string.Empty;
    public int SyncStatus { get; set; }

    public string? SpecimenNumber { get; set; }
    public string? TestItem { get; set; }
    public string? MinSpec { get; set; }
    public string? MaxSpec { get; set; }
    public int SeqNo { get; set; }
}
