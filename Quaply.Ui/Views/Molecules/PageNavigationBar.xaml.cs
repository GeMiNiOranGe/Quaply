using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Quaply.Ui.Views.Molecules;

/// <summary>
/// Provides Back and Forward buttons and a breadcrumb for page-level navigation.
/// </summary>
///
/// <remarks>
/// <list type="bullet">
///   <item>
///   <b>Role:</b> presentation only. The control has no knowledge of the navigation
///   service or business logic; behavior is supplied entirely through
///   <see cref="GoBackCommand"/> and <see cref="GoForwardCommand"/>.
///   </item>
///   <item>
///   <b>Disabled state:</b> a <c>null</c> command disables the corresponding button.
///   </item>
///   <item>
///   <b>Scope:</b> page actions such as Trash, Add or Export are intentionally
///   excluded. Pages place them beside this control.
///   </item>
///   <item>
///   <b>Command source:</b> the default commands are expected to be defined in
///   navigation ViewModel, which receives the navigation service through
///   dependency injection. Derived ViewModels may override them for custom
///   behavior, such as confirming when there are unsaved changes.
///   </item>
/// </list>
///
/// <para><b>Example</b></para>
/// <code language="xml">
/// &lt;molecules:PageNavigationBar
///   GoBackCommand="{Binding GoBackCommand}"
///   GoForwardCommand="{Binding GoForwardCommand}"
/// /&gt;
/// </code>
/// </remarks>
public partial class PageNavigationBar : UserControl
{
    /// <summary>
    /// Identifies the <see cref="GoBackCommand"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty GoBackCommandProperty =
        DependencyProperty.Register(
            nameof(GoBackCommand),
            typeof(ICommand),
            typeof(PageNavigationBar),
            new PropertyMetadata(null)
        );

    /// <summary>
    /// Identifies the <see cref="GoForwardCommand"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty GoForwardCommandProperty =
        DependencyProperty.Register(
            nameof(GoForwardCommand),
            typeof(ICommand),
            typeof(PageNavigationBar),
            new PropertyMetadata(null)
        );

    public PageNavigationBar()
    {
        InitializeComponent();
    }

    // --- GoBackCommand ---

    /// <summary>
    /// Gets or sets the command executed by the Back button.
    /// A null value disables the button.
    /// </summary>
    public ICommand? GoBackCommand
    {
        get => (ICommand?)GetValue(GoBackCommandProperty);
        set => SetValue(GoBackCommandProperty, value);
    }

    // --- GoForwardCommand ---

    /// <summary>
    /// Gets or sets the command executed by the Forward button.
    /// A null value disables the button.
    /// </summary>
    public ICommand? GoForwardCommand
    {
        get => (ICommand?)GetValue(GoForwardCommandProperty);
        set => SetValue(GoForwardCommandProperty, value);
    }
}
