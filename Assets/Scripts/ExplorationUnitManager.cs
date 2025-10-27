using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ExplorationUnitManager : MonoBehaviour
{
    [SerializeField] private Transform explorePanel;
    [SerializeField] private Transform infoPanel;
    [SerializeField] private Transform upgradePanel;
    public TextMeshProUGUI explorationUnitLevelText;

    [Header("Explore Panel Tabs")]
    [SerializeField] private GameObject levelOneTab;
    [SerializeField] private GameObject levelTwoTab;
    [SerializeField] private GameObject levelThreeTab;

    [Header("Boat Visuals")]
    [SerializeField] private GameObject levelOneBoat;
    [SerializeField] private GameObject levelTwoBoat;
    [SerializeField] private GameObject levelThreeBoat;

    const int EXPLORE_BUTTON = 1;
    const int INFO_BUTTON = 2;
    const int UPGRADE_BUTTON = 3;
    const int STARTING_LEVEL = 1;
    const int ENDING_LEVEL = 3;
    const int UPGRADE_COST = 100; // adjust as needed
    const int LEVEL_1_TAB_ID = 11;
    const int LEVEL_2_TAB_ID = 12;
    const int LEVEL_3_TAB_ID = 13;
    const int LEVEL_1_EXPLORE_COST = 200; // adjust as needed
    const int LEVEL_2_EXPLORE_COST = 300; // adjust as needed
    const int LEVEL_3_EXPLORE_COST = 400; // adjust as needed
    const int EXPLORE_DURATION = 2;

    private static int explorationUnitLevel = STARTING_LEVEL;
    private static int exploreEndTurn = 0;

    private int activeExploreTabId = LEVEL_1_TAB_ID;

    private void Awake()
    {
        if (infoPanel == null)
        {
            Debug.LogError("Info Panel is not assigned in the Inspector!");
        }
        else
        {
            infoPanel.gameObject.SetActive(false);
        }

        if (explorePanel == null)
        {
            Debug.LogError("Explore Panel is not assigned");
        }
        else
        {
            explorePanel.gameObject.SetActive(false);
        }

        if (explorationUnitLevelText == null)
        {
            Debug.LogError("Exploration Unit Level Text is not assigned");
        }
        else
        {
            explorationUnitLevelText.text = "Level " + explorationUnitLevel.ToString();
        }
    }

    private bool IsExplorationOngoing()
    {
        return false; // PopUpManager.CurrentTurn < exploreEndTurn;
    }

    private bool HasEnoughCurrency(int cost)
    {
        return true; // CurrencyManager.Instance.GetPearls() >= cost;
    }

    public void RequestExplorationUnitPanel(int buttonID)
    {
        bool isExploring = IsExplorationOngoing();

        switch (buttonID)
        {
            case EXPLORE_BUTTON:
                ShowExplorationPanel();
                UpdateExploreButton(activeExploreTabId, isExploring);
                explorePanel.transform.Find("ExitButton").GetComponent<Button>().onClick.AddListener(() => CloseExplorationUnitPanel(EXPLORE_BUTTON));
                break;
            case INFO_BUTTON:
                ShowInfoPanel();
                infoPanel.transform.Find("ExitButton").GetComponent<Button>().onClick.AddListener(() => CloseExplorationUnitPanel(INFO_BUTTON));
                break;
            case UPGRADE_BUTTON:
                ShowUpgradePanel();
                Transform yesButtonTransform = upgradePanel.transform.Find("YesButton");
                Button yesButton = yesButtonTransform.GetComponent<Button>();
                if (yesButton != null)
                {
                    yesButton.onClick.RemoveAllListeners();
                    yesButton.onClick.AddListener(() => UpgradeExplorationUnit());

                    bool canAffordUpgrade = HasEnoughCurrency(UPGRADE_COST);
                    bool canUpgrade = canAffordUpgrade && !isExploring;

                    yesButton.interactable = canUpgrade;
                }
                else
                    Debug.LogError("YesButton not found on upgrade panel.");

                upgradePanel.transform.Find("CancelButton").GetComponent<Button>().onClick.AddListener(() => CloseExplorationUnitPanel(UPGRADE_BUTTON));
                break;
            default:
                Debug.Log("Building Panel: Unknown button ID.");
                break;
        }
    }

    public void switchExploreTab(int tabId)
    {
        activeExploreTabId = tabId;

        // Deactivate all three tabs
        if (levelOneTab != null) levelOneTab.SetActive(false);
        if (levelTwoTab != null) levelTwoTab.SetActive(false);
        if (levelThreeTab != null) levelThreeTab.SetActive(false);

        // Activate the requested panel
        switch (tabId)
        {
            case LEVEL_1_TAB_ID:
                if (levelOneTab != null) levelOneTab.SetActive(true);
                break;
            case LEVEL_2_TAB_ID:
                if (levelTwoTab != null) levelTwoTab.SetActive(true);
                break;
            case LEVEL_3_TAB_ID:
                if (levelThreeTab != null) levelThreeTab.SetActive(true);
                break;
            default:
                Debug.LogWarning("Unknown explore tab ID requested: " + tabId);
                break;
        }

        UpdateExploreButton(tabId, IsExplorationOngoing());
    }

    public void UpdateExploreButton(int tabId, bool isExploring)
    {
        GameObject activeTab = null;
        int exploreCost = 0;
        int requiredLevel = 0;

        switch (tabId)
        {
            case LEVEL_1_TAB_ID:
                activeTab = levelOneTab;
                exploreCost = LEVEL_1_EXPLORE_COST;
                requiredLevel = 1;
                break;
            case LEVEL_2_TAB_ID:
                activeTab = levelTwoTab;
                exploreCost = LEVEL_2_EXPLORE_COST;
                requiredLevel = 2;
                break;
            case LEVEL_3_TAB_ID:
                activeTab = levelThreeTab;
                exploreCost = LEVEL_3_EXPLORE_COST;
                requiredLevel = 3;
                break;
            default:
                Debug.LogWarning("Invalid tab ID provided in updateExploreButton().");
                return;
        }

        if (activeTab == null) return;
        if (activeTab.activeSelf) activeTab.SetActive(true);

        Transform startButtonTransform = activeTab.transform.Find("ExploreButton");

        if (startButtonTransform != null)
        {
            Button startButton = startButtonTransform.GetComponent<Button>();
            if (startButton != null)
            {
                startButton.onClick.RemoveAllListeners();

                if (explorationUnitLevel < requiredLevel)
                {
                    startButton.onClick.AddListener(() => RequestExplorationUnitPanel(UPGRADE_BUTTON));
                    startButton.interactable = explorationUnitLevel < ENDING_LEVEL;

                    Transform actionTextTransform = startButtonTransform.transform.Find("ExploreCost");
                    TextMeshProUGUI actionText = null;
                    if (actionTextTransform != null)
                    {
                        actionText = actionTextTransform.GetComponent<TextMeshProUGUI>();
                    }
                    if (actionText != null)
                    {
                        actionText.text = "Upgrade";
                    }
                }

                startButton.onClick.AddListener(startExploration);

                bool canAffordExplore = HasEnoughCurrency(exploreCost);
                bool canExplore = canAffordExplore && !isExploring;

                startButton.interactable = canExplore;

                //set explore button text to show exploration cost
                Transform costTextTransform = startButtonTransform.transform.Find("ExploreCost");
                TextMeshProUGUI costText = null;
                if (costTextTransform != null)
                {
                    costText = costTextTransform.GetComponent<TextMeshProUGUI>();
                }

                if (costText != null)
                {
                    costText.text = exploreCost.ToString();
                }
                else
                    Debug.LogWarning("TMP component ExploreCost not found in ExploreButton");
            }
            else
                Debug.LogWarning("Button component not found on ExploreButton of child tab");
        }
        else
            Debug.LogWarning("ExploreButton child not found in tab");
    }

    public void startExploration()
    {
        if (IsExplorationOngoing()) return;
        Debug.Log("Exploration started!");

        // add exploration duration to current turn and assign to the exploration's ending turn
        //exploreEndTurn = PopUpManager.CurrentTurn + EXPLORE_DURATION;

        //deduct exploration cost here

        HideAllBoats();
        CloseExplorationPanel();
        PopUpManager.Instance.EnablePlayerInput();
    }

    public void FinishExploration()
    {
        exploreEndTurn = 0;
        UpdateBoatVisuals();
    }

    public void UpgradeExplorationUnit()
    {
        // Check if the exploration unit can be upgraded
        if (explorationUnitLevel < ENDING_LEVEL)
        {
            explorationUnitLevel += 1;
            UpdateBoatVisuals();
        }
        else
        {
            Debug.Log("Exploration Unit is already at max level.");
        }


        explorationUnitLevelText.text = "Level " + explorationUnitLevel.ToString();

        // Close the upgrade panel after upgrading
        CloseUpgradePanel();
        PopUpManager.Instance.EnablePlayerInput();
    }

    private void HideAllBoats()
    {
        if (levelOneBoat != null) levelOneBoat.SetActive(false);
        if (levelTwoBoat != null) levelTwoBoat.SetActive(false);
        if (levelThreeBoat != null) levelThreeBoat.SetActive(false);
    }

    private void UpdateBoatVisuals()
    {
        if (IsExplorationOngoing())
        {
            HideAllBoats();
            return;
        }

        if (levelOneBoat != null) levelOneBoat.SetActive(explorationUnitLevel >= 1);
        if (levelTwoBoat != null) levelTwoBoat.SetActive(explorationUnitLevel >= 2);
        if (levelThreeBoat != null) levelThreeBoat.SetActive(explorationUnitLevel >= 3);
    }

    public int GetLevel() { return explorationUnitLevel; }

    public void CloseExplorationUnitPanel(int buttonID)
    {
        switch (buttonID)
        {
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

    private void ShowExplorationPanel()
    {
        explorePanel.gameObject.SetActive(true);
        int requiredLevel = activeExploreTabId - LEVEL_1_TAB_ID + 1;
        if (explorationUnitLevel < requiredLevel)
            switchExploreTab(LEVEL_1_TAB_ID + explorationUnitLevel - 1);
        else
            switchExploreTab(activeExploreTabId);
    }

    private void ShowInfoPanel()
    {
        infoPanel.gameObject.SetActive(true);
    }

    private void ShowUpgradePanel()
    {
        upgradePanel.gameObject.SetActive(true);
    }
    private void CloseExplorationPanel()
    {
        explorePanel.gameObject.SetActive(false);
    }
    private void CloseInfoPanel()
    {
        infoPanel.gameObject.SetActive(false);
    }
    private void CloseUpgradePanel()
    {
        upgradePanel.gameObject.SetActive(false);
    }
}