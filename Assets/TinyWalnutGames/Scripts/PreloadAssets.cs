using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using TinyWalnutGames.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TinyWalnutGames
{
    public class PreloadAssets : MonoBehaviour
    {
        [Header("UI Document (Optional)")]
        [Tooltip("Assign your UI Document here if you want to use it for tooltips or other UI elements. Can be left empty if not needed.")]
        [SerializeField]
        private UIDocument uiDocument;
        private VisualElement rootVisualElement;        
        private VisualElement progressBarRoot;
        private VisualElement progressBarFill;

        [Header("Addressable Keys (Full Paths Required)")]
        [Tooltip("Assign full addressable asset paths here, e.g. 'Assets/TinyWalnutGames/HiddenObjectGameTemplate/UI Toolkit/Templates/tooltip.uxml'")]
        [SerializeField]
        private List<string> addressableKeysToPreload = new();

        [SerializeField]
        private string nextSceneName = "";
        [SerializeField]
        private int nextSceneIndex = -1;

#if UNITY_EDITOR
        private void OnValidate()
        {
            foreach (var key in addressableKeysToPreload)
            {
                if (string.IsNullOrWhiteSpace(key) || !key.StartsWith("Assets/"))
                {
                    Debug.LogWarning($"[PreloadAssets] Addressable key '{key}' is not a full asset path. Please assign full paths in the inspector.", this);
                }
            }
        }
#endif

        private void Start()
        {
            if (uiDocument != null)
            {
                rootVisualElement = uiDocument.rootVisualElement;
                // Get the root of the progress bar
                progressBarRoot = rootVisualElement?.Q<VisualElement>("progressbar");
                // Get the fill element
                progressBarFill = progressBarRoot?
                    .Q<VisualElement>("unity-progress-bar")?
                    .Q<VisualElement>(className: "unity-progress-bar__background")?
                    .Q<VisualElement>(className: "unity-progress-bar__progress");
                SetProgressBar(0f, "Initializing...");
            }
            StartCoroutine(InitializeRoutine());
        }

        private IEnumerator InitializeRoutine()
        {
#if UNITY_WEBGL
            Debug.Log("[PreloadAssets] Initializing Addressables for WebGL...");
            var initHandle = Addressables.InitializeAsync();
            while (!initHandle.IsDone)
            {
                SetProgressBar(0.05f, "Initializing Addressables...");
                yield return null;
            }
            if (initHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("[PreloadAssets] Addressables initialization failed on WebGL.");
                SetProgressBar(0f, "Initialization Failed");
                yield break;
            }
            Debug.Log("[PreloadAssets] Addressables initialized successfully for WebGL.");
#endif

            int totalSteps = addressableKeysToPreload.Count + 1; // +1 for Tooltip
            int completedSteps = 0;

            // 1. Preload Addressable assets
            for (int i = 0; i < addressableKeysToPreload.Count; i++)
            {
                string key = addressableKeysToPreload[i];
                if (string.IsNullOrWhiteSpace(key) || !key.StartsWith("Assets/"))
                {
                    Debug.LogError($"[PreloadAssets] Invalid addressable key: '{key}'. Must be a full asset path.");
                    continue;
                }

                var handle = Addressables.LoadAssetAsync<Object>(key);
                while (!handle.IsDone)
                {
                    float progress = (completedSteps + handle.PercentComplete) / totalSteps;
                    SetProgressBar(progress, $"Loading: {key}");
                    yield return null;
                }

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError($"[PreloadAssets] Failed to preload Addressable: {key}");
                }
                else
                {
                    Debug.Log($"[PreloadAssets] Preloaded Addressable: {key}");
                }
                completedSteps++;
                SetProgressBar((float)completedSteps / totalSteps, $"Loaded: {key}");
            }

            // 2. Wait for Tooltip template to be ready
            if (!Tooltip.IsTemplateReady)
            {
                bool ready = false;
                System.Action onReady = () => { ready = true; };

                Tooltip.TooltipTemplateLoaded += onReady;

                Debug.Log("[PreloadAssets] Waiting for Tooltip template to load...");
                while (!Tooltip.IsTemplateReady && !ready)
                {
                    float progress = (float)completedSteps / totalSteps;
                    SetProgressBar(progress, "Loading Tooltip Template...");
                    yield return null;
                }
                Tooltip.TooltipTemplateLoaded -= onReady;
                Debug.Log("[PreloadAssets] Tooltip template is ready.");
            }
            completedSteps++;
            SetProgressBar(1f, "Loading Scene...");

            // 3. Load the next scene
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                Debug.Log($"[PreloadAssets] Loading scene by name: {nextSceneName}");
                yield return LoadSceneAsync(nextSceneName);
            }
            else if (nextSceneIndex >= 0)
            {
                Debug.Log($"[PreloadAssets] Loading scene by index: {nextSceneIndex}");
                yield return LoadSceneAsync(nextSceneIndex);
            }
            else
            {
                Debug.LogWarning("[PreloadAssets] No next scene specified to load.");
                SetProgressBar(1f, "Done");
            }
        }

        private void SetProgressBar(float progress, string message)
        {
            if (progressBarFill != null)
            {
                progress = Mathf.Clamp01(progress);
                progressBarFill.style.width = Length.Percent(progress * 100f);
                progressBarFill.tooltip = message;
            }
        }

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            var asyncOp = SceneManager.LoadSceneAsync(sceneName);
            while (!asyncOp.isDone)
            {
                SetProgressBar(1f - asyncOp.progress * 0.1f, "Loading Scene...");
                yield return null;
            }
        }

        private IEnumerator LoadSceneAsync(int sceneIndex)
        {
            var asyncOp = SceneManager.LoadSceneAsync(sceneIndex);
            while (!asyncOp.isDone)
            {
                SetProgressBar(1f - asyncOp.progress * 0.1f, "Loading Scene...");
                yield return null;
            }
        }
    }
}
