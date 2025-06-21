using UnityEditor;
using UnityEngine;

namespace TinyWalnutGames.Tools
{
    [CustomEditor(typeof(CycleActiveChildren))]
    public class CycleActiveChildrenEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            CycleActiveChildren cycler = (CycleActiveChildren)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Cycle Controls", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Previous"))
            {
                cycler.PreviousChild();
            }
            if (GUILayout.Button("Next"))
            {
                cycler.NextChild();
            }
            EditorGUILayout.EndHorizontal();

            int count = cycler.GetChildCount();
            if (count > 0)
            {
                EditorGUILayout.LabelField($"Current: {cycler.GetCurrentIndex() + 1} / {count}");
            }
            else
            {
                EditorGUILayout.LabelField("No children found.");
            }
        }
    }
}
