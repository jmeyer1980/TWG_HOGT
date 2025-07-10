using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using TinyWalnutGames.UITKTemplates.Tools;
using System.Collections.Generic;

namespace TinyWalnutGames.UITKTemplates.MainMenu
{
    /// <summary>
    /// Handles the Settings Menu UI and logic, including localization and audio settings.
    /// Instantiates from a VisualTreeAsset template, not a UIDocument.
    /// </summary>
    public class SettingsMenu : MonoBehaviour
    {
        // Singleton instance for global access
        public static SettingsMenu Instance { get; private set; }

        [Tooltip("Settings menu VisualTreeAsset template (UXML).")]
        public VisualTreeAsset settingsTemplate;

        // Add reference to LanguageDropdown component (should be on the same GameObject)
        public LanguageDropdown languageDropdown;

        private VisualElement _settingsPanelRoot;
        private VisualElement _settingsBackground;
        private VisualElement _settingsPanel;
        private Button _closeSettingsButton;
        private Button _resetMinigameButton;
        private Toggle _musicToggle;
        private Toggle _sfxToggle;
        private Slider _musicVolumeSlider;
        private Slider _sfxVolumeSlider;

        private bool _initialized = false;

        // Add: Validation config for modular UI validation
        private UIDocumentValidationConfig _validationConfig;

        // Dummy UIDocument ref for validator compatibility (not used for template-based panels)
        private static UIDocument UnsafeNullUIDocumentRef = null;

        private void Awake()
        {
            // Ensure this is the only instance
            if (Instance == null)
            {
                Instance = this;
                // deparent the GameObject to avoid issues with scene loading
                transform.SetParent(null, true);
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Setup validation config for settings panel
            _validationConfig = new UIDocumentValidationConfig
            {
                NamedElements = new Dictionary<string, System.Type>
                {
                    { "settingsbackground", typeof(VisualElement) },
                    { "settings_panel", typeof(VisualElement) },
                    { "button_close_settings", typeof(Button) },
                    { "button_reset_minigame", typeof(Button) },
                    { "toggle_music", typeof(Toggle) },
                    { "toggle_sfx", typeof(Toggle) },
                    { "m_vol", typeof(Slider) },
                    { "sfx_vol", typeof(Slider) },
                    { "dropdown_language", typeof(DropdownField) }
                },
                ProgressBarName = null // Not required in settings menu
            };

            LocalizationHelper.LocaleChanged += OnLocaleChanged;

            RefreshLocalizedUI();
        }

        private void OnDestroy()
        {
            LocalizationHelper.LocaleChanged -= OnLocaleChanged;
        }

        /// <summary>
        /// Call this after the main UI root is available.
        /// </summary>
        public void Initialize(VisualElement parent)
        {
            if (_initialized) return;
            if (settingsTemplate == null)
            {
                Debug.LogError("[SettingsMenu] Settings template is not assigned.");
                return;
            }
            if (parent == null)
            {
                Debug.LogError("[SettingsMenu] Parent VisualElement is null.");
                return;
            }

            _settingsPanelRoot = settingsTemplate.CloneTree();
            parent.Add(_settingsPanelRoot);

            // Validate the settings panel root using the modular validator
            UIDocumentValidator.ValidateOrFixUIDocument(
                this, ref UnsafeNullUIDocumentRef, null, _validationConfig, out _, out _, false);

            _settingsBackground = _settingsPanelRoot.Q<VisualElement>("settingsbackground");
            _settingsPanel = _settingsPanelRoot.Q<VisualElement>("settings_panel");
            _closeSettingsButton = _settingsPanelRoot.Q<Button>("button_close_settings");
            _resetMinigameButton = _settingsPanelRoot.Q<Button>("button_reset_minigame");
            _musicToggle = _settingsPanelRoot.Q<Toggle>("toggle_music");
            _sfxToggle = _settingsPanelRoot.Q<Toggle>("toggle_sfx");
            _musicVolumeSlider = _settingsPanelRoot.Q<Slider>("m_vol");
            _sfxVolumeSlider = _settingsPanelRoot.Q<Slider>("sfx_vol");

            // Initialize the language dropdown with the settings panel root
            if (languageDropdown != null)
            {
                languageDropdown.Initialize(_settingsPanelRoot);
            }

            // Hide settings panel and background initially
            if (_settingsPanel != null)
                _settingsPanel.style.display = DisplayStyle.None;
            if (_settingsBackground != null)
                _settingsBackground.style.display = DisplayStyle.None;

            RegisterButtonWithSound(_closeSettingsButton, () =>
            {
                Hide();
                PlayUISound("close");
            });

            RegisterButtonWithSound(_resetMinigameButton, () =>
            {
                CorkBoardMiniGame corkBoardMiniGame = Object.FindFirstObjectByType<CorkBoardMiniGame>();
                if (corkBoardMiniGame != null)
                    corkBoardMiniGame.ResetMiniGame();
                PlayUISound("error");
            });

            RegisterToggleWithSound(_musicToggle, (val) =>
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.SetMusicMute(!val);
            });

            RegisterToggleWithSound(_sfxToggle, (val) =>
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.SetSFXMute(!val);
            });

            RegisterSliderWithSound(_musicVolumeSlider, value =>
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.SetMusicVolume(value);
            });

            RegisterSliderWithSound(_sfxVolumeSlider, value =>
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.SetSFXVolume(value);
            });
            // Validate Locale
            var localeCode = LocalizationHelper.GetCurrentLocaleCode();
            if (localeCode == null)
                LocalizationHelper.ResetLocaleToSystemDefault();
            // Validate the initial state of the settings UI with the modular validator
            // This ensures all elements are correctly initialized and localized
            UIDocumentValidator.ValidateOrFixUIDocument(
                this, ref UnsafeNullUIDocumentRef, null, _validationConfig, out _, out _, true);

            RefreshLocalizedUI();
            _initialized = true;
        }

        private void RegisterButtonWithSound(Button button, System.Action onClick)
        {
            if (button == null) return;
            button.clicked += () =>
            {
                PlayUISound("click");
                onClick?.Invoke();
            };
        }

        private void RegisterToggleWithSound(Toggle toggle, System.Action<bool> onValueChanged)
        {
            if (toggle == null) return;
            toggle.RegisterValueChangedCallback(evt =>
            {
                PlayUISound("toggle");
                onValueChanged?.Invoke(evt.newValue);
            });
        }

        private void RegisterSliderWithSound(Slider slider, System.Action<float> onValueChanged)
        {
            if (slider == null) return;
            slider.RegisterValueChangedCallback(evt =>
            {
                PlayUISound("drag");
                onValueChanged?.Invoke(evt.newValue);
            });
        }

        private static void PlayUISound(string key)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(key);
        }

        public void RefreshLocalizedUI()
        {
#if UNITY_WEBGL
            if (_settingsPanelRoot == null) return;
            var closeSettingsButton = _settingsPanelRoot.Q<Button>("button_close_settings");
            var resetMinigameButton = _settingsPanelRoot.Q<Button>("button_reset_minigame");
            var settingsLabel = _settingsPanelRoot.Q<Label>("label_settings");
            var musicSlider = _settingsPanelRoot.Q<Slider>("m_vol");
            var sfxSlider = _settingsPanelRoot.Q<Slider>("sfx_vol");
            var musicToggle = _settingsPanelRoot.Q<Toggle>("toggle_music");
            var sfxToggle = _settingsPanelRoot.Q<Toggle>("toggle_sfx");

            if (closeSettingsButton != null) closeSettingsButton.text = LocalizationHelper.GetLocalizedString("ui", "btn_close_label");
            if (resetMinigameButton != null) resetMinigameButton.text = LocalizationHelper.GetLocalizedString("ui", "btn_reset_minigame_label");
            if (settingsLabel != null) settingsLabel.text = LocalizationHelper.GetLocalizedString("ui", "label_settings_title");
            if (musicSlider != null) musicSlider.label = LocalizationHelper.GetLocalizedString("ui", "label_music");
            if (sfxSlider != null) sfxSlider.label = LocalizationHelper.GetLocalizedString("ui", "label_sfx");
            if (musicToggle != null) musicToggle.tooltip = LocalizationHelper.GetLocalizedString("tooltips", "music_tooltip");
            if (sfxToggle != null) sfxToggle.tooltip = LocalizationHelper.GetLocalizedString("tooltips", "sfx_tooltip");
#else       
            if (_settingsPanelRoot == null) return;
            // Refresh all localized UI elements in the settings panel
            _initialized = true;
            var closeSettingsButton = _settingsPanelRoot.Q<Button>("button_close_settings");
            var resetMinigameButton = _settingsPanelRoot.Q<Button>("button_reset_minigame");
            var settingsLabel = _settingsPanelRoot.Q<Label>("label_settings");
            var musicSlider = _settingsPanelRoot.Q<Slider>("m_vol");
            var sfxSlider = _settingsPanelRoot.Q<Slider>("sfx_vol");
            var musicToggle = _settingsPanelRoot.Q<Toggle>("toggle_music");
            var sfxToggle = _settingsPanelRoot.Q<Toggle>("toggle_sfx");
            if (closeSettingsButton != null) closeSettingsButton.text = LocalizationHelper.GetLocalizedString("HOGT_UI", "btn_close_label");
            if (resetMinigameButton != null) resetMinigameButton.text = LocalizationHelper.GetLocalizedString("HOGT_UI", "btn_reset_minigame_label");
            if (settingsLabel != null) settingsLabel.text = LocalizationHelper.GetLocalizedString("HOGT_UI", "label_settings_title");
            if (musicSlider != null) musicSlider.label = LocalizationHelper.GetLocalizedString("HOGT_UI", "label_music");
            if (sfxSlider != null) sfxSlider.label = LocalizationHelper.GetLocalizedString("HOGT_UI", "label_sfx");
            if (musicToggle != null) musicToggle.tooltip = LocalizationHelper.GetLocalizedString("tooltips", "music_tooltip");
#endif
            // Update tooltip font for settings tooltips
            if (TooltipManager.Instance != null)
            {
                TooltipManager.Instance.SetFontAsset(LocalizationHelper.GetFontForCurrentLocale());
            }
        }

        public void Show()
        {
            // Show the background and the panel
            if (_settingsBackground != null)
                _settingsBackground.style.display = DisplayStyle.Flex;
            if (_settingsPanel != null)
                _settingsPanel.style.display = DisplayStyle.Flex;

            // Ensure the settings panel is initialized
            if (!_initialized)
            {
                Initialize(_settingsPanelRoot?.parent);
                if (!_initialized)
                    Debug.LogWarning("[SettingsMenu] SettingsMenu not initialized. Call Initialize() first.");                
            }
            if (languageDropdown != null)
                languageDropdown.Initialize(_settingsPanelRoot);

            // Refresh localized UI to ensure all text is up-to-date
            RefreshLocalizedUI();
        }

        public void Hide()
        {
            if (_settingsPanel != null)
                _settingsPanel.style.display = DisplayStyle.None;
            if (_settingsBackground != null)
                _settingsBackground.style.display = DisplayStyle.None;
        }

        // Add: LocaleChanged handler to ensure all UI and tooltips update
        private void OnLocaleChanged()
        {
            RefreshLocalizedUI();
            if (TooltipManager.Instance != null)
            {
                TooltipManager.Instance.SetFontAsset(LocalizationHelper.GetFontForCurrentLocale());
            }
            MainMenuController.RaiseLocalizedUIRefresh();
        }
    }
}
