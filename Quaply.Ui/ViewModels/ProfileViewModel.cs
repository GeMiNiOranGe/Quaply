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

namespace Quaply.Ui.ViewModels;

public partial class ProfileViewModel(
    INavigator navigator,
    IProfileService service
) : NavigableViewModel(navigator), INavigationAware
{
    private readonly IProfileService _service = service;

    [ObservableProperty]
    public partial ProfileItem? SelectedItem { get; set; }

    public ObservableCollection<ProfileItem> Items
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
        await LoadAsync();
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

        await _service.DeleteProfileAsync(item.Id);
        Items.Remove(item);

        if (SelectedItem == item)
        {
            SelectedItem = null;
        }
    }

    private async Task LoadAsync()
    {
        try
        {
            ItemsQuery<ProfileSortField> query = new(
                SearchText: null,
                UpdatedRange: RelativeDateRange.All,
                Sorting: new SortOption<ProfileSortField>(
                    Field: ProfileSortField.UpdatedAt,
                    Descending: true
                ),
                Paging: new PageOption(1, 20)
            );

            PagedResult<Profile> result = await _service.GetProfilesPagedAsync(
                query
            );
            Items = new(
                result.Items.Select(profile => new ProfileItem(profile))
            );
        }
        finally { }
    }
}
