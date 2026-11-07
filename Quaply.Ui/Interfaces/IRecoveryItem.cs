namespace Quaply.Ui.Interfaces;

/// <summary>
/// Contract for a row shown in a recovery view.
/// Implemented by *RecoveryItem wrappers such as ProfileRecoveryItem.
/// </summary>
public interface IRecoveryItem : ISelectableItem
{
    /// <summary>
    /// Human-readable name used in confirmation dialogs (e.g. FullName,
    /// CompanyName, ...).
    /// </summary>
    string DisplayName { get; }

    int Id { get; }
}
