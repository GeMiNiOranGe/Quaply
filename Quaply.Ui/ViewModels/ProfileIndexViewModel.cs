using Quaply.Data.Models;
using Quaply.Data.Querying.Base;
using Quaply.Data.Querying.Profiles;
using Quaply.Service.Interfaces;
using Quaply.Ui.Interfaces;
using Quaply.Ui.Models;
using Quaply.Ui.ViewModels.Base;
using Wpf.Ui.Controls;

namespace Quaply.Ui.ViewModels;

public class ProfileIndexViewModel(
    INavigator navigator,
    IProfileService service,
    IDialogPresenter dialogPresenter
)
    : IndexViewModel<Profile, ProfileItem, ProfileSortField>(
        navigator,
        dialogPresenter,
        defaultSortField: ProfileSortField.UpdatedAt
    )
{
    protected override string EntityName => "profile";

    protected override string EntityNamePlural => "profiles";

    protected override Task<PagedResult<Profile>> FetchPageAsync(
        ItemsQuery<ProfileSortField> query
    )
    {
        return service.GetProfilesPagedAsync(query);
    }

    protected override Task<int> FetchTotalCountAsync()
    {
        return service.GetProfileCountAsync();
    }

    protected override ProfileItem CreateItem(Profile entity)
    {
        return new(entity);
    }

    protected override Task DeleteItemAsync(int id)
    {
        return service.DeleteProfileAsync(id);
    }

    protected override Task DeleteItemsAsync(IReadOnlyList<int> ids)
    {
        return service.DeleteRangeProfilesAsync(ids);
    }

    protected override Task NavigateBackAsync()
    {
        return Navigator.NavigateToAsync<WorkExperienceIndexViewModel>();
    }

    protected override Task OpenRecoveryAsync()
    {
        return Navigator.NavigateToAsync<ProfileRecoveryViewModel>();
    }

    protected override Task OpenAddEditorAsync()
    {
        return Navigator.NavigateToAsync<
            ProfileEditorViewModel,
            ProfileEditorParameter
        >(ProfileEditorParameter.ForAdd());
    }

    protected override Task OpenEditEditorAsync(ProfileItem item)
    {
        return Navigator.NavigateToAsync<
            ProfileEditorViewModel,
            ProfileEditorParameter
        >(ProfileEditorParameter.ForEdit(item.Id));
    }

    protected override Task OpenDuplicateEditorAsync(ProfileItem item)
    {
        return Navigator.NavigateToAsync<
            ProfileEditorViewModel,
            ProfileEditorParameter
        >(ProfileEditorParameter.ForDuplicate(item.Id));
    }

    // The warning adapts to what the user is actually about to lose:
    // the "unlinked from resumes" part only appears when it is true.
    protected override string BuildDeleteWarning(
        IReadOnlyList<ProfileItem> items
    )
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

    protected override Task LoadStatsAsync()
    {
        Stats.Clear();

        for (int i = 0; i < 4; i++)
        {
            Stats.Add(
                new StatItem(
                    Icon: SymbolRegular.Person24,
                    Label: "Total Profiles",
                    Value: "5",
                    SubLabel: "Contact identities available for your resumes"
                )
            );
        }

        return Task.CompletedTask;
    }
}
