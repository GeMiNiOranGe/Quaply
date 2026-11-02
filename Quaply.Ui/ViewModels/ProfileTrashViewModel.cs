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

public partial class ProfileTrashViewModel(
    INavigator navigator,
    IProfileService service,
    IDialogPresenter dialogPresenter
) : NavigableViewModel(navigator), INavigationAware
{
    private readonly IProfileService _service = service;
    private readonly IDialogPresenter _dialogPresenter = dialogPresenter;

    [ObservableProperty]
    public partial DeletedProfileItem? SelectedItem { get; set; }

    public ObservableCollection<DeletedProfileItem> Items
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
        await Navigator.NavigateToAsync<ProfileViewModel>();
    }

    [RelayCommand]
    private async Task RestoreAsync(DeletedProfileItem? item)
    {
        if (item is null)
        {
            return;
        }

        await _service.RestoreProfileAsync(item.Id);
        Items.Remove(item);

        if (SelectedItem == item)
        {
            SelectedItem = null;
        }
    }

    [RelayCommand]
    private async Task PurgeAsync(DeletedProfileItem? item)
    {
        if (item is null)
        {
            return;
        }

        bool confirmed = await _dialogPresenter.ShowConfirmationAsync(
            title: "Confirm permanent deletion",
            message: $"Permanently delete the profile '{item.FullName}'? This action cannot be undone.",
            primaryButtonText: "Delete permanently",
            closeButtonText: "Cancel",
            defaultButton: ContentDialogButton.Close
        );

        if (!confirmed)
        {
            return;
        }

        await _service.PurgeProfileAsync(item.Id);
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
            DeletedProfileQuery query = new(
                SearchText: "",
                DeletedRange: RelativeDateRange.All,
                Sort: new DeletedProfileSortOption(
                    Field: DeletedProfileSortField.DeletedAt,
                    Descending: true
                ),
                Paging: new PageOption(1, 20)
            );

            PagedResult<Profile> result =
                await _service.GetDeletedProfilesPagedAsync(query);

            Items = new(result.Items.Select(p => new DeletedProfileItem(p)));
        }
        finally { }
    }
}
