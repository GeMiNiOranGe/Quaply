using Quaply.Ui.ViewModels.Base;

namespace Quaply.Ui.Interfaces;

public interface IViewModelFactory
{
    ViewModel Create(Type type);
}
