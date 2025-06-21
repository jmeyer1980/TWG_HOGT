/*
 * This code is part of a Unity script that controls the camera to orbit around and look at a target.
 */
using UnityEngine;

namespace TinyWalnutGames.Tools
{
	/// <summary>
	/// This class controls the camera to orbit around and look at a target
	/// </summary>
	public class CameraController : MonoBehaviour
	{
        /// <summary>
        /// The speed at which the camera rotates around the target.
        /// </summary>
        public float rotationSpeed = 5.0f;

        /// <summary>
        /// The speed at which the camera zooms in and out.
        /// </summary>
        public float zoomSpeed = 2.0f;

        /// <summary>
        /// The minimum distance the camera can zoom in to the target.
        /// </summary>
        public float minZoomDistance = 2.0f;

        /// <summary>
        /// The maximum distance the camera can zoom out from the target.
        /// </summary>
        public float maxZoomDistance = 10.0f;

        /// <summary>
        /// The height offset of the camera from the target.
        /// </summary>
        public float heightOffset = 2.0f;

        /// <summary>
        /// The target the camera orbits around.
        /// </summary>
        private Transform orbitTarget;

        /// <summary>
        /// The target the camera looks at.
        /// </summary>
        private Transform lookAtTarget;

        /// <summary>
        /// The current distance of the camera from the target.
        /// </summary>
        private float currentZoomDistance;

        /// <summary>
        /// The current rotation of the camera around the X-axis.
        /// </summary>
        private float currentRotationX = 0.0f;

        /// <summary>
        /// The current rotation of the camera around the Y-axis.
        /// </summary>
        private float currentRotationY = 0.0f;

        /// <summary>
        /// The last recorded mouse position.
        /// </summary>
        private Vector3 lastMousePosition;

        /// <summary>
        /// Start is called before the first frame update
        /// Here we find the player GameObject and assign it to orbitTarget and lookAtTarget.
        /// </summary>
        void Start()
		{
			// Find the player GameObject
			GameObject player = GameObject.FindGameObjectWithTag("Player");
			if (player != null)
			{
				// Assign the player to orbitTarget if not already assigned
				if (orbitTarget == null)
				{
					orbitTarget = player.transform;
				}

				// Assign lookAtTarget if not already assigned
				if (lookAtTarget == null)
				{
					// Find the child object named "target" within the player object
					Transform lookAtPoint = player.transform.Find("target");
					if (lookAtPoint != null)
					{
						lookAtTarget = lookAtPoint;
					}
					else
					{
						Debug.LogWarning("Child object 'target' not found in player object.");
					}
				}
			}
			else
			{
				Debug.LogWarning("Player not found. Please ensure there is a GameObject with the 'Player' tag.");
			}

			// Initialize the current zoom distance based on the initial position
			currentZoomDistance = Vector3.Distance(transform.position, orbitTarget.position);
			// Snap the camera to the target's position
			SnapToTarget();
		}

        /// <summary>
        /// Update is called once per frame
        /// Here we handle the input from keyboard, mouse, and touch.
        /// </summary>
        void Update()
		{
			// Rotate the camera based on input keys
			if (Input.GetKey(KeyCode.W))
			{
				currentRotationY += rotationSpeed * Time.deltaTime;
			}
			if (Input.GetKey(KeyCode.S))
			{
				currentRotationY -= rotationSpeed * Time.deltaTime;
			}
			if (Input.GetKey(KeyCode.A))
			{
				currentRotationX -= rotationSpeed * Time.deltaTime;
			}
			if (Input.GetKey(KeyCode.D))
			{
				currentRotationX += rotationSpeed * Time.deltaTime;
			}

			// Rotate the camera based on mouse drag
			if (Input.GetMouseButtonDown(0))
			{
				lastMousePosition = Input.mousePosition;
			}
			if (Input.GetMouseButton(0))
			{
				Vector3 delta = Input.mousePosition - lastMousePosition;
				currentRotationX += delta.x * rotationSpeed * Time.deltaTime;
				currentRotationY -= delta.y * rotationSpeed * Time.deltaTime;
				lastMousePosition = Input.mousePosition;
			}

			// Zoom the camera based on mouse scroll wheel input
			float scroll = Input.GetAxis("Mouse ScrollWheel");
			currentZoomDistance -= scroll * zoomSpeed;
			currentZoomDistance = Mathf.Clamp(currentZoomDistance, minZoomDistance, maxZoomDistance);

			// Calculate the new position and rotation of the camera
			Quaternion rotation = Quaternion.Euler(currentRotationY, currentRotationX, 0);
			Vector3 offset = new(0, heightOffset, -currentZoomDistance);
			transform.position = orbitTarget.position + rotation * offset;
			// Make the camera look at the target
			transform.LookAt(lookAtTarget);
		}

		/// <summary>
		/// Method to snap the camera to the target's position
		/// </summary>
		private void SnapToTarget()
		{
			Quaternion rotation = Quaternion.Euler(currentRotationY, currentRotationX, 0);
			Vector3 offset = new(0, heightOffset, -currentZoomDistance);
			transform.position = orbitTarget.position + rotation * offset;
			transform.LookAt(lookAtTarget);
		}
	}
}
