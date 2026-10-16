using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Quaply.Ui.Foundations;

namespace Quaply.Ui.Views.Atoms;

/// <summary>
/// One <i>logical</i> column of an <see cref="AdaptiveDataGrid"/>: it resolves to
/// one or several physical <see cref="DataGridColumn"/>s depending on the width.
/// The <see cref="DataGridColumn"/> counterpart of
/// <see cref="AdaptiveContent"/>: a <see cref="Breakpoints"/> plus
/// Compact / Medium / Expanded slots, same downward fallback
/// (Expanded -> Medium -> Compact).
/// <para>
/// A slot holds zero, one or several columns, because merging "Company" and
/// "Position" into one column changes the column count, not just a template.
/// </para>
/// <para>
/// This is not a <see cref="DataGridColumn"/> itself, so it can only be declared
/// inside an <see cref="AdaptiveDataGrid"/>, never in <c>DataGrid.Columns</c>.
/// A column that is the same in two slots (e.g. Duration in Medium and Expanded)
/// should be the <b>same instance</b> (<c>StaticResource</c>): the grid diffs by
/// instance, so a shared column is left untouched when the width moves between
/// those slots.
/// </para>
/// </summary>
public sealed class AdaptiveColumn : DependencyObject
{
    public static readonly DependencyProperty BreakpointsProperty =
        DependencyProperty.Register(
            nameof(Breakpoints),
            typeof(BreakpointSet),
            typeof(AdaptiveColumn),
            new PropertyMetadata(null)
        );

    /// <summary>
    /// Same type as <c>AdaptiveContent.Breakpoints</c>: <c>Breakpoints="0;800;1040"</c>.
    /// Defaults to <see cref="BreakpointSet.Default"/>.
    /// </summary>
    public BreakpointSet? Breakpoints
    {
        get => (BreakpointSet?)GetValue(BreakpointsProperty);
        set => SetValue(BreakpointsProperty, value);
    }

    public ObservableCollection<DataGridColumn> Compact { get; } = [];

    public ObservableCollection<DataGridColumn> Medium { get; } = [];

    public ObservableCollection<DataGridColumn> Expanded { get; } = [];

    /// <summary>
    /// The physical columns this logical column contributes
    /// at <paramref name="width"/>.
    /// </summary>
    internal ObservableCollection<DataGridColumn> Resolve(double width)
    {
        SizeClass sizeClass = (Breakpoints ?? BreakpointSet.Default).Resolve(
            width
        );

        return sizeClass switch
        {
            SizeClass.Expanded => Expanded.Count > 0 ? Expanded
            : Medium.Count > 0 ? Medium
            : Compact,
            SizeClass.Medium => Medium.Count > 0 ? Medium : Compact,
            _ => Compact,
        };
    }
}
