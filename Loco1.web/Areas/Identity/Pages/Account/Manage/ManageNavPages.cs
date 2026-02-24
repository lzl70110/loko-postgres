// File: Areas/Identity/Pages/Account/Manage/ManageNavPages.cs
namespace Loco1.Web.Areas.Identity.Pages.Account.Manage;

using Microsoft.AspNetCore.Mvc.Rendering;

public static class ManageNavPages
    {
    public static string Index => "Index";
    public static string ChangePassword => "ChangePassword";

    public static string IndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, Index);
    public static string ChangePasswordNavClass(ViewContext viewContext) => PageNavClass(viewContext, ChangePassword);

    public static string PageNavClass(ViewContext viewContext, string page)
        {
        var activePage = viewContext.ViewData["ActivePage"] as string
            ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
        return string.Equals(activePage, page, System.StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }