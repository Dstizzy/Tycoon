using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class MainUIManager : MonoBehaviour {
    public Button MainMenuButton; // Assign to this object
    public Button[] DropdownButtons; // Assign to this object

    [SerializeField] private TextMeshProUGUI pearCountText;
    [SerializeField] private TextMeshProUGUI crystalCountText;

    private bool isVisible = false;

    private void Start() {
        // Initially hide dropdown buttons
        foreach (Button btn in DropdownButtons) {
            btn.gameObject.SetActive(false);
        }
        // Add listener to main menu button
        MainMenuButton.onClick.AddListener(ToggleMenu);

        ChangePearlCountText(InventoryManager.Instance.pearlCount);
        ChangeCrystalCountText(InventoryManager.Instance.crystalCount);
        InventoryManager.Instance.OnPearlCountChanged += ChangePearlCountText;
        InventoryManager.Instance.OnCrystalCountChanged += ChangeCrystalCountText;
    }
    public void ToggleMenu() {
        isVisible = !isVisible;
        DropdownButtons[0].gameObject.SetActive(isVisible);
        DropdownButtons[1].gameObject.SetActive(isVisible);
    }

    public void ChangePearlCountText(int newPearlCount) {
        if (pearCountText != null)
            pearCountText.text = newPearlCount.ToString();
    }
    public void ChangeCrystalCountText(int newCrystalCount) {
        if (crystalCountText != null)
            crystalCountText.text = newCrystalCount.ToString();
    }
}