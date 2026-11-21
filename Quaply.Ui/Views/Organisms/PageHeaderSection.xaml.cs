using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Quaply.Ui.Views.Organisms;

/// <summary>
/// Page header block composed of <see cref="PageHeader"/>, <see cref="PageNavigationBar"/>
/// and a slot for page-specific actions.
/// </summary>
///
/// <remarks>
/// <list type="bullet">
///   <item>
///   <b>Role:</b> composition only. <see cref="Title"/>, <see cref="Subtitle"/>,
///   <see cref="GoBackCommand"/> and <see cref="GoForwardCommand"/> are forwarded
///   unchanged to the inner molecules; the control contains no logic of its own.
///   </item>
///   <item>
///   <b>Actions slot:</b> business buttons (Trash, Add, ...) are not part of this
///   control. A page supplies them through <see cref="Actions"/>, which is rendered
///   on the right side of the navigation row. Pages that need none (e.g. recovery)
///   leave it unset.
///   </item>
///   <item>
///   <b>Single element:</b> <see cref="Actions"/> accepts one element. To supply
///   several buttons, wrap them in a container such as <c>StackPanel</c>.
///   </item>
///   <item>
///   <b>DataContext:</b> this control does not set its own <c>DataContext</c>, so
///   bindings declared inside <see cref="Actions"/> resolve against the page
///   ViewModel. Consumers should not set one either, or those bindings will break.
///   </item>
///   <item>
///   <b>Layout:</b> the control defines no outer margin. Spacing below the header
///   is the responsibility of the page.
///   </item>
/// </list>
///
/// <para><b>Example</b></para>
/// <code language="xml">
/// &lt;organisms:PageHeaderSection
///   Title="The title"
///   Subtitle="The subtitle."
///   GoBackCommand="{Binding GoBackCommand}"
///   GoForwardCommand="{Binding GoForwardCommand}"
/// &gt;
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
    /// Identifies the <see cref="Title"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(PageHeaderSection),
            new PropertyMetadata(string.Empty)
        );

    /// <summary>
    /// Identifies the <see cref="Subtitle"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SubtitleProperty =
        DependencyProperty.Register(
            nameof(Subtitle),
            typeof(string),
            typeof(PageHeaderSection),
            new PropertyMetadata(string.Empty)
        );

    /// <summary>
    /// Identifies the <see cref="GoBackCommand"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty GoBackCommandProperty =
        DependencyProperty.Register(
            nameof(GoBackCommand),
            typeof(ICommand),
            typeof(PageHeaderSection),
            new PropertyMetadata(null)
        );

    /// <summary>
    /// Identifies the <see cref="GoForwardCommand"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty GoForwardCommandProperty =
        DependencyProperty.Register(
            nameof(GoForwardCommand),
            typeof(ICommand),
            typeof(PageHeaderSection),
            new PropertyMetadata(null)
        );

    /// <summary>
    /// Identifies the <see cref="Actions"/> dependency property.
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

    // --- Title ---

    /// <summary>
    /// Gets or sets the page title. Forwarded to <see cref="PageHeader.Title"/>.
    /// </summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    // --- Subtitle ---

    /// <summary>
    /// Gets or sets the description shown below the title. Hidden when null or empty.
    /// Forwarded to <see cref="PageHeader.Subtitle"/>.
    /// </summary>
    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    // --- GoBackCommand ---

    /// <summary>
    /// Gets or sets the command for the Back button. Null disables the button.
    /// Forwarded to <see cref="PageNavigationBar.GoBackCommand"/>.
    /// </summary>
    public ICommand? GoBackCommand
    {
        get => (ICommand?)GetValue(GoBackCommandProperty);
        set => SetValue(GoBackCommandProperty, value);
    }

    // --- GoForwardCommand ---

    /// <summary>
    /// Gets or sets the command for the Forward button. Null disables the button.
    /// Forwarded to <see cref="PageNavigationBar.GoForwardCommand"/>.
    /// </summary>
    public ICommand? GoForwardCommand
    {
        get => (ICommand?)GetValue(GoForwardCommandProperty);
        set => SetValue(GoForwardCommandProperty, value);
    }

    // --- Actions ---

    /// <summary>
    /// Gets or sets page-specific content displayed on the right side of the
    /// navigation row, typically a <c>StackPanel</c> of buttons.
    /// When unset, the slot is empty.
    /// </summary>
    public object? Actions
    {
        get => GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }
}
