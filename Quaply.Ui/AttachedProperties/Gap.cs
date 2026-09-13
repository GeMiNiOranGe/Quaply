using System.Windows;
using System.Windows.Controls;

namespace Quaply.Ui.AttachedProperties;

public static class Gap
{
    public static readonly DependencyProperty VerticalProperty =
        DependencyProperty.RegisterAttached(
            "Vertical",
            typeof(double),
            typeof(Gap),
            new PropertyMetadata(0.0, OnGapChanged)
        );

    public static readonly DependencyProperty HorizontalProperty =
        DependencyProperty.RegisterAttached(
            "Horizontal",
            typeof(double),
            typeof(Gap),
            new PropertyMetadata(0.0, OnGapChanged)
        );

    public static readonly DependencyProperty UniformProperty =
        DependencyProperty.RegisterAttached(
            "Uniform",
            typeof(double),
            typeof(Gap),
            new PropertyMetadata(0.0, OnUniformChanged)
        );

    public static void SetVertical(DependencyObject d, double value)
    {
        d.SetValue(VerticalProperty, value);
    }

    public static double GetVertical(DependencyObject d)
    {
        return (double)d.GetValue(VerticalProperty);
    }

    public static void SetHorizontal(DependencyObject d, double value)
    {
        d.SetValue(HorizontalProperty, value);
    }

    public static double GetHorizontal(DependencyObject d)
    {
        return (double)d.GetValue(HorizontalProperty);
    }

    public static void SetUniform(DependencyObject d, double value)
    {
        d.SetValue(UniformProperty, value);
    }

    public static double GetUniform(DependencyObject d)
    {
        return (double)d.GetValue(UniformProperty);
    }

    private static void OnUniformChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        d.SetValue(VerticalProperty, e.NewValue);
        d.SetValue(HorizontalProperty, e.NewValue);
    }

    // Store the original margin of each child so that Apply() is always idempotent.
    private static readonly DependencyProperty BaseMarginProperty =
        DependencyProperty.RegisterAttached(
            "BaseMargin",
            typeof(Thickness),
            typeof(Gap),
            new PropertyMetadata(new Thickness(double.NaN))
        ); // NaN = Not yet captured

    private static void OnGapChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        if (d is not Panel panel)
        {
            return;
        }

        // Avoid duplicate registrations if the Vertical/Horizontal changes frequently.
        panel.Loaded -= Panel_Loaded;
        panel.Loaded += Panel_Loaded;

        if (panel.IsLoaded)
        {
            Apply(panel);
        }
    }

    private static void Panel_Loaded(object sender, RoutedEventArgs e)
    {
        Apply((Panel)sender);
    }

    private static Thickness GetOrCaptureBaseMargin(FrameworkElement child)
    {
        Thickness stored = (Thickness)child.GetValue(BaseMarginProperty);

        if (double.IsNaN(stored.Left))
        {
            // preserve the original margin you declared in XAML (if any)
            stored = child.Margin;
            child.SetValue(BaseMarginProperty, stored);
        }

        return stored;
    }

    private static void Apply(Panel panel)
    {
        double vGap = GetVertical(panel);
        double hGap = GetHorizontal(panel);
        if (vGap == 0 && hGap == 0)
        {
            return;
        }

        bool horizontal =
            panel is StackPanel sp && sp.Orientation == Orientation.Horizontal;
        double gap = horizontal ? hGap : vGap;

        List<FrameworkElement> children =
        [
            .. panel.Children.OfType<FrameworkElement>(),
        ];

        for (int i = 0; i < children.Count; i++)
        {
            FrameworkElement child = children[i];
            Thickness baseMargin = GetOrCaptureBaseMargin(child);
            double trailing = i == children.Count - 1 ? 0 : gap;

            child.Margin = horizontal
                ? new Thickness(
                    baseMargin.Left,
                    baseMargin.Top,
                    baseMargin.Right + trailing,
                    baseMargin.Bottom
                )
                : new Thickness(
                    baseMargin.Left,
                    baseMargin.Top,
                    baseMargin.Right,
                    baseMargin.Bottom + trailing
                );
        }
    }
}
