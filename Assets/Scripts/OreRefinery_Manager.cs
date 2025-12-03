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

        TurnManager.OnTurnEnded += ProduceOres;

        CalculateRefineryValues();

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
                upgradePanel.Find("YesButton").GetComponent<Button>().onClick.AddListener(() => UpgradeOreRefinory());
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

   // --- ADDED: Unsubscribe when destroyed ---
   private void OnDestroy()
   {
      if (TurnManager.Instance != null)
      {
         TurnManager.OnTurnEnded -= ProduceOres;
      }
   }

   // --- MODIFIED: Calculation Function ---
   private void CalculateRefineryValues()
   {
      // Ore production logic
      CurrentOreProduction = 10 + (5 * oreLevel);

      // Pearl consumption logic
      NextUpgradeCostInPearls = 50 * oreLevel;
   }

   // --- ADDED: This function is called by the TurnManager's event ---
   private void ProduceOres()
   {
      // Use InventoryManager.Instance.TryAddOre to add ore.
      InventoryManager.Instance.TryAddOre(CurrentOreProduction);

      // Notify UI (or other scripts) that oreCount has changed.
      InventoryManager.Instance.OnOreCountChanged?.Invoke(InventoryManager.Instance.oreCount);

      Debug.Log("OreRefinery produced " + CurrentOreProduction + " Ore. Total Ore: " + InventoryManager.Instance.oreCount);
   }

   // --- MODIFIED: Now spends Pearls using InventoryManager ---
   public void UpgradeOreRefinory()
   {

      if (oreLevel < ENDING_LEVEL)
      {

         // 1. Attempt to pay Pearls using InventoryManager.
         if (InventoryManager.Instance.pearlCount >= NextUpgradeCostInPearls)
         {
            // 2. (Success) Enough Pearls, so spend them.
            InventoryManager.Instance.TrySpendPearl(NextUpgradeCostInPearls);

            oreLevel += 1;
            CalculateRefineryValues(); // Recalculate production/cost for the next level.
            Debug.Log("Upgrade successful to Level " + oreLevel);
         }
         else
         {
            // 3. (Failure) Not enough Pearls.
            Debug.Log("UPGRADE FAILED: Not enough Pearls. Need " + NextUpgradeCostInPearls);
         }

      }
      else
      {
         Debug.Log("Ore Refinery is already at max level.");
      }

      // (Existing Panel/UI update logic)
      oreRefineryLevelText.text = "Level " + oreLevel.ToString();
      upgradePanel.transform.Find("YesButton").GetComponent<Button>().onClick.RemoveAllListeners();
      upgradePanel.transform.Find("CancelButton").GetComponent<Button>().onClick.RemoveAllListeners();
      CloseUpgradePanel();
      PopUpManager.Instance.EnablePlayerInput();
   }
}
