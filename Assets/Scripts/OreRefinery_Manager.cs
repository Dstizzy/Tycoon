using System.Collections.Generic;
using System;
using TMPro;


using UnityEngine;
using UnityEngine.UI;

using static TickerSystem;

public class OreRefinery_Manager : MonoBehaviour
{

   public static OreRefinery_Manager Instance { get; private set; }

   const int INFO_BUTTON    = 1;
   const int UPGRADE_BUTTON = 2;
   const int STARTING_LEVEL = 1;
   const int ENDING_LEVEL   = 3;
   const int LEVEL_2_PEARL_COST = 100;
   const int LEVEL_2_ORE_COST = 50;
   const int LEVEL_3_PEARL_COST = 300;
   const int LEVEL_3_ORE_COST = 100;

   public PanelManager panelManager;
   [SerializeField] private Transform infoPanel;
   [SerializeField] public Transform upgradePanel;
   [SerializeField] private GameObject buildingCanvas;
   [SerializeField] private GameObject jamPanel;
   public TextMeshProUGUI oreRefineryLevelText;

   [Header("Ore Visuals")]
   [SerializeField] private List<SpriteRenderer> buildingSpriteRenderer;
   [SerializeField] private List<Sprite> oreLevelSprites;

   [Header("Upgrade Costs")]
   [SerializeField] private int level2PearlCost = 100;
   [SerializeField] private int level2OreCost = 50;
   [SerializeField] private int level3PearlCost = 300;
   [SerializeField] private int level3OreCost = 100;

   TickerSystem ticker;

   public int  oreLevel          = STARTING_LEVEL;
   public int  jammingChance     = 0;
   public bool IsBlocked         = false;
   public bool tutorialUpgrade   = false;
   public bool manualResetOption = false;

   public int CurrentOreProduction { get; private set; }
   public int NextUpgradeCostInPearls { get; private set; }
   public int NextUpgradeCostInOre { get; private set; }

   // Changes the tutorial state to allow the upgrade tutorial to show after the player has completed the Trade Hut tutorial

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
            Button exitBtn = infoPanel.transform.Find("ExitButton").GetComponent<Button>();
            exitBtn.onClick.RemoveAllListeners(); 
            exitBtn.onClick.AddListener(() => CloseOreRefinoryPanel(INFO_BUTTON));
            break;

         case UPGRADE_BUTTON:
            ShowUpgradePanel();

            Button yesBtn = upgradePanel.Find("YesButton").GetComponent<Button>();
            yesBtn.onClick.RemoveAllListeners(); 
            yesBtn.onClick.AddListener(() => UpgradeOreRefinery());

            Button cancelBtn = upgradePanel.transform.Find("CancelButton").GetComponent<Button>();
            cancelBtn.onClick.RemoveAllListeners(); 
            cancelBtn.onClick.AddListener(() => CloseOreRefinoryPanel(UPGRADE_BUTTON));
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
      panelManager.OpenPanel(infoPanel.gameObject);

      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(false);
   }

   private void ShowUpgradePanel()
   {
      int pearlUpgradeCost = 0;
      int oreUpgradeCost = 0;
      string upgradeTitle = "";
      string upgradeExplanation = "";

      panelManager.OpenPanel(upgradePanel.gameObject);

      Transform titleTextTransform = upgradePanel.Find("Title");
      Transform mainTextTransform = upgradePanel.Find("UpgradePanelText");
      Transform pearlCostTransform = upgradePanel.Find("PearlCostText");
      Transform oreCostTransform = upgradePanel.Find("OreCostText");
      Transform explanationTransform = upgradePanel.Find("ExplanationText");

      TextMeshProUGUI title = titleTextTransform != null ? titleTextTransform.GetComponent<TextMeshProUGUI>() : null;
      TextMeshProUGUI upgradeText = mainTextTransform != null ? mainTextTransform.GetComponent<TextMeshProUGUI>() : null;
      TextMeshProUGUI pearlCostText = pearlCostTransform != null ? pearlCostTransform.GetComponent<TextMeshProUGUI>() : null;
      TextMeshProUGUI oreCostText = oreCostTransform != null ? oreCostTransform.GetComponent<TextMeshProUGUI>() : null;
      TextMeshProUGUI expText = explanationTransform != null ? explanationTransform.GetComponent<TextMeshProUGUI>() : null;

      if (oreLevel == 1)
      {
         pearlUpgradeCost = LEVEL_2_PEARL_COST;
         oreUpgradeCost = LEVEL_2_ORE_COST;
         upgradeTitle = "REWARD: Increase Ore/Turn";
         upgradeExplanation = "The ore gained every turn increases to 25 but increases jamming";
      }
      if (oreLevel == 2)
      {
         pearlUpgradeCost = LEVEL_3_PEARL_COST;
         oreUpgradeCost = LEVEL_3_ORE_COST;
         upgradeTitle = "REWARD: Increase Ore/Turn";
         upgradeExplanation = "The ore gained every turn increases to 60 but increases jamming";
      }

      if (oreLevel < ENDING_LEVEL)
      {
         Debug.Log(oreLevel);
         if (title != null)
            title.text = $"LEVEL {oreLevel +1} UPGRADE";
         if (upgradeText != null)
            upgradeText.text = upgradeTitle;

         if (pearlCostText != null)
            pearlCostText.text = $"{pearlUpgradeCost}";

         if (oreCostText != null)
            oreCostText.text = $"{oreUpgradeCost}";

         if (expText != null)
         {
            expText.gameObject.SetActive(true);
            expText.text = $"<color=black>{upgradeExplanation}</color>";
         }

         Transform yesBtn = upgradePanel.Find("YesButton");
         if (yesBtn != null) yesBtn.gameObject.SetActive(true);
      }
      else
      {
         if (upgradeText != null)
            upgradeText.text = "Max Level Reached!";

         if(pearlCostText != null)
            pearlCostText.gameObject.SetActive(false);

         if (oreCostText != null)
            oreCostText.gameObject.SetActive(false);

         if (expText != null)
            expText.gameObject.SetActive(false); // Hide explanation if max level

         Transform yesBtn = upgradePanel.Find("YesButton");
         if (yesBtn != null) yesBtn.gameObject.SetActive(false);
      }

      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(false);
   }

   private void CloseInfoPanel()
   {
      panelManager.ClosePanel(infoPanel.gameObject);

      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(true);
   }
   private void CloseUpgradePanel()
   {
      panelManager.ClosePanel(upgradePanel.gameObject);

      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(true);
   }

   private void CalculateRefineryValues()
   {
      switch (oreLevel)
      {
         case 1:
            Debug.Log($"Ore Refinery Level 1: Produces 10 Ore per turn.");
            CurrentOreProduction = 10;
            NextUpgradeCostInPearls = level2PearlCost;
            NextUpgradeCostInOre = level2OreCost;
            break;
         case 2:
            Debug.Log($"Ore Refinery Level 2: Produces 25 Ore per turn.");
            CurrentOreProduction = 25;
            NextUpgradeCostInPearls = level3PearlCost;
            NextUpgradeCostInOre = level3OreCost;
            break;
         case 3:
            Debug.Log("Ore Refinery Level 3: Produces 60 Ore per turn. MAX LEVEL.");
            CurrentOreProduction = 60;
            NextUpgradeCostInPearls = 0;
            NextUpgradeCostInOre = 0;
            break;
         default:
            Debug.Log("Unknown Ore Refinery Level.");
            break;
      }
   }

   public void ActivateJamButton()
   {
      buildingCanvas.transform.Find("JamButton").gameObject.SetActive(true);
      buildingCanvas.transform.Find("JamButton").GetComponent<Button>().onClick.AddListener(() => OpenJamPanel());
   }

   public void ActivateJamSymbol()
   {
      buildingCanvas.transform.Find("JammedSymbol").gameObject.SetActive(true);
   }

   public void OpenJamPanel()
   {
      jamPanel.SetActive(true);

      // Get the buttons
      Button exitBtn = jamPanel.transform.Find("ExitButton").GetComponent<Button>();
      Button unjamBtn = jamPanel.transform.Find("PayButtons/UnjamButton").GetComponent<Button>();
      Button payItBtn = jamPanel.transform.Find("PayButtons/PayItButton").GetComponent<Button>();
      Button waitBtn  = jamPanel.transform.Find("PayButtons/WaitButton").GetComponent<Button>();

      // Clear and Re-assign Exit Button
      exitBtn.onClick.RemoveAllListeners();
      exitBtn.onClick.AddListener(() => CloseJamPanel());

      // Clear and Re-assign Patch Kit Button
      unjamBtn.onClick.RemoveAllListeners();
      unjamBtn.onClick.AddListener(() => PayForUnjamming(1));

      // Clear and Re-assign Pay Button
      payItBtn.onClick.RemoveAllListeners();
      payItBtn.onClick.AddListener(() => PayForUnjamming(2));

      waitBtn.onClick.RemoveAllListeners();
      waitBtn.onClick.AddListener(() => PayForUnjamming(3));

      PopUpManager.Instance.DisablePlayerInput();
   }

   public void PayForUnjamming(int paymentType)
   {
      if (paymentType == 1)
      {
         if (InventoryManager.Instance.TryUsePatchKit(1))
         {
            IsBlocked = false;
            buildingCanvas.transform.Find("JammedSymbol").gameObject.SetActive(false);
            DeactivateJamButton();
            CloseJamPanel();
            Debug.Log("Ore Refinery unjammed successfully.");
            ticker.ShowTicker("Ore Refinery unjammed successfully.", Color.green, MessageTypes.ResultMessage);
         } 
         else
         {
            Debug.Log("Not enough Patch Kits to unjam the Ore Refinery.");
         }
      }
      else if (paymentType == 2) 
      {
         if (InventoryManager.Instance.TrySpendPearl(30))
         {
            IsBlocked = false;
            buildingCanvas.transform.Find("JammedSymbol").gameObject.SetActive(false);
            DeactivateJamButton();
            CloseJamPanel();
            ticker.ShowTicker("Ore Refinery unjammed successfully.", Color.green, MessageTypes.ResultMessage);
            Debug.Log("Ore Refinery unjammed successfully.");
         }
         else
         {
            Debug.Log("Not enough Pearls to unjam the Ore Refinery.");
         }
      }
      else
      {
         manualResetOption = true;
         CloseJamPanel();
         ticker.ShowTicker("Manual Counter started", Color.green, MessageTypes.ResultMessage);
      }
   }

   // Reduces jamming percentage when user unlocks tier 1 in lab
   public void ReduceJamming(int oreAmount)
   {
      jammingChance -= oreAmount;

      if (jammingChance < 0)
         jammingChance = 0;

      Debug.Log($"Refinery improved! Jamming chance is now {jammingChance}%");
      ticker.ShowTicker($"Refinery improved! Jamming chance is now {jammingChance}%", Color.green, MessageTypes.ResultMessage);
   }

   private void ProduceOres()
   {
      int roll = UnityEngine.Random.Range(0, 100);

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
      buildingCanvas.transform.Find("JamButton").GetComponent<Button>().onClick.RemoveListener(() => OpenJamPanel());
      buildingCanvas.transform.Find("JamButton").gameObject.SetActive(false);

   }

   public void UpgradeOreRefinery()
   {
      if (oreLevel >= ENDING_LEVEL)
      {
         Debug.Log("Ore Refinery is already at max level.");
         ticker.ShowTicker("Ore Refinery is already at max level.", Color.white, MessageTypes.ResultMessage);
         return;
      }

      if (InventoryManager.Instance.pearlCount >= NextUpgradeCostInPearls && InventoryManager.Instance.oreCount >= NextUpgradeCostInOre)
      {
         InventoryManager.Instance.TrySpendPearl(NextUpgradeCostInPearls);
         InventoryManager.Instance.TrySpendOre(NextUpgradeCostInOre);
         
         oreLevel += 1;
         UpdateOreRefinerySprites();
         CalculateRefineryValues();

         oreRefineryLevelText.text = "Level " + oreLevel.ToString();
         Debug.Log($"Ore Refinery upgraded to level {oreLevel}!");
         ticker.ShowTicker($"Ore Refinery upgraded to level {oreLevel}!", Color.green, MessageTypes.ResultMessage);

         if (oreLevel == ENDING_LEVEL)
         {
            InventoryManager.Instance.OreRefineryUpgradeIcon.gameObject.SetActive(false);
         }
         CloseOreRefinoryPanel(UPGRADE_BUTTON);
      }
      else
      {
         Debug.Log("Not enough resources to upgrade the Ore Refinery.");
         ticker.ShowTicker("Not enough Pearls to upgrade the Ore Refinery.", Color.red, MessageTypes.ResultMessage);
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
      buildingCanvas.transform.Find("JammedSymbol").gameObject.SetActive(false);
   }

   private void UpdateOreRefinerySprites()
   {
      int index = oreLevel - 1;

      if (buildingSpriteRenderer != null && index < oreLevelSprites.Count)
      {
         buildingSpriteRenderer[index - 1].gameObject.SetActive(false);
         buildingSpriteRenderer[index].gameObject.SetActive(true);
         Debug.Log($"Forge Visuals Updated to Level {oreLevel}");
      }
   }
}