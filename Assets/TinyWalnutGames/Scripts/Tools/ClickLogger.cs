using UnityEngine;

namespace TinyWalnutGames.Tools
{
    public class ClickLogger : MonoBehaviour
    {
        // this is a simple script to log button clicks in the console.
        // it is used to test the button click events in the UI.
        // it is attached to the button prefab in the scene.
        public void OnClick()
        {
            Debug.Log("Button clicked: " + gameObject.name);
        }
    }
}