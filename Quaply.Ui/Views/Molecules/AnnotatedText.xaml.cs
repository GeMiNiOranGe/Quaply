using System.Windows;
using System.Windows.Controls;

namespace Quaply.Ui.Views.Molecules;

/// <summary>
/// A presentational molecule that displays a primary text and an optional secondary text.
/// </summary>
///
/// <remarks>
/// <para>
/// The secondary text is hidden by default. Setting <see cref="SecondaryText"/>
/// does not make it visible automatically; visibility is controlled independently
/// through <see cref="IsSecondaryTextVisible"/>.
/// </para>
/// <para>
/// The visual structure lives in the default <see cref="ControlTemplate"/>
/// (Themes/Generic.xaml) and can be replaced by the consumer.
/// </para>
///
/// <para><b>Example</b></para>
/// <code language="xml">
/// &lt;molecules:AnnotatedText
///   Text="{Binding Summary}"
///   SecondaryText="{Binding Description}"
///   IsSecondaryTextVisible="{Binding HasDescription}"
/// /&gt;
/// </code>
/// </remarks>
public class AnnotatedText : Control
{
    static AnnotatedText()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(AnnotatedText),
            new FrameworkPropertyMetadata(typeof(AnnotatedText))
        );
    }

    /// <summary>
    /// Identifies the <see cref="Text"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(AnnotatedText),
            new PropertyMetadata(string.Empty)
        );

    /// <summary>
    /// Identifies the <see cref="SecondaryText"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SecondaryTextProperty =
        DependencyProperty.Register(
            nameof(SecondaryText),
            typeof(string),
            typeof(AnnotatedText),
            new PropertyMetadata(string.Empty)
        );

    /// <summary>
    /// Identifies the <see cref="IsSecondaryTextVisible"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsSecondaryTextVisibleProperty =
        DependencyProperty.Register(
            nameof(IsSecondaryTextVisible),
            typeof(bool),
            typeof(AnnotatedText),
            new PropertyMetadata(false)
        );

    /// <summary>
    /// Gets or sets the primary text displayed by the control. The default is
    /// <see cref="string.Empty"/>.
    /// </summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Gets or sets the secondary text displayed by the control when
    /// <see cref="IsSecondaryTextVisible"/> is <see langword="true"/>.
    /// The default is <see cref="string.Empty"/>.
    /// </summary>
    public string SecondaryText
    {
        get => (string)GetValue(SecondaryTextProperty);
        set => SetValue(SecondaryTextProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the secondary text is visible.
    /// The default is <see langword="false"/>.
    /// </summary>
    public bool IsSecondaryTextVisible
    {
        get => (bool)GetValue(IsSecondaryTextVisibleProperty);
        set => SetValue(IsSecondaryTextVisibleProperty, value);
    }
}
