#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Xml;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using Newtonsoft.Json;
using UnityEngine.Localization.Tables;
using NUnit.Framework;

namespace TinyWalnutGames.Localization
{
#if UNITY_LOCALIZATION && !UNITY_WEBGL
    /// <summary>
    /// Helper class for localization in Gather Craft Market.
    /// Handles loading and retrieving localized strings from JSON tables (WebGL) or Unity.Localization (other platforms).
    /// </summary>
    public static class LocalizationHelper
    {
        public static event Action LocaleChanged;

        [Serializable]
        public class LanguageChoice
        {
            public string Key; // Locale code, e.g. "en", "zh-Hans"
            public double Id;  // Not used outside WebGL/JSON, but kept for compatibility
            public string NativeName; // The language name in its own language
        }

        private static List<LanguageChoice> _languageChoices;
        private static List<string> _availableLocales;
        private static readonly Dictionary<(string table, string locale), LocalizationTable> _localizationTables = new();
        private static Dictionary<string, FontAsset> _fontByLocale = new(); // Optimized: font lookup by locale
        private static bool _isLoaded = false; // Tracks if tables are assigned

        private const string PlayerPrefsLocaleKey = "SelectedLocale";

        public static List<LanguageChoice> GetLanguageChoices()
        {
            if (_languageChoices == null)
            {
                _languageChoices = new List<LanguageChoice>();
                var locales = LocalizationSettings.AvailableLocales.Locales;
                foreach (var locale in locales)
                {
                    string localeCode = locale.Identifier.Code;
                    string nativeName = GetLocalizedStringForLocale("GCML_LanguageChoices", localeCode, locale, localeCode);
                    _languageChoices.Add(new LanguageChoice
                    {
                        Key = localeCode,
                        NativeName = nativeName
                    });
                }
            }
            return _languageChoices;
        }

        public static List<string> GetAvailableLocales()
        {
            var locales = LocalizationSettings.AvailableLocales.Locales;
            var result = new List<string>(locales.Count);
            foreach (var locale in locales)
                result.Add(locale.Identifier.Code);
            return result;
        }

        public static string GetLocalizedString(string table, string key, string fallback = "en")
        {
            if (string.IsNullOrEmpty(key))
                return fallback;

            if (LocalizationSettings.SelectedLocale != null)
            {
                var localized = LocalizationSettings.StringDatabase.GetLocalizedString(table, key);
                if (!string.IsNullOrEmpty(localized))
                    return localized;
            }
            return fallback;
        }

        public static string GetLocalizedStringForLocale(string table, string key, Locale locale, string fallback = "en")
        {
            var originalLocale = LocalizationSettings.SelectedLocale;
            LocalizationSettings.SelectedLocale = locale;
            var localized = LocalizationSettings.StringDatabase.GetLocalizedString(table, key);
            LocalizationSettings.SelectedLocale = originalLocale;
            return !string.IsNullOrEmpty(localized) ? localized : fallback;
        }

        public static IEnumerator PreloadAllLocalesAsync()
        {
            var preloadOp = LocalizationSettings.InitializationOperation;
            while (!preloadOp.IsDone)
                yield return null;
        }

        public static void InitializeLocale()
        {
            string savedLocale = LoadLocaleFromPrefs();
            if (!string.IsNullOrEmpty(savedLocale) && GetAvailableLocales().Contains(savedLocale))
            {
                SetLocale(savedLocale);
            }
            else
            {
                string systemLocale = GetSystemLocale();
                if (GetAvailableLocales().Contains(systemLocale))
                {
                    SetLocale(systemLocale);
                    SaveLocaleToPrefs(systemLocale);
                }
                else
                {
                    SetLocale("en");
                    SaveLocaleToPrefs("en");
                }
            }
        }

        /// <summary>
        /// Assigns localization tables and fonts to the helper. Call this once at startup.
        /// </summary>
        /// <param name="tables">Array of LocalizationTable, one per supported locale.</param>
        /// <param name="fontByLocale">Dictionary mapping locale codes to Fonts.</param>
        public static void AssignLocalizationTables(LocalizationTable[] tables, Dictionary<string, FontAsset> fontByLocale = null)
        {
            _localizationTables.Clear();
            if (tables != null)
            {
                foreach (var table in tables)
                {
                    if (table != null)
                    {
                        // Expect table names like "tooltips_en", "ui_zh-Hans"
                        var nameParts = table.name.Split('_');
                        if (nameParts.Length >= 2)
                        {
                            var tableName = string.Join("_", nameParts, 0, nameParts.Length - 1);
                            var locale = nameParts[^1];
                            _localizationTables[(tableName, locale)] = table;
                        }
                    }
                }
            }
            _fontByLocale = fontByLocale != null ? new Dictionary<string, FontAsset>(fontByLocale) : new Dictionary<string, FontAsset>();
            _isLoaded = true;
            _languageChoices = null;
            _availableLocales = null;
        }

        public static void SaveLocaleToPrefs(string localeCode)
        {
            PlayerPrefs.SetString(PlayerPrefsLocaleKey, localeCode);
            PlayerPrefs.Save();
        }

        public static string LoadLocaleFromPrefs()
        {
            return PlayerPrefs.GetString(PlayerPrefsLocaleKey, null);
        }

        public static void ResetLocaleToSystemDefault()
        {
            string systemLocale = GetSystemLocale();
            if (GetAvailableLocales().Contains(systemLocale))
            {
                SetLocale(systemLocale);
                SaveLocaleToPrefs(systemLocale);
            }
            else
            {
                SetLocale("en");
                SaveLocaleToPrefs("en");
            }
        }

        /// <summary>
        /// Gets the system locale based on the current platform.
        /// </summary>
        /// <returns></returns>
        public static string GetSystemLocale()
        {
            SystemLanguage lang = Application.systemLanguage;
            return lang switch
            {
                // You may add more cases here for other languages as needed
                SystemLanguage.English => "en",
                SystemLanguage.ChineseSimplified => "zh-Hans",
                // example: SystemLanguage.ChineseTraditional => "zh-Hant",
                // SystemLanguage.French => "fr",
                // SystemLanguage.Spanish => "es",
                // SystemLanguage.German => "de",
                // SystemLanguage.Italian => "it",
                // SystemLanguage.Japanese => "ja",
                // SystemLanguage.Korean => "ko",
                // SystemLanguage.Portuguese => "pt",
                // SystemLanguage.Russian => "ru",
                // SystemLanguage.Arabic => "vi",
                // RTL // SystemLanguage.Arabic => "ar",
                // RTL // SystemLanguage.Hebrew => "he",
                // RTL // SystemLanguage.Persian => "fa",
                // RTL // SystemLanguage.Urdu => "ur",
                _ => "en", // Default to English
            };
        }

        public static void SetLocale(string localeCode)
        {
            var locales = LocalizationSettings.AvailableLocales.Locales;
            foreach (var locale in locales)
            {
                if (locale.Identifier.Code == localeCode)
                {
                    if (LocalizationSettings.SelectedLocale != locale)
                    {
                        LocalizationSettings.SelectedLocale = locale;
                        SaveLocaleToPrefs(localeCode);
                        LocaleChanged?.Invoke();
                    }
                    break;
                }
            }
        }

        public static string GetCurrentLocaleCode()
        {
            if (LocalizationSettings.SelectedLocale == null)
            {
                return "en";
            }
            else
            {
                return LocalizationSettings.SelectedLocale.Identifier.Code;
            }
        }

        // UnityLocalization does not require manual font assignment, but you can still retrieve fonts if needed.
        public static FontAsset GetFontForLocale(string localeCode)
        {
            if (!string.IsNullOrEmpty(localeCode) && _fontByLocale != null && _fontByLocale.TryGetValue(localeCode, out var font) && font != null)
                return font;
            foreach (var f in _fontByLocale.Values)
            {
                if (f != null)
                    return f;
            }
            return null; // Return null if no font found for the locale
        }

        // UnityLocalization does not require manual font assignment, but you can still retrieve the font for the current locale.
        public static FontAsset GetFontForCurrentLocale()
        {
            if (LocalizationSettings.SelectedLocale == null)
                return null;
            string localeCode = LocalizationSettings.SelectedLocale.Identifier.Code;
            return GetFontForLocale(localeCode);
        }

        // UnityLocalization does not require manual sprite loading, but you can implement a similar method if needed.
        public static Sprite LoadLocaleSprite(string fallbackLocale = "en")
        {
            // Unity.Localization does not use Resources.Load for sprites, so this is a placeholder.
            // You can implement your own logic if you have sprites stored in Resources.
            return null; // Return null as Unity.Localization does not handle sprites this way
        }
    }

#else // UNITY_WEBGL
    /// <summary>
    /// Helper class for localization.
    /// Handles loading and retrieving localized strings from assigned LocalizationTable components.
    /// Also manages font assignment per locale.
    /// </summary>
    public static class LocalizationHelper
    {
        [Serializable]
        public class LanguageChoice
        {
            public string Key; // Locale code, e.g. "en", "zh-Hans"
            public double Id;  // Not used outside WebGL/JSON, but kept for compatibility
            public string NativeName; // The language name in its own language
        }

        private static List<LanguageChoice> _languageChoices;
        private static List<string> _availableLocales;
        private readonly static Dictionary<(string table, string locale), LocalizationTable> _localizationTables = new();
        private readonly static Dictionary<string, FontAsset> _fontByLocale = new(); // Optimized: font lookup by locale
        private static string _currentLocale = "en";
        public static event Action LocaleChanged;
        private static bool _isLoaded = false;

        private const string PlayerPrefsLocaleKey = "SelectedLocale";

        public static IEnumerator PreloadAllLocalesAsync()
        {
            EnsureTablesLoaded();
            yield return null;
        }

        public static void AssignLocalizationTablesWebGL(LocalizationTable[] tables)
        {
            _localizationTables.Clear();
            if (tables != null)
            {
                foreach (var table in tables)
                {
                    if (table != null)
                    {
                        // Expect table names like "tooltips_en", "ui_zh-Hans"
                        var nameParts = table.name.Split('_');
                        if (nameParts.Length >= 2)
                        {
                            var tableName = string.Join("_", nameParts, 0, nameParts.Length - 1);
                            var locale = nameParts[^1];
                            _localizationTables[(tableName, locale)] = table;
                        }
                    }
                }
            }
            _isLoaded = true;
            _languageChoices = null;
            _availableLocales = null;
        }

        public static FontAsset GetFontForCurrentLocale()
        {
            return GetFontForLocale(_currentLocale);
        }

        public static FontAsset GetFontForLocale(string localeCode)
        {
            if (!string.IsNullOrEmpty(localeCode) && _fontByLocale != null && _fontByLocale.TryGetValue(localeCode, out var font) && font != null)
                return font;
            foreach (var f in _fontByLocale.Values)
            {
                if (f != null)
                    return f;
            }
            return null;
        }

        /// <summary>
        /// Loads a locale-specific sprite from Resources for the current locale.
        /// Example: Resources/en.png, Resources/zh_hans.png
        /// </summary>
        /// <param name="fallbackLocale">Fallback locale code if not found (default: "en")</param>
        /// <returns>The loaded Sprite, or null if not found.</returns>
        public static Sprite LoadLocaleSprite(string fallbackLocale = "en")
        {
            string locale = GetCurrentLocaleCode();

            // Normalize locale code to match your resource file naming
            string resourceName = locale.ToLowerInvariant()
                .Replace("-", "_")      // e.g. zh-Hans -> zh_hans
                .Replace("zh_hans", "zh_hans"); // explicit for clarity

            // Try to load the sprite for the current locale
            Sprite sprite = Resources.Load<Sprite>(resourceName);

            // Fallback if not found
            if (sprite == null && fallbackLocale != null)
            {
                string fallbackResource = fallbackLocale.ToLowerInvariant()
                    .Replace("-", "_");
                sprite = Resources.Load<Sprite>(fallbackResource);
            }

            return sprite;
        }

        private static void EnsureTablesLoaded()
        {
            if (!_isLoaded)
            {
                Debug.LogWarning("Localization tables not assigned. Call LocalizationHelper.AssignLocalizationTables at startup.");
            }
        }

        // --- Refactored: Get all tables for a locale ---
        private static IEnumerable<LocalizationTable> GetAllTablesForLocale(string locale)
        {
            foreach (var kvp in _localizationTables)
            {
                if (kvp.Key.locale == locale)
                    yield return kvp.Value;
            }
        }

        // --- Refactored: Get all tables for a table name and locale ---
        private static IEnumerable<LocalizationTable> GetTables(string table, string locale)
        {
            if (_localizationTables.TryGetValue((table, locale), out var foundTable) && foundTable != null)
            {
                yield return foundTable;
            }
        }

        // --- Refactored: Aggregate all entries for a table name and current locale ---
        private static Dictionary<string, string> GetTable(string table)
        {
            EnsureTablesLoaded();
            var dict = new Dictionary<string, string>();
            foreach (var tbl in GetTables(table, _currentLocale))
            {
                foreach (var entry in tbl.entries)
                {
                    if (!string.IsNullOrEmpty(entry.key) && !string.IsNullOrEmpty(entry.targetText))
                    {
                        dict[entry.key] = entry.targetText;
                    }
                }
            }
            return dict;
        }

        // --- Refactored: Aggregate all entries for a table name and given locale ---
        private static Dictionary<string, string> GetTableForLocale(string table, string locale)
        {
            EnsureTablesLoaded();
            var dict = new Dictionary<string, string>();
            foreach (var tbl in GetTables(table, locale))
            {
                foreach (var entry in tbl.entries)
                {
                    if (!string.IsNullOrEmpty(entry.key) && !string.IsNullOrEmpty(entry.targetText))
                    {
                        dict[entry.key] = entry.targetText;
                    }
                }
            }
            return dict;
        }

        public static List<LanguageChoice> GetLanguageChoices()
        {
            if (_languageChoices == null)
            {
                _languageChoices = new List<LanguageChoice>();
                var langTable = GetTable("GCML_LanguageChoices");
                if (langTable.Count > 0)
                {
                    foreach (var kvp in langTable)
                    {
                        _languageChoices.Add(new LanguageChoice
                        {
                            Key = kvp.Key,
                            NativeName = kvp.Value
                        });
                    }
                }
                else
                {
                    foreach (var locale in GetAvailableLocales())
                    {
                        _languageChoices.Add(new LanguageChoice
                        {
                            Key = locale,
                            NativeName = locale
                        });
                    }
                }
            }
            return _languageChoices;
        }

        public static List<string> GetAvailableLocales()
        {
            if (_availableLocales == null)
            {
                EnsureTablesLoaded();
                var localeSet = new HashSet<string>();
                foreach (var (table, locale) in _localizationTables.Keys)
                {
                    localeSet.Add(locale);
                }
                _availableLocales = new List<string>(localeSet);
                if (_availableLocales.Count == 0)
                {
                    _availableLocales.AddRange(new[] { "en", "zh-Hans"});
                }
            }
            return _availableLocales;
        }

        public static string GetLocalizedString(string table, string key, string fallback = "en")
        {
            if (string.IsNullOrEmpty(key))
                return fallback;

            var tbl = GetTable(table);
            if (tbl != null && tbl.TryGetValue(key, out var value) && !string.IsNullOrEmpty(value))
                return value;
            return fallback;
        }

        public static string GetLocalizedStringForLocale(string table, string key, string locale, string fallback = "en")
        {
            var tbl = GetTableForLocale(table, locale);
            if (tbl != null && tbl.TryGetValue(key, out var value) && !string.IsNullOrEmpty(value))
                return value;
            return fallback;
        }

        public static void InitializeLocale()
        {
            string savedLocale = LoadLocaleFromPrefs();
            if (!string.IsNullOrEmpty(savedLocale) && GetAvailableLocales().Contains(savedLocale))
            {
                SetLocale(savedLocale);
            }
            else
            {
                string systemLocale = GetSystemLocale();
                if (GetAvailableLocales().Contains(systemLocale))
                {
                    SetLocale(systemLocale);
                    SaveLocaleToPrefs(systemLocale);
                }
                else
                {
                    SetLocale("en");
                    SaveLocaleToPrefs("en");
                }
            }
        }

        public static void SaveLocaleToPrefs(string localeCode)
        {
            PlayerPrefs.SetString(PlayerPrefsLocaleKey, localeCode);
            PlayerPrefs.Save();
        }

        public static string LoadLocaleFromPrefs()
        {
            return PlayerPrefs.GetString(PlayerPrefsLocaleKey, null);
        }

        public static void ResetLocaleToSystemDefault()
        {
            string systemLocale = GetSystemLocale();
            if (GetAvailableLocales().Contains(systemLocale))
            {
                SetLocale(systemLocale);
                SaveLocaleToPrefs(systemLocale);
            }
            else
            {
                SetLocale("en");
                SaveLocaleToPrefs("en");
            }
        }

        public static string GetSystemLocale()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            string browserLang = GetBrowserLanguageString();
            if (!string.IsNullOrEmpty(browserLang))
            {
                if (browserLang.StartsWith("zh")) return "zh-Hans";
                if (browserLang.StartsWith("en")) return "en";
            }
#endif
            SystemLanguage lang = Application.systemLanguage;
            return lang switch
            {
                SystemLanguage.English => "en",
                SystemLanguage.ChineseSimplified => "zh-Hans",
                _ => "en",
            };
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern IntPtr GetBrowserLanguage();

        private static string GetBrowserLanguageString()
        {
            IntPtr ptr = GetBrowserLanguage();
            if (ptr == IntPtr.Zero)
                return "en";
            string lang = Marshal.PtrToStringAnsi(ptr);
            // Optionally free the memory if you allocate in JS
            return lang ?? "en";
        }
#endif

        public static void SetLocale(string localeCode)
        {
            if (_currentLocale != localeCode)
            {
                _currentLocale = localeCode;
                _languageChoices = null;
                _availableLocales = null;
                SaveLocaleToPrefs(localeCode);
                LocaleChanged?.Invoke();
            }
        }

        public static void SetLocaleTable(LocalizationTable localeTable)
        {
            if (localeTable == null)
            {
                Debug.LogWarning("Attempted to set a null LocalizationTable. No changes made.");
                return;
            }
            var localeCode = localeTable.name.Split('_')[^1];
            SetLocale(localeCode);
        }

        public static string GetCurrentLocaleCode()
        {
            return _currentLocale;
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void EditorLocaleSync()
        {
            UnityEditor.EditorApplication.playModeStateChanged += (state) =>
            {
                if (state == UnityEditor.PlayModeStateChange.EnteredPlayMode)
                {
                    UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocaleChanged += OnEditorLocaleChanged;
                    SyncLocaleWithUnityLocalization();
                }
                else if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                {
                    UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocaleChanged -= OnEditorLocaleChanged;
                }
            };
        }

        private static void OnEditorLocaleChanged(Locale newLocale)
        {
            SyncLocaleWithUnityLocalization();
        }

        private static void SyncLocaleWithUnityLocalization()
        {
            var unityLocale = UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale;
            if (unityLocale != null)
            {
                var code = unityLocale.Identifier.Code;
                if (_currentLocale != code)
                {
                    _currentLocale = code;
                    _languageChoices = null;
                    _availableLocales = null;
                    LocaleChanged?.Invoke();
                    Debug.Log($"[LocalizationHelper] Synced WebGL locale to Unity.Localization: {code}");
                }
            }
        }
#endif
    }
#endif
}