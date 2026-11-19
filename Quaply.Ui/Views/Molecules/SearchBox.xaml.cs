using System.Windows;
using System.Windows.Controls;

namespace Quaply.Ui.Views.Molecules;

/// <summary>
/// Interaction logic for SearchBox.xaml
/// </summary>
public partial class SearchBox : UserControl
{
    public static readonly DependencyProperty TextBoxPaddingProperty =
        DependencyProperty.Register(
            nameof(TextBoxPadding),
            typeof(Thickness?),
            typeof(SearchBox),
            new PropertyMetadata(null, OnTextBoxPaddingChanged)
        );

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(SearchBox),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault
            )
        );

    public static readonly DependencyProperty PlaceholderTextProperty =
        DependencyProperty.Register(
            nameof(PlaceholderText),
            typeof(string),
            typeof(SearchBox),
            new PropertyMetadata("Search...")
        );

    public static readonly DependencyProperty IsBusyProperty =
        DependencyProperty.Register(
            nameof(IsBusy),
            typeof(bool),
            typeof(SearchBox),
            new PropertyMetadata(false)
        );

    public static readonly DependencyProperty ProgressRingSizeProperty =
        DependencyProperty.Register(
            nameof(ProgressRingSize),
            typeof(double),
            typeof(SearchBox),
            new PropertyMetadata(16d)
        );

    public SearchBox()
    {
        InitializeComponent();
    }

    // --- TextBoxPadding ---

    public Thickness? TextBoxPadding
    {
        get => (Thickness?)GetValue(TextBoxPaddingProperty);
        set => SetValue(TextBoxPaddingProperty, value);
    }

    private static void OnTextBoxPaddingChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        SearchBox searchBox = (SearchBox)d;

        if (e.NewValue is Thickness padding)
        {
            searchBox.innerTextBox.Padding = padding;
        }
        else
        {
            searchBox.innerTextBox.ClearValue(PaddingProperty);
        }
    }

    // --- Text ---

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    // --- PlaceholderText ---

    public string PlaceholderText
    {
        get => (string)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    // --- IsBusy ---

    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }

    // --- ProgressRingSize ---

    public double ProgressRingSize
    {
        get => (double)GetValue(ProgressRingSizeProperty);
        set => SetValue(ProgressRingSizeProperty, value);
    }
}
