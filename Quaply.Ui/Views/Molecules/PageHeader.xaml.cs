using System.Windows;
using System.Windows.Controls;

namespace Quaply.Ui.Views.Molecules;

/// <summary>
/// A presentational molecule that displays a title with an optional subtitle.
/// </summary>
///
/// <remarks>
/// <para>
/// The subtitle line is collapsed when <see cref="Subtitle"/> is
/// <see langword="null"/> or empty, independently of the title.
/// </para>
/// <para>
/// The visual structure lives in the default <see cref="ControlTemplate"/>
/// (Themes/Generic.xaml) and can be replaced by the consumer.
/// </para>
///
/// <para><b>Example</b></para>
/// <code>
/// &lt;molecules:PageHeader
///   Title="{Binding HeaderText}"
///   Subtitle="{Binding SummaryText}"
/// /&gt;
/// </code>
/// </remarks>
public class PageHeader : Control
{
    static PageHeader()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(PageHeader),
            new FrameworkPropertyMetadata(typeof(PageHeader))
        );
    }

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

    // --- Title ---

    /// <summary>
    /// Gets or sets the main title. The default is <see cref="string.Empty"/>.
    /// </summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    // --- Subtitle ---

    /// <summary>
    /// Gets or sets the secondary text shown below the title. The default is
    /// <see cref="string.Empty"/>, which hides the subtitle line.
    /// </summary>
    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }
}
