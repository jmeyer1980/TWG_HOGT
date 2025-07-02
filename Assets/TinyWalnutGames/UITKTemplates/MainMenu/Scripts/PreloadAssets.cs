using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System;
#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
#endif

namespace TinyWalnutGames.UITKTemplates.MainMenu
{
    public class PreloadAssets : MonoBehaviour
    {
        [Header("UI Document (Optional)")]
        [Tooltip("Assign your UI Document here if you want to use it for tooltips or other UI elements. Can be left empty if not needed.")]
        [SerializeField]
        private UIDocument uiDocument;
        private VisualElement rootVisualElement;
        private ProgressBar progressBar;        

        public VisualTreeAsset tooltipTemplate;

        [Tooltip("If set, this scene will be loaded automatically after preloading is complete. Leave blank to stay on init scene.")]
        public string autoAdvanceSceneName = "";

        [Tooltip("If set, this scene will be loaded after preloading if autoAdvanceSceneName is empty.")]
        [SerializeField]
        private string nextSceneName = "";

        [Tooltip("If set (>=0), this scene index will be loaded after preloading if autoAdvanceSceneName and nextSceneName are empty.")]
        [SerializeField]
        private int nextSceneIndex = -1;

        // Singleton instance for global access
        private static PreloadAssets _instance;
        public static PreloadAssets Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<PreloadAssets>();
                    if (_instance == null)
                    {
                        var go = new GameObject("PreloadAssets");
                        _instance = go.AddComponent<PreloadAssets>();
                    }
                }
                return _instance;
            }
        }

        // Event: Raised when all preloading is complete and safe to proceed
        public static event Action PreloadComplete;

        // --- ADDED: Centralized scene loading control ---
        public static bool CanLoadScenes => Instance != null && Instance.IsReady;

        /// <summary>
        /// Use this method instead of SceneManager.LoadScene or LoadSceneAsync. 
        /// It will block scene loading until preloading is complete.
        /// </summary>
        public static void LoadSceneWhenReady(string sceneNameOrPath)
        {
            Debug.Log($"[PreloadAssets] Request to load scene '{sceneNameOrPath}' (IsReady={sceneNameOrPath})");
            if (!Instance.IsReady)
            {
                Debug.LogWarning("[PreloadAssets] Cannot load scene: preloading not complete.");
                return;
            }
            Instance.StartCoroutine(Instance.LoadSceneWhenReadyCoroutine(sceneNameOrPath));
        }

        public static void LoadSceneWhenReady(int sceneIndex)
        {
            if (Instance == null)
            {
                Debug.LogError("[PreloadAssets] Cannot load scene: PreloadAssets.Instance is null.");
                return;
            }
            if (!Instance.IsReady)
            {
                Debug.LogWarning($"[PreloadAssets] Scene load request for index '{sceneIndex}' ignored: preloading not complete.");
                return;
            }
            Instance.StartCoroutine(Instance.LoadSceneWhenReadyCoroutine(sceneIndex));
        }

       private IEnumerator LoadSceneWhenReadyCoroutine(string sceneNameOrPath)
        {
            // No longer wait for preloading; only called if IsReady is true.
            string sceneToLoad = sceneNameOrPath;
#if UNITY_EDITOR
            // If the string looks like a path, try to load by path
            if (sceneNameOrPath.EndsWith(".unity", System.StringComparison.OrdinalIgnoreCase))
            {
                if (!System.IO.File.Exists(sceneNameOrPath))
                {
                    Debug.LogError($"[PreloadAssets] Scene path '{sceneNameOrPath}' does not exist.");
                    yield break;
                }
                sceneToLoad = sceneNameOrPath;
            }
            else
            {
                // Try to resolve to path from EditorBuildSettings
                var found = UnityEditor.EditorBuildSettings.scenes.FirstOrDefault(s => s.enabled && s.path.ToLower().Contains(sceneNameOrPath.ToLower()));
                if (found != null)
                {
                    sceneToLoad = found.path;
                }
            }
#endif

            Debug.Log($"[PreloadAssets] Loading scene: {sceneToLoad}");
            var async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneToLoad);
            if (async == null)
            {
                Debug.LogError($"[PreloadAssets] Failed to start loading scene '{sceneToLoad}'.");
                yield break;
            }
            yield return async;
            Debug.Log($"[PreloadAssets] Scene '{sceneToLoad}' loaded.");
        }

        private IEnumerator LoadSceneWhenReadyCoroutine(int sceneIndex)
        {
            // No longer wait for preloading; only called if IsReady is true.
            yield return LoadSceneAsync(sceneIndex);
        }

        private IEnumerator LoadSceneAsync(int sceneIndex)
        {
            // No longer wait for preloading; only called if IsReady is true.
            var asyncOp = SceneManager.LoadSceneAsync(sceneIndex);
            if (asyncOp == null)
            {
                Debug.LogError($"[PreloadAssets] Failed to start loading scene with index '{sceneIndex}'.");
                yield break;
            }
            while (!asyncOp.isDone)
            {
                float progress = Mathf.Clamp01(asyncOp.progress / 0.9f);
                SetProgressBar(progress, "Loading Scene...");
                yield return null;
            }
            SetProgressBar(1f, "Scene Loaded!");
            OnSceneLoadComplete(asyncOp);
        }

        private void Awake()
        {
            // Singleton pattern: only one instance, persist across scenes
            if (_instance == null)
            {
                _instance = this;
                
                // deparent the GameObject
                this.transform.SetParent(null, true);
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                DestroyImmediate(gameObject); // Destroy duplicate immediately
                return;
            }

            // deparent the GameObject to avoid issues with scene loading
            transform.SetParent(null, true);
            SceneManager.sceneLoaded += OnSceneLoaded; // Subscribe to scene load event

            // --- REMOVED: SceneManager.activeSceneChanged warning subscription ---
            // The following code is removed to prevent warning unless preloading is complete:
            // #if UNITY_EDITOR
            // SceneManager.activeSceneChanged += (oldScene, newScene) =>
            // {
            //     if (!IsReady)
            //     {
            //         Debug.LogWarning("[PreloadAssets] WARNING: Scene changed before preloading was complete!");
            //     }
            // };
            // #endif
        }

        private void OnDestroy()
        {
            if (Instance == this)
                SceneManager.sceneLoaded -= OnSceneLoaded; // Unsubscribe to avoid memory leaks
            PreloadComplete = null; // Clear all subscribers on destroy
        }

        private void Start()
        {
            Addressables.InitializeAsync().Completed += OnAddressablesReady;

            if (uiDocument != null)
            {
                rootVisualElement = uiDocument.rootVisualElement;
                progressBar = rootVisualElement.Q<ProgressBar>("progressbar");
                if (progressBar == null)
                    Debug.LogWarning("ProgressBarFill element not found in the UI Document.");
                else
                    SetProgressBar(0f, "Initializing...");
            }
            else
            {
                Debug.LogWarning("No UIDocument assigned. Progress bar will not be displayed.");
            }

            // TooltipManager is now a singleton, not attached to this GameObject
            if (TooltipManager.Instance != null)
            {
                if (rootVisualElement != null)
                    TooltipManager.Instance.Initialize(rootVisualElement);
                else
                    Debug.LogWarning("TooltipManager initialized without a root VisualElement. Tooltips may not function correctly.");
            }
            else
            {
                Debug.LogWarning("TooltipManager singleton instance not found. Tooltips will not be available.");
            }
        }

        private void SetProgressBar(float progress, string message)
        {
            if (progressBar != null)
            {
                progress = Mathf.Clamp01(progress);
                progressBar.value = progress * 100f; // Scale to match ProgressBar highValue of 100
                progressBar.title = message;
            }
        }

        private void OnAddressablesReady(AsyncOperationHandle<IResourceLocator> handle)
        {
            Debug.Log("Addressables initialized.");
            // Do not initialize TooltipManager here; wait for scene's UIDocument after load.
            StartCoroutine(PreloadAllAndTooltipCoroutine());
        }

        // Add this field to fix _assetCache errors
        private readonly Dictionary<string, UnityEngine.Object> _assetCache = new();

        // Add this property to fix IsReady errors
        public bool IsReady { get; private set; } = false;

        // Add this property to expose loaded preload asset keys for testing
        public IEnumerable<string> LoadedPreloadKeys
        {
            get { return _assetCache.Keys; }
        }

        // --- Add this: Public generic getter for preloaded assets ---
        /// <summary>
        /// Retrieve a preloaded asset by key and type.
        /// </summary>
        public T Get<T>(string key) where T : UnityEngine.Object
        {
            if (_assetCache.TryGetValue(key, out var obj))
            {
                return obj as T;
            }
            Debug.LogWarning($"[PreloadAssets] Asset with key '{key}' not found in cache.");
            return null;
        }

        // Optimized: Wait for all preload assets to finish before checking for tooltip.
        // Loads ALL assets labeled "Preload" and updates the progress bar as each loads.
        private IEnumerator PreloadAllAndTooltipCoroutine()
        {
            // 1. Preload all addressable assets labeled "Preload"
            var downloadHandle = Addressables.DownloadDependenciesAsync("Preload");
            yield return downloadHandle;
            var locationsHandle = Addressables.LoadResourceLocationsAsync("Preload");
            yield return locationsHandle;

            var locations = locationsHandle.Result;
            int total = locations.Count;
            int loaded = 0;

            if (total == 0)
            {
                Debug.LogWarning("[PreloadAssets] No assets found with label 'Preload'. All required assets must be labeled 'Preload' in Addressables.");
            }

            foreach (var loc in locations)
            {
                var handle = Addressables.LoadAssetAsync<UnityEngine.Object>(loc.PrimaryKey);
                yield return handle;
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    _assetCache[loc.PrimaryKey] = handle.Result;
                }
                else
                {
                    Debug.LogWarning($"[PreloadAssets] Failed to load asset at address '{loc.PrimaryKey}'.");
                }
                loaded++;
                SetProgressBar((float)loaded / total, $"Loading assets... ({loaded}/{total})");
            }

            // Log all loaded keys for debugging
            Debug.Log("[PreloadAssets] Loaded Preload asset keys: " + string.Join(", ", _assetCache.Keys));

            Debug.Log("[PreloadAssets] All assets with label 'Preload' loaded.");

            // 2. Now wait for tooltip template to be ready (if needed)
            float progress = Mathf.Clamp01((float)loaded / (total > 0 ? total : 1));
            while (!Tooltip.IsTemplateReady)
            {
                SetProgressBar(progress, "Waiting for Tooltip template to load...");
                progress = Mathf.Min(progress + 0.01f, 0.99f);
                yield return new WaitForSeconds(0.1f);
            }
            Debug.Log("[PreloadAssets] Tooltip template loaded and ready.");

            // 3. Mark as ready and raise event (do NOT load next scene here)
            IsReady = true;
            SetProgressBar(1f, "All assets loaded!");
            Debug.Log("[PreloadAssets] All preload assets and tooltip template loaded and ready.");
            PreloadComplete?.Invoke();

            // --- AUTO-ADVANCE LOGIC ---
            // 1. Prefer autoAdvanceSceneName if set
            if (!string.IsNullOrEmpty(autoAdvanceSceneName))
            {
                Debug.Log($"[PreloadAssets] Auto-advancing to scene '{autoAdvanceSceneName}' after preload.");
                LoadSceneWhenReady(autoAdvanceSceneName);
            }
            // 2. Otherwise, use nextSceneName if set
            else if (!string.IsNullOrEmpty(nextSceneName))
            {
                Debug.Log($"[PreloadAssets] Advancing to nextSceneName '{nextSceneName}' after preload.");
                LoadSceneWhenReady(nextSceneName);
            }
            // 3. Otherwise, use nextSceneIndex if set (>=0)
            else if (nextSceneIndex >= 0)
            {
                Debug.Log($"[PreloadAssets] Advancing to nextSceneIndex '{nextSceneIndex}' after preload.");
                LoadSceneWhenReady(nextSceneIndex);
            }
            // 4. Otherwise, stay on init scene
            else
            {
                Debug.Log("[PreloadAssets] No auto-advance scene set. Staying on init scene.");
            }
        }

        private void OnSceneLoadComplete(AsyncOperation asyncOp)
        {
            if (asyncOp.isDone)
            {
                Debug.Log("Scene loaded successfully.");
            }
            else
            {
                Debug.LogError("Failed to load scene.");
            }
        }

        // This method will be called every time a new scene is loaded
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Always find the UIDocument in the new scene (if any)
            uiDocument = FindFirstObjectByType<UIDocument>();
            if (uiDocument != null)
            {
                rootVisualElement = uiDocument.rootVisualElement;
                progressBar = rootVisualElement.Q<ProgressBar>("progressbar");
                // Always re-initialize TooltipManager with the new rootVisualElement
                if (TooltipManager.Instance != null)
                    TooltipManager.Instance.Initialize(rootVisualElement);
            }
            else
            {
                rootVisualElement = null;
                progressBar = null;
            }

#if UNITY_EDITOR
            // Debug logging of all root GameObjects and MainMenuController state after scene load
            PreloadAssetsDebug.LogAllRootObjectsAndMainMenuState();
#endif
        }

#if UNITY_EDITOR
        // TEST HOOK: Allow tests to force IsReady to true to avoid test hangs
        public void ForceReadyForTests()
        {
            IsReady = true;
            PreloadComplete?.Invoke();
        }
#endif
    }

    /*
        NOTE FOR WEBGL DEPLOYMENT:
        ----------------------------------------
        For WebGL builds, all Addressable assets must be hosted in the StreamingAssets folder and included in the build.
        The Addressables runtime path is overridden to use StreamingAssets for WebGL.
        Loading assets from external domains or URLs will result in CORS errors and assets will not load.
        Ensure your Addressables build path and load path are set to "StreamingAssets" or a relative path on your web server.
        See Unity documentation: https://docs.unity3d.com/Manual/webgl-networking.html
    */

    // IMPORTANT: The 'init' scene must be the first scene in the build and is responsible for preloading all assets.
    // Do not attempt to load any scene before 'init'.
    // All scene loading after 'init' must use PreloadAssets.LoadSceneWhenReady to ensure preloading is complete.
}

public static class PreloadAssetsDebug
{
    public static void LogAllRootObjectsAndMainMenuState()
    {
        var scene = SceneManager.GetActiveScene();
        Debug.Log($"[PreloadAssetsDebug] Active scene: {scene.name}");
        foreach (var go in scene.GetRootGameObjects())
        {
            Debug.Log($"[PreloadAssetsDebug] Root GameObject: {go.name}, activeSelf={go.activeSelf}");
            var mmc = go.GetComponentInChildren<TinyWalnutGames.UITKTemplates.MainMenu.MainMenuController>(true);
            if (mmc != null)
            {
                Debug.Log($"[PreloadAssetsDebug] MainMenuController found on '{mmc.gameObject.name}': GameObject.activeSelf={mmc.gameObject.activeSelf}, enabled={mmc.enabled}");
            }
        }
    }
}
