using UnityEngine;
using UnityEngine.UI;

public class ExplorationUnitManager : MonoBehaviour
{
    const int EXPLORE_BUTTON = 1;
    const int INFO_BUTTON = 2;
    const int UPGRADE_BUTTON = 3;
    const int STARTING_LEVEL = 1;
    const int ENDING_LEVEL = 5;

    [SerializeField] private Transform infoPanel;
    [SerializeField] private Transform upgradePanel;
    private static int explorationUnitLevel = STARTING_LEVEL;

    private void Awake() 
    {
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
                ShowUpgradePanel();
                upgradePanel.transform.Find("YesButton").GetComponent<Button>().onClick.AddListener(() => UpgradeExplorationUnit());
                upgradePanel.transform.Find("CancelButton").GetComponent<Button>().onClick.AddListener(() => CloseExplorationUnitPanel(UPGRADE_BUTTON));
                break;
            default:
                Debug.Log("Building Panel: Unknown button ID.");
                break;
        }
    }

    public void UpgradeExplorationUnit()
    {
        // Check if the exploration unit can be upgraded
        if (ExplorationUnitManager.explorationUnitLevel < ENDING_LEVEL)
        {
            explorationUnitLevel += 1;
        }
        else
        {
            Debug.Log("Exploration Unit is already at max level.");
        }

        // Close the upgrade panel after upgrading
        CloseUpgradePanel();
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
                CloseUpgradePanel();
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
