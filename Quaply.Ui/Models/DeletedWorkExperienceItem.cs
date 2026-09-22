using CommunityToolkit.Mvvm.ComponentModel;
using Quaply.Data.Models;
using Quaply.Ui.Interfaces;

namespace Quaply.Ui.Models;

/// <summary>
/// Wraps a soft-deleted WorkExperience with UI-only state (selection,
/// purge countdown) without polluting the EF entity itself.
/// </summary>
public partial class DeletedWorkExperienceItem(WorkExperience workExperience)
    : ObservableObject,
        ISelectableItem
{
    public WorkExperience WorkExperience { get; } = workExperience;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    public string CompanyName => WorkExperience.CompanyName;

    public string PositionTitle => WorkExperience.PositionTitle;

    public string? Description => WorkExperience.Description;

    public DateOnly? EndDate => WorkExperience.EndDate;

    public DateTime? DeletedAt => WorkExperience.DeletedAt;
}
