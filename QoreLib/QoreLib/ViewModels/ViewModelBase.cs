using CommunityToolkit.Mvvm.ComponentModel;
using ReactiveUI;

namespace QoreLib.ViewModels;

public partial class ViewModelBase : ObservableRecipient
{
    public ViewModelBase()
    {
        IsActive = true;
    }
}