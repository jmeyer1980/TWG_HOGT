using UnityEditorInternal;
using UnityEditor;
using UnityEngine;
using TinyWalnutGames.UITKTemplates.HOGT;

[CustomEditor(typeof(LevelUIController))]
public class LevelUIControllerEditor : Editor
{
    private SerializedProperty levelDataProp;
    private SerializedObject levelDataSO;
    private ReorderableList objectsToFindList;

    private void OnEnable()
    {
        levelDataProp = serializedObject.FindProperty("levelData");
        UpdateObjectsToFindList();
    }

    private void UpdateObjectsToFindList()
    {
        var controller = (LevelUIController)target;
        if (controller.LevelData != null)
        {
            levelDataSO = new SerializedObject(controller.LevelData);
            var objectsToFindProp = levelDataSO.FindProperty("objectsToFind");

            // Debug log to help with troubleshooting
            Debug.Log($"UpdateObjectsToFindList called. objectsToFindProp is {(objectsToFindProp != null ? "not null" : "null")}");

            objectsToFindList = new ReorderableList(levelDataSO, objectsToFindProp, true, true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Hidden Objects List (objectsToFind)"),
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    var element = objectsToFindProp.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(rect, element, GUIContent.none);
                }
            };
        }
        else
        {
            levelDataSO = null;
            objectsToFindList = null;
        }
    }

    public override void OnInspectorGUI()
    {
        // Always draw the default inspector (helps in debug mode)
        DrawDefaultInspector();

        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(levelDataProp);
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            UpdateObjectsToFindList();
        }

        var controller = (LevelUIController)target;

        if (controller.LevelData == null)
        {
            EditorGUILayout.HelpBox("Assign a LevelData asset to see its objectsToFind list.", MessageType.Info);
        }
        else if (objectsToFindList != null && levelDataSO != null)
        {
            levelDataSO.Update();
            objectsToFindList.DoLayoutList();
            levelDataSO.ApplyModifiedProperties();
        }

#if UNITY_EDITOR
        // Add a button to save hidden object positions from UI in edit mode
        if (!Application.isPlaying)
        {
            if (GUILayout.Button("Save Hidden Object Positions From UI"))
            {
                ((LevelUIController)target).SaveHiddenObjectPositionsFromUI();
            }
        }
#endif

        serializedObject.ApplyModifiedProperties();
    }
}
