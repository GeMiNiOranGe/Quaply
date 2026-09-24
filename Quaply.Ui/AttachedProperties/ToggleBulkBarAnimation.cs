using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Quaply.Ui.AttachedProperties;

/// <summary>
/// Attached property to toggle the visibility of the BulkBar with animation.
/// </summary>
public class ToggleBulkBarAnimation
{
    public static readonly DependencyProperty IsVisibleProperty =
        DependencyProperty.RegisterAttached(
            "IsVisible",
            typeof(bool),
            typeof(ToggleBulkBarAnimation),
            new PropertyMetadata(true, OnIsVisibleChanged)
        );

    public static void SetIsVisible(DependencyObject d, bool value)
    {
        d.SetValue(IsVisibleProperty, value);
    }

    public static bool GetIsVisible(DependencyObject d)
    {
        return (bool)d.GetValue(IsVisibleProperty);
    }

    private static void OnIsVisibleChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        if (d is not FrameworkElement element)
        {
            return;
        }

        if (element.RenderTransform is not TranslateTransform)
        {
            element.RenderTransform = new TranslateTransform();
        }

        bool isVisible = (bool)e.NewValue;

        KeyboardNavigationMode mode = isVisible
            ? KeyboardNavigationMode.Continue
            : KeyboardNavigationMode.None;

        string resourceKey = isVisible
            ? "BulkBarShowStoryboard"
            : "BulkBarHideStoryboard";

        KeyboardNavigation.SetTabNavigation(element, mode);

        if (element.TryFindResource(resourceKey) is Storyboard storyboard)
        {
            storyboard.Begin(element);
        }
    }
}
