using System.Collections.ObjectModel;
using System.ComponentModel;

namespace CapacitorTestPlatform.UI.Models;

/// <summary>
/// 动态测试数据行 - 支持任意列的表格数据。
/// </summary>
public class TestDataRow : INotifyPropertyChanged
{
    /// <summary>行数据存储（列名 → 值）</summary>
    private readonly Dictionary<string, string> _data = new();

    /// <summary>全局列名有序列表（静态共享，按首次出现顺序排列）</summary>
    private static readonly List<string> _columnOrder = new();

    /// <summary>已注册的列名集合（用于去重判断）</summary>
    private static readonly HashSet<string> _knownColumns = new();

    /// <summary>属性变更通知事件</summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 按列名索引读写数据，设置值时自动注册新列并触发属性变更通知。
    /// </summary>
    /// <param name="key">列名</param>
    /// <returns>对应列的值，不存在时返回空字符串。</returns>
    public string this[string key]
    {
        get => _data.TryGetValue(key, out var v) ? v : "";
        set
        {
            _data[key] = value;
            if (_knownColumns.Add(key))
                _columnOrder.Add(key);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(key));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        }
    }

    /// <summary>行序号，变更时触发通知以刷新 UI 显示。</summary>
    private int _seqNo;
    public int SeqNo
    {
        get => _seqNo;
        set
        {
            _seqNo = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SeqNo)));
        }
    }

    /// <summary>只读数据字典，供外部遍历。</summary>
    public IReadOnlyDictionary<string, string> Data => _data;

    /// <summary>当前已知列名的有序列表。</summary>
    public IReadOnlyList<string> ColumnOrder => _columnOrder;

    /// <summary>
    /// 批量设置行数据，每一项会通过索引器写入以触发列注册和通知。
    /// </summary>
    /// <param name="values">列名与值的字典。</param>
    public void SetData(Dictionary<string, string> values)
    {
        foreach (var kv in values)
            this[kv.Key] = kv.Value;
    }

    /// <summary>
    /// 获取当前已知的所有列名（按首次出现顺序）。
    /// </summary>
    public static List<string> GetColumnOrder() => new(_columnOrder);

    /// <summary>
    /// 注册初始列顺序，用于预定义列的场景。
    /// </summary>
    /// <param name="columns">要注册的列名集合。</param>
    public static void RegisterColumns(IEnumerable<string> columns)
    {
        foreach (var col in columns)
        {
            if (_knownColumns.Add(col))
                _columnOrder.Add(col);
        }
    }

    /// <summary>
    /// 重置全局列信息，清空列顺序和已知列记录。
    /// </summary>
    public static void ResetColumns()
    {
        _columnOrder.Clear();
        _knownColumns.Clear();
    }
}

/// <summary>
/// 测试数据表 - 管理所有行数据，支持动态列。
/// </summary>
public class TestDataTable
{
    /// <summary>测试数据行集合，支持集合变更通知。</summary>
    public ObservableCollection<TestDataRow> Rows { get; } = new();

    /// <summary>当前表格的列名列表。</summary>
    public List<string> Columns { get; private set; } = new();

    /// <summary>
    /// 添加一行数据，自动分配序号并刷新列信息。
    /// </summary>
    /// <param name="data">列名与值的字典。</param>
    public void AddRow(Dictionary<string, string> data)
    {
        var row = new TestDataRow { SeqNo = Rows.Count + 1 };
        row.SetData(data);
        Rows.Add(row);

        // 刷新列信息
        Columns = TestDataRow.GetColumnOrder();
    }

    /// <summary>
    /// 删除指定行并重新编号所有剩余行。
    /// </summary>
    /// <param name="row">要删除的数据行。</param>
    public void RemoveRow(TestDataRow row)
    {
        Rows.Remove(row);
        // 重新编号
        for (int i = 0; i < Rows.Count; i++)
            Rows[i].SeqNo = i + 1;
    }

    /// <summary>
    /// 清空所有行数据并重置全局列信息。
    /// </summary>
    public void Clear()
    {
        Rows.Clear();
        TestDataRow.ResetColumns();
        Columns = new();
    }
}
