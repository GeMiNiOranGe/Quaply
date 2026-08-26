namespace Quaply.Ui.Interfaces;

public interface IDialogPresenter
{
    Task<bool> ShowConfirmationAsync(
        string title,
        string message,
        string primaryButtonText = "Confirm",
        string closeButtonText = "Cancel"
    );

    Task<bool> ShowDangerConfirmationAsync(
        string title,
        string message,
        string primaryButtonText = "Confirm",
        string closeButtonText = "Cancel"
    );
}
