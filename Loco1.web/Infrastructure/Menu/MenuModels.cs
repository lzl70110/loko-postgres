namespace Loco1.Web.Infrastructure.Menu
{
    // DTO used by Razor views; no inheritance expected
    public sealed class PermissionMenuItem
    {
        public string Code { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public bool Granted { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public sealed class PermissionMenuGroup
    {
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public List<PermissionMenuItem> Items { get; set; } = new();
    }
}
