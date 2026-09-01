using System.Collections.ObjectModel;
using System.ComponentModel;

namespace CapacitorTestPlatform.UI.Models;

/// <summary>
/// 动态测试数据行 - 支持任意列的表格数据
/// </summary>
public class TestDataRow : INotifyPropertyChanged
{
    private readonly Dictionary<string, string> _data = new();
    private static readonly List<string> _columnOrder = new();
    private static readonly HashSet<string> _knownColumns = new();

    public event PropertyChangedEventHandler? PropertyChanged;

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

    public IReadOnlyDictionary<string, string> Data => _data;
    public IReadOnlyList<string> ColumnOrder => _columnOrder;

    public void SetData(Dictionary<string, string> values)
    {
        foreach (var kv in values)
            this[kv.Key] = kv.Value;
    }

    /// <summary>
    /// 获取当前已知的所有列名（按首次出现顺序）
    /// </summary>
    public static List<string> GetColumnOrder() => new(_columnOrder);

    /// <summary>
    /// 注册初始列顺序
    /// </summary>
    public static void RegisterColumns(IEnumerable<string> columns)
    {
        foreach (var col in columns)
        {
            if (_knownColumns.Add(col))
                _columnOrder.Add(col);
        }
    }

    public static void ResetColumns()
    {
        _columnOrder.Clear();
        _knownColumns.Clear();
    }
}

/// <summary>
/// 测试数据表 - 管理所有行数据
/// </summary>
public class TestDataTable
{
    public ObservableCollection<TestDataRow> Rows { get; } = new();
    public List<string> Columns { get; private set; } = new();

    public void AddRow(Dictionary<string, string> data)
    {
        var row = new TestDataRow { SeqNo = Rows.Count + 1 };
        row.SetData(data);
        Rows.Add(row);

        // 刷新列信息
        Columns = TestDataRow.GetColumnOrder();
    }

    public void RemoveRow(TestDataRow row)
    {
        Rows.Remove(row);
        // 重新编号
        for (int i = 0; i < Rows.Count; i++)
            Rows[i].SeqNo = i + 1;
    }

    public void Clear()
    {
        Rows.Clear();
        TestDataRow.ResetColumns();
        Columns = new();
    }
}
