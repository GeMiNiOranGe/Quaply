using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Quaply.Data.Models;
using Quaply.Service.Interfaces;
using Quaply.Ui.Interfaces;
using Quaply.Ui.ViewModels.Base;

namespace Quaply.Ui.ViewModels;

public partial class WorkExperienceViewModel(
    INavigator navigator,
    IWorkExperienceService service
) : NavigableViewModel(navigator), INavigationAware
{
    private readonly IWorkExperienceService _service = service;

    [ObservableProperty]
    public partial WorkExperience? SelectedWorkExperience { get; set; }

    public ObservableCollection<WorkExperience> WorkExperiences
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public async Task OnNavigatedToAsync()
    {
        await LoadWorkExperiencesAsync();
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
    private async Task EditWorkExperienceAsync(WorkExperience? workExperience)
    {
        if (workExperience is null)
        {
            return;
        }

        await Navigator.NavigateToAsync<
            WorkExperienceEditorViewModel,
            WorkExperienceEditorParameter
        >(WorkExperienceEditorParameter.ForEdit(workExperience.Id));
    }

    [RelayCommand]
    private async Task DuplicateWorkExperienceAsync(
        WorkExperience? workExperience
    )
    {
        if (workExperience is null)
        {
            return;
        }

        await Navigator.NavigateToAsync<
            WorkExperienceEditorViewModel,
            WorkExperienceEditorParameter
        >(WorkExperienceEditorParameter.ForDuplicate(workExperience.Id));
    }

    [RelayCommand]
    private async Task DeleteWorkExperienceAsync(WorkExperience? workExperience)
    {
        if (workExperience is null)
        {
            return;
        }

        await _service.DeleteWorkExperienceAsync(workExperience.Id);
        WorkExperiences.Remove(workExperience);

        if (SelectedWorkExperience == workExperience)
        {
            SelectedWorkExperience = null;
        }
    }

    private async Task LoadWorkExperiencesAsync()
    {
        IEnumerable<WorkExperience> items =
            await _service.GetWorkExperiencesAsync();
        WorkExperiences = new(items.OrderByDescending(w => w.StartDate));
    }
}
