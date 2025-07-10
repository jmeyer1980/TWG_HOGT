using UnityEngine;
using UnityEngine.UIElements;

namespace TinyWalnutGames.UITKTemplates.MainMenu
{
    /// <summary>
    /// Handles the preload scene's UI, subscribing to AssetPreloader events.
    /// </summary>
    public class PreloadUIController : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private string progressBarName = "progressbar";
        [SerializeField] private TooltipManager tooltipManager;
        private ProgressBar progressBar;

        private void Awake()
        {
            if (uiDocument == null)
                uiDocument = FindFirstObjectByType<UIDocument>();
            if (uiDocument != null)
            {
                progressBar = uiDocument.rootVisualElement.Q<ProgressBar>(progressBarName);
                if (progressBar == null)
                {
                    Debug.LogWarning($"[PreloadUIController] ProgressBar with name '{progressBarName}' not found in UIDocument.");
                }
            }
            else
            {
                Debug.LogWarning("[PreloadUIController] UIDocument not found.");
            }
            if (tooltipManager == null)
            {
                tooltipManager = FindFirstObjectByType<TooltipManager>();
                if (tooltipManager == null)
                {
                    Debug.LogWarning("[PreloadUIController] TooltipManager not found.");
                }
            }
        }

        private void OnEnable()
        {
            if (AssetPreloader.Instance != null)
            {
                AssetPreloader.Instance.ProgressChanged += OnProgressChanged;
                AssetPreloader.Instance.PreloadComplete += OnPreloadComplete;
            }
        }

        private void OnDisable()
        {
            if (AssetPreloader.Instance != null)
            {
                AssetPreloader.Instance.ProgressChanged -= OnProgressChanged;
                AssetPreloader.Instance.PreloadComplete -= OnPreloadComplete;
            }
        }

        private void OnProgressChanged(float value, string message)
        {
            if (progressBar != null)
            {
                progressBar.value = Mathf.Clamp01(value);
                progressBar.MarkDirtyRepaint();
                progressBar.title = message;
            }
            else
            {
                Debug.LogWarning("[PreloadUIController] ProgressBar is null in OnProgressChanged.");
            }
        }

        private void OnPreloadComplete()
        {
            // Optionally hide or disable the preload UI
            if (progressBar != null)
            {
                progressBar.title = "Done!";
            }
        }
    }
}
