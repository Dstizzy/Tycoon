using UnityEngine;
using UnityEngine.UI;

public class MenuButtonController : MonoBehaviour
{
    public Button[] DropdownButtons; // Assign to this object

    private bool isVisible = false;

    public void ToggleMenu()
    {
        isVisible = !isVisible;
        DropdownButtons[0].gameObject.SetActive(isVisible);
        DropdownButtons[1].gameObject.SetActive(isVisible);

    }
}