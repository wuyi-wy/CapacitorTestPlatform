using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CapacitorTestPlatform.UI.Models;

/// <summary>
/// 测试数据行，支持属性变更通知。
/// 列注册由所属 TestDataTable 实例管理，不再使用静态全局状态。
/// </summary>
public class TestDataRow : INotifyPropertyChanged
{
    private readonly Dictionary<string, string> _data = new();
    private int _seqNo;

    /// <summary>所属数据表实例，写入新列时自动注册到该表</summary>
    public TestDataTable? ParentTable { get; set; }

    /// <summary>行序号，支持属性变更通知（删除行后自动重编号）</summary>
    public int SeqNo
    {
        get => _seqNo;
        set { _seqNo = value; OnPropertyChanged(); }
    }

    /// <summary>所有数据的只读副本</summary>
    public IReadOnlyDictionary<string, string> Data => _data;

    /// <summary>索引器，按列名读写数据。写入未知列时自动注册到所属表格。</summary>
    public string this[string key]
    {
        get => _data.GetValueOrDefault(key, "");
        set
        {
            _data[key] = value;
            ParentTable?.RegisterColumn(key);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

/// <summary>
/// 测试数据动态表格容器，管理列顺序和数据行。
/// 每个实例独立维护自己的列注册状态，支持多表格并存。
/// </summary>
public class TestDataTable
{
    /// <summary>本表格的列名有序集合（实例级）</summary>
    private readonly List<string> _columnOrder = new();

    /// <summary>本表格已知列名集合（实例级，用于快速去重判断）</summary>
    private readonly HashSet<string> _knownColumns = new();

    /// <summary>表格标题（如 testItem 名称"电容量"）</summary>
    public string Title { get; set; } = "数据";

    /// <summary>子标题（LCR数字电桥专用：测量列名如"C(μF)"、"损耗"、"ESR(MΩ)"、"阻抗(MΩ)"，其他设备为空）</summary>
    public string SubTitle { get; set; } = "";

    /// <summary>设备分类（如"LCR数字电桥"、"漏电流测试仪"），用于外层 Tab 分组</summary>
    public string DeviceCategory { get; set; } = "";

    /// <summary>显示名称：有 SubTitle 时返回 SubTitle，否则返回 Title</summary>
    public string DisplayName => string.IsNullOrEmpty(SubTitle) ? Title : SubTitle;

    /// <summary>数据行集合</summary>
    public ObservableCollection<TestDataRow> Rows { get; } = new();

    /// <summary>获取本表格的列名有序列表（只读副本）</summary>
    public List<string> GetColumnOrder() => new(_columnOrder);

    /// <summary>
    /// 注册一个新列名到本表格。已存在的列名会被忽略。
    /// </summary>
    /// <param name="column">要注册的列名</param>
    public void RegisterColumn(string column)
    {
        if (_knownColumns.Add(column))
        {
            _columnOrder.Add(column);
        }
    }

    /// <summary>
    /// 批量预注册列名（保持传入顺序）。
    /// </summary>
    /// <param name="columns">要注册的列名集合</param>
    public void RegisterColumns(IEnumerable<string> columns)
    {
        foreach (var col in columns)
            RegisterColumn(col);
    }

    /// <summary>
    /// 向表格添加一行数据，自动注册新出现的列名。
    /// </summary>
    /// <param name="rowData">列名 → 值 字典</param>
    public void AddRow(Dictionary<string, string> rowData)
    {
        var row = new TestDataRow { SeqNo = Rows.Count + 1, ParentTable = this };
        foreach (var kv in rowData)
            row[kv.Key] = kv.Value;
        Rows.Add(row);
    }

    /// <summary>
    /// 从表格中移除一行并重新编号后续行。
    /// </summary>
    /// <param name="row">要移除的数据行</param>
    public void RemoveRow(TestDataRow row)
    {
        var index = Rows.IndexOf(row);
        if (index < 0) return;

        Rows.RemoveAt(index);

        // 重新编号后续行
        for (int i = index; i < Rows.Count; i++)
            Rows[i].SeqNo = i + 1;
    }

    /// <summary>
    /// 清空本表格所有行和列状态，不影响其他表格实例。
    /// </summary>
    public void Clear()
    {
        Rows.Clear();
        _columnOrder.Clear();
        _knownColumns.Clear();
    }

    /// <summary>
    /// 检查本表格是否包含指定列名。
    /// </summary>
    /// <param name="columns">要检查的列名集合</param>
    /// <returns>所有列名都已注册时返回 true</returns>
    public bool HasColumns(IEnumerable<string> columns)
    {
        return columns.All(c => _knownColumns.Contains(c));
    }
}
