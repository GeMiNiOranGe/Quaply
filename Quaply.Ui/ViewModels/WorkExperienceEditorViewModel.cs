using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Quaply.Data.Models;
using Quaply.Service.Interfaces;
using Quaply.Ui.Interfaces;
using Quaply.Ui.Validations;
using Quaply.Ui.ViewModels.Base;
using Wpf.Ui.Controls;

namespace Quaply.Ui.ViewModels;

public partial class WorkExperienceEditorViewModel(
    INavigator navigator,
    IWorkExperienceService service,
    IDialogPresenter dialogPresenter
)
    : NavigableViewModel(navigator),
        INavigationAware<WorkExperienceEditorParameter>
{
    private const int DescriptionMaxLength = 1000;

    private sealed record WorkExperienceFormSnapshot(
        string CompanyName,
        string PositionTitle,
        string Description,
        DateOnly StartDate,
        DateOnly? EndDate,
        bool IsCurrentlyWorking
    );

    private readonly IWorkExperienceService _service = service;
    private readonly IDialogPresenter _dialogPresenter = dialogPresenter;

    // Only Edit needs to remember which row gets updated on Save.
    private int? _editingWorkExperienceId;

    private WorkExperienceFormSnapshot _baseline = EmptySnapshot();

    [ObservableProperty]
    public partial string PageTitle { get; private set; }

    [ObservableProperty]
    public partial string ResetButtonText { get; private set; }

    [ObservableProperty]
    public partial SymbolRegular ResetButtonIcon { get; private set; }

    [ObservableProperty]
    public partial string SaveButtonText { get; private set; }

    [ObservableProperty]
    public partial bool IsSaving { get; private set; }

    [NotifyCanExecuteChangedFor(nameof(ResetCommand))]
    [NotifyPropertyChangedFor(nameof(CanEditEndDate))]
    [ObservableProperty]
    public partial bool IsCurrentlyWorking { get; set; }

    public bool CanEditEndDate => !IsCurrentlyWorking;

    public int DescriptionCharacterCount => Description.Length;

    public string? DurationPreview =>
        BuildDurationPreview(StartDate, EndDate, IsCurrentlyWorking);

    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(ResetCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [MaxLength(
        255,
        ErrorMessage = "Company name must be at most 255 characters."
    )]
    [ObservableProperty]
    [Required(ErrorMessage = "Company name is required.")]
    public partial string CompanyName { get; set; } = string.Empty;

    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(ResetCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [MaxLength(
        255,
        ErrorMessage = "Position title must be at most 255 characters."
    )]
    [ObservableProperty]
    public partial string PositionTitle { get; set; } = string.Empty;

    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(ResetCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [MaxLength(
        DescriptionMaxLength,
        ErrorMessage = "Description must be at most 1000 characters."
    )]
    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;

    [NotifyCanExecuteChangedFor(nameof(ResetCommand))]
    [ObservableProperty]
    public partial DateOnly StartDate { get; set; }

    [DateNotBefore(
        nameof(StartDate),
        ErrorMessage = "End date cannot be earlier than start date."
    )]
    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(ResetCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [ObservableProperty]
    public partial DateOnly? EndDate { get; set; }

    partial void OnDescriptionChanged(string value)
    {
        OnPropertyChanged(nameof(DescriptionCharacterCount));
    }

    partial void OnStartDateChanged(DateOnly value)
    {
        OnPropertyChanged(nameof(DurationPreview));
        // Re-validate EndDate since its rule depends on StartDate.
        ValidateProperty(EndDate, nameof(EndDate));
    }

    partial void OnEndDateChanged(DateOnly? value)
    {
        OnPropertyChanged(nameof(DurationPreview));
    }

    partial void OnIsCurrentlyWorkingChanged(bool value)
    {
        if (value)
        {
            EndDate = null;
        }

        OnPropertyChanged(nameof(DurationPreview));
    }

    public async Task OnNavigatedToAsync(
        WorkExperienceEditorParameter parameter
    )
    {
        switch (parameter)
        {
            case WorkExperienceEditorParameter.Edit edit:
                await EnterEditModeAsync(edit);
                break;
            case WorkExperienceEditorParameter.Duplicate duplicate:
                await EnterDuplicateModeAsync(duplicate);
                break;
            case WorkExperienceEditorParameter.Add add:
                EnterAddMode(add);
                break;
            default:
                EnterAddMode(new());
                break;
        }
    }

    /// <summary>
    /// Current navigation parameter driving this editor's mode.
    /// Assigning this also derives <see cref="PageTitle"/> and
    /// <see cref="SaveButtonText"/> - do not set those two directly.
    /// </summary>
    private WorkExperienceEditorParameter EditorParameter
    {
        get;
        set
        {
            field = value;
            (PageTitle, ResetButtonText, ResetButtonIcon, SaveButtonText) =
                value switch
                {
                    WorkExperienceEditorParameter.Edit => (
                        "Edit Work Experience",
                        "Reset Changes",
                        SymbolRegular.ArrowReset24,
                        "Save Changes"
                    ),
                    WorkExperienceEditorParameter.Duplicate => (
                        "Duplicate Work Experience",
                        "Reset Changes",
                        SymbolRegular.ArrowReset24,
                        "Create Duplicate"
                    ),
                    WorkExperienceEditorParameter.Add or _ => (
                        "Add Work Experience",
                        "Clear",
                        SymbolRegular.Eraser24,
                        "Add Work Experience"
                    ),
                };
        }
    } = new WorkExperienceEditorParameter.Add();

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        ValidateAllProperties();

        if (HasErrors)
        {
            return;
        }

        IsSaving = true;

        try
        {
            if (_editingWorkExperienceId is int id)
            {
                WorkExperience? existing =
                    await _service.GetWorkExperienceByIdAsync(id);
                if (existing is null)
                {
                    return;
                }

                ApplyFormTo(existing);
                await _service.UpdateWorkExperienceAsync(existing);
            }
            else
            {
                // Covers both Add and Duplicate: always a new row.
                WorkExperience workExperience = new();
                ApplyFormTo(workExperience);
                await _service.CreateWorkExperienceAsync(workExperience);
            }

            await Navigator.NavigateToAsync<WorkExperienceViewModel>();
        }
        finally
        {
            IsSaving = false;
        }
    }

    private bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(CompanyName)
            && !HasErrors
            && !IsSaving;
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Navigator.NavigateToAsync<WorkExperienceViewModel>();
    }

    [RelayCommand(CanExecute = nameof(CanReset))]
    private async Task ResetAsync()
    {
        bool confirmed = await _dialogPresenter.ShowDangerConfirmationAsync(
            title: "Discard changes?",
            message: "Your unsaved changes will be lost. This action cannot be undone.",
            primaryButtonText: "Discard",
            closeButtonText: "Keep editing"
        );

        if (!confirmed)
        {
            return;
        }

        switch (EditorParameter)
        {
            case WorkExperienceEditorParameter.Edit edit:
                await ReloadFormFromAsync(edit.Id);
                break;
            case WorkExperienceEditorParameter.Duplicate duplicate:
                await ReloadFormFromAsync(duplicate.SourceId);
                break;
            case WorkExperienceEditorParameter.Add:
            default:
                ClearForm();
                ClearErrors();
                CaptureBaseline();
                break;
        }
    }

    private bool CanReset()
    {
        return !IsSaving && CurrentSnapshot() != _baseline;
    }

    private async Task ReloadFormFromAsync(int workExperienceId)
    {
        WorkExperience? workExperience =
            await _service.GetWorkExperienceByIdAsync(workExperienceId);
        if (workExperience is not null)
        {
            FillFormFrom(workExperience);
        }

        ClearErrors();
        CaptureBaseline();
    }

    private void EnterAddMode(WorkExperienceEditorParameter.Add add)
    {
        _editingWorkExperienceId = null;

        EditorParameter = add;
        ClearForm();
        ClearErrors();
        CaptureBaseline();
    }

    private async Task EnterEditModeAsync(
        WorkExperienceEditorParameter.Edit edit
    )
    {
        WorkExperience? workExperience =
            await _service.GetWorkExperienceByIdAsync(edit.Id);
        if (workExperience is null)
        {
            EnterAddMode(new WorkExperienceEditorParameter.Add());
            return;
        }

        _editingWorkExperienceId = edit.Id;

        EditorParameter = edit;
        FillFormFrom(workExperience);
        ClearErrors();
        CaptureBaseline();
    }

    private async Task EnterDuplicateModeAsync(
        WorkExperienceEditorParameter.Duplicate duplicate
    )
    {
        WorkExperience? source = await _service.GetWorkExperienceByIdAsync(
            duplicate.SourceId
        );
        if (source is null)
        {
            EnterAddMode(new WorkExperienceEditorParameter.Add());
            return;
        }

        _editingWorkExperienceId = null;

        EditorParameter = duplicate;
        FillFormFrom(source);
        ClearErrors();
        CaptureBaseline();
    }

    private void ClearForm()
    {
        CompanyName = string.Empty;
        PositionTitle = string.Empty;
        Description = string.Empty;
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow);
        EndDate = null;
        IsCurrentlyWorking = false;
    }

    private void FillFormFrom(WorkExperience workExperience)
    {
        CompanyName = workExperience.CompanyName;
        PositionTitle = workExperience.PositionTitle ?? string.Empty;
        Description = workExperience.Description ?? string.Empty;
        StartDate = workExperience.StartDate;
        EndDate = workExperience.EndDate;
        IsCurrentlyWorking = workExperience.EndDate is null;
    }

    private void ApplyFormTo(WorkExperience workExperience)
    {
        workExperience.CompanyName = CompanyName.Trim();
        workExperience.PositionTitle = string.IsNullOrWhiteSpace(PositionTitle)
            ? null
            : PositionTitle.Trim();
        workExperience.Description = string.IsNullOrWhiteSpace(Description)
            ? null
            : Description.Trim();
        workExperience.StartDate = StartDate;
        workExperience.EndDate = IsCurrentlyWorking ? null : EndDate;
    }

    private void CaptureBaseline()
    {
        _baseline = CurrentSnapshot();
        ResetCommand.NotifyCanExecuteChanged();
    }

    private WorkExperienceFormSnapshot CurrentSnapshot()
    {
        return new(
            CompanyName,
            PositionTitle,
            Description,
            StartDate,
            EndDate,
            IsCurrentlyWorking
        );
    }

    private static WorkExperienceFormSnapshot EmptySnapshot()
    {
        return new(
            string.Empty,
            string.Empty,
            string.Empty,
            DateOnly.FromDateTime(DateTime.UtcNow),
            null,
            false
        );
    }

    private static string? BuildDurationPreview(
        DateOnly start,
        DateOnly? end,
        bool isCurrent
    )
    {
        DateOnly resolvedEnd =
            isCurrent || end is null
                ? DateOnly.FromDateTime(DateTime.Today)
                : end.Value;

        if (resolvedEnd < start)
        {
            return null;
        }

        int totalMonths =
            ((resolvedEnd.Year - start.Year) * 12)
            + resolvedEnd.Month
            - start.Month;
        int years = totalMonths / 12;
        int months = totalMonths % 12;

        return (years, months) switch
        {
            (0, 0) => "Less than a month",
            (0, _) => $"{months} month{(months > 1 ? "s" : "")}",
            (_, 0) => $"{years} year{(years > 1 ? "s" : "")}",
            _ =>
                $"{years} year{(years > 1 ? "s" : "")} {months} month{(months > 1 ? "s" : "")}",
        };
    }
}
