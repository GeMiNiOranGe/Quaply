using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Quaply.Ui.Views.Atoms;

/// <summary>
/// The ordered layout of an <see cref="AdaptiveDataGrid"/>: plain
/// <see cref="DataGridColumn"/>s (always present) and <see cref="AdaptiveColumn"/>s
/// (resolved per width), in final display order.
/// <para>
/// Items are validated when added, so a wrong item type fails at XAML load with
/// the offending line instead of later during a width change.
/// </para>
/// </summary>
public sealed class AdaptiveColumns : ObservableCollection<object>
{
    protected override void InsertItem(int index, object item)
    {
        Validate(item);
        base.InsertItem(index, item);
    }

    protected override void SetItem(int index, object item)
    {
        Validate(item);
        base.SetItem(index, item);
    }

    private static void Validate(object item)
    {
        if (item is not (DataGridColumn or AdaptiveColumn))
        {
            throw new ArgumentException(
                $"AdaptiveDataGrid only accepts DataGridColumn or AdaptiveColumn, got {item?.GetType().Name ?? "null"}."
            );
        }
    }
}
