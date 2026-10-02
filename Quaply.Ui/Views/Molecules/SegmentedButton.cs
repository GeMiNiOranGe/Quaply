using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace Quaply.Ui.Views.Molecules;

/// <summary>
/// A row of 2-3 mutually exclusive buttons. For more options use ComboBox
/// or DropDownButton.
/// </summary>
public sealed class SegmentedButton : ItemsControl
{
    public const int MinItems = 2;
    public const int MaxItems = 3;

    public static readonly DependencyProperty SelectedValueProperty =
        DependencyProperty.Register(
            nameof(SelectedValue),
            typeof(object),
            typeof(SegmentedButton),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (d, _) => ((SegmentedButton)d).RefreshSegments()
            )
        );

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(
            nameof(Command),
            typeof(ICommand),
            typeof(SegmentedButton)
        );

    /// <summary>
    /// Appearance of segments that are NOT selected.
    /// </summary>
    public static readonly DependencyProperty AppearanceProperty =
        DependencyProperty.Register(
            nameof(Appearance),
            typeof(ControlAppearance),
            typeof(SegmentedButton),
            new PropertyMetadata(
                ControlAppearance.Transparent,
                (d, _) => ((SegmentedButton)d).RefreshSegments()
            )
        );

    /// <summary>
    /// Appearance of the selected segment.
    /// </summary>
    public static readonly DependencyProperty SelectedAppearanceProperty =
        DependencyProperty.Register(
            nameof(SelectedAppearance),
            typeof(ControlAppearance),
            typeof(SegmentedButton),
            new PropertyMetadata(
                ControlAppearance.Secondary,
                (d, _) => ((SegmentedButton)d).RefreshSegments()
            )
        );

    static SegmentedButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SegmentedButton),
            new FrameworkPropertyMetadata(typeof(SegmentedButton))
        );
    }

    public SegmentedButton()
    {
        AddHandler(
            ButtonBase.ClickEvent,
            new RoutedEventHandler(OnSegmentClick)
        );
        ItemContainerGenerator.StatusChanged += OnGeneratorStatusChanged;
        Loaded += (_, _) =>
            Debug.Assert(
                Items.Count is >= MinItems and <= MaxItems,
                $"{nameof(SegmentedButton)} expects {MinItems}-{MaxItems} items, got {Items.Count}."
            );
    }

    public object? SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    /// <summary>
    /// Executed with the clicked segment's value as parameter.
    /// </summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public ControlAppearance Appearance
    {
        get => (ControlAppearance)GetValue(AppearanceProperty);
        set => SetValue(AppearanceProperty, value);
    }

    public ControlAppearance SelectedAppearance
    {
        get => (ControlAppearance)GetValue(SelectedAppearanceProperty);
        set => SetValue(SelectedAppearanceProperty, value);
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is SegmentedButtonItem;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new SegmentedButtonItem();
    }

    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);

        if (Items.Count > MaxItems)
        {
            throw new InvalidOperationException(
                $"{nameof(SegmentedButton)} supports at most {MaxItems} items. "
                    + "Use a ComboBox or DropDownButton for more options."
            );
        }

        RefreshSegments();
    }

    private void OnGeneratorStatusChanged(object? sender, EventArgs e)
    {
        if (
            ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated
        )
        {
            RefreshSegments();
        }
    }

    private void OnSegmentClick(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not SegmentedButtonItem container)
        {
            return;
        }

        object item = ItemContainerGenerator.ItemFromContainer(container);
        if (item == DependencyProperty.UnsetValue)
        {
            return;
        }

        object? value = ValueOf(item);
        if (Equals(value, SelectedValue))
        {
            return;
        }

        SetCurrentValue(SelectedValueProperty, value);

        if (Command?.CanExecute(value) == true)
        {
            Command.Execute(value);
        }
    }

    private void RefreshSegments()
    {
        int count = Items.Count;

        for (int i = 0; i < count; i++)
        {
            if (
                ItemContainerGenerator.ContainerFromIndex(i)
                is not SegmentedButtonItem segment
            )
            {
                continue;
            }

            bool isSelected = Equals(ValueOf(Items[i]), SelectedValue);

            segment.Appearance = isSelected ? SelectedAppearance : Appearance;
            segment.CornerRadius =
                count == 1 ? new CornerRadius(4)
                : i == 0 ? new CornerRadius(4, 0, 0, 4)
                : i == count - 1 ? new CornerRadius(0, 4, 4, 0)
                : new CornerRadius(0);
        }
    }

    private static object? ValueOf(object item)
    {
        return item is SegmentedButtonItem s ? s.Value : item;
    }
}
