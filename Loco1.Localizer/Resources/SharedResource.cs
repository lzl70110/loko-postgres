using Microsoft.Extensions.Localization;

namespace Loco1.Localizer
    {
    // Marker class for SharedResource-only localization
    public class SharedResource { }

    // EN: Extension that returns the key itself if it's missing in .resx
    public static class LocalizerExtensions
        {
        public static LocalizedString F(this IStringLocalizer localizer, string key)
            {
            var value = localizer[key];
            // If not found => fallback to the key text (no more "missing key" worries)
            return value.ResourceNotFound
                ? new LocalizedString(key, key, true)
                : value;
            }

        // EN: Overload with formatting args, still with fallback behavior
        public static LocalizedString F(this IStringLocalizer localizer, string key, params object[] args)
            {
            var value = localizer[key, args];
            return value.ResourceNotFound
                ? new LocalizedString(key, string.Format(key, args), true)
                : value;
            }
        }
    }