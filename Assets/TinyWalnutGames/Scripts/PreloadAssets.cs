using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.Initialization;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using TinyWalnutGames.UI;
using System.IO;

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
        private ProgressBar progressBar;        

        // tooltip manager is attached to this game object.
        private TooltipManager tooltipManager;
        public VisualTreeAsset tooltipTemplate;

        [SerializeField]
        private string nextSceneName = "";
        [SerializeField]
        private int nextSceneIndex;

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

            tooltipManager = GetComponent<TooltipManager>();
            if (tooltipManager != null)
            {
                if (rootVisualElement != null)
                    tooltipManager.Initialize(rootVisualElement);
                else
                    Debug.LogWarning("TooltipManager initialized without a root VisualElement. Tooltips may not function correctly.");
            }
            else
            {
                Debug.LogWarning("TooltipManager component not found on this GameObject. Tooltips will not be available.");
            }
        }


        private void SetProgressBar(float progress, string message)
        {
            if (progressBar != null)
            {
                progress = Mathf.Clamp01(progress);
                progressBar.value = progress; // Assuming you have a ProgressBar component
            }
        }

        private void OnAddressablesReady(AsyncOperationHandle<IResourceLocator> handle)
        {
            Debug.Log("Addressables initialized.");
            Debug.Log("triggering tooltip preload from preload assets.");
            tooltipManager.Initialize(rootVisualElement);

            // Wait for the tooltip template to be ready before proceeding
            StartCoroutine(WaitForTooltipTemplate());
        }


        private IEnumerator WaitForTooltipTemplate()
        {
            float progress = 0.1f;
            // while tooltip is not ready, we will show a progress bar and wait, with a slight delay to avoid busy-waiting.
            while (!Tooltip.IsTemplateReady)
            {
                SetProgressBar(progress, "Waiting for Tooltip template to load...");
                progress = Mathf.Min(progress + 0.01f, 0.9f); // Slowly increase progress bar for user feedback
                yield return new WaitForSeconds(0.1f);
            }
            Debug.Log("Tooltip template loaded and ready.");
            OnTooltipTemplateLoaded();
        }


        // Add this method to handle the tooltip template loaded event
        private void OnTooltipTemplateLoaded()
        {            
            Debug.Log("Tooltip template loaded and ready.");
            StartCoroutine(LoadSceneAsync(nextSceneName));
        }

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            var asyncOp = SceneManager.LoadSceneAsync(sceneName);
            var Completed = new WaitForEndOfFrame();
            while (!asyncOp.isDone)
            {
                SetProgressBar(1f - asyncOp.progress * 0.1f, "Loading Scene...");
                yield return Completed;
            }
            OnSceneLoadComplete(asyncOp);
        }

        // Overload for index-based scene loading
        private IEnumerator LoadSceneAsync(int sceneIndex)
        {
            var asyncOp = SceneManager.LoadSceneAsync(sceneIndex);
            while (!asyncOp.isDone)
            {
                SetProgressBar(1f - asyncOp.progress * 0.1f, "Loading Scene...");
                yield return null;
            }
            OnSceneLoadComplete(asyncOp);
        }

        // Fix 2: Accept AsyncOperation for scene load completion
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
}
