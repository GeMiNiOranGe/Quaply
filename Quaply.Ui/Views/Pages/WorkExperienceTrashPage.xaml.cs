using System.Windows;
using System.Windows.Controls;

namespace Quaply.Ui.Views.Pages;

/// <summary>
/// Interaction logic for WorkExperienceTrashPage.xaml
/// </summary>
public partial class WorkExperienceTrashPage : UserControl
{
    public WorkExperienceTrashPage()
    {
        InitializeComponent();
    }

    private void SelectAllCheckBox_Click(object sender, RoutedEventArgs e)
    {
        CheckBox checkBox = (CheckBox)sender;

        // WPF's default 3-state cycle allows the user to click their way
        // into the indeterminate state. That state should only ever be
        // set programmatically (by the ViewModel, based on partial
        // selection), so if a user click just produced null, force it
        // back to false instead of letting it sit as indeterminate.
        checkBox.IsChecked ??= false;
    }
}
