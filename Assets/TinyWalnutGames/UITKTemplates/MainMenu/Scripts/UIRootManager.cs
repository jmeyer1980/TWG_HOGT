using UnityEngine;
using UnityEngine.UIElements;

namespace TinyWalnutGames.UITKTemplates.Tools
{
public class UIRootManager : MonoBehaviour
    {
        public static UIRootManager Instance { get; private set; }
        public UIDocument RootUIDocument; // Assign in inspector or instantiate at runtime
        // a list of UI Documents for hot-swapping scenes
        public string CurrentLocalCode { get; private set; }

        // event for localization changes useable by scene controllers
        public event System.Action LocaleChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // sign up for locale change event
            LocalizationHelper.LocaleChanged += OnLocaleChanged;
        }        

        /// <summary>
        /// Assigns the given scene UIDocument as the current root UIDocument.
        /// To be used by scene controllers to register their UI when the scene loads.
        /// No parenting or visual tree manipulation is performed here.
        /// </summary>
        public void AttachSceneUIDocument(UIDocument sceneUIDocument)
        {
            // If the provided UIDocument is null, keep searching until we find one or exhaust options
            UIDocument doc = sceneUIDocument;
            if (doc == null)
            {
                doc = FindFirstObjectByType<UIDocument>();
                if (doc == null)
                {
                    Debug.LogWarning("[UIRootManager] AttachSceneUIDocument: No valid UIDocument found in the scene.");
                    return; // Exit if no valid UIDocument is available
                }
            }

            // Now we are guaranteed to have a non-null UIDocument
            if (RootUIDocument != doc)
            {
                RootUIDocument = doc;
                Debug.Log($"[UIRootManager] RootUIDocument assigned to '{doc.gameObject.name}' at {Time.time}.");
            }
            else
            {
                Debug.Log($"[UIRootManager] RootUIDocument already set to '{doc.gameObject.name}' at {Time.time}.");
            }

            LocaleChanged?.Invoke();
        }

        public void TheLocaleChanged()
        {
            // Notify listeners that the locale has changed
            LocaleChanged?.Invoke();
        }

        // Handler for LocalizationHelper.LocaleChanged
        private void OnLocaleChanged()
        {
            // Only set locale if changed by user action, not here
            // Remove SetLocale and PlayerPrefs logic from here
            // Instead, just update CurrentLocalCode and notify listeners

            CurrentLocalCode = LocalizationHelper.GetCurrentLocaleCode();
            LocaleChanged?.Invoke();
        }

        //get and retun the locale code to the current UI Controller in the scene
        public string CurrentLocaleCode()
        {
            var localizationTableHolder = WebglLocalizationTableHolder.Instance;
            if (localizationTableHolder == null)
            {
                Debug.LogWarning("LocalizationTableHolder instance not found. Returning default locale code.");
                return "en"; // Default to English if no localization table is found
            }

            var localizationTable = localizationTableHolder.GetComponent<WebglLocalizationTableHolder>();
            if (localizationTable == null || localizationTable.TableByNameAndLocale.Count == 0)
            {
                Debug.LogWarning("LocalizationTableHolder or Tables not found. Returning default locale code.");
                return "en"; // Default to English if no localization table is found
            }

            // Return the current locale code from GetCurrentLocaleCode method
            CurrentLocalCode = LocalizationHelper.GetCurrentLocaleCode();
            return CurrentLocalCode;
        }
    }
}