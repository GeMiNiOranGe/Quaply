using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Quaply.Data.Models;
using Quaply.Data.Querying;
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

    [ObservableProperty]
    public partial double PreviewPanelWidth { get; set; } = 320.0;

    [NotifyPropertyChangedFor(nameof(IsPreviewPanelOpen))]
    [NotifyPropertyChangedFor(nameof(PinTooltip))]
    [ObservableProperty]
    public partial bool IsPreviewPanelPinned { get; set; }

    [ObservableProperty]
    public partial bool IsPreviewPanelOpen { get; set; } = false;

    public string PinTooltip =>
        IsPreviewPanelPinned ? "Unpin panel" : "Keep panel open";

    public string SortDirectionTooltip =>
        IsSortDescending ? "Sort ascending" : "Sort descending";

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

    [ObservableProperty]
    public partial DeletedWorkExperienceItem? PreviewedItem { get; set; }

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    public bool HasDeletedItems => DeletedItems.Count > 0;

    public int SelectedCount => DeletedItems.Count(i => i.IsSelected);

    public bool HasSelection => SelectedCount > 0;

    [ObservableProperty]
    public partial bool? IsAllSelected { get; set; } = false;

    [ObservableProperty]
    public partial WorkExperienceSortField SortField { get; set; } =
        WorkExperienceSortField.DeletedAt;

    // ComboBox data source - derived directly from an enum;
    // do not hardcode the list.
    public IEnumerable<WorkExperienceSortField> SortFieldOptions { get; } =
        Enum.GetValues<WorkExperienceSortField>();

    [NotifyPropertyChangedFor(nameof(SortDirectionTooltip))]
    [ObservableProperty]
    public partial bool IsSortDescending { get; set; } = true;

    public async Task OnNavigatedToAsync()
    {
        await LoadDeletedWorkExperiencesAsync();
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
        // The panel width adjusts based on its pinned state: wider when pinned
        // for long-term use, and narrower when unpinned for quick viewing.
        // TODO: Consider implementing an automatic resizing feature.
        PreviewPanelWidth = value ? 360.0 : 320.0;

        // Unpin while the panel is empty: there's no reason to keep it open,
        // following the rule "unpinned + nothing to show = closed."
        if (!value && PreviewedItem is null)
        {
            IsPreviewPanelOpen = false;
        }
    }

    partial void OnSortFieldChanged(WorkExperienceSortField value)
    {
        _ = LoadDeletedWorkExperiencesAsync();
    }

    partial void OnIsSortDescendingChanged(bool value)
    {
        _ = LoadDeletedWorkExperiencesAsync();
    }

    [RelayCommand]
    private async Task BackToWorkExperiencesAsync()
    {
        await Navigator.NavigateToAsync<WorkExperienceViewModel>();
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

        OnPropertyChanged(nameof(HasSelection));
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
        IsPreviewPanelPinned = false;
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
        RemoveFromList(item);
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
        RemoveFromList(item);
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

        RemoveFromList(selected);
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

        RemoveFromList(selected);
    }

    [RelayCommand(CanExecute = nameof(HasDeletedItems))]
    private async Task EmptyTrashAsync()
    {
        bool confirmed = await _dialogPresenter.ShowDangerConfirmationAsync(
            title: "Empty trash?",
            message: BuildPurgeWarning([.. DeletedItems]),
            primaryButtonText: "Empty trash",
            closeButtonText: "Cancel"
        );

        if (!confirmed)
        {
            return;
        }

        await _service.PurgeRangeWorkExperiencesAsync(
            DeletedItems.Select(item => item.WorkExperience.Id)
        );

        DeletedItems = [];

        RestoreSelectedCommand.NotifyCanExecuteChanged();
        PurgeSelectedCommand.NotifyCanExecuteChanged();
        EmptyTrashCommand.NotifyCanExecuteChanged();

        UpdateSelectAllState();
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

    private void RemoveFromList(DeletedWorkExperienceItem item)
    {
        DeletedItems.Remove(item);

        OnPropertyChanged(nameof(HasDeletedItems));
        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(HasSelection));

        RestoreSelectedCommand.NotifyCanExecuteChanged();
        PurgeSelectedCommand.NotifyCanExecuteChanged();
        EmptyTrashCommand.NotifyCanExecuteChanged();

        UpdateSelectAllState();
        ClearPreviewIfRemoved([item]);
    }

    private void RemoveFromList(IEnumerable<DeletedWorkExperienceItem> items)
    {
        foreach (DeletedWorkExperienceItem item in items.ToList())
        {
            DeletedItems.Remove(item);
        }

        OnPropertyChanged(nameof(HasDeletedItems));
        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(HasSelection));

        RestoreSelectedCommand.NotifyCanExecuteChanged();
        PurgeSelectedCommand.NotifyCanExecuteChanged();
        EmptyTrashCommand.NotifyCanExecuteChanged();

        UpdateSelectAllState();
        ClearPreviewIfRemoved(items);
    }

    private async Task LoadDeletedWorkExperiencesAsync()
    {
        WorkExperienceSortOption sortOption = new(SortField, IsSortDescending);

        IEnumerable<WorkExperience> deleted =
            await _service.GetDeletedWorkExperiencesAsync(sortOption);

        DeletedItems = new(
            deleted.Select(w => new DeletedWorkExperienceItem(w))
        );

        UpdateSelectAllState();
    }
}
