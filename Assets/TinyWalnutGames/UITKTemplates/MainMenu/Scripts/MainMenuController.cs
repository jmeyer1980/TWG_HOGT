using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEngine.TestRunner;
using UnityEngine.TextCore.Text;
#endif
using System.Collections.Generic;
using TinyWalnutGames.UITKTemplates.Tools;
using System;

namespace TinyWalnutGames.UITKTemplates.MainMenu
{
    public class MainMenuController : MonoBehaviour
    {
        public UIDocument uiDocument;

        // Reference to SettingsMenu (template-based, not UIDocument)
        public SettingsMenu settingsMenu;

        // scene to load on play
        [Tooltip("Scene to load when the player clicks the Play button. Defaults to the next scene in build settings.")]
        [SerializeField]
        private string nextSceneName = "LevelSelection";

        // Event to allow other scripts to refresh UI
        public static event System.Action LocalizedUIRefreshRequested;

        private const float TooltipMargin = 16f; // Space between mouse and tooltip
        private const float TooltipSafeBound = 8f; // Minimum space from screen edge

        private VisualElement _languageDropdown;
        private Label _labelLanguage;

        private bool tooltipManagerInitialized = false;
        private bool _localizationReady = false;
        private readonly bool _tooltipTemplateReady = false;

        private bool _preloadSubscribed = false;

        private Button playButton;
        private Button settingsButton;

        public MainMenuController(bool tooltipTemplateReady)
        {
            _tooltipTemplateReady = tooltipTemplateReady;
        }

        private readonly string sfxToggleTooltip;
        private readonly string musicToggleTooltip;
        private readonly string languageDropdownTooltip;
        private readonly string resetMinigameButtonTooltip;
        private readonly string openSettingsButtonTooltip;
        private readonly string playButtonTooltip;

        // --- UI Sound Effect Key Definitions ---
        // See documentation above for the meaning of each key.

        /// <summary>
        /// Safely plays a UI sound effect by key using AudioManager.Instance, if available.
        /// </summary>
        private static void PlayUISound(string key)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(key);
        }

        private void Awake()
        {       
            Debug.Log($"[MainMenuController] Awake: GameObject.activeSelf={gameObject.activeSelf}, enabled={enabled}, uiDocument={(uiDocument != null ? "assigned" : "null")}");

            // Ensure uiDocument is assigned, try to find it in the scene if not
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
                if (uiDocument == null)
                {
                    uiDocument = FindFirstObjectByType<UIDocument>();
                    if (uiDocument != null)
                        Debug.LogWarning("[MainMenuController] UIDocument was not assigned in Inspector, but was found in the scene and assigned.");
                }
            }

            if (uiDocument == null)
            {
                Debug.LogError("[MainMenuController] UIDocument is not assigned and could not be found in the scene. Please assign it in the Inspector.");
                return;
            }

            // Debug: Print rootVisualElement children
            if (uiDocument.rootVisualElement != null)
            {
                Debug.Log($"[MainMenuController] rootVisualElement child count: {uiDocument.rootVisualElement.childCount}");
                foreach (var child in uiDocument.rootVisualElement.Children())
                    Debug.Log($"[MainMenuController] Child: {child.name} ({child.GetType()})");
            }
            else
            {
                Debug.LogError("[MainMenuController] UIDocument rootVisualElement is null. Ensure the UIDocument is set up correctly and MainMenu.uxml is assigned.");
                return;
            }

            LocalizationHelper.SetLocale("zh-hans"); // Default to English locale
            LocalizationHelper.InitializeLocale();

            LocalizationHelper.LocaleChanged += OnLocaleChanged;
            LocalizationHelper.LocaleChanged += UpdateLanguageSprite;
            string localeCode = LocalizationHelper.GetCurrentLocaleCode();
            LocalizationHelper.SetLocale(localeCode);
            LocalizedUIRefreshRequested += RefreshLocalizedUI;

            Tooltip.TooltipTemplateLoaded += OnTooltipTemplateLoaded;
            Debug.Log("[MainMenuController] Subscribed to TooltipTemplateLoaded.");

            _localizationReady = LocalizationHelper.GetAvailableLocales().Count > 0;
            if (_localizationReady)
                OnLocalizationReady();

            // Register tooltips and refresh UI, but do NOT assign playButton/settingsButton here
            var root = uiDocument.rootVisualElement;
            RegisterTooltipEvents(root);
            RaiseLocalizedUIRefresh();

            if (TooltipManager.Instance == null)
            {
                Debug.LogWarning("[MainMenuController] TooltipManager.Instance is null. Ensure TooltipManager is present in the scene.");
#if UNITY_EDITOR
                if (Application.isEditor && Application.isPlaying)
                    return;
#endif
            }
            else
            {
                TryInitTooltipManager();
            }
        }

        private void OnEnable()
        {
            Debug.Log($"[MainMenuController] MainMenu enabled. GameObject.activeSelf={gameObject.activeSelf}, enabled={enabled}");
        }

        private void OnDisable()
        {
            Debug.LogWarning($"[MainMenuController] MainMenu DISABLED! This should not happen unless intentionally unloading or hiding the main menu. GameObject.activeSelf={gameObject.activeSelf}, enabled={enabled}");
#if UNITY_EDITOR
            Debug.Assert(false, "[MainMenuController] MainMenu was disabled unexpectedly.");
#endif
            LocalizationHelper.LocaleChanged -= OnLocaleChanged;
            LocalizationHelper.LocaleChanged -= UpdateLanguageSprite;
            LocalizationHelper.SetLocale("en");
            LocalizedUIRefreshRequested -= RefreshLocalizedUI;
            Tooltip.TooltipTemplateLoaded -= OnTooltipTemplateLoaded;
            Debug.Log("[MainMenuController] Unsubscribed from TooltipTemplateLoaded.");
        }

        private void OnDestroy()
        {
            if (_preloadSubscribed)
                PreloadAssets.PreloadComplete -= OnPreloadComplete;

            if (playButton != null)
                playButton.clicked -= OnPlayButtonClicked;
            if (settingsButton != null)
                settingsButton.clicked -= OnSettingsButtonClicked;
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

        private void Start()
        {
            Debug.Log($"[MainMenuController] Start: GameObject.activeSelf={gameObject.activeSelf}, enabled={enabled}");

            var root = uiDocument.rootVisualElement;

            // Play button: try direct, then via container
            playButton = root.Q<Button>("button_play");
            if (playButton == null)
            {
                var playContainer = root.Q<VisualElement>("PlayButtonStickyNote");
                if (playContainer != null)
                {
                    playButton = playContainer.Q<Button>("button_play");
                    if (playButton != null)
                        Debug.Log("[MainMenuController] playButton found via PlayButtonStickyNote container query.");
                }
            }

            // Settings button: try direct, then via container
            settingsButton = root.Q<Button>("button_open_settings");
            if (settingsButton == null)
            {
                var settingsContainer = root.Q<VisualElement>("SettingsButtonStickyNote");
                if (settingsContainer != null)
                {
                    settingsButton = settingsContainer.Q<Button>("button_open_settings");
                    if (settingsButton != null)
                        Debug.Log("[MainMenuController] settingsButton found via SettingsButtonStickyNote container query.");
                }
            }

            if (playButton == null)
                Debug.LogError("[MainMenuController] playButton is null in Start. Check UXML hierarchy and button name.");
            if (settingsButton == null)
                Debug.LogWarning("[MainMenuController] settingsButton not found in UI.");

            if (settingsMenu == null)
            {
                Debug.LogError("[MainMenuController] settingsMenu is null in Start. Please assign it in the inspector or initialize it before use.");
                return;
            }

            // Ensure settingsMenu is initialized with the main UI root
            if (settingsMenu != null && root != null)
            {
                settingsMenu.Initialize(root);
            }

            if (playButton != null)
                playButton.clicked += OnPlayButtonClicked;
            if (settingsButton != null)
                settingsButton.clicked += OnSettingsButtonClicked;

            if (PreloadAssets.Instance != null && !PreloadAssets.Instance.IsReady)
            {
                Debug.Log("[MainMenuController] Waiting for PreloadAssets to complete before initializing menu.");
                PreloadAssets.PreloadComplete += OnPreloadComplete;
                _preloadSubscribed = true;
            }
            else
            {
                OnPreloadComplete();
            }
        }

        private void OnPreloadComplete()
        {
            Debug.Log("[MainMenuController] PreloadAssets.PreloadComplete fired. Now initializing menu logic.");
            // Only load MainMenu if we're not already in the MainMenu scene
            if (SceneManager.GetActiveScene().name != "MainMenu")
            {
                PreloadAssets.LoadSceneWhenReady("MainMenu");
            }
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
            if (element is null)
            {
                throw new ArgumentNullException(nameof(element));
            }

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

            var labeltitle = root.Q<UnityEngine.UIElements.Label>("label_title");
            var labelsubtitle = root.Q<UnityEngine.UIElements.Label>("label_subtitle");

            // Use LocalizationHelper to get localized strings
            if (playButton != null) playButton.text = LocalizationHelper.GetLocalizedString("ui", "btn_play_label");
            if (openSettingsButton != null) openSettingsButton.text = LocalizationHelper.GetLocalizedString("ui", "btn_settings_label");
            if (labeltitle != null) labeltitle.text = LocalizationHelper.GetLocalizedString("ui", "label_title");
            if (labelsubtitle != null) labelsubtitle.text = LocalizationHelper.GetLocalizedString("ui", "label_subtitle");

#if UNITY_LOCALIZATION

            var playButtonElem = root.Q<Button>("button_play");
            var openSettingsButtonElem = root.Q<Button>("button_open_settings");
            if (playButtonElem != null) playButtonTooltip = playButtonElem.tooltip;
            if (openSettingsButtonElem != null) openSettingsButtonTooltip = openSettingsButtonElem.tooltip;

            // Only assign the tooltip key, not the localized string
            if (playButton != null) playButton.tooltip = "play_tooltip";
            if (openSettingsButton != null) openSettingsButton.tooltip = "settings_tooltip";
            if (playButton != null && playButtonTooltip != null) playButton.tooltip = LocalizationHelper.GetLocalizedString("tooltips", "play_tooltip");
            if (openSettingsButton != null && openSettingsButtonTooltip != null) openSettingsButton.tooltip = LocalizationHelper.GetLocalizedString("tooltips", "settings_tooltip");
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

            // Assign tooltips using the correct table
            if (playButton != null) playButton.tooltip = "play_tooltip";
            if (openSettingsButton != null) openSettingsButton.tooltip = "settings_tooltip";

            // Assign the correct font for the current locale using LocalizationHelper
            var font = LocalizationHelper.GetFontForLocale(localeCode);
            if (font != null)
            {
                if (playButton != null) playButton.style.unityFontDefinition = new FontDefinition { fontAsset = font };
                if (openSettingsButton != null) openSettingsButton.style.unityFontDefinition = new FontDefinition { fontAsset = font };
            }

            // tooltip font assignment: assign to the tooltip label if it exists
            if (TooltipManager.Instance != null)
            {
                TooltipManager.Instance.SetFontAsset(LocalizationHelper.GetFontForCurrentLocale());
            }
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
            if (TooltipManager.Instance != null)
            {
                TooltipManager.Instance.SetFontAsset(LocalizationHelper.GetFontForCurrentLocale());
            }
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

        // Ensure you do NOT call SceneManager.LoadScene directly before PreloadAssets.IsReady is true.
        // Always use PreloadAssets.LoadSceneWhenReady for all scene transitions after init.

        private void OnPlayButtonClicked()
        {
            Debug.Log("[MainMenuController] Play button clicked.");
            if (!PreloadAssets.CanLoadScenes)
            {
                Debug.LogWarning("[MainMenuController] Cannot load scene: PreloadAssets not ready.");
                return;
            }
            // Only load if not already auto-advancing
            if (!string.IsNullOrEmpty(PreloadAssets.Instance.autoAdvanceSceneName))
            {
                Debug.LogWarning("[MainMenuController] autoAdvanceSceneName is set. Manual scene load may conflict.");
            }
            PreloadAssets.LoadSceneWhenReady("LevelSelection");
        }

        private void OnSettingsButtonClicked()
        {
            Debug.Log("[MainMenuController] Settings button clicked.");
            ShowSettingsPanel();
        }

        private void ShowSettingsPanel()
        {
            if (settingsMenu == null)
            {
                Debug.LogError("[MainMenuController] settingsMenu is null. Cannot show settings panel.");
                return;
            }
            settingsMenu.Show();
        }
    }
}