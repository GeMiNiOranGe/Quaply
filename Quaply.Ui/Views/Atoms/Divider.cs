using System.Windows;
using System.Windows.Controls;

namespace Quaply.Ui.Views.Atoms;

/// <summary>
/// A simple visual divider used to separate content sections, supporting both
/// horizontal and vertical orientations through a single control.
/// </summary>
/// <remarks>
/// <para>
/// Replaces the built-in WPF <see cref="Separator"/> for standalone layout
/// usage (e.g. between sections in a <see cref="StackPanel"/>),
/// since <see cref="Separator"/> is designed primarily for
/// <see cref="Menu"/>/<see cref="ContextMenu"/> item containers and relies
/// on theme resources (e.g. <c>SeparatorBorderBrush</c>) that are not part
/// of this application's design token set.
/// </para>
/// <para>
/// The default style and <see cref="ControlTemplate"/> for this control
/// are defined in <c>Themes/Generic.xaml</c>, following the standard WPF
/// Custom Control convention.
/// </para>
/// </remarks>
/// <example>
/// Horizontal divider (default):
/// <code>&lt;atoms:Divider /&gt;</code>
/// Vertical divider:
/// <code>&lt;atoms:Divider Orientation="Vertical" /&gt;</code>
/// </example>
public class Divider : Control
{
    /// <summary>
    /// Identifies the <see cref="Orientation"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(Divider),
            new FrameworkPropertyMetadata(Orientation.Horizontal)
        );

    /// <summary>
    /// Overrides the default style key so WPF resolves the control's default
    /// <see cref="Style"/> from <c>Themes/Generic.xaml</c> instead of the base
    /// <see cref="Control"/> style.
    /// </summary>
    static Divider()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Divider),
            new FrameworkPropertyMetadata(typeof(Divider))
        );
    }

    /// <summary>
    /// Gets or sets the orientation of the divider line.
    /// Defaults to <see cref="Orientation.Horizontal"/>.
    /// </summary>
    /// <value>
    /// <see cref="Orientation.Horizontal"/> to render a full-width horizontal rule,
    /// or <see cref="Orientation.Vertical"/> to render a full-height vertical rule.
    /// </value>
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
}
