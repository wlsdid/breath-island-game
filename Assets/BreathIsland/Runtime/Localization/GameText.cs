using System;
using UnityEngine.Localization.Settings;
namespace BreathIsland
{
    public static class GameText
    {
        public const string Table = "GameText";
        public static string Get(string key, bool korean)
        {
            LocalizationSettings.InitializationOperation.WaitForCompletion();
            var locale = LocalizationSettings.AvailableLocales.GetLocale(korean ? "ko" : "en");
            if (locale == null) throw new InvalidOperationException("Missing game locale.");
            return LocalizationSettings.StringDatabase.GetLocalizedString(Table, key, locale);
        }
    }
}
