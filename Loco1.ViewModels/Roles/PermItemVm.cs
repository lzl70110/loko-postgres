namespace Loco1.ViewModels.Roles;

// EN: Permission item in a group (checkbox UI).
public sealed class PermItemVm
{
    public string Code { get; set; } = string.Empty;    // permission code
    public string Display { get; set; } = string.Empty; // UI text
    public bool Granted { get; set; }                   // checkbox state
}