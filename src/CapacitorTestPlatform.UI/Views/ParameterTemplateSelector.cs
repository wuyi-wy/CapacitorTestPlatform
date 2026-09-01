using System.Windows;
using System.Windows.Controls;
using CapacitorTestPlatform.UI.ViewModels;

namespace CapacitorTestPlatform.UI.Views;

/// <summary>
/// 参数模板选择器，根据参数的 InputType 属性选择下拉框或文本框模板。
/// </summary>
public class ParameterTemplateSelector : DataTemplateSelector
{
    /// <summary>下拉框输入模板</summary>
    public DataTemplate? ComboBoxTemplate { get; set; }

    /// <summary>文本框输入模板</summary>
    public DataTemplate? TextBoxTemplate { get; set; }

    /// <summary>
    /// 根据参数项的 InputType 返回对应的 DataTemplate。
    /// </summary>
    /// <param name="item">数据项（BindableParameter）。</param>
    /// <param name="container">承载元素。</param>
    /// <returns>匹配的 DataTemplate，不匹配时回退到基类行为。</returns>
    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        if (item is BindableParameter param)
        {
            return param.InputType == "ComboBox" ? ComboBoxTemplate : TextBoxTemplate;
        }
        return base.SelectTemplate(item, container);
    }
}
