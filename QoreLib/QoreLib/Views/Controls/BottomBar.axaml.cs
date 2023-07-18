using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using QoreLib.Services;
using QoreLib.ViewModels.ControlsViewModels;

namespace QoreLib.Views.Controls;

public partial class BottomBar : UserControl
{
    public BottomBar()
    {
        InitializeComponent();
        Task.Run(async () =>
        {
            while (ServiceLocator.Instance.DatabaseService == null)
            {
                await Task.Delay(50);
            }

            var databaseService = ServiceLocator.Instance.DatabaseService;
            Dispatcher.UIThread.InvokeAsync(() => { DataContext = new BottomBarViewModel(databaseService); });
        });
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}