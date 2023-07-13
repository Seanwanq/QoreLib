using System.Windows.Input;

namespace QoreLib.ViewModels.ControlsViewModels;

public partial class SideBarButtonViewModel : ViewModelBase
{
    public string Content { get; }
    public ICommand Command { get; }
    public int Parameter { get; }

    public SideBarButtonViewModel(string content, ICommand command, int parameter)
    {
        Content = content;
        Command = command;
        Parameter = parameter;
    }
}