using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace TinyWalnutGames.UITKTemplates.MainMenu
{
    /// <summary>
    /// Handles addressable asset preloading and scene loading, with no UI dependencies.
    /// </summary>
    public class AssetPreloader : MonoBehaviour
    {
        public static AssetPreloader Instance { get; private set; }

        public event Action<float, string> ProgressChanged;
        public event Action PreloadComplete;

        [Tooltip("If set, this scene will be loaded automatically after preloading is complete. Leave blank to stay on init scene.")]
        public string autoAdvanceSceneName = "";

        // Can load scenes bool
        public bool CanLoadScenes => IsReady;
        
        [Tooltip("If set, this scene will be loaded after preloading if autoAdvanceSceneName is empty.")]
        [SerializeField]
        private string nextSceneName = "";

        [Tooltip("If set (>=0), this scene index will be loaded after preloading if autoAdvanceSceneName and nextSceneName are empty.")]
        [SerializeField]
        private int nextSceneIndex = -1;

        private readonly Dictionary<string, UnityEngine.Object> _assetCache = new();
        public bool IsReady { get; private set; } = false;

        public IEnumerable<string> LoadedPreloadKeys => _assetCache.Keys;

        private void Awake()
        {
            // Deparent this object before any initialization to avoid issues with scene loading
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }

            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                DestroyImmediate(gameObject);
                return;
            }
        }

        private void Start()
        {
            Addressables.InitializeAsync().Completed += OnAddressablesReady;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
            PreloadComplete = null;
        }

        private void OnAddressablesReady(AsyncOperationHandle<IResourceLocator> handle)
        {
            StartCoroutine(PreloadAllCoroutine());
        }

        private IEnumerator PreloadAllCoroutine()
        {
            // 1. Preload all addressable assets labeled "Preload"
            var downloadHandle = Addressables.DownloadDependenciesAsync("Preload");
            yield return downloadHandle;
            var locationsHandle = Addressables.LoadResourceLocationsAsync("Preload");
            yield return locationsHandle;

            var locations = locationsHandle.Result;
            int total = locations.Count;
            int loaded = 0;

            ProgressChanged?.Invoke(0f, $"Loading assets... (0/{total})");

            if (total == 0)
            {
                Debug.LogWarning("[AssetPreloader] No assets found with label 'Preload'.");
            }

            foreach (var loc in locations)
            {
                var handle = Addressables.LoadAssetAsync<UnityEngine.Object>(loc.PrimaryKey);
                yield return handle;
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    _assetCache[loc.PrimaryKey] = handle.Result;
                    Debug.Log($"[AssetPreloader] Loaded asset: {loc.PrimaryKey} ({loaded + 1}/{total})"); // Per-asset log
                }
                else
                {
                    Debug.LogWarning($"[AssetPreloader] Failed to load asset at address '{loc.PrimaryKey}'.");
                }
                loaded++;
                float progress = total > 0 ? (float)loaded / total : 1f;
                ProgressChanged?.Invoke(progress, $"Loading assets... ({loaded}/{total})");
                yield return null; // Yield to allow UI updates and then continue loading
//#if UNITY_EDITOR
//                // Artificial delay for testing progress bar updates in Editor only
//                yield return new WaitForSeconds(0.1f);
//#endif
            }

            Debug.Log("[AssetPreloader] Loaded Preload asset keys: " + string.Join(", ", _assetCache.Keys));
            Debug.Log("[AssetPreloader] All assets with label 'Preload' loaded.");

            // 2. Mark as ready and raise event
            IsReady = true;
            ProgressChanged?.Invoke(1f, "All assets loaded!");
            PreloadComplete?.Invoke();

            // --- AUTO-ADVANCE LOGIC ---
            if (!string.IsNullOrEmpty(autoAdvanceSceneName))
            {
                LoadSceneWhenReady(autoAdvanceSceneName);
            }
            else if (!string.IsNullOrEmpty(nextSceneName))
            {
                LoadSceneWhenReady(nextSceneName);
            }
            else if (nextSceneIndex >= 0)
            {
                LoadSceneWhenReady(nextSceneIndex);
            }
        }

        public T Get<T>(string key) where T : UnityEngine.Object
        {
            if (_assetCache.TryGetValue(key, out var obj))
            {
                return obj as T;
            }
            Debug.LogWarning($"[AssetPreloader] Asset with key '{key}' not found in cache.");
            return null;
        }

        public static void LoadSceneWhenReady(string sceneNameOrPath)
        {
            if (Instance == null || !Instance.IsReady)
            {
                Debug.LogWarning("[AssetPreloader] Cannot load scene: preloading not complete.");
                return;
            }
            Instance.StartCoroutine(Instance.LoadSceneWhenReadyCoroutine(sceneNameOrPath));
        }

        public static void LoadSceneWhenReady(int sceneIndex)
        {
            if (Instance == null || !Instance.IsReady)
            {
                Debug.LogWarning($"[AssetPreloader] Scene load request for index '{sceneIndex}' ignored: preloading not complete.");
                return;
            }
            Instance.StartCoroutine(Instance.LoadSceneWhenReadyCoroutine(sceneIndex));
        }

        private IEnumerator LoadSceneWhenReadyCoroutine(string sceneNameOrPath)
        {
            var async = SceneManager.LoadSceneAsync(sceneNameOrPath);
            if (async == null)
            {
                Debug.LogError($"[AssetPreloader] Failed to start loading scene '{sceneNameOrPath}'.");
                yield break;
            }
            yield return async;
        }

        private IEnumerator LoadSceneWhenReadyCoroutine(int sceneIndex)
        {
            var asyncOp = SceneManager.LoadSceneAsync(sceneIndex);
            if (asyncOp == null)
            {
                Debug.LogError($"[AssetPreloader] Failed to start loading scene with index '{sceneIndex}'.");
                yield break;
            }
            yield return asyncOp;
        }
    }
}
