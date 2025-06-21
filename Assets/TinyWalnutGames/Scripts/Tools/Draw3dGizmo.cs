using UnityEngine;
using UnityEditor;

namespace TinyWalnutGames.Tools
{
    [ExecuteInEditMode]
    public class Draw3dGizmo : MonoBehaviour
    {
        [Tooltip("The color of the gizmo.")]
        public Color gizmoColor = Color.red;
        [Tooltip("The size of the gizmo.")]
        public float gizmoSize = 0.5f;
        [Tooltip("The type of gizmo to draw.")]
        public GizmoType gizmoType = GizmoType.Sphere;
        [Tooltip("The type of gizmo to draw.")]

        /// The type of gizmo to draw.
        [SerializeField]
        public enum GizmoType
        {
            Sphere,
            Cube,
            Arrow
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                DrawGizmo();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
            {
                DrawGizmo();
            }
        }

        private void DrawGizmo()
        {
            Color prevColor = Gizmos.color;
            Gizmos.color = gizmoColor;

            switch (gizmoType)
            {
                case GizmoType.Sphere:
                    Gizmos.DrawSphere(transform.position, gizmoSize);
                    break;
                case GizmoType.Cube:
                    Gizmos.DrawCube(transform.position, Vector3.one * gizmoSize);
                    break;
                case GizmoType.Arrow:
                    // Draw an arrow in the forward direction
                    Vector3 start = transform.position;
                    Vector3 end = start + transform.forward * gizmoSize;
                    Gizmos.DrawLine(start, end);

                    // Draw arrowhead
                    float headSize = gizmoSize * 0.2f;
                    Vector3 right = Quaternion.LookRotation(transform.forward) * Quaternion.Euler(0, 150, 0) * Vector3.forward;
                    Vector3 left = Quaternion.LookRotation(transform.forward) * Quaternion.Euler(0, -150, 0) * Vector3.forward;
                    Gizmos.DrawRay(end, right * headSize);
                    Gizmos.DrawRay(end, left * headSize);
                    break;
            }

            Gizmos.color = prevColor;
        }
    }
}