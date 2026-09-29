using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Quaply.Ui.AttachedProperties;

public static class DataGridStarColumnRefreshBehavior
{
    public static readonly DependencyProperty RefreshTriggerProperty =
        DependencyProperty.RegisterAttached(
            "RefreshTrigger",
            typeof(object),
            typeof(DataGridStarColumnRefreshBehavior),
            new PropertyMetadata(null, OnRefreshTriggerChanged)
        );

    public static void SetRefreshTrigger(DependencyObject element, object value)
    {
        element.SetValue(RefreshTriggerProperty, value);
    }

    public static object GetRefreshTrigger(DependencyObject element)
    {
        return element.GetValue(RefreshTriggerProperty);
    }

    private static void OnRefreshTriggerChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        if (d is not DataGrid grid)
        {
            return;
        }

        // Wait for the layout pass triggered by the Visibility binding
        // to complete before forcing a recalculation of the Star width.
        grid.Dispatcher.BeginInvoke(
            DispatcherPriority.Loaded,
            new Action(() =>
            {
                foreach (DataGridColumn? column in grid.Columns)
                {
                    if (!column.Width.IsStar)
                    {
                        continue;
                    }

                    DataGridLength starWidth = column.Width;
                    column.Width = new DataGridLength(
                        0,
                        DataGridLengthUnitType.Pixel
                    );
                    column.Width = starWidth;
                }
            })
        );
    }
}
