using TMPro;

using UnityEngine;

public class PopUp : MonoBehaviour {
    [SerializeField] private TMP_Text textComponent;


    public void SetText(string value) {
        if (textComponent == null) {
            Debug.LogError("PopUp textComponent is not assigned in the Inspector!");
            return;
        }
        textComponent.text = value;
    }

}