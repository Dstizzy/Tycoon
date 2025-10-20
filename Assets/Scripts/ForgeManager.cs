using UnityEngine;
using UnityEngine.UI;

public class ForgeManager : MonoBehaviour {
    [SerializeField] private Transform craftPanel;
    [SerializeField] private Transform infoPanel;
    [SerializeField] private Transform upgradePanel;

    const int CRAFT_BUTTON = 1;
    const int INFO_BUTTON = 2;
    const int UPGRADE_BUTTON = 3;

    private void Awake() {
        if (infoPanel == null) {
            Debug.LogError("Info Panel is not assigned in the Inspector!");
        } else {
            infoPanel.gameObject.SetActive(false);
        }

        if( craftPanel == null) {
            Debug.LogError("Craft Panel is not assigned");
        } else {
            craftPanel.gameObject.SetActive(false);
        }
    }

    public void RequestForgePanel(int buttonID) {
        switch (buttonID) {
            case CRAFT_BUTTON:
                ShowCraftPanel();
                craftPanel.transform.Find("ExitButton").GetComponent<Button>().onClick.AddListener(() => CloseForgePanel(CRAFT_BUTTON));
                break;
            case INFO_BUTTON:
                ShowInfoPanel();
                infoPanel.transform.Find("ExitButton").GetComponent<Button>().onClick.AddListener(() => CloseForgePanel(INFO_BUTTON));
                break;
            case UPGRADE_BUTTON:
                Debug.Log("Building Panel: Info requested.");
                break;
            default:
                Debug.Log("Building Panel: Unknown button ID.");
                break;
        }
    }

    public void CloseForgePanel(int buttonID) {
        switch (buttonID) {
            case CRAFT_BUTTON:
                CloseCraftPanel();
                break;
            case INFO_BUTTON:
                CloseInfoPanel();
                break;
            case UPGRADE_BUTTON:
                Debug.Log("Building Panel: Info requested.");
                break;
            default:
                Debug.Log("Building Panel: Unknown button ID.");
                break;
        }
        PopUpManager.Instance.EnablePlayerInput();
    }
    private void ShowCraftPanel() {
        craftPanel.gameObject.SetActive(true);
    }
    private void ShowInfoPanel() {
        infoPanel.gameObject.SetActive(true);
    }
    private void ShowUpgradePanel() {
        upgradePanel.gameObject.SetActive(true);
    }
    private void CloseCraftPanel() {
        craftPanel.gameObject.SetActive(false);
    }
    private void CloseInfoPanel() {
        infoPanel.gameObject.SetActive(false);
    }
    private void CloseUpgradePanel() {
        upgradePanel.gameObject.SetActive(false);
    }
}