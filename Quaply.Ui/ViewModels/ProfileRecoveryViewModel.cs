using Quaply.Data.Models;
using Quaply.Data.Querying.Base;
using Quaply.Data.Querying.Profiles;
using Quaply.Service.Interfaces;
using Quaply.Ui.Interfaces;
using Quaply.Ui.Models;
using Quaply.Ui.ViewModels.Base;

namespace Quaply.Ui.ViewModels;

public class ProfileRecoveryViewModel(
    INavigator navigator,
    IProfileService service,
    IDialogPresenter dialogPresenter
)
    : RecoveryViewModel<Profile, ProfileRecoveryItem, DeletedProfileSortField>(
        navigator,
        dialogPresenter,
        DeletedProfileSortField.DeletedAt
    )
{
    protected override string EntityNamePlural => "profiles";

    protected override Task NavigateBackAsync()
    {
        return Navigator.NavigateToAsync<ProfileIndexViewModel>();
    }

    protected override Task<PagedResult<Profile>> FetchPageAsync(
        DeletedItemsQuery<DeletedProfileSortField> query
    )
    {
        return service.GetDeletedProfilesPagedAsync(query);
    }

    protected override Task<int> FetchTotalCountAsync()
    {
        return service.GetDeletedProfileCountAsync();
    }

    protected override ProfileRecoveryItem CreateItem(Profile Profile)
    {
        return new(Profile);
    }

    protected override Task RestoreItemAsync(int id)
    {
        return service.RestoreProfileAsync(id);
    }

    protected override Task RestoreItemsAsync(IReadOnlyList<int> ids)
    {
        return service.RestoreRangeProfilesAsync(ids);
    }

    protected override Task PurgeItemAsync(int id)
    {
        return service.PurgeProfileAsync(id);
    }

    protected override Task PurgeItemsAsync(IReadOnlyList<int> ids)
    {
        return service.PurgeRangeProfilesAsync(ids);
    }

    protected override Task PurgeAllAsync()
    {
        return service.PurgeDeletedProfilesAsync();
    }
}
