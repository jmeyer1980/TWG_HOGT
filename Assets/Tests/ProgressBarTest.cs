using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

public class ProgressBarTest : MonoBehaviour
{
    public UIDocument doc;

    void Start()
    {
        var root = doc != null ? doc.rootVisualElement : null;
        var bar = root?.Q<ProgressBar>("progressbar");
        if (bar != null)
        {
            bar.lowValue = 0;
            bar.highValue = 10;
            bar.value = 7;
            bar.title = "Should be 70%";
            bar.MarkDirtyRepaint();
            Debug.Log($"[ProgressBarTest] Minimal test: Set progress bar to 7/10 (low={bar.lowValue}, high={bar.highValue}, value={bar.value}, name={bar.name}, hash={bar.GetHashCode()})");
        }
        else
        {
            Debug.LogError("[ProgressBarTest] Minimal test: ProgressBar not found");
        }
    }

    [UnityTest]
    public IEnumerator ProgressBar_Fills_WhenValueSet()
    {
        // Create a GameObject and add UIDocument
        var go = new GameObject("TestDoc");
        var doc = go.AddComponent<UIDocument>();

        // Create a root VisualElement and ProgressBar
        var root = new VisualElement();
        ProgressBar bar = new()
        {
            name = "progressbar"
        };
        root.Add(bar);

        // Assign root to UIDocument
        doc.panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        doc.visualTreeAsset = null; // Not using UXML for this test
        doc.rootVisualElement.Add(root);

        // Wait a frame for UI to initialize
        yield return null;

        // Set ProgressBar values
        bar.lowValue = 0;
        bar.highValue = 10;
        bar.value = 7;
        bar.title = "Should be 70%";
        bar.MarkDirtyRepaint();

        // Assert value is set correctly
        Assert.AreEqual(7, bar.value);
        Assert.AreEqual(0, bar.lowValue);
        Assert.AreEqual(10, bar.highValue);

        // Optionally, check the title
        Assert.AreEqual("Should be 70%", bar.title);

        // Clean up
        Object.DestroyImmediate(go);
    }
}
