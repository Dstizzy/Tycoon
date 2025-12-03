using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OreRefinery_Manager : MonoBehaviour { 

    public static OreRefinery_Manager Instance { get; private set; }

    const int INFO_BUTTON = 1;
    const int UPGRADE_BUTTON = 2;
    const int STARTING_LEVEL = 1;
    const int ENDING_LEVEL = 5;

    [SerializeField] private Transform infoPanel;
    [SerializeField] private Transform upgradePanel;
    public TextMeshProUGUI oreRefineryLevelText;

    public int oreLevel = STARTING_LEVEL;

    public int CurrentOreProduction { get; private set; }
    public int NextUpgradeCostInPearls { get; private set; }

    private void Awake() {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        if (infoPanel == null) {
            Debug.LogError("Info Panel is not assigned in the Inspector!");
        } else {
            infoPanel.gameObject.SetActive(false);
        }

        // --- ADDED: Subscribe to the TurnManager's event ---
        //TurnManager.OnTurnEnded += ProduceOres;

        //CalculateRefineryValues();

        if (oreRefineryLevelText == null) {
            Debug.LogError("Ore Refinery Level Text is not assigned");
        } else {
            oreRefineryLevelText.text = "Level " + oreLevel.ToString();
        }
    }

    public void RequestOreRefinoryPanel(int buttonID) {
        switch (buttonID) {
            case INFO_BUTTON:
                ShowInfoPanel();
                infoPanel.transform.Find("ExitButton").GetComponent<Button>().onClick.AddListener(() => CloseOreRefinoryPanel(INFO_BUTTON));
                break;
            case UPGRADE_BUTTON:
                ShowUpgradePanel();
                //upgradePanel.Find("YesButton").GetComponent<Button>().onClick.AddListener(() => UpgradeOreRefinory());
                upgradePanel.transform.Find("CancelButton").GetComponent<Button>().onClick.AddListener(() => CloseOreRefinoryPanel(UPGRADE_BUTTON));
                break;
            default:
                Debug.Log("Building Panel: Unknown button ID.");
                break;
        }
    }

    public void CloseOreRefinoryPanel(int buttonID) {
        switch (buttonID) 
        {
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
    private void ShowInfoPanel() 
    {
        infoPanel.gameObject.SetActive(true);
    }
    private void ShowUpgradePanel() 
    {
        upgradePanel.gameObject.SetActive(true);
    }
    //private void CloseTradePanel() {
    //    refinePanel.gameObject.SetActive(false);
    //}
    private void CloseInfoPanel() 
    {
        infoPanel.gameObject.SetActive(false);
    }
    private void CloseUpgradePanel() 
    {
        upgradePanel.gameObject.SetActive(false);
    }
}
