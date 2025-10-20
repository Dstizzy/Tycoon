using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class OreRefinery_Manager : MonoBehaviour {
    const int REFINE_BUTTON = 1;
    const int INFO_BUTTON = 2;
    const int UPGRADE_BUTTON = 3;
    const int STARTING_LEVEL = 1;
    const int ENDING_LEVEL = 5;

    [SerializeField] private Transform infoPanel;
    [SerializeField] private Transform upgradePanel;
    public TextMeshProUGUI oreRefineryText;

    private static int oreLevel = STARTING_LEVEL;

    private void Awake() {
        if (infoPanel == null) {
            Debug.LogError("Info Panel is not assigned in the Inspector!");
        } else {
            infoPanel.gameObject.SetActive(false);
        }

        if(oreRefineryText == null) {
            Debug.LogError("Ore Refinery Level Text is not assigned");
        } else {
            oreRefineryText.text = "Level " + oreLevel.ToString();
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
                ShowUpgradePanel();
                upgradePanel.Find("YesButton").GetComponent<Button>().onClick.AddListener(() => UpgradeOreRefinory());
                upgradePanel.transform.Find("CancelButton").GetComponent<Button>().onClick.AddListener(() => CloseOreRefinoryPanel(UPGRADE_BUTTON));
                break;
            default:
                Debug.Log("Building Panel: Unknown button ID.");
                break;
        }
    }

    public void UpgradeOreRefinory()
    {
        // Check if the ore refinery can be upgraded
        if (oreLevel < ENDING_LEVEL)
        {
            oreLevel += 1;
        }
        else
        {
            Debug.Log("Ore Refinery is already at max level.");
        }

        oreRefineryText.text = "Level " + oreLevel.ToString();
        // Close the upgrade panel after upgrading
        CloseUpgradePanel();
        PopUpManager.Instance.EnablePlayerInput();
    }

    public void CloseOreRefinoryPanel(int buttonID) {
        switch (buttonID) {
            case REFINE_BUTTON:
                break;
            case INFO_BUTTON:
                CloseInfoPanel();
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
    //private void ShowExplorePanel() {
    //    refinePanel.gameObject.SetActive(true);
    //}
    private void ShowInfoPanel() {
        infoPanel.gameObject.SetActive(true);
    }
    private void ShowUpgradePanel() {
        upgradePanel.gameObject.SetActive(true);
    }
    //private void CloseTradePanel() {
    //    refinePanel.gameObject.SetActive(false);
    //}
    private void CloseInfoPanel() {
        infoPanel.gameObject.SetActive(false);
    }
    private void CloseUpgradePanel() {
        upgradePanel.gameObject.SetActive(false);
    }
}
