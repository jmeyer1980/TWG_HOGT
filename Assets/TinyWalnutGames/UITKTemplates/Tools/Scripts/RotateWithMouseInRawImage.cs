/*
 * This code is part of a Unity script that allows for rotating a camera around a target object using mouse input within a RawImage.
 */
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TinyWalnutGames.UITKTemplates.Tools
{
    /// <summary>
    /// This class is used with a raw image and will allow you to rotate the view of an object through that raw image,
    /// meaning you can click and drag your mouse within the image and the camera will rotate around the object being viewed.
    /// </summary>
    public class RotateWithMouseInRawImage : MonoBehaviour
    {
        /// <summary>
        /// The raw image component that will be used for the rotation.
        /// </summary>
        public RawImage characterViewer;

        /// <summary>
        /// The speed at which the camera rotates around the target.
        /// </summary>
        public float rotationSpeed = 5f;

        /// <summary>
        /// Whether to use the player tag to find the orbit target.
        /// </summary>
        public bool usePlayerTag = true;

        /// <summary>
        /// The target object that the camera will orbit around.
        /// </summary>
		public Transform orbitTarget;

        /// <summary>
        /// The target object that the camera will look at.
        /// </summary>
        public Transform lookAtTarget;

        /// <summary>
        /// The distance from the orbit target.
        /// </summary>
        public float distance = 5f;

        /// <summary>
        /// The height of the camera above the orbit target.
        /// </summary>
        public float height = 2f;

        /// <summary>
        /// Flag to indicate if the user is currently dragging the mouse.
        /// </summary>
        private bool isDragging = false;

        /// <summary>
        /// The previously selected GameObject before the drag started.
        /// </summary>
        private GameObject previouslySelected;

        /// <summary>
        /// The offset from the orbit target to the camera position.
        /// </summary>
        private Vector3 offset;

        /// <summary>
        /// Initializes the camera position and rotation based on the orbit target and look at target.
        /// </summary>
        void Start()
        {
            // Check if the orbit target and look at target are assigned
            if (usePlayerTag)
            {
                // Find the player GameObject by tag
                GameObject player = GameObject.FindGameObjectWithTag("Player");

                // If the player is found, assign it to orbitTarget
                if (player != null)
                {
                    orbitTarget = player.transform;

                    // Set lookAtTarget to the second spine object if available
                    Transform secondSpine = player.transform.Find("SecondSpine");
                    if (secondSpine != null)
                    {
                        lookAtTarget = secondSpine;
                    }
                    else
                    {
                        // Otherwise, set lookAtTarget to a point 1.5 units up the Y axis from the orbitTarget
                        GameObject lookAtPoint = new("LookAtPoint");
                        lookAtPoint.transform.position = orbitTarget.position + new Vector3(0, 1.5f, 0);
                        lookAtTarget = lookAtPoint.transform;
                    }
                }
                else
                {
                    Debug.LogWarning("Player not found. Please ensure there is a GameObject with the 'Player' tag.");
                }
            }
            else
            {
                if (orbitTarget == null || lookAtTarget == null)
                {
                    Debug.LogWarning("Orbit Target or Look At Target not assigned.");
                }
            }

            // Calculate the initial offset from the target
            offset = new Vector3(0, height, -distance);
            SnapToTarget();
        }

        /// <summary>
        /// Handles mouse input to rotate the camera around the target.
        /// </summary>
        void Update()
        {
            // Check if the character viewer is assigned
            if (Input.GetMouseButtonDown(0))
            {
                // Convert the mouse position to local coordinates within the character viewer
                Vector2 localMousePosition = characterViewer.rectTransform.InverseTransformPoint(Input.mousePosition);

                // Check if the mouse position is within the bounds of the character viewer
                if (characterViewer.rectTransform.rect.Contains(localMousePosition))
                {
                    isDragging = true;
                    previouslySelected = EventSystem.current.currentSelectedGameObject;
                    EventSystem.current.SetSelectedGameObject(null);
                }
            }

            // Handle mouse drag to rotate the camera
            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                EventSystem.current.SetSelectedGameObject(previouslySelected);
            }

            // Handle mouse drag to rotate the camera
            if (isDragging)
            {
                float rotationX = Input.GetAxis("Mouse X") * rotationSpeed;
                offset = Quaternion.Euler(0, rotationX, 0) * offset;
                Vector3 desiredPosition = orbitTarget.position + offset;
                transform.position = desiredPosition;
                transform.LookAt(lookAtTarget);
            }
        }

        /// <summary>
        /// Snaps the camera to the target's position and sets its rotation.
        /// </summary>
        private void SnapToTarget()
        {
            Vector3 desiredPosition = orbitTarget.position + offset;
            transform.position = desiredPosition;
            transform.LookAt(lookAtTarget);
        }
    }
}
