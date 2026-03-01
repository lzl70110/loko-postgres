using Microsoft.Extensions.Localization;

namespace Loco1.Localizer
    {
    // Marker class for SharedResource-only localization
    public class SharedResource { }

    public static class LocalizerExtensions
        {
        public static LocalizedString F(this IStringLocalizer localizer, string key)
            {
            var value = localizer[key];
            return value.ResourceNotFound
                ? new LocalizedString(key, key, true)
                : value;
            }

        public static LocalizedString F(this IStringLocalizer localizer, string key, params object[] args)
            {
            var value = localizer[key, args];
            return value.ResourceNotFound
                ? new LocalizedString(key, string.Format(key, args), true)
                : value;
            }
        }
    }