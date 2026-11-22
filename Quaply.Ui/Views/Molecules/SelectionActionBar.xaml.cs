using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace Quaply.Ui.Views.Molecules;

/// <summary>
/// A presentational molecule: a floating bar that summarizes how many items are
/// selected, hosts the actions available for them, and lets the user clear the selection.
/// </summary>
///
/// <remarks>
/// <para>
/// The control knows nothing about what is selected or which actions apply.
/// The consumer supplies the count, the actions and the clear command.
/// </para>
///
/// <para>
/// Placement (<see cref="FrameworkElement.Margin"/>, alignment,
/// <see cref="FrameworkElement.MaxWidth"/>) is the consumer's responsibility.
/// The control does not set its own DataContext, so bindings on its properties
/// and inside <see cref="Actions"/> resolve against the consumer.
/// </para>
///
/// <para><b>Example</b></para>
/// <code language="xml">
/// &lt;molecules:SelectionActionBar
///   IsOpen="{Binding HasSelection}"
///   SelectedCount="{Binding SelectedCount}"
///   ClearCommand="{Binding ClearSelectionCommand}"
/// &gt;
///   &lt;ui:Button Command="{Binding DeleteSelectedCommand}" Content="Delete" /&gt;
/// &lt;/molecules:SelectionActionBar&gt;
/// </code>
/// </remarks>
[ContentProperty(nameof(Actions))]
public partial class SelectionActionBar : UserControl
{
    /// <summary>
    /// Identifies the <see cref="SelectedCount"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SelectedCountProperty =
        DependencyProperty.Register(
            nameof(SelectedCount),
            typeof(int),
            typeof(SelectionActionBar),
            new PropertyMetadata(0, OnSummaryInputChanged)
        );

    /// <summary>
    /// Identifies the <see cref="SummaryFormat"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SummaryFormatProperty =
        DependencyProperty.Register(
            nameof(SummaryFormat),
            typeof(string),
            typeof(SelectionActionBar),
            new PropertyMetadata("{0} selected", OnSummaryInputChanged)
        );

    /// <summary>
    /// Identifies the <see cref="ClearCommand"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ClearCommandProperty =
        DependencyProperty.Register(
            nameof(ClearCommand),
            typeof(ICommand),
            typeof(SelectionActionBar),
            new PropertyMetadata(null)
        );

    /// <summary>
    /// Identifies the <see cref="ClearToolTip"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ClearToolTipProperty =
        DependencyProperty.Register(
            nameof(ClearToolTip),
            typeof(string),
            typeof(SelectionActionBar),
            new PropertyMetadata("Clear selection")
        );

    /// <summary>
    /// Identifies the <see cref="IsOpen"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(
            nameof(IsOpen),
            typeof(bool),
            typeof(SelectionActionBar),
            new PropertyMetadata(false)
        );

    /// <summary>
    /// Identifies the <see cref="Actions"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ActionsProperty =
        DependencyProperty.Register(
            nameof(Actions),
            typeof(object),
            typeof(SelectionActionBar),
            new PropertyMetadata(null)
        );

    // Read-only: the formatted text is derived from SelectedCount + SummaryFormat.
    private static readonly DependencyPropertyKey SummaryTextPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(SummaryText),
            typeof(string),
            typeof(SelectionActionBar),
            new PropertyMetadata(string.Empty)
        );

    /// <summary>
    /// Identifies the <see cref="SummaryText"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SummaryTextProperty =
        SummaryTextPropertyKey.DependencyProperty;

    public SelectionActionBar()
    {
        UpdateSummaryText();
        InitializeComponent();
    }

    /// <summary>
    /// Gets or sets the number of selected items. The default is 0.
    /// </summary>
    public int SelectedCount
    {
        get => (int)GetValue(SelectedCountProperty);
        set => SetValue(SelectedCountProperty, value);
    }

    /// <summary>
    /// Gets or sets the composite format used for the summary, where <c>{0}</c> is
    /// <see cref="SelectedCount"/>. The default is <c>"{0} selected"</c>.
    /// </summary>
    public string SummaryFormat
    {
        get => (string)GetValue(SummaryFormatProperty);
        set => SetValue(SummaryFormatProperty, value);
    }

    /// <summary>
    /// Gets or sets the command executed by the clear button.
    /// </summary>
    public ICommand? ClearCommand
    {
        get => (ICommand?)GetValue(ClearCommandProperty);
        set => SetValue(ClearCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the tooltip of the clear button. The default is "Clear selection".
    /// </summary>
    public string ClearToolTip
    {
        get => (string)GetValue(ClearToolTipProperty);
        set => SetValue(ClearToolTipProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the bar is shown. Changing it plays
    /// the show/hide animation. The default is <c>false</c>.
    /// </summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>
    /// Gets or sets the actions shown before the clear button, usually buttons.
    /// Wrap several elements in a horizontal panel, because this is a single object.
    /// </summary>
    public object? Actions
    {
        get => GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }

    /// <summary>
    /// Gets the formatted summary text. Read-only.
    /// </summary>
    public string SummaryText => (string)GetValue(SummaryTextProperty);

    private static void OnSummaryInputChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        ((SelectionActionBar)d).UpdateSummaryText();
    }

    private void UpdateSummaryText()
    {
        string text;

        try
        {
            text = string.Format(
                CultureInfo.CurrentCulture,
                SummaryFormat ?? "{0}",
                SelectedCount
            );
        }
        catch (FormatException)
        {
            text = SelectedCount.ToString(CultureInfo.CurrentCulture);
        }

        SetValue(SummaryTextPropertyKey, text);
    }
}
