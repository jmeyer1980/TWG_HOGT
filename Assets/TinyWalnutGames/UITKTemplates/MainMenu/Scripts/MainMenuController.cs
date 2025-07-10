using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEngine.TestRunner;
using UnityEngine.TextCore.Text;
#endif
#if UNITY_LOCALIZATION
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.Localization.SmartFormat.Core.Parsing;
using UnityEngine.Localization.SmartFormat.Core.Formatting;
#endif
using System.Collections.Generic;
using TinyWalnutGames.UITKTemplates.Tools;
using System;
using System.Collections;

namespace TinyWalnutGames.UITKTemplates.MainMenu
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument; // Ensure this is assigned in the inspector
        [SerializeField] private VisualTreeAsset uiVisualTreeAsset; // The UXML template for the main menu UI

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

        private readonly VisualElement _languageDropdown;
        private readonly Label _labelLanguage;

        private bool tooltipManagerInitialized = false;
        private bool _localizationReady = false;
        private readonly bool _tooltipTemplateReady = false;

        private bool _preloadSubscribed = false;

        private Button playButton;
        private Button settingsButton;

        private readonly string sfxToggleTooltip;
        private readonly string musicToggleTooltip;
        private readonly string languageDropdownTooltip;
        private readonly string resetMinigameButtonTooltip;
        private string openSettingsButtonTooltip;
        private string playButtonTooltip;

        // Add this field to make config accessible throughout the class
        private UIDocumentValidationConfig config;

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

        // FIX: Remove 'yield return' from Awake (cannot use yield in void methods).
        // Move all coroutine logic out of Awake and into the coroutine itself.

        private void Awake()
        {
            Debug.Log($"[MainMenuController] Awake: GameObject.activeSelf={gameObject.activeSelf}, enabled={enabled}, uiDocument={(uiDocument != null ? "assigned" : "null")}");

            // Initialize config as a field so it can be reused elsewhere
            config = new UIDocumentValidationConfig
            {
                NamedElements = new Dictionary<string, System.Type>
                {
                    { "button_play", typeof(Button) },
                    { "button_open_settings", typeof(Button) }
                },
                ProgressBarName = null // Not required in main menu
            };
            UIDocumentValidator.ValidateOrFixUIDocument(this, ref uiDocument, uiVisualTreeAsset, config, out var root, out var progressBar, true);

            // Only proceed if rootVisualElement is valid
            if (uiDocument.rootVisualElement == null)
            {
                Debug.LogError("[MainMenuController] UIDocument rootVisualElement is null. Ensure the UIDocument is set up correctly and MainMenu.uxml is assigned.");
                return; // Exit early if rootVisualElement is not valid
            }

            // Debug: Print rootVisualElement children
            Debug.Log($"[MainMenuController] rootVisualElement child count: {uiDocument.rootVisualElement.childCount}");
            foreach (var child in uiDocument.rootVisualElement.Children())
                Debug.Log($"[MainMenuController] Child: {child.name} ({child.GetType()})");

            // Start the coroutine to get and set the current locale code
            StartCoroutine(GetAndSetCurrentLocaleCode());
            Debug.Log("[MainMenuController] Left GetAndSetCurrentLocaleCode coroutine.");
            Debug.Log("[MainMenuController] Entering ValidateOrFixLocale method.");
            // Validate or fix the UIDocument and locale: ValidateOrFixLocale(MonoBehaviour context, bool showDevToast, out bool wasFixed)
            UIDocumentValidator.ValidateOrFixLocale(this, true, out bool wasFixed);

            if (settingsMenu == null)
            {
                settingsMenu = SettingsMenu.Instance;
                if (settingsMenu == null)
                {
                    Debug.LogError("[MainMenuController] SettingsMenu.Instance is null. Settings menu will not function.");
                }
            }
            // Initialize settings menu with the main UI root
            if (settingsMenu != null && uiDocument != null && uiDocument.rootVisualElement != null)
            {
                Debug.Log("[MainMenuController] Initializing SettingsMenu with rootVisualElement from UIDocument.");
                // Initialize settings menu with the root VisualElement
                SettingsMenu.Instance.Initialize(uiDocument.rootVisualElement);
            }
        }

        private void OnEnable()
        {
            Debug.Log($"[MainMenuController] MainMenu enabled. GameObject.activeSelf={gameObject.activeSelf}, enabled={enabled}");

            // Subscribe to centralized locale change event
            if (UIRootManager.Instance != null)
                UIRootManager.Instance.LocaleChanged += RefreshLocalizedUI;
            
            if (settingsMenu == null)
            {
                settingsMenu = SettingsMenu.Instance;
                if (settingsMenu == null)
                {
                    Debug.LogError("[MainMenuController] SettingsMenu.Instance is null. Settings menu will not function.");
                }
            }
            // Initialize settings menu with the main UI root
            if (settingsMenu != null && uiDocument != null && uiDocument.rootVisualElement != null)
            {
                settingsMenu.Initialize(uiDocument.rootVisualElement);
            }

            LocalizedUIRefreshRequested?.Invoke();
        }

        private void OnDisable()
        {
            Debug.LogWarning($"[MainMenuController] MainMenu DISABLED! This should not happen unless intentionally unloading or hiding the main menu. GameObject.activeSelf={gameObject.activeSelf}, enabled={enabled}");
#if UNITY_EDITOR
            Debug.Assert(false, "[MainMenuController] MainMenu was disabled unexpectedly.");
#endif

            // Unsubscribe from centralized locale change event
            if (UIRootManager.Instance != null)
                UIRootManager.Instance.LocaleChanged -= RefreshLocalizedUI;

            LocalizedUIRefreshRequested -= RefreshLocalizedUI;
            Tooltip.TooltipTemplateLoaded -= OnTooltipTemplateLoaded;
            Debug.Log("[MainMenuController] Unsubscribed from TooltipTemplateLoaded.");
        }

        private void OnDestroy()
        {
            if (_preloadSubscribed)
                AssetPreloader.Instance.PreloadComplete -= OnPreloadComplete;

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
                // TryInitTooltipManager();
            }
        }

        private void OnLocalizationReady()
        {
            Debug.Log("[MainMenuController] OnLocalizationReady: Localization IS NOW READY.");
            _localizationReady = true;
            Debug.Log("[MainMenuController] Localization is now ready.");

            // Explicitly re-initialize tooltips with latest localization if possible
            if (TooltipManager.Instance != null && Tooltip.IsTemplateReady)
            {
                var root = uiDocument.rootVisualElement;
                TooltipManager.Instance.Initialize(root);
                RegisterTooltipEvents(root);
                Debug.Log("[MainMenuController] TooltipManager re-initialized after localization became ready.");
            }

            RefreshLocalizedUI(); // Ensure UI and tooltips use latest localization

            // Fallback: still call TryInitTooltipManager for any additional logic
            TryInitTooltipManager();
        }

        // Coroutine to get and set the current locale code from the UIRootManager
        private IEnumerator GetAndSetCurrentLocaleCode()
        {
            Debug.Log("[MainMenuController] GetAndSetCurrentLocaleCode coroutine started.");
            
            // Wait for both UIRootManager and UI to be ready
            Debug.Log("[MainMenuController] Waiting for UIRootManager and UIDocument to be ready...");
            while (UIRootManager.Instance == null || UIRootManager.Instance.CurrentLocaleCode() == null || uiDocument == null || uiDocument.rootVisualElement == null)
            { 
                Debug.Log("[MainMenuController] Waiting for UIRootManager and UIDocument to be ready...");
                if (UIRootManager.Instance == null)
                    Debug.LogWarning("[MainMenuController] UIRootManager is not initialized yet and never will be unless we exit the coroutine?");                   
                if (uiDocument == null)
                    Debug.LogWarning("[MainMenuController] UIDocument is not assigned or initialized yet.");
                if (uiDocument.rootVisualElement == null)
                    Debug.LogWarning("[MainMenuController] UIDocument rootVisualElement is not ready yet.");
                if (LocalizationHelper.GetCurrentLocaleCode() == null)
                    Debug.LogWarning("[MainMenuController] LocalizationHelper.GetCurrentLocaleCode() is null. Waiting for localization to be ready.");
                else
                    Debug.Log($"[MainMenuController] Current locale code: {LocalizationHelper.GetCurrentLocaleCode()}");
                // Yield until the next frame to avoid blocking the main thread
                yield return null;
            }
            
            Debug.Log("[MainMenuController] UIRootManager and UIDocument are ready, proceeding to set locale.");
            
            string localeCode = UIRootManager.Instance.CurrentLocaleCode();
            
            Debug.Log($"[MainMenuController] Current locale code already set from UIRootManager: {localeCode}");
            
            // Only set locale if not already set
            if (LocalizationHelper.GetCurrentLocaleCode() != localeCode)
            {
                Debug.Log($"[MainMenuController] Was not already set. Setting locale to: {localeCode}");
                LocalizationHelper.SetLocale(localeCode);
                PlayerPrefs.SetString("selected_locale", localeCode);
                PlayerPrefs.Save();
                Debug.Log($"[MainMenuController] Current locale set to: {localeCode}");
            }
            
            Debug.Log("[MainMenuController] Locale set, now waiting for localization to be ready.");

            // Wait for fixed update to ensure the locale is set before refreshing UI
            yield return new WaitForFixedUpdate(); // this exits on the next frame, the next line does not fire?
            OnLocalizationReady();
            // Use shared validator for UIDocument and locale
            UIDocumentValidator.ValidateOrFixUIDocument(this, ref uiDocument, uiVisualTreeAsset, config, out _, out _, true);
            Debug.Log("[MainMenuController] GetAndSetCurrentLocaleCode coroutine completed.");
        }

        private void TryInitTooltipManager()
        {
            Debug.Log("[MainMenuController] TryInitTooltipManager invoked.");
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

            // Subscribe to centralized locale change event
            if (UIRootManager.Instance != null)
                UIRootManager.Instance.LocaleChanged += RefreshLocalizedUI;

            // Register tooltips and other UI only after root is valid
            Tooltip.TooltipTemplateLoaded += OnTooltipTemplateLoaded;

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
                settingsMenu = SettingsMenu.Instance;
                if (settingsMenu == null)
                {
                    Debug.LogError("[MainMenuController] SettingsMenu.Instance is null. Settings menu will not function.");
                }
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

            if (AssetPreloader.Instance != null && !AssetPreloader.Instance.IsReady)
            {
                Debug.Log("[MainMenuController] Waiting for PreloadAssets to complete before initializing menu.");
                AssetPreloader.Instance.PreloadComplete += OnPreloadComplete;
                _preloadSubscribed = true;
            }
            else
            {
                OnPreloadComplete();
            }

            // Start coroutine to attach UIDocument to UIRootManager when ready
            StartCoroutine(AttachUIDocumentWhenReady());
        }

        // Add this coroutine to ensure UIDocument is attached only when ready
        private IEnumerator AttachUIDocumentWhenReady()
        {
            // Wait for UIRootManager and UIDocument to be ready
            while (UIRootManager.Instance == null ||
                   uiDocument == null ||
                   uiDocument.rootVisualElement == null)
            {
                yield return null;
            }
            Debug.Log("[MainMenuController] Assigning UIDocument to UIRootManager (no parenting, just assignment).");
            // UIRootManager.Instance.AttachSceneUIDocument(uiDocument);
        }

        private void OnPreloadComplete()
        {
            Debug.Log("[MainMenuController] PreloadAssets.PreloadComplete fired. Now initializing menu logic.");
            // Only load MainMenu if we're not already in the MainMenu scene
            if (SceneManager.GetActiveScene().name != "MainMenu")
            {
                AssetPreloader.LoadSceneWhenReady("MainMenu");
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
                    // For sliders, register value change to show tooltip with value
                    if (element is Slider slider)
                    {
                        RegisterSliderTooltipHandlers(slider);
                    }
                    else
                    {
                        RegisterTooltipHandlers(element, element.tooltip);
                    }
                }
            }
        }

        // New: Register slider value change to show tooltip with value
        private void RegisterSliderTooltipHandlers(Slider slider)
        {
            slider.RegisterCallback<PointerEnterEvent>(evt =>
            {
                ShowTooltip(slider, slider.value);
                PlayUISound("focus");
            });
            slider.RegisterCallback<PointerLeaveEvent>(_ =>
            {
                HideTooltip();
            });
            slider.RegisterCallback<PointerMoveEvent>(evt =>
            {
                MoveTooltip(evt.position);
            });
            slider.RegisterValueChangedCallback(evt =>
            {
                ShowTooltip(slider, evt.newValue);
                PlayUISound("drag");
            });
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

            if (TooltipManager.Instance == null || !TooltipManager.Instance.IsInitialized) return;
            Vector2 mousePos = Mouse.current.position.ReadValue();
            TooltipManager.Instance.Show(tooltipText, mousePos);
        }

        // Show tooltip for sliders with value
        private void ShowTooltip(VisualElement element, float value)
        {
            if (TooltipManager.Instance == null || !TooltipManager.Instance.IsInitialized) return;
            Vector2 mousePos = Mouse.current.position.ReadValue();
            TooltipManager.Instance.ShowWithValue(element.name, value, mousePos);
            PlayUISound("drag"); // Play 'drag' sound when adjusting volume
        }

        private void MoveTooltip(Vector2 mousePosition)
        {
            if (TooltipManager.Instance == null || !TooltipManager.Instance.IsInitialized) return;
            TooltipManager.Instance.Move(mousePosition);
        }

        private void HideTooltip()
        {
            if (TooltipManager.Instance == null || !TooltipManager.Instance.IsInitialized) return;
            TooltipManager.Instance.Hide();
        }

        private void RefreshLocalizedUI()
        {
#if UNITY_LOCALIZATION
            var root = uiDocument.rootVisualElement;
            var playButton = root.Q<UnityEngine.UIElements.Button>("button_play");
            var openSettingsButton = root.Q<UnityEngine.UIElements.Button>("button_open_settings");

            var labeltitle = root.Q<UnityEngine.UIElements.Label>("label_title");
            var labelsubtitle = root.Q<UnityEngine.UIElements.Label>("label_subtitle");

            // Use LocalizationHelper to get localized strings
            if (playButton != null) playButton.text = LocalizationHelper.GetLocalizedString("HOGT_UI", "btn_play_label");
            if (openSettingsButton != null) openSettingsButton.text = LocalizationHelper.GetLocalizedString("HOGT_UI", "btn_settings_label");
            if (labeltitle != null) labeltitle.text = LocalizationHelper.GetLocalizedString("HOGT_UI", "label_title");
            if (labelsubtitle != null) labelsubtitle.text = LocalizationHelper.GetLocalizedString("HOGT_UI", "label_subtitle");
            // Assign tooltips using the correct table
            if (playButton != null) playButton.tooltip = LocalizationHelper.GetLocalizedString("tooltips", "play_tooltip");
            if (openSettingsButton != null) openSettingsButton.tooltip = LocalizationHelper.GetLocalizedString("tooltips", "settings_tooltip");
            // Assign the correct font for the current locale using LocalizationHelper
            var font = LocalizationHelper.GetFontForCurrentLocale();
            if (font != null)
            {
                if (playButton != null) playButton.style.unityFontDefinition = new FontDefinition { fontAsset = font };
                if (openSettingsButton != null) openSettingsButton.style.unityFontDefinition = new FontDefinition { fontAsset = font };

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

#if UNITY_WEBGL && !UNITY_LOCALIZATION // WebGL builds use the localization table approach
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
            if (!AssetPreloader.Instance.CanLoadScenes)
            {
                Debug.LogWarning("[MainMenuController] Cannot load scene: PreloadAssets not ready.");
                return;
            }
            // Only load if not already auto-advancing
            if (!string.IsNullOrEmpty(AssetPreloader.Instance.autoAdvanceSceneName))
            {
                Debug.LogWarning("[MainMenuController] autoAdvanceSceneName is set. Manual scene load may conflict.");
            }
            AssetPreloader.LoadSceneWhenReady("LevelSelection");
        }

        private void OnSettingsButtonClicked()
        {
            Debug.Log("[MainMenuController] Settings button clicked.");
            ShowSettingsPanel();
        }

        private void ShowSettingsPanel()
        {
            // Ensure settingsMenu is initialized
            if (settingsMenu == null)
                settingsMenu = SettingsMenu.Instance;
            SettingsMenu.Instance.Initialize(uiDocument.rootVisualElement);
            // is it still null?
            if (settingsMenu == null)
            {
                Debug.LogError("[MainMenuController] SettingsMenu is not initialized. Cannot show settings panel.");
                return;
            }
            else // show the settings menu
            {
                settingsMenu.Show();
            }
        }
    }
}