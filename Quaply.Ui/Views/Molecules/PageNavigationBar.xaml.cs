using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Quaply.Ui.Views.Molecules;

/// <summary>
/// Page navigation molecule: Back, Forward and a Breadcrumb.
/// </summary>
///
/// <remarks>
/// <list type="bullet">
///   <item>
///   The control knows NO business logic and NOTHING about Navigation. It only
///   receives <see cref="ICommand"/>s from outside via DependencyProperty.
///   </item>
///   <item>
///   Trash / Add / Export buttons do NOT belong here; the page places them next
///   to this control.
///   </item>
///   <item>
///   A <c>null</c> command disables the matching button.
///   </item>
///   <item>
///   Commands should live in <c>PageViewModelBase</c> (injected with navigation
///   service); child ViewModels can override them for custom behavior
///   (e.g. confirm on unsaved changes).
///   </item>
/// </list>
/// </remarks>
public partial class PageNavigationBar : UserControl
{
    /// <summary>
    /// Dependency property for <see cref="GoBackCommand"/>.
    /// </summary>
    public static readonly DependencyProperty GoBackCommandProperty =
        DependencyProperty.Register(
            nameof(GoBackCommand),
            typeof(ICommand),
            typeof(PageNavigationBar),
            new PropertyMetadata(null)
        );

    /// <summary>
    /// Dependency property for <see cref="GoForwardCommand"/>.
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

    /// <summary>
    /// Command for the Back button. Null disables the button.
    /// </summary>
    public ICommand? GoBackCommand
    {
        get => (ICommand?)GetValue(GoBackCommandProperty);
        set => SetValue(GoBackCommandProperty, value);
    }

    /// <summary>
    /// Command for the Forward button. Null disables the button.
    /// </summary>
    public ICommand? GoForwardCommand
    {
        get => (ICommand?)GetValue(GoForwardCommandProperty);
        set => SetValue(GoForwardCommandProperty, value);
    }
}
