using UnityEngine;

namespace TinyWalnutGames.Tools
{
    [ExecuteAlways]
    public class CycleActiveChildren : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        private int currentIndex = 0;

        private void OnEnable()
        {
            UpdateChildren();
            SetActiveChild(currentIndex);
        }

        private void OnTransformChildrenChanged()
        {
            UpdateChildren();
            SetActiveChild(currentIndex);
        }

        private void UpdateChildren()
        {
            // Clamp currentIndex if children count changes
            int childCount = transform.childCount;
            if (childCount == 0)
            {
                currentIndex = 0;
                return;
            }
            if (currentIndex >= childCount)
                currentIndex = 0;
        }

        public void NextChild()
        {
            int childCount = transform.childCount;
            if (childCount == 0) return;
            currentIndex = (currentIndex + 1) % childCount;
            SetActiveChild(currentIndex);
        }

        public void PreviousChild()
        {
            int childCount = transform.childCount;
            if (childCount == 0) return;
            currentIndex = (currentIndex - 1 + childCount) % childCount;
            SetActiveChild(currentIndex);
        }

        public void SetActiveChild(int index)
        {
            int childCount = transform.childCount;
            if (childCount == 0) return;
            for (int i = 0; i < childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(i == index);
            }
        }

        public int GetCurrentIndex()
        {
            return currentIndex;
        }

        public int GetChildCount()
        {
            return transform.childCount;
        }
    }
}
