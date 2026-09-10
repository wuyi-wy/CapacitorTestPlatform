using CapacitorTestPlatform.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace CapacitorTestPlatform.UI.Views;

/// <summary>
/// 根据设备分类下的表格数量选择不同的内容模板。
/// 多表格（LCR数字电桥）→ 内层 TabControl；单表格（漏电流/绝缘电阻）→ 直接 DataGrid。
/// </summary>
public class DeviceCategoryTemplateSelector : DataTemplateSelector
{
    /// <summary>多表格模板（含内层 TabControl），资源键</summary>
    public DataTemplate? MultiTableTemplate { get; set; }

    /// <summary>单表格模板（直接 DataGrid），资源键</summary>
    public DataTemplate? SingleTableTemplate { get; set; }

    /// <summary>
    /// 根据表格数量选择模板：>1 个表格用 MultiTableTemplate，否则用 SingleTableTemplate。
    /// </summary>
    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        if (item is DeviceCategoryTab tab)
        {
            return tab.Tables.Count > 1 ? MultiTableTemplate : SingleTableTemplate;
        }
        return base.SelectTemplate(item, container);
    }
}
