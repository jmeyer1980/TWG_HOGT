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
            animator?.SetTrigger("Reveal");
            // Possibly call DialogManager to show a message
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
