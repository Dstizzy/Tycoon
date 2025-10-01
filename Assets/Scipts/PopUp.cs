using TMPro;

using UnityEngine;
// REMOVE: using UnityEngine.UI; (It is deprecated and not needed for TMPro)

public class PopUp : MonoBehaviour {
    // Ensure this is properly assigned in the Inspector of the Prefab!
    [SerializeField] private TMP_Text textComponent;

    // REMOVE the public string text_value field.
    // The manager will directly call a setter function or property instead.

    // A public method to set the text immediately upon instantiation
    public void SetText(string value) {
        if (textComponent == null) {
            // Failsafe: Log an error if the component is still unassigned in the Inspector
            Debug.LogError("PopUp textComponent is not assigned in the Inspector!");
            return;
        }
        textComponent.text = value;
    }

    // REMOVE the Start() method entirely to eliminate the crash risk.
}