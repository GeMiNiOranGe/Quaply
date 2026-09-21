namespace Quaply.Ui.Interfaces;

/// <summary>
/// Marks a row-wrapper model as checkbox-selectable, so
/// DataGridMultiSelectBehavior can sync it without knowing the concrete
/// item type of any specific page.
/// </summary>
public interface ISelectableItem
{
    bool IsSelected { get; set; }
}
