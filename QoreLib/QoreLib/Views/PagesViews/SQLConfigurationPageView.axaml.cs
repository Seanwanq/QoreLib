using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace QoreLib.Views.PagesViews;

public partial class SQLConfigurationPageView : UserControl
{
    public SQLConfigurationPageView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}