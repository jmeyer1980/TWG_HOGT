using UnityEngine;
using System.Collections.Generic;
using UnityEngine.TextCore.Text;
using System.Linq;

namespace TinyWalnutGames.Localization
{
    /// <summary>
    /// Attach this to a GameObject in your scene and assign all LocalizationTable assets in the Inspector.
    /// This script must be placed after TMPro and before any game stuff runs. -100 seeems to be a good place.
    /// </summary>
    public class LocalizationTableHolder : MonoBehaviour
    {
        [Tooltip("Assign one LocalizationTable per supported locale.")]
        public LocalizationTable[] tables;

        [Tooltip("Assign fonts with their corresponding locale codes.")]
        public LocaleFontPair[] localeFonts;

        // Dictionary for fast locale code to FontAsset lookup
        public Dictionary<string, FontAsset> FontByLocale { get; private set; }

        // Dictionary for fast [table name]_[locale code] to LocalizationTable lookup (WebGL style)
        public Dictionary<string, LocalizationTable> TableByNameAndLocale { get; private set; }

        [System.Serializable]
        public struct LocaleFontPair
        {
            public string localeCode;
            public FontAsset font;
        }

        private void Awake()
        {
            BuildTableByNameAndLocale();
#if UNITY_WEBGL // && UNITY_EDITOR // Always use WebGL method, even in Editor WHEN WebGL testing
            LocalizationHelper.AssignLocalizationTablesWebGL(TableByNameAndLocale.Values.ToArray());
            // confirm that the tables are assigned and confirm after they have loaded. I am getting warnings that they are not assigned.
            foreach (var kvp in TableByNameAndLocale)
            {
                if (kvp.Value == null)
                {
                    Debug.LogWarning($"LocalizationTable '{kvp.Key}' is not assigned in the LocalizationTableHolder.");
                }
                else
                {
                    Debug.Log($"LocalizationTable '{kvp.Key}' is assigned.");
                }
            }
            // Notify MainMenuController or other listeners that localization is ready
            // (You may use a static event or call a method directly if needed)
#endif
            // Build the dictionary for fast lookup
            if (localeFonts != null)
            {
                FontByLocale = new Dictionary<string, FontAsset>(localeFonts.Length);
                foreach (var pair in localeFonts)
                {
                    if (!string.IsNullOrEmpty(pair.localeCode) && pair.font != null)
                    {
                        FontByLocale[pair.localeCode] = pair.font;
                    }
                }
            }
            else
            {
                FontByLocale = new Dictionary<string, FontAsset>();
            }
        }

        /// <summary>
        /// Builds the [table name]_[locale code] => LocalizationTable dictionary for WebGL-style lookup.
        /// </summary>
        private void BuildTableByNameAndLocale()
        {
            TableByNameAndLocale = new Dictionary<string, LocalizationTable>();
            if (tables != null)
            {
                foreach (var table in tables)
                {
                    if (table == null) continue;
                    // Use asset name as table name, and table.localeCode for the locale
                    // Asset name may include locale, so strip it if needed
                    string assetName = table.name;
                    string locale = table.entries.Count > 0 ? table.entries[0].localeCode : "en"; // Default to "en" if no entries
                    string baseName = assetName;
                    // Remove _[locale] suffix if present
                    if (!string.IsNullOrEmpty(locale) && assetName.EndsWith("_" + locale))
                    {
                        baseName = assetName[..^(locale.Length + 1)];
                    }
                    string key = $"{baseName}_{locale}";
                    TableByNameAndLocale[key] = table;
                }
            }
        }
    }
}
