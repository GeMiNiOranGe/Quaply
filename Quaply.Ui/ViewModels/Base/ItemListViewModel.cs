using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Quaply.Data.Querying.Base;
using Quaply.Ui.Interfaces;

namespace Quaply.Ui.ViewModels.Base;

/// <summary>
/// Shared engine of every list page (Index and Recovery): paging, search
/// (debounced), date filter, extra filters, sort, multi-select, preview panel
/// and the load / reload pipeline.
///
/// <para>
/// This class knows NOTHING about delete, restore or purge. Those actions
/// live in <c>IndexViewModel</c> / <c>RecoveryViewModel</c>.
/// </para>
/// </summary>
/// <typeparam name="TEntity">
/// Domain entity, e.g. Profile.
/// </typeparam>
/// <typeparam name="TItem">
/// UI wrapper for one row.
/// </typeparam>
/// <typeparam name="TSortField">
/// Sort-field enum of the entity.
/// </typeparam>
/// <typeparam name="TQuery">
/// Query record sent to the service, e.g. ItemsQuery or DeletedItemsQuery.
/// </typeparam>
public abstract partial class ItemListViewModel<
    TEntity,
    TItem,
    TSortField,
    TQuery
> : NavigableViewModel, INavigationAware
    where TEntity : class
    where TItem : class, ISelectableItem
    where TSortField : struct, Enum
    where TQuery : class
{
    private sealed class ReloadSuppressionScope(
        ItemListViewModel<TEntity, TItem, TSortField, TQuery> owner
    ) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            owner._suppressReloadDepth--;
        }
    }

    private const int SearchDebounceMilliseconds = 300;

    private static readonly IReadOnlyList<int> DefaultPageSizeOptions =
    [
        20,
        50,
        100,
    ];

    private static readonly string DefaultSearchText = string.Empty;

    private static readonly RelativeDateRange DefaultDateRange =
        RelativeDateRange.All;

    // Guard so that several query inputs can change at once
    // (constructor defaults, ResetFilters) but trigger ONE reload at most.
    // A depth counter (not a bool) so scopes can be nested safely.
    private int _suppressReloadDepth;

    // Prevents IsAllSelected's setter and per-item ToggleSelection
    // from re-triggering each other in a loop.
    private bool _isSyncingSelectAll;

    private int _loadRequestId;

    protected ItemListViewModel(
        INavigator navigator,
        IDialogPresenter dialogPresenter,
        TSortField defaultSortField,
        bool defaultSortDescending = true
    )
        : base(navigator)
    {
        DialogPresenter = dialogPresenter;

        using (SuppressReload())
        {
            SortField = defaultSortField;
            IsSortDescending = defaultSortDescending;
        }
    }

    protected IDialogPresenter DialogPresenter { get; }

    private bool IsReloadSuppressed => _suppressReloadDepth > 0;

    /// <summary>
    /// While the returned scope is alive, filter/sort changes do not trigger
    /// a reload. Dispose it (use `using`) to lift the suppression, then call
    /// <see cref="ReloadFromFirstPage"/> yourself if a reload is needed.
    /// </summary>
    protected IDisposable SuppressReload()
    {
        _suppressReloadDepth++;
        return new ReloadSuppressionScope(this);
    }

    // -------------------------------------------------------------------------
    // Extension points
    // -------------------------------------------------------------------------

    // --- Must provide (abstract) ---------------------------------------------

    /// <summary>
    /// Lower-case plural noun for UI text, e.g. "profiles".
    /// </summary>
    protected abstract string EntityNamePlural { get; }

    /// <summary>
    /// Empty-state title when the collection itself has no items at all.
    /// </summary>
    protected abstract string EmptyListTitle { get; }

    /// <summary>
    /// Empty-state message when the collection itself has no items at all.
    /// </summary>
    protected abstract string EmptyListMessage { get; }

    /// <summary>
    /// Override to add extra filters to the query.
    /// </summary>
    protected abstract TQuery BuildQuery();

    protected abstract Task<PagedResult<TEntity>> FetchPageAsync(TQuery query);

    /// <summary>
    /// Total number of records ignoring search/filters.
    /// </summary>
    protected abstract Task<int> FetchTotalCountAsync();

    protected abstract TItem CreateItem(TEntity entity);

    protected abstract Task NavigateBackAsync();

    /// <summary>
    /// Notify the CanExecute of the commands that depend on HasSelection
    /// (e.g. DeleteSelectedCommand, RestoreSelectedCommand).
    /// </summary>
    protected abstract void NotifySelectionCommandsCanExecuteChanged();

    // --- May provide (virtual) -----------------------------------------------

    /// <summary>
    /// Called after every successful load, once paging state is updated.
    /// Use it to refresh commands that depend on HasAnyItems.
    /// </summary>
    protected virtual void NotifyPagingCommandsCanExecuteChanged() { }

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

    [NotifyPropertyChangedFor(nameof(IsFiltering))]
    [NotifyCanExecuteChangedFor(nameof(ResetFiltersCommand))]
    [ObservableProperty]
    public partial string SearchText { get; set; } = DefaultSearchText;

    // Index  = "last updated" range, Recovery = "deleted at" range.
    [NotifyPropertyChangedFor(nameof(IsFiltering))]
    [NotifyCanExecuteChangedFor(nameof(ResetFiltersCommand))]
    [ObservableProperty]
    public partial RelativeDateRange SelectedDateRange { get; set; } =
        DefaultDateRange;

    [ObservableProperty]
    public partial TSortField SortField { get; set; }

    [NotifyPropertyChangedFor(nameof(SortDirectionTooltip))]
    [ObservableProperty]
    public partial bool IsSortDescending { get; set; } = true;

    [ObservableProperty]
    public partial bool IsSearching { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    public virtual bool IsFiltering =>
        !string.IsNullOrWhiteSpace(SearchText)
        || SelectedDateRange != RelativeDateRange.All;

    public bool HasAnyItems => TotalCount > 0;

    public bool HasVisibleItems => Items.Count > 0;

    public string SortDirectionTooltip =>
        IsSortDescending ? "Sort ascending" : "Sort descending";

    public string EmptyStateTitle =>
        HasAnyItems ? $"No matching {EntityNamePlural}" : EmptyListTitle;

    public string EmptyStateMessage =>
        HasAnyItems ? "Try changing your search or filters." : EmptyListMessage;

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

    public virtual async Task OnNavigatedToAsync()
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
        if (IsReloadSuppressed)
        {
            return;
        }

        IsSearching = true;
        ReloadFromFirstPage(debounce: true);
    }

    partial void OnSelectedDateRangeChanged(RelativeDateRange value)
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
    // Commands
    // -------------------------------------------------------------------------

    // --- Navigation ----------------------------------------------------------

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await NavigateBackAsync();
    }

    // --- Search / filter / sort / paging -------------------------------------

    [RelayCommand]
    private void SetDateRange(RelativeDateRange range)
    {
        SelectedDateRange = range;
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

    // Used by the "Clear filters" button of the "no matching ..." empty state:
    // the user is stuck on an empty list, so give them a one-click way out.
    [RelayCommand(CanExecute = nameof(IsFiltering))]
    protected virtual void ResetFilters()
    {
        using (SuppressReload())
        {
            SearchText = DefaultSearchText;
            SelectedDateRange = DefaultDateRange;
        }

        ReloadFromFirstPage();
    }

    [RelayCommand]
    protected virtual async Task RefreshAsync()
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

    // --- Preview panel -------------------------------------------------------

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

    // --- Selection -----------------------------------------------------------

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
            await Task.Delay(SearchDebounceMilliseconds);

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
            TQuery query = BuildQuery();

            PagedResult<TEntity> result = await FetchPageAsync(query);

            // Without filters, the filtered count equals the total count.
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

    /// <summary>
    /// Call after items were deleted / restored / purged.
    /// </summary>
    protected virtual async Task ReloadAfterRemovalAsync(
        IReadOnlyList<TItem> removed
    )
    {
        ClearPreviewIfRemoved(removed);

        if (SelectedItem is not null && removed.Contains(SelectedItem))
        {
            SelectedItem = null;
        }

        await LoadAsync();
    }

    protected List<TItem> GetSelectedItems()
    {
        return [.. Items.Where(i => i.IsSelected)];
    }

    protected void ClearPreviewIfRemoved(IReadOnlyList<TItem> removedItems)
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

    /// <summary>
    /// Go back to page 1 and reload. No-op while a
    /// <see cref="SuppressReload"/> scope is alive.
    /// Derived pages call this when a filter they own changes.
    /// </summary>
    protected void ReloadFromFirstPage(bool debounce = false)
    {
        if (IsReloadSuppressed)
        {
            return;
        }

        CurrentPage = 1;
        _ = LoadAsync(debounce);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

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

        NotifySelectionCommandsCanExecuteChanged();
    }

    private void RaisePagingChanged()
    {
        OnPropertyChanged(nameof(ResultSummary));
        OnPropertyChanged(nameof(PageIndicator));
        OnPropertyChanged(nameof(HasAnyItems));
        OnPropertyChanged(nameof(EmptyStateTitle));
        OnPropertyChanged(nameof(EmptyStateMessage));

        NotifyPagingCommandsCanExecuteChanged();
    }
}
