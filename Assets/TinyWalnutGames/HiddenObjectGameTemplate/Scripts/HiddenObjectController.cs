using UnityEngine;
using UnityEngine.EventSystems;

namespace TinyWalnutGames.HOGT
{
    // Attach to each hidden object for click/drag logic
    public class HiddenObjectController : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private Animator animator;

        void Start()
        {
            animator = GetComponent<Animator>();
        }

        // For pixel hunting: detect clicks on the object
        public void OnPointerClick(PointerEventData eventData)
        {
            // TODO: Check if clicked area is correct, then trigger found event
            Debug.Log("Object clicked!");
            // reveal the object, but avoid null propogation just in case it is a game object and not a visual element
            if (animator != null)
            {
                animator.SetTrigger("Found");
            }
            else
            {
                Debug.LogWarning("Animator not found on HiddenObjectController.");
            }
            // Todo: implement a dialog manager 
            // possibly call DialogManager to show a message.
            // This could be a reaction for finding secret hidden objects
        }

        // For drag and drop: start dragging
        public void OnBeginDrag(PointerEventData eventData)
        {
            Debug.Log("Drag started");
            // TODO: Add any highlight or effect
        }

        public void OnDrag(PointerEventData eventData)
        {
            // Optional: implement draggable behavior
            transform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Debug.Log("Drag ended");
            // TODO: Snap back object or finalize the drop placement
        }
    }
}
