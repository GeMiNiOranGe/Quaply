using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Quaply.Data.Querying.Base;
using Quaply.Ui.Interfaces;
using Quaply.Ui.Models;

namespace Quaply.Ui.ViewModels.Base;

/// <summary>
/// Base class for every "index" page: a paged, searchable, sortable list of
/// cards/rows with bulk selection, a preview panel and delete-to-trash.
///
/// Derived classes supply only what is entity-specific: the service calls,
/// the item wrapper, navigation targets, wording and (optionally) stats.
/// </summary>
/// <typeparam name="TEntity">
/// Data model returned by the service.
/// </typeparam>
/// <typeparam name="TItem">
/// UI wrapper around TEntity.
/// </typeparam>
/// <typeparam name="TSortField">
/// Sort-field enum of the entity.
/// </typeparam>
public abstract partial class IndexViewModel<TEntity, TItem, TSortField>
    : NavigableViewModel,
        INavigationAware
    where TEntity : class
    where TItem : class, IIndexItem
    where TSortField : struct, Enum
{
    private const int SearchDebounceMilliseconds = 300;

    private static readonly IReadOnlyList<int> DefaultPageSizeOptions =
    [
        20,
        50,
        100,
    ];

    private static readonly string DefaultSearchText = string.Empty;

    private static readonly RelativeDateRange DefaultUpdatedRange =
        RelativeDateRange.All;

    private readonly IDialogPresenter _dialogPresenter;

    // Prevents IsAllSelected's setter and per-item ToggleSelection
    // from re-triggering each other in a loop.
    private bool _isSyncingSelectAll;

    // Guard flag so that several query inputs can change at once
    // (constructor defaults, ResetFilters) but trigger ONE reload at most.
    private bool _suppressReload;

    private int _loadRequestId;

    protected IndexViewModel(
        INavigator navigator,
        IDialogPresenter dialogPresenter,
        TSortField defaultSortField,
        bool defaultSortDescending = true
    )
        : base(navigator)
    {
        _dialogPresenter = dialogPresenter;

        _suppressReload = true;
        SortField = defaultSortField;
        IsSortDescending = defaultSortDescending;
        _suppressReload = false;
    }

    // -------------------------------------------------------------------------
    // Extension points (what a derived page must / may provide)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Override when the page has filters beyond search + date.
    /// </summary>
    protected virtual bool HasExtraFilters => false;

    /// <summary>
    /// Singular, lowercase: "profile", "work experience".
    /// </summary>
    protected abstract string EntityName { get; }

    /// <summary>
    /// Lower-case plural noun for UI text, e.g. "profiles".
    /// </summary>
    protected abstract string EntityNamePlural { get; }

    protected abstract Task<PagedResult<TEntity>> FetchPageAsync(
        ItemsQuery<TSortField> query
    );

    /// <summary>
    /// Total number of records ignoring search/filters.
    /// </summary>
    protected abstract Task<int> FetchTotalCountAsync();

    protected abstract TItem CreateItem(TEntity entity);

    protected abstract string BuildDeleteWarning(IReadOnlyList<TItem> items);

    protected abstract Task DeleteItemAsync(int id);

    protected abstract Task DeleteItemsAsync(IReadOnlyList<int> ids);

    protected abstract Task NavigateBackAsync();

    protected abstract Task OpenRecoveryAsync();

    protected abstract Task OpenAddEditorAsync();

    protected abstract Task OpenEditEditorAsync(TItem item);

    protected abstract Task OpenDuplicateEditorAsync(TItem item);

    /// <summary>
    /// Stats are independent of search/filter/paging.
    /// </summary>
    protected virtual Task LoadStatsAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Reset the extra filters; reload is handled by the base.
    /// </summary>
    protected virtual void ResetExtraFilters() { }

    /// <summary>
    /// Override to add extra filters to the query.
    /// </summary>
    protected virtual ItemsQuery<TSortField> BuildQuery()
    {
        return new(
            SearchText: SearchText,
            UpdatedRange: SelectedUpdatedRange,
            Sorting: new SortOption<TSortField>(
                Field: SortField,
                Descending: IsSortDescending
            ),
            Paging: new PageOption(CurrentPage, PageSize)
        );
    }

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

    // Filled by the derived class in LoadStatsAsync.
    public ObservableCollection<StatItem> Stats { get; } = [];

    [ObservableProperty]
    public partial TItem? SelectedItem { get; set; }

    [NotifyPropertyChangedFor(nameof(IsFiltering))]
    [NotifyCanExecuteChangedFor(nameof(ResetFiltersCommand))]
    [ObservableProperty]
    public partial string SearchText { get; set; } = DefaultSearchText;

    [NotifyPropertyChangedFor(nameof(IsFiltering))]
    [NotifyCanExecuteChangedFor(nameof(ResetFiltersCommand))]
    [ObservableProperty]
    public partial RelativeDateRange SelectedUpdatedRange { get; set; } =
        DefaultUpdatedRange;

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
        || SelectedUpdatedRange != RelativeDateRange.All
        || HasExtraFilters;

    public bool HasAnyItems => TotalCount > 0;

    public bool HasVisibleItems => Items.Count > 0;

    public string SortDirectionTooltip =>
        IsSortDescending ? "Sort ascending" : "Sort descending";

    public string EmptyStateTitle =>
        HasAnyItems
            ? $"No matching {EntityNamePlural}"
            : $"No {EntityNamePlural} yet";

    public string EmptyStateMessage =>
        HasAnyItems
            ? "Try changing your search or filters."
            : $"Add your first {EntityName} to get started.";

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
        await LoadStatsAsync();
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
        if (_suppressReload)
        {
            return;
        }

        IsSearching = true;
        ReloadFromFirstPage(debounce: true);
    }

    partial void OnSelectedUpdatedRangeChanged(RelativeDateRange value)
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

    [RelayCommand]
    private async Task NavigateToRecoveryAsync()
    {
        await OpenRecoveryAsync();
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        await OpenAddEditorAsync();
    }

    [RelayCommand]
    private async Task EditAsync(TItem? item)
    {
        if (item is not null)
        {
            await OpenEditEditorAsync(item);
        }
    }

    [RelayCommand]
    private async Task DuplicateAsync(TItem? item)
    {
        if (item is not null)
        {
            await OpenDuplicateEditorAsync(item);
        }
    }

    // -------------------------------------------------------------------------
    // Commands: search / filter / sort / paging
    // -------------------------------------------------------------------------

    [RelayCommand]
    private void SetUpdatedRange(RelativeDateRange range)
    {
        SelectedUpdatedRange = range;
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
    private void ResetFilters()
    {
        _suppressReload = true;

        try
        {
            SearchText = DefaultSearchText;
            SelectedUpdatedRange = DefaultUpdatedRange;
            ResetExtraFilters();
        }
        finally
        {
            _suppressReload = false;
        }

        ReloadFromFirstPage();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadAsync();
        await LoadStatsAsync();
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
    private async Task DeleteAsync(TItem? item)
    {
        if (item is null)
        {
            return;
        }

        if (!await ConfirmPurgeAsync($"Move {EntityName} to trash?", [item]))
        {
            return;
        }

        await DeleteItemAsync(item.Id);
        await ReloadAfterRemovalAsync([item]);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DeleteSelectedAsync()
    {
        List<TItem> selected = GetSelectedItems();

        if (selected.Count == 0)
        {
            return;
        }

        if (
            !await ConfirmPurgeAsync(
                $"Move {selected.Count} {EntityNamePlural} to trash?",
                selected
            )
        )
        {
            return;
        }

        await DeleteItemsAsync([.. selected.Select(i => i.Id)]);
        await ReloadAfterRemovalAsync(selected);
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
            PagedResult<TEntity> result = await FetchPageAsync(BuildQuery());

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
    /// Derived pages call this whenever one of their extra filters changes.
    /// </summary>
    protected void RaiseFilterStateChanged()
    {
        OnPropertyChanged(nameof(IsFiltering));
        ResetFiltersCommand.NotifyCanExecuteChanged();
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

    private async Task ReloadAfterRemovalAsync(IEnumerable<TItem> removed)
    {
        ClearPreviewIfRemoved(removed);

        if (SelectedItem is not null && removed.Contains(SelectedItem))
        {
            SelectedItem = null;
        }

        await LoadAsync();
        await LoadStatsAsync();
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
            message: BuildDeleteWarning(items),
            primaryButtonText: "Move to trash",
            closeButtonText: "Cancel"
        );
    }

    private void ClearPreviewIfRemoved(IEnumerable<TItem> removedItems)
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

        DeleteSelectedCommand.NotifyCanExecuteChanged();
    }

    private void RaisePagingChanged()
    {
        OnPropertyChanged(nameof(ResultSummary));
        OnPropertyChanged(nameof(PageIndicator));
        OnPropertyChanged(nameof(HasAnyItems));
        OnPropertyChanged(nameof(EmptyStateTitle));
        OnPropertyChanged(nameof(EmptyStateMessage));
    }
}
