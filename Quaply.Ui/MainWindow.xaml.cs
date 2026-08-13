using Wpf.Ui;
using Wpf.Ui.Controls;

namespace Quaply.Ui;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : FluentWindow
{
    public MainWindow(IContentDialogService contentDialogService)
    {
        InitializeComponent();
        contentDialogService.SetDialogHost(rootContentDialogPresenter);
    }
}
