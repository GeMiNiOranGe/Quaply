namespace Quaply.Ui.Interfaces;

/// <summary>
/// Contract for a row shown in an index view.
/// Implemented by *Item wrappers such as ProfileItem, WorkExperienceItem, ...
/// </summary>
public interface IIndexItem : ISelectableItem
{
    int Id { get; }
}
