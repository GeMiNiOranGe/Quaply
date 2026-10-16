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
using Wpf.Ui.Controls;

namespace Quaply.Ui.ViewModels;

public partial class WorkExperienceViewModel(
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
    public partial int TotalCount { get; set; }

    public bool IsFiltering =>
        !string.IsNullOrWhiteSpace(SearchText)
        || SelectedUpdatedRange != RelativeDateRange.All;

    public bool HasWorkExperienceItems => TotalCount > 0;

    public string PageIndicator => $"Page {CurrentPage} of {TotalPages}";

    public string ResultSummary
    {
        get
        {
            if (WorkExperienceItems.Count == 0)
            {
                return "No work experiences";
            }

            int first = (CurrentPage - 1) * PageSize + 1;
            int last = first + WorkExperienceItems.Count - 1;
            return $"Showing {first}-{last} of {FilteredCount}";
        }
    }

    public string EmptyStateTitle =>
        HasWorkExperienceItems
            ? "No matching work experiences"
            : "No work experiences yet";

    public string EmptyStateMessage =>
        HasWorkExperienceItems
            ? "Try changing your search or filters."
            : "Add your first work experience to get started.";

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

    [ObservableProperty]
    public partial WorkExperienceItem? SelectedWorkExperienceItem { get; set; }

    public ObservableCollection<WorkExperienceItem> WorkExperienceItems
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasWorkExperienceItems));
        }
    } = [];

    [NotifyPropertyChangedFor(nameof(IsPreviewOverlayVisible))]
    [NotifyPropertyChangedFor(nameof(IsPreviewPanelVisible))]
    [ObservableProperty]
    public partial WorkExperienceItem? PreviewedItem { get; set; }

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    public bool HasWorkExperiences => WorkExperienceItems.Count > 0;

    public int SelectedCount => WorkExperienceItems.Count(i => i.IsSelected);

    public bool HasSelection => SelectedCount > 0;

    [ObservableProperty]
    public partial RelativeDateRange SelectedUpdatedRange { get; set; } =
        RelativeDateRange.All;

    [ObservableProperty]
    public partial bool? IsAllSelected { get; set; } = false;

    [ObservableProperty]
    public partial WorkExperienceSortField SortField { get; set; } =
        WorkExperienceSortField.EndDate;

    [NotifyPropertyChangedFor(nameof(SortDirectionTooltip))]
    [ObservableProperty]
    public partial bool IsSortDescending { get; set; } = true;

    public ObservableCollection<StatItem> Stats { get; } = [];

    public async Task OnNavigatedToAsync()
    {
        await LoadWorkExperiencesAsync();
        LoadStats();
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

    partial void OnSelectedUpdatedRangeChanged(RelativeDateRange value)
    {
        ReloadFromFirstPage();
    }

    partial void OnSortFieldChanged(WorkExperienceSortField value)
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

        foreach (WorkExperienceItem item in WorkExperienceItems)
        {
            item.IsSelected = value.Value;
        }

        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(HasSelection));

        DeleteSelectedCommand.NotifyCanExecuteChanged();
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

    [RelayCommand]
    private async Task NavigateToProfileAsync()
    {
        await Navigator.NavigateToAsync<ProfileViewModel>();
    }

    [RelayCommand]
    private async Task NavigateToWorkExperienceTrashAsync()
    {
        await Navigator.NavigateToAsync<WorkExperienceTrashViewModel>();
    }

    [RelayCommand]
    private async Task AddWorkExperienceAsync()
    {
        await Navigator.NavigateToAsync<
            WorkExperienceEditorViewModel,
            WorkExperienceEditorParameter
        >(WorkExperienceEditorParameter.ForAdd());
    }

    [RelayCommand]
    private void SetUpdatedRange(RelativeDateRange filter)
    {
        SelectedUpdatedRange = filter;
    }

    [RelayCommand]
    private void SetSortField(WorkExperienceSortField field)
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
        await LoadWorkExperiencesAsync();
    }

    // Called by the checkbox column's Checked/Unchecked so the bulk toolbar
    // and "N selected" count stay in sync.
    [RelayCommand]
    private void ToggleSelection()
    {
        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(HasSelection));

        DeleteSelectedCommand.NotifyCanExecuteChanged();

        UpdateSelectAllState();
    }

    [RelayCommand]
    private void ViewDetailWorkExperience(WorkExperienceItem? item)
    {
        if (item is null)
        {
            return;
        }

        PreviewedItem = item;
        IsPreviewPanelOpen = true;
    }

    [RelayCommand]
    private async Task EditWorkExperienceAsync(WorkExperienceItem? item)
    {
        if (item is null)
        {
            return;
        }

        await Navigator.NavigateToAsync<
            WorkExperienceEditorViewModel,
            WorkExperienceEditorParameter
        >(WorkExperienceEditorParameter.ForEdit(item.Id));
    }

    [RelayCommand]
    private async Task DuplicateWorkExperienceAsync(WorkExperienceItem? item)
    {
        if (item is null)
        {
            return;
        }

        await Navigator.NavigateToAsync<
            WorkExperienceEditorViewModel,
            WorkExperienceEditorParameter
        >(WorkExperienceEditorParameter.ForDuplicate(item.Id));
    }

    [RelayCommand]
    private async Task DeleteWorkExperienceAsync(WorkExperienceItem? item)
    {
        if (item is null)
        {
            return;
        }

        await _service.DeleteWorkExperienceAsync(item.Id);
        await ReloadAfterRemovalAsync([item]);
    }

    [RelayCommand(CanExecute = nameof(CanGoToPreviousPage))]
    private async Task GoToPreviousPageAsync()
    {
        CurrentPage--;
        await LoadWorkExperiencesAsync();
    }

    [RelayCommand(CanExecute = nameof(CanGoToNextPage))]
    private async Task GoToNextPageAsync()
    {
        CurrentPage++;
        await LoadWorkExperiencesAsync();
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

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DeleteSelectedAsync()
    {
        List<WorkExperienceItem> selected =
        [
            .. WorkExperienceItems.Where(i => i.IsSelected),
        ];

        if (selected.Count == 0)
        {
            return;
        }

        bool confirmed = await _dialogPresenter.ShowDangerConfirmationAsync(
            title: $"Delete {selected.Count} items?",
            message: BuildDeleteWarning(selected),
            primaryButtonText: "Delete",
            closeButtonText: "Cancel"
        );

        if (!confirmed)
        {
            return;
        }

        await _service.DeleteRangeWorkExperiencesAsync(
            selected.Select(i => i.WorkExperience.Id)
        );

        await ReloadAfterRemovalAsync(selected);
    }

    [RelayCommand]
    private void ClearSelection()
    {
        foreach (WorkExperienceItem item in WorkExperienceItems)
        {
            item.IsSelected = false;
        }

        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(HasSelection));

        DeleteSelectedCommand.NotifyCanExecuteChanged();

        UpdateSelectAllState();
    }

    private void RaisePagingChanged()
    {
        OnPropertyChanged(nameof(ResultSummary));
        OnPropertyChanged(nameof(PageIndicator));
        OnPropertyChanged(nameof(IsFiltering));
        OnPropertyChanged(nameof(HasWorkExperienceItems));
        OnPropertyChanged(nameof(EmptyStateTitle));
        OnPropertyChanged(nameof(EmptyStateMessage));
    }

    private void ClearPreviewIfRemoved(
        IEnumerable<WorkExperienceItem> removedItems
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

    private static string BuildDeleteWarning(List<WorkExperienceItem> items)
    {
        string subject =
            items.Count == 1
                ? $"'{items[0].CompanyName}'"
                : $"{items.Count} work experiences";

        return $"This will move {subject} to the trash. You can restore them from there anytime.";
    }

    private void UpdateSelectAllState()
    {
        _isSyncingSelectAll = true;

        IsAllSelected = WorkExperienceItems.Count switch
        {
            0 => false,
            _ when WorkExperienceItems.All(i => i.IsSelected) => true,
            _ when WorkExperienceItems.All(i => !i.IsSelected) => false,
            _ => null, // Indeterminate: select only a portion
        };

        _isSyncingSelectAll = false;
    }

    private async Task LoadWorkExperiencesAsync(bool debounce = false)
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
            WorkExperienceQuery query = new(
                SearchText: SearchText,
                UpdatedRange: SelectedUpdatedRange,
                Sort: new WorkExperienceSortOption(
                    Field: SortField,
                    Descending: IsSortDescending
                ),
                Paging: new PageOption(CurrentPage, PageSize)
            );

            PagedResult<WorkExperience> result =
                await _service.GetWorkExperiencesPagedAsync(query);

            // When nothing is filtered, the filtered count IS the total.
            int total = IsFiltering
                ? await _service.GetWorkExperienceCountAsync()
                : result.TotalCount;

            // To handle cases where the database query runs slowly
            // and a newer request intervenes while waiting for the result
            // (even without debouncing).
            if (requestId != _loadRequestId)
            {
                return;
            }

            WorkExperienceItems = new(
                result.Items.Select(w => new WorkExperienceItem(w))
            );

            CurrentPage = result.Page;
            TotalPages = result.TotalPages;
            FilteredCount = result.TotalCount;
            TotalCount = total;

            UpdateSelectAllState();

            OnPropertyChanged(nameof(SelectedCount));
            OnPropertyChanged(nameof(HasSelection));

            DeleteSelectedCommand.NotifyCanExecuteChanged();

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

    private void LoadStats()
    {
        Stats.Clear();

        Stats.Add(
            new StatItem(
                Icon: SymbolRegular.Delete24,
                Label: "Total Deleted Records",
                Value: "03 Items",
                SubLabel: "Can be restored at any time"
            )
        );

        Stats.Add(
            new StatItem(
                Icon: SymbolRegular.HistoryDismiss24,
                Label: "Storage Experience Time",
                Value: "4.3 Years",
                SubLabel: "Total experience time in the archive"
            )
        );

        Stats.Add(
            new StatItem(
                Icon: SymbolRegular.CheckmarkCircle24,
                Label: "Instant Restoration",
                Value: "Automatic",
                SubLabel: "Linked data and skills will be reactivated immediately upon restoration"
            )
        );
    }

    private async Task ReloadAfterRemovalAsync(
        IEnumerable<WorkExperienceItem> removed
    )
    {
        ClearPreviewIfRemoved(removed);

        if (
            SelectedWorkExperienceItem is not null
            && removed.Contains(SelectedWorkExperienceItem)
        )
        {
            SelectedWorkExperienceItem = null;
        }

        await LoadWorkExperiencesAsync();
    }

    private void ReloadFromFirstPage(bool debounce = false)
    {
        CurrentPage = 1;
        _ = LoadWorkExperiencesAsync(debounce);
    }
}
