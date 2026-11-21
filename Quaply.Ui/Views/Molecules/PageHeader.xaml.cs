using System.Windows;
using System.Windows.Controls;

namespace Quaply.Ui.Views.Molecules;

/// <summary>
/// Displays a page title with an optional subtitle.
/// </summary>
///
/// <remarks>
/// <list type="bullet">
///   <item>
///   <b>Role:</b> presentation only. The control exposes no commands and has no
///   dependency on any ViewModel or business logic.
///   </item>
///   <item>
///   <b>Subtitle:</b> the subtitle line is collapsed when <see cref="Subtitle"/>
///   is null or empty.
///   </item>
///   <item>
///   <b>Binding:</b> internal bindings do not modify <c>DataContext</c>, so
///   consumers can bind <see cref="Title"/> and <see cref="Subtitle"/> directly
///   to their own ViewModel (e.g. <c>Title="{Binding ...}"</c>).
///   </item>
///   <item>
///   <b>Composition:</b> typically placed above <see cref="PageNavigationBar"/>.
///   <see cref="PageHeaderSection"/> combines both.
///   </item>
/// </list>
///
/// <para><b>Example</b></para>
/// <code language="xml">
/// &lt;molecules:PageHeader Title="The title" Subtitle="The subtitle." /&gt;
/// </code>
/// </remarks>
public partial class PageHeader : UserControl
{
    /// <summary>
    /// Identifies the <see cref="Title"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(PageHeader),
            new PropertyMetadata(string.Empty)
        );

    /// <summary>
    /// Identifies the <see cref="Subtitle"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SubtitleProperty =
        DependencyProperty.Register(
            nameof(Subtitle),
            typeof(string),
            typeof(PageHeader),
            new PropertyMetadata(string.Empty)
        );

    public PageHeader()
    {
        InitializeComponent();
    }

    // --- Title ---

    /// <summary>
    /// Gets or sets the main page title, rendered in a large SemiBold font.
    /// </summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    // --- Subtitle ---

    /// <summary>
    /// Gets or sets the short description shown below the title.
    /// The line is hidden when the value is null or empty.
    /// </summary>
    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }
}
