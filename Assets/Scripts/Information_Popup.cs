using TMPro;

using UnityEngine;

public class Information_Popup : MonoBehaviour
{
    [SerializeField] private TMP_Text aboutText;

    public void SetText(string info) {
        if (aboutText == null) {
            Debug.LogError("Information_Popup textComponent is not assigned in the Inspector!");
            return;
        }
        aboutText.text = info;
    }

}
