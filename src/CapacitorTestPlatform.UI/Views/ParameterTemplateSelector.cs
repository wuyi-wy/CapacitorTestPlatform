using System.Windows;
using System.Windows.Controls;
using CapacitorTestPlatform.UI.ViewModels;

namespace CapacitorTestPlatform.UI.Views;

public class ParameterTemplateSelector : DataTemplateSelector
{
    public DataTemplate? ComboBoxTemplate { get; set; }
    public DataTemplate? TextBoxTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        if (item is BindableParameter param)
        {
            return param.InputType == "ComboBox" ? ComboBoxTemplate : TextBoxTemplate;
        }
        return base.SelectTemplate(item, container);
    }
}
