using Avalonia.Controls;
using QoreLib.ViewModels;

namespace QoreLib.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        DataContext = new MainViewModel();
        InitializeComponent();
    }
}