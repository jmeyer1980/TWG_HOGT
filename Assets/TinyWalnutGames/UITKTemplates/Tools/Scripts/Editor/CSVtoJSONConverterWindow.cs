#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace TinyWalnutGames.UITKTemplates.Tools.Editor
{
    public class CSVtoJSONConverterWindow : EditorWindow
    {
        private readonly List<TextAsset> csvAssets = new();
        private string outputFolder = "Assets/Resources";

        [MenuItem("Tools/CSV to JSON Converter")]
        public static void ShowWindow()
        {
            GetWindow<CSVtoJSONConverterWindow>("CSV to JSON Converter");
        }

        private void OnGUI()
        {
            GUILayout.Label("CSV to JSON Converter", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Drag CSV files from the Project window below:");

            // Draw drag-and-drop area
            Rect dropArea = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drag & Drop CSV files here", EditorStyles.helpBox);

            // Handle drag-and-drop events
            Event evt = Event.current;
            if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
            {
                if (dropArea.Contains(evt.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        foreach (Object draggedObject in DragAndDrop.objectReferences)
                        {
                            if (draggedObject is TextAsset textAsset)
                            {
                                string assetPath = AssetDatabase.GetAssetPath(textAsset);
                                if (assetPath.EndsWith(".csv", System.StringComparison.OrdinalIgnoreCase) && !csvAssets.Contains(textAsset))
                                {
                                    csvAssets.Add(textAsset);
                                }
                            }
                        }
                    }
                    evt.Use();
                }
            }

            // Draw array of object fields for CSV files
            int removeIndex = -1;
            for (int i = 0; i < csvAssets.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                csvAssets[i] = (TextAsset)EditorGUILayout.ObjectField(csvAssets[i], typeof(TextAsset), false);

                // Remove button
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    removeIndex = i;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (removeIndex >= 0)
                csvAssets.RemoveAt(removeIndex);

            GUILayout.Space(10);

            GUILayout.Label("Output Folder (relative to Assets):");
            outputFolder = EditorGUILayout.TextField(outputFolder);

            GUILayout.Space(10);

            if (GUILayout.Button("Convert to JSON"))
            {
                foreach (var asset in csvAssets)
                {
                    if (asset == null) continue;
                    string assetPath = AssetDatabase.GetAssetPath(asset);
                    if (!assetPath.EndsWith(".csv", System.StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.LogWarning($"Skipped non-CSV asset: {assetPath}");
                        continue;
                    }
                    string fullPath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
                    string json = CSVtoJSONConverter.ConvertCSVtoJSON(fullPath);
                    string fileName = Path.GetFileNameWithoutExtension(assetPath) + ".json";
                    string outputPath = Path.Combine(outputFolder, fileName);

                    // Ensure output folder exists
                    string fullOutputPath = Path.Combine(Application.dataPath, outputFolder.Replace("Assets/", ""));
                    if (!Directory.Exists(fullOutputPath))
                        Directory.CreateDirectory(fullOutputPath);

                    File.WriteAllText(Path.Combine(fullOutputPath, fileName), json);
                    Debug.Log($"Converted {assetPath} to {outputPath}");
                }
                AssetDatabase.Refresh();
            }

            if (GUILayout.Button("Clear Selection"))
            {
                csvAssets.Clear();
            }
        }

        // Optional: Context menu for right-click in Project window
        [MenuItem("Assets/Convert CSV to JSON", true)]
        private static bool ValidateConvertCSV()
        {
            foreach (var obj in Selection.objects)
            {
                string assetPath = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(assetPath) && assetPath.EndsWith(".csv", System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        [MenuItem("Assets/Convert CSV to JSON")]
        private static void ConvertCSVContext()
        {
            foreach (var obj in Selection.objects)
            {
                string assetPath = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(assetPath) && assetPath.EndsWith(".csv", System.StringComparison.OrdinalIgnoreCase))
                {
                    string fullPath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
                    string json = CSVtoJSONConverter.ConvertCSVtoJSON(fullPath);
                    string outputFolder = Path.GetDirectoryName(assetPath);
                    string fileName = Path.GetFileNameWithoutExtension(assetPath) + ".json";
                    string outputPath = Path.Combine(outputFolder, fileName);
                    File.WriteAllText(outputPath, json);
                    Debug.Log($"Converted {assetPath} to {outputPath}");
                }
            }
            AssetDatabase.Refresh();
        }
    }
}
#endif