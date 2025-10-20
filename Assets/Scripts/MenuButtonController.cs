using UnityEngine;
using UnityEngine.UI;

public class MenuButtonController : MonoBehaviour
{
    public Button MainMenuButton; // Assign to this object
    public Button[] DropdownButtons; // Assign to this object

    private bool isVisible = false;

    private void Start()
    {
        // Initially hide dropdown buttons
        foreach (Button btn in DropdownButtons)
        {
            btn.gameObject.SetActive(false);
        }
        // Add listener to main menu button
        MainMenuButton.onClick.AddListener(ToggleMenu);
    }
    public void ToggleMenu()
    {
        isVisible = !isVisible;
        DropdownButtons[0].gameObject.SetActive(isVisible);
        DropdownButtons[1].gameObject.SetActive(isVisible);

    }
}