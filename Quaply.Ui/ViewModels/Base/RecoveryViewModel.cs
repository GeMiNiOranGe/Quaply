using CommunityToolkit.Mvvm.Input;
using Quaply.Data.Querying.Base;
using Quaply.Ui.Interfaces;

namespace Quaply.Ui.ViewModels.Base;

/// <summary>
/// Shared behaviour for every "Recovery" page: the shared list engine plus
/// restore / purge / empty trash.
/// Derived classes only supply entity-specific bits (service calls, labels).
/// </summary>
/// <typeparam name="TEntity">Domain entity, e.g. Profile.</typeparam>
/// <typeparam name="TItem">
/// UI wrapper for one row, e.g. ProfileRecoveryItem.
/// </typeparam>
/// <typeparam name="TSortField">
/// Sort-field enum, e.g. DeletedProfileSortField.
/// </typeparam>
public abstract partial class RecoveryViewModel<TEntity, TItem, TSortField>(
    INavigator navigator,
    IDialogPresenter dialogPresenter,
    TSortField defaultSortField
)
    : ItemListViewModel<
        TEntity,
        TItem,
        TSortField,
        DeletedItemsQuery<TSortField>
    >(navigator, dialogPresenter, defaultSortField)
    where TEntity : class
    where TItem : class, IRecoveryItem
    where TSortField : struct, Enum
{
    // -------------------------------------------------------------------------
    // Extension points (Recovery-specific)
    // -------------------------------------------------------------------------

    protected abstract Task RestoreItemAsync(int id);

    protected abstract Task RestoreItemsAsync(IReadOnlyList<int> ids);

    protected abstract Task PurgeItemAsync(int id);

    protected abstract Task PurgeItemsAsync(IReadOnlyList<int> ids);

    protected abstract Task PurgeAllAsync();

    // -------------------------------------------------------------------------
    // Base-class implementations
    // -------------------------------------------------------------------------

    protected override string EmptyListTitle => "Trash is empty";

    protected override string EmptyListMessage =>
        $"Deleted {EntityNamePlural} will show up here.";

    protected sealed override DeletedItemsQuery<TSortField> BuildQuery()
    {
        return new(
            SearchText: SearchText,
            DeletedRange: SelectedDateRange,
            Sorting: new SortOption<TSortField>(
                Field: SortField,
                Descending: IsSortDescending
            ),
            Paging: new PageOption(Page: CurrentPage, PageSize: PageSize)
        );
    }

    protected override void NotifySelectionCommandsCanExecuteChanged()
    {
        RestoreSelectedCommand.NotifyCanExecuteChanged();
        PurgeSelectedCommand.NotifyCanExecuteChanged();
    }

    protected override void NotifyPagingCommandsCanExecuteChanged()
    {
        EmptyCommand.NotifyCanExecuteChanged();
    }

    // -------------------------------------------------------------------------
    // Commands
    // -------------------------------------------------------------------------

    // --- Restore -------------------------------------------------------------

    [RelayCommand]
    private async Task RestoreAsync(TItem? item)
    {
        if (item is null)
        {
            return;
        }

        await RestoreItemAsync(item.Id);
        await ReloadAfterRemovalAsync([item]);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task RestoreSelectedAsync()
    {
        List<TItem> selected = GetSelectedItems();

        if (selected.Count == 0)
        {
            return;
        }

        await RestoreItemsAsync([.. selected.Select(i => i.Id)]);
        await ReloadAfterRemovalAsync(selected);
    }

    // --- Purge ---------------------------------------------------------------

    [RelayCommand]
    private async Task PurgeAsync(TItem? item)
    {
        if (item is null)
        {
            return;
        }

        if (!await ConfirmPurgeAsync("Delete permanently?", [item]))
        {
            return;
        }

        await PurgeItemAsync(item.Id);
        await ReloadAfterRemovalAsync([item]);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task PurgeSelectedAsync()
    {
        List<TItem> selected = GetSelectedItems();

        if (selected.Count == 0)
        {
            return;
        }

        if (
            !await ConfirmPurgeAsync(
                $"Delete {selected.Count} items permanently?",
                selected
            )
        )
        {
            return;
        }

        await PurgeItemsAsync([.. selected.Select(i => i.Id)]);
        await ReloadAfterRemovalAsync(selected);
    }

    // --- Empty ---------------------------------------------------------------

    [RelayCommand(CanExecute = nameof(HasAnyItems))]
    private async Task EmptyAsync()
    {
        bool confirmed = await DialogPresenter.ShowExplicitConfirmationAsync(
            title: "Empty trash?",
            message: $"This will permanently delete all {TotalCount} {EntityNamePlural} in the trash, including any hidden by your search or filters. This action cannot be undone.",
            primaryButtonText: "Empty trash"
        );

        if (!confirmed)
        {
            return;
        }

        await PurgeAllAsync();
        await ReloadAfterRemovalAsync(Items);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private Task<bool> ConfirmPurgeAsync(
        string title,
        IReadOnlyList<TItem> items
    )
    {
        return DialogPresenter.ShowExplicitConfirmationAsync(
            title: title,
            message: BuildPurgeWarning(items),
            primaryButtonText: "Delete permanently"
        );
    }

    private string BuildPurgeWarning(IReadOnlyList<TItem> items)
    {
        string subject =
            items.Count == 1
                ? $"'{items[0].DisplayName}'"
                : $"{items.Count} {EntityNamePlural}";

        return $"This will permanently delete {subject}. This action cannot be undone.";
    }
}
