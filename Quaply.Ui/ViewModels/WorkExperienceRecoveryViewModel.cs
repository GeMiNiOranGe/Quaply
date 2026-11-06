using Quaply.Data.Models;
using Quaply.Data.Querying.Base;
using Quaply.Data.Querying.WorkExperiences;
using Quaply.Service.Interfaces;
using Quaply.Ui.Interfaces;
using Quaply.Ui.Models;
using Quaply.Ui.ViewModels.Base;

namespace Quaply.Ui.ViewModels;

public class WorkExperienceRecoveryViewModel(
    INavigator navigator,
    IWorkExperienceService service,
    IDialogPresenter dialogPresenter
)
    : RecoveryViewModel<
        WorkExperience,
        WorkExperienceRecoveryItem,
        DeletedWorkExperienceSortField
    >(navigator, dialogPresenter, DeletedWorkExperienceSortField.DeletedAt)
{
    protected override string EntityNamePlural => "work experiences";

    protected override Task NavigateBackAsync()
    {
        return Navigator.NavigateToAsync<WorkExperienceViewModel>();
    }

    protected override Task<PagedResult<WorkExperience>> FetchPageAsync(
        DeletedItemsQuery<DeletedWorkExperienceSortField> query
    )
    {
        return service.GetDeletedWorkExperiencesPagedAsync(query);
    }

    protected override Task<int> FetchTotalCountAsync()
    {
        return service.GetDeletedWorkExperienceCountAsync();
    }

    protected override WorkExperienceRecoveryItem CreateItem(
        WorkExperience WorkExperience
    )
    {
        return new(WorkExperience);
    }

    protected override Task RestoreItemAsync(int id)
    {
        return service.RestoreWorkExperienceAsync(id);
    }

    protected override Task RestoreItemsAsync(IReadOnlyList<int> ids)
    {
        return service.RestoreRangeWorkExperiencesAsync(ids);
    }

    protected override Task PurgeItemAsync(int id)
    {
        return service.PurgeWorkExperienceAsync(id);
    }

    protected override Task PurgeItemsAsync(IReadOnlyList<int> ids)
    {
        return service.PurgeRangeWorkExperiencesAsync(ids);
    }

    protected override Task PurgeAllAsync()
    {
        return service.PurgeDeletedWorkExperiencesAsync();
    }
}
