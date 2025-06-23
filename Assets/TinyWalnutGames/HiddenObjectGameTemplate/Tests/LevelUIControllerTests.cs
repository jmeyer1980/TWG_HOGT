using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using TinyWalnutGames.HOGT;
using UnityEngine.TestTools; // Add this for LogAssert

public class LevelUIControllerTests
{
    private LevelUIController controller;
    private LevelData testLevelData;
    private UIDocument testUIDocument;
    private VisualElement root;
    private Label levelNameLabel;
    private VisualElement backgroundImage;
    private VisualElement objectsList;

    [SetUp]
    public void SetUp()
    {
        // Create test objects
        var go = new GameObject();

        // Setup UI Document and elements BEFORE adding LevelUIController
        testUIDocument = go.AddComponent<UIDocument>();
        root = new VisualElement();
        testUIDocument.panelSettings = ScriptableObject.CreateInstance<PanelSettings>(); // Prevents null ref in some Unity versions

        // Add UI elements to root
        levelNameLabel = new()
        {
            name = "levelNameLabel"
        };
        backgroundImage = new()
        {
            name = "backgroundImage"
        };
        objectsList = new()
        {
            name = "objectsList"
        };
        root.Add(levelNameLabel);
        root.Add(backgroundImage);
        root.Add(objectsList);

        // Assign root to UIDocument
        testUIDocument.visualTreeAsset = null; // Not using a VisualTreeAsset in tests
        testUIDocument.rootVisualElement.Clear();
        testUIDocument.rootVisualElement.Add(levelNameLabel);
        testUIDocument.rootVisualElement.Add(backgroundImage);
        testUIDocument.rootVisualElement.Add(objectsList);

        // Create and assign LevelData
        testLevelData = ScriptableObject.CreateInstance<LevelData>();
        testLevelData.levelName = "Test Level";
        testLevelData.levelBackground = Texture2D.blackTexture;
        testLevelData.objectsToFind = new System.Collections.Generic.List<HiddenObjectData>();

        var obj1 = ScriptableObject.CreateInstance<HiddenObjectData>();
        obj1.objectName = "Object 1";
        testLevelData.objectsToFind.Add(obj1);

        var obj2 = ScriptableObject.CreateInstance<HiddenObjectData>();
        obj2.objectName = "Object 2";
        testLevelData.objectsToFind.Add(obj2);

        // Add controller after dependencies are ready
        controller = go.AddComponent<LevelUIController>();

        // Inject dependencies BEFORE Awake is called
        controller.InjectDependencies(testLevelData, testUIDocument);

        // Manually call Awake if needed (Unity calls it automatically in play mode, but not always in tests)
        // LogAssert.ignoreFailingMessages = true; // Uncomment if you want to suppress error logs

        // Initialize UI references
        controller.InitializeUIReferences();
    }

    [Test]
    public void SetLevelName_SetsLabelText()
    {
        controller.SetLevelName();
        Assert.AreEqual("Test Level", levelNameLabel.text);
    }

    [Test]
    public void SetBackgroundImage_SetsBackgroundImage()
    {
        controller.SetBackgroundImage();
        Assert.AreEqual(new StyleBackground(Texture2D.blackTexture), backgroundImage.style.backgroundImage);
    }

    [Test]
    public void PopulateObjectsList_AddsCorrectLabels()
    {
        controller.PopulateObjectsList();
        Assert.AreEqual(2, objectsList.childCount);
        Assert.AreEqual("Object 1", (objectsList[0] as Label).text);
        Assert.AreEqual("Object 2", (objectsList[1] as Label).text);
    }
}
