using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Quaply.Ui.Views.Molecules;

/// <summary>
/// A presentational molecule that displays back and forward buttons next to a
/// breadcrumb.
/// </summary>
///
/// <remarks>
/// <para>
/// The control contains no navigation logic. What each button does, and
/// whether it can run, is decided by the supplied commands'
/// <c>CanExecute</c>.
/// </para>
/// <para>
/// Each button is disabled while its command is <see langword="null"/>.
/// </para>
/// <para>
/// The visual structure lives in the default <see cref="ControlTemplate"/>
/// (Themes/Generic.xaml) and can be replaced by the consumer.
/// </para>
///
/// <para><b>Example</b></para>
/// <code>
/// &lt;molecules:PageNavigationBar
///   GoBackCommand="{Binding GoToPreviousCommand}"
///   GoBackToolTip="{Binding PreviousText}"
///   GoForwardCommand="{Binding GoToNextCommand}"
///   GoForwardToolTip="{Binding NextText}"
/// /&gt;
/// </code>
/// </remarks>
public class PageNavigationBar : Control
{
    static PageNavigationBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(PageNavigationBar),
            new FrameworkPropertyMetadata(typeof(PageNavigationBar))
        );
    }

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
    /// Identifies the <see cref="GoBackToolTip"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty GoBackToolTipProperty =
        DependencyProperty.Register(
            nameof(GoBackToolTip),
            typeof(object),
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

    /// <summary>
    /// Identifies the <see cref="GoForwardToolTip"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty GoForwardToolTipProperty =
        DependencyProperty.Register(
            nameof(GoForwardToolTip),
            typeof(object),
            typeof(PageNavigationBar),
            new PropertyMetadata(null)
        );

    // --- GoBackCommand ---

    /// <summary>
    /// Gets or sets the command executed by the back button. The default is
    /// <see langword="null"/>, which disables the button.
    /// </summary>
    public ICommand? GoBackCommand
    {
        get => (ICommand?)GetValue(GoBackCommandProperty);
        set => SetValue(GoBackCommandProperty, value);
    }

    // --- GoBackToolTip ---

    /// <summary>
    /// Gets or sets the tooltip of the back button. The default is
    /// <see langword="null"/>, which shows no tooltip.
    /// </summary>
    public object? GoBackToolTip
    {
        get => GetValue(GoBackToolTipProperty);
        set => SetValue(GoBackToolTipProperty, value);
    }

    // --- GoForwardCommand ---

    /// <summary>
    /// Gets or sets the command executed by the forward button. The default is
    /// <see langword="null"/>, which disables the button.
    /// </summary>
    public ICommand? GoForwardCommand
    {
        get => (ICommand?)GetValue(GoForwardCommandProperty);
        set => SetValue(GoForwardCommandProperty, value);
    }

    // --- GoForwardToolTip ---

    /// <summary>
    /// Gets or sets the tooltip of the forward button. The default is
    /// <see langword="null"/>, which shows no tooltip.
    /// </summary>
    public object? GoForwardToolTip
    {
        get => GetValue(GoForwardToolTipProperty);
        set => SetValue(GoForwardToolTipProperty, value);
    }
}
