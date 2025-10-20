using UnityEngine;
using UnityEngine.UI;

public class ExplorationUnitManager : MonoBehaviour
{
    [SerializeField] private Transform infoPanel;
    [SerializeField] private Transform upgradePanel;

    const int EXPLORE_BUTTON = 1;
    const int INFO_BUTTON = 2;
    const int UPGRADE_BUTTON = 3;

    private void Awake() {
        if (infoPanel == null) {
            Debug.LogError("Trade Panel is not assigned in the Inspector!");
        } else {
            infoPanel.gameObject.SetActive(false);
        }
    }

    public void RequestExplorationUnitPanel(int buttonID) {
        switch (buttonID) {
            case EXPLORE_BUTTON:
                break;
            case INFO_BUTTON:
                ShowInfoPanel();
                infoPanel.transform.Find("ExitButton").GetComponent<Button>().onClick.AddListener(() => CloseExplorationUnitPanel(INFO_BUTTON));
                break;
            case UPGRADE_BUTTON:
                Debug.Log("Building Panel: Info requested.");
                break;
            default:
                Debug.Log("Building Panel: Unknown button ID.");
                break;
        }
    }

    public void CloseExplorationUnitPanel(int buttonID) {
        switch (buttonID) {
            case EXPLORE_BUTTON:
                break;
            case INFO_BUTTON:
                CloseInfoPanel();
                Debug.Log("Building Panel: Sell requested.");
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

    private void ShowInfoPanel() {
        infoPanel.gameObject.SetActive(true);
    }

    private void ShowUpgradePanel() {
        upgradePanel.gameObject.SetActive(true);
    }
    private void CloseInfoPanel() {
        infoPanel.gameObject.SetActive(false);
    }
    private void CloseUpgradePanel() {
        upgradePanel.gameObject.SetActive(false);
    }
}
