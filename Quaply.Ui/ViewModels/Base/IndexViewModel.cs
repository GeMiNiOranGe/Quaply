using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Quaply.Data.Querying.Base;
using Quaply.Ui.Interfaces;
using Quaply.Ui.Models;

namespace Quaply.Ui.ViewModels.Base;

/// <summary>
/// Base class for every "index" page: the shared list engine
/// (<see cref="ItemListViewModel{TEntity,TItem,TSortField,TQuery}"/>) plus
/// add / edit / duplicate, delete-to-trash, recovery navigation, extra
/// filters and stats.
///
/// Derived classes supply only what is entity-specific: the service calls,
/// the item wrapper, navigation targets, wording and (optionally) stats and
/// extra filters.
/// </summary>
/// <typeparam name="TEntity">
/// Data model returned by the service.
/// </typeparam>
/// <typeparam name="TItem">
/// UI wrapper around TEntity.
/// </typeparam>
/// <typeparam name="TSortField">
/// Sort-field enum of the entity.
/// </typeparam>
public abstract partial class IndexViewModel<TEntity, TItem, TSortField>(
    INavigator navigator,
    IDialogPresenter dialogPresenter,
    TSortField defaultSortField,
    bool defaultSortDescending = true
)
    : ItemListViewModel<TEntity, TItem, TSortField, ItemsQuery<TSortField>>(
        navigator,
        dialogPresenter,
        defaultSortField,
        defaultSortDescending
    )
    where TEntity : class
    where TItem : class, IIndexItem
    where TSortField : struct, Enum
{
    // -------------------------------------------------------------------------
    // Extension points (Index-specific)
    // -------------------------------------------------------------------------

    // --- Must provide (abstract) ---------------------------------------------

    /// <summary>
    /// Singular, lowercase: "profile", "work experience".
    /// </summary>
    protected abstract string EntityName { get; }

    protected abstract string BuildDeleteWarning(IReadOnlyList<TItem> items);

    protected abstract Task DeleteItemAsync(int id);

    protected abstract Task DeleteItemsAsync(IReadOnlyList<int> ids);

    protected abstract Task OpenRecoveryAsync();

    protected abstract Task OpenAddEditorAsync();

    protected abstract Task OpenEditEditorAsync(TItem item);

    protected abstract Task OpenDuplicateEditorAsync(TItem item);

    // --- May provide (virtual) -----------------------------------------------

    /// <summary>
    /// Override when the page has filters beyond search + date.
    /// </summary>
    protected virtual bool HasExtraFilters => false;

    /// <summary>
    /// Reset the extra filters; reload is handled by the base.
    /// </summary>
    protected virtual void ResetExtraFilters() { }

    /// <summary>
    /// Stats are independent of search/filter/paging, so they are loaded on
    /// navigation, refresh and after deletes - not on every page change.
    /// </summary>
    protected virtual Task LoadStatsAsync()
    {
        return Task.CompletedTask;
    }

    // -------------------------------------------------------------------------
    // Base-class implementations
    // -------------------------------------------------------------------------

    public override bool IsFiltering => base.IsFiltering || HasExtraFilters;

    protected override string EmptyListTitle => $"No {EntityNamePlural} yet";

    protected override string EmptyListMessage =>
        $"Add your first {EntityName} to get started.";

    /// <summary>
    /// Override (and call base) to add extra filters to the query.
    /// </summary>
    protected override ItemsQuery<TSortField> BuildQuery()
    {
        return new(
            SearchText: SearchText,
            UpdatedRange: SelectedDateRange,
            Sorting: new SortOption<TSortField>(
                Field: SortField,
                Descending: IsSortDescending
            ),
            Paging: new PageOption(Page: CurrentPage, PageSize: PageSize)
        );
    }

    protected override void NotifySelectionCommandsCanExecuteChanged()
    {
        DeleteSelectedCommand.NotifyCanExecuteChanged();
    }

    protected override void ResetFilters()
    {
        // The extra filters must not reload on their own...
        using (SuppressReload())
        {
            ResetExtraFilters();
        }

        // ...base resets search + date, then reloads once with everything reset.
        base.ResetFilters();
    }

    public override async Task OnNavigatedToAsync()
    {
        await base.OnNavigatedToAsync();
        await LoadStatsAsync();
    }

    protected override async Task RefreshAsync()
    {
        await base.RefreshAsync();
        await LoadStatsAsync();
    }

    protected override async Task ReloadAfterRemovalAsync(
        IReadOnlyList<TItem> removed
    )
    {
        await base.ReloadAfterRemovalAsync(removed);
        await LoadStatsAsync();
    }

    // -------------------------------------------------------------------------
    // Stats
    // -------------------------------------------------------------------------

    // Filled by the derived class in LoadStatsAsync.
    public ObservableCollection<StatItem> Stats { get; } = [];

    // -------------------------------------------------------------------------
    // Commands
    // -------------------------------------------------------------------------

    // --- Navigation ----------------------------------------------------------

    [RelayCommand]
    private async Task NavigateToRecoveryAsync()
    {
        await OpenRecoveryAsync();
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        await OpenAddEditorAsync();
    }

    [RelayCommand]
    private async Task EditAsync(TItem? item)
    {
        if (item is not null)
        {
            await OpenEditEditorAsync(item);
        }
    }

    [RelayCommand]
    private async Task DuplicateAsync(TItem? item)
    {
        if (item is not null)
        {
            await OpenDuplicateEditorAsync(item);
        }
    }

    // --- Delete (move to trash) ----------------------------------------------

    [RelayCommand]
    private async Task DeleteAsync(TItem? item)
    {
        if (item is null)
        {
            return;
        }

        if (!await ConfirmDeleteAsync($"Move {EntityName} to trash?", [item]))
        {
            return;
        }

        await DeleteItemAsync(item.Id);
        await ReloadAfterRemovalAsync([item]);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DeleteSelectedAsync()
    {
        List<TItem> selected = GetSelectedItems();

        if (selected.Count == 0)
        {
            return;
        }

        if (
            !await ConfirmDeleteAsync(
                $"Move {selected.Count} {EntityNamePlural} to trash?",
                selected
            )
        )
        {
            return;
        }

        await DeleteItemsAsync([.. selected.Select(i => i.Id)]);
        await ReloadAfterRemovalAsync(selected);
    }

    // -------------------------------------------------------------------------
    // Extra filter methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Derived pages call this when one of their extra filters changes.
    /// </summary>
    protected void OnExtraFilterChanged()
    {
        RaiseFilterStateChanged();

        // No-op while a SuppressReload scope is alive (see ResetFilters).
        ReloadFromFirstPage();
    }

    /// <summary>
    /// Derived pages call this when only the "is filtering" state changed
    /// and no reload is needed.
    /// </summary>
    protected void RaiseFilterStateChanged()
    {
        OnPropertyChanged(nameof(IsFiltering));
        ResetFiltersCommand.NotifyCanExecuteChanged();
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private Task<bool> ConfirmDeleteAsync(
        string title,
        IReadOnlyList<TItem> items
    )
    {
        return DialogPresenter.ShowSafeFirstConfirmationAsync(
            title: title,
            message: BuildDeleteWarning(items),
            primaryButtonText: "Move to trash"
        );
    }
}
