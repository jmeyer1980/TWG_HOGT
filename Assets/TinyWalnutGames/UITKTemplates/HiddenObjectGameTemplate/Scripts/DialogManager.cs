using UnityEngine;
using UnityEngine.UI;

namespace TinyWalnutGames.UITKTemplates.HOGT
{
    // Manages dialog UI (can be adapted for UI Toolkit)
    public class DialogManager : MonoBehaviour
    {
        public GameObject dialogPanel;
        public Text dialogText;

        // Display a dialog with provided text
        public void ShowDialog(string message)
        {
            dialogPanel.SetActive(true);
            dialogText.text = message;
        }

        // Hide the dialog panel
        public void HideDialog()
        {
            dialogPanel.SetActive(false);
        }
    }
}
