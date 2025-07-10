using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using UnityEngine.Serialization;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using UnityEditor; 
using TinyWalnutGames.UITKTemplates.MainMenu; 
using TinyWalnutGames.UITKTemplates.Tools;
using System.Linq;

/// <summary>
/// Controls the population of the LevelOverlay UI with data from a LevelData ScriptableObject.
/// Refactored for testability: UI logic is separated into methods, dependencies can be injected, and fields are accessible for testing.
/// </summary>
namespace TinyWalnutGames.UITKTemplates.HOGT
{
    [ExecuteAlways] // Changed from ExecuteInEditMode to ExecuteAlways
    public class LevelUIController : MonoBehaviour
    {
        // Use a private field for LevelData
        [SerializeField] private LevelData levelData;
        public LevelData LevelData
        {
            get => levelData;
            set => levelData = value;
        }


        [SerializeField] private UIDocument uiDocument;
        public UIDocument expectedUIRoot;
        [SerializeField] private VisualTreeAsset uiVisualTreeAsset;
        [SerializeField] private GameObject sparkleEffectPrefab;

        [SerializeField] private Label levelNameLabel;
        [SerializeField] private VisualElement backgroundImage;
        [SerializeField] private VisualElement objectsList;
        [SerializeField] private VisualElement playArea;
        [SerializeField] private VisualElement hListViewContainer;
        [SerializeField] private VisualElement vListViewContainer;

        [FormerlySerializedAs("itemList")]
        [SerializeField] private List<VisualElement> itemList;

        [SerializeField] private VisualTreeAsset itemContainerTemplate;

        // Dynamic key for LevelData, e.g. "LD01" for Level 1
        private string LevelDataKey => $"LD0{GetCurrentLevelNumber()}";
        private const string VisualTreeKey = "LevelUIVisualTree";

        private bool IsInEditMode => !Application.isPlaying;
        private bool _preloadSubscribed = false;

        private void Awake()
        {
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
                if (uiDocument == null)
                {
                    Debug.LogWarning("[LevelUIController] No UIDocument assigned or found on GameObject. Creating a default UIDocument for testing.");
#if UNITY_EDITOR
                    uiDocument = gameObject.AddComponent<UIDocument>();
                    var testUxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Resources/TestRootVisualElement.uxml");
                    if (testUxml != null)
                    {
                        uiDocument.visualTreeAsset = testUxml;
                    }
#endif
                }
            }
            if (uiDocument == null)
            {
                uiDocument = FindFirstObjectByType<UIDocument>();
                if (uiDocument == null)
                {
                    Debug.LogWarning("[LevelUIController] UIDocument not found in scene. UI tests may fail.");
                    return;
                }
            }
            if (uiDocument.rootVisualElement == null)
            {
                Debug.LogWarning("[LevelUIController] rootVisualElement is null. UI tests may fail.");
                return;
            }

            expectedUIRoot = uiDocument;
        }

        private void OnEnable()
        {
            if (IsInEditMode)
            {
                TryPopulateUIInEditMode();
            }

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

        private void OnValidate()
        {
            if (IsInEditMode)
            {
                TryPopulateUIInEditMode();
            }
        }

        private void TryPopulateUIInEditMode()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null || LevelData == null)
                return;

            var root = uiDocument.rootVisualElement;
            if (root == null)
                return;

            InitializeUIReferences();
            PlaceHiddenObjectsInUI();
        }

        private void Start()
        {
            // Wait for PreloadAssets if needed, then attach UIDocument when ready
            if (AssetPreloader.Instance != null && !AssetPreloader.Instance.IsReady)
            {
                Debug.Log("[LevelUIController] Waiting for PreloadAssets to complete before initializing UI.");
                AssetPreloader.Instance.PreloadComplete += OnPreloadComplete; // FIX: Use instance, not static
                _preloadSubscribed = true;
            }
            else
            {
                OnPreloadComplete();
            }
            // Start coroutine to attach UIDocument to UIRootManager when ready
            StartCoroutine(AttachUIDocumentWhenReady());
        }

        private void OnDestroy()
        {
            if (_preloadSubscribed && AssetPreloader.Instance != null)
                AssetPreloader.Instance.PreloadComplete -= OnPreloadComplete; // FIX: Use instance, not static
        }

        private void OnPreloadComplete()
        {
            Debug.Log("[LevelUIController] PreloadAssets.PreloadComplete fired. Now initializing level UI.");
            Initialize();
            InitializeUIReferences();
            PopulateUI();
            // Start coroutine to attach UIDocument after PreloadAssets is ready
            StartCoroutine(AttachUIDocumentWhenReady());
        }

        private IEnumerator WaitForPreloadAndInit()
        {
            while (AssetPreloader.Instance == null || !AssetPreloader.Instance.IsReady)
                yield return null;

            Initialize();
            InitializeUIReferences();
            PopulateUI();
        }

        // Add this coroutine to ensure UIDocument is attached only when ready
        private IEnumerator AttachUIDocumentWhenReady()
        {
            while (UIRootManager.Instance == null ||
                   uiDocument == null ||
                   uiDocument.rootVisualElement == null)
            {
                yield return null;
            }
            Debug.Log("[LevelUIController] Assigning UIDocument to UIRootManager (no parenting, just assignment).");
           // UIRootManager.Instance.AttachSceneUIDocument(uiDocument);
        }

        public void InjectDependencies(LevelData data, UIDocument document, VisualTreeAsset visualTree)
        {
            LevelData = data;
            uiDocument = document;
            uiVisualTreeAsset = visualTree;
            if (uiDocument != null && uiVisualTreeAsset != null)
            {
                uiDocument.visualTreeAsset = uiVisualTreeAsset;
            }
        }

        /// <summary>
        /// Initializes the LevelUIController by loading LevelData and VisualTreeAsset if not already assigned,
        /// sets up the UIDocument, initializes hidden objects, and validates the UI and locale.
        /// </summary>
        public void Initialize()
        {
            Debug.Log($"[LevelUIController] Initialize called. LevelData is {(LevelData == null ? "null" : "already assigned")}");
            if (LevelData == null)
            {
                string key = LevelDataKey;
                Debug.Log("[LevelUIController] Attempting to get LevelData from PreloadAssets with key: " + key);
                LevelData = AssetPreloader.Instance.Get<LevelData>(key);
                if (LevelData == null)
                {
                    Debug.LogError("[LevelUIController] LevelData not found in preloaded assets.");
                }
                else
                {
                    Debug.Log($"[LevelUIController] LevelData initialized: {LevelData.levelName}");
                }
            }

            if (uiVisualTreeAsset == null)
            {
                uiVisualTreeAsset = AssetPreloader.Instance.Get<VisualTreeAsset>(VisualTreeKey);
                if (uiVisualTreeAsset == null)
                {
                    Debug.LogError("VisualTreeAsset not found in preloaded assets.");
                }
                else
                {
                    Debug.Log("VisualTreeAsset initialized for LevelUIController.");
                }
            }
            if (uiDocument != null && uiVisualTreeAsset != null)
            {
                uiDocument.visualTreeAsset = uiVisualTreeAsset;
                if (uiDocument != null && uiVisualTreeAsset != null)
                {
                    uiDocument.visualTreeAsset = uiVisualTreeAsset;
                    //if (UIRootManager.Instance != null)
                    //    UIRootManager.Instance.AttachSceneUIDocument(uiDocument);
                    Debug.Log($"UIDocument visual tree, {uiVisualTreeAsset}, asset set successfully.");
                }

                Debug.Log($"UIDocument visual tree, {uiVisualTreeAsset}, asset set successfully.");
            }

            if (LevelData != null && LevelData.objectsToFind != null)
            {
                foreach (var obj in LevelData.objectsToFind)
                {
                    if (obj != null)
                    {
                        obj.Initialize();
                    }
                }
            }

            // Use shared validator for UIDocument and locale
            UIDocumentValidationConfig config = new()
            {
                NamedElements = new Dictionary<string, System.Type>
                {
                    { "playarea", typeof(VisualElement) },
                    { "objectsList", typeof(VisualElement) }
                }
            };
            UIDocumentValidator.ValidateOrFixUIDocument(this, ref uiDocument, uiVisualTreeAsset, config, out _, out _, true);
        }

        public void InitializeUIReferences()
        {
            if (uiDocument == null) return;
            var root = uiDocument.rootVisualElement;
            levelNameLabel = root.Q<Label>("levelNameLabel");
            backgroundImage = root.Q<VisualElement>("backgroundImage");
            objectsList = root.Q<VisualElement>("objectsList");
            playArea = root.Q<VisualElement>("playarea");
            hListViewContainer = root.Q<VisualElement>("h-listview-container");
            vListViewContainer = root.Q<VisualElement>("v-listview-container");
        }

        public void PopulateUI()
        {
            SetLevelName();
            SetBackgroundImage();
            PopulateObjectsList();
            PopulatePlayArea();
            PopulateListViewContainers();
            Debug.Log("[LevelUIController] UI populated with LevelData.");
            // Start the coroutine to get and set the current locale code
            StartCoroutine(GetAndSetCurrentLocaleCode());
        }

        public void SetLevelName()
        {
            if (levelNameLabel != null && LevelData != null)
                levelNameLabel.text = LevelData.levelName;
        }

        public void SetBackgroundImage()
        {
            if (backgroundImage != null && LevelData != null && LevelData.levelBackground != null)
                backgroundImage.style.backgroundImage = new StyleBackground(LevelData.levelBackground);
        }

        public void PopulateObjectsList()
        {
            if (objectsList != null && LevelData != null && LevelData.objectsToFind != null)
            {
                objectsList.Clear();
                foreach (var obj in LevelData.objectsToFind)
                {
                    if (obj != null)
                    {
                        // Use the itemContainerTemplate for consistent UI
                        var objElem = CreateHiddenObjectElement(obj, false, isListView: true);
                        objectsList.Add(objElem);
                    }
                }
            }
        }

        public void PopulatePlayArea()
        {
            if (playArea != null && LevelData != null && LevelData.objectsToFind != null)
            {
                // Optional: Hide pre-existing reference items at runtime (if you want to keep them for reference)
                foreach (var child in playArea.Children().ToList())
                {
                    // Hide any child that is not a TemplateContainer (i.e., pre-existing UXML reference)
                    if (child is not TemplateContainer)
                        child.style.display = DisplayStyle.None;
                }
                // Or, if you want a clean slate, uncomment the next line:
                // playArea.Clear();

                foreach (var obj in LevelData.objectsToFind)
                {
                    if (obj == null) continue;
                    obj.Initialize();

                    var ve = CreateHiddenObjectElement(obj, false);
                    ve.style.position = Position.Absolute;
                    if (obj.position != Vector2.zero)
                    {
                        ve.style.left = obj.position.x;
                        ve.style.top = obj.position.y;
                    }
                    if (obj.size != Vector2.zero)
                    {
                        ve.style.width = obj.size.x;
                        ve.style.height = obj.size.y;
                    }
                    // Set rotation if present in tags
                    var rotTag = obj.tags?.FirstOrDefault(t => t.StartsWith("rotation:"));
                    if (!string.IsNullOrEmpty(rotTag) && float.TryParse(rotTag[9..], out float angle))
                    {
                        ve.style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Degree)));
                    }
                    // Add click listener
                    ve.RegisterCallback<ClickEvent>(evt => OnHiddenObjectClicked(obj));
                    // Ensure name is set for lookup/saving
                    ve.name = obj.objectName;
                    playArea.Add(ve);
                    obj.playAreaElement = ve;

                    Debug.Log($"[LevelUIController] Added '{obj.objectName}' to playArea. Position: {obj.position}, Size: {obj.size}");
                }
                Debug.Log($"[LevelUIController] playArea has {playArea.childCount} children after population.");
                for (int i = 0; i < playArea.childCount; i++)
                {
                    Debug.Log($"[LevelUIController] playArea child {i}: {playArea[i].name}");
                }
                Debug.Log($"[LevelUIController] Total objects added to playArea: {playArea.childCount}");
            }
            else
            {
                Debug.LogWarning("[LevelUIController] playArea, LevelData, or objectsToFind is null in PopulatePlayArea.");
            }
        }

        public void PopulateListViewContainers()
        {
            if (LevelData == null || LevelData.objectsToFind == null) return;

            VisualElement hListViewContent = hListViewContainer?.Q("h-listview-container");
            VisualElement vListViewContent = vListViewContainer?.Q("v-listview-container");

            hListViewContent?.Clear();
            vListViewContent?.Clear();

            int hCount = 0, vCount = 0;
            foreach (var obj in LevelData.objectsToFind)
            {
                if (obj == null) continue;
                var hElem = CreateHiddenObjectElement(obj, false, isListView: true);
                var vElem = CreateHiddenObjectElement(obj, false, isListView: true);
                hListViewContent?.Add(hElem);
                vListViewContent?.Add(vElem);
                hCount++;
                vCount++;
                Debug.Log($"[LevelUIController] Added '{obj.objectName}' to list views.");
            }
            Debug.Log($"[LevelUIController] Total objects added to hListView: {hCount}, vListView: {vCount}");
        }

        private void PlaceHiddenObjectsInUI()
        {
            if (uiDocument == null || LevelData == null)
            {
                Debug.LogWarning("[LevelUIController] Cannot place hidden objects: missing UIDocument or LevelData.");
                return;
            }

            var root = uiDocument.rootVisualElement;
            if (root == null)
            {
                Debug.LogWarning("[LevelUIController] rootVisualElement is null.");
                return;
            }

            var hListView = root.Q<VisualElement>("h-listview-container");
            var vListView = root.Q<VisualElement>("v-listview-container");
            var playarea = root.Q<GroupBox>("playarea");

            var hListViewContent = hListView?.Q("unity-content-container");
            var vListViewContent = vListView?.Q("unity-content-container");

            hListViewContent?.Clear();
            vListViewContent?.Clear();
            playarea?.Clear();

            foreach (var objData in LevelData.objectsToFind)
            {
                if (objData == null) continue;

                var playObj = CreateHiddenObjectElement(objData, false);
                playObj.style.position = Position.Absolute;
                if (objData.position != Vector2.zero)
                {
                    playObj.style.left = objData.position.x;
                    playObj.style.top = objData.position.y;
                }
                if (objData.size != Vector2.zero)
                {
                    playObj.style.width = objData.size.x;
                    playObj.style.height = objData.size.y;
                }
                // Set rotation if present in tags
                var rotTag = objData.tags?.FirstOrDefault(t => t.StartsWith("rotation:"));
                if (!string.IsNullOrEmpty(rotTag) && float.TryParse(rotTag[9..], out float angle))
                {
                    playObj.style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Degree)));
                }
                // Add click listener
                playObj.RegisterCallback<ClickEvent>(evt => OnHiddenObjectClicked(objData));
                playObj.name = objData.objectName;
                playarea?.Add(playObj);
                objData.visualElement = playObj;

                hListViewContent?.Add(CreateHiddenObjectElement(objData, false, isListView: true));
                vListViewContent?.Add(CreateHiddenObjectElement(objData, false, isListView: true));
            }
        }

        private VisualElement CreateHiddenObjectElement(HiddenObjectData objData, bool showToast = false, bool isListView = false)
        {
            if (itemContainerTemplate == null)
            {
                Debug.LogError("itemContainerTemplate (item_container.uxml) is not assigned in LevelUIController.");
                return new Label("Missing Template");
            }

            var container = itemContainerTemplate.Instantiate();
            // Ensure consistent naming for instantiated objects
            container.name = objData.objectName;

            Debug.Log($"[LevelUIController] Instantiated UI element for hidden object: {objData.objectName}");
            var toast = container.Q<VisualElement>("toast");
            if (toast != null)
            {
                // Use the new dynamic toast background system
                var toastBg = objData.GetToastBackground(0);
                if (toastBg != null)
                    toast.style.backgroundImage = new StyleBackground(toastBg);
                else
                    toast.style.backgroundImage = null;
                toast.style.display = showToast ? DisplayStyle.Flex : DisplayStyle.None;
            }

            var contents = container.Q<VisualElement>("contents");
            if (contents != null)
            {
                // Use the new dynamic object sprite system
                var objSprite = objData.GetObjectSprite(0);
                if (objSprite != null)
                    contents.style.backgroundImage = new StyleBackground(objSprite);
                else
                    contents.style.backgroundImage = null;
            }

            // For list view, show the name as a label
            if (isListView)
            {
                var nameLabel = container.Q<Label>("objectNameLabel");
                if (nameLabel == null)
                {
                    // If the template doesn't have a label, add one
                    nameLabel = new()
                    {
                        name = "objectNameLabel"
                    };
                    container.Add(nameLabel);
                }
                nameLabel.text = objData.objectName;
            }

            // Add click listener for all hidden objects
            container.RegisterCallback<ClickEvent>(evt => OnHiddenObjectClicked(objData));

            return container;
        }

        private void OnHiddenObjectClicked(HiddenObjectData objData)
        {
            Debug.Log($"[LevelUIController] Hidden object clicked: {objData.objectName}");
            // Implement your logic here, e.g., mark as found, show toast, etc.
            ShowItemToast(objData);
            // Optionally, call OnObjectFoundUI if appropriate
            // OnObjectFoundUI(objData);
        }

        public void OnObjectFoundUI(HiddenObjectData foundObject)
        {
            if (objectsList != null)
            {
                foreach (var child in objectsList.Children())
                {
                    if (child is Label label && label.text == foundObject.objectName)
                    {
                        label.AddToClassList("found");
                        break;
                    }
                }
            }

            if (foundObject.playAreaElement != null)
            {
                foundObject.playAreaElement.AddToClassList("found");
                foundObject.playAreaElement.style.opacity = 0.5f;
            }
        }

        public void ShowItemToast(HiddenObjectData foundObject)
        {
            Debug.Log($"Item found: {foundObject.objectName}");
        }

        //public void PlaySparkleEffect(Vector3 position)
        //{
        //    // TODO: Instantiate a sparkle particle effect at the given position (world to UI conversion if needed)
        //}

        public void PlayFoundSound()
        {
            // TODO: Play a sound effect (use AudioManager or similar)
        }

        public void CheckLevelCompletion()
        {
            if (LevelData != null && LevelData.objectsToFind != null)
            {
                bool allFound = LevelData.objectsToFind.All(obj => LevelData.objectsFound.Contains(obj));
                if (allFound)
                {
                    Debug.Log("Level complete!");
                    // TODO: Trigger win UI, next level, etc.
                }
            }
        }

        public VisualElement GetPlayAreaElement()
        {
            if (uiDocument == null) return null;
            var root = uiDocument.rootVisualElement;
            return root?.Q<VisualElement>("playarea");
        }

        public VisualElement GetHListViewContent()
        {
            if (uiDocument == null) return null;
            var root = uiDocument.rootVisualElement;
            var hListView = root?.Q<VisualElement>("h-listview-container");
            return hListView?.Q("unity-content-container");
        }

        public VisualElement GetVListViewContent()
        {
            if (uiDocument == null) return null;
            var root = uiDocument.rootVisualElement;
            var vListView = root?.Q<VisualElement>("v-listview-container");
            return vListView?.Q("unity-content-container");
        }

        public void DebugRefreshHiddenObjectsUI()
        {
            PlaceHiddenObjectsInUI();
        }

#if UNITY_EDITOR
        [ContextMenu("Debug/Refresh Hidden Objects UI")]
        private void DebugRefreshHiddenObjectsUI_ContextMenu()
        {
            Debug.Log("[LevelUIController] Refreshing hidden objects UI via context menu.");
            PlaceHiddenObjectsInUI();
        }

        [ContextMenu("Debug/Save Hidden Object Positions From UI")]
        public void SaveHiddenObjectPositionsFromUI()
        {
            if (uiDocument == null || LevelData == null) return;
            var root = uiDocument.rootVisualElement;
            var playarea = root.Q<GroupBox>("playarea");
            if (playarea == null) return;

            Undo.RecordObject(LevelData, "Save Hidden Object Positions");

            foreach (var objData in LevelData.objectsToFind)
            {
                if (objData == null) continue;
                // Use consistent naming for lookup (no "_container" suffix)
                var ve = playarea.Q<VisualElement>(objData.objectName);
                if (ve != null)
                {
                    // Save position and size
                    objData.position = new Vector2(
                        ve.style.left.value.value,
                        ve.style.top.value.value
                    );
                    objData.size = new Vector2(
                        ve.style.width.value.value,
                        ve.style.height.value.value
                    );
                    // Save rotation (if set)
                    float angle = ve.resolvedStyle.rotate.angle.value;
                    objData.tags.RemoveAll(t => t.StartsWith("rotation:"));
                    if (Mathf.Abs(angle) > 0.01f)
                    {
                        objData.tags.Add($"rotation:{angle}");
                    }
                    EditorUtility.SetDirty(objData);
                    Debug.Log($"[LevelUIController] Saved for {objData.objectName}: pos={objData.position}, size={objData.size}, rot={angle}");
                }
                else
                {
                    Debug.LogWarning($"[LevelUIController] Could not find VisualElement for {objData.objectName} in playarea.");
                }
            }
            AssetDatabase.SaveAssets();
        }
#endif

        private int GetCurrentLevelNumber()
        {
            var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (sceneName.StartsWith("Level"))
            {
                if (int.TryParse(sceneName[5..], out int num))
                    return num;
            }
            return 1;
        }

        // Coroutine to get and set the current locale code from the UIRootManager
        private IEnumerator GetAndSetCurrentLocaleCode()
        {
            // Wait for both UIRootManager and UI to be ready
            while (UIRootManager.Instance == null || UIRootManager.Instance.CurrentLocaleCode() == null || uiDocument == null || uiDocument.rootVisualElement == null)
                yield return null;
            string localeCode = UIRootManager.Instance.CurrentLocaleCode();
            // Only set locale if not already set
            if (LocalizationHelper.GetCurrentLocaleCode() != localeCode)
            {
                LocalizationHelper.SetLocale(localeCode);
                PlayerPrefs.SetString("selected_locale", localeCode);
                PlayerPrefs.Save();
                Debug.Log($"[LevelUIController] Current locale set to: {localeCode}");
            }

            // wait for fixed update to ensure the locale is set before refreshing UI
            yield return new WaitForFixedUpdate();
            RefreshLocalizedUI();
        }

        private void RefreshLocalizedUI()
        {
            if (uiDocument == null || LevelData == null)
            {
                Debug.LogWarning("[LevelUIController] Cannot refresh localized UI: missing UIDocument or LevelData.");
                return;
            }
            var root = uiDocument.rootVisualElement;
            if (root == null)
            {
                Debug.LogWarning("[LevelUIController] rootVisualElement is null.");
                return;
            }

#if UNITY_WEBGL
// Localize level name
            string levelNameKey = LevelData.levelNameKey;
            string localizedLevelName = !string.IsNullOrEmpty(levelNameKey)
                ? LocalizationHelper.GetLocalizedString("ui", levelNameKey, LevelData.levelName)
                : LevelData.levelName;
            if (levelNameLabel != null)
                levelNameLabel.text = localizedLevelName;

            // Localize object names in the objects list
            if (objectsList != null && LevelData.objectsToFind != null)
            {
                int i = 0;
                foreach (var obj in LevelData.objectsToFind)
                {
                    if (obj == null) continue;
                    string objNameKey = obj.objectNameKey;
                    string localizedObjName = !string.IsNullOrEmpty(objNameKey)
                        ? LocalizationHelper.GetLocalizedString("ui", objNameKey, obj.objectName)
                        : obj.objectName;

                    // Find the corresponding label in the UI (assuming order matches)
                    var objElem = objectsList.ElementAt(i);
                    var nameLabel = objElem.Q<Label>("objectNameLabel");
                    if (nameLabel != null)
                        nameLabel.text = localizedObjName;
                    i++;
                }
            }
#else
            // Localize level name
            string levelNameKey = LevelData.levelNameKey;
            string localizedLevelName = !string.IsNullOrEmpty(levelNameKey)
                ? LocalizationHelper.GetLocalizedString("HOTG_UI", levelNameKey, LevelData.levelName)
                : LevelData.levelName;
            if (levelNameLabel != null)
                levelNameLabel.text = localizedLevelName;

            // Localize object names in the objects list
            if (objectsList != null && LevelData.objectsToFind != null)
            {
                int i = 0;
                foreach (var obj in LevelData.objectsToFind)
                {
                    if (obj == null) continue;
                    string objNameKey = obj.objectNameKey;
                    string localizedObjName = !string.IsNullOrEmpty(objNameKey)
                        ? LocalizationHelper.GetLocalizedString("HOTG_UI", objNameKey, obj.objectName)
                        : obj.objectName;

                    // Find the corresponding label in the UI (assuming order matches)
                    var objElem = objectsList.ElementAt(i);
                    var nameLabel = objElem.Q<Label>("objectNameLabel");
                    if (nameLabel != null)
                        nameLabel.text = localizedObjName;
                    i++;
                }
            }
#endif


            // except for unimplemented dialogue, there is nothing else to localize here.
            // placeholder for dialogue localization
            // if (dialogueLabel != null)
            // {
            //     string dialogueKey = LevelData.dialogueKey;
            //     string localizedDialogue = !string.IsNullOrEmpty(dialogueKey)
            //         ? LocalizationHelper.GetLocalizedString("ui", dialogueKey, LevelData.dialogue)
            //         : LevelData.dialogue;
            //     dialogueLabel.text = localizedDialogue;
            //  
            //     Debug.Log($"[LevelUIController] Localized dialogue: {localizedDialogue}");
            //  
            //     // Optionally, you can also localize the button text if applicable
            //     var button = root.Q<Button>("dialogueButton");
            //     if (button != null)
            //     {
            //         string buttonKey = LevelData.dialogueButtonKey;
            //         string localizedButtonText = !string.IsNullOrEmpty(buttonKey)
            //             ? LocalizationHelper.GetLocalizedString("ui", buttonKey, "Next")
            //             : "Next";
            //         button.text = localizedButtonText;
            //         Debug.Log($"[LevelUIController] Localized button text: {localizedButtonText}");
            //      
            //         button.RegisterCallback<ClickEvent>(evt => OnDialogueButtonClicked());
            //      
            //         Debug.Log("[LevelUIController] Registered click event for dialogue button.");
            //      
            //         // Ensure the button is visible
            //         button.style.display = DisplayStyle.Flex;
            //      
            //      
            //         Debug.Log("[LevelUIController] Dialogue button is visible.");
            //      
            //      }
            //         else
            //      {
            //          Debug.LogWarning("[LevelUIController] Dialogue button not found in UI.");
            //      }
            // }
        }
    }
}
