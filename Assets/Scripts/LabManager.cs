// libraries                                                                                     
using System;
using System.ComponentModel;
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
   public const int T2_PROD_HARPOON = 2;
   public const int T3_PROD_ENGINE = 1;
   public const int T1_EXPL_HARPOON = 1;
   public const int T2_EXPL_DIVING_BELL = 1;
   public const int T3_EXPL_DIVING_BELL = 2;

   public const int LAB_TUTORIAL = 1;
   public const int VICTORY_TUTORIAL = 2;

   /* Inspector Variables                                                                       */
   [SerializeField] private Transform innovatePanel;
   [SerializeField] private Transform infoPanel;

   [SerializeField] private GameObject pathButtons;
   [SerializeField] private GameObject initialTab;
   [SerializeField] private GameObject commerceTab;
   [SerializeField] private GameObject productionTab;
   [SerializeField] private GameObject explorationTab;
                    private TickerSystem ticker;


   // Public variables                                                                          
   public static int  currentCommerceTier { get; set; } = 0;
   public static bool headUnlocked            = false;
   public static bool bodyUnlocked            = false;
   public static bool tailUnlocked            = false;
   public        bool labTutorialFunction     = false;
   public        bool victoryTutorialFunction = false;

   // Private instances
   TradeHutManager  tradeHutManager;
   ShipManager      shipManager;
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


      if (TradeHutManager.Instance == null)
         Debug.LogError("Trade Hut instance is not initialized");
      else
         tradeHutManager = TradeHutManager.Instance;
      
      if (InventoryManager.Instance == null)
         Debug.LogError("Inventory instance is not initialized");
      else
         inv    = InventoryManager.Instance;
      
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
      innovatePanel.gameObject.SetActive(true);

      if (labTutorialFunction)
      {
         initialTab.transform.Find("LabTutorialText").gameObject.SetActive(true);
         pathButtons.transform.Find("commercePath").GetComponent<Button>().onClick.AddListener(() => ShowPath(commerceTab));
      }
      else
      {
         pathButtons.transform.Find("commercePath").GetComponent<Button>().onClick.AddListener(() => ShowPath(commerceTab));
         pathButtons.transform.Find("productionPath").GetComponent<Button>().onClick.AddListener(() => ShowPath(productionTab));
         pathButtons.transform.Find("explorationPath").GetComponent<Button>().onClick.AddListener(() => ShowPath(explorationTab));
      }

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
       Button t1Btn = tab.transform.Find("buttonContainer/tierOneButton").GetComponent<Button>();
       Button t2Btn = tab.transform.Find("buttonContainer/tierTwoButton").GetComponent<Button>();
       Button t3Btn = tab.transform.Find("buttonContainer/tierThreeButton").GetComponent<Button>();
   
       // Clear and Re-assign
       backBtn.onClick.RemoveAllListeners();
       backBtn.onClick.AddListener(() => BackToInitialTab(tab));
   
       t1Btn.onClick.RemoveAllListeners();
       t1Btn.onClick.AddListener(() => HandleInnovation(tab, TIER_ONE));
   
       t2Btn.onClick.RemoveAllListeners();
       t2Btn.onClick.AddListener(() => HandleInnovation(tab, TIER_TWO));
   
       t3Btn.onClick.RemoveAllListeners();
       t3Btn.onClick.AddListener(() => HandleInnovation(tab, TIER_THREE));
   }

   // Handle the innovation purchase and unlocking of the next tier node upon clicking the buy button
   private void HandleInnovation(GameObject tab, int tier)
   {
      string requiredItem = "";
      int    pearlCost    = 0, 
             itemCost     = 0;

      Func<int, bool> useItemMethod = null;

      if (tab == commerceTab)
      {
         switch (tier)
         {
            case TIER_ONE:
               pearlCost     = T1_COMM_PEARL;
               itemCost      = T1_COMM_CRUDE_TOOL;
               requiredItem  = "Crude Tool";
               useItemMethod = inv.TryUseCrudeTool;
               break;
            case TIER_TWO:
               pearlCost     = T2_COMM_PEARL;
               itemCost      = T2_COMM_PRESSURE_VALVE;
               requiredItem  = "Pressure Valve";
               useItemMethod = ForgeManager.Instance.hasTier2Blueprint ? inv.TryUsePressureValve : null;
               break;
            case TIER_THREE:
               pearlCost     = T3_COMM_PEARL;
               itemCost      = T3_COMM_LENS;
               requiredItem  = "Precision Lens";
               useItemMethod = ForgeManager.Instance.hasTier3Blueprint ? inv.TryUsePrecisionLens : null;
               break;
         }
      }
      else if (tab == productionTab)
      {
         switch (tier)
         {
            case TIER_ONE:
               pearlCost     = T1_PROD_PEARL;
               itemCost      = T1_PROD_PATCH_KIT;
               requiredItem  = "Patch Kit";
               useItemMethod = inv.TryUsePatchKit;
               break;
            case TIER_TWO:
               pearlCost     = T2_PROD_PEARL;
               itemCost      = T2_PROD_HARPOON;
               requiredItem  = "Harpoon";
               useItemMethod = inv.TryUseHarpoon;
               break;
            case TIER_THREE:
               pearlCost     = T3_PROD_PEARL;
               itemCost      = T3_PROD_ENGINE;
               requiredItem  = "Engine";
               useItemMethod = inv.TryUseEngine;
               break;
         }
      }
      else if (tab == explorationTab)
      {
         switch (tier)
         {
            case TIER_ONE:
               pearlCost     = T1_EXPL_PEARL;
               itemCost      = T1_EXPL_HARPOON;
               requiredItem  = "Harpoon";
               useItemMethod = inv.TryUseHarpoon;
               break;
            case TIER_TWO:
               pearlCost     = T2_EXPL_PEARL;
               itemCost      = T2_EXPL_DIVING_BELL;
               requiredItem  = "Diving Bell";
               useItemMethod = inv.TryUseDivingBell;
               break;
            case TIER_THREE:
               pearlCost     = T3_EXPL_PEARL;
               itemCost      = T3_EXPL_DIVING_BELL;
               requiredItem  = "Diving Bell";
               useItemMethod = inv.TryUseDivingBell;
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
               tab.transform.Find("branch/firstConnector/unfilledConnector").gameObject.SetActive(false);
               tab.transform.Find("branch/firstConnector/filledConnector").gameObject.SetActive(true);
               tab.transform.Find("branch/tierNodeOneContainer/tierNodeOneUnfilled").gameObject.SetActive(false);
               tab.transform.Find("branch/tierNodeOneContainer/tierNodeOneFilled").gameObject.SetActive(true);
               tab.transform.Find("buttonContainer/tierOneButton").gameObject.SetActive(false);
               tab.transform.Find("costContainer/tierOneCost").gameObject.SetActive(false);
               tab.transform.Find("costContainer/tierOneImages").gameObject.SetActive(false);
               break;
            case TIER_TWO:
               ImplementTierTwoInnovation(tab);
               UnlockNextNode(tab, 3);
               tab.transform.Find("branch/secondConnector/unfilledConnector").gameObject.SetActive(false);
               tab.transform.Find("branch/secondConnector/filledConnector").gameObject.SetActive(true);
               tab.transform.Find("branch/tierNodeTwoContainer/tierNodeTwoUnfilled").gameObject.SetActive(false);
               tab.transform.Find("branch/tierNodeTwoContainer/tierNodeTwoFilled").gameObject.SetActive(true);
               tab.transform.Find("buttonContainer/tierTwoButton").gameObject.SetActive(false);
               tab.transform.Find("costContainer/tierTwoCost").gameObject.SetActive(false);
               tab.transform.Find("costContainer/tierTwoImages").gameObject.SetActive(false);
               break;
            case TIER_THREE:
               ImplementTierThreeInnovation(tab);
               tab.transform.Find("branch/thirdConnector/unfilledConnector").gameObject.SetActive(false);
               tab.transform.Find("branch/thirdConnector/filledConnector").gameObject.SetActive(true);
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
      
      if(useItemMethod == null) 
      {
         Debug.LogError("Use item methods was not found.");
         ticker.ShowTicker($"{itemName}s have not been unlocked!", Color.red, TickerSystem.MessageTypes.ResultMessage);

      } else
      {
         // Link to inventory to spend the item
         if((inv.pearlCount >= pearlCost))
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
         headUnlocked = true;

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
         bodyUnlocked = true;

         ticker.ShowTicker("Product Branch Tier 1 unlocked", Color.green, TickerSystem.MessageTypes.ResultMessage);
      }
      /* Ships have health and fuel increased                                                    */
      else if (tabType == explorationTab)
      {
         if (shipManager != null)
            shipManager.ApplyLabShipBonus();

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
         tradeHutManager.RecycleButton.gameObject.SetActive(true);

      // Unlock tier 2 item (reinforces component); forge now has 5% chance to produce a
      //    bonus item upon crafting a single item                                             
      else if (tabType == productionTab)
      {
         Debug.Log("Unlock Overclock and Tier 2 Blueprints");
         if (ForgeManager.Instance != null)
         {
            ForgeManager.Instance.UnlockOverclock();
         }
      }
      /* Unlocks chance to find crafts on explorations                                         */
      else if (tabType == explorationTab)
         if (shipManager != null)
            shipManager.UnlockTier2Choices();
         else
         {
            Debug.Log("There is no tab");
         }
   }

   // Permanently increase the sell value of all items by 20%, remove negative world events from the trade hut, and unlock tier 3 innovation
   public void ImplementTierThreeInnovation(GameObject tabType)
   {
      int crudeToolSellValueIncrease = Mathf.CeilToInt(GetItemValue(ItemType.CrudeTool) * 1.2f) - GetItemValue(ItemType.CrudeTool),
          harpoonSellValueIncrease = Mathf.CeilToInt(GetItemValue(ItemType.Harpoon) * 1.2f) - GetItemValue(ItemType.Harpoon),
          pressureValveSellValueIncrease = Mathf.CeilToInt(GetItemValue(ItemType.PressureValve) * 1.2f) - GetItemValue(ItemType.PressureValve),
          engineSellValueIncrease = Mathf.CeilToInt(GetItemValue(ItemType.Engine) * 1.2f) - GetItemValue(ItemType.Engine);

      if (tabType == commerceTab)
      {
         // Removes the negative world events
         tradeHutManager.isTier3BuffACtive = true;

         // Raises sell items base price by 1.2 
         TryIncreaseCrudeToolSellValue(crudeToolSellValueIncrease);
         TryIncreaseHarpoonSellValue(harpoonSellValueIncrease);
         TryIncreasePressureValveValue(pressureValveSellValueIncrease);
         TryIncreaseEngineSellValue(engineSellValueIncrease);
      }
      // Unlock tier 3 itme (Artifact); Crafting results in two items being made               
      else if (tabType == productionTab)
      {
         Debug.Log("Unlock Faster Crafting and Tier 3 Blueprints");
         if (ForgeManager.Instance != null)
         {
            ForgeManager.Instance.UnlockReduceCraftingTime();
         }
      }
      /* Double exploration rewards                                                            */
      else if (tabType == explorationTab)
      {
         if (shipManager != null)
            shipManager.ApplyLabRewardBonus();
      }
      else
      {
         Debug.Log("There is no tab");
      }
   }

   // Unlock the next tier node upon buying the previous tier node                             
   public void UnlockNextNode(GameObject tab, int tier)
   {
      Color currentColor;

      // Get rid of the tier 2 lock and turn on buttons and text                               
      if (tier == 2)
      {
         tab.transform.Find("lockContainer/tierTwoLock").gameObject.SetActive(false);

         currentColor = tab.transform.Find("buttonContainer/tierTwoButton").GetComponent<Image>().color;
         currentColor.a = 1.0f;
         tab.transform.Find("buttonContainer/tierTwoButton").GetComponent<Image>().color = currentColor;
         tab.transform.Find("buttonContainer/tierTwoButton").GetComponent<Button>().interactable = true;

         currentColor = tab.transform.Find("costContainer/tierTwoCost").GetComponent<TextMeshProUGUI>().color;
         currentColor.a = 1.0f;
         tab.transform.Find("costContainer/tierTwoCost").GetComponent<TextMeshProUGUI>().color = currentColor;

      }
      // Get ride of the tier 3 lock and turn on buttons and text                              
      else
      {
         if (tier != 3)
         {
            Debug.Log("Accessing wrong tier node");
            return;
         }
         tab.transform.Find("lockContainer/tierThreeLock").gameObject.SetActive(false);

         currentColor = tab.transform.Find("buttonContainer/tierThreeButton").GetComponent<Image>().color;
         currentColor.a = 255;
         tab.transform.Find("buttonContainer/tierThreeButton").GetComponent<Image>().color = currentColor;
         tab.transform.Find("buttonContainer/tierThreeButton").GetComponent<Button>().interactable = true;

         currentColor = tab.transform.Find("costContainer/tierThreeCost").GetComponent<TextMeshProUGUI>().color;
         currentColor.a = 255;
         tab.transform.Find("costContainer/tierThreeCost").GetComponent<TextMeshProUGUI>().color = currentColor;

      }
   }

   // Return to the initial tab upon clicking the back arrow button                            
   private void BackToInitialTab(GameObject tab)
   {
      tab.gameObject.SetActive(false);
      initialTab.gameObject.SetActive(true);
   }

   // Open up the info panel                                                                   
   private void ShowInfoPanel()
   {
      infoPanel.gameObject.SetActive(true);

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
      innovatePanel.gameObject.SetActive(false);

      if (labTutorialFunction)
      {
         commerceTab.transform.Find("LabTutorialText").gameObject.SetActive(false);
         labTutorialFunction = false;
      }

      pathButtons.transform.Find("commercePath").GetComponent<Button>().onClick.RemoveAllListeners();
      pathButtons.transform.Find("productionPath").GetComponent<Button>().onClick.RemoveAllListeners();
      pathButtons.transform.Find("explorationPath").GetComponent<Button>().onClick.RemoveAllListeners();
      

      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(true);
   }

   // Close the info panel                                                                     
   private void CloseInfoPanel()
   {
      infoPanel.gameObject.SetActive(false);

      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(true);
   }
}