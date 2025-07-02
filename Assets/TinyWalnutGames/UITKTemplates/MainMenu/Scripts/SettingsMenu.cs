using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using TinyWalnutGames.UITKTemplates.Tools;

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

        private VisualElement _settingsPanelRoot; // is this supposed to be set to settings_panel.xml?
        private VisualElement _settingsBackground; // #settingsbackground, the background element for the settings panel, used to hide the play area when settings are open and positioned the panel correctly.
        private VisualElement _settingsPanel; // The main settings panel container, where all settings UI elements are placed.
        private Button _closeSettingsButton;
        private Button _resetMinigameButton;
        private Toggle _musicToggle;
        private Toggle _sfxToggle;
        private Slider _musicVolumeSlider;
        private Slider _sfxVolumeSlider;

        private bool _initialized = false;

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

            LocalizationHelper.LocaleChanged += RefreshLocalizedUI;
        }

        private void OnDestroy()
        {
            LocalizationHelper.LocaleChanged -= RefreshLocalizedUI;
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

            // Assign the background element (ensure the name matches your UXML)
            _settingsBackground = _settingsPanelRoot.Q<VisualElement>("settingsbackground"); // <-- fix: assign to background
            _settingsPanel = _settingsPanelRoot.Q<VisualElement>("settings_panel"); // <-- fix: assign to panel inside background
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
        }

        public void Show()
        {
            // Show the background and the panel
            if (_settingsBackground != null)
                _settingsBackground.style.display = DisplayStyle.Flex;
            if (_settingsPanel != null)
                _settingsPanel.style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            if (_settingsPanel != null)
                _settingsPanel.style.display = DisplayStyle.None;
            if (_settingsBackground != null)
                _settingsBackground.style.display = DisplayStyle.None;
        }
    }
}
