using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Quaply.Ui.Views.Organisms;

/// <summary>
/// A presentational organism that displays a summary text on the left and the
/// page size selector with page navigation on the right.
/// </summary>
///
/// <remarks>
/// <para>
/// The control composes <c>AnnotatedText</c>, <c>LabeledComboBox</c> and
/// <c>Pager</c>, and is responsible only for presenting the supplied values. The
/// consumer provides the summary text, the page size options, the selected page
/// size, the page indicator text, the commands executed by the navigation
/// buttons and the tooltips shown on them.
/// </para>
///
/// <para>
/// The control does not calculate pages, slice data or track the current page.
/// Whether a navigation button is enabled is decided entirely by the
/// <c>CanExecute</c> logic of <see cref="PreviousPageCommand"/> and
/// <see cref="NextPageCommand"/>.
/// </para>
///
/// <para>
/// The secondary text is hidden by default. Setting <see cref="SecondaryText"/>
/// does not make it visible automatically; visibility is controlled independently
/// through <see cref="IsSecondaryTextVisible"/>.
/// </para>
///
/// <para>
/// The control does not provide any display text of its own. The navigation
/// tooltips are empty unless the consumer sets <see cref="PreviousPageToolTip"/>
/// and <see cref="NextPageToolTip"/>.
/// </para>
///
/// <para>
/// <see cref="PageSize"/> binds two-way by default, so the size chosen by the
/// user flows back to the consumer without specifying <c>Mode=TwoWay</c>.
/// </para>
///
/// <para>
/// The control does not set its own <c>DataContext</c>, so bindings on its
/// properties resolve against the consumer.
/// </para>
///
/// <para><b>Example</b></para>
/// <code>
/// &lt;organisms:PaginationFooter
///   Text="{Binding SummaryText}"
///   SecondaryText="{Binding SecondarySummaryText}"
///   IsSecondaryTextVisible="{Binding HasSecondarySummary}"
///   PageSizeLabel="Rows per page"
///   PageSizeOptions="{Binding PageSizeOptions}"
///   PageSize="{Binding SelectedPageSize}"
///   PageIndicatorText="{Binding PageIndicatorText}"
///   PreviousPageCommand="{Binding GoToPreviousPageCommand}"
///   PreviousPageToolTip="Previous page"
///   NextPageCommand="{Binding GoToNextPageCommand}"
///   NextPageToolTip="Next page"
/// /&gt;
/// </code>
/// </remarks>
public partial class PaginationFooter : UserControl
{
    /// <summary>
    /// Identifies the <see cref="Text"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(PaginationFooter),
            new PropertyMetadata(string.Empty)
        );

    /// <summary>
    /// Identifies the <see cref="SecondaryText"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SecondaryTextProperty =
        DependencyProperty.Register(
            nameof(SecondaryText),
            typeof(string),
            typeof(PaginationFooter),
            new PropertyMetadata(string.Empty)
        );

    /// <summary>
    /// Identifies the <see cref="IsSecondaryTextVisible"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsSecondaryTextVisibleProperty =
        DependencyProperty.Register(
            nameof(IsSecondaryTextVisible),
            typeof(bool),
            typeof(PaginationFooter),
            new PropertyMetadata(false)
        );

    /// <summary>
    /// Identifies the <see cref="PageSizeLabel"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty PageSizeLabelProperty =
        DependencyProperty.Register(
            nameof(PageSizeLabel),
            typeof(string),
            typeof(PaginationFooter),
            new PropertyMetadata(string.Empty)
        );

    /// <summary>
    /// Identifies the <see cref="PageSizeOptions"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty PageSizeOptionsProperty =
        DependencyProperty.Register(
            nameof(PageSizeOptions),
            typeof(IEnumerable),
            typeof(PaginationFooter),
            new PropertyMetadata(null)
        );

    /// <summary>
    /// Identifies the <see cref="PageSize"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty PageSizeProperty =
        DependencyProperty.Register(
            nameof(PageSize),
            typeof(object),
            typeof(PaginationFooter),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault
            )
        );

    /// <summary>
    /// Identifies the <see cref="PageIndicatorText"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty PageIndicatorTextProperty =
        DependencyProperty.Register(
            nameof(PageIndicatorText),
            typeof(string),
            typeof(PaginationFooter),
            new PropertyMetadata(string.Empty)
        );

    /// <summary>
    /// Identifies the <see cref="PreviousPageCommand"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty PreviousPageCommandProperty =
        DependencyProperty.Register(
            nameof(PreviousPageCommand),
            typeof(ICommand),
            typeof(PaginationFooter),
            new PropertyMetadata(null)
        );

    /// <summary>
    /// Identifies the <see cref="PreviousPageToolTip"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty PreviousPageToolTipProperty =
        DependencyProperty.Register(
            nameof(PreviousPageToolTip),
            typeof(string),
            typeof(PaginationFooter),
            new PropertyMetadata(string.Empty)
        );

    /// <summary>
    /// Identifies the <see cref="NextPageCommand"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty NextPageCommandProperty =
        DependencyProperty.Register(
            nameof(NextPageCommand),
            typeof(ICommand),
            typeof(PaginationFooter),
            new PropertyMetadata(null)
        );

    /// <summary>
    /// Identifies the <see cref="NextPageToolTip"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty NextPageToolTipProperty =
        DependencyProperty.Register(
            nameof(NextPageToolTip),
            typeof(string),
            typeof(PaginationFooter),
            new PropertyMetadata(string.Empty)
        );

    public PaginationFooter()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Gets or sets the primary text displayed on the left side of the footer.
    /// The default is <see cref="string.Empty"/>.
    /// </summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Gets or sets the secondary text displayed beside the primary text when
    /// <see cref="IsSecondaryTextVisible"/> is <see langword="true"/>.
    /// The default is <see cref="string.Empty"/>.
    /// </summary>
    public string SecondaryText
    {
        get => (string)GetValue(SecondaryTextProperty);
        set => SetValue(SecondaryTextProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the secondary text is visible.
    /// The default is <see langword="false"/>.
    /// </summary>
    public bool IsSecondaryTextVisible
    {
        get => (bool)GetValue(IsSecondaryTextVisibleProperty);
        set => SetValue(IsSecondaryTextVisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets the label displayed beside the page size selector.
    /// The default is <see cref="string.Empty"/>.
    /// </summary>
    public string PageSizeLabel
    {
        get => (string)GetValue(PageSizeLabelProperty);
        set => SetValue(PageSizeLabelProperty, value);
    }

    /// <summary>
    /// Gets or sets the collection of page sizes the user can choose from.
    /// The default is <see langword="null"/>.
    /// </summary>
    public IEnumerable? PageSizeOptions
    {
        get => (IEnumerable?)GetValue(PageSizeOptionsProperty);
        set => SetValue(PageSizeOptionsProperty, value);
    }

    /// <summary>
    /// Gets or sets the currently selected page size. The default is
    /// <see langword="null"/>, which means no page size is selected.
    /// </summary>
    public object? PageSize
    {
        get => GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the text displayed beside the navigation buttons, for example
    /// the current page position. The default is <see cref="string.Empty"/>.
    /// </summary>
    public string PageIndicatorText
    {
        get => (string)GetValue(PageIndicatorTextProperty);
        set => SetValue(PageIndicatorTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the command executed when the previous page button is clicked.
    /// The default is <see langword="null"/>.
    /// </summary>
    public ICommand? PreviousPageCommand
    {
        get => (ICommand?)GetValue(PreviousPageCommandProperty);
        set => SetValue(PreviousPageCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the tooltip shown on the previous page button.
    /// The default is <see cref="string.Empty"/>.
    /// </summary>
    public string PreviousPageToolTip
    {
        get => (string)GetValue(PreviousPageToolTipProperty);
        set => SetValue(PreviousPageToolTipProperty, value);
    }

    /// <summary>
    /// Gets or sets the command executed when the next page button is clicked.
    /// The default is <see langword="null"/>.
    /// </summary>
    public ICommand? NextPageCommand
    {
        get => (ICommand?)GetValue(NextPageCommandProperty);
        set => SetValue(NextPageCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the tooltip shown on the next page button.
    /// The default is <see cref="string.Empty"/>.
    /// </summary>
    public string NextPageToolTip
    {
        get => (string)GetValue(NextPageToolTipProperty);
        set => SetValue(NextPageToolTipProperty, value);
    }
}
