using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Quaply.Ui.Views.Molecules;

/// <summary>
/// A presentational molecule that displays a text with previous and next buttons.
/// </summary>
///
/// <remarks>
/// <para>
/// The control is responsible only for presenting the supplied text and the two
/// navigation buttons. The consumer provides the text, the commands executed by
/// each button and the tooltips shown on them.
/// </para>
///
/// <para>
/// The control does not track the current position or the number of pages. Whether
/// a button is enabled is decided entirely by the <c>CanExecute</c> logic of the
/// supplied <see cref="PreviousCommand"/> and <see cref="NextCommand"/>.
/// </para>
///
/// <para>
/// The control does not set its own <c>DataContext</c>, so bindings on its
/// properties resolve against the consumer.
/// </para>
///
/// <para><b>Example</b></para>
/// <code>
/// &lt;molecules:Pager
///   Text="{Binding PageText}"
///   PreviousCommand="{Binding GoToPreviousCommand}"
///   NextCommand="{Binding GoToNextCommand}"
///   PreviousToolTip="Previous page"
///   NextToolTip="Next page"
/// /&gt;
/// </code>
/// </remarks>
public partial class Pager : UserControl
{
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

    public Pager()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Gets or sets the text displayed between the navigation buttons, for example
    /// the current position. The default is <see cref="string.Empty"/>.
    /// </summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Gets or sets the command executed when the previous button is clicked.
    /// The default is <see langword="null"/>.
    /// </summary>
    public ICommand? PreviousCommand
    {
        get => (ICommand?)GetValue(PreviousCommandProperty);
        set => SetValue(PreviousCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the tooltip shown on the previous button. The default is null.
    /// </summary>
    public string PreviousToolTip
    {
        get => (string)GetValue(PreviousToolTipProperty);
        set => SetValue(PreviousToolTipProperty, value);
    }

    /// <summary>
    /// Gets or sets the command executed when the next button is clicked.
    /// The default is <see langword="null"/>.
    /// </summary>
    public ICommand? NextCommand
    {
        get => (ICommand?)GetValue(NextCommandProperty);
        set => SetValue(NextCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the tooltip shown on the next button. The default is null.
    /// </summary>
    public string NextToolTip
    {
        get => (string)GetValue(NextToolTipProperty);
        set => SetValue(NextToolTipProperty, value);
    }
}
