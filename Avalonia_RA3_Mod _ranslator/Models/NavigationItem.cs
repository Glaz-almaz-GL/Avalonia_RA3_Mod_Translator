using Avalonia.Controls;
using FluentIcons.Common;
using Huskui.Avalonia.Controls;
using System.Windows.Input;

namespace Avalonia_RA3_Mod__ranslator.Models;

public class NavigationItem()
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Symbol Icon { get; set; }
    public bool IsNew { get; set; }
    public bool IsUpdated { get; set; }
    public ICommand? Command { get; set; }
    public Page? NavigationPage { get; set; }
}
