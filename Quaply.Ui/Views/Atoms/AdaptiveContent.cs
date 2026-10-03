using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Quaply.Ui.Foundations;

namespace Quaply.Ui.Views.Atoms;

/// <summary>
/// Shows exactly one of <see cref="Compact"/>, <see cref="Medium"/> or <see cref="Expanded"/>
/// depending on the current size class, resolved through <see cref="Breakpoints"/>.
/// <para>
/// - Slots are <see cref="DataTemplate"/>s, so only the active layout is ever instantiated.
///   The template's DataContext is this panel's DataContext.<br/>
/// - Fallback goes downwards: Expanded -> Medium -> Compact.
///   A slot that is not declared reuses the next smaller one.<br/>
/// - <see cref="Breakpoints"/> defaults to <see cref="BreakpointSet.Default"/>.
///   Override per instance with a string: <c>Breakpoints="0;500;700"</c>.<br/>
/// - Derives from <see cref="ContentPresenter"/>: no control template, no extra Border.
/// </para>
/// <para>
/// Width source: set <see cref="ReferenceWidth"/> (e.g. bound to a container's ActualWidth).
/// If left unset, the panel measures itself. Do NOT rely on self-measuring when the panel sits in
/// an Auto-sized container, because its content would then affect its own width.
/// </para>
/// </summary>
public sealed class AdaptiveContent : ContentPresenter
{
    public static readonly DependencyProperty CompactProperty = RegisterSlot(
        nameof(Compact)
    );
    public static readonly DependencyProperty MediumProperty = RegisterSlot(
        nameof(Medium)
    );
    public static readonly DependencyProperty ExpandedProperty = RegisterSlot(
        nameof(Expanded)
    );

    public static readonly DependencyProperty ReferenceWidthProperty =
        DependencyProperty.Register(
            nameof(ReferenceWidth),
            typeof(double),
            typeof(AdaptiveContent),
            new PropertyMetadata(double.NaN, OnInputChanged)
        );

    public static readonly DependencyProperty BreakpointsProperty =
        DependencyProperty.Register(
            nameof(Breakpoints),
            typeof(BreakpointSet),
            typeof(AdaptiveContent),
            new PropertyMetadata(BreakpointSet.Default, OnInputChanged)
        );

    private static readonly DependencyPropertyKey SizeClassPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(SizeClass),
            typeof(SizeClass),
            typeof(AdaptiveContent),
            new PropertyMetadata(SizeClass.Compact)
        );

    public static readonly DependencyProperty SizeClassProperty =
        SizeClassPropertyKey.DependencyProperty;

    public AdaptiveContent()
    {
        SizeChanged += OnSizeChanged;
    }

    public DataTemplate? Compact
    {
        get => (DataTemplate?)GetValue(CompactProperty);
        set => SetValue(CompactProperty, value);
    }

    public DataTemplate? Medium
    {
        get => (DataTemplate?)GetValue(MediumProperty);
        set => SetValue(MediumProperty, value);
    }

    public DataTemplate? Expanded
    {
        get => (DataTemplate?)GetValue(ExpandedProperty);
        set => SetValue(ExpandedProperty, value);
    }

    /// <summary>
    /// Width used to pick the size class.
    /// <see cref="double.NaN"/> (default) = use own ActualWidth.
    /// </summary>
    public double ReferenceWidth
    {
        get => (double)GetValue(ReferenceWidthProperty);
        set => SetValue(ReferenceWidthProperty, value);
    }

    /// <summary>
    /// Breakpoints used to resolve the size class.
    /// Defaults to <see cref="BreakpointSet.Default"/>.
    /// In XAML: <c>Breakpoints="0;500;700"</c> (Compact;Medium;Expanded lower bounds).
    /// </summary>
    public BreakpointSet? Breakpoints
    {
        get => (BreakpointSet?)GetValue(BreakpointsProperty);
        set => SetValue(BreakpointsProperty, value);
    }

    /// <summary>
    /// The currently active size class (read-only, bindable).
    /// </summary>
    public SizeClass SizeClass => (SizeClass)GetValue(SizeClassProperty);

    private static DependencyProperty RegisterSlot(string name) =>
        DependencyProperty.Register(
            name,
            typeof(DataTemplate),
            typeof(AdaptiveContent),
            new PropertyMetadata(null, OnInputChanged)
        );

    private static void OnInputChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        ((AdaptiveContent)d).Refresh();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (double.IsNaN(ReferenceWidth))
        {
            Refresh();
        }
    }

    private void Refresh()
    {
        double width = double.IsNaN(ReferenceWidth)
            ? ActualWidth
            : ReferenceWidth;

        // Not measured yet: don't build a layout for a width we don't know.
        if (width <= 0)
        {
            return;
        }

        SizeClass sizeClass = (Breakpoints ?? BreakpointSet.Default).Resolve(
            width
        );
        SetValue(SizeClassPropertyKey, sizeClass);

        DataTemplate? template = sizeClass switch
        {
            SizeClass.Expanded => Expanded ?? Medium ?? Compact,
            SizeClass.Medium => Medium ?? Compact,
            _ => Compact,
        };

        if (ReferenceEquals(template, ContentTemplate))
        {
            return;
        }

        ContentTemplate = template;

        // Content = our own DataContext, so the template's root inherits the ViewModel.
        // Without a template we clear Content, otherwise ContentPresenter would print ToString().
        if (template is null)
        {
            ClearValue(ContentProperty);
        }
        else if (GetBindingExpression(ContentProperty) is null)
        {
            SetBinding(ContentProperty, new Binding());
        }
    }
}
