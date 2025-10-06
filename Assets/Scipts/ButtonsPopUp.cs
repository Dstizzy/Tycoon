using TMPro;

using UnityEngine;

public class ButtonsPopUp : MonoBehaviour {
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private TMP_Text buttonText_2;
    [SerializeField] private TMP_Text buttonText_3;


    public void SetText(string text_1,string text_2, string text_3) {
        if (buttonText == null && buttonText_2 == null && buttonText_3) {
            Debug.LogError("PopUp textComponent is not assigned in the Inspector!");
            return;
        }
        buttonText.text = text_1;
        buttonText_2.text = text_2;
        buttonText_3.text = text_3;
    }

}