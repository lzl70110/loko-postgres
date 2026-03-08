// Loco1.Web/TagHelpers/LocTagHelper.cs
using Loco1.Localizer;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;

namespace Loco1.Web.TagHelpers
{
    /// <summary>
    /// Usage:
    ///   <loc key="Perm_Loco_Add" />
    ///   <loc key="@($"Group_{groupName}")" />
    ///   <loc key="InvalidRepairTypeFor" args="@(new object[]{ Model.Type })" />
    /// Resolution order: SharedResource -> ViewLocalizer -> key as-is (never breaks UI).
    /// </summary>
    [HtmlTargetElement("loc", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class LocTagHelper(IStringLocalizer<SharedResource> shared, IViewLocalizer view) : TagHelper
    {
        private readonly IStringLocalizer<SharedResource> _shared = shared;
        private readonly IViewLocalizer _view = view;

        [HtmlAttributeName("key")]
        public string Key { get; set; } = string.Empty;

        [HtmlAttributeName("args")]
        public object[]? Args { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null; // render text only (no <loc>)

            // 1) Try SharedResource (LocalizedString supports ResourceNotFound)
            var s = (Args == null || Args.Length == 0) ? _shared[Key] : _shared[Key, Args];
            if (!s.ResourceNotFound)
            {
                output.Content.SetContent(s.Value);
                return;
            }

            // 2) Try ViewLocalizer
            //    LocalizedHtmlString does NOT expose ResourceNotFound, so we compare by value.
            var v = (Args == null || Args.Length == 0) ? _view[Key] : _view[Key, Args];

            // If value differs from the key, we assume a translation exists.
            if (!string.Equals(v.Value, Key, StringComparison.Ordinal))
            {
                output.Content.SetContent(v.Value);
                return;
            }

            // 3) Final fallback: key itself (formatted if args present)
            output.Content.SetContent(Args is { Length: > 0 } ? string.Format(Key, Args) : Key);
        }
    }
}