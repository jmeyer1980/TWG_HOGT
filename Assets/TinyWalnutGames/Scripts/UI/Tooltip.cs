using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace TinyWalnutGames.UI
{
    /// <summary>
    /// Tooltip component that can be instantiated from a UXML template.
    /// Encapsulates tooltip logic for UI Toolkit.
    /// </summary>
    [UxmlElement] // Use the new UI Toolkit attribute for UXML support
    public partial class Tooltip : VisualElement
    {
        private Label _description;

        // Placement constants (can be made public if you want to configure)
        private const float TooltipMargin = 16f;
        private const float TooltipSafeBound = 8f;

        // Addressable key for the tooltip template
        private const string TooltipUxmlAddress = "Assets/TinyWalnutGames/HiddenObjectGameTemplate/UI Toolkit/Templates/tooltip.uxml"; // Make sure this matches your Addressable key

        public event System.Action OnTooltipReady;

        /// <summary>
        /// Event invoked when the tooltip template is loaded and ready.
        /// </summary>
        public static event System.Action TooltipTemplateLoaded;

        // Shared static VisualTreeAsset for all tooltips
        private static VisualTreeAsset _sharedTooltipTemplate;
        private static bool _isLoadingTemplate = false;
        private readonly static System.Collections.Generic.List<Tooltip> _pendingTooltips = new();

        /// <summary>
        /// Returns true if the tooltip template is loaded and ready.
        /// </summary>
        public static bool IsTemplateReady => _sharedTooltipTemplate != null;

        public Tooltip()
        {
            Debug.Log("[Tooltip] Constructor called. (Should only be called from code, not UXML)");
            TryInitializeTemplate();
        }

        private void TryInitializeTemplate()
        {
            if (_sharedTooltipTemplate != null)
            {
                Debug.Log("[Tooltip] Using already loaded Addressables template.");
                _sharedTooltipTemplate.CloneTree(this);
                _description = this.Q<Label>("tooltip_description");
                OnTemplateLoaded();
            }
            else
            {
                Debug.Log("[Tooltip] Addressables template not loaded yet. Queuing this Tooltip instance for load.");
                _pendingTooltips.Add(this);
                if (!_isLoadingTemplate)
                {
                    _isLoadingTemplate = true;
                    Debug.Log("[Tooltip] Starting Addressables template load.");
#if UNITY_WEBGL // && !UNITY_EDITOR
                        LoadTooltipTemplateWebGL();
#else
                    _ = LoadTooltipTemplateAsync();
#endif
                }
            }
        }

        private void OnTemplateLoaded()
        {
            Debug.Log($"[Tooltip] Tooltip template loaded successfully. IsTemplateReady: {IsTemplateReady}");
            TooltipTemplateLoaded?.Invoke();
            OnTooltipReady?.Invoke();
        }

#if UNITY_WEBGL // && !UNITY_EDITOR
            /// <summary>
            /// Loads the tooltip template from Addressables using a callback (WebGL safe).
            /// </summary>
            public void LoadTooltipTemplateWebGL()
            {
                Addressables.LoadAssetAsync<VisualTreeAsset>(TooltipUxmlAddress).Completed += handle =>
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
                    {
                        _sharedTooltipTemplate = handle.Result;
                        foreach (var tooltip in _pendingTooltips)
                        {
                            _sharedTooltipTemplate.CloneTree(tooltip);
                            tooltip._description = tooltip.Q<Label>("tooltip_description");
                            tooltip.OnTemplateLoaded();
                        }
                        _pendingTooltips.Clear();
                    }
                    else
                    {
                        Debug.LogError("Tooltip UXML template not found in Addressables (WebGL)!");
                    }
                    Addressables.Release(handle);
                };
            }
#else
        /// <summary>
        /// Loads the tooltip template from Addressables asynchronously (non-WebGL).
        /// </summary>
        private async System.Threading.Tasks.Task LoadTooltipTemplateAsync()
        {
            var handle = Addressables.LoadAssetAsync<VisualTreeAsset>(TooltipUxmlAddress);
            await handle.Task;
            if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
            {
                _sharedTooltipTemplate = handle.Result;
                foreach (var tooltip in _pendingTooltips)
                {
                    _sharedTooltipTemplate.CloneTree(tooltip);
                    tooltip._description = tooltip.Q<Label>("tooltip_description");
                    tooltip.OnTemplateLoaded();
                }
                _pendingTooltips.Clear();
            }
            else
            {
                Debug.LogError("Tooltip UXML template not found in Addressables!");
            }
            Addressables.Release(handle);
        }
#endif

        /// <summary>
        /// Initialize the Tooltip from a VisualElement (e.g., loaded from UXML template).
        /// </summary>
        public void Initialize(VisualElement tooltipRoot)
        {
            if (tooltipRoot == null)
            {
                Debug.LogError("Tooltip root element is null. Cannot initialize Tooltip.");
                return;
            }
            else if (tooltipRoot.childCount == 0)
            {
                Debug.LogError("Tooltip root element has no children. Cannot initialize Tooltip.");
                return;
            }
            else if (tooltipRoot.Q<Label>("tooltip_description") == null)
            {
                Debug.LogError("Tooltip root element does not contain a Label with name 'tooltip_description'. Cannot initialize Tooltip.");
                return;
            }
            // else, we should be good... debug the goodness
            Debug.Log("Initializing Tooltip with provided root element. You should see it.");

            // Clear existing content and set up the tooltip
            this.Clear();
            this.Add(tooltipRoot);
            this.style.position = Position.Absolute;
            this.style.display = DisplayStyle.None;
            this.style.visibility = Visibility.Hidden;
            _description = tooltipRoot.Q<Label>("tooltip_description");
        }

        /// <summary>
        /// Set the tooltip text.
        /// </summary>
        /// <param name="text">The text to display in the tooltip. This should be provided directly from the UI document, not from a localization key.</param>
        public void SetText(string text)
        {
            if (_description != null)
            {
                Debug.Log($"[Tooltip] SetText: {text}");
                _description.text = text;
            }
            else
            {
                Debug.LogWarning("[Tooltip] SetText called but _description is null.");
            }
        }

        /// <summary>
        /// Show the tooltip at a given mouse position, using panel for bounds.
        /// </summary>
        public void Show(Vector2 mouseScreenPosition, IPanel panel = null)
        {
            Debug.Log($"[Tooltip] Show called at {mouseScreenPosition}");
            PlaceTooltip(mouseScreenPosition, panel);
            this.style.display = DisplayStyle.Flex;
            this.style.visibility = Visibility.Visible;
        }

        /// <summary>
        /// Hide the tooltip.
        /// </summary>
        public void Hide()
        {
            Debug.Log("[Tooltip] Hide called.");
            this.style.visibility = Visibility.Hidden;
            this.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// Place the tooltip near the given mouse position, ensuring it stays on screen.
        /// </summary>
        /// <param name="mouseScreenPosition">Mouse position in screen coordinates.</param>
        /// <param name="panel">Panel for coordinate conversion and bounds. If null, uses this.panel.</param>
        public void PlaceTooltip(Vector2 mouseScreenPosition, IPanel panel = null)
        {
            panel ??= this.panel;
            if (panel == null)
                return;

            // Convert screen to panel coordinates
            Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(panel, mouseScreenPosition);

            float tooltipWidth = this.resolvedStyle.width;
            float tooltipHeight = this.resolvedStyle.height;

            // Get panel size (fallback to Screen if not available)
            float panelWidth = panel.visualTree.resolvedStyle.width > 0 ? panel.visualTree.resolvedStyle.width : Screen.width;
            float panelHeight = panel.visualTree.resolvedStyle.height > 0 ? panel.visualTree.resolvedStyle.height : Screen.height;

            // Default position: bottom-right of cursor
            float left = panelPos.x + TooltipMargin;
            float top = panelPos.y + TooltipMargin;

            // Adjust horizontal position if overflowing right edge
            if (left + tooltipWidth + TooltipSafeBound > panelWidth)
            {
                left = panelPos.x - tooltipWidth - TooltipMargin;
                if (left < TooltipSafeBound)
                    left = panelWidth - tooltipWidth - TooltipSafeBound;
            }
            else if (left < TooltipSafeBound)
            {
                left = TooltipSafeBound;
            }

            // Adjust vertical position if overflowing bottom edge
            if (top + tooltipHeight + TooltipSafeBound > panelHeight)
            {
                top = panelPos.y - tooltipHeight - TooltipMargin;
                if (top < TooltipSafeBound)
                    top = panelHeight - tooltipHeight - TooltipSafeBound;
            }
            else if (top < TooltipSafeBound)
            {
                top = TooltipSafeBound;
            }

            this.style.left = left;
            this.style.top = top;
        }
    }
}
