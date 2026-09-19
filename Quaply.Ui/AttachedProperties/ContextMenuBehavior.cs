using System.Windows;
using System.Windows.Controls;

namespace Quaply.Ui.AttachedProperties;

public static class ContextMenuBehavior
{
    public static readonly DependencyProperty OpenOnClickProperty =
        DependencyProperty.RegisterAttached(
            "OpenOnClick",
            typeof(bool),
            typeof(ContextMenuBehavior),
            new PropertyMetadata(false, OnOpenOnClickChanged)
        );

    public static void SetOpenOnClick(DependencyObject element, bool value)
    {
        element.SetValue(OpenOnClickProperty, value);
    }

    public static bool GetOpenOnClick(DependencyObject element)
    {
        return (bool)element.GetValue(OpenOnClickProperty);
    }

    private static void OnOpenOnClickChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        if (d is not Button button)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            button.Click += Button_Click;
        }
        else
        {
            button.Click -= Button_Click;
        }
    }

    private static void Button_Click(object sender, RoutedEventArgs e)
    {
        Button button = (Button)sender;
        button.ContextMenu.PlacementTarget = button;
        button.ContextMenu.IsOpen = true;
    }
}
