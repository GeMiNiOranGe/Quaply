using System.Windows;

namespace Quaply.Ui.AttachedProperties;

/// <summary>
/// Track the element's ActualWidth and push the "narrower than Breakpoint" result
/// to <see cref="IsBelowBreakpointProperty"/> (using Mode=OneWayToSource
/// to push it to the ViewModel).
/// </summary>
public static class WidthBreakpointBehavior
{
    public static readonly DependencyProperty BreakpointProperty =
        DependencyProperty.RegisterAttached(
            "Breakpoint",
            typeof(double),
            typeof(WidthBreakpointBehavior),
            new PropertyMetadata(double.NaN, OnBreakpointChanged)
        );

    public static readonly DependencyProperty IsBelowBreakpointProperty =
        DependencyProperty.RegisterAttached(
            "IsBelowBreakpoint",
            typeof(bool),
            typeof(WidthBreakpointBehavior),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault
            )
        );

    // --- Breakpoint ---

    public static double GetBreakpoint(DependencyObject obj)
    {
        return (double)obj.GetValue(BreakpointProperty);
    }

    public static void SetBreakpoint(DependencyObject obj, double value)
    {
        obj.SetValue(BreakpointProperty, value);
    }

    private static void OnBreakpointChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        if (d is not FrameworkElement element)
        {
            return;
        }

        element.SizeChanged -= OnSizeChanged;

        if (e.NewValue is double breakpoint && !double.IsNaN(breakpoint))
        {
            element.SizeChanged += OnSizeChanged;
            Evaluate(element);
        }
    }

    // --- IsBelowBreakpoint ---

    public static bool GetIsBelowBreakpoint(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsBelowBreakpointProperty);
    }

    public static void SetIsBelowBreakpoint(DependencyObject obj, bool value)
    {
        obj.SetValue(IsBelowBreakpointProperty, value);
    }

    // --- Utilities ---

    private static void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.WidthChanged)
        {
            Evaluate((FrameworkElement)sender);
        }
    }

    private static void Evaluate(FrameworkElement element)
    {
        double width = element.ActualWidth;

        // The first layout pass hasn't been completed yet,
        // so there are no reliable measurements.
        if (width <= 0)
        {
            return;
        }

        // SetCurrentValue (not SetValue): preserves the OneWayToSource binding.
        // SetValue removes the binding from the property.
        element.SetCurrentValue(
            IsBelowBreakpointProperty,
            width < GetBreakpoint(element)
        );
    }
}
