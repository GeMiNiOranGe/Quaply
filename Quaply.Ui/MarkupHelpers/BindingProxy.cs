using System.Windows;

namespace Quaply.Ui.MarkupHelpers;

/// <summary>
/// Bridges a binding source (the DataContext or a single value) to elements
/// that live OUTSIDE the visual tree.
/// </summary>
///
/// <remarks>
/// <para><b>WHY:</b></para>
/// <para>
/// ContextMenu, Popup and DataGridColumn (Header, Visibility, Width, ...)
/// are not part of the UserControl's visual tree, so ElementName and
/// RelativeSource AncestorType cannot find anything. A Freezable declared
/// in Resources inherits the DataContext and can hold a binding of its own,
/// so it works as a bridge: bind the source into <c>Data</c> where the binding
/// works, then read <c>Data</c> from where it does not.
/// </para>
///
/// <para><b>TWO FLAVORS:</b></para>
/// <para><b><i>DataContext flavor:</i></b></para>
/// <c>Data</c> is the ViewModel.
/// Use it for Commands/properties from ContextMenu, Popup, etc.
/// Key convention: <c>MyViewModelProxy</c>.
/// <list type="number">
///   <item>
///   Declare it in the UserControl's own Resources (NOT in App.xaml
///   or a shared ResourceDictionary, otherwise it loses the correct DataContext):
/// <code>
///     &lt;markup:BindingProxy x:Key="MyViewModelProxy" Data="{Binding}" /&gt;
/// </code>
///   </item>
///   <item>
///   Bind a command through it:
/// <code>
///     &lt;YourElement.Command&gt;
///       &lt;Binding
///         Path="Data.DeleteCommand"
///         Source="{StaticResource MyViewModelProxy}"
///       /&gt;
///     &lt;/YourElement.Command&gt;
/// </code>
///   </item>
///   <item>
///   Keep CommandParameter as a plain {Binding}. ContextMenu inherits
///   its DataContext from PlacementTarget, which is the row item.
///   </item>
/// </list>
///
/// <para><b><i>Value flavor:</i></b></para>
/// <c>Data</c> is a single value from a named element,
/// used for properties outside the visual tree (e.g. DataGridColumn.Width).
/// Key convention: name it after the value (e.g. <c>MyProxy</c>).
/// <list type="number">
///   <item>
///   Declare it in the Resources of a parent element (e.g. Grid.Resources),
///   pointing at a named element:
/// <code>
///     &lt;markup:BindingProxy
///       x:Key="MyProxy"
///       Data="{Binding ActualWidth, ElementName=yourElementName}"
///     /&gt;
/// </code>
///   </item>
///   <item>
///   Read it, usually through a converter:
/// <code>
///     &lt;YourElement.Width&gt;
///       &lt;Binding
///         Path="Data"
///         Source="{StaticResource MyProxy}"
///         Converter="{StaticResource YourConverter}"
///         ConverterParameter="0.4"
///       /&gt;
///     &lt;/YourElement.Width&gt;
/// </code>
///   </item>
/// </list>
///
/// <para><b>GOTCHAS:</b></para>
/// <list type="number">
///   <item>
///     <b>Both flavors</b>
///     <list type="bullet">
///       <item>
///       Binding paths are strings, so there is no compile-time check.
///       If a binding silently fails, check the Output window.
///       </item>
///       <item>
///       With nested UserControls, give each proxy its own key
///       (e.g. ExperienceProxy).
///       </item>
///       <item>
///       CreateInstanceCore must be overridden because this class derives
///       from Freezable.
///       </item>
///     </list>
///   </item>
///
///   <item>
///     <b>DataContext flavor</b>
///     <list type="bullet">
///       <item>
///       Data is the DataContext (ViewModel), not the UserControl itself.
///       </item>
///     </list>
///   </item>
///
///   <item>
///     <b>Value flavor</b>
///     <list type="bullet">
///       <item>
///       The source element still needs x:Name (ElementName requires it).
///       </item>
///       <item>
///       Data is a boxed value. ActualWidth is 0 before the first layout pass,
///       so the converter must handle 0 (e.g. fall back to DataGridLength.Auto).
///       </item>
///       <item>
///       DataGridColumn.Width is DataGridLength, not double, so a converter
///       is required.
///       </item>
///       <item>
///       ActualWidth includes the vertical scrollbar; subtract it if columns
///       should fit exactly.
///       </item>
///     </list>
///   </item>
/// </list>
/// </remarks>
public class BindingProxy : Freezable
{
    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(
            nameof(Data),
            typeof(object),
            typeof(BindingProxy),
            new UIPropertyMetadata(null)
        );

    public object Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    // Mandatory override - Freezable requires this method to clone itself.
    protected override Freezable CreateInstanceCore()
    {
        return new BindingProxy();
    }
}
