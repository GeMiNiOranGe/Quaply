using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Quaply.Data.Querying.Base;
using Quaply.Ui.Interfaces;
using Wpf.Ui.Controls;

namespace Quaply.Ui.ViewModels.Base;

/// <summary>
/// Shared behaviour for every "Recovery" page: paging, search, filter, sort,
/// multi-select, preview panel, restore / purge / empty.
/// Derived classes only supply entity-specific bits (service calls, labels).
/// </summary>
/// <typeparam name="TEntity">
/// Domain entity, e.g. Profile.
/// </typeparam>
/// <typeparam name="TItem">
/// UI wrapper for one row, e.g. ProfileRecoveryItem.
/// </typeparam>
/// <typeparam name="TSortField">
/// Sort-field enum, e.g. DeletedProfileSortField.
/// </typeparam>
public abstract partial class RecoveryViewModel<TEntity, TItem, TSortField>
    : NavigableViewModel,
        INavigationAware
    where TItem : class, IRecoveryItem
    where TSortField : struct, Enum
{
    private static readonly IReadOnlyList<int> DefaultPageSizeOptions =
    [
        20,
        50,
        100,
    ];

    private readonly IDialogPresenter _dialogPresenter;

    // Prevents IsAllSelected's setter and per-item ToggleSelection
    // from re-triggering each other in a loop.
    private bool _isSyncingSelectAll;

    // Guard flag so that several query inputs can change at once
    // (constructor defaults, ResetFilters) but trigger ONE reload at most.
    private readonly bool _suppressReload;

    private int _loadRequestId;

    protected RecoveryViewModel(
        INavigator navigator,
        IDialogPresenter dialogPresenter,
        TSortField defaultSortField
    )
        : base(navigator)
    {
        _dialogPresenter = dialogPresenter;

        _suppressReload = true;
        SortField = defaultSortField;
        _suppressReload = false;
    }

    // -------------------------------------------------------------------------
    // Extension points (what a derived page must / may provide)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Lower-case plural noun for UI text, e.g. "profiles".
    /// </summary>
    protected abstract string EntityNamePlural { get; }

    protected abstract Task<PagedResult<TEntity>> FetchPageAsync(
        DeletedItemsQuery<TSortField> query
    );

    /// <summary>
    /// Total number of records ignoring search/filters.
    /// </summary>
    protected abstract Task<int> FetchTotalCountAsync();

    protected abstract TItem CreateItem(TEntity entity);

    protected abstract Task RestoreItemAsync(int id);

    protected abstract Task RestoreItemsAsync(IReadOnlyList<int> ids);

    protected abstract Task PurgeItemAsync(int id);

    protected abstract Task PurgeItemsAsync(IReadOnlyList<int> ids);

    protected abstract Task PurgeAllAsync();

    protected abstract Task NavigateBackAsync();

    // -------------------------------------------------------------------------
    // Paging
    // -------------------------------------------------------------------------

    public IReadOnlyList<int> PageSizeOptions => DefaultPageSizeOptions;

    [ObservableProperty]
    public partial int CurrentPage { get; set; } = 1;

    [ObservableProperty]
    public partial int TotalPages { get; set; } = 1;

    [ObservableProperty]
    public partial int PageSize { get; set; } = DefaultPageSizeOptions[0];

    [ObservableProperty]
    public partial int FilteredCount { get; set; }

    [ObservableProperty]
    public partial int TotalCount { get; set; }

    public string PageIndicator => $"Page {CurrentPage} of {TotalPages}";

    public string ResultSummary
    {
        get
        {
            if (Items.Count == 0)
            {
                return $"No {EntityNamePlural}";
            }

            int first = (CurrentPage - 1) * PageSize + 1;
            int last = first + Items.Count - 1;
            return $"Showing {first}-{last} of {FilteredCount}";
        }
    }

    [NotifyCanExecuteChangedFor(nameof(GoToPreviousPageCommand))]
    [ObservableProperty]
    public partial bool CanGoToPreviousPage { get; set; }

    [NotifyCanExecuteChangedFor(nameof(GoToNextPageCommand))]
    [ObservableProperty]
    public partial bool CanGoToNextPage { get; set; }

    // -------------------------------------------------------------------------
    // Items, loading, empty state, search, filter, sort
    // -------------------------------------------------------------------------

    public ObservableCollection<TItem> Items
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasVisibleItems));
        }
    } = [];

    [ObservableProperty]
    public partial TItem? SelectedItem { get; set; }

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial RelativeDateRange SelectedDeletedRange { get; set; } =
        RelativeDateRange.All;

    [ObservableProperty]
    public partial TSortField SortField { get; set; }

    [NotifyPropertyChangedFor(nameof(SortDirectionTooltip))]
    [ObservableProperty]
    public partial bool IsSortDescending { get; set; } = true;

    [ObservableProperty]
    public partial bool IsSearching { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    public bool IsFiltering =>
        !string.IsNullOrWhiteSpace(SearchText)
        || SelectedDeletedRange != RelativeDateRange.All;

    public bool HasAnyItems => TotalCount > 0;

    public bool HasVisibleItems => Items.Count > 0;

    public string SortDirectionTooltip =>
        IsSortDescending ? "Sort ascending" : "Sort descending";

    public string EmptyStateTitle =>
        HasAnyItems ? $"No matching {EntityNamePlural}" : "Trash is empty";

    public string EmptyStateMessage =>
        HasAnyItems
            ? "Try changing your search or filters."
            : $"Deleted {EntityNamePlural} will show up here.";

    // -------------------------------------------------------------------------
    // Selection
    // -------------------------------------------------------------------------

    [ObservableProperty]
    public partial bool? IsAllSelected { get; set; } = false;

    public int SelectedCount => Items.Count(i => i.IsSelected);

    public bool HasSelection => SelectedCount > 0;

    // -------------------------------------------------------------------------
    // Preview panel
    // -------------------------------------------------------------------------

    [ObservableProperty]
    public partial double PreviewPanelWidth { get; set; }

    [NotifyPropertyChangedFor(nameof(IsPreviewPanelOpen))]
    [NotifyPropertyChangedFor(nameof(PinTooltip))]
    [ObservableProperty]
    public partial bool IsPreviewPanelPinned { get; set; }

    [NotifyPropertyChangedFor(nameof(IsPreviewInlineVisible))]
    [NotifyPropertyChangedFor(nameof(IsPreviewOverlayVisible))]
    [NotifyPropertyChangedFor(nameof(IsPreviewPanelVisible))]
    [ObservableProperty]
    public partial bool IsPreviewPanelOpen { get; set; }

    // Pushed from `WidthBreakpointBehavior` (OneWayToSource).
    [NotifyPropertyChangedFor(nameof(IsPreviewInlineVisible))]
    [NotifyPropertyChangedFor(nameof(IsPreviewOverlayVisible))]
    [NotifyPropertyChangedFor(nameof(IsPreviewPanelVisible))]
    [ObservableProperty]
    public partial bool IsPreviewOverlayMode { get; set; }

    [NotifyPropertyChangedFor(nameof(IsPreviewOverlayVisible))]
    [NotifyPropertyChangedFor(nameof(IsPreviewPanelVisible))]
    [ObservableProperty]
    public partial TItem? PreviewedItem { get; set; }

    public string PinTooltip =>
        IsPreviewPanelPinned ? "Unpin panel" : "Keep panel open";

    // The panel occupies a separate column next to the card list.
    public bool IsPreviewInlineVisible =>
        IsPreviewPanelOpen && !IsPreviewOverlayMode;

    // A panel floats above the list. It appears only when there is an item
    // to view; an empty-state overlay would merely obscure the list.
    public bool IsPreviewOverlayVisible =>
        IsPreviewPanelOpen && IsPreviewOverlayMode && PreviewedItem is not null;

    public bool IsPreviewPanelVisible =>
        IsPreviewInlineVisible || IsPreviewOverlayVisible;

    // -------------------------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------------------------

    public async Task OnNavigatedToAsync()
    {
        await LoadAsync();
    }

    // -------------------------------------------------------------------------
    // Property-changed hooks
    // -------------------------------------------------------------------------

    partial void OnPageSizeChanged(int value)
    {
        ReloadFromFirstPage();
    }

    partial void OnSearchTextChanged(string value)
    {
        IsSearching = true;
        ReloadFromFirstPage(debounce: true);
    }

    partial void OnSelectedDeletedRangeChanged(RelativeDateRange value)
    {
        ReloadFromFirstPage();
    }

    partial void OnSortFieldChanged(TSortField value)
    {
        ReloadFromFirstPage();
    }

    partial void OnIsSortDescendingChanged(bool value)
    {
        ReloadFromFirstPage();
    }

    partial void OnIsAllSelectedChanged(bool? value)
    {
        // Skip when we're the ones setting this value programmatically
        // (see UpdateSelectAllState), or when it's the indeterminate state,
        // which should only ever be computed, never set by the user directly.
        if (_isSyncingSelectAll || value is null)
        {
            return;
        }

        foreach (TItem item in Items)
        {
            item.IsSelected = value.Value;
        }

        RaiseSelectionChanged();
    }

    partial void OnIsPreviewPanelPinnedChanged(bool value)
    {
        // "Unpinned + nothing to show = closed."
        if (!value && PreviewedItem is null)
        {
            IsPreviewPanelOpen = false;
        }
    }

    partial void OnIsPreviewOverlayModeChanged(bool value)
    {
        // Returning to inline mode: a pinned panel must reappear.
        if (!value && IsPreviewPanelPinned)
        {
            IsPreviewPanelOpen = true;
        }
    }

    // -------------------------------------------------------------------------
    // Commands: navigation
    // -------------------------------------------------------------------------

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await NavigateBackAsync();
    }

    // -------------------------------------------------------------------------
    // Commands: search / filter / sort / paging
    // -------------------------------------------------------------------------

    [RelayCommand]
    private void SetDeletedRange(RelativeDateRange range)
    {
        SelectedDeletedRange = range;
    }

    [RelayCommand]
    private void SetSortField(TSortField field)
    {
        SortField = field;
    }

    [RelayCommand]
    private void ToggleSortDirection()
    {
        IsSortDescending = !IsSortDescending;
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadAsync();
    }

    [RelayCommand(CanExecute = nameof(CanGoToPreviousPage))]
    private async Task GoToPreviousPageAsync()
    {
        if (IsLoading)
        {
            return;
        }

        CurrentPage--;
        await LoadAsync();
    }

    [RelayCommand(CanExecute = nameof(CanGoToNextPage))]
    private async Task GoToNextPageAsync()
    {
        if (IsLoading)
        {
            return;
        }

        CurrentPage++;
        await LoadAsync();
    }

    [RelayCommand]
    private void TogglePreviewPanelPin()
    {
        IsPreviewPanelPinned = !IsPreviewPanelPinned;
    }

    [RelayCommand]
    private void ClosePreviewPanel()
    {
        IsPreviewPanelOpen = false;

        // Pinning only matters in inline mode. Temporarily closing the
        // overlay must not clear the user's pin choice.
        if (!IsPreviewOverlayMode)
        {
            IsPreviewPanelPinned = false;
        }
    }

    [RelayCommand]
    private void ViewDetails(TItem? item)
    {
        if (item is null)
        {
            return;
        }

        PreviewedItem = item;
        IsPreviewPanelOpen = true;
    }

    // Called by the checkbox column's Checked/Unchecked so the bulk toolbar
    // and "N selected" count stay in sync.
    [RelayCommand]
    private void ToggleSelection()
    {
        RaiseSelectionChanged();
        UpdateSelectAllState();
    }

    [RelayCommand]
    private void ClearSelection()
    {
        foreach (TItem item in Items)
        {
            item.IsSelected = false;
        }

        RaiseSelectionChanged();
        UpdateSelectAllState();
    }

    [RelayCommand]
    private async Task RestoreAsync(TItem? item)
    {
        if (item is null)
        {
            return;
        }

        await RestoreItemAsync(item.Id);
        await ReloadAfterRemovalAsync([item]);
    }

    [RelayCommand]
    private async Task PurgeAsync(TItem? item)
    {
        if (item is null)
        {
            return;
        }

        if (!await ConfirmPurgeAsync("Delete permanently?", [item]))
        {
            return;
        }

        await PurgeItemAsync(item.Id);
        await ReloadAfterRemovalAsync([item]);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task RestoreSelectedAsync()
    {
        List<TItem> selected = GetSelectedItems();

        if (selected.Count == 0)
        {
            return;
        }

        await RestoreItemsAsync([.. selected.Select(i => i.Id)]);
        await ReloadAfterRemovalAsync(selected);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task PurgeSelectedAsync()
    {
        List<TItem> selected = GetSelectedItems();

        if (selected.Count == 0)
        {
            return;
        }

        if (
            !await ConfirmPurgeAsync(
                $"Delete {selected.Count} items permanently?",
                selected
            )
        )
        {
            return;
        }

        await PurgeItemsAsync([.. selected.Select(i => i.Id)]);
        await ReloadAfterRemovalAsync(selected);
    }

    [RelayCommand(CanExecute = nameof(HasAnyItems))]
    private async Task EmptyAsync()
    {
        bool confirmed = await _dialogPresenter.ShowConfirmationAsync(
            title: "Empty trash?",
            message: $"This will permanently delete all {TotalCount} {EntityNamePlural} in the trash, including any hidden by your search or filters. This action cannot be undone.",
            primaryButtonText: "Empty trash",
            closeButtonText: "Cancel",
            defaultButton: ContentDialogButton.Close
        );

        if (!confirmed)
        {
            return;
        }

        await PurgeAllAsync();

        ClearPreviewIfRemoved([.. Items]);
        await LoadAsync();
    }

    // -------------------------------------------------------------------------
    // Loading
    // -------------------------------------------------------------------------

    /// <summary>
    /// Reload the current page (keeps the current page number).
    /// </summary>
    protected async Task LoadAsync(bool debounce = false)
    {
        int requestId = ++_loadRequestId;

        if (debounce)
        {
            await Task.Delay(300);

            // The user typed another character while waiting
            // -> this request is obsolete; discard it.
            if (requestId != _loadRequestId)
            {
                return;
            }
        }

        IsSearching = false;
        IsLoading = true;

        try
        {
            DeletedItemsQuery<TSortField> query = new(
                SearchText: SearchText,
                DeletedRange: SelectedDeletedRange,
                Sorting: new SortOption<TSortField>(
                    Field: SortField,
                    Descending: IsSortDescending
                ),
                Paging: new PageOption(CurrentPage, PageSize)
            );

            PagedResult<TEntity> result = await FetchPageAsync(query);

            // Without filters, the filtered count equals the total deleted items.
            int totalCount = IsFiltering
                ? await FetchTotalCountAsync()
                : result.TotalCount;

            // A newer request may have started while the query was running
            // (even without debouncing).
            if (requestId != _loadRequestId)
            {
                return;
            }

            Items = new(result.Items.Select(CreateItem));

            CurrentPage = result.Page;
            TotalPages = result.TotalPages;
            FilteredCount = result.TotalCount;
            CanGoToPreviousPage = result.HasPreviousPage;
            CanGoToNextPage = result.HasNextPage;
            TotalCount = totalCount;

            UpdateSelectAllState();
            RaiseSelectionChanged();
            RaisePagingChanged();
        }
        finally
        {
            if (requestId == _loadRequestId)
            {
                IsLoading = false;
            }
        }
    }

    private void ReloadFromFirstPage(bool debounce = false)
    {
        if (_suppressReload)
        {
            return;
        }

        CurrentPage = 1;
        _ = LoadAsync(debounce);
    }

    private async Task ReloadAfterRemovalAsync(
        IReadOnlyCollection<TItem> removed
    )
    {
        ClearPreviewIfRemoved(removed);

        if (SelectedItem is not null && removed.Contains(SelectedItem))
        {
            SelectedItem = null;
        }

        await LoadAsync();
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private List<TItem> GetSelectedItems()
    {
        return [.. Items.Where(i => i.IsSelected)];
    }

    private Task<bool> ConfirmPurgeAsync(
        string title,
        IReadOnlyList<TItem> items
    )
    {
        return _dialogPresenter.ShowConfirmationAsync(
            title: title,
            message: BuildPurgeWarning(items),
            primaryButtonText: "Delete permanently",
            closeButtonText: "Cancel",
            defaultButton: ContentDialogButton.Close
        );
    }

    private string BuildPurgeWarning(IReadOnlyList<TItem> items)
    {
        string subject =
            items.Count == 1
                ? $"'{items[0].DisplayName}'"
                : $"{items.Count} {EntityNamePlural}";

        return $"This will permanently delete {subject}. This action cannot be undone.";
    }

    private void ClearPreviewIfRemoved(IReadOnlyCollection<TItem> removedItems)
    {
        if (PreviewedItem is null || !removedItems.Contains(PreviewedItem))
        {
            return;
        }

        PreviewedItem = null;

        // Pinned: keep the panel open and show its empty state instead of
        // making it vanish abruptly. Not pinned: close as usual.
        if (!IsPreviewPanelPinned)
        {
            IsPreviewPanelOpen = false;
        }
    }

    private void UpdateSelectAllState()
    {
        _isSyncingSelectAll = true;

        try
        {
            IsAllSelected = Items.Count switch
            {
                0 => false,
                _ when Items.All(i => i.IsSelected) => true,
                _ when Items.All(i => !i.IsSelected) => false,
                _ => null, // Indeterminate: only some selected
            };
        }
        finally
        {
            _isSyncingSelectAll = false;
        }
    }

    private void RaiseSelectionChanged()
    {
        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(HasSelection));

        RestoreSelectedCommand.NotifyCanExecuteChanged();
        PurgeSelectedCommand.NotifyCanExecuteChanged();
    }

    private void RaisePagingChanged()
    {
        OnPropertyChanged(nameof(ResultSummary));
        OnPropertyChanged(nameof(PageIndicator));
        OnPropertyChanged(nameof(IsFiltering));
        OnPropertyChanged(nameof(HasAnyItems));
        OnPropertyChanged(nameof(EmptyStateTitle));
        OnPropertyChanged(nameof(EmptyStateMessage));
        EmptyCommand.NotifyCanExecuteChanged();
    }
}
