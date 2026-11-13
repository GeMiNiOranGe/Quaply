using Quaply.Data.Models;
using Quaply.Data.Querying.Base;
using Quaply.Data.Querying.WorkExperiences;
using Quaply.Service.Interfaces;
using Quaply.Ui.Interfaces;
using Quaply.Ui.Models;
using Quaply.Ui.ViewModels.Base;
using Wpf.Ui.Controls;

namespace Quaply.Ui.ViewModels;

public class WorkExperienceIndexViewModel(
    INavigator navigator,
    IWorkExperienceService service,
    IDialogPresenter dialogPresenter
)
    : IndexViewModel<
        WorkExperience,
        WorkExperienceItem,
        WorkExperienceSortField
    >(
        navigator,
        dialogPresenter,
        defaultSortField: WorkExperienceSortField.UpdatedAt
    )
{
    protected override string EntityName => "work experience";

    protected override string EntityNamePlural => "work experiences";

    protected override Task<PagedResult<WorkExperience>> FetchPageAsync(
        ItemsQuery<WorkExperienceSortField> query
    )
    {
        return service.GetWorkExperiencesPagedAsync(query);
    }

    protected override Task<int> FetchTotalCountAsync()
    {
        return service.GetWorkExperienceCountAsync();
    }

    protected override WorkExperienceItem CreateItem(WorkExperience entity)
    {
        return new(entity);
    }

    protected override Task DeleteItemAsync(int id)
    {
        return service.DeleteWorkExperienceAsync(id);
    }

    protected override Task DeleteItemsAsync(IReadOnlyList<int> ids)
    {
        return service.DeleteRangeWorkExperiencesAsync(ids);
    }

    protected override Task NavigateBackAsync()
    {
        return Navigator.NavigateToAsync<ProfileIndexViewModel>();
    }

    protected override Task OpenRecoveryAsync()
    {
        return Navigator.NavigateToAsync<WorkExperienceRecoveryViewModel>();
    }

    protected override Task OpenAddEditorAsync()
    {
        return Navigator.NavigateToAsync<
            WorkExperienceEditorViewModel,
            WorkExperienceEditorParameter
        >(WorkExperienceEditorParameter.ForAdd());
    }

    protected override Task OpenEditEditorAsync(WorkExperienceItem item)
    {
        return Navigator.NavigateToAsync<
            WorkExperienceEditorViewModel,
            WorkExperienceEditorParameter
        >(WorkExperienceEditorParameter.ForEdit(item.Id));
    }

    protected override Task OpenDuplicateEditorAsync(WorkExperienceItem item)
    {
        return Navigator.NavigateToAsync<
            WorkExperienceEditorViewModel,
            WorkExperienceEditorParameter
        >(WorkExperienceEditorParameter.ForDuplicate(item.Id));
    }

    // The warning adapts to what the user is actually about to lose:
    // the "unlinked from resumes" part only appears when it is true.
    protected override string BuildDeleteWarning(
        IReadOnlyList<WorkExperienceItem> items
    )
    {
        string subject =
            items.Count == 1
                ? $"'{items[0].CompanyName}'"
                : $"{items.Count} work experiences";

        string pronoun = items.Count == 1 ? "it" : "them";

        return $"This will move {subject} to the trash, "
            + $"along with all projects belonging to {pronoun}.\n\n"
            + $"Any other items linked to {pronoun} will be unlinked. "
            + "You can restore them from the trash later, "
            + "but those links will not be restored.";
    }

    protected override Task LoadStatsAsync()
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

        return Task.CompletedTask;
    }
}
