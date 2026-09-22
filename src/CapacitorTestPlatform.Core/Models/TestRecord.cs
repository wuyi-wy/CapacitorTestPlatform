using System;

namespace CapacitorTestPlatform.Core.Models;

/// <summary>
/// 检测记录，对应本地 CheckData 表。
/// Dapper 通过属性名与列名自动映射（大小写不敏感）。
/// SeqNo、SpecimenNumber 等为 UI 层临时属性，不持久化到数据库。
/// </summary>
public class TestRecord
{
    /// <summary>主键自增ID</summary>
    public int Id { get; set; }

    /// <summary>计划编号 / 申请单号</summary>
    public string PlanNo { get; set; } = string.Empty;

    /// <summary>批次号</summary>
    public string Lot { get; set; } = string.Empty;

    /// <summary>设备编号</summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>设备类型（如 LCR数字电桥、漏电流测试仪）</summary>
    public string DeviceType { get; set; } = string.Empty;

    /// <summary>检测项序号</summary>
    public string ItemNo { get; set; } = string.Empty;

    /// <summary>规格名称</summary>
    public string SpecName { get; set; } = string.Empty;

    /// <summary>检测项名称（如 C(μF)、IL正向(μA)）</summary>
    public string CheckName { get; set; } = string.Empty;

    /// <summary>检测值</summary>
    public string CheckValue { get; set; } = string.Empty;

    /// <summary>检测结果（PASS/FAIL）</summary>
    public string Result { get; set; } = string.Empty;

    /// <summary>测试时间</summary>
    public string TestTime { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    /// <summary>操作员</summary>
    public string Operator { get; set; } = string.Empty;

    /// <summary>备注 / 试件编号</summary>
    public string Remark { get; set; } = string.Empty;

    /// <summary>同步状态：0=未同步，1=已同步到远程</summary>
    public int SyncStatus { get; set; }

    /// <summary>导出的 Excel 文件路径（历史记录可据此打开文件）</summary>
    public string ExcelPath { get; set; } = string.Empty;

    // ----- 以下为 UI 层临时属性，不写入 CheckData 表 -----

    /// <summary>试件编号（UI 层使用，通过 Remark 字段存储）</summary>
    public string? SpecimenNumber { get; set; }

    /// <summary>测试项名称（UI 层显示用）</summary>
    public string? TestItem { get; set; }

    /// <summary>规格下限（UI 层判定用）</summary>
    public string? MinSpec { get; set; }

    /// <summary>规格上限（UI 层判定用）</summary>
    public string? MaxSpec { get; set; }

    /// <summary>行序号（UI 层显示用，删除行后自动重编号）</summary>
    public int SeqNo { get; set; }
}
