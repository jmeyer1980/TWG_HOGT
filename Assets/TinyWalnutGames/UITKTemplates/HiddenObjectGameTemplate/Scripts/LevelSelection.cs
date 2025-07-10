using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Localization;
using UnityEngine.UIElements;
using TinyWalnutGames.UITKTemplates.MainMenu;
using TinyWalnutGames.UITKTemplates.Tools;
using System.Collections.Generic;
using System.Linq;

namespace TinyWalnutGames.UITKTemplates.HOGT
{
    // Manages level progression and loading
    public class LevelSelection : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument; // Ensure this is assigned in the inspector
        [SerializeField] private UIDocument expectedUIRoot; // Reference to the expected root UIDocument, set in the Inspector
        [SerializeField] private VisualTreeAsset uiVisualTreeAsset; // Reference to the UXML template for the UI Document
        [SerializeField] private VisualTreeAsset expecteduiVisualTreeAsset; // Reference to the expected UXML template for the UI Document
        public VisualTreeAsset levelCardTemplate; // Reference to the level card template. Is this correct? The template is saved as a UI Document
        public int currentLevelIndex = 0; // Highest unlocked level
        public LevelCard[] levelCards; // Assign these in the Inspector

        // Reference to the root visual element
        private VisualElement root;
        // Reference to the level card list
        private VisualElement levelCardList;
        // Reference to the level card template
        private readonly VisualElement levelCardTemplateElement;

        private LevelCard selectedLevelCard = null;
        private int selectedLevelIndex = -1;

        private Button playButton;
        private Button returnToMenuButton;

        private bool _preloadSubscribed = false;

        // Add flag to track when cards are loaded
        private bool levelCardsLoaded = false;

        // Add: Validation config for modular UI validation
        private UIDocumentValidationConfig _validationConfig;

        // Add at class scope:
        private Dictionary<int, LevelCard> _cardLookup;

        private void Awake()
        {
            Debug.Log($"[LevelSelection][Awake] Entered. uiDocument: {(uiDocument == null ? "NULL" : uiDocument.ToString())}, expectedUIRoot: {(expectedUIRoot == null ? "NULL" : expectedUIRoot.ToString())}");

            if (FindFirstObjectByType<LevelSelection>() == null)
            {
                Debug.Log("[LevelSelection][Awake] LevelManager not found in the scene. Attempting to find it.");
                this.GetComponent<LevelSelection>();
            }
            else
            {
                Debug.Log("[LevelSelection][Awake] LevelManager already exists in the scene.");

                if (levelCards == null || levelCards.Length == 0)
                {
                    Debug.LogError("[LevelSelection][Awake] No level cards assigned! Please assign them in the Inspector.");
                }
            }

            if (levelCardTemplate == null)
            {
                Debug.LogError("[LevelSelection][Awake] Level card template is not assigned! Please assign it in the Inspector.");
                levelCardTemplate = Resources.Load<VisualTreeAsset>("LevelCardTemplate");
                if (levelCardTemplate == null)
                {
                    Debug.LogError("[LevelSelection][Awake] Level card template not found in Resources. Please assign it in the Inspector.");
                }
                return;
            }

            // Log UIDocument assignment state
            Debug.Log($"[LevelSelection][Awake] Before validation: uiDocument: {(uiDocument == null ? "NULL" : uiDocument.ToString())}, expectedUIRoot: {(expectedUIRoot == null ? "NULL" : expectedUIRoot.ToString())}");

            _validationConfig = new UIDocumentValidationConfig
            {
                NamedElements = new Dictionary<string, System.Type>
                {
                    { "level_card_list", typeof(VisualElement) },
                    { "button_play", typeof(Button) },
                    { "button_return_to_menu", typeof(Button) }
                },
                ProgressBarName = null
            };

            LoadLevelCards();

            Debug.Log($"[LevelSelection][Awake] LevelManager initialized with uiDocument: {(uiDocument == null ? "NULL" : uiDocument.ToString())}, expectedUIRoot: {(expectedUIRoot == null ? "NULL" : expectedUIRoot.ToString())}.");

            TinyWalnutGames.UITKTemplates.Tools.LocalizationHelper.LocaleChanged += RefreshLocalizedUI;
        }

        private void OnValidate()
        {
            Debug.Log($"[LevelSelection][OnValidate] Called. uiDocument: {(uiDocument == null ? "NULL" : uiDocument.ToString())}, expectedUIRoot: {(expectedUIRoot == null ? "NULL" : expectedUIRoot.ToString())}");

            if (levelCards == null || levelCards.Length == 0)
            {
                Debug.LogWarning("[LevelSelection][OnValidate] Level cards array is empty. Please assign level cards in the Inspector.");
            }
            if (levelCardTemplate == null)
            {
                Debug.LogWarning("[LevelSelection][OnValidate] Level card template is not assigned. Please assign a valid VisualTreeAsset.");
            }
            if (expectedUIRoot == null && uiDocument != null)
            {
                expectedUIRoot = uiDocument;
                Debug.Log("[LevelSelection][OnValidate] expectedUIRoot assigned from uiDocument.");
            }
            else if (uiDocument == null && expectedUIRoot != null)
            {
                uiDocument = expectedUIRoot;
                Debug.Log("[LevelSelection][OnValidate] uiDocument assigned from expectedUIRoot.");
            }
            if (uiVisualTreeAsset == null && expecteduiVisualTreeAsset != null)
            {
                uiVisualTreeAsset = expecteduiVisualTreeAsset;
                Debug.Log("[LevelSelection][OnValidate] uiVisualTreeAsset assigned from expecteduiVisualTreeAsset.");
            }
            else if (expecteduiVisualTreeAsset == null && uiVisualTreeAsset != null)
            {
                expecteduiVisualTreeAsset = uiVisualTreeAsset;
                Debug.Log("[LevelSelection][OnValidate] expecteduiVisualTreeAsset assigned from uiVisualTreeAsset.");
            }

            // Log rootVisualElement state
            if (uiDocument != null)
                Debug.Log($"[LevelSelection][OnValidate] uiDocument.rootVisualElement: {(uiDocument.rootVisualElement == null ? "NULL" : uiDocument.rootVisualElement.ToString())}");
            if (expectedUIRoot != null)
                Debug.Log($"[LevelSelection][OnValidate] expectedUIRoot.rootVisualElement: {(expectedUIRoot.rootVisualElement == null ? "NULL" : expectedUIRoot.rootVisualElement.ToString())}");

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.EditorApplication.RepaintHierarchyWindow();
#endif
        }

        private void OnEnable()
        {
            // Subscribe to centralized locale change event
            if (UIRootManager.Instance != null)
                UIRootManager.Instance.LocaleChanged += RefreshLocalizedUI;
        }

        private void OnDisable()
        {
            // Unsubscribe from centralized locale change event
            if (UIRootManager.Instance != null)
                UIRootManager.Instance.LocaleChanged -= RefreshLocalizedUI;
        }

        private void OnDestroy()
        {
            if (_preloadSubscribed)
                AssetPreloader.Instance.PreloadComplete -= OnPreloadComplete;

            TinyWalnutGames.UITKTemplates.Tools.LocalizationHelper.LocaleChanged -= RefreshLocalizedUI;
        }

        /// <summary>
        /// Registers tooltip events for UI elements in the root visual element.
        /// </summary>
        /// <param name="root"></param>
        private void RegisterTooltipEvents(VisualElement root)
        {
            // Register tooltips for the play button
            var playButton = root.Q<Button>("button_play");
            if (playButton != null && !string.IsNullOrEmpty(playButton.tooltip))
            {
                playButton.RegisterCallback<PointerEnterEvent>(evt =>
                {
                    if (TooltipManager.Instance != null)
                        TooltipManager.Instance.Show(playButton.tooltip, evt.position);
                });
                playButton.RegisterCallback<PointerLeaveEvent>(_ =>
                {
                    if (TooltipManager.Instance != null)
                        TooltipManager.Instance.Hide();
                });
                playButton.RegisterCallback<PointerMoveEvent>(evt =>
                {
                    if (TooltipManager.Instance != null)
                        TooltipManager.Instance.Move(evt.position);
                });
            }
            // Register tooltips for the back button
            var returnToMenuButton = root.Q<Button>("button_return_to_menu");
            if (returnToMenuButton != null && !string.IsNullOrEmpty(returnToMenuButton.tooltip))
            {
                returnToMenuButton.RegisterCallback<PointerEnterEvent>(evt =>
                {
                    if (TooltipManager.Instance != null)
                        TooltipManager.Instance.Show(returnToMenuButton.tooltip, evt.position);
                });
                returnToMenuButton.RegisterCallback<PointerLeaveEvent>(_ =>
                {
                    if (TooltipManager.Instance != null)
                        TooltipManager.Instance.Hide();
                });
                returnToMenuButton.RegisterCallback<PointerMoveEvent>(evt =>
                {
                    if (TooltipManager.Instance != null)
                        TooltipManager.Instance.Move(evt.position);
                });
            }
            // Register tooltips for level cards
            if (levelCardList != null)
            {
                foreach (var levelCard in levelCardList.Children())
                {
                    var levelIndexLabel = levelCard.Q<Label>("lvl_int");
                    if (levelIndexLabel != null && !string.IsNullOrEmpty(levelIndexLabel.tooltip))
                    {
                        levelIndexLabel.RegisterCallback<PointerEnterEvent>(evt =>
                        {
                            if (TooltipManager.Instance != null)
                                TooltipManager.Instance.Show(levelIndexLabel.tooltip, evt.position);
                        });
                        levelIndexLabel.RegisterCallback<PointerLeaveEvent>(_ =>
                        {
                            if (TooltipManager.Instance != null)
                                TooltipManager.Instance.Hide();
                        });
                        levelIndexLabel.RegisterCallback<PointerMoveEvent>(evt =>
                        {
                            if (TooltipManager.Instance != null)
                                TooltipManager.Instance.Move(evt.position);
                        });
                    }
                }
            }
        }


        /// <summary>
        /// Refreshes the localized UI elements based on the current locale.
        /// 100% written by Jerry
        /// </summary>
        private void RefreshLocalizedUI()
        {
            if (levelCardList == null || levelCards == null)
            {
                Debug.LogWarning("[LevelUIController] Missing levelCardList or levelCards array. Aborting localization refresh.");
                return;
            }

            // Build a quick lookup by levelIndex (only once or when levelCards changes)
            if (_cardLookup == null || _cardLookup.Count != levelCards.Length)
            {
                _cardLookup = levelCards.ToDictionary(card => card.levelIndex);
            }

            // Grab locale once per refresh
            string locale = LocalizationHelper.GetCurrentLocaleCode();

            foreach (VisualElement levelCard in levelCardList.Children())
            {
                // 1) Safely get & parse level index
                var idxLabel = levelCard.Q<Label>("lvl_int");
                if (idxLabel == null || !int.TryParse(idxLabel.text, out int levelIndex))
                    continue;

                // 2) Lookup data object
                if (!_cardLookup.TryGetValue(levelIndex, out LevelCard cardData))
                    continue;

                // 3) Localize & assign name
                string nameKey = cardData.levelNameKey;
#if UNITY_WEBGL
                string localizedName = LocalizationHelper.GetLocalizedStringForLocale(
                    "HOGT_Level_Cards", nameKey, locale, cardData.levelName
                );
#else
                var localeObj = LocalizationHelper.GetLocaleFromCode(locale);
                string localizedName = LocalizationHelper.GetLocalizedStringForLocale(
                    "HOGT_Level_Cards", nameKey, localeObj, cardData.levelName
                );
#endif
                idxLabel.text = localizedName;
                Debug.Log($"[LevelUIController] Localized name for level {levelIndex}: {localizedName}");

                // 4) Localize & assign description
                var descLabel = levelCard.Q<Label>("lvl_description");
                if (descLabel != null)
                {
                    string descKey = cardData.levelDescriptionKey;
#if UNITY_WEBGL
                    string localizedDesc = LocalizationHelper.GetLocalizedStringForLocale(
                        "HOGT_Level_Cards", descKey, locale, cardData.levelDescription
                    );
#else
                    string localizedDesc = LocalizationHelper.GetLocalizedStringForLocale(
                        "HOGT_Level_Cards", descKey, localeObj, cardData.levelDescription
                    );
#endif
                    descLabel.text = localizedDesc;
                    Debug.Log($"[LevelUIController] Localized description for level {levelIndex}: {localizedDesc}");
                }

                // 5) Register tooltip events for localized elements
                RegisterTooltipEvents(root);
            }
        }

        // Initialize the level manager
        private void Start()
        {
            Debug.Log($"[LevelSelection][Start] Called. uiDocument: {(uiDocument == null ? "NULL" : uiDocument.ToString())}, expectedUIRoot: {(expectedUIRoot == null ? "NULL" : expectedUIRoot.ToString())}");
            if (AssetPreloader.Instance != null && !AssetPreloader.Instance.IsReady)
            {
                Debug.Log("[LevelSelection][Start] Waiting for PreloadAssets to complete before starting level logic.");
                AssetPreloader.Instance.PreloadComplete += OnPreloadComplete;
                _preloadSubscribed = true;
            }
            else
            {
                Debug.Log("[LevelSelection][Start] PreloadAssets is ready, starting level logic immediately.");
            }

            expectedUIRoot = uiDocument;
            Debug.Log($"[LevelSelection][Start] expectedUIRoot set to uiDocument. expectedUIRoot: {(expectedUIRoot == null ? "NULL" : expectedUIRoot.ToString())}");

            if (uiDocument == null)
            {
                Debug.LogError("[LevelSelection][Start] uiDocument is NULL at Start! This must be assigned in the Inspector or by script.");
            }
            else
            {
                root = uiDocument.rootVisualElement;
                Debug.Log($"[LevelSelection][Start] rootVisualElement: {(root == null ? "NULL" : root.ToString())}");
            }

            UIDocumentValidator.ValidateOrFixUIDocument(
                this, ref uiDocument, uiVisualTreeAsset, _validationConfig, out var docValid, out var docMsg, true);
            Debug.Log($"[LevelSelection][Start] UIDocumentValidator.ValidateOrFixUIDocument result: valid={docValid}, msg={docMsg}");

            if (uiVisualTreeAsset == null || expecteduiVisualTreeAsset == null)
            {
                Debug.LogError("[LevelSelection][Start] UI Visual Tree Asset or Expected UI Visual Tree Asset is not assigned! Please assign them in the Inspector.");
                return;
            }

            if (root != null && expectedUIRoot != null && root != expectedUIRoot.rootVisualElement)
            {
#if UNITY_EDITOR
                string expectedUxmlName = uiVisualTreeAsset != null ? uiVisualTreeAsset.name : "NULL";
                int expectedUxmlId = uiVisualTreeAsset != null ? uiVisualTreeAsset.GetInstanceID() : -1;
                string expectedUxmlPath = uiVisualTreeAsset != null ? UnityEditor.AssetDatabase.GetAssetPath(uiVisualTreeAsset) : "NULL";
                var assignedVta = uiDocument != null ? uiDocument.visualTreeAsset : null;
                string assignedUxmlName = assignedVta != null ? assignedVta.name : "NULL";
                int assignedUxmlId = assignedVta != null ? assignedVta.GetInstanceID() : -1;
                string assignedUxmlPath = assignedVta != null ? UnityEditor.AssetDatabase.GetAssetPath(assignedVta) : "NULL";
#else
                string expectedUxmlName = uiVisualTreeAsset != null ? uiVisualTreeAsset.name : "NULL";
                int expectedUxmlId = uiVisualTreeAsset != null ? uiVisualTreeAsset.GetInstanceID() : -1;
                string expectedUxmlPath = "N/A";
                var assignedVta = uiDocument != null ? uiDocument.visualTreeAsset : null;
                string assignedUxmlName = assignedVta != null ? assignedVta.name : "NULL";
                int assignedUxmlId = assignedVta != null ? assignedVta.GetInstanceID() : -1;
                string assignedUxmlPath = "N/A";
#endif
                if (expectedUxmlId != assignedUxmlId || expectedUxmlPath != assignedUxmlPath)
                {
                    Debug.LogError(
                        $"[LevelSelection][Start] The root visual element does not match the expected UXML template.\n" +
                        $"Expected VisualTreeAsset: name='{expectedUxmlName}', id={expectedUxmlId}, path={expectedUxmlPath}\n" +
                        $"Assigned VisualTreeAsset: name='{assignedUxmlName}', id={assignedUxmlId}, path={assignedUxmlPath}\n" +
                        $"Please check your setup."
                    );
                }
                else
                {
                    Debug.LogWarning(
                        $"[LevelSelection][Start] The root visual element does not match the expected UXML template, but the assigned VisualTreeAsset matches the expected one.\n" +
                        $"VisualTreeAsset: name='{expectedUxmlName}', id={expectedUxmlId}, path={expectedUxmlPath}\n" +
                        $"This likely means the UXML content is missing required elements, or there is a duplicate UIDocument, or script execution order/UI initialization is incorrect. Please check the UXML content and scene setup."
                    );
                }
            }

            if (uiDocument == null || uiDocument.rootVisualElement == null)
            {
                Debug.LogError($"[LevelSelection][Start] UIDocument or rootVisualElement is null. uiDocument: {(uiDocument == null ? "NULL" : uiDocument.ToString())}, rootVisualElement: {(uiDocument == null ? "N/A" : (uiDocument.rootVisualElement == null ? "NULL" : uiDocument.rootVisualElement.ToString()))}");
            }
            if (expectedUIRoot == null)
            {
                Debug.LogError("[LevelSelection][Start] Expected UI root is not assigned! Please assign it in the Inspector.");
            }

            // initialize tooltips and tooltip manager
            if (uiVisualTreeAsset != null)
            {
                if (TooltipManager.Instance == null)
                {
                    Debug.Log("[LevelSelection][Start] TooltipManager instance not found, creating a new one.");
                    GameObject tooltipManagerObj = new("TooltipManager");
                    tooltipManagerObj.AddComponent<TooltipManager>();
                }
                else
                {
                    Debug.Log("[LevelSelection][Start] TooltipManager instance already exists. Initializing root.");
                    TooltipManager.Instance.Initialize(root);                    
                }
            }
            else
            {
                Debug.LogError("[LevelSelection][Start] uiVisualTreeAsset is not assigned! Please assign it in the Inspector.");
            }

            playButton = root?.Q<Button>("button_play");
            if (playButton != null)
            {
                playButton.SetEnabled(false);
                playButton.clicked += OnPlayButtonClicked;
                Debug.Log("[LevelSelection][Start] Play button initialized and disabled.");

                // Register tooltip events for the play button
                if (!string.IsNullOrEmpty(playButton.tooltip))
                {
                    playButton.RegisterCallback<PointerEnterEvent>(evt =>
                    {                        
                        if (TooltipManager.Instance != null)
                            TooltipManager.Instance.Show(playButton.tooltip, evt.position);
                        
                    });
                    playButton.RegisterCallback<PointerLeaveEvent>(_ =>
                    {
                        if (TooltipManager.Instance != null)
                            TooltipManager.Instance.Hide();
                    });
                    playButton.RegisterCallback<PointerMoveEvent>(evt =>
                    {
                        if (TooltipManager.Instance != null)
                            TooltipManager.Instance.Move(evt.position);
                    });
                }
            }
            else
            {
                Debug.LogError("[LevelSelection][Start] Play button not found in the UI. Check UXML for name='button_play'.");
            }

            returnToMenuButton = root?.Q<Button>("button_return_to_menu");
            if (returnToMenuButton != null)
            {
                // register tooltip events for the return to menu button
                returnToMenuButton.SetEnabled(true);
                Debug.Log("[LevelSelection][Start] Return to menu button initialized and enabled.");

                if (string.IsNullOrEmpty(returnToMenuButton.tooltip))
                {
                    returnToMenuButton.tooltip = "Return to Main Menu";
                    Debug.Log("[LevelSelection][Start] Return to menu button tooltip set to default.");
                }
                returnToMenuButton.RegisterCallback<PointerEnterEvent>(evt =>
                {
                    if (TooltipManager.Instance != null)
                        TooltipManager.Instance.Show(returnToMenuButton.tooltip, evt.position);
                });
                returnToMenuButton.RegisterCallback<PointerLeaveEvent>(_ =>
                {
                    if (TooltipManager.Instance != null)
                        TooltipManager.Instance.Hide();
                });
                returnToMenuButton.RegisterCallback<PointerMoveEvent>(evt =>
                {
                    if (TooltipManager.Instance != null)
                        TooltipManager.Instance.Move(evt.position);
                });

                // Register the click event for the return to menu button
                returnToMenuButton.clicked += OnReturnToMenuClicked; 
            }
        }

        private IEnumerator AttachUIDocumentWhenReady()
        {
            while (UIRootManager.Instance == null ||
                   uiDocument == null ||
                   uiDocument.rootVisualElement == null)
            {
                yield return null;
            }
            Debug.Log("[LevelSelection] Assigning UIDocument to UIRootManager (no parenting, just assignment).");
            // UIRootManager.Instance.AttachSceneUIDocument(uiDocument);
        }

        private void OnPreloadComplete()
        {
            Debug.Log("[LevelSelection] PreloadAssets.PreloadComplete fired. Now starting level selection logic by spawning the card visual element template.");
            if (levelCardTemplate == null)
            {
                Debug.LogError("[LevelSelection] Level card template is not assigned! Please assign it in the Inspector.");
                return;
            }
        }

        private void LoadLevel(int levelIndex)
        {
            if (IsLevelUnlocked(levelIndex))
            {
                AssetPreloader.LoadSceneWhenReady(levelIndex);
            }
            else
            {
                Debug.Log("Level is locked!");
            }
        }

        private void LoadLevel(string sceneName)
        {
            AssetPreloader.LoadSceneWhenReady(sceneName);
        }

        private bool IsLevelUnlocked(int levelIndex)
        {
            return levelIndex <= currentLevelIndex + 1;
        }

        private void LoadLevelCards()
        {
            Debug.Log($"[LevelSelection][LoadLevelCards] Called. uiDocument: {(uiDocument == null ? "NULL" : uiDocument.ToString())}");
            StartCoroutine(LoadLevelCardsCoroutine());
        }

        private IEnumerator LoadLevelCardsCoroutine()
        {
            if (uiDocument == null)
            {
                Debug.LogError("[LevelSelection][LoadLevelCardsCoroutine] uiDocument is NULL! Cannot load level cards.");
                yield break;
            }
            root = uiDocument.rootVisualElement;
            levelCardList = root.Q<VisualElement>("level_card_list");

            if (levelCards == null || levelCards.Length == 0)
            {
                Debug.LogError("[LevelSelection][LoadLevelCardsCoroutine] No level cards assigned! Please assign them in the Inspector.");
                yield break;
            }
            if (levelCardTemplate == null)
            {
                Debug.LogError("[LevelSelection][LoadLevelCardsCoroutine] Level card template is not assigned! Please assign it in the Inspector.");
                yield break;
            }
            if (levelCardList == null)
            {
                Debug.LogError("[LevelSelection][LoadLevelCardsCoroutine] Level card list VisualElement not found in the UI.");
                yield break;
            }

            Debug.Log($"[LevelSelection][LoadLevelCardsCoroutine] Loading {levelCards.Length} level cards into UI.");

            levelCardList.Clear();

            for (int i = 0; i < levelCards.Length; i++)
            {
                LevelCard cardData = levelCards[i];
                VisualElement levelCard = levelCardTemplate.CloneTree();
                
                var levelCardTooltipHolder = levelCard.Q<VisualElement>("LevelCardTemplate");
                if (levelCardTooltipHolder != null)
                {
                    levelCardTooltipHolder.tooltip = cardData.levelDescription;
                    Debug.Log($"[LevelSelection][LoadLevelCardsCoroutine] Level card {i} tooltip set to: {cardData.levelName}");
                }
                else
                {
                    Debug.LogWarning($"[LevelSelection][LoadLevelCardsCoroutine] Level card {i} tooltip holder not found.");
                }

                var levelIndexLabel = levelCard.Q<Label>("lvl_int");
                if (levelIndexLabel != null)
                    levelIndexLabel.text = cardData.levelIndex.ToString();

                var levelNameLabel = levelCard.Q<Label>("lvl_name");
                if (levelNameLabel != null)
                    levelNameLabel.text = cardData.levelName;

                var spriteElement = levelCard.Q<VisualElement>("level_card_thumbnail");
                if (spriteElement != null && cardData.thumbnail != null)
                    spriteElement.style.backgroundImage = new StyleBackground((Texture2D)cardData.thumbnail);

                var lockElement = levelCard.Q<VisualElement>("level_lock");
                if (lockElement != null)
                    lockElement.style.display = cardData.isUnlocked ? DisplayStyle.None : DisplayStyle.Flex;

                levelCard.RegisterCallback<ClickEvent>(evt =>
                {
                    if (levelCardsLoaded && cardData.isUnlocked)
                    {
                        selectedLevelCard = cardData;
                        selectedLevelIndex = cardData.levelIndex;
                        playButton?.SetEnabled(true);

                        Debug.Log($"Level {cardData.levelIndex} selected!");
                    }
                    else if (!levelCardsLoaded)
                    {
                        Debug.LogWarning("Cards not fully loaded yet!");
                    }
                    else
                    {
                        Debug.Log("Level is locked!");
                    }
                });

                // Validate the card VisualElement directly, not the main UIDocument
                bool cardValid = UIDocumentValidator.ValidateVisualElementTree(
                    levelCard, cardData.Config, out var cardMsg);
                Debug.Log($"[LevelSelection][LoadLevelCardsCoroutine] Card {i} (levelIndex={cardData.levelIndex}) validation: valid={cardValid}, msg={cardMsg}");
                // validate locale for the card
                if (cardValid)
                {
                    // Localize the level name and description
                    string locale = LocalizationHelper.GetCurrentLocaleCode();
#if UNITY_WEBGL
                    string localizedName = LocalizationHelper.GetLocalizedStringForLocale(
                        "LevelNames", cardData.levelNameKey, locale, cardData.levelName);
                    string localizedDescription = LocalizationHelper.GetLocalizedStringForLocale(
                        "LevelDescriptions", cardData.levelDescriptionKey, locale, cardData.levelDescription);
#else
                    var localeObj = LocalizationHelper.GetLocaleFromCode(locale);
                    string localizedName = LocalizationHelper.GetLocalizedStringForLocale(
                        "HOGT_Level_Cards", cardData.levelNameKey, localeObj, cardData.levelName);
                    string localizedDescription = LocalizationHelper.GetLocalizedStringForLocale(
                        "HOGT_Level_Cards", cardData.levelDescriptionKey, localeObj, cardData.levelDescription);
#endif
                    levelNameLabel.text = localizedName;
                    var descriptionLabel = levelCard.Q<Label>("lvl_description");
                    if (descriptionLabel != null)
                        descriptionLabel.text = localizedDescription;
                    // use UIDocumentValidator to validate or fix the card Locale
                    UIDocumentValidator.ValidateOrFixVisualElementLocale(
                        levelCard, cardData.Config, out var localeValid, out var localeMsg);
                }
                else
                {
                    Debug.LogWarning($"[LevelSelection][LoadLevelCardsCoroutine] Card {i} (levelIndex={cardData.levelIndex}) failed validation: {cardMsg}");
                }

                levelCardList.Add(levelCard);
            }

            levelCardsLoaded = true;
            if (selectedLevelCard != null && selectedLevelCard.isUnlocked)
                playButton?.SetEnabled(true);
            else
                playButton?.SetEnabled(false);

            Debug.Log("[LevelSelection][LoadLevelCardsCoroutine] All level cards loaded.");
            yield break;
        }

        private void OnPlayButtonClicked()
        {
            if (!levelCardsLoaded)
            {
                Debug.LogWarning("Cannot load scene: Level cards not finished loading.");
                return;
            }

            if (selectedLevelCard != null && selectedLevelCard.isUnlocked)
            {
                if (!string.IsNullOrEmpty(selectedLevelCard.sceneName))
                {
                    AssetPreloader.LoadSceneWhenReady(selectedLevelCard.sceneName);
                }
                else
                {
                    AssetPreloader.LoadSceneWhenReady(selectedLevelCard.levelIndex);
                }
            }
            else
            {
                Debug.LogWarning("No level card selected or card is locked.");
            }
        }

        private void OnReturnToMenuClicked()
        {
            AssetPreloader.LoadSceneWhenReady("MainMenu");
        }

        private IEnumerator GetAndSetCurrentLocaleCode()
        {
            Debug.Log("[LevelSelection][GetAndSetCurrentLocaleCode] Waiting for UIRootManager, locale, and UIDocument to be ready...");
            while (UIRootManager.Instance == null || UIRootManager.Instance.CurrentLocaleCode() == null || uiDocument == null || uiDocument.rootVisualElement == null)
            {
                if (UIRootManager.Instance == null)
                    Debug.LogWarning("[LevelSelection][GetAndSetCurrentLocaleCode] UIRootManager.Instance is null.");
                if (uiDocument == null)
                    Debug.LogWarning("[LevelSelection][GetAndSetCurrentLocaleCode] uiDocument is null.");
                else if (uiDocument.rootVisualElement == null)
                    Debug.LogWarning("[LevelSelection][GetAndSetCurrentLocaleCode] uiDocument.rootVisualElement is null.");
                yield return null;
            }
            string localeCode = UIRootManager.Instance.CurrentLocaleCode();
            if (LocalizationHelper.GetCurrentLocaleCode() != localeCode)
            {
                LocalizationHelper.SetLocale(localeCode);
                PlayerPrefs.SetString("selected_locale", localeCode);
                PlayerPrefs.Save();
                Debug.Log($"[LevelSelection][GetAndSetCurrentLocaleCode] Current locale set to: {localeCode}");
            }

            yield return new WaitForFixedUpdate();

            UIDocumentValidator.ValidateOrFixUIDocument(
                this, ref uiDocument, uiVisualTreeAsset, _validationConfig, out var valid, out var msg, true);
            Debug.Log($"[LevelSelection][GetAndSetCurrentLocaleCode] UIDocumentValidator.ValidateOrFixUIDocument result: valid={valid}, msg={msg}");

            RefreshLocalizedUI();
        }
    }
}
