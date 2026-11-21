using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Wpf.Ui.Controls;

namespace Quaply.Ui.Views.Molecules;

/// <summary>
/// A presentational molecule that shows an icon, a title, a description and an
/// optional action area. It has no knowledge of why it is shown or which actions apply.
/// </summary>
/// <remarks>
/// The consumer decides when the control is visible (bind <see cref="UIElement.Visibility"/>)
/// and which actions appear (put them in <see cref="Actions"/> and control their visibility).
/// The control does not set its own DataContext, so bindings resolve against the consumer.
///
/// <para><b>Example</b></para>
/// <code language="xml">
/// &lt;molecules:EmptyState
///   Title="{Binding EmptyStateTitle}"
///   Description="{Binding EmptyStateDescription}"
///   Icon="{ui:SymbolIcon Briefcase24}"
///   Visibility="{Binding HasVisibleItems, Converter={converters:InverseBooleanToVisibilityConverter}}"
/// &gt;
///   &lt;StackPanel&gt;
///     &lt;ui:Button
///       Appearance="Primary"
///       Command="{Binding AddItemCommand}"
///       Content="Add item"
///       Icon="{ui:SymbolIcon Add24}"
///       Visibility="{Binding HasAnyItems, Converter={converters:InverseBooleanToVisibilityConverter}}"
///     /&gt;
///     &lt;ui:Button
///       Command="{Binding ClearFiltersCommand}"
///       Content="Clear filters"
///       Visibility="{Binding HasAnyItems, Converter={converters:BooleanToVisibilityConverter}}"
///     /&gt;
///   &lt;/StackPanel&gt;
/// &lt;/molecules:EmptyState&gt;
/// </code>
/// </remarks>
[ContentProperty(nameof(Actions))]
public partial class EmptyState : UserControl
{
    /// <summary>
    /// Identifies the <see cref="Icon"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(
            nameof(Icon),
            typeof(IconElement),
            typeof(EmptyState),
            new PropertyMetadata(null)
        );

    /// <summary>
    /// Identifies the <see cref="Title"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(EmptyState),
            new PropertyMetadata(string.Empty)
        );

    /// <summary>
    /// Identifies the <see cref="Description"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(
            nameof(Description),
            typeof(string),
            typeof(EmptyState),
            new PropertyMetadata(string.Empty)
        );

    /// <summary>
    /// Identifies the <see cref="Actions"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ActionsProperty =
        DependencyProperty.Register(
            nameof(Actions),
            typeof(object),
            typeof(EmptyState),
            new PropertyMetadata(null)
        );

    public EmptyState()
    {
        // Per-instance default: a UIElement in PropertyMetadata would be shared.
        SetCurrentValue(IconProperty, new SymbolIcon(SymbolRegular.Document24));

        InitializeComponent();
    }

    /// <summary>
    /// Gets or sets the icon displayed above the title.
    /// The default is a <see cref="SymbolIcon"/> with <see cref="SymbolRegular.Document24"/>.
    /// </summary>
    /// <remarks>
    /// Usage in XAML: <c>Icon="{ui:SymbolIcon Briefcase24}"</c>.
    /// The control applies its own size and foreground; set them on the icon only to override.
    /// </remarks>
    public IconElement? Icon
    {
        get => (IconElement?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Gets or sets the headline text, shown in semi-bold below the icon.
    /// The default is an empty string.
    /// </summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Gets or sets the secondary text shown below <see cref="Title"/>.
    /// The default is an empty string.
    /// </summary>
    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>
    /// Gets or sets the content shown below the description, usually buttons.
    /// Wrap several elements in a panel, because this is a single object.
    /// </summary>
    public object? Actions
    {
        get => GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }
}
