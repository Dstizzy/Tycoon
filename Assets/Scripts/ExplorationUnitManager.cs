using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class ExplorationUnitManager : MonoBehaviour {
    [SerializeField] private Transform explorePanel;
    [SerializeField] private Transform infoPanel;
    [SerializeField] private Transform upgradePanel;
    public TextMeshProUGUI explorationUnitLevelText;

    const int EXPLORE_BUTTON = 1;
    const int INFO_BUTTON = 2;
    const int UPGRADE_BUTTON = 3;
    const int STARTING_LEVEL = 1;
    const int ENDING_LEVEL = 5;

    private static int explorationUnitLevel = STARTING_LEVEL;

    private void Awake() {
        if (infoPanel == null) {
            Debug.LogError("Trade Panel is not assigned in the Inspector!");
        } else {
            infoPanel.gameObject.SetActive(false);
        }

        if (explorePanel == null) {
            Debug.LogError("Explore Panel is not assigned");
        } else {
            explorePanel.gameObject.SetActive(false);
        }

        if(explorationUnitLevelText == null) {
            Debug.LogError("Exploration Unit Level Text is not assigned");
        } else {
            explorationUnitLevelText.text = "Level " + explorationUnitLevel.ToString();
        }
    }

    public void RequestExplorationUnitPanel(int buttonID) {
        switch (buttonID) {
            case EXPLORE_BUTTON:
                ShowExplorationPanel();
                explorePanel.transform.Find("ExitButton").GetComponent<Button>().onClick.AddListener(() => CloseExplorationUnitPanel(EXPLORE_BUTTON));
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

    public void UpgradeExplorationUnit() {
        // Check if the exploration unit can be upgraded
        if (explorationUnitLevel < ENDING_LEVEL) {
            explorationUnitLevel += 1;
        } else {
            Debug.Log("Exploration Unit is already at max level.");
        }


        explorationUnitLevelText.text = "Level " + explorationUnitLevel.ToString();

        // Close the upgrade panel after upgrading
        CloseUpgradePanel();
        PopUpManager.Instance.EnablePlayerInput();
    }

    public int GetLevel() { return explorationUnitLevel; }

    public void CloseExplorationUnitPanel(int buttonID) {
        switch (buttonID) {
            case EXPLORE_BUTTON:
                CloseExplorationPanel();
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

    private void ShowExplorationPanel() {
        explorePanel.gameObject.SetActive(true);
    }

    private void ShowInfoPanel() {
        infoPanel.gameObject.SetActive(true);
    }

    private void ShowUpgradePanel() {
        upgradePanel.gameObject.SetActive(true);
    }
    private void CloseExplorationPanel() {
        explorePanel.gameObject.SetActive(false);
    }
    private void CloseInfoPanel() {
        infoPanel.gameObject.SetActive(false);
    }
    private void CloseUpgradePanel() {
        upgradePanel.gameObject.SetActive(false);
    }
}
