using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Quaply.Ui.AttachedProperties;

namespace Quaply.Ui.Views.Atoms;

/// <summary>
/// A <see cref="DataGrid"/> whose columns adapt to the width, the grid-level
/// counterpart of <see cref="AdaptiveContent"/>.
/// <para>
/// Children are the <see cref="Layout"/>, read top to bottom as the final column
/// order: plain <see cref="DataGridColumn"/>s (always present) and
/// <see cref="AdaptiveColumn"/>s (resolved per width). <c>Columns</c> must stay
/// empty: columns declared there throw at load, and columns added later are
/// removed by the next sync since they are not part of the resolved layout.
/// <code>
/// &lt;atoms:AdaptiveDataGrid ReferenceWidth="{Binding ...}"&gt;
///   &lt;StaticResource ResourceKey="CheckboxColumn" /&gt;
///
///   &lt;atoms:AdaptiveColumn Breakpoints="0;960"&gt;
///     &lt;atoms:AdaptiveColumn.Compact&gt;...&lt;/atoms:AdaptiveColumn.Compact&gt;
///     &lt;atoms:AdaptiveColumn.Medium&gt;...&lt;/atoms:AdaptiveColumn.Medium&gt;
///   &lt;/atoms:AdaptiveColumn&gt;
///
///   &lt;StaticResource ResourceKey="ActionsColumn" /&gt;
/// &lt;/atoms:AdaptiveDataGrid&gt;
/// </code>
/// Property elements such as <c>.Resources</c> must come before the first child.
/// </para>
/// <para>
/// Width source, like <see cref="AdaptiveContent"/>: set <see cref="ReferenceWidth"/>;
/// if left unset, the grid measures itself.
/// </para>
/// <para>
/// Sync is a diff against the desired list, not Clear + Add: columns not in the
/// new state are removed for real (never just collapsed), columns already in the
/// right place are not touched, and only the missing ones are inserted. Move is
/// deliberately not used: <c>ObservableCollection.Move</c> bypasses the
/// <c>InsertItem</c>/<c>RemoveItem</c> overrides that <c>DataGrid</c>'s column
/// collection relies on. A column instance may appear at most once in a resolved
/// state. Do not put a width-based <c>Visibility</c> on these columns: the layout
/// already decides whether a column exists.
/// </para>
/// </summary>
[ContentProperty(nameof(Layout))]
public sealed class AdaptiveDataGrid : DataGrid
{
    public static readonly DependencyProperty ReferenceWidthProperty =
        DependencyProperty.Register(
            nameof(ReferenceWidth),
            typeof(double),
            typeof(AdaptiveDataGrid),
            new PropertyMetadata(double.NaN, OnReferenceWidthChanged)
        );

    public AdaptiveDataGrid()
    {
        // Implicit styles (e.g. Wpf.Ui's DataGrid style) are keyed by exact type
        // and do not reach derived types, so point at the DataGrid one explicitly.
        SetResourceReference(StyleProperty, typeof(DataGrid));

        // Layout owns the columns: auto-generated ones would just be removed again.
        AutoGenerateColumns = false;

        Layout.CollectionChanged += (_, _) => Refresh();
        SizeChanged += OnSizeChanged;
    }

    /// <summary>
    /// The ordered columns: <see cref="DataGridColumn"/>
    /// or <see cref="AdaptiveColumn"/>.
    /// </summary>
    public AdaptiveColumns Layout { get; } = [];

    /// <summary>
    /// Width used to pick each <see cref="AdaptiveColumn"/>'s size class.
    /// <see cref="double.NaN"/> (default) = use own ActualWidth.
    /// </summary>
    public double ReferenceWidth
    {
        get => (double)GetValue(ReferenceWidthProperty);
        set => SetValue(ReferenceWidthProperty, value);
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        // Fail fast instead of letting Refresh() silently remove them.
        if (Columns.Count > 0)
        {
            throw new InvalidOperationException(
                "AdaptiveDataGrid builds its columns from Layout. Declare columns inside Layout, not in Columns."
            );
        }

        Refresh();
    }

    private static void OnReferenceWidthChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        ((AdaptiveDataGrid)d).Refresh();
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
        // Wait for XAML load to finish adding children (OnInitialized re-runs this).
        if (!IsInitialized)
        {
            return;
        }

        double width = double.IsNaN(ReferenceWidth)
            ? ActualWidth
            : ReferenceWidth;

        // Not measured yet: don't build columns for a width we don't know.
        if (width <= 0)
        {
            return;
        }

        List<DataGridColumn> desired = [];

        foreach (object item in Layout)
        {
            switch (item)
            {
                case DataGridColumn column:
                    desired.Add(column);
                    break;
                case AdaptiveColumn adaptive:
                    desired.AddRange(adaptive.Resolve(width));
                    break;
            }
        }

        if (Sync(Columns, desired))
        {
            DataGridStarColumnRefreshBehavior.RefreshStarColumns(this);
        }
    }

    /// <summary>
    /// Makes <paramref name="columns"/> equal
    /// to <paramref name="desired"/> touching as little as possible.
    /// </summary>
    private static bool Sync(
        ObservableCollection<DataGridColumn> columns,
        List<DataGridColumn> desired
    )
    {
        bool changed = false;
        HashSet<DataGridColumn> keep = [.. desired];

        // 1. Remove what the new state doesn't want.
        for (int i = columns.Count - 1; i >= 0; i--)
        {
            if (!keep.Contains(columns[i]))
            {
                columns.RemoveAt(i);
                changed = true;
            }
        }

        // 2. What remains is a subsequence of desired (layout order is fixed),
        //    so any mismatch at i means desired[i] is missing: insert it.
        for (int i = 0; i < desired.Count; i++)
        {
            if (i >= columns.Count || !ReferenceEquals(columns[i], desired[i]))
            {
                columns.Insert(i, desired[i]);
                changed = true;
            }
        }

        return changed;
    }
}
