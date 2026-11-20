using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Quaply.Ui.Views.Organisms;

/// <summary>
/// Organism that composes <see cref="PageHeader"/> and <see cref="PageNavigationBar"/>
/// and exposes an <see cref="Actions"/> slot for page-specific buttons.
/// </summary>
///
/// <remarks>
/// <list type="bullet">
///   <item>
///   Only forwards properties to the molecules; it contains no logic of its own.
///   </item>
///   <item>
///   Business buttons (Trash, Add, ...) are NOT defined here. The page passes them
///   via <see cref="Actions"/>; pages without them (e.g. recovery) leave it unset.
///   </item>
///   <item>
///   <see cref="Actions"/> holds a single element: wrap several buttons in a StackPanel.
///   </item>
///   <item>
///   Do not set DataContext on this control, or bindings inside <see cref="Actions"/>
///   will stop resolving against the page ViewModel.
///   </item>
///   <item>
///   No outer margin: spacing below the header is decided by the page.
///   </item>
/// </list>
///
/// <para><b><i>Example:</i></b></para>
/// <code language="xml">
/// &lt;organisms:PageHeaderSection
///     Title="The title"
///     Subtitle="The subtitle."
///     GoBackCommand="{Binding GoBackCommand}"
///     GoForwardCommand="{Binding GoForwardCommand}"&gt;
///   &lt;organisms:PageHeaderSection.Actions&gt;
///     &lt;StackPanel Orientation="Horizontal"&gt;
///       &lt;ui:Button Command="{Binding AddCommand}" Content="Add item" /&gt;
///       &lt;ui:Button Command="{Binding ShowMoreCommand}" Content="More" /&gt;
///     &lt;/StackPanel&gt;
///   &lt;/organisms:PageHeaderSection.Actions&gt;
/// &lt;/organisms:PageHeaderSection&gt;
/// </code>
/// </remarks>
public partial class PageHeaderSection : UserControl
{
    /// <summary>
    /// Dependency property for <see cref="Title"/>.
    /// </summary>
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(PageHeaderSection),
            new PropertyMetadata(string.Empty)
        );

    /// <summary>
    /// Dependency property for <see cref="Subtitle"/>.
    /// </summary>
    public static readonly DependencyProperty SubtitleProperty =
        DependencyProperty.Register(
            nameof(Subtitle),
            typeof(string),
            typeof(PageHeaderSection),
            new PropertyMetadata(string.Empty)
        );

    /// <summary>
    /// Dependency property for <see cref="GoBackCommand"/>.
    /// </summary>
    public static readonly DependencyProperty GoBackCommandProperty =
        DependencyProperty.Register(
            nameof(GoBackCommand),
            typeof(ICommand),
            typeof(PageHeaderSection),
            new PropertyMetadata(null)
        );

    /// <summary>
    /// Dependency property for <see cref="GoForwardCommand"/>.
    /// </summary>
    public static readonly DependencyProperty GoForwardCommandProperty =
        DependencyProperty.Register(
            nameof(GoForwardCommand),
            typeof(ICommand),
            typeof(PageHeaderSection),
            new PropertyMetadata(null)
        );

    /// <summary>
    /// Dependency property for <see cref="Actions"/>.
    /// </summary>
    public static readonly DependencyProperty ActionsProperty =
        DependencyProperty.Register(
            nameof(Actions),
            typeof(object),
            typeof(PageHeaderSection),
            new PropertyMetadata(null)
        );

    public PageHeaderSection()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Main page title. Forwarded to <see cref="PageHeader"/>.
    /// </summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Short description under the title. Empty hides it.
    /// Forwarded to <see cref="PageHeader"/>.
    /// </summary>
    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    /// <summary>
    /// Command for the Back button. Null disables it.
    /// Forwarded to <see cref="PageNavigationBar"/>.
    /// </summary>
    public ICommand? GoBackCommand
    {
        get => (ICommand?)GetValue(GoBackCommandProperty);
        set => SetValue(GoBackCommandProperty, value);
    }

    /// <summary>
    /// Command for the Forward button. Null disables it.
    /// Forwarded to <see cref="PageNavigationBar"/>.
    /// </summary>
    public ICommand? GoForwardCommand
    {
        get => (ICommand?)GetValue(GoForwardCommandProperty);
        set => SetValue(GoForwardCommandProperty, value);
    }

    /// <summary>
    /// Page-specific content shown on the right side of the navigation row
    /// (typically a StackPanel of buttons). Unset means the slot stays empty.
    /// </summary>
    public object? Actions
    {
        get => GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }
}
