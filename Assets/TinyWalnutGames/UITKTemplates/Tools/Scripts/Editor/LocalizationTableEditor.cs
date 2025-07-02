using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace TinyWalnutGames.UITKTemplates.Tools.Editor
{
    /// <summary>
    /// Editor window for converting XLIFF files to ScriptableObject localization tables.
    /// </summary>
    public class LocalizationTableEditorWindow : EditorWindow
    {
        private List<Object> droppedFiles = new();
        private Vector2 scroll;
        [SerializeField] private string statusMessage = "";

        // Set the target directory for locale assets
        public string LocaleAssetFolder;

        [MenuItem("Tools/Localization/XLIFF to Scriptable Table Converter")]
        public static void ShowWindow()
        {
            GetWindow<LocalizationTableEditorWindow>("XLIFF Table Converter");
        }

        void OnEnable()
        {
            // Ensure droppedFiles is not null after domain reloads
            droppedFiles ??= new();
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("XLIFF to Scriptable Table Converter", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // field for entering the asset folder path
            LocaleAssetFolder = EditorGUILayout.TextField("Locale Asset Folder", LocaleAssetFolder);
            if (!Directory.Exists(LocaleAssetFolder))
            {
                EditorGUILayout.HelpBox("The specified folder does not exist. Please create it or check the path.", MessageType.Warning);
            }
            EditorGUILayout.Space();

            // Drag-and-drop area
            Rect dropArea = GUILayoutUtility.GetRect(0, 60, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drag & Drop XLIFF files here", EditorStyles.helpBox);

            HandleDragAndDrop(dropArea);

            EditorGUILayout.Space();

            // List of added files
            EditorGUILayout.LabelField("Files to Convert:", EditorStyles.boldLabel);
            if (droppedFiles != null && droppedFiles.Count > 0)
            {
                scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(80));
                for (int i = 0; i < droppedFiles.Count; i++)
                {
                    EditorGUILayout.ObjectField(droppedFiles[i], typeof(TextAsset), false);
                }
                EditorGUILayout.EndScrollView();

                EditorGUILayout.Space();

                // Button to trigger conversion
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Convert XLIFF Files"))
                {
                    // Always create separate tables for each file
                    CreateTablesForEachXLIFF();
                }

                // Optionally, a button to clear the list
                if (GUILayout.Button("Clear List"))
                {
                    droppedFiles.Clear();
                    statusMessage = "";
                    Repaint();
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("Drop one or more XLIFF files above to add them to the list.", MessageType.Info);
            }

            if (!string.IsNullOrEmpty(statusMessage))
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(statusMessage, MessageType.None);
            }
        }

        /// <summary>
        /// Handles drag-and-drop logic and updates the droppedFiles list.
        /// </summary>
        private void HandleDragAndDrop(Rect dropArea)
        {
            Event evt = Event.current;
            if ((evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform) && dropArea.Contains(evt.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    bool added = false;
                    foreach (var obj in DragAndDrop.objectReferences)
                    {
                        string path = AssetDatabase.GetAssetPath(obj);
                        string ext = Path.GetExtension(path).ToLowerInvariant();
                        if ((ext == ".xlf" || ext == ".xliff") && !droppedFiles.Contains(obj))
                        {
                            droppedFiles.Add(obj);
                            added = true;
                        }
                    }
                    evt.Use();
                    if (added)
                    {
                        statusMessage = $"Added {DragAndDrop.objectReferences.Length} file(s).";
                        Repaint();
                    }
                    else
                    {
                        statusMessage = "No new valid XLIFF files were added.";
                        Repaint();
                    }
                }
            }
        }

        private void CreateTablesForEachXLIFF()
        {
            statusMessage = "";
            Directory.CreateDirectory(LocaleAssetFolder);
            foreach (var obj in droppedFiles)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (File.Exists(path))
                {
                    string xliffText = File.ReadAllText(path);
                    var table = ScriptableObject.CreateInstance<LocalizationTable>();
                    LoadXLIFFAuto(table, xliffText, path);

                    // Extract base name and locale code from filename
                    string fileName = Path.GetFileNameWithoutExtension(path);
                    string baseName = fileName;
                    string localeCode = "en"; // default fallback

                    // Try to extract locale code from filename, e.g., HOGT_UI_en.xlf
                    int lastUnderscore = fileName.LastIndexOf('_');
                    if (lastUnderscore > 0 && lastUnderscore < fileName.Length - 1)
                    {
                        baseName = fileName[..lastUnderscore];
                        localeCode = fileName[(lastUnderscore + 1)..];
                    }

                    string assetName = $"{baseName}_{localeCode}";
                    string assetPath = $"{LocaleAssetFolder}/{assetName}.asset";
                    AssetDatabase.CreateAsset(table, assetPath);
                    statusMessage += $"Created: {assetPath}\n";
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Repaint();
        }

        private void LoadXLIFFAuto(LocalizationTable table, string xliffText, string filename)
        {
            if (xliffText.Contains("version=\"2.0\""))
                table.LoadFromXLIFF2(xliffText, filename);
            else
                table.LoadFromXLIFF(xliffText);
        }
    }
}