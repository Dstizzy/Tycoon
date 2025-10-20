using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class ForgeManager : MonoBehaviour {
    const int CRAFT_BUTTON = 1;
    const int INFO_BUTTON = 2;
    const int UPGRADE_BUTTON = 3;
    const int STARTING_LEVEL = 1;
    const int ENDING_LEVEL = 5;

    [SerializeField] private Transform craftPanel;
    [SerializeField] private Transform infoPanel;
    [SerializeField] private Transform upgradePanel;
    public TextMeshProUGUI forgeLevelText;


    private static int forgeLevel = STARTING_LEVEL;

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

        if(forgeLevelText == null) {
            Debug.LogError("Forge Level Text is not assigned");
        } else {
            forgeLevelText.text = "Level " + forgeLevel.ToString();
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
                ShowUpgradePanel();
                upgradePanel.transform.Find("YesButton").GetComponent<Button>().onClick.AddListener(() => UpgradeForge());
                upgradePanel.transform.Find("CancelButton").GetComponent<Button>().onClick.AddListener(() => CloseForgePanel(UPGRADE_BUTTON));
                break;
            default:
                Debug.Log("Building Panel: Unknown button ID.");
                break;
        }
    }
    public void UpgradeForge()
    {
        // Check if the forge can be upgraded
        if (forgeLevel < ENDING_LEVEL)
        {
            forgeLevel += 1;
        }
        else
        {
            Debug.Log("Exploration Unit is already at max level.");
        }

        forgeLevelText.text = "Level " + forgeLevel.ToString();
        // Close the upgrade panel after upgrading
        CloseUpgradePanel();
        PopUpManager.Instance.EnablePlayerInput();
    }

    public int GetLevel() { return forgeLevel; }

    public void CloseForgePanel(int buttonID) {
        switch (buttonID) {
            case CRAFT_BUTTON:
                CloseCraftPanel();
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