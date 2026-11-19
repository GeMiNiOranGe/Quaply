using System.Windows;
using System.Windows.Controls;

namespace Quaply.Ui.Views.Molecules;

/// <summary>
/// Molecule that displays the title and subtitle of a page.
/// </summary>
///
/// <remarks>
/// <list type="bullet">
///   <item>
///   Pure presentation: no commands, no business knowledge,
///   and unaware of the ViewModel using it.
///   </item>
///   <item>
///   An empty or null <see cref="Subtitle"/> hides the subtitle line.
///   </item>
///   <item>
///   Internal bindings do not touch DataContext, so a page can still write
///   <c>Title="{Binding ...}"</c> against its own ViewModel.
///   </item>
///   <item>
///   Usually placed above <see cref="PageNavigationBar"/>;
///   <see cref="PageHeaderSection"/> combines both.
///   </item>
/// </list>
///
/// <para><b><i>Example:</i></b></para>
/// <code>
/// &lt;molecules:PageHeader
///     Title="Work Experiences"
///     Subtitle="Select the experiences that best match the job." /&gt;
/// </code>
/// </remarks>
public partial class PageHeader : UserControl
{
    /// <summary>
    /// Dependency property for <see cref="Title"/>.
    /// </summary>
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(PageHeader),
            new PropertyMetadata(string.Empty)
        );

    /// <summary>
    /// Dependency property for <see cref="Subtitle"/>.
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
    /// Main page title (large font, SemiBold).
    /// </summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    // --- Subtitle ---

    /// <summary>
    /// Short description below the title. Empty hides the line.
    /// </summary>
    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }
}
