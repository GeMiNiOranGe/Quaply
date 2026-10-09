using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Quaply.Data.Models;
using Quaply.Data.Querying.Base;
using Quaply.Data.Querying.WorkExperiences;
using Quaply.Service.Interfaces;
using Quaply.Ui.Interfaces;
using Quaply.Ui.Models;
using Quaply.Ui.ViewModels.Base;

namespace Quaply.Ui.ViewModels;

public partial class WorkExperienceTrashViewModel(
    INavigator navigator,
    IWorkExperienceService service,
    IDialogPresenter dialogPresenter
) : NavigableViewModel(navigator), INavigationAware
{
    private readonly IWorkExperienceService _service = service;
    private readonly IDialogPresenter _dialogPresenter = dialogPresenter;

    // Guard flag to prevent IsAllSelected's setter and per-item ToggleSelection
    // from re-triggering each other in a loop.
    private bool _isSyncingSelectAll;

    private int _loadRequestId;

    public static IReadOnlyList<int> PageSizeOptions { get; } = [20, 50, 100];

    [NotifyCanExecuteChangedFor(nameof(GoToPreviousPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(GoToNextPageCommand))]
    [ObservableProperty]
    public partial int CurrentPage { get; set; } = 1;

    [NotifyCanExecuteChangedFor(nameof(GoToNextPageCommand))]
    [ObservableProperty]
    public partial int TotalPages { get; set; } = 1;

    [ObservableProperty]
    public partial int PageSize { get; set; } = PageSizeOptions[0];

    [ObservableProperty]
    public partial int FilteredCount { get; set; }

    [ObservableProperty]
    public partial int TotalDeletedCount { get; set; }

    public bool IsFiltering =>
        !string.IsNullOrWhiteSpace(SearchText)
        || SelectedDeletedRange != RelativeDateRange.All;

    public bool HasTrashItems => TotalDeletedCount > 0;

    public string PageIndicator => $"Page {CurrentPage} of {TotalPages}";

    public string ResultSummary
    {
        get
        {
            if (DeletedItems.Count == 0)
            {
                return "No work experiences";
            }

            int first = (CurrentPage - 1) * PageSize + 1;
            int last = first + DeletedItems.Count - 1;
            return $"Showing {first}-{last} of {FilteredCount}";
        }
    }

    public string EmptyStateTitle =>
        HasTrashItems ? "No matching work experiences" : "Trash is empty";

    public string EmptyStateMessage =>
        HasTrashItems
            ? "Try changing your search or filters."
            : "Deleted work experiences will show up here.";

    private bool CanGoToPreviousPage => CurrentPage > 1;
    private bool CanGoToNextPage => CurrentPage < TotalPages;

    [ObservableProperty]
    public partial bool IsSearching { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

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
    public partial bool IsPreviewPanelOpen { get; set; } = false;

    // Pushed from `WidthBreakpointBehavior` (OneWayToSource).
    [NotifyPropertyChangedFor(nameof(IsPreviewInlineVisible))]
    [NotifyPropertyChangedFor(nameof(IsPreviewOverlayVisible))]
    [NotifyPropertyChangedFor(nameof(IsPreviewPanelVisible))]
    [ObservableProperty]
    public partial bool IsPreviewOverlayMode { get; set; }

    public string PinTooltip =>
        IsPreviewPanelPinned ? "Unpin panel" : "Keep panel open";

    public string SortDirectionTooltip =>
        IsSortDescending ? "Sort ascending" : "Sort descending";

    // The panel occupies a separate column next to the card list.
    public bool IsPreviewInlineVisible =>
        IsPreviewPanelOpen && !IsPreviewOverlayMode;

    // A panel floats above the list. It appears only when there is an item
    // to view; an empty state overlay (e.g., "Select a row...")
    // would merely obscure the list unnecessarily.
    public bool IsPreviewOverlayVisible =>
        IsPreviewPanelOpen && IsPreviewOverlayMode && PreviewedItem is not null;

    public bool IsPreviewPanelVisible =>
        IsPreviewInlineVisible || IsPreviewOverlayVisible;

    public ObservableCollection<DeletedWorkExperienceItem> DeletedItems
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasDeletedItems));
        }
    } = [];

    [ObservableProperty]
    public partial DeletedWorkExperienceItem? SelectedItem { get; set; }

    [NotifyPropertyChangedFor(nameof(IsPreviewOverlayVisible))]
    [NotifyPropertyChangedFor(nameof(IsPreviewPanelVisible))]
    [ObservableProperty]
    public partial DeletedWorkExperienceItem? PreviewedItem { get; set; }

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    public bool HasDeletedItems => DeletedItems.Count > 0;

    public int SelectedCount => DeletedItems.Count(i => i.IsSelected);

    public bool HasSelection => SelectedCount > 0;

    [ObservableProperty]
    public partial RelativeDateRange SelectedDeletedRange { get; set; } =
        RelativeDateRange.All;

    [ObservableProperty]
    public partial bool? IsAllSelected { get; set; } = false;

    [ObservableProperty]
    public partial DeletedWorkExperienceSortField SortField { get; set; } =
        DeletedWorkExperienceSortField.DeletedAt;

    [NotifyPropertyChangedFor(nameof(SortDirectionTooltip))]
    [ObservableProperty]
    public partial bool IsSortDescending { get; set; } = true;

    public async Task OnNavigatedToAsync()
    {
        await LoadDeletedWorkExperiencesAsync();
    }

    partial void OnPageSizeChanged(int value)
    {
        ReloadFromFirstPage();
    }

    partial void OnSearchTextChanged(string value)
    {
        IsSearching = true;
        ReloadFromFirstPage(debounce: true);
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

        foreach (DeletedWorkExperienceItem item in DeletedItems)
        {
            item.IsSelected = value.Value;
        }

        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(HasSelection));

        RestoreSelectedCommand.NotifyCanExecuteChanged();
        PurgeSelectedCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsPreviewPanelPinnedChanged(bool value)
    {
        // Unpin while the panel is empty: there's no reason to keep it open,
        // following the rule "unpinned + nothing to show = closed."
        if (!value && PreviewedItem is null)
        {
            IsPreviewPanelOpen = false;
        }
    }

    partial void OnIsPreviewOverlayModeChanged(bool value)
    {
        // Returning to inline mode: the pinned panel must reappear
        // (it might have been closed while in overlay mode).
        if (!value && IsPreviewPanelPinned)
        {
            IsPreviewPanelOpen = true;
        }
    }

    partial void OnSelectedDeletedRangeChanged(RelativeDateRange value)
    {
        ReloadFromFirstPage();
    }

    partial void OnSortFieldChanged(DeletedWorkExperienceSortField value)
    {
        ReloadFromFirstPage();
    }

    partial void OnIsSortDescendingChanged(bool value)
    {
        ReloadFromFirstPage();
    }

    [RelayCommand]
    private async Task BackToWorkExperiencesAsync()
    {
        await Navigator.NavigateToAsync<WorkExperienceViewModel>();
    }

    [RelayCommand]
    private void SetDeletedRange(RelativeDateRange range)
    {
        SelectedDeletedRange = range;
    }

    [RelayCommand]
    private void SetSortField(DeletedWorkExperienceSortField field)
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
        await LoadDeletedWorkExperiencesAsync();
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

        // Pinning is only relevant in inline mode. Temporarily closing
        // the overlay should not clear the user's pinning selection.
        if (!IsPreviewOverlayMode)
        {
            IsPreviewPanelPinned = false;
        }
    }

    [RelayCommand]
    private void ViewDetailWorkExperience(DeletedWorkExperienceItem? item)
    {
        if (item is null)
        {
            return;
        }

        PreviewedItem = item;
        IsPreviewPanelOpen = true;
    }

    [RelayCommand]
    private async Task RestoreWorkExperienceAsync(
        DeletedWorkExperienceItem? item
    )
    {
        if (item is null)
        {
            return;
        }

        await _service.RestoreWorkExperienceAsync(item.WorkExperience.Id);
        await ReloadAfterRemovalAsync([item]);
    }

    [RelayCommand]
    private async Task PurgeWorkExperienceAsync(DeletedWorkExperienceItem? item)
    {
        if (item is null)
        {
            return;
        }

        bool confirmed = await _dialogPresenter.ShowDangerConfirmationAsync(
            title: "Delete permanently?",
            message: BuildPurgeWarning([item]),
            primaryButtonText: "Delete permanently",
            closeButtonText: "Cancel"
        );

        if (!confirmed)
        {
            return;
        }

        await _service.PurgeWorkExperienceAsync(item.WorkExperience.Id);
        await ReloadAfterRemovalAsync([item]);
    }

    // Called by the checkbox column's Checked/Unchecked so the bulk toolbar
    // and "N selected" count stay in sync.
    [RelayCommand]
    private void ToggleSelection()
    {
        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(HasSelection));

        RestoreSelectedCommand.NotifyCanExecuteChanged();
        PurgeSelectedCommand.NotifyCanExecuteChanged();

        UpdateSelectAllState();
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task RestoreSelectedAsync()
    {
        List<DeletedWorkExperienceItem> selected =
        [
            .. DeletedItems.Where(i => i.IsSelected),
        ];

        if (selected.Count == 0)
        {
            return;
        }

        await _service.RestoreRangeWorkExperiencesAsync(
            selected.Select(i => i.WorkExperience.Id)
        );

        await ReloadAfterRemovalAsync(selected);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task PurgeSelectedAsync()
    {
        List<DeletedWorkExperienceItem> selected =
        [
            .. DeletedItems.Where(i => i.IsSelected),
        ];

        if (selected.Count == 0)
        {
            return;
        }

        bool confirmed = await _dialogPresenter.ShowDangerConfirmationAsync(
            title: $"Delete {selected.Count} items permanently?",
            message: BuildPurgeWarning(selected),
            primaryButtonText: "Delete permanently",
            closeButtonText: "Cancel"
        );

        if (!confirmed)
        {
            return;
        }

        await _service.PurgeRangeWorkExperiencesAsync(
            selected.Select(i => i.WorkExperience.Id)
        );

        await ReloadAfterRemovalAsync(selected);
    }

    [RelayCommand(CanExecute = nameof(HasTrashItems))]
    private async Task EmptyTrashAsync()
    {
        bool confirmed = await _dialogPresenter.ShowDangerConfirmationAsync(
            title: "Empty trash?",
            message: $"This will permanently delete all {TotalDeletedCount} work experiences in the trash, including any hidden by your search or filters. This action cannot be undone.",
            primaryButtonText: "Empty trash",
            closeButtonText: "Cancel"
        );

        if (!confirmed)
        {
            return;
        }

        await _service.PurgeDeletedWorkExperiencesAsync();

        ClearPreviewIfRemoved([.. DeletedItems]);
        await LoadDeletedWorkExperiencesAsync();
    }

    [RelayCommand]
    private void ClearSelection()
    {
        foreach (DeletedWorkExperienceItem item in DeletedItems)
        {
            item.IsSelected = false;
        }

        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(HasSelection));

        RestoreSelectedCommand.NotifyCanExecuteChanged();
        PurgeSelectedCommand.NotifyCanExecuteChanged();

        UpdateSelectAllState();
    }

    [RelayCommand(CanExecute = nameof(CanGoToPreviousPage))]
    private async Task GoToPreviousPageAsync()
    {
        CurrentPage--;
        await LoadDeletedWorkExperiencesAsync();
    }

    [RelayCommand(CanExecute = nameof(CanGoToNextPage))]
    private async Task GoToNextPageAsync()
    {
        CurrentPage++;
        await LoadDeletedWorkExperiencesAsync();
    }

    private void RaisePagingChanged()
    {
        OnPropertyChanged(nameof(ResultSummary));
        OnPropertyChanged(nameof(PageIndicator));
        OnPropertyChanged(nameof(IsFiltering));
        OnPropertyChanged(nameof(HasTrashItems));
        OnPropertyChanged(nameof(EmptyStateTitle));
        OnPropertyChanged(nameof(EmptyStateMessage));
        EmptyTrashCommand.NotifyCanExecuteChanged();
    }

    private void ClearPreviewIfRemoved(
        IEnumerable<DeletedWorkExperienceItem> removedItems
    )
    {
        if (PreviewedItem is null || !removedItems.Contains(PreviewedItem))
        {
            return;
        }

        PreviewedItem = null;

        // If pinned: keep the panel open and display the empty state instead of
        // having it disappear abruptly. If not pinned: close as usual.
        if (!IsPreviewPanelPinned)
        {
            IsPreviewPanelOpen = false;
        }
    }

    private static string BuildPurgeWarning(
        List<DeletedWorkExperienceItem> items
    )
    {
        string subject =
            items.Count == 1
                ? $"'{items[0].CompanyName}'"
                : $"{items.Count} work experiences";

        return $"This will permanently delete {subject}. This action cannot be undone.";
    }

    private void UpdateSelectAllState()
    {
        _isSyncingSelectAll = true;

        IsAllSelected = DeletedItems.Count switch
        {
            0 => false,
            _ when DeletedItems.All(i => i.IsSelected) => true,
            _ when DeletedItems.All(i => !i.IsSelected) => false,
            _ => null, // Indeterminate: select only a portion
        };

        _isSyncingSelectAll = false;
    }

    private async Task LoadDeletedWorkExperiencesAsync(bool debounce = false)
    {
        int requestId = ++_loadRequestId;

        if (debounce)
        {
            await Task.Delay(300);

            // While waiting, if the user types another character
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
            WorkExperienceDeletedQuery query = new(
                SearchText: SearchText,
                DeletedRange: SelectedDeletedRange,
                Sort: new WorkExperienceSortOption(
                    Field: SortField,
                    Descending: IsSortDescending
                ),
                Paging: new PageOption(CurrentPage, PageSize)
            );

            PagedResult<WorkExperience> result =
                await _service.GetDeletedWorkExperiencesPagedAsync(query);

            // When nothing is filtered, the filtered count IS the trash total.
            int totalInTrash = IsFiltering
                ? await _service.GetDeletedWorkExperienceCountAsync()
                : result.TotalCount;

            // To handle cases where the database query runs slowly
            // and a newer request intervenes while waiting for the result
            // (even without debouncing).
            if (requestId != _loadRequestId)
            {
                return;
            }

            DeletedItems = new(
                result.Items.Select(w => new DeletedWorkExperienceItem(w))
            );

            CurrentPage = result.Page;
            TotalPages = result.TotalPages;
            FilteredCount = result.TotalCount;
            TotalDeletedCount = totalInTrash;

            UpdateSelectAllState();

            OnPropertyChanged(nameof(SelectedCount));
            OnPropertyChanged(nameof(HasSelection));

            RestoreSelectedCommand.NotifyCanExecuteChanged();
            PurgeSelectedCommand.NotifyCanExecuteChanged();

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

    private async Task ReloadAfterRemovalAsync(
        IEnumerable<DeletedWorkExperienceItem> removed
    )
    {
        ClearPreviewIfRemoved(removed);
        await LoadDeletedWorkExperiencesAsync();
    }

    private void ReloadFromFirstPage(bool debounce = false)
    {
        CurrentPage = 1;
        _ = LoadDeletedWorkExperiencesAsync(debounce);
    }
}
