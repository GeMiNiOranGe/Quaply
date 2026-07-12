using Quaply.Ui.ViewModels.Base;

namespace Quaply.Ui.Interfaces;

public interface IHostNavigator : INavigator
{
    ViewModel? Current { get; }
}
