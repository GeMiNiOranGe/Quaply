namespace Quaply.Ui.Interfaces;

/// <summary>
/// Contract for a row shown in a trash (soft-deleted) list.
/// Implemented by Deleted*Item wrappers such as DeletedProfileItem.
/// </summary>
public interface IRecoveryItem : ISelectableItem
{
    /// <summary>
    /// Human-readable name used in confirmation dialogs (e.g. FullName, CompanyName).
    /// </summary>
    string DisplayName { get; }

    int Id { get; }
}
