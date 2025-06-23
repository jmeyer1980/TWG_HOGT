using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using TinyWalnutGames.Localization;
using TinyWalnutGames.Tools;
using TinyWalnutGames.HOGT.UI;
using UnityEngine.TextCore.Text;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using TinyWalnutGames.UI; // Add this for TooltipManager and Tooltip

namespace TinyWalnutGames.UI
{
    public class MainMenuController : MonoBehaviour
    {
        public UIDocument uiDocument;

        // scene to load on play
        [Tooltip("Scene to load when the player clicks the Play button. Defaults to the next scene in build settings.")]
        [SerializeField]
        private string nextSceneName = "";

        // Event to allow other scripts to refresh UI
        public static event System.Action LocalizedUIRefreshRequested;

        private const float TooltipMargin = 16f; // Space between mouse and tooltip
        private const float TooltipSafeBound = 8f; // Minimum space from screen edge

        private VisualElement _languageDropdown;
        private Label _labelLanguage;

        private bool tooltipManagerInitialized = false;
        private bool _localizationReady = false;
        private bool _tooltipTemplateReady = false;
        private string sfxToggleTooltip;
        private string musicToggleTooltip;
        private string languageDropdownTooltip;
        private string resetMinigameButtonTooltip;
        private string openSettingsButtonTooltip;
        private string playButtonTooltip;

        // --- UI Sound Effect Key Definitions ---
        // See documentation above for the meaning of each key.

        /// <summary>
        /// Safely plays a UI sound effect by key using AudioManager.Instance, if available.
        /// </summary>
        private static void PlayUISound(string key)
        {
            if (TinyWalnutGames.Tools.AudioManager.Instance != null)
                TinyWalnutGames.Tools.AudioManager.Instance.PlaySFX(key);
        }

        private void Awake()
        {
            LocalizationHelper.SetLocale("zh-hans"); // Default to English locale
            // Initialize locale from PlayerPrefs or system/browser
            LocalizationHelper.InitializeLocale();

            LocalizationHelper.LocaleChanged += OnLocaleChanged;
            LocalizationHelper.LocaleChanged += UpdateLanguageSprite;
            string localeCode = LocalizationHelper.GetCurrentLocaleCode();
            LocalizationHelper.SetLocale(localeCode);
            LocalizedUIRefreshRequested += RefreshLocalizedUI;

            // Listen for tooltip template loaded event
            Tooltip.TooltipTemplateLoaded += OnTooltipTemplateLoaded;
            Debug.Log("[MainMenuController] Subscribed to TooltipTemplateLoaded.");

            // Listen for localization ready (after tables assigned)
            _localizationReady = TinyWalnutGames.Localization.LocalizationHelper.GetAvailableLocales().Count > 0;
            if (_localizationReady)
                OnLocalizationReady();

            RegisterTooltipEvents(uiDocument.rootVisualElement);
            RaiseLocalizedUIRefresh();

            // TooltipManager is now a singleton MonoBehaviour, so no direct instantiation here.
            // However, we still need to ensure it is initialized after the UI Document is ready.
            // So we will check for UI Document first and then initialize TooltipManager.
            if (uiDocument == null)
            {
                Debug.LogError("[MainMenuController] UIDocument is not assigned. Please assign it in the Inspector.");
                return;
            }
            else if (uiDocument.rootVisualElement == null)
            {
                Debug.LogError("[MainMenuController] UIDocument rootVisualElement is null. Ensure the UIDocument is set up correctly.");
                return;
            }

            // Tooltip manager exists in the scene as a singleton MonoBehaviour. But we can still check if it is null.
            if (TooltipManager.Instance == null)
            {
                Debug.LogWarning("[MainMenuController] TooltipManager.Instance is null. Ensure TooltipManager is present in the scene.");
            }
            else
            {
                TryInitTooltipManager();
            }
        }

        private void OnDisable()
        {
            LocalizationHelper.LocaleChanged -= OnLocaleChanged;
            LocalizationHelper.LocaleChanged -= UpdateLanguageSprite;
            LocalizationHelper.SetLocale("en");
            LocalizedUIRefreshRequested -= RefreshLocalizedUI;
            Tooltip.TooltipTemplateLoaded -= OnTooltipTemplateLoaded;
            Debug.Log("[MainMenuController] Unsubscribed from TooltipTemplateLoaded.");
        }

        private void OnTooltipTemplateLoaded()
        {
            if (!tooltipManagerInitialized)
            {
                tooltipManagerInitialized = true;
                TryInitTooltipManager();
            }
        }

        private void OnLocalizationReady()
        {
            _localizationReady = true;
            TryInitTooltipManager();
        }

        private void TryInitTooltipManager()
        {
            // Debug: Log the current state of all relevant flags
            Debug.Log($"[MainMenuController] TryInitTooltipManager called. " +
                      $"TooltipManager.Instance: {(TooltipManager.Instance != null ? "present" : "null")}, " +
                      $"Tooltip.IsTemplateReady: {Tooltip.IsTemplateReady}, " +
                      $"_localizationReady: {_localizationReady}");

            // Use the singleton instance and initialize with the root VisualElement
            if (TooltipManager.Instance != null && Tooltip.IsTemplateReady && _localizationReady)
            {
                var root = uiDocument.rootVisualElement;
                TooltipManager.Instance.Initialize(root);
                RefreshLocalizedUI(); // Ensure localized UI is refreshed after TooltipManager initialization
                RegisterTooltipEvents(uiDocument.rootVisualElement);
                Debug.Log("[MainMenuController] TooltipManager initialized and tooltip events registered.");
            }
            else
            {
                if (TooltipManager.Instance == null)
                {
                    Debug.LogWarning("[MainMenuController] TooltipManager.Instance is null. Ensure TooltipManager is present in the scene.");
                }
                if (!Tooltip.IsTemplateReady)
                {
                    Debug.LogWarning("[MainMenuController] TooltipManager not initialized: Tooltip template not ready. Will wait for TooltipTemplateLoaded event.");
                    Tooltip.TooltipTemplateLoaded -= OnTooltipTemplateLoaded;
                    Tooltip.TooltipTemplateLoaded += OnTooltipTemplateLoaded;
                }
                if (!_localizationReady)
                {
                    Debug.LogWarning("[MainMenuController] TooltipManager not initialized: Localization is not ready.");
                }
                if (TooltipManager.Instance == null || !Tooltip.IsTemplateReady || !_localizationReady)
                {
                    Debug.Log("[MainMenuController] TooltipManager initialization blocked. " +
                              $"Instance: {(TooltipManager.Instance != null ? "present" : "null")}, " +
                              $"TemplateReady: {Tooltip.IsTemplateReady}, " +
                              $"LocalizationReady: {_localizationReady}");
                }
            }
        }

        void Start()
        {
            var root = uiDocument.rootVisualElement;

            var settingsPanel = root.Q<VisualElement>("settings_panel");
            var openSettingsButton = root.Q<UnityEngine.UIElements.Button>("button_open_settings");
            var musicToggle = root.Q<UnityEngine.UIElements.Toggle>("toggle_music");
            var sfxToggle = root.Q<UnityEngine.UIElements.Toggle>("toggle_sfx");
            var musicVolumeSlider = root.Q<UnityEngine.UIElements.Slider>("m_vol");
            var sfxVolumeSlider = root.Q<UnityEngine.UIElements.Slider>("sfx_vol");
            var resetMinigameButton = root.Q<UnityEngine.UIElements.Button>("button_reset_minigame");
            var closeSettingsButton = root.Q<UnityEngine.UIElements.Button>("button_close_settings");
            var playButton = root.Q<UnityEngine.UIElements.Button>("button_play");

            // Defensive null checks
            if (settingsPanel == null) Debug.LogWarning("Settings panel not found in the UI document.");
            if (openSettingsButton == null) Debug.LogWarning("Open settings button not found in the UI document.");
            if (musicVolumeSlider == null) Debug.LogWarning("Music volume slider not found in the UI document.");
            if (sfxVolumeSlider == null) Debug.LogWarning("SFX volume slider not found in the UI document.");
            if (resetMinigameButton == null) Debug.LogWarning("Reset minigame button not found in the UI document.");
            if (closeSettingsButton == null) Debug.LogWarning("Close settings button not found in the UI document.");
            if (playButton == null) Debug.LogWarning("Play button not found in the UI document.");

            // Hide settings panel initially
            if (settingsPanel != null)
                settingsPanel.style.display = DisplayStyle.None;

            // Set initial toggle state to reflect enabled status (true = enabled, false = muted)
            if (musicToggle != null)
            {
                if (AudioManager.Instance != null)
                    musicToggle.value = !AudioManager.Instance.IsMusicMuted();
            }
            if (sfxToggle != null)
            {
                if (AudioManager.Instance != null)
                    sfxToggle.value = !AudioManager.Instance.IsSFXMuted();
            }

            // Centralized button event registration with sound
            RegisterButtonWithSound(openSettingsButton, () =>
            {
                if (settingsPanel != null)
                    settingsPanel.style.display = DisplayStyle.Flex;
                PlayUISound("open"); // Play 'open' sound when opening settings
            });

            RegisterButtonWithSound(resetMinigameButton, () =>
            {
                // Reset the minigame state, UI, and PlayerPrefs for this minigame only
                CorkBoardMiniGame corkBoardMiniGame = Object.FindFirstObjectByType<CorkBoardMiniGame>();
                if (corkBoardMiniGame != null)
                {
                    corkBoardMiniGame.ResetMiniGame();
                }
                else
                {
                    Debug.LogWarning("CorkBoardMiniGame instance not found in the scene.");
                }
                Debug.Log("Resetting minigame state...");
                PlayUISound("error"); // Play 'error' sound for reset (or choose another if more appropriate)
            });

            RegisterButtonWithSound(closeSettingsButton, () =>
            {
                if (settingsPanel != null)
                    settingsPanel.style.display = DisplayStyle.None;
                PlayUISound("close"); // Play 'close' sound when closing settings
            });

            RegisterButtonWithSound(playButton, () =>
            {                
                SceneManager.LoadScene(nextSceneName);
                PlayUISound("pick"); // Play 'pick' sound when starting the game
            });

            // Register toggle sound effects
            RegisterToggleWithSound(musicToggle);
            RegisterToggleWithSound(sfxToggle);

            // Register volume sliders.
            RegisterSliderWithSound(musicVolumeSlider, value =>
            {
                // avoid null propogation, using code safer for older Unity versions
                if (AudioManager.Instance != null)
                    AudioManager.Instance.SetMusicVolume(value);
                ShowTooltip(musicVolumeSlider, value);
            });
            RegisterSliderWithSound(sfxVolumeSlider, value =>
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.SetSFXVolume(value); // this version avoids null propogation, and is safer for older Unity versions
                ShowTooltip(sfxVolumeSlider, value);
            });

            // Register slider drag events for looping drag sound
            RegisterSliderDragSound(musicVolumeSlider);
            RegisterSliderDragSound(sfxVolumeSlider);

            // Find the language dropdown and label
            if (root != null)
            {
                _languageDropdown = root.Q<VisualElement>("language_dropdown");
                if (_languageDropdown != null)
                    _labelLanguage = _languageDropdown.Q<Label>("label_language");
            }

            UpdateLanguageSprite();
            LocalizationHelper.InitializeLocale();
            RaiseLocalizedUIRefresh();
            RegisterTooltipEvents(root);
        }

        // Helper to register button click with sound
        private void RegisterButtonWithSound(UnityEngine.UIElements.Button button, System.Action onClick)
        {
            if (button == null) return;
            button.clicked += () =>
            {
                PlayUISound("click"); // Play 'click' sound for all button presses
                onClick?.Invoke();
            };
        }

        // Helper to register toggle change with sound
        private void RegisterToggleWithSound(UnityEngine.UIElements.Toggle toggle)
        {
            if (toggle == null) return;
            toggle.RegisterValueChangedCallback(evt =>
            {
                PlayUISound("toggle"); // Play 'toggle' sound for toggle switches
                // true = enabled (unmuted), false = disabled (muted)
                if (toggle.name == "toggle_music")
                {
                    if (AudioManager.Instance != null)
                        AudioManager.Instance.SetMusicMute(!evt.newValue);
                }
                else if (toggle.name == "toggle_sfx")
                {
                    if (AudioManager.Instance != null)
                        AudioManager.Instance.SetSFXMute(!evt.newValue);
                }
            });
        }

        // Helper to link m_vol and sfx_vol slider control to their counterparts in AudioManager
        private void RegisterSliderWithSound(UnityEngine.UIElements.Slider slider, System.Action<float> onValueChanged)
        {
            if (slider == null) return;
            slider.RegisterValueChangedCallback(evt =>
            {
                PlayUISound("drag"); // Play 'drag' sound when adjusting volume
                onValueChanged?.Invoke(evt.newValue);
            });
        }

        // Register slider drag events for looping drag sound
        private void RegisterSliderDragSound(UnityEngine.UIElements.Slider slider)
        {
            if (slider == null) return;

            // Track drag state
            bool isDragging = false;

            slider.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (!isDragging)
                {
                    isDragging = true;
                    if (AudioManager.Instance != null)
                        AudioManager.Instance.PlayLoopingSFX("fade"); // Play 'fade' sound when dragging slider
                }
            });

            slider.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (isDragging)
                {
                    isDragging = false;
                    if (AudioManager.Instance != null)
                        AudioManager.Instance.StopLoopingSFX(); // Stop sound when releasing slider
                }
            });

            // Also stop sound if pointer leaves the slider while dragging
            slider.RegisterCallback<PointerLeaveEvent>(evt =>
            {
                if (isDragging)
                {
                    isDragging = false;
                    TooltipManager.Instance?.Hide(); // Hide tooltip when leaving slider
                }
            });
        }

        private void RegisterTooltipEvents(VisualElement root)
        {
            // Find all elements with a tooltip attribute and register handlers
            var elementsWithTooltips = new List<VisualElement>
            {
                root.Q<UnityEngine.UIElements.Button>("button_play"),
                root.Q<UnityEngine.UIElements.Button>("button_open_settings"),
                root.Q<UnityEngine.UIElements.Slider>("m_vol"),
                root.Q<UnityEngine.UIElements.Slider>("sfx_vol"),
                root.Q<UnityEngine.UIElements.Button>("button_reset_minigame"),
                root.Q<UnityEngine.UIElements.Toggle>("toggle_music"),
                root.Q<UnityEngine.UIElements.Toggle>("toggle_sfx"),
                root.Q<VisualElement>("language_dropdown")
            };

            foreach (var element in elementsWithTooltips)
            {
                if (element != null && !string.IsNullOrEmpty(element.tooltip))
                {
                    RegisterTooltipHandlers(element, element.tooltip);
                }
            }
        }

        private void RegisterTooltipHandlers(VisualElement element, string _)
        {
            element.RegisterCallback<PointerEnterEvent>(evt =>
            {
                ShowTooltip(element, element.tooltip);
                PlayUISound("focus"); // Play 'focus' sound when hovering/focusing a UI element
            });
            element.RegisterCallback<PointerLeaveEvent>(_ =>
            {
                HideTooltip();
            });
            element.RegisterCallback<PointerMoveEvent>(evt =>
            {
                MoveTooltip(evt.position);
            });
        }

        // Show tooltip for buttons and other elements with a string tooltip
        private void ShowTooltip(VisualElement element, string tooltipText)
        {
            if (TooltipManager.Instance == null) return;
            Vector2 mousePos = Mouse.current.position.ReadValue();
            TooltipManager.Instance.Show(tooltipText, mousePos);
        }

        // Show tooltip for sliders with value
        private void ShowTooltip(VisualElement element, float value)
        {
            if (TooltipManager.Instance == null) return;
            Vector2 mousePos = Mouse.current.position.ReadValue();
            TooltipManager.Instance.ShowWithValue(element.name, value, mousePos);
            PlayUISound("drag"); // Play 'drag' sound when adjusting volume
        }

        private void MoveTooltip(Vector2 mousePosition)
        {
            if (TooltipManager.Instance == null) return;
            TooltipManager.Instance.Move(mousePosition);
        }

        private void HideTooltip()
        {
            if (TooltipManager.Instance == null) return;
            TooltipManager.Instance.Hide();
        }

        private void RefreshLocalizedUI()
        {
            var root = uiDocument.rootVisualElement;
            var playButton = root.Q<UnityEngine.UIElements.Button>("button_play");
            var openSettingsButton = root.Q<UnityEngine.UIElements.Button>("button_open_settings");
            var resetMinigameButton = root.Q<UnityEngine.UIElements.Button>("button_reset_minigame");
            var closeSettingsButton = root.Q<UnityEngine.UIElements.Button>("button_close_settings");            
            var settingsLabel = root.Q<UnityEngine.UIElements.Label>("label_settings");
            var languageLabel = root.Q<UnityEngine.UIElements.Label>("label_language");
            var languageDropdownVisualElement = root.Q<UnityEngine.UIElements.VisualElement>("language_dropdown"); // tooltip is on the visual element, not the dropdown itself
            var musicSlider = root.Q<UnityEngine.UIElements.Slider>("m_vol");
            var sfxSlider = root.Q<UnityEngine.UIElements.Slider>("sfx_vol");

            var labeltitle = root.Q<UnityEngine.UIElements.Label>("label_title");
            var labelsubtitle = root.Q<UnityEngine.UIElements.Label>("label_subtitle");

            // Use LocalizationHelper to get localized strings
            if (playButton != null) playButton.text = LocalizationHelper.GetLocalizedString("ui", "btn_play_label");
            if (openSettingsButton != null) openSettingsButton.text = LocalizationHelper.GetLocalizedString("ui", "btn_settings_label");
            if (closeSettingsButton != null) closeSettingsButton.text = LocalizationHelper.GetLocalizedString("ui", "btn_close_label");
            if (settingsLabel != null) settingsLabel.text = LocalizationHelper.GetLocalizedString("ui", "label_settings_title");
            if (languageLabel != null) languageLabel.text = LocalizationHelper.GetLocalizedString("ui", "label_language");
            if (languageDropdownVisualElement != null) languageDropdownVisualElement.tooltip = LocalizationHelper.GetLocalizedString("tooltips", "language_tooltip");
            if (musicSlider != null) musicSlider.label = LocalizationHelper.GetLocalizedString("ui", "label_music");
            if (sfxSlider != null) sfxSlider.label = LocalizationHelper.GetLocalizedString("ui", "label_sfx");
            if (labeltitle != null) labeltitle.text = LocalizationHelper.GetLocalizedString("ui", "label_title");
            if (labelsubtitle != null) labelsubtitle.text = LocalizationHelper.GetLocalizedString("ui", "label_subtitle");

#if UNITY_LOCALIZATION

            var playButtonElem = root.Q<Button>("button_play");
            var openSettingsButtonElem = root.Q<Button>("button_open_settings");
            var resetMinigameButtonElem = root.Q<Button>("button_reset_minigame");
            var musicToggle = root.Q<Toggle>("toggle_music");
            var musicToggleElem = root.Q<Toggle>("toggle_music");
            var sfxToggleElem = root.Q<Toggle>("toggle_sfx");
            var sfxToggle = root.Q<Toggle>("toggle_sfx");
            if (playButtonElem != null) playButtonTooltip = playButtonElem.tooltip;
            if (openSettingsButtonElem != null) openSettingsButtonTooltip = openSettingsButtonElem.tooltip;
            if (resetMinigameButtonElem != null) resetMinigameButtonTooltip = resetMinigameButtonElem.tooltip;
            if (languageDropdownVisualElement != null) languageDropdownTooltip = languageDropdownVisualElement.tooltip;
            if (musicToggleElem != null) musicToggleTooltip = musicToggleElem.tooltip;
            if (sfxToggleElem != null) sfxToggleTooltip = sfxToggleElem.tooltip;

            // Only assign the tooltip key, not the localized string
            if (playButton != null) playButton.tooltip = "play_tooltip";
            if (openSettingsButton != null) openSettingsButton.tooltip = "settings_tooltip";
            if (resetMinigameButton != null) resetMinigameButton.tooltip = "reset_tooltip";
            if (languageDropdownVisualElement != null) languageDropdownVisualElement.tooltip = "language_tooltip";
            if (musicToggle != null) musicToggle.tooltip = "music_tooltip";
            if (sfxToggle != null) sfxToggle.tooltip = "sfx_tooltip";
            if (playButton != null && playButtonTooltip != null) playButton.tooltip = LocalizationHelper.GetLocalizedString("tooltips", "play_tooltip");
            if (openSettingsButton != null && openSettingsButtonTooltip != null) openSettingsButton.tooltip = LocalizationHelper.GetLocalizedString("tooltips", "settings_tooltip");
            if (resetMinigameButton != null && resetMinigameButtonTooltip != null) resetMinigameButton.tooltip = LocalizationHelper.GetLocalizedString("tooltips", "reset_tooltip");
            if (musicToggle != null && musicToggleTooltip != null) musicToggle.tooltip = LocalizationHelper.GetLocalizedString("tooltips", "music_tooltip");
            if (sfxToggle != null && sfxToggleTooltip != null) sfxToggle.tooltip = LocalizationHelper.GetLocalizedString("tooltips", "sfx_tooltip");
#endif

#if UNITY_WEBGL // && !UNITY_LOCALIZATION, // WebGL builds use the localization table approach
            string localeCode = LocalizationHelper.GetCurrentLocaleCode();

            string T(string table, string key)
            {
                var translation = LocalizationHelper.GetLocalizedString(table, key, key);
                if (!string.IsNullOrEmpty(translation))
                    return translation;
                Debug.LogWarning($"Key '{key}' not found in table '{table}' for locale '{localeCode}'. Using key as fallback.");
                return key;
            }

            if (playButton != null) playButton.text = T("ui", "btn_play_label");
            if (openSettingsButton != null) openSettingsButton.text = T("ui", "btn_settings_label");
            if (closeSettingsButton != null) closeSettingsButton.text = T("ui", "btn_close_settings_label");
            if (settingsLabel != null) settingsLabel.text = T("ui", "label_settings_title");
            if (languageLabel != null) languageLabel.text = T("ui", "label_language");

            // Assign tooltips using the correct table
            if (playButton != null) playButton.tooltip = "play_tooltip";
            if (openSettingsButton != null) openSettingsButton.tooltip = "settings_tooltip";
            if (resetMinigameButton != null) resetMinigameButton.tooltip = "reset_minigame_tooltip";
            if (musicSlider != null) musicSlider.tooltip = "music_tooltip";
            if (sfxSlider != null) sfxSlider.tooltip = "sfx_tooltip";
            if (languageDropdownVisualElement != null) languageDropdownVisualElement.tooltip = "language_tooltip";
            var musicToggle = root.Q<UnityEngine.UIElements.Toggle>("toggle_music");
            if (musicToggle != null) musicToggle.tooltip = "music_tooltip";
            var sfxToggle = root.Q<UnityEngine.UIElements.Toggle>("toggle_sfx");
            if (sfxToggle != null) sfxToggle.tooltip = "sfx_tooltip";

            // Assign the correct font for the current locale using LocalizationHelper
            var font = LocalizationHelper.GetFontForLocale(localeCode);
            if (font != null)
            {
                if (playButton != null) playButton.style.unityFontDefinition = new FontDefinition { fontAsset = font };
                if (openSettingsButton != null) openSettingsButton.style.unityFontDefinition = new FontDefinition { fontAsset = font };
                if (closeSettingsButton != null) closeSettingsButton.style.unityFontDefinition = new FontDefinition { fontAsset = font };
                if (settingsLabel != null) settingsLabel.style.unityFontDefinition = new FontDefinition { fontAsset = font };
                if (languageLabel != null) languageLabel.style.unityFontDefinition = new FontDefinition { fontAsset = font };
            }

            // tooltip font assignment: assign to the tooltip label if it exists
            TooltipManager.Instance?.SetFontAsset(LocalizationHelper.GetFontForCurrentLocale());
#endif
        }

        private void UpdateLanguageSprite()
        {
            if (_labelLanguage == null)
                return;

            // Load the sprite for the current locale
            var sprite = LocalizationHelper.LoadLocaleSprite();
            if (sprite != null)
            {
                // Convert Sprite to Texture2D
                var texture = sprite.texture;
                if (texture != null)
                {
                    _labelLanguage.style.backgroundImage = new StyleBackground(texture);
                }
            }
            else
            {
                // Optionally clear or set a default background
                _labelLanguage.style.backgroundImage = new StyleBackground(); // Clear the background image
            }
        }

        private void OnLocaleChanged()
        {
            RefreshLocalizedUI();
            RegisterTooltipEvents(uiDocument.rootVisualElement);
            TooltipManager.Instance?.SetFontAsset(LocalizationHelper.GetFontForCurrentLocale());
        }

        public static void RaiseLocalizedUIRefresh()
        {
            LocalizedUIRefreshRequested?.Invoke();
        }

        // Example: Call this method when the user selects a new language from the dropdown
        public void OnLanguageSelected(string localeCode)
        {
            LocalizationHelper.SetLocale(localeCode);
            LocalizationHelper.SaveLocaleToPrefs(localeCode);
            RaiseLocalizedUIRefresh();
        }

        // Example: Call this method from a "Reset Language" button to revert to system/browser default
        public void OnResetLanguageToSystemDefault()
        {
            LocalizationHelper.ResetLocaleToSystemDefault();
            RaiseLocalizedUIRefresh();
        }
    }
}