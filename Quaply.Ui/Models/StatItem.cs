using Wpf.Ui.Controls;

namespace Quaply.Ui.Models;

public record StatItem(
    SymbolRegular Icon,
    string Label,
    string Value,
    string SubLabel
);
