using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Quaply.Ui.Interfaces;

namespace Quaply.Ui.AttachedProperties;

/// <summary>
/// Keeps a DataGrid's checkbox column (ISelectableItem.IsSelected) in sync
/// with its native row selection (click / Ctrl+Click / Shift+Click), so the
/// built-in "selected row" style always matches the checked rows and vice
/// versa. Optionally wires up a "Select All" header checkbox too.
///
///   <DataGrid
///     x:Name="grid"
///     behaviors:DataGridMultiSelectBehavior.IsEnabled="True"
///     behaviors:DataGridMultiSelectBehavior.AfterSyncCommand="{Binding ToggleSelectionCommand}">
///
///   <CheckBox
///     behaviors:DataGridMultiSelectBehavior.SelectAllFor="{Binding ElementName=grid}"
///     IsChecked="{Binding IsAllSelected}"
///     IsThreeState="True" />
///
/// Items in the DataGrid's ItemsSource must implement ISelectableItem.
/// </summary>
public static class DataGridMultiSelectBehavior
{
    private enum SelectionGesture
    {
        None,
        Ctrl,
        Shift,
    }

    // Per-grid state, tracked via ConditionalWeakTable instead of a plain
    // Dictionary - a Dictionary keyed by DataGrid would hold a strong
    // reference and leak the grid (and its page) across navigations.
    private sealed class GridState
    {
        public SelectionGesture Gesture;
        public bool IsCheckBoxOriginatedClick;
        public bool IsSyncingFromHeader;
    }

    private static readonly ConditionalWeakTable<DataGrid, GridState> _states =
    [];

    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(DataGridMultiSelectBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged)
        );

    // Invoked after every sync so the owning ViewModel can refresh derived
    // state (SelectedCount, IsAllSelected, etc.). A command instead of a
    // hardcoded method name keeps this behavior ViewModel-agnostic.
    public static readonly DependencyProperty AfterSyncCommandProperty =
        DependencyProperty.RegisterAttached(
            "AfterSyncCommand",
            typeof(ICommand),
            typeof(DataGridMultiSelectBehavior),
            new PropertyMetadata(null)
        );

    // Select all for header checkbox
    public static readonly DependencyProperty SelectAllForProperty =
        DependencyProperty.RegisterAttached(
            "SelectAllFor",
            typeof(DataGrid),
            typeof(DataGridMultiSelectBehavior),
            new PropertyMetadata(null, OnSelectAllForChanged)
        );

    // --- IsEnabled ---

    public static void SetIsEnabled(DependencyObject element, bool value)
    {
        element.SetValue(IsEnabledProperty, value);
    }

    public static bool GetIsEnabled(DependencyObject element)
    {
        return (bool)element.GetValue(IsEnabledProperty);
    }

    private static void OnIsEnabledChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        if (d is not DataGrid grid)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            grid.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            grid.SelectionChanged += OnSelectionChanged;
        }
        else
        {
            grid.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
            grid.SelectionChanged -= OnSelectionChanged;
            _states.Remove(grid);
        }
    }

    // --- AfterSyncCommand ---

    public static void SetAfterSyncCommand(
        DependencyObject element,
        ICommand value
    )
    {
        element.SetValue(AfterSyncCommandProperty, value);
    }

    public static ICommand GetAfterSyncCommand(DependencyObject element)
    {
        return (ICommand)element.GetValue(AfterSyncCommandProperty);
    }

    // --- SelectAllFor ---

    public static void SetSelectAllFor(DependencyObject element, DataGrid value)
    {
        element.SetValue(SelectAllForProperty, value);
    }

    public static DataGrid GetSelectAllFor(DependencyObject element)
    {
        return (DataGrid)element.GetValue(SelectAllForProperty);
    }

    private static void OnSelectAllForChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        if (d is not CheckBox checkBox)
        {
            return;
        }

        checkBox.Click -= OnSelectAllCheckBoxClick;

        if (e.NewValue is DataGrid)
        {
            checkBox.Click += OnSelectAllCheckBoxClick;
        }
    }

    private static void OnSelectAllCheckBoxClick(
        object sender,
        RoutedEventArgs e
    )
    {
        CheckBox checkBox = (CheckBox)sender;
        DataGrid grid = GetSelectAllFor(checkBox);

        if (grid is null)
        {
            return;
        }

        // Indeterminate is reserved for ViewModel partial-selection state.
        // If a user click produces null, reset it to false.
        checkBox.IsChecked ??= false;

        // Mirror the header checkbox onto the grid's own selection too, so
        // rows stay in sync whichever trigger (native or checkbox) fires.
        GridState state = _states.GetOrCreateValue(grid);
        state.IsSyncingFromHeader = true;

        if (checkBox.IsChecked == true)
        {
            grid.SelectAll();
        }
        else
        {
            grid.UnselectAll();
        }

        state.IsSyncingFromHeader = false;
    }

    // --- Core selection logic (driven by IsEnabled) ---

    private static void OnPreviewMouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e
    )
    {
        DataGrid grid = (DataGrid)sender;
        GridState state = _states.GetOrCreateValue(grid);

        bool ctrl = (Keyboard.Modifiers & ModifierKeys.Control) != 0;
        bool shift = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
        bool isCheckBoxClick = IsWithinCheckBox(
            e.OriginalSource as DependencyObject
        );

        state.Gesture =
            shift ? SelectionGesture.Shift
            : ctrl ? SelectionGesture.Ctrl
            : SelectionGesture.None;

        state.IsCheckBoxOriginatedClick =
            isCheckBoxClick && state.Gesture == SelectionGesture.None;

        // Clicking a checkbox while Ctrl/Shift is held would otherwise let
        // the checkbox toggle itself AND the DataGrid's native multi-select
        // both fire - two sources fighting over the same value. Marking the
        // event handled stops the checkbox's own click-to-toggle behavior;
        // the DataGrid still processes selection regardless, since it
        // always handles selection ahead of child controls.
        if (isCheckBoxClick && state.Gesture != SelectionGesture.None)
        {
            e.Handled = true;
        }
    }

    private static void OnSelectionChanged(
        object sender,
        SelectionChangedEventArgs e
    )
    {
        DataGrid grid = (DataGrid)sender;
        GridState state = _states.GetOrCreateValue(grid);

        if (state.IsSyncingFromHeader || state.IsCheckBoxOriginatedClick)
        {
            return;
        }

        switch (state.Gesture)
        {
            case SelectionGesture.None:
            {
                // Plain click: start fresh, clear every checkbox. The
                // clicked row still shows selected via the DataGrid's own
                // native selection, just without being checked.
                foreach (
                    ISelectableItem item in grid.Items.OfType<ISelectableItem>()
                )
                {
                    item.IsSelected = false;
                }
                break;
            }
            case SelectionGesture.Ctrl:
            {
                // Ctrl+Click only toggles the row(s) that just changed,
                // leaving every other row's checkbox untouched.
                foreach (
                    ISelectableItem item in e.RemovedItems.OfType<ISelectableItem>()
                )
                {
                    item.IsSelected = false;
                }

                foreach (
                    ISelectableItem item in e.AddedItems.OfType<ISelectableItem>()
                )
                {
                    item.IsSelected = true;
                }
                break;
            }
            case SelectionGesture.Shift:
            {
                // Shift+Click redefines the whole range from the anchor, so
                // resync every checkbox against the grid's current
                // selection rather than just the newly added items.
                foreach (
                    ISelectableItem item in grid.Items.OfType<ISelectableItem>()
                )
                {
                    item.IsSelected = grid.SelectedItems.Contains(item);
                }
                break;
            }
        }

        GetAfterSyncCommand(grid)?.Execute(null);
    }

    // Walks up the visual tree from the click point to check whether it
    // landed on a CheckBox. Stops at DataGridRow since a checkbox can only
    // ever live within its own row's bounds.
    private static bool IsWithinCheckBox(DependencyObject? source)
    {
        while (source is not null)
        {
            if (source is CheckBox)
            {
                return true;
            }

            if (source is DataGridRow)
            {
                return false;
            }

            source = VisualTreeHelper.GetParent(source);
        }

        return false;
    }
}
