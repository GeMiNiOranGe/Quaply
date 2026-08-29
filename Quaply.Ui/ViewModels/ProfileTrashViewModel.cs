using Quaply.Service.Interfaces;
using Quaply.Ui.Interfaces;
using Quaply.Ui.ViewModels.Base;

namespace Quaply.Ui.ViewModels;

public partial class ProfileTrashViewModel(
    INavigator navigator,
    IProfileService service
) : NavigableViewModel(navigator)
{
    private readonly IProfileService _service = service;
}
