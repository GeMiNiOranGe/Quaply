using System.Windows;
using System.Windows.Controls;
using Quaply.Ui.ViewModels;

namespace Quaply.Ui.Views.Pages;

/// <summary>
/// Interaction logic for ProfilePage.xaml
/// </summary>
public partial class ProfilePage : UserControl
{
    public ProfilePage()
    {
        InitializeComponent();
        Loaded += ProfilePage_Loaded;
    }

    private async void ProfilePage_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ProfileViewModel viewModel)
        {
            await viewModel.LoadProfilesCommand.ExecuteAsync(null);
        }
    }
}
