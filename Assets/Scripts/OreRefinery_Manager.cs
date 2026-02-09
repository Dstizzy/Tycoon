using NUnit.Framework.Constraints;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OreRefinery_Manager : MonoBehaviour
{

   public static OreRefinery_Manager Instance { get; private set; }

   const int INFO_BUTTON = 1;
   const int UPGRADE_BUTTON = 2;
   const int STARTING_LEVEL = 1;
   const int ENDING_LEVEL = 4;

   [SerializeField] private Transform       infoPanel;
   [SerializeField] private Transform       upgradePanel;
   [SerializeField] private GameObject      buildingCanvas;
   [SerializeField] private GameObject      jamPanel;
                    public  TextMeshProUGUI oreRefineryLevelText;

   public int oreLevel = STARTING_LEVEL;
   public int jammingChance = 15; // Starting percentage for jamming
   public int CurrentOreProduction { get; private set; }
   public int NextUpgradeCostInPearls { get; private set; }

   public int NextUpgradeCostInOre { get; private set; }

   private void Awake()
   {
      if (Instance != null && Instance != this)
      {
         Destroy(gameObject);
         return;
      }
      Instance = this;

      if (infoPanel == null)
      {
         Debug.LogError("Info Panel is not assigned in the Inspector!");
      }
      else
      {
         infoPanel.gameObject.SetActive(false);
      }

      TurnManager.OnTurnEnded += ProduceOres;

      CalculateRefineryValues();

      if (oreRefineryLevelText == null)
      {
         Debug.LogError("Ore Refinery Level Text is not assigned");
      }
      else
      {
         oreRefineryLevelText.text = "Level " + oreLevel.ToString();
      }
   }

   public void RequestOreRefinoryPanel(int buttonID)
   {
      switch (buttonID)
      {
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

   public void CloseOreRefinoryPanel(int buttonID)
   {
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
      switch (oreLevel)
      {
         case 1:
            Debug.Log("Ore Refinery Level 1: Produces 10 Ore per turn. Upgrade Cost: 100 Pearls + 50 ore.");
            CurrentOreProduction = 10;
            NextUpgradeCostInPearls = 100;
            NextUpgradeCostInOre = 50;
            break;
         case 2:
            Debug.Log("Ore Refinery Level 2: Produces 25 Ore per turn. Upgrade Cost: 350 pearls + 1 patch kit.");
            CurrentOreProduction = 25;
            NextUpgradeCostInPearls = 350;
            //NextUpgradeCostInPatchKits = 1;
            break;
         case 3:
            Debug.Log("Ore Refinery Level 3: Produces 60 Ore per turn. Upgrade Cost: 1000 pearls + 1 precision lens");
            CurrentOreProduction = 60;
            NextUpgradeCostInPearls = 1000;
            //NextUpgradeCostInPrecisionLens = 1; Need to implement Precision Lens in InventoryManager
            break;
         case 4:
            Debug.Log("Ore Refinery Level 4: Produces 150 Ore per turn.");
            CurrentOreProduction = 150;
            break;
         default:
            Debug.Log("Unknown Ore Refinery Level.");
            break;
      }
   }

   // Reduces jamming percentage when user unlocks tier 1 in lab
   public void ReduceJamming(int oreAmount)
   {
      jammingChance -= oreAmount;

      if (jammingChance < 0)
         jammingChance = 0;

      Debug.Log($"Refinery improved! Jamming chance is now {jammingChance}%");
   }

   private void ProduceOres()
   {
      int roll = Random.Range(0, 100);

      if (roll < jammingChance)
      {
         Debug.Log($"<color=red>Refinery Jammed! (Rolled {roll} vs Chance {jammingChance})</color>");
      }

      InventoryManager.Instance.TryAddOre(CurrentOreProduction);
   }

   // --- MODIFIED: Now spends Pearls using InventoryManager ---
   public void UpgradeOreRefinory()
   {

      if (oreLevel < ENDING_LEVEL)
      {

         // 1. Attempt to pay Pearls using InventoryManager.
         if (InventoryManager.Instance.pearlCount >= NextUpgradeCostInPearls && InventoryManager.Instance.oreCount >= NextUpgradeCostInOre)
         {
            // 2. (Success) Enough Pearls, so spend them.
            InventoryManager.Instance.TrySpendPearl(NextUpgradeCostInPearls);
            InventoryManager.Instance.TrySpendOre(NextUpgradeCostInOre);

            oreLevel += 1;
            JammingPercentage += 5;
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

      switch (oreLevel)
      {
         case 2:
            upgradePanel.transform.Find("UpgradePanelLvlOne").gameObject.SetActive(false);
            upgradePanel.transform.Find("UpgradePanelLvlTwo").gameObject.SetActive(true);
            break;
         case 3:
            upgradePanel.transform.Find("UpgradePanelLvlTwo").gameObject.SetActive(false);
            upgradePanel.transform.Find("UpgradePanelLvlThree").gameObject.SetActive(true);
            break;
         default:
            break;
      }

      // (Existing Panel/UI update logic)
      oreRefineryLevelText.text = "Level " + oreLevel.ToString();
      upgradePanel.transform.Find("YesButton").GetComponent<Button>().onClick.RemoveAllListeners();
      upgradePanel.transform.Find("CancelButton").GetComponent<Button>().onClick.RemoveAllListeners();
      CloseUpgradePanel();
      PopUpManager.Instance.EnablePlayerInput();
   }

   

   
}
