using Wpf.Ui.Controls;

namespace Quaply.Ui.Interfaces;

public interface IDialogPresenter
{
    Task<bool> ShowConfirmationAsync(
        string title,
        string message,
        string primaryButtonText = "Confirm",
        string closeButtonText = "Cancel"
    );

    Task<bool> ShowExplicitConfirmationAsync(
        string title,
        string message,
        string primaryButtonText = "Confirm",
        string closeButtonText = "Cancel"
    );

    /// <summary>
    /// Safety-first confirmation: the cancel path is emphasized and receives
    /// Enter by default, so accidental key presses do not trigger the action.
    /// </summary>
    Task<bool> ShowSafeFirstConfirmationAsync(
        string title,
        string message,
        string primaryButtonText = "Confirm",
        string closeButtonText = "Cancel"
    );
}
