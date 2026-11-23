using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Quaply.Ui.Views.Molecules;

/// <summary>
/// A presentational molecule that displays a label next to a combo box.
/// </summary>
///
/// <remarks>
/// <para>
/// The control is responsible only for presenting the supplied label and items.
/// The consumer provides the label, the items to choose from and the currently
/// selected item.
/// </para>
///
/// <para>
/// <see cref="SelectedItem"/> binds two-way by default, so the selection made by
/// the user flows back to the consumer without specifying <c>Mode=TwoWay</c>.
/// </para>
///
/// <para>
/// The control does not set its own <c>DataContext</c>, so bindings on its
/// properties resolve against the consumer.
/// </para>
///
/// <para><b>Example</b></para>
/// <code>
/// &lt;molecules:LabeledComboBox
///   Label="Sort by"
///   ItemsSource="{Binding SortOptions}"
///   SelectedItem="{Binding SelectedSortOption}"
/// /&gt;
/// </code>
/// </remarks>
public partial class LabeledComboBox : UserControl
{
    /// <summary>
    /// Identifies the <see cref="Label"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            nameof(Label),
            typeof(string),
            typeof(LabeledComboBox),
            new PropertyMetadata(string.Empty)
        );

    /// <summary>
    /// Identifies the <see cref="ItemsSource"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(LabeledComboBox),
            new PropertyMetadata(null)
        );

    /// <summary>
    /// Identifies the <see cref="SelectedItem"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(object),
            typeof(LabeledComboBox),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault
            )
        );

    public LabeledComboBox()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Gets or sets the label displayed beside the combo box. The default is
    /// <see cref="string.Empty"/>.
    /// </summary>
    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>
    /// Gets or sets the collection of items the user can choose from.
    /// The default is <see langword="null"/>.
    /// </summary>
    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the currently selected item. The default is
    /// <see langword="null"/>, which means no item is selected.
    /// </summary>
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }
}
