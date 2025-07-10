using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TinyWalnutGames.UITKTemplates.Tools
{
    /// <summary>
    /// Helper for validating and repairing UIDocument and locale state across scenes.
    /// </summary>
    public static class UIDocumentValidator
    {
        /// <summary>
        /// Validates and repairs a UIDocument and locale state. Optionally shows a dev-only toast.
        /// </summary>
        /// <param name="uiDocument">The UIDocument to validate.</param>
        /// <param name="uiVisualTreeAsset">The expected VisualTreeAsset.</param>
        /// <param name="rootVisualElement">The root VisualElement (out).</param>
        /// <param name="progressBar">The ProgressBar (out, or pass in an existing one).</param>
        /// <param name="showDevToast">Show a dev-only toast if true (default: true, only in editor).</param>
        /// <param name="progressBarName">The name of the ProgressBar to query if progressBar is null. If null, no ProgressBar is required.</param>
        /// <returns>True if valid or fixed, false if unrecoverable.</returns>
        public static bool ValidateOrFixUIDocument(
            MonoBehaviour context,
            ref UIDocument uiDocument,
            VisualTreeAsset uiVisualTreeAsset,
            out VisualElement rootVisualElement,
            out ProgressBar progressBar,
            bool showDevToast = true,
            string progressBarName = "progressbar")
        {
            bool wasFixed = false;
            rootVisualElement = null;
            progressBar = null;

            // Log GameObject and UIDocument info
            string contextTypeName = context != null ? context.GetType().Name : "NULL";

            // Log current and expected VisualTreeAsset info
            var expectedVta = uiVisualTreeAsset;
            var currentVta = null as VisualTreeAsset;
            if (uiDocument != null && uiDocument.visualTreeAsset != null)
            {
                currentVta = uiDocument.visualTreeAsset;
            }
#if UNITY_EDITOR
            string currentVtaPath = (currentVta != null) ? UnityEditor.AssetDatabase.GetAssetPath(currentVta) : "NULL";
            string expectedVtaPath = (expectedVta != null) ? UnityEditor.AssetDatabase.GetAssetPath(expectedVta) : "NULL";
#else
            string currentVtaPath = "N/A";
            string expectedVtaPath = "N/A";
#endif
            Debug.Log("[" + (context != null ? context.GetType().Name : "NULL") + "] Current visualTreeAsset: name='" + (currentVta != null ? currentVta.name : "NULL") + "', id=" + (currentVta != null ? currentVta.GetInstanceID() : -1) + ", path=" + currentVtaPath);
            Debug.Log("[" + (context != null ? context.GetType().Name : "NULL") + "] Expected visualTreeAsset: name='" + (expectedVta != null ? expectedVta.name : "NULL") + "', id=" + (expectedVta != null ? expectedVta.GetInstanceID() : -1) + ", path=" + expectedVtaPath);

            // Try to assign UIDocument if null, and only set wasFixed if it worked
            if (uiDocument == null)
            {
                uiDocument = UnityEngine.Object.FindFirstObjectByType<UIDocument>();
                if (uiDocument == null)
                {
                    Debug.LogError("[" + context.GetType().Name + "] UIDocument is not assigned and could not be found in the scene.");
                    return false;
                }
                else
                {
                    return wasFixed = true;
                }
            }

            // Try to assign visualTreeAsset if needed, and only set wasFixed if it worked
            if (uiVisualTreeAsset != null && !ReferenceEquals(uiDocument.visualTreeAsset, uiVisualTreeAsset))
            {
                Debug.LogWarning("[" + context.GetType().Name + "] UIDocument visualTreeAsset does not match the expected template. Reassigning.");
                uiDocument.visualTreeAsset = uiVisualTreeAsset;

                // Log the actual visualTreeAsset assigned
                var assignedVta = uiDocument.visualTreeAsset;
#if UNITY_EDITOR
                string assignedVtaPath = (assignedVta != null) ? UnityEditor.AssetDatabase.GetAssetPath(assignedVta) : "NULL";
#else
                string assignedVtaPath = "N/A";
#endif
                // debug log the assigned visualTreeAsset
                Debug.Log("[" + context.GetType().Name + "] Assigned visualTreeAsset: name='" + (assignedVta != null ? assignedVta.name : "NULL") + "', id=" + (assignedVta != null ? assignedVta.GetInstanceID() : -1) + ", path=" + assignedVtaPath);

                // Force update of the rootVisualElement after assigning visualTreeAsset
                uiDocument.enabled = false;
                uiDocument.enabled = true;

                // Re-fetch rootVisualElement and verify
                rootVisualElement = uiDocument.rootVisualElement;
                if (rootVisualElement == null)
                {
                    Debug.LogError("[" + context.GetType().Name + "] After assigning visualTreeAsset, UIDocument.rootVisualElement is still null.");
                    return false;
                }
                return wasFixed = true;
            }
            else
            {
                rootVisualElement = uiDocument.rootVisualElement;
            }

            if (rootVisualElement == null)
            {
                Debug.LogError("[" + context.GetType().Name + "] UIDocument.rootVisualElement is null. Ensure the UIDocument is set up correctly and the VisualTreeAsset is assigned.");
                return false;
            }

            // Additional diagnostics: log children of rootVisualElement
            Debug.Log(
                "[" + (context != null ? context.GetType().Name : "NULL") + "] rootVisualElement type: " +
                (rootVisualElement != null ? rootVisualElement.GetType().Name : "NULL") +
                ", name: " + (rootVisualElement != null ? rootVisualElement.name : "NULL") +
                ", childCount: " + (rootVisualElement != null ? rootVisualElement.childCount.ToString() : "NULL")
            );
            int childIdx = 0;
            foreach (var child in rootVisualElement.Children())
            {
                Debug.Log(
                    "[" + (context != null ? context.GetType().Name : "NULL") + "] rootVisualElement type: " +
                    " child[" + childIdx + "]: name='" + child.name + "', type=" + child.GetType().Name);
                childIdx++;
            }

            // Extra verification: check for a known element from the template (e.g., "level_card_list")
            var expectedElement = rootVisualElement.Q("level_card_list");
            if (expectedElement == null)
            {
#if UNITY_EDITOR
                string expectedUxmlName = (uiVisualTreeAsset != null) ? uiVisualTreeAsset.name : "NULL";
                int expectedUxmlId = (uiVisualTreeAsset != null) ? uiVisualTreeAsset.GetInstanceID() : -1;
                string expectedUxmlPath = (uiVisualTreeAsset != null) ? UnityEditor.AssetDatabase.GetAssetPath(uiVisualTreeAsset) : "NULL";
                VisualTreeAsset assignedVta = null;
                if (uiDocument != null)
                    assignedVta = uiDocument.visualTreeAsset;
                string assignedUxmlName = assignedVta != null ? assignedVta.name : "NULL";
                int assignedUxmlId = assignedVta != null ? assignedVta.GetInstanceID() : -1;
                string assignedUxmlPath = (assignedVta != null) ? UnityEditor.AssetDatabase.GetAssetPath(assignedVta) : "NULL";
#else
                string expectedUxmlName = (uiVisualTreeAsset != null) ? uiVisualTreeAsset.name : "NULL";
                int expectedUxmlId = (uiVisualTreeAsset != null) ? uiVisualTreeAsset.GetInstanceID() : -1;
                string expectedUxmlPath = "N/A";
                var assignedVta = (uiDocument != null) ? uiDocument.visualTreeAsset : null;
                string assignedUxmlName = (assignedVta != null) ? assignedVta.name : "NULL";
                int assignedUxmlId = (assignedVta != null) ? assignedVta.GetInstanceID() : -1;
                string assignedUxmlPath = "N/A";
#endif
                if (expectedUxmlId != assignedUxmlId || expectedUxmlPath != assignedUxmlPath)
                {
                    Debug.LogError(
                        "[" + (context != null ? context.GetType().Name : "NULL") + "] rootVisualElement does not contain required element 'level_card_list'.\n" +
                        "Expected VisualTreeAsset: name='" + expectedUxmlName + "', id=" + expectedUxmlId + ", path=" + expectedUxmlPath + "\n" +
                        "Assigned VisualTreeAsset: name='" + assignedUxmlName + "', id=" + assignedUxmlId + ", path=" + assignedUxmlPath + "\n" +
                        "The assigned UXML may not be correct or the UI hierarchy is not built as expected."
                    );
                }
                else
                {
                    Debug.LogWarning(
                        "[" + (context != null ? context.GetType().Name : "NULL") + "] rootVisualElement does not contain required element 'level_card_list', but the assigned VisualTreeAsset matches the expected one.\n" +
                        "VisualTreeAsset: name='" + expectedUxmlName + "', id=" + expectedUxmlId + ", path=" + expectedUxmlPath + "\n" +
                        "This likely means the UXML content is missing required elements, or there is a duplicate UIDocument, or script execution order/UI initialization is incorrect. Please check the UXML content and scene setup."
                    );
                }
                return false;
            }

            // ProgressBar logic: only require if a name is provided
            ProgressBar foundProgressBar = null;
            if (!string.IsNullOrEmpty(progressBarName))
            {
                foundProgressBar = rootVisualElement.Q<ProgressBar>(progressBarName);
                if (foundProgressBar == null)
                {
                    Debug.LogWarning("[" + context.GetType().Name + "] ProgressBar with name '" + progressBarName + "' not found in UIDocument after validation. Listing children for diagnostics:");
                    foreach (var child in rootVisualElement.Children())
                        Debug.Log("[" + context.GetType().Name + "] Child: " + child.name + " (" + child.GetType() + ")");
                    // Not a hard error: allow validation to succeed without a progress bar
                }
                else
                {
                    if (foundProgressBar.name != progressBarName)
                    {
                        Debug.LogWarning("[" + context.GetType().Name + "] ProgressBar name does not match expected '" + progressBarName + "'. Reassigning.");
                        foundProgressBar.name = progressBarName;
                        // Verify the fix
                        if (foundProgressBar.name == progressBarName)
                        {
                            wasFixed = true;
                        }
                        else
                        {
                            Debug.LogError("[" + context.GetType().Name + "] Failed to set ProgressBar name to '" + progressBarName + "'.");
                            // Not a hard error, continue
                        }
                    }
                }
            }
            progressBar = foundProgressBar;

            SetProgressBar(progressBar, 0f, "Validating UIDocument...");
            Debug.Log("[" + (context != null ? context.GetType().Name : "NULL") + "] UIDocument is correctly set up with the expected root visual element.");

            ValidateOrFixLocale(context, showDevToast, out bool localeWasFixed);

#if UNITY_EDITOR
            if (showDevToast && rootVisualElement != null)
            {
                var toast = new Label("✅ UIDocument and locale verified");
                toast.style.position = Position.Absolute;
                toast.style.top = 10;
                toast.style.right = 10;
                toast.style.backgroundColor = new StyleColor(new Color(0, 0.5f, 0, 0.8f));
                toast.style.color = Color.white;
                toast.style.paddingLeft = 8;
                toast.style.paddingRight = 8;
                toast.style.paddingTop = 4;
                toast.style.paddingBottom = 4;
                toast.style.unityFontStyleAndWeight = FontStyle.Bold;
                rootVisualElement.Add(toast);
                if (context != null)
                    context.StartCoroutine(RemoveToastAfterDelay(toast, 2f));
            }
#endif
            return wasFixed || localeWasFixed;
        }

        /// <summary>
        /// Generalized UI Document validator using a config object for modular validation.
        /// </summary>
        public static bool ValidateOrFixUIDocument(
            MonoBehaviour context,
            ref UIDocument uiDocument,
            VisualTreeAsset uiVisualTreeAsset,
            UIDocumentValidationConfig config,
            out VisualElement rootVisualElement,
            out ProgressBar progressBar,
            bool showDevToast = true)
        {
            bool wasFixed = false;
            rootVisualElement = null;
            progressBar = null;

            // Log GameObject and UIDocument info
            string contextName = context != null ? context.GetType().Name : "NULL";
            string gameObjectName = "NULL";
            int uiDocumentId = -1;
            if (!ReferenceEquals(uiDocument, null))
            {
                if (!ReferenceEquals(uiDocument.gameObject, null))
                    gameObjectName = uiDocument.gameObject.name;
                uiDocumentId = uiDocument.GetInstanceID();
            }
            Debug.Log($"[{contextName}] ValidateOrFixUIDocument: GameObject='{gameObjectName}', UIDocument instanceID={uiDocumentId}");

            var currentVta = null as VisualTreeAsset;
            if (!ReferenceEquals(uiDocument, null))
                currentVta = uiDocument.visualTreeAsset;
            else
            {
                Debug.LogError($"[{contextName}] UIDocument is null. Cannot validate visualTreeAsset.");
                rootVisualElement = null;
                return false;
            }

            var expectedVta = uiVisualTreeAsset;
#if UNITY_EDITOR
            string currentVtaPath = (currentVta != null) ? UnityEditor.AssetDatabase.GetAssetPath(currentVta) : "NULL";
            string expectedVtaPath = (expectedVta != null) ? UnityEditor.AssetDatabase.GetAssetPath(expectedVta) : "NULL";
#else
            string currentVtaPath = "N/A";
            string expectedVtaPath = "N/A";
#endif
            string contextType = (context != null) ? context.GetType().Name : "NULL";
            string currentVtaName = "NULL";
            int currentVtaId = -1;
            if (!ReferenceEquals(currentVta, null))
            {
                currentVtaName = currentVta.name;
                currentVtaId = currentVta.GetInstanceID();
            }
            string expectedVtaName = (expectedVta != null) ? expectedVta.name : "NULL";
            int expectedVtaId = (expectedVta != null) ? expectedVta.GetInstanceID() : -1;

            Debug.Log($"[{contextType}] Current visualTreeAsset: name='{currentVtaName}', id={currentVtaId}, path={currentVtaPath}");
            Debug.Log($"[{contextType}] Expected visualTreeAsset: name='{expectedVtaName}', id={expectedVtaId}, path={expectedVtaPath}");
            // Try to assign UIDocument if null, and only set wasFixed if it worked
            if (uiDocument == null)
            {
                uiDocument = UnityEngine.Object.FindFirstObjectByType<UIDocument>();
                if (uiDocument == null)
                {
                    Debug.LogError($"[{(context != null ? context.GetType().Name : "NULL")}] UIDocument is not assigned and could not be found in the scene.");
                    return false;
                }
                else
                {
                    wasFixed = true;
                }
            }

            // Try to assign visualTreeAsset if needed, and only set wasFixed if it worked
            if (uiVisualTreeAsset != null && !ReferenceEquals(uiDocument.visualTreeAsset, uiVisualTreeAsset))
            {
                Debug.LogWarning($"[{(context != null ? context.GetType().Name : "NULL")}] UIDocument visualTreeAsset does not match the expected template. Reassigning.");
                uiDocument.visualTreeAsset = uiVisualTreeAsset;

                // Log the actual visualTreeAsset assigned
                var assignedVta = uiDocument.visualTreeAsset;
#if UNITY_EDITOR
                string assignedVtaPath = (assignedVta != null) ? UnityEditor.AssetDatabase.GetAssetPath(assignedVta) : "NULL";
#else
                string assignedVtaPath = "N/A";
#endif
                Debug.Log($"[{(context != null ? context.GetType().Name : "NULL")}] Assigned visualTreeAsset: name='{(assignedVta != null ? assignedVta.name : "NULL")}', id={(assignedVta != null ? assignedVta.GetInstanceID() : -1)}, path={assignedVtaPath}");

                // Force update of the rootVisualElement after assigning visualTreeAsset
                uiDocument.enabled = false;
                uiDocument.enabled = true;

                // Re-fetch rootVisualElement and verify
                rootVisualElement = uiDocument.rootVisualElement;
                if (rootVisualElement == null)
                {
                    Debug.LogError($"[{(context != null ? context.GetType().Name : "NULL")}] After assigning visualTreeAsset, UIDocument.rootVisualElement is still null.");
                    return false;
                }
                wasFixed = true;
            }
            else
            {
                rootVisualElement = uiDocument.rootVisualElement;
            }

            if (rootVisualElement == null)
            {
                Debug.LogError($"[{(context != null ? context.GetType().Name : "NULL")}] UIDocument.rootVisualElement is null. Ensure the UIDocument is set up correctly and the VisualTreeAsset is assigned.");
                return false;
            }

            // Additional diagnostics: log children of rootVisualElement
            Debug.Log($"[{(context != null ? context.GetType().Name : "NULL")}] rootVisualElement type: {rootVisualElement.GetType().Name}, name: {rootVisualElement.name}, childCount: {rootVisualElement.childCount}");
            int childIdx = 0;
            foreach (var child in rootVisualElement.Children())
            {
                Debug.Log($"[{(context != null ? context.GetType().Name : "NULL")}] rootVisualElement child[{childIdx}]: name='{child.name}', type={child.GetType().Name}");
                childIdx++;
            }

//            // Extra verification: check for a known element from the template (e.g., "level_card_list")
//            var expectedElement = rootVisualElement.Q("level_card_list");
//            if (expectedElement == null)
//            {
//#if UNITY_EDITOR
//                string expectedUxmlName = (uiVisualTreeAsset != null) ? uiVisualTreeAsset.name : "NULL";
//                int expectedUxmlId = (uiVisualTreeAsset != null) ? uiVisualTreeAsset.GetInstanceID() : -1;
//                string expectedUxmlPath = (uiVisualTreeAsset != null) ? UnityEditor.AssetDatabase.GetAssetPath(uiVisualTreeAsset) : "NULL";
//                var assignedVta = (uiDocument != null) ? uiDocument.visualTreeAsset : null;
//                string assignedUxmlName = (assignedVta != null) ? assignedVta.name : "NULL";
//                int assignedUxmlId = (assignedVta != null) ? assignedVta.GetInstanceID() : -1;
//                string assignedUxmlPath = (assignedVta != null) ? UnityEditor.AssetDatabase.GetAssetPath(assignedVta) : "NULL";
//#else
//                string expectedUxmlName = (uiVisualTreeAsset != null) ? uiVisualTreeAsset.name : "NULL";
//                int expectedUxmlId = (uiVisualTreeAsset != null) ? uiVisualTreeAsset.GetInstanceID() : -1;
//                string expectedUxmlPath = "N/A";
//                var assignedVta = (uiDocument != null) ? uiDocument.visualTreeAsset : null;
//                string assignedUxmlName = (assignedVta != null) ? assignedVta.name : "NULL";
//                int assignedUxmlId = (assignedVta != null) ? assignedVta.GetInstanceID() : -1;
//                string assignedUxmlPath = "N/A";
//#endif
//                if (expectedUxmlId != assignedUxmlId || expectedUxmlPath != assignedUxmlPath)
//                {
//                    Debug.LogError(
//                        $"[{(context != null ? context.GetType().Name : "NULL")}] rootVisualElement does not contain required element 'level_card_list'.\n" +
//                        $"Expected VisualTreeAsset: name='{expectedUxmlName}', id={expectedUxmlId}, path={expectedUxmlPath}\n" +
//                        $"Assigned VisualTreeAsset: name='{assignedUxmlName}', id={assignedUxmlId}, path={assignedUxmlPath}\n" +
//                        $"The assigned UXML may not be correct or the UI hierarchy is not built as expected."
//                    );
//                }
//                else
//                {
//                    Debug.LogWarning(
//                        $"[{(context != null ? context.GetType().Name : "NULL")}] rootVisualElement does not contain required element 'level_card_list', but the assigned VisualTreeAsset matches the expected one.\n" +
//                        $"VisualTreeAsset: name='{expectedUxmlName}', id={expectedUxmlId}, path={expectedUxmlPath}\n" +
//                        $"This likely means the UXML content is missing required elements, or there is a duplicate UIDocument, or script execution order/UI initialization is incorrect. Please check the UXML content and scene setup."
//                    );
//                }
//                return false;
//            }

            // Validate required named elements (name + type)
            if (config != null && config.NamedElements != null)
            {
                foreach (var kvp in config.NamedElements)
                {
                    var element = rootVisualElement.Q(kvp.Key);
                    if (element == null || (kvp.Value != null && !kvp.Value.IsInstanceOfType(element)))
                    {
                        Debug.LogError($"[{(context != null ? context.GetType().Name : "NULL")}] Required element '{kvp.Key}' of type '{(kvp.Value != null ? kvp.Value.Name : "NULL")}' not found.");
                        return false;
                    }
                }
            }

            // Validate required element names (any type)
            if (config != null && config.RequiredElementNames != null)
            {
                foreach (var name in config.RequiredElementNames)
                {
                    var element = rootVisualElement.Q(name);
                    if (element == null)
                    {
                        Debug.LogError($"[{(context != null ? context.GetType().Name : "NULL")}] Required element '{name}' not found.");
                        return false;
                    }
                }
            }

            // Validate required element types (at least one of each type)
            if (config != null && config.RequiredElementTypes != null)
            {
                foreach (var type in config.RequiredElementTypes)
                {
                    var element = rootVisualElement.Q(type.ToString());
                    if (element == null)
                    {
                        Debug.LogError($"[{(context != null ? context.GetType().Name : "NULL")}] Required element of type '{type.Name}' not found.");
                        return false;
                    }
                }
            }

            // ProgressBar logic (optional)
            progressBar = null;
            if (config != null && !string.IsNullOrEmpty(config.ProgressBarName))
            {
                progressBar = rootVisualElement.Q<ProgressBar>(config.ProgressBarName);
                if (progressBar == null)
                {
                    Debug.LogWarning($"[{(context != null ? context.GetType().Name : "NULL")}] ProgressBar '{config.ProgressBarName}' not found.");
                }
            }

            SetProgressBar(progressBar, 0f, "Validating UIDocument...");
            Debug.Log($"[{(context != null ? context.GetType().Name : "NULL")}] UIDocument is correctly set up with the expected root visual element.");

            ValidateOrFixLocale(context, showDevToast, out bool localeWasFixed);

#if UNITY_EDITOR
            if (showDevToast && rootVisualElement != null)
            {
                var toast = new Label("✅ UIDocument and locale verified");
                toast.style.position = Position.Absolute;
                toast.style.top = 10;
                toast.style.right = 10;
                toast.style.backgroundColor = new StyleColor(new Color(0, 0.5f, 0, 0.8f));
                toast.style.color = Color.white;
                toast.style.paddingLeft = 8;
                toast.style.paddingRight = 8;
                toast.style.paddingTop = 4;
                toast.style.paddingBottom = 4;
                toast.style.unityFontStyleAndWeight = FontStyle.Bold;
                rootVisualElement.Add(toast);
                if (context != null)
                    context.StartCoroutine(RemoveToastAfterDelay(toast, 2f));
            }
#endif
            return wasFixed || localeWasFixed;
        }

        private static void SetProgressBar(ProgressBar progressBar, float progress, string message)
        {
            if (progressBar != null)
            {
                progress = Mathf.Clamp01(progress);
                progressBar.value = progress * 100f;
                progressBar.title = message;
                progressBar.MarkDirtyRepaint(); // Force redraw
                Debug.Log($"[SetProgressBar] value={progressBar.value}, low={progressBar.lowValue}, high={progressBar.highValue}, title={progressBar.title}, hash={progressBar.GetHashCode()}, name={progressBar.name}");
            }
        }

#if UNITY_EDITOR
        private static IEnumerator RemoveToastAfterDelay(Label toast, float delay)
        {
            yield return new WaitForSeconds(delay);
            toast.RemoveFromHierarchy();
        }
#endif

        /// <summary>
        /// Validates and repairs locale state. Optionally shows a dev-only toast.
        /// </summary>
        public static void ValidateOrFixLocale(MonoBehaviour context, bool showDevToast, out bool wasFixed)
        {
            wasFixed = false;
            static string Timestamp() => DateTime.Now.ToString("HH:mm:ss.fff");
            string contextName = context != null ? context.GetType().Name : "NULL";
            string beforeLocale = LocalizationHelper.GetCurrentLocaleCode();

            // Step 1: Ensure locale is not null
            if (beforeLocale == null)
            {
                if (showDevToast)
                    Debug.LogWarning($"[{Timestamp()}][{contextName}] Current locale code is null. Setting default locale to 'en'.");
                LocalizationHelper.SetLocale("en");
                wasFixed = true;

                // Verify fix
                string afterLocale = LocalizationHelper.GetCurrentLocaleCode();
                if (afterLocale == null)
                {
                    Debug.LogError($"[{Timestamp()}][{contextName}] Failed to set locale to 'en'. Locale is still null after fix attempt.");
                }
                else if (afterLocale != "en")
                {
                    Debug.LogError($"[{Timestamp()}][{contextName}] Attempted to set locale to 'en', but current locale is '{afterLocale}'.");
                }
                else
                {
                    Debug.Log($"[{Timestamp()}][{contextName}] Locale successfully set to 'en'.");
                }
            }
            else
            {
                if (showDevToast)
                    Debug.Log($"[{Timestamp()}][{contextName}] Current locale code is: {beforeLocale}");
            }

            // Step 2: Ensure locale matches PlayerPrefs
            string currentLocale = LocalizationHelper.GetCurrentLocaleCode();
            string prefsLocale = PlayerPrefs.GetString("selected_locale", "en");
            if (currentLocale != prefsLocale)
            {
                if (showDevToast)
                    Debug.LogWarning($"[{Timestamp()}][{contextName}] Current locale ('{currentLocale}') does not match PlayerPrefs ('{prefsLocale}'). Setting locale to PlayerPrefs value.");
                LocalizationHelper.SetLocale(prefsLocale);
                PlayerPrefs.SetString("selected_locale", LocalizationHelper.GetCurrentLocaleCode());
                wasFixed = true;

                // Verify fix
                string afterLocale = LocalizationHelper.GetCurrentLocaleCode();
                if (afterLocale != prefsLocale)
                {
                    Debug.LogError($"[{Timestamp()}][{contextName}] Failed to set locale to PlayerPrefs value '{prefsLocale}'. Current locale is '{afterLocale}'.");
                }
                else
                {
                    Debug.Log($"[{Timestamp()}][{contextName}] Locale successfully set to PlayerPrefs value '{prefsLocale}'.");
                }
            }
            else
            {
                if (showDevToast)
                    Debug.Log($"[{Timestamp()}][{contextName}] Locale matches PlayerPrefs ('{prefsLocale}').");
            }
        }

        /// <summary>
        /// ValidateOrFixVisualElementLocale(levelCard, cardData.Config, out var localeValid, out var localeMsg);
        /// </summary>
        public static void ValidateOrFixVisualElementLocale(
            VisualElement element,
            UIDocumentValidationConfig config,
            out bool localeValid,
            out string localeMsg)
        {
            localeValid = true;
            localeMsg = "";
            if (element == null)
            {
                localeValid = false;
                localeMsg = "VisualElement is null.";
                Debug.LogError($"[UIDocumentValidator] {localeMsg}");
                return;
            }
            // Check for required named elements
            if (config != null && config.NamedElements != null)
            {
                foreach (var kvp in config.NamedElements)
                {
                    var child = element.Q(kvp.Key);
                    if (child == null || (kvp.Value != null && !kvp.Value.IsInstanceOfType(child)))
                    {
                        localeValid = false;
                        localeMsg = $"Required element '{kvp.Key}' of type '{(kvp.Value != null ? kvp.Value.Name : "NULL")}' not found.";
                        Debug.LogError($"[UIDocumentValidator] {localeMsg}");
                        return;
                    }
                }
            }
            // Custom validation logic
            if (config != null && config.CustomValidation != null)
            {
                config.CustomValidation.Invoke(element);
            }
            Debug.Log($"[UIDocumentValidator] VisualElement locale validation passed.");
        }

        /// <summary>
        /// Coroutine to show init.uxml as a loading screen, then swap to the final UI after validation.
        /// </summary>
        public static IEnumerator ValidateWithLoadingScreen(
            MonoBehaviour context,
            UIDocument uiDocument,
            VisualTreeAsset initUxml,
            VisualTreeAsset finalUxml,
            Action<bool> onComplete)
        {
            if (uiDocument == null || initUxml == null || finalUxml == null)
            {
                Debug.LogError("[UIDocumentValidator] Missing required arguments for loading screen validation.");
                onComplete?.Invoke(false);
                yield break;
            }

            uiDocument.visualTreeAsset = initUxml;
            yield return null; // Let UI update

            // Simulate a short delay for user feedback
            yield return new WaitForSeconds(0.2f);

            bool valid = ValidateOrFixUIDocument(context, ref uiDocument, finalUxml, out VisualElement root, out ProgressBar progressBar, true);

            // Swap to the final UI
            uiDocument.visualTreeAsset = finalUxml;
            yield return null;

            onComplete?.Invoke(valid);
        }

        /// <summary>
        /// Validates a VisualElement tree (not a UIDocument) against a UIDocumentValidationConfig.
        /// </summary>
        public static bool ValidateVisualElementTree(
            VisualElement rootVisualElement,
            UIDocumentValidationConfig config,
            out string message)
        {
            message = "";
            if (rootVisualElement == null)
            {
                message = "Root VisualElement is null.";
                Debug.LogError($"[UIDocumentValidator] {message}");
                return false;
            }

            // Log children for diagnostics
            Debug.Log($"[UIDocumentValidator] ValidateVisualElementTree: root type: {rootVisualElement.GetType().Name}, name: {rootVisualElement.name}, childCount: {rootVisualElement.childCount}");
            int childIdx = 0;
            foreach (var child in rootVisualElement.Children())
            {
                Debug.Log($"[UIDocumentValidator] root child[{childIdx}]: name='{child.name}', type={child.GetType().Name}");
                childIdx++;
            }

            // Validate required named elements (name + type)
            if (config != null && config.NamedElements != null)
            {
                foreach (var kvp in config.NamedElements)
                {
                    var element = rootVisualElement.Q(kvp.Key);
                    if (element == null || (kvp.Value != null && !kvp.Value.IsInstanceOfType(element)))
                    {
                        message = $"Required element '{kvp.Key}' of type '{(kvp.Value != null ? kvp.Value.Name : "NULL")}' not found.";
                        Debug.LogError($"[UIDocumentValidator] {message}");
                        return false;
                    }
                }
            }

            // Validate required element names (any type)
            if (config != null && config.RequiredElementNames != null)
            {
                foreach (var name in config.RequiredElementNames)
                {
                    var element = rootVisualElement.Q(name);
                    if (element == null)
                    {
                        message = $"Required element '{name}' not found.";
                        Debug.LogError($"[UIDocumentValidator] {message}");
                        return false;
                    }
                }
            }

            // Validate required element types (at least one of each type)
            if (config != null && config.RequiredElementTypes != null)
            {
                foreach (var type in config.RequiredElementTypes)
                {
                    bool found = false;
                    foreach (var el in rootVisualElement.Query<VisualElement>().ToList())
                    {
                        if (type.IsInstanceOfType(el))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        message = $"Required element of type '{type.Name}' not found.";
                        Debug.LogError($"[UIDocumentValidator] {message}");
                        return false;
                    }
                }
            }

            // Custom validation logic
            if (config != null && config.CustomValidation != null)
            {
                config.CustomValidation.Invoke(rootVisualElement);
            }

            message = "VisualElement tree is valid.";
            Debug.Log($"[UIDocumentValidator] {message}");
            return true;
        }
    }
}
