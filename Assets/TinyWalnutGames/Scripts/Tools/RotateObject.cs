/*
 * This code is part of a Unity script that allows for rotating a GameObject using keyboard, mouse, or touch input.
 */
using UnityEngine;
using UnityEngine.EventSystems;

namespace TinyWalnutGames.Tools
{
    /// <summary>
    /// This class allows for rotating a GameObject using keyboard, mouse, or touch input.
    /// </summary>
    public class RotateObject : MonoBehaviour
	{
        /// <summary>
        /// The speed at which the object rotates.
        /// </summary>
        public float rotationSpeed = 100f;

        /// <summary>
        /// The mouse sensitivity multiplier for rotation.
        /// </summary>
        public float mouseSensitivity = 1f;

        /// <summary>
        /// The recorded start position of the touch or mouse input.
        /// </summary>
        private Vector2 startTouchPosition;

        /// <summary>
        /// The current position of the touch or mouse input.
        /// </summary>
        private Vector2 currentTouchPosition;

        /// <summary>
        /// Flag to indicate if the user is currently dragging the mouse or touch input.
        /// </summary>
        private bool isDragging = false;

        /// <summary>
        /// Called once per frame.
		/// Here we handle the input from keyboard, mouse, and touch.
        /// </summary>
        void Update()
		{
			HandleKeyboardInput();
			HandleTouchInput();
			HandleMouseInput();
		}

        /// <summary>
        /// Handles keyboard input for rotation.
        /// </summary>
        private void HandleKeyboardInput()
		{
            // get the horizontal input axis (A/D or Left/Right arrow keys)
            float horizontalInput = Input.GetAxis("Horizontal");

            // if the horizontal input is not zero, rotate the object
            if (horizontalInput != 0)
			{
				Rotate(horizontalInput);
			}
		}

        /// <summary>
        /// Handles touch input for rotation.
        /// </summary>
        private void HandleTouchInput()
		{
            // Check if there is a touch input
            if (Input.touchCount == 1)
			{
                // Get the first touch
                Touch touch = Input.GetTouch(0);

                // handle the touch phases
                switch (touch.phase)
				{
				case TouchPhase.Began:
					startTouchPosition = touch.position;
					isDragging = true;
					break;

				case TouchPhase.Moved:
					if (isDragging)
					{
						currentTouchPosition = touch.position;
						float deltaX = currentTouchPosition.x - startTouchPosition.x;
						Rotate(deltaX * Time.deltaTime);
						startTouchPosition = currentTouchPosition;
					}
					break;

				case TouchPhase.Ended:
				case TouchPhase.Canceled:
					isDragging = false;
					break;
				}
			}
		}

        /// <summary>
        /// Handles mouse input for rotation.
        /// </summary>
        private void HandleMouseInput()
		{
			// on mouse down
			if (Input.GetMouseButtonDown(0))
			{
				startTouchPosition = Input.mousePosition;
				isDragging = true;
			}

            // on mouse drag
            if (Input.GetMouseButton(0) && isDragging)
			{
				currentTouchPosition = Input.mousePosition;
				float deltaX = currentTouchPosition.x - startTouchPosition.x;
				Rotate(deltaX * mouseSensitivity * Time.deltaTime); // Apply mouse sensitivity here
				startTouchPosition = currentTouchPosition;
			}

            // on mouse up
            if (Input.GetMouseButtonUp(0))
			{
				isDragging = false;
			}
		}

        /// <summary>
        /// Rotates the object based on the input amount.
        /// </summary>
        /// <param name="amount">The amount to rotate.</param>
        private void Rotate(float amount)
		{
			transform.Rotate(Vector3.up, -(amount * rotationSpeed * Time.deltaTime));
		}
	}
}