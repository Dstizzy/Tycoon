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

   [SerializeField] private Transform infoPanel;
   [SerializeField] private Transform upgradePanel;
   [SerializeField] private GameObject buildingCanvas;
   [SerializeField] private GameObject jamPanel;
   public TextMeshProUGUI oreRefineryLevelText;

   TickerSystem ticker;

   public int oreLevel = STARTING_LEVEL;

   public int jammingChance = 10;

   public bool IsBlocked = false;

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

        ticker = TickerSystem.Instance;
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
            upgradePanel.Find("YesButton").GetComponent<Button>().onClick.AddListener(() => UpgradeOreRefinery());
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
   //private void OnDestroy()
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
            //NextUpgradeCostInPatchKits = 1; Need to implement Patch Kits in InventoryManager
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

   public void ActivateJamButton()
   {
      buildingCanvas.transform.Find("Jam_Button").gameObject.SetActive(true);
      buildingCanvas.transform.Find("Jam_Button").GetComponent<Button>().onClick.AddListener(() => OpenJamPanel());
   }

   public void ActivateJamSymbol()
   {
      buildingCanvas.transform.Find("Jammed_Symbol").gameObject.SetActive(true);
   }

   public void OpenJamPanel()
   {
      jamPanel.SetActive(true);
      jamPanel.transform.Find("ExitButton").GetComponent<Button>().onClick.AddListener(() => CloseJamPanel());
      jamPanel.transform.Find("PayButtons/UnjamButton").GetComponent<Button>().onClick.AddListener(() => PayForUnjamming(1));
      jamPanel.transform.Find("PayButtons/PayItButton").GetComponent<Button>().onClick.AddListener(() => PayForUnjamming(2));

      PopUpManager.Instance.DisablePlayerInput();
   }

   public void PayForUnjamming(int paymentType)
   {
      if (paymentType == 1)
      {
         if (InventoryManager.Instance.patchKitCount >= 1)
         {
            //InventoryManager.Instance.TrySpendPatchKit(1);
            IsBlocked = false;
            buildingCanvas.transform.Find("Jammed_Symbol").gameObject.SetActive(false);
            CloseJamPanel();
            Debug.Log("Ore Refinery unjammed successfully.");
         }
         else
         {
            Debug.Log("Not enough Patch Kits to unjam the Ore Refinery.");
         }
      }
      else
      {
         if (InventoryManager.Instance.pearlCount >= 100)
         {
            InventoryManager.Instance.TrySpendPearl(100);
            IsBlocked = false;
            buildingCanvas.transform.Find("Jammed_Symbol").gameObject.SetActive(false);
            CloseJamPanel();
            Debug.Log("Ore Refinery unjammed successfully.");
         }
         else
         {
            Debug.Log("Not enough Pearls to unjam the Ore Refinery.");
            ticker.ShowTicker("Not enough Pearls to unjam the Ore Refinery.", Color.red, TickerSystem.MessageTypes.ResultMessage);
         }
      }
   }

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
         IsBlocked = true;
         ActivateJamButton();
         ActivateJamSymbol();
      }

      InventoryManager.Instance.TryAddOre(CurrentOreProduction);
   }

   public void CloseJamPanel()
   {
      jamPanel.SetActive(false);
      jamPanel.transform.Find("ExitButton").GetComponent<Button>().onClick.RemoveAllListeners();
      PopUpManager.Instance.EnablePlayerInput();
   }
   public void DeactivateJamButton()
   {
      buildingCanvas.transform.Find("Jam_Button").GetComponent<Button>().onClick.RemoveListener(() => OpenJamPanel());
      buildingCanvas.transform.Find("Jam_Button").gameObject.SetActive(false);

   }

   public void UpgradeOreRefinery()
   {
      if (oreLevel >= ENDING_LEVEL)
      {
         Debug.Log("Ore Refinery is already at max level.");
         ticker.ShowTicker("Ore Refinery is already at max level.", Color.white, TickerSystem.MessageTypes.ResultMessage);
         return;
      }
      if (InventoryManager.Instance.pearlCount >= NextUpgradeCostInPearls && InventoryManager.Instance.oreCount >= NextUpgradeCostInOre)
      {
         InventoryManager.Instance.TrySpendPearl(NextUpgradeCostInPearls);
         InventoryManager.Instance.TrySpendOre(NextUpgradeCostInOre);
         
         oreLevel++;
         CalculateRefineryValues();

         oreRefineryLevelText.text = "Level " + oreLevel.ToString();
         Debug.Log($"Ore Refinery upgraded to level {oreLevel}!");
         ticker.ShowTicker($"Ore Refinery upgraded to level {oreLevel}!", Color.green, TickerSystem.MessageTypes.ResultMessage);
      }
      else
      {
         Debug.Log("Not enough resources to upgrade the Ore Refinery.");
         ticker.ShowTicker("Not enough Pearls to upgrade the Ore Refinery.", Color.red, TickerSystem.MessageTypes.ResultMessage);
      }
   }

   public void ActivateManualResetCounter()
   {
      buildingCanvas.transform.Find("JamCounter").gameObject.SetActive(true);
      buildingCanvas.transform.Find("JamCounter").GetComponent<TextMeshProUGUI>().text = ($"{TurnManager.jamTurnCounter.ToString()}...");
   }

   public void DeactivateManualResetCounter()
   {
      buildingCanvas.transform.Find("JamCounter").gameObject.SetActive(false);
   }

   public void DeactivateJamSymbol()
   {
      buildingCanvas.transform.Find("Jammed_Symbol").gameObject.SetActive(false);
   }
}