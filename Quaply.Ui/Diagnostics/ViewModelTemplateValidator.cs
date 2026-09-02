using System.Windows;
using Quaply.Ui.Utilities;
using Quaply.Ui.ViewModels;
using Quaply.Ui.ViewModels.Base;

namespace Quaply.Ui.Diagnostics;

public static class ViewModelTemplateValidator
{
    private static readonly Type[] ExcludedFromTemplateValidation =
    [
        typeof(MainViewModel),
        typeof(HostNavigator),
    ];

    public static void ValidateAll(ResourceDictionary resources)
    {
        List<Type> missing = [];
        IEnumerable<Type> viewModelTypes = GetAllViewModelTypes();

        foreach (var vmType in viewModelTypes)
        {
            bool found = resources
                .Values.OfType<DataTemplate>()
                .Any(dt => dt.DataType is Type dataType && dataType == vmType);

            if (!found)
            {
                missing.Add(vmType);
            }
        }

        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                $"Missing DataTemplate for: {string.Join(", ", missing.Select(t => t.Name))}"
            );
        }
    }

    private static IEnumerable<Type> GetAllViewModelTypes()
    {
        return typeof(App)
            .Assembly.GetTypes()
            .Where(t => typeof(ViewModel).IsAssignableFrom(t) && !t.IsAbstract)
            .Except(ExcludedFromTemplateValidation);
    }
}
