using Quaply.Ui.Interfaces;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace Quaply.Ui.Utilities;

public sealed class DialogPresenter(IContentDialogService contentDialogService)
    : IDialogPresenter
{
    private readonly IContentDialogService _contentDialogService =
        contentDialogService;

    public Task<bool> ShowConfirmationAsync(
        string title,
        string message,
        string primaryButtonText = "Confirm",
        string closeButtonText = "Cancel"
    )
    {
        return ShowConfirmationCoreAsync(
            title,
            message,
            primaryButtonText,
            closeButtonText,
            primaryAppearance: ControlAppearance.Primary,
            closeAppearance: ControlAppearance.Secondary,
            defaultButton: ContentDialogButton.Primary
        );
    }

    public Task<bool> ShowExplicitConfirmationAsync(
        string title,
        string message,
        string primaryButtonText,
        string closeButtonText = "Cancel"
    )
    {
        return ShowConfirmationCoreAsync(
            title,
            message,
            primaryButtonText,
            closeButtonText,
            primaryAppearance: ControlAppearance.Primary,
            closeAppearance: ControlAppearance.Secondary,
            defaultButton: ContentDialogButton.Close
        );
    }

    public Task<bool> ShowSafeFirstConfirmationAsync(
        string title,
        string message,
        string primaryButtonText = "Confirm",
        string closeButtonText = "Cancel"
    )
    {
        return ShowConfirmationCoreAsync(
            title,
            message,
            primaryButtonText,
            closeButtonText,
            primaryAppearance: ControlAppearance.Secondary,
            closeAppearance: ControlAppearance.Primary,
            defaultButton: ContentDialogButton.Close
        );
    }

    private async Task<bool> ShowConfirmationCoreAsync(
        string title,
        string message,
        string primaryButtonText,
        string closeButtonText,
        ControlAppearance primaryAppearance,
        ControlAppearance closeAppearance,
        ContentDialogButton defaultButton
    )
    {
        ContentDialog dialog = new()
        {
            Title = title,
            Content = message,
            PrimaryButtonText = primaryButtonText,
            CloseButtonText = closeButtonText,

            PrimaryButtonAppearance = primaryAppearance,
            CloseButtonAppearance = closeAppearance,

            // The button that receives the Enter key.
            DefaultButton = defaultButton,
        };

        ContentDialogResult result = await _contentDialogService.ShowAsync(
            dialog,
            CancellationToken.None
        );

        return result == ContentDialogResult.Primary;
    }
}
