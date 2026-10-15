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
    IWorkExperienceService service
) : NavigableViewModel(navigator), INavigationAware
{
    private readonly IWorkExperienceService _service = service;

    [ObservableProperty]
    public partial WorkExperienceItem? SelectedWorkExperienceItem { get; set; }

    public ObservableCollection<WorkExperienceItem> WorkExperienceItems
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public ObservableCollection<StatItem> Stats { get; } = [];

    public async Task OnNavigatedToAsync()
    {
        await LoadWorkExperiencesAsync();
        LoadStats();
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
        WorkExperienceItems.Remove(item);

        if (SelectedWorkExperienceItem == item)
        {
            SelectedWorkExperienceItem = null;
        }
    }

    private async Task LoadWorkExperiencesAsync()
    {
        try
        {
            WorkExperienceQuery query = new(
                SearchText: "",
                UpdatedRange: RelativeDateRange.All,
                Sort: new WorkExperienceSortOption(
                    Field: WorkExperienceSortField.EndDate,
                    Descending: true
                ),
                Paging: new PageOption(1, 20)
            );

            PagedResult<WorkExperience> result =
                await _service.GetWorkExperiencesPagedAsync(query);

            WorkExperienceItems = new(
                result.Items.Select(w => new WorkExperienceItem(w))
            );
        }
        finally { }
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
}
