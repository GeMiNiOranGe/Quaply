using Wpf.Ui.Controls;

namespace Quaply.Ui.Interfaces;

public interface IDialogPresenter
{
    /// <summary>
    /// Shows a standard confirmation dialog. The Primary (confirm) button is
    /// visually emphasized, while the Close (cancel) button is shown in a
    /// secondary style. Which button receives the Enter key is controlled by
    /// <paramref name="defaultButton"/>.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="message">The message shown in the dialog body.</param>
    /// <param name="primaryButtonText">The text of the confirm button.</param>
    /// <param name="closeButtonText">The text of the cancel button.</param>
    /// <param name="defaultButton">
    /// The button that receives the Enter key. Defaults to
    /// <see cref="ContentDialogButton.Primary"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the user confirmed;
    /// otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// Use this for routine confirmations. For actions that are hard to undo,
    /// prefer <see cref="ShowExplicitConfirmationAsync"/> (confirm button keeps
    /// its primary look, but Enter triggers cancel) or
    /// <see cref="ShowSafeFirstConfirmationAsync"/> (cancel button is emphasized
    /// and receives Enter).
    /// </remarks>
    Task<bool> ShowConfirmationAsync(
        string title,
        string message,
        string primaryButtonText = "Confirm",
        string closeButtonText = "Cancel"
    );

    /// <summary>
    /// Shows an explicit confirmation dialog. The Primary (confirm) button keeps
    /// its accent highlight, but pressing Enter triggers the Close (cancel) button
    /// instead. The user must deliberately click or Tab to the confirm button,
    /// so an accidental key press cannot confirm the action.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="message">The message shown in the dialog body.</param>
    /// <param name="primaryButtonText">The text of the confirm button.</param>
    /// <param name="closeButtonText">The text of the cancel button.</param>
    /// <returns>
    /// <see langword="true"/> if the user explicitly confirmed;
    /// otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// Use this for actions that are hard to undo but where the confirm button
    /// should still look like the primary action. If the cancel path should also
    /// be visually emphasized, use <see cref="ShowSafeFirstConfirmationAsync"/>.
    /// </remarks>
    Task<bool> ShowExplicitConfirmationAsync(
        string title,
        string message,
        string primaryButtonText = "Confirm",
        string closeButtonText = "Cancel"
    );

    /// <summary>
    /// Shows a safety-first confirmation dialog. The Close (cancel) button is
    /// visually emphasized and also receives the Enter key, while the Primary
    /// (confirm) button is shown in a neutral style. This steers the user toward
    /// backing out of the action.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="message">The message shown in the dialog body.</param>
    /// <param name="primaryButtonText">The text of the confirm button.</param>
    /// <param name="closeButtonText">The text of the cancel button.</param>
    /// <returns>
    /// <see langword="true"/> if the user confirmed;
    /// otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// Use this for destructive or irreversible actions (for example, deleting
    /// data). If the confirm button should keep its primary look, use
    /// <see cref="ShowExplicitConfirmationAsync"/>.
    /// </remarks>
    Task<bool> ShowSafeFirstConfirmationAsync(
        string title,
        string message,
        string primaryButtonText = "Confirm",
        string closeButtonText = "Cancel"
    );
}
