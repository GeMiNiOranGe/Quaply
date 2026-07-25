using Quaply.Ui.Interfaces;
using Quaply.Ui.ViewModels.Base;

namespace Quaply.Ui.ViewModels;

public partial class ProfileEditorViewModel(INavigator navigator)
    : NavigableViewModel(navigator) { }
