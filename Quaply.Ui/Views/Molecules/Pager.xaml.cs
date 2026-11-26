using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Quaply.Ui.Views.Molecules;

/// <summary>
/// A presentational molecule that displays a text between previous and next
/// buttons.
/// </summary>
///
/// <remarks>
/// <para>
/// The control does not track the current position or the number of pages.
/// Whether each button is enabled is decided by the <c>CanExecute</c> logic of
/// <see cref="PreviousCommand"/> and <see cref="NextCommand"/>.
/// </para>
/// <para>
/// The visual structure lives in the default <see cref="ControlTemplate"/>
/// (Themes/Generic.xaml) and can be replaced by the consumer.
/// </para>
///
/// <para><b>Example</b></para>
/// <code>
/// &lt;molecules:Pager
///   Text="{Binding PageText}"
///   PreviousCommand="{Binding GoToPreviousCommand}"
///   PreviousToolTip="{Binding PreviousText}"
///   NextCommand="{Binding GoToNextCommand}"
///   NextToolTip="{Binding NextText}"
/// /&gt;
/// </code>
/// </remarks>
public class Pager : Control
{
    static Pager()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Pager),
            new FrameworkPropertyMetadata(typeof(Pager))
        );
    }

    /// <summary>
    /// Identifies the <see cref="Text"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(Pager),
            new PropertyMetadata(string.Empty)
        );

    /// <summary>
    /// Identifies the <see cref="PreviousCommand"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty PreviousCommandProperty =
        DependencyProperty.Register(
            nameof(PreviousCommand),
            typeof(ICommand),
            typeof(Pager),
            new PropertyMetadata(null)
        );

    /// <summary>
    /// Identifies the <see cref="PreviousToolTip"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty PreviousToolTipProperty =
        DependencyProperty.Register(
            nameof(PreviousToolTip),
            typeof(string),
            typeof(Pager),
            new PropertyMetadata(string.Empty)
        );

    /// <summary>
    /// Identifies the <see cref="NextCommand"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty NextCommandProperty =
        DependencyProperty.Register(
            nameof(NextCommand),
            typeof(ICommand),
            typeof(Pager),
            new PropertyMetadata(null)
        );

    /// <summary>
    /// Identifies the <see cref="NextToolTip"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty NextToolTipProperty =
        DependencyProperty.Register(
            nameof(NextToolTip),
            typeof(string),
            typeof(Pager),
            new PropertyMetadata(string.Empty)
        );

    /// <summary>
    /// Gets or sets the text displayed between the buttons. The default is
    /// <see cref="string.Empty"/>.
    /// </summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Gets or sets the command executed by the previous button. The default is
    /// <see langword="null"/>.
    /// </summary>
    public ICommand? PreviousCommand
    {
        get => (ICommand?)GetValue(PreviousCommandProperty);
        set => SetValue(PreviousCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the tooltip of the previous button. The default is
    /// <see cref="string.Empty"/>.
    /// </summary>
    public string PreviousToolTip
    {
        get => (string)GetValue(PreviousToolTipProperty);
        set => SetValue(PreviousToolTipProperty, value);
    }

    /// <summary>
    /// Gets or sets the command executed by the next button. The default is
    /// <see langword="null"/>.
    /// </summary>
    public ICommand? NextCommand
    {
        get => (ICommand?)GetValue(NextCommandProperty);
        set => SetValue(NextCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the tooltip of the next button. The default is
    /// <see cref="string.Empty"/>.
    /// </summary>
    public string NextToolTip
    {
        get => (string)GetValue(NextToolTipProperty);
        set => SetValue(NextToolTipProperty, value);
    }
}
