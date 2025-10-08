using TMPro;

using UnityEngine;

public class ButtonsPopUp : MonoBehaviour {
    [SerializeField] public TMP_Text buttonText;

    public void SetText(string text_1) {
        if (buttonText == null) {
            Debug.LogError("PopUp textComponent is not assigned in the Inspector!");
            return;
        }
        buttonText.text = text_1;
    }
}