using UnityEngine;
using UnityEngine.UI;

public class OreRefinery_Manager : MonoBehaviour {
    [SerializeField] private Transform refinePanel;
    [SerializeField] private Transform infoPanel;
    [SerializeField] private Transform upgradePanel;

    const int REFINE_BUTTON = 1;
    const int INFO_BUTTON = 2;
    const int UPGRADE_BUTTON = 3;

    private void Awake() {
        if (infoPanel == null) {
            Debug.LogError("Info Panel is not assigned in the Inspector!");
        } else {
            infoPanel.gameObject.SetActive(false);
        }
    }

    public void RequestOreRefinoryPanel(int buttonID) {
        switch (buttonID) {
            case REFINE_BUTTON:
                Debug.Log("Ore Refinery Panel: Refine requested.");
                break;
            case INFO_BUTTON:
                ShowInfoPanel();
                infoPanel.transform.Find("ExitButton").GetComponent<Button>().onClick.AddListener(() => CloseOreRefinoryPanel(INFO_BUTTON));
                break;
            case UPGRADE_BUTTON:
                Debug.Log("Building Panel: Info requested.");
                break;
            default:
                Debug.Log("Building Panel: Unknown button ID.");
                break;
        }
    }

    public void CloseOreRefinoryPanel(int buttonID) {
        switch (buttonID) {
            case REFINE_BUTTON:
                CloseTradePanel();
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
    private void ShowExplorePanel() {
        refinePanel.gameObject.SetActive(true);
    }
    private void ShowInfoPanel() {
        infoPanel.gameObject.SetActive(true);
    }
    private void ShowUpgradePanel() {
        upgradePanel.gameObject.SetActive(true);
    }
    private void CloseTradePanel() {
        refinePanel.gameObject.SetActive(false);
    }
    private void CloseInfoPanel() {
        infoPanel.gameObject.SetActive(false);
    }
    private void CloseUpgradePanel() {
        upgradePanel.gameObject.SetActive(false);
    }
}
