using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class MainUIManager : MonoBehaviour {
    public Button MainMenuButton;
    public Button[] DropdownButtons;
    public Button InventoryButton;

    [SerializeField] private TextMeshProUGUI pearCountText;
    [SerializeField] private TextMeshProUGUI oreCountText;

    private bool isVisible = false;

    private void Awake() {

      if (InventoryManager.Instance == null) {
         // This is primarily for safety, though the script execution order should help.
         // You might need to check your Unity Project Settings -> Script Execution Order 
         // to ensure InventoryManager runs before MainUIManager.
         Debug.LogError("InventoryManager instance is not yet available in MainUIManager Awake. Delaying subscriptions.");
         // If it's not ready, we cannot subscribe yet and must defer to Start().
         return;
      }

      // Initially hide dropdown buttons
      foreach (Button btn in DropdownButtons) {
            btn.gameObject.SetActive(false);
        }
        // Add listener to main menu button
        MainMenuButton.onClick.AddListener(ToggleMenu);
        InventoryButton.onClick.AddListener(() => {
            InventoryManager.Instance.ShowInventoryPanel();
        });

        ChangePearlCountText(InventoryManager.Instance.pearlCount);
        ChangeOreCountText(InventoryManager.Instance.oreCount);
        InventoryManager.Instance.OnOreCountChanged   += ChangeOreCountText;
        InventoryManager.Instance.OnPearlCountChanged += ChangePearlCountText;
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
   
    public void ChangeOreCountText(int newOreCount) {
        if (oreCountText != null)
         oreCountText.text = newOreCount.ToString();
    }

}