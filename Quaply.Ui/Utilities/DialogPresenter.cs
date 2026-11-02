using Quaply.Ui.Interfaces;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace Quaply.Ui.Utilities;

public sealed class DialogPresenter(IContentDialogService contentDialogService)
    : IDialogPresenter
{
    private readonly IContentDialogService _contentDialogService =
        contentDialogService;

    public async Task<bool> ShowConfirmationAsync(
        string title,
        string message,
        string primaryButtonText = "Confirm",
        string closeButtonText = "Cancel",
        ContentDialogButton defaultButton = ContentDialogButton.Primary
    )
    {
        ContentDialog dialog = new()
        {
            Title = title,
            Content = message,
            PrimaryButtonText = primaryButtonText,
            CloseButtonText = closeButtonText,

            // Always highlight the primary button, regardless of
            // where the Enter key is focused.
            PrimaryButtonAppearance = ControlAppearance.Primary,
            CloseButtonAppearance = ControlAppearance.Secondary,

            // The button that receives the Enter key.
            DefaultButton = defaultButton,
        };

        ContentDialogResult result = await _contentDialogService.ShowAsync(
            dialog,
            CancellationToken.None
        );

        return result == ContentDialogResult.Primary;
    }

    public async Task<bool> ShowDangerConfirmationAsync(
        string title,
        string message,
        string primaryButtonText = "Confirm",
        string closeButtonText = "Cancel"
    )
    {
        ContentDialog dialog = new()
        {
            Title = title,
            Content = message,
            PrimaryButtonText = primaryButtonText,
            CloseButtonText = closeButtonText,

            // Change the accent colors for "Close" and "Primary" to a neutral color.
            PrimaryButtonAppearance = ControlAppearance.Secondary,
            CloseButtonAppearance = ControlAppearance.Primary,

            // Default to "Close" to prevent dangerous actions.
            DefaultButton = ContentDialogButton.Close,
        };

        ContentDialogResult result = await _contentDialogService.ShowAsync(
            dialog,
            CancellationToken.None
        );

        return result == ContentDialogResult.Primary;
    }
}
