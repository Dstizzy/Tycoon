// libraries                                                                                     
using System;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using static Item;

public class LabManager : MonoBehaviour
{
   // Symbolic Constants                                                                        
   public const int INNOVATE_BUTTON = 1;
   public const int INFO_BUTTON = 2;
   public const int UPGRADE_BUTTON = 3;
   public const int TIER_ONE = 1;
   public const int TIER_TWO = 2;
   public const int TIER_THREE = 3;

   public const int T1_COMM_PEARL = 100;
   public const int T1_PROD_PEARL = 150;
   public const int T1_EXPL_PEARL = 200;
   public const int T2_COMM_PEARL = 300;
   public const int T2_PROD_PEARL = 400;
   public const int T2_EXPL_PEARL = 500;
   public const int T3_COMM_PEARL = 800;
   public const int T3_PROD_PEARL = 1000;
   public const int T3_EXPL_PEARL = 1200;

   public const int T1_COMM_CRUDE_TOOL = 1;
   public const int T2_COMM_PRESSURE_VALVE = 2;
   public const int T3_COMM_LENS = 1;

   public const int T1_PROD_PATCH_KIT = 1;
   public const int T2_PROD_PRESSURE_VALVE = 2;
   public const int T3_PROD_LENS = 1;
   public const int T1_EXPL_HARPOON = 1;
   public const int T2_EXPL_DIVING_BELL = 1;
   public const int T3_EXPL_DIVING_BELL = 2;

   public const int LAB_TUTORIAL = 1;
   public const int VICTORY_TUTORIAL = 2;

   // Inspector Variables
   public PanelManager panelManager;
   public Button victoryButton;
   [SerializeField] private Transform innovatePanel;
   [SerializeField] private Transform infoPanel;

   [SerializeField] private GameObject pathButtons;
   [SerializeField] private GameObject initialTab;
   [SerializeField] private GameObject commerceTab;
   [SerializeField] private GameObject productionTab;
   [SerializeField] private GameObject explorationTab;
   [SerializeField] private GameObject victoryPanel;
   private TickerSystem ticker;

   [Header("Head Victory UI")]
   [SerializeField] private TextMeshProUGUI headTextLabel;
   [SerializeField] private Image headLineImage;
   [SerializeField] private TextMeshProUGUI headInfoLabel;
   [SerializeField] private GameObject headCompletedContainer;
   [SerializeField] private Image headCheckMarkImage;

   [Header("Tail Victory UI")]
   [SerializeField] private TextMeshProUGUI tailTextImage;
   [SerializeField] private Image tailLineImage;
   [SerializeField] private TextMeshProUGUI tailInfoImage;
   [SerializeField] private GameObject tailCompletedContainer;
   [SerializeField] private Image tailCheckMarkImage;

   [Header("Submarine Parts")]
   [SerializeField] private GameObject submarineFull;
   [SerializeField] private GameObject submarineSkelParent;
   [SerializeField] private GameObject submarineBlackParent;
   [SerializeField] private GameObject submarineSkelBody;
   [SerializeField] private GameObject submarineSkelHead;
   [SerializeField] private GameObject submarineSkelTail;
   [SerializeField] private GameObject submarineBlackedOutHead;
   [SerializeField] private GameObject submarineBlackedOutTail;


   // Public variables
   public static int  currentCommerceTier { get; set; } = 0;
   public static bool headUnlocked            = false;
   public static bool tailUnlocked            = false;
   private       bool commerceFinished        = false;
   private       bool productionFinished      = false;
   private       bool explorationFinished     = false;
   public        bool labTutorialFunction     = false;
   public        bool victoryTutorialFunction = false;
   private       bool techTreeCompleted;

   // Private instances
   TradeHutManager tradeHutManager;
   ShipManager shipManager;
   InventoryManager inv;

   public static LabManager labManager { get; set; }

   // Check if all required game objects exist and are in there required states                 
   private void Awake()
   {

      if (labManager != null && labManager != this)
         Destroy(this.gameObject);
      else
      {
         labManager = this;
         DontDestroyOnLoad(this.gameObject);
      }

      victoryButton.onClick.AddListener(() => {
         AudioManager.Instance.PlayClick();
         ShowVictoryPanel();
      });

      if (victoryButton != null)
      {
         victoryPanel.transform.Find("BackArrow").GetComponent<Button>().onClick.AddListener(() =>
         {
            AudioManager.Instance.PlayClick();
            panelManager.ClosePanel(victoryPanel.gameObject);
            PopUpManager.Instance.EnablePlayerInput();
         });
      }

      if (TradeHutManager.Instance == null)
         Debug.LogError("Trade Hut instance is not initialized");
      else
         tradeHutManager = TradeHutManager.Instance;

      if (InventoryManager.Instance == null)
         Debug.LogError("Inventory instance is not initialized");
      else
         inv = InventoryManager.Instance;

      if (ShipManager.Instance == null)
         Debug.LogError("Ship Manager instance is not initialized");
      else
         shipManager = ShipManager.Instance;

      if (TickerSystem.Instance == null)
         Debug.LogError("Ticker instance is not initialized");
      else
         ticker = TickerSystem.Instance;


      /* Set the info panel to inactive if it exists                                           */
      if (infoPanel == null)
      {
         Debug.LogError("Info Panel is not assigned in the Inspector!");
      }
      else
      {
         infoPanel.gameObject.SetActive(false);
      }

      // Set the research panel to inactive if it exists                                       
      if (innovatePanel == null)
      {
         Debug.LogError("Innovate Panel is not assigned");
      }
      else
      {
         innovatePanel.gameObject.SetActive(false);
      }

      // Set the research panel to inactive if it exists                                       
      if (initialTab == null)
      {
         Debug.LogError("Commerce Tab is not assigned");
      }
      else
      {
         initialTab.gameObject.SetActive(true);
      }

      // Set the research panel to inactive if it exists                                       
      if (commerceTab == null)
      {
         Debug.LogError("Commerce Tab is not assigned");
      }
      else
      {
         commerceTab.gameObject.SetActive(false);
      }

      // Set the research panel to inactive if it exists                                       
      if (productionTab == null)
      {
         Debug.LogError("Production Tab is not assigned");
      }
      else
      {
         productionTab.gameObject.SetActive(false);
      }

      // Set the research panel to inactive if it exists
      if (explorationTab == null)
      {
         Debug.LogError("Commerce Tab is not assigned");
      }
      else
      {
         explorationTab.gameObject.SetActive(false);
      }

      commerceFinished = false;
      productionFinished = false;
      explorationFinished = false;
      headUnlocked = false;
      tailUnlocked = false;
   }

   // Open up a lab panel upon clicking the corresponding button                                
   public void RequestLabPanel(int buttonID)
   {
      switch (buttonID)
      {
         case INNOVATE_BUTTON:
            ShowInnovatePanel();
            innovatePanel.transform.Find("ExitButton").GetComponent<Button>().onClick.AddListener(() => CloseLabPanel(INNOVATE_BUTTON));
            break;
         case INFO_BUTTON:
            ShowInfoPanel();
            infoPanel.transform.Find("ExitButton").GetComponent<Button>().onClick.AddListener(() => CloseLabPanel(INFO_BUTTON));
            break;
         default:
            Debug.Log("Building Panel: Unknown button ID.");
            break;
      }
   }

   // Close the lab panel upon clicking the exit button                                         
   public void CloseLabPanel(int buttonID)
   {
      switch (buttonID)
      {
         case INNOVATE_BUTTON:
            CloseInnovatePanel();
            break;
         case INFO_BUTTON:
            CloseInfoPanel();
            break;
         default:
            Debug.Log("Building Panel: Unknown button ID.");
            break;
      }
      PopUpManager.Instance.EnablePlayerInput();
   }

   // Open up the research panel and assign the buttons in the initial panel                    
   private void ShowInnovatePanel()
   {
      panelManager.OpenPanel(innovatePanel.gameObject);
      innovatePanel.gameObject.SetActive(true);

      pathButtons.transform.Find("commercePath").GetComponent<Button>().onClick.AddListener(() => ShowPath(commerceTab));
      pathButtons.transform.Find("productionPath").GetComponent<Button>().onClick.AddListener(() => ShowPath(productionTab));
      pathButtons.transform.Find("explorationPath").GetComponent<Button>().onClick.AddListener(() => ShowPath(explorationTab));


      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(false);
   }

   // Show the corresponding path tab upon clicking the path button
   private void ShowPath(GameObject tab)
   {
      initialTab.gameObject.SetActive(false);
      tab.gameObject.SetActive(true);

      // Get the buttons
      Button backBtn = tab.transform.Find("backArrow").GetComponent<Button>();
      backBtn.onClick.RemoveAllListeners();
      backBtn.onClick.AddListener(() => BackToInitialTab(tab));
      Button t1Btn = tab.transform.Find("buttonContainer/tierOneButton").GetComponent<Button>();
      Button t2Btn = tab.transform.Find("buttonContainer/tierTwoButton").GetComponent<Button>();
      Button t3Btn = tab.transform.Find("buttonContainer/tierThreeButton").GetComponent<Button>();

      t1Btn.onClick.RemoveAllListeners();
      t1Btn.onClick.AddListener(() => HandleInnovation(tab, TIER_ONE));

      t2Btn.onClick.RemoveAllListeners();
      t2Btn.onClick.AddListener(() => HandleInnovation(tab, TIER_TWO));

      t3Btn.onClick.RemoveAllListeners();
      t3Btn.onClick.AddListener(() => HandleInnovation(tab, TIER_THREE));
   }

   // Handle the innovation purchase and unlocking of the next tier node upon clicking the buy button
   // Handle the innovation purchase and unlocking of the next tier node upon clicking the buy button
   private void HandleInnovation(GameObject tab, int tier)
   {
      string requiredItem = "";
      int pearlCost = 0,
             itemCost = 0;

      Func<int, bool> useItemMethod = null;

      if (tab == commerceTab)
      {
         switch (tier)
         {
            case TIER_ONE:
               pearlCost = T1_COMM_PEARL;
               itemCost = T1_COMM_CRUDE_TOOL;
               requiredItem = "Crude Tool";
               useItemMethod = inv.TryUseCrudeTool;
               break;
            case TIER_TWO:
               pearlCost = T2_COMM_PEARL;
               itemCost = T2_COMM_PRESSURE_VALVE;
               requiredItem = "Pressure Valve";
               // Requires Tier 2 Blueprint
               useItemMethod = ForgeManager.Instance.hasTier2Blueprint ? inv.TryUsePressureValve : null;
               break;
            case TIER_THREE:
               pearlCost = T3_COMM_PEARL;
               itemCost = T3_COMM_LENS;
               requiredItem = "Precision Lens";
               // Requires Tier 3 Blueprint
               useItemMethod = ForgeManager.Instance.hasTier3Blueprint ? inv.TryUsePrecisionLens : null;
               break;
         }
      }
      else if (tab == productionTab)
      {
         switch (tier)
         {
            case TIER_ONE:
               pearlCost = T1_PROD_PEARL;
               itemCost = T1_PROD_PATCH_KIT;
               requiredItem = "Patch Kit";
               useItemMethod = inv.TryUsePatchKit;
               break;
            case TIER_TWO:
               pearlCost = T2_PROD_PEARL;
               itemCost = T2_PROD_PRESSURE_VALVE;
               requiredItem = "Pressure Valve";
               // Requires Tier 2 Blueprint
               useItemMethod = ForgeManager.Instance.hasTier2Blueprint ? inv.TryUsePressureValve : null;
               break;
            case TIER_THREE:
               pearlCost = T3_PROD_PEARL;
               itemCost = T3_PROD_LENS;
               requiredItem = "Precision Lens";
               // Requires Tier 3 Blueprint
               useItemMethod = ForgeManager.Instance.hasTier3Blueprint ? inv.TryUsePrecisionLens : null;
               break;
         }
      }
      else if (tab == explorationTab)
      {
         switch (tier)
         {
            case TIER_ONE:
               pearlCost = T1_EXPL_PEARL;
               itemCost = T1_EXPL_HARPOON;
               requiredItem = "Harpoon";
               useItemMethod = inv.TryUseHarpoon;
               break;
            case TIER_TWO:
               pearlCost = T2_EXPL_PEARL;
               itemCost = T2_EXPL_DIVING_BELL;
               requiredItem = "Diving Bell";
               // Requires Tier 2 Blueprint
               useItemMethod = ForgeManager.Instance.hasTier2Blueprint ? inv.TryUseDivingBell : null;
               break;
            case TIER_THREE:
               pearlCost = T3_EXPL_PEARL;
               itemCost = T3_EXPL_DIVING_BELL;
               requiredItem = "Diving Bell";
               // Requires Tier 2 Blueprint
               useItemMethod = ForgeManager.Instance.hasTier2Blueprint ? inv.TryUseDivingBell : null;
               break;
         }
      }

      // Attempt to unlock tiers with the corresponding cost
      if (PerformBuy(pearlCost, itemCost, requiredItem, useItemMethod))
      {
         switch (tier)
         {
            case TIER_ONE:
               ImplementTierOneInnovation(tab);
               UnlockNextNode(tab, 2);
               tab.transform.Find("branch/tierNodeOneContainer/tierNodeOneUnfilled").gameObject.SetActive(false);
               tab.transform.Find("branch/tierNodeOneContainer/tierNodeOneFilled").gameObject.SetActive(true);
               tab.transform.Find("buttonContainer/tierOneButton").gameObject.SetActive(false);
               tab.transform.Find("costContainer/tierOneCost").gameObject.SetActive(false);
               tab.transform.Find("costContainer/tierOneImages").gameObject.SetActive(false);
               break;
            case TIER_TWO:
               ImplementTierTwoInnovation(tab);
               UnlockNextNode(tab, 3);
               tab.transform.Find("branch/tierNodeTwoContainer/tierNodeTwoUnfilled").gameObject.SetActive(false);
               tab.transform.Find("branch/tierNodeTwoContainer/tierNodeTwoFilled").gameObject.SetActive(true);
               tab.transform.Find("buttonContainer/tierTwoButton").gameObject.SetActive(false);
               tab.transform.Find("costContainer/tierTwoCost").gameObject.SetActive(false);
               tab.transform.Find("costContainer/tierTwoImages").gameObject.SetActive(false);
               break;
            case TIER_THREE:
               ImplementTierThreeInnovation(tab);
               tab.transform.Find("branch/tierNodeThreeContainer/TierNodeThreeUnfilled").gameObject.SetActive(false);
               tab.transform.Find("branch/tierNodeThreeContainer/TierNodeThreeFilled").gameObject.SetActive(true);
               tab.transform.Find("buttonContainer/tierThreeButton").gameObject.SetActive(false);
               tab.transform.Find("costContainer/tierThreeCost").gameObject.SetActive(false);
               tab.transform.Find("costContainer/tierThreeImages").gameObject.SetActive(false);
               break;
         }
      }
   }

   /* Spend certain amount of resources and give corresponding innovations                     */
   public bool PerformBuy(int pearlCost, int itemCost, string itemName, Func<int, bool> useItemMethod)
   {
      bool isSuccess = false;

      if (useItemMethod == null)
      {
         Debug.LogError("Use item methods was not found.");
         ticker.ShowTicker($"{itemName}s have not been unlocked!", Color.red, TickerSystem.MessageTypes.ResultMessage);

      }
      else
      {
         // Link to inventory to spend the item
         if (inv.pearlCount >= pearlCost)
         {
            if (useItemMethod(itemCost) && inv.TrySpendPearl(pearlCost))
               isSuccess = true;
            else
            {
               Debug.Log($"Not enough {itemName}s to spend!");
               ticker.ShowTicker($"Not enough {itemName}s to spend!", Color.red, TickerSystem.MessageTypes.ResultMessage);
            }
         }
         else
         {
            Debug.LogError("Not enough pearls to spend");
            ticker.ShowTicker("Not enough pearls to spend", Color.red, TickerSystem.MessageTypes.ResultMessage);
         }
      }

      return isSuccess;
   }

   // Implement the innovation effects and changes to the game based on the innovation purchased
   public void ImplementTierOneInnovation(GameObject tabType)
   {
      // Permanently increase base sale price of all items by 10%                              
      if (tabType == commerceTab)
      {
         currentCommerceTier = TIER_ONE;

         foreach (Transform item in tradeHutManager.SellItems)
            item.Find("NextValue").GetComponent<TextMeshProUGUI>().gameObject.SetActive(true);

         ApplyDiscountToBuyItems(.2f);

         ticker.ShowTicker("Commerce Branch Tier 1 unlocked", Color.green, TickerSystem.MessageTypes.ResultMessage);
      }
      // Permanently reduce gold spent on refinery upkeep by 50%                               
      else if (tabType == productionTab)
      {
         Debug.Log("Reduce ore jamming percentage by 5%");
         if (OreRefinery_Manager.Instance != null)
         {
            OreRefinery_Manager.Instance.ReduceJamming(5);
         }

         ticker.ShowTicker("Product Branch Tier 1 unlocked", Color.green, TickerSystem.MessageTypes.ResultMessage);
      }
      /* Ships have health and fuel increased                                                    */
      else if (tabType == explorationTab)
      {
         if (ShipManager.Instance != null)
            ShipManager.Instance.ApplyLabShipBonus();

         ticker.ShowTicker("Exploration Branch Tier 1 unlocked", Color.green, TickerSystem.MessageTypes.ResultMessage);
      }
      else
      {
         Debug.Log("There is no tab");
      }
   }

   // Permanently increase the chance of getting better rewards from the trade hut and forge, and unlock tier 2 innovation
   public void ImplementTierTwoInnovation(GameObject tabType)
   {
      // Grant action to gameple 50 gold for 60% chance to get 250 back                       
      if (tabType == commerceTab)
      {
         tradeHutManager.RecycleButton.gameObject.SetActive(true);
         ticker.ShowTicker("Commerce Branch Tier 2 unlocked", Color.green, TickerSystem.MessageTypes.ResultMessage);
      }

      // Unlock tier 2 item (reinforces component); forge now has 5% chance to produce a
      //    bonus item upon crafting a single item                                             
      else if (tabType == productionTab)
      {
         Debug.Log("Unlock Overclock and Tier 2 Blueprints");
         if (ForgeManager.Instance != null)
         {
            ForgeManager.Instance.UnlockOverclock();
         }
         ticker.ShowTicker("Product Branch Tier 2 unlocked", Color.green, TickerSystem.MessageTypes.ResultMessage);
      }
      /* Unlocks chance to find crafts on explorations                                         */
      else
      {
         if (tabType == explorationTab)
         {
            if (ShipManager.Instance != null)
               ShipManager.Instance.UnlockTier2Choices();
            else
               Debug.Log("There is no tab");
            ticker.ShowTicker("Exploration Branch Tier 2 unlocked", Color.green, TickerSystem.MessageTypes.ResultMessage);
         }
      }

   }

   // Permanently increase the sell value of all items by 20%, remove negative world events from the trade hut, and unlock tier 3 innovation
   public void ImplementTierThreeInnovation(GameObject tabType)
   {
      if (tabType == commerceTab)
      {
         commerceFinished = true;
         tradeHutManager.isTier3BuffACtive = true;
         ticker.ShowTicker("Commerce Branch Maxed!", Color.yellow, TickerSystem.MessageTypes.ResultMessage);
      }
      else if (tabType == productionTab)
      {
         productionFinished = true;
         ForgeManager.Instance?.UnlockReduceCraftingTime();
         ticker.ShowTicker("Production Branch Maxed!", Color.yellow, TickerSystem.MessageTypes.ResultMessage);
      }
      else if (tabType == explorationTab)
      {
         explorationFinished = true;
         ShipManager.Instance?.ApplyLabRewardBonus();
         ticker.ShowTicker("Exploration Tech Maxed!", Color.yellow, TickerSystem.MessageTypes.ResultMessage);
      }

      // Check if this was the final branch needed for the Head
      CheckTechTreeCompletion();
   }

   private void CheckTechTreeCompletion()
   {
      if (!commerceFinished || !productionFinished || !explorationFinished)
         return;

      Transform panelFlask1 = initialTab.transform.Find("PanelFlask1");
      Transform panelFlask2 = initialTab.transform.Find("PanelFlask2");
      Transform panelFlask3 = initialTab.transform.Find("PanelFlask3");
      Transform panelFlask4 = initialTab.transform.Find("PanelFlask4");

      if (panelFlask1 != null) panelFlask1.gameObject.SetActive(false);
      if (panelFlask2 != null) panelFlask2.gameObject.SetActive(false);
      if (panelFlask3 != null) panelFlask3.gameObject.SetActive(false);
      if (panelFlask4 != null) panelFlask4.gameObject.SetActive(true);

      if (!headUnlocked)
      {
         ticker.ShowTicker("TECH TREE COMPLETE: Submarine Head Acquired!", Color.green, TickerSystem.MessageTypes.ResultMessage);
         ActivateHead();
      }

      if (tailUnlocked && !GameEndingState.HasEndingTriggered)
         ActivateFinalForm();
   }

   // Handles evolution of flask
   public void HandleFlask()
   {
      if (initialTab.transform.Find("PanelFlask1").gameObject.activeSelf)
      {
         initialTab.transform.Find("PanelFlask1").gameObject.SetActive(false);
         initialTab.transform.Find("PanelFlask2").gameObject.SetActive(true);
      }
      else if (initialTab.transform.Find("PanelFlask2").gameObject.activeSelf)
      {
         initialTab.transform.Find("PanelFlask2").gameObject.SetActive(false);
         initialTab.transform.Find("PanelFlask3").gameObject.SetActive(true);
      }
      else if (initialTab.transform.Find("PanelFlask3").gameObject.activeSelf)
      {
         initialTab.transform.Find("PanelFlask3").gameObject.SetActive(false);
         initialTab.transform.Find("PanelFlask4").gameObject.SetActive(true);
      }
      else
         Debug.Log("There is no flask");
   }
   // Unlock the next tier node upon buying the previous tier node                             
   public void UnlockNextNode(GameObject tab, int tier)
   {
      Color currentColor;

      // Get rid of the tier 2 lock and turn on buttons and text                               
      if (tier == 2)
      {
         currentColor = tab.transform.Find("buttonContainer/tierTwoButton").GetComponent<Image>().color;
         currentColor.a = 1.0f;
         tab.transform.Find("buttonContainer/tierTwoButton").GetComponent<Image>().color = currentColor;
         tab.transform.Find("buttonContainer/tierTwoButton").GetComponent<Button>().interactable = true;

         currentColor = tab.transform.Find("costContainer/tierTwoCost").GetComponent<TextMeshProUGUI>().color;
         currentColor.a = 1.0f;
         tab.transform.Find("costContainer/tierTwoCost").GetComponent<TextMeshProUGUI>().color = currentColor;

         Transform tierTwoImages = tab.transform.Find("costContainer/tierTwoImages");
         if (tierTwoImages != null)
            tierTwoImages.gameObject.SetActive(true);
      }
      else
      {
         // Get ride of the tier 3 lock and turn on buttons and text 
         if (tier != 3)
         {
            Debug.Log("Accessing wrong tier node");
            return;
         }

         currentColor = tab.transform.Find("buttonContainer/tierThreeButton").GetComponent<Image>().color;
         currentColor.a = 1.0f;
         tab.transform.Find("buttonContainer/tierThreeButton").GetComponent<Image>().color = currentColor;
         tab.transform.Find("buttonContainer/tierThreeButton").GetComponent<Button>().interactable = true;

         currentColor = tab.transform.Find("costContainer/tierThreeCost").GetComponent<TextMeshProUGUI>().color;
         currentColor.a = 1.0f;
         tab.transform.Find("costContainer/tierThreeCost").GetComponent<TextMeshProUGUI>().color = currentColor;

         Transform tierThreeImages = tab.transform.Find("costContainer/tierThreeImages");
         if (tierThreeImages != null)
            tierThreeImages.gameObject.SetActive(true);
      }
   }

   // Return to the initial tab upon clicking the back arrow button                            
   private void BackToInitialTab(GameObject tab)
   {
      tab.SetActive(false);
      initialTab.SetActive(true);
   }

   // Open up the info panel                                                                   
   private void ShowInfoPanel()
   {
      panelManager.OpenPanel(infoPanel.gameObject);

      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(false);
   }

   // Close the research panel                                                                 
   private void CloseInnovatePanel()
   {
      commerceTab.gameObject.SetActive(false);
      productionTab.gameObject.SetActive(false);
      explorationTab.gameObject.SetActive(false);
      initialTab.gameObject.SetActive(true);
      panelManager.ClosePanel(innovatePanel.gameObject);

      /*if (labTutorialFunction)
      {
         commerceTab.transform.Find("LabTutorialText").gameObject.SetActive(false);
         labTutorialFunction = false;
      }*/

      pathButtons.transform.Find("commercePath").GetComponent<Button>().onClick.RemoveAllListeners();
      pathButtons.transform.Find("productionPath").GetComponent<Button>().onClick.RemoveAllListeners();
      pathButtons.transform.Find("explorationPath").GetComponent<Button>().onClick.RemoveAllListeners();


      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(true);
   }

   // Close the info panel                                                                     
   private void CloseInfoPanel()
   {
      panelManager.ClosePanel(infoPanel.gameObject);

      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(true);
   }

   //Shows the victory panel and sets up the buttons for the submarine assembly
   public void ShowVictoryPanel()
   {
      if (victoryPanel == null) return;
      panelManager.OpenPanel(victoryPanel.gameObject);
      PopUpManager.Instance.DisablePlayerInput();

      if (submarineSkelHead != null) submarineSkelHead.SetActive(headUnlocked);
      if (submarineSkelTail != null) submarineSkelTail.SetActive(tailUnlocked);

      if (submarineBlackedOutHead != null) submarineBlackedOutHead.SetActive(!headUnlocked);
      if (submarineBlackedOutTail != null) submarineBlackedOutTail.SetActive(!tailUnlocked);

      UpdateVictoryUI();

      if (headUnlocked && tailUnlocked)
      {
         ActivateFinalForm();
      }
   }

   // Activates the head of the submarine
   public void ActivateHead()
   {
      headUnlocked = true;
      if (headTextLabel != null) headTextLabel.color = new Color(1, 1, 1, 0.5f);
      if (headLineImage != null) headLineImage.color = new Color(1, 1, 1, 0.5f);
      if (headInfoLabel != null) headInfoLabel.color = new Color(1, 1, 1, 0.5f);
      if (headCompletedContainer != null) headCompletedContainer.SetActive(true);

      if (submarineBlackedOutHead != null) submarineBlackedOutHead.SetActive(false);
      if (submarineSkelHead != null) submarineSkelHead.SetActive(true);

      if (tailUnlocked) ActivateFinalForm();
   }

   private void UpdateVictoryUI()
   {
      // Head UI Feedback
      float headAlpha = headUnlocked ? 0.5f : 1.0f;
      if (headTextLabel != null) headTextLabel.color = new Color(1, 1, 1, headAlpha);
      if (headCompletedContainer != null) headCompletedContainer.SetActive(headUnlocked);

      // Tail UI Feedback
      float tailAlpha = tailUnlocked ? 0.5f : 1.0f;
      if (tailTextImage != null) tailTextImage.color = new Color(1, 1, 1, tailAlpha);
      if (tailCompletedContainer != null) tailCompletedContainer.SetActive(tailUnlocked);
   }

   // Activates the tail of the submarine
   public void ActivateTail()
   {
      tailUnlocked = true;
      ticker.ShowTicker("EXPLORATION COMPLETE: Submarine Tail Acquired!", Color.cyan, TickerSystem.MessageTypes.ResultMessage);
      if (tailTextImage != null) tailTextImage.color = new Color(1, 1, 1, 0.5f);
      if (tailLineImage != null) tailLineImage.color = new Color(1, 1, 1, 0.5f);
      if (tailInfoImage != null) tailInfoImage.color = new Color(1, 1, 1, 0.5f);
      if (tailCompletedContainer != null) tailCompletedContainer.SetActive(true);

      if (submarineBlackedOutTail != null) submarineBlackedOutTail.SetActive(false);
      if (submarineSkelTail != null) submarineSkelTail.SetActive(true);

      if (headUnlocked) ActivateFinalForm();
   }

   // Activates the final form of the submarine when all parts are active
   public void ActivateFinalForm()
   {
      if (submarineBlackParent != null) submarineBlackParent.SetActive(false);
      if (submarineSkelParent != null) submarineSkelParent.SetActive(false);
      if (submarineFull != null) submarineFull.SetActive(true);

      // Trigger ending if all parts are present
      if (!GameEndingState.HasEndingTriggered)
      {
         GameEndingState.LoadSuccessEnding();
      }
   }

   public void UnlockHead()
   {
      victoryPanel.transform.Find("SubInfo/HeadPart").gameObject.SetActive(false);
      victoryPanel.transform.Find("SubInfo/BuySect/HeadPart").gameObject.SetActive(true);

      Button headBuyButton = victoryPanel.transform.Find("SubInfo/BuySect/HeadPart/HeadBuyButton").GetComponent<Button>();
      headBuyButton.onClick.RemoveAllListeners();
      headBuyButton.onClick.AddListener(() =>
      {
         if (InventoryManager.Instance.TrySpendPearl(1000) && InventoryManager.Instance.TryUseEngine(1))
         {
            ActivateHead();
         }
         else
         {
            Debug.Log("Not enough resources to buy head");
            ticker.ShowTicker("Not enough resources to buy head", Color.red, TickerSystem.MessageTypes.ResultMessage);
         }
      });
   }

   public void UnlockTail()
   {
      victoryPanel.transform.Find("SubInfo/TailPart").gameObject.SetActive(false);
      victoryPanel.transform.Find("SubInfo/BuySect/TailPart").gameObject.SetActive(true);
      
      Button tailBuyButton = victoryPanel.transform.Find("SubInfo/BuySect/TailPart/TailBuyButton").GetComponent<Button>();
      tailBuyButton.onClick.RemoveAllListeners();
      tailBuyButton.onClick.AddListener(() =>
      {
         if (InventoryManager.Instance.TrySpendPearl(1000) && InventoryManager.Instance.TryUsePrecisionLens(1))
         {
            ActivateTail();
         }
         else
         {
            Debug.Log("Not enough resources to buy tail");
            ticker.ShowTicker("Not enough resources to buy tail", Color.red, TickerSystem.MessageTypes.ResultMessage);
         }
      });
   }
}