using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Quaply.Data.Models;
using Quaply.Data.Querying.Base;
using Quaply.Data.Querying.Profiles;
using Quaply.Service.Interfaces;
using Quaply.Ui.Interfaces;
using Quaply.Ui.Models;
using Quaply.Ui.ViewModels.Base;
using Wpf.Ui.Controls;

namespace Quaply.Ui.ViewModels;

public partial class ProfileViewModel(
    INavigator navigator,
    IProfileService service,
    IDialogPresenter dialogPresenter
) : NavigableViewModel(navigator), INavigationAware
{
    private readonly IProfileService _service = service;
    private readonly IDialogPresenter _dialogPresenter = dialogPresenter;

    // Guard flag to prevent IsAllSelected's setter and per-item ToggleSelection
    // from re-triggering each other in a loop.
    private bool _isSyncingSelectAll;

    // Guard flag so that ResetFilters changes several filter properties
    // but triggers only ONE reload instead of one per property.
    private bool _isResettingFilters;

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

    public bool HasAnyItems => TotalCount > 0;

    public string PageIndicator => $"Page {CurrentPage} of {TotalPages}";

    public string ResultSummary
    {
        get
        {
            if (Items.Count == 0)
            {
                return "No profiles";
            }

            int first = (CurrentPage - 1) * PageSize + 1;
            int last = first + Items.Count - 1;
            return $"Showing {first}-{last} of {FilteredCount}";
        }
    }

    public string EmptyStateTitle =>
        HasAnyItems ? "No matching profiles" : "No profiles yet";

    public string EmptyStateMessage =>
        HasAnyItems
            ? "Try changing your search or filters."
            : "Add your first profile to get started.";

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
    public partial ProfileItem? SelectedItem { get; set; }

    public ObservableCollection<ProfileItem> Items
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasVisibleItems));
        }
    } = [];

    [NotifyPropertyChangedFor(nameof(IsPreviewOverlayVisible))]
    [NotifyPropertyChangedFor(nameof(IsPreviewPanelVisible))]
    [ObservableProperty]
    public partial ProfileItem? PreviewedItem { get; set; }

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    public bool HasVisibleItems => Items.Count > 0;

    public int SelectedCount => Items.Count(i => i.IsSelected);

    public bool HasSelection => SelectedCount > 0;

    [ObservableProperty]
    public partial RelativeDateRange SelectedUpdatedRange { get; set; } =
        RelativeDateRange.All;

    [ObservableProperty]
    public partial bool? IsAllSelected { get; set; } = false;

    [ObservableProperty]
    public partial ProfileSortField SortField { get; set; } =
        ProfileSortField.UpdatedAt;

    [NotifyPropertyChangedFor(nameof(SortDirectionTooltip))]
    [ObservableProperty]
    public partial bool IsSortDescending { get; set; } = true;

    public ObservableCollection<StatItem> Stats { get; } = [];

    public async Task OnNavigatedToAsync()
    {
        await LoadAsync();
        await LoadStatsAsync();
    }

    partial void OnPageSizeChanged(int value)
    {
        ReloadFromFirstPage();
    }

    partial void OnSearchTextChanged(string value)
    {
        if (_isResettingFilters)
        {
            return;
        }

        IsSearching = true;
        ReloadFromFirstPage(debounce: true);
    }

    partial void OnSelectedUpdatedRangeChanged(RelativeDateRange value)
    {
        if (_isResettingFilters)
        {
            return;
        }

        ReloadFromFirstPage();
    }

    partial void OnSortFieldChanged(ProfileSortField value)
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

        foreach (ProfileItem item in Items)
        {
            item.IsSelected = value.Value;
        }

        RaiseSelectionChanged();
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
    private async Task GoBackAsync()
    {
        await Navigator.NavigateToAsync<WorkExperienceViewModel>();
    }

    [RelayCommand]
    private async Task NavigateToRecoveryAsync()
    {
        await Navigator.NavigateToAsync<ProfileRecoveryViewModel>();
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        await Navigator.NavigateToAsync<
            ProfileEditorViewModel,
            ProfileEditorParameter
        >(ProfileEditorParameter.ForAdd());
    }

    [RelayCommand]
    private void SetUpdatedRange(RelativeDateRange filter)
    {
        SelectedUpdatedRange = filter;
    }

    [RelayCommand]
    private void SetSortField(ProfileSortField field)
    {
        SortField = field;
    }

    [RelayCommand]
    private void ToggleSortDirection()
    {
        IsSortDescending = !IsSortDescending;
    }

    // Used by the "Clear filters" button of the "no matching profiles"
    // empty state: the user is stuck on an empty list, so give them
    // a one-click way out.
    [RelayCommand]
    private void ResetFilters()
    {
        _isResettingFilters = true;

        try
        {
            SearchText = string.Empty;
            SelectedUpdatedRange = RelativeDateRange.All;
        }
        finally
        {
            _isResettingFilters = false;
        }

        ReloadFromFirstPage();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadAsync();
        await LoadStatsAsync();
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
    private void ViewDetails(ProfileItem? item)
    {
        if (item is null)
        {
            return;
        }

        PreviewedItem = item;
        IsPreviewPanelOpen = true;
    }

    [RelayCommand]
    private async Task EditAsync(ProfileItem? item)
    {
        if (item is null)
        {
            return;
        }

        await Navigator.NavigateToAsync<
            ProfileEditorViewModel,
            ProfileEditorParameter
        >(ProfileEditorParameter.ForEdit(item.Id));
    }

    [RelayCommand]
    private async Task DuplicateAsync(ProfileItem? item)
    {
        if (item is null)
        {
            return;
        }

        await Navigator.NavigateToAsync<
            ProfileEditorViewModel,
            ProfileEditorParameter
        >(ProfileEditorParameter.ForDuplicate(item.Id));
    }

    [RelayCommand]
    private async Task DeleteAsync(ProfileItem? item)
    {
        if (item is null)
        {
            return;
        }

        bool confirmed = await _dialogPresenter.ShowDangerConfirmationAsync(
            title: "Delete profile?",
            message: BuildDeleteWarning([item]),
            primaryButtonText: "Delete",
            closeButtonText: "Cancel"
        );

        if (!confirmed)
        {
            return;
        }

        await _service.DeleteProfileAsync(item.Id);
        await ReloadAfterRemovalAsync([item]);
    }

    [RelayCommand(CanExecute = nameof(CanGoToPreviousPage))]
    private async Task GoToPreviousPageAsync()
    {
        CurrentPage--;
        await LoadAsync();
    }

    [RelayCommand(CanExecute = nameof(CanGoToNextPage))]
    private async Task GoToNextPageAsync()
    {
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
        List<ProfileItem> selected = [.. Items.Where(i => i.IsSelected)];

        if (selected.Count == 0)
        {
            return;
        }

        bool confirmed = await _dialogPresenter.ShowDangerConfirmationAsync(
            title: $"Delete {selected.Count} profiles?",
            message: BuildDeleteWarning(selected),
            primaryButtonText: "Delete",
            closeButtonText: "Cancel"
        );

        if (!confirmed)
        {
            return;
        }

        await _service.DeleteRangeProfilesAsync(
            selected.Select(i => i.Profile.Id)
        );

        await ReloadAfterRemovalAsync(selected);
    }

    [RelayCommand]
    private void ClearSelection()
    {
        foreach (ProfileItem item in Items)
        {
            item.IsSelected = false;
        }

        RaiseSelectionChanged();
        UpdateSelectAllState();
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
        OnPropertyChanged(nameof(IsFiltering));
        OnPropertyChanged(nameof(HasAnyItems));
        OnPropertyChanged(nameof(EmptyStateTitle));
        OnPropertyChanged(nameof(EmptyStateMessage));
    }

    private void ClearPreviewIfRemoved(IEnumerable<ProfileItem> removedItems)
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

    // The warning adapts to what the user is actually about to lose:
    // the "unlinked from resumes" part only appears when it is true.
    private static string BuildDeleteWarning(List<ProfileItem> items)
    {
        string subject =
            items.Count == 1
                ? $"'{items[0].FullName}'"
                : $"{items.Count} profiles";

        string pronoun = items.Count == 1 ? "it" : "them";

        string message = $"This will move {subject} to the trash.";

        if (items.Any(i => i.ResumeCount > 0))
        {
            return message
                + $"\n\nAny resume using {pronoun} will be unlinked. "
                + $"You can restore {pronoun} from the trash later, "
                + "but those links will not be restored.";
        }

        return message
            + $"\n\nYou can restore {pronoun} from the trash at any time.";
    }

    private void UpdateSelectAllState()
    {
        _isSyncingSelectAll = true;

        IsAllSelected = Items.Count switch
        {
            0 => false,
            _ when Items.All(i => i.IsSelected) => true,
            _ when Items.All(i => !i.IsSelected) => false,
            _ => null, // Indeterminate: select only a portion
        };

        _isSyncingSelectAll = false;
    }

    private async Task LoadAsync(bool debounce = false)
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
            ItemsQuery<ProfileSortField> query = new(
                SearchText: SearchText,
                UpdatedRange: SelectedUpdatedRange,
                Sorting: new SortOption<ProfileSortField>(
                    Field: SortField,
                    Descending: IsSortDescending
                ),
                Paging: new PageOption(CurrentPage, PageSize)
            );

            PagedResult<Profile> result = await _service.GetProfilesPagedAsync(
                query
            );

            // When nothing is filtered, the filtered count IS the total.
            int totalCount = IsFiltering
                ? await _service.GetProfileCountAsync()
                : result.TotalCount;

            // To handle cases where the database query runs slowly
            // and a newer request intervenes while waiting for the result
            // (even without debouncing).
            if (requestId != _loadRequestId)
            {
                return;
            }

            Items = new(result.Items.Select(p => new ProfileItem(p)));

            CurrentPage = result.Page;
            TotalPages = result.TotalPages;
            FilteredCount = result.TotalCount;
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

    // Real numbers from the service. The stats are independent of
    // search/filter/paging, so they are loaded separately from the list.
    private async Task LoadStatsAsync()
    {
        Stats.Clear();

        Stats.Add(
            new StatItem(
                Icon: SymbolRegular.Person24,
                Label: "Total Profiles",
                Value: "5",
                SubLabel: "Contact identities available for your resumes"
            )
        );

        Stats.Add(
            new StatItem(
                Icon: SymbolRegular.Document24,
                Label: "Used in Resumes",
                Value: $"5 of 5",
                SubLabel: "Profiles linked to at least one resume"
            )
        );

        Stats.Add(
            new StatItem(
                Icon: SymbolRegular.Link24,
                Label: "Online Presence",
                Value: $"5 of 5",
                SubLabel: "Profiles with a LinkedIn, GitHub or portfolio link"
            )
        );
    }

    private async Task ReloadAfterRemovalAsync(IEnumerable<ProfileItem> removed)
    {
        ClearPreviewIfRemoved(removed);

        if (SelectedItem is not null && removed.Contains(SelectedItem))
        {
            SelectedItem = null;
        }

        await LoadAsync();
        await LoadStatsAsync();
    }

    private void ReloadFromFirstPage(bool debounce = false)
    {
        CurrentPage = 1;
        _ = LoadAsync(debounce);
    }
}
