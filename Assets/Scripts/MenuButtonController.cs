using UnityEngine;

public class MenuButtonController : MonoBehaviour
{
    public GameObject ButtonPanel; // Assign to this object

    private bool isVisible = false;

    public void ToggleMenu()
    {
        isVisible = !isVisible;
        ButtonPanel.SetActive(isVisible);

    }
}


