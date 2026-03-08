using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Localization;

namespace Loco1.Web.Infrastructure;

// EN: Helper to safely put localized strings into TempData
public static class TempDataExtensions
{
    public static void Set(this ITempDataDictionary temp, string key, LocalizedString text)
        => temp[key] = text.Value;
}