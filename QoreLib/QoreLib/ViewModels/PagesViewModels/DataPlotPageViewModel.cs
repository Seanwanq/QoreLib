using QoreLib.Services;

namespace QoreLib.ViewModels.PagesViewModels;

public partial class DataPlotPageViewModel : PageViewModelBase
{
    private readonly IDatabaseService? _databaseService;
    public DataPlotPageViewModel(IDatabaseService? databaseService)
    {
        IsActive = true;
        _databaseService = databaseService;
    }
}