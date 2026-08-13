using Quaply.Ui.Interfaces;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

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
        string closeButtonText = "Cancel"
    )
    {
        ContentDialogResult result =
            await _contentDialogService.ShowSimpleDialogAsync(
                new SimpleContentDialogCreateOptions
                {
                    Title = title,
                    Content = message,
                    PrimaryButtonText = primaryButtonText,
                    CloseButtonText = closeButtonText,
                }
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
