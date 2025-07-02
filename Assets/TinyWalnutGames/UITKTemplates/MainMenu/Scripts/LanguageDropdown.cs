using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TinyWalnutGames.UITKTemplates.Tools;

namespace TinyWalnutGames.UITKTemplates.MainMenu
{
    /// <summary>
    /// Dropdown for selecting application language.
    /// </summary>
    /// <remarks>
    /// Uses a static enum to ensure consistent language options across all platforms.
    /// </remarks>
    public enum Language
    {
        zh_Hans, // Chinese (Simplified)
        en      // English
    }

    /// <summary>
    /// Dropdown for selecting application language.
    /// </summary>
    public class LanguageDropdown : MonoBehaviour
    {
        private DropdownField _dropdown;
        private List<LocalizationHelper.LanguageChoice> _languageChoices;
        private List<string> _localeCodes;
        private Label _labelLanguage;

        // Store parent for re-initialization if needed
        private VisualElement _parent;

        // New: Call this to initialize the dropdown with the parent VisualElement (e.g. settings menu root)
        public void Initialize(VisualElement parent)
        {
            _parent = parent;
            if (_parent == null)
            {
                Debug.LogError("[LanguageDropdown] Parent VisualElement is null.");
                return;
            }

            _dropdown = _parent.Q<DropdownField>("dropdown_language");
            if (_dropdown == null)
            {
                Debug.LogError("[LanguageDropdown] dropdown_language not found.");
                return;
            }

            var languageDropdown = _parent.Q<VisualElement>("language_dropdown");
            if (languageDropdown != null)
                _labelLanguage = languageDropdown.Q<Label>("label_language");

            PopulateDropdown(_dropdown);
            UpdateLanguageSprite();
        }

        private void OnEnable()
        {
            LocalizationHelper.LocaleChanged += OnLocaleChanged;
            LocalizationHelper.LocaleChanged += UpdateLanguageSprite;
        }

        private void OnDisable()
        {
            LocalizationHelper.LocaleChanged -= OnLocaleChanged;
            LocalizationHelper.LocaleChanged -= UpdateLanguageSprite;
        }

        private void PopulateDropdown(DropdownField dropdown)
        {
            _languageChoices = LocalizationHelper.GetLanguageChoices();
            _localeCodes = new List<string>(_languageChoices.Count);
            var choices = new List<string>(_languageChoices.Count);

            for (int i = 0; i < _languageChoices.Count; i++)
            {
                var lang = _languageChoices[i];
                _localeCodes.Add(lang.Key);

#if !UNITY_WEBGL
                // Fetch the native name for each language using the helper
                string nativeName = LocalizationHelper.GetLocalizedStringForLocale(
                    "HOGT_UI",
                    lang.Key,
                    GetLocaleByCode(lang.Key),
                    lang.Key
                );
#endif

#if UNITY_WEBGL
                // For WebGL, use the native name directly from the LanguageChoice
                string nativeName = lang.NativeName;
#endif

                // Always append the code in Latin script
                choices.Add($"{nativeName} ({lang.Key})");
            }

            dropdown.choices = choices;

            // Set default index based on current locale
            int defaultIndex = 0;
            var code = LocalizationHelper.GetCurrentLocaleCode();
            for (int i = 0; i < _localeCodes.Count; i++)
            {
                if (_localeCodes[i] == code)
                {
                    defaultIndex = i;
                    break;
                }
            }
            dropdown.index = defaultIndex;

            dropdown.UnregisterValueChangedCallback(OnDropdownValueChanged);
            dropdown.RegisterValueChangedCallback(OnDropdownValueChanged);
        }

        private void OnDropdownValueChanged(ChangeEvent<string> evt)
        {
            var selectedIndex = _dropdown.index;
            if (selectedIndex >= 0 && selectedIndex < _localeCodes.Count)
            {
                var localeCode = _localeCodes[selectedIndex];
                LocalizationHelper.SetLocale(localeCode);
                Debug.Log("Locale set to: " + localeCode);
            }
            else
            {
                Debug.LogWarning("Selected index is out of range: " + selectedIndex);
            }

            MainMenuController.RaiseLocalizedUIRefresh();
            var settingsMenu = FindFirstObjectByType<SettingsMenu>();
            if (settingsMenu != null)
                settingsMenu.SendMessage("RefreshLocalizedUI", SendMessageOptions.DontRequireReceiver);
        }

        private void OnLocaleChanged()
        {
            var code = LocalizationHelper.GetCurrentLocaleCode();

            if (_dropdown == null || _localeCodes == null)
                return;

            // Update the dropdown index and choices based on the current locale
            PopulateDropdown(_dropdown);

            for (int i = 0; i < _localeCodes.Count; i++)
            {
                if (_localeCodes[i] == code)
                {
                    _dropdown.index = i;
                    break;
                }
            }

            MainMenuController.RaiseLocalizedUIRefresh();
            var settingsMenu = FindFirstObjectByType<SettingsMenu>();
            if (settingsMenu != null)
                settingsMenu.SendMessage("RefreshLocalizedUI", SendMessageOptions.DontRequireReceiver);
        }

        private void UpdateLanguageSprite()
        {
#if UNITY_WEBGL
            if (_labelLanguage == null)
                return;

            var sprite = LocalizationHelper.LoadLocaleSprite();
            if (sprite != null)
            {
                var texture = sprite.texture;
                if (texture != null)
                {
                    _labelLanguage.style.backgroundImage = new StyleBackground(texture);
                }
            }
            else
            {
                _labelLanguage.style.backgroundImage = null;
            } 
#endif
            // On Non-WebGL platforms, the sprite is handled by the LocalizationHelper directly
        }

        // Helper to get Locale object by code
        private UnityEngine.Localization.Locale GetLocaleByCode(string code)
        {
            var locales = UnityEngine.Localization.Settings.LocalizationSettings.AvailableLocales.Locales;
            for (int i = 0; i < locales.Count; i++)
            {
                if (locales[i].Identifier.Code == code)
                    return locales[i];
            }
            return null;
        }
    }
}