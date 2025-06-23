using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.TextCore.Text;
using TinyWalnutGames.Localization; // Add this for localization helper

namespace TinyWalnutGames.UI
{
    /// <summary>
    /// Singleton MonoBehaviour manager for UI Toolkit tooltips. /fox
    /// </summary>
    public class TooltipManager : MonoBehaviour
    {
        public static TooltipManager Instance { get; private set; }

        [SerializeField]
        private VisualTreeAsset _tooltipTemplate;

        [Tooltip("The Tooltip instance used for displaying tooltips in the UI. Leave this blank")]
        public Tooltip _tooltip;
        private VisualElement _root;
        private bool _templateReady = false;
        private bool _initialized = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[TooltipManager] Duplicate instance detected, destroying this one.");
                Destroy(this);
                return;
            }
            Instance = this;
            // Deparent the singleton so it is not a child of any other GameObject
            transform.SetParent(null);
            DontDestroyOnLoad(this.gameObject);

            // Initialize the Tooltip system
            if (_tooltipTemplate == null)
            {
                Debug.LogError("[TooltipManager] Tooltip template is not assigned. Please assign a valid VisualTreeAsset.");
                return;
            }
            Tooltip tempTooltip = new Tooltip();
            tempTooltip.SetSharedTemplate(_tooltipTemplate);

            Tooltip.TooltipTemplateLoaded += OnTooltipTemplateLoaded;
            _templateReady = Tooltip.IsTemplateReady;
            Debug.Log("[TooltipManager] Singleton instance created.");
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
            Tooltip.TooltipTemplateLoaded -= OnTooltipTemplateLoaded;
        }

        /// <summary>
        /// Call this after your UIDocument is ready to set up the tooltip system.
        /// </summary>
        public void Initialize(VisualElement root)
        {
            if (_initialized && _root == root)
                return;

            _root = root;

            // Remove any Tooltip that may have been created by UXML/UI Document
            var uxmlTooltip = root.Q<Tooltip>();
            if (uxmlTooltip != null)
            {
                Debug.LogWarning("[TooltipManager] Removing Tooltip instance created by UXML/UI Document. Tooltip should only be created in code.");
                root.Remove(uxmlTooltip);
            }

            // Always create Tooltip in code to ensure Addressables template is used
            _tooltip = new Tooltip();
            root.Add(_tooltip);

            _tooltip.Hide();
            _initialized = true;
            Debug.Log("[TooltipManager] Initialized with root VisualElement and Tooltip created in code.");
        }

        private void OnTooltipTemplateLoaded()
        {
            _templateReady = true;
            Debug.Log("[TooltipManager] Received TooltipTemplateLoaded event. Template is now ready.");
        }

        public void SetFontAsset(FontAsset fontAsset)
        {
            if (_tooltip != null)
            {
                var label = _tooltip.Q<Label>("tooltip_description");
                if (label != null && fontAsset != null)
                {
                    label.style.unityFontDefinition = new UnityEngine.UIElements.FontDefinition { fontAsset = fontAsset };
                }
            }
        }

        public void SetFont(FontAsset fontAsset) => SetFontAsset(fontAsset);

        /// <summary>
        /// Show the tooltip using text from the UI document.
        /// </summary>
        /// <param name="tooltipText">The text to display in the tooltip.</param>
        /// <param name="mouseScreenPosition">The mouse position for tooltip placement.</param>
        public void Show(string tooltipText, Vector2 mouseScreenPosition)
        {
            if (!_initialized)
            {
                Debug.LogWarning("[TooltipManager] Not initialized. Call Initialize() with the root VisualElement.");
                return;
            }
            if (!Tooltip.IsTemplateReady)
            {
                Debug.LogWarning("[TooltipManager] Template not ready, skipping tooltip display.");
                return;
            }
            Debug.Log($"[TooltipManager] Show called with text: {tooltipText} at {mouseScreenPosition}");

            _tooltip.SetText(tooltipText);
            _tooltip.Show(mouseScreenPosition, _root.panel);
        }

        /// <summary>
        /// Show the tooltip with a value, using text from the UI document.
        /// </summary>
        /// <param name="tooltipText">The text to display in the tooltip.</param>
        /// <param name="value">A value to append to the tooltip text.</param>
        /// <param name="mouseScreenPosition">The mouse position for tooltip placement.</param>
        public void ShowWithValue(string tooltipText, float value, Vector2 mouseScreenPosition)
        {
            if (!_initialized)
            {
                Debug.LogWarning("[TooltipManager] Not initialized. Call Initialize() with the root VisualElement.");
                return;
            }
            if (!Tooltip.IsTemplateReady)
            {
                Debug.LogWarning("[TooltipManager] Template not ready, skipping tooltip display.");
                return;
            }
#if UNITY_WEBGL
            string localized = LocalizationHelper.GetLocalizedString("tooltips", tooltipText, tooltipText);
            string displayText = $"{localized}: {Mathf.RoundToInt(value * 100)}%";
            _tooltip.SetText(displayText);
#else
            string displayText = $"{tooltipText}: {Mathf.RoundToInt(value * 100)}%";
            _tooltip.SetText(displayText);
#endif
            Debug.Log($"[TooltipManager] ShowWithValue called for text: {tooltipText} value: {value}");
            _tooltip.Show(mouseScreenPosition, _root.panel);
        }

        public void Move(Vector2 mouseScreenPosition)
        {
            if (!_initialized) return;
            Debug.Log($"[TooltipManager] Move called to {mouseScreenPosition}");
            _tooltip.PlaceTooltip(mouseScreenPosition, _tooltip.panel);
        }

        public void Hide()
        {
            if (!_initialized) return;
            Debug.Log("[TooltipManager] Hide called.");
            _tooltip.Hide();
        }
    }
}