using Avalonia.Controls;
using QoreLib.ViewModels;

namespace QoreLib.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        DataContext = new MainWindowViewModel();
        InitializeComponent();
    }
}