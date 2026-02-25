/* libraries                                                                                     */
using System;
using System.ComponentModel;
using TMPro;

using UnityEngine;
using UnityEngine.UI;

using static Item;

public class LabManager : MonoBehaviour
{
    /* Symbolic Constants                                                                        */
    public const int INNOVATE_BUTTON = 1;
    public const int INFO_BUTTON = 2;
    public const int UPGRADE_BUTTON = 3;
    public const int TIER_ONE = 1;
    public const int TIER_TWO = 2;
    public const int TIER_THREE = 3;
    public const int TIER_ONE_PEARL_COST = 100;
    public const int TIER_ONE_ITEM_COST = 10;
    public const int TIER_TWO_PEARL_COST = 350;
    public const int TIER_TWO_ITEM_COST = 25;
    public const int TIER_THREE_PEARL_COST = 700;
    public const int TIER_THREE_ITEM_COST = 50;
   public const int PROD_T1_PEARL = 400;
   public const int PROD_T1_LENS = 2;
   public const int PROD_T2_PEARL = 1000;
   public const int PROD_T2_LENS = 5;
   public const int PROD_T3_PEARL = 3000;
   public const int PROD_T3_LENS = 10;
   public const int EXPL_T1_PEARL = 0;
   public const int EXPL_T1_ITEM = 0;
   public const int EXPL_T2_PEARL = 0;
   public const int EXPL_T2_ITEM = 0;
   public const int EXPL_T3_PEARL = 0;
   public const int EXPL_T3_ITEM = 0;

   /* Inspector Variables                                                                       */
   [SerializeField] private Transform innovatePanel;
    [SerializeField] private Transform infoPanel;

    [SerializeField] private GameObject pathButtons;
    [SerializeField] private GameObject initialTab;
    [SerializeField] private GameObject commerceTab;
    [SerializeField] private GameObject productionTab;
    [SerializeField] private GameObject explorationTab;
 

   /* Public variables                                                                          */
   public static int currentCommerceTier { get; private set; } = 0;


   TradeHutManager tradeHutManager;
   ShipManager shipManager;
   
   public static LabManager labManager;

    /* Check if all required game objects exist and are in there required states                 */
    private void Awake() 
    {
      if (labManager != null && labManager != this)
         Destroy(this.gameObject);
      else {
         labManager = this;
         DontDestroyOnLoad(this.gameObject);
      }

      tradeHutManager = TradeHutManager.Instance;

      if (tradeHutManager == null)
          Debug.LogError("Insance is not initialized");


        /* Set the info panel to inactive if it exists                                           */
        if (infoPanel == null) 
        {
            Debug.LogError("Info Panel is not assigned in the Inspector!");
        } else 
        {
            infoPanel.gameObject.SetActive(false);
        }

        /* Set the research panel to inactive if it exists                                       */
        if (innovatePanel == null) 
        {
            Debug.LogError("Innovate Panel is not assigned");
        } else 
        {
            innovatePanel.gameObject.SetActive(false);
        }

        /* Set the research panel to inactive if it exists                                       */
        if (initialTab == null)
        {
            Debug.LogError("Commerce Tab is not assigned");
        }
        else
        {
            initialTab.gameObject.SetActive(true);
        }

        /* Set the research panel to inactive if it exists                                       */
        if (commerceTab == null)
        {
            Debug.LogError("Commerce Tab is not assigned");
        }
        else
        {
            commerceTab.gameObject.SetActive(false);
        }

        /* Set the research panel to inactive if it exists                                       */
        if (productionTab == null)
        {
            Debug.LogError("Production Tab is not assigned");
        }
        else
        {
            productionTab.gameObject.SetActive(false);
        }

        /* Set the research panel to inactive if it exists                                       */
        if (explorationTab == null)
        {
            Debug.LogError("Commerce Tab is not assigned");
        }
        else
        {
            explorationTab.gameObject.SetActive(false);
        } 
    }

    /* Open up a lab panel upon clicking the corresponding button                                */
    public void RequestLabPanel(int buttonID) 
    {
        switch (buttonID) {
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

    /* Close the lab panel upon clicking the exit button                                         */
    public void CloseLabPanel(int buttonID) 
    {
        switch (buttonID) {
            case INNOVATE_BUTTON:
                CloseInnovatePanel();
                break;
            case INFO_BUTTON:
                CloseInfoPanel();
                break;
            case UPGRADE_BUTTON:
                Debug.Log("Building Panel: Info requested.");
                break;
            default:
                Debug.Log("Building Panel: Unknown button ID.");
                break;
        }
        PopUpManager.Instance.EnablePlayerInput();
    }

    /* Open up the research panel and assign the buttons in the initial panel                    */
    private void ShowInnovatePanel() 
    {
        innovatePanel.gameObject.SetActive(true);
        pathButtons.transform.Find("commercePath").GetComponent<Button>().onClick.AddListener(() => ShowPath(commerceTab));
        pathButtons.transform.Find("productionPath").GetComponent<Button>().onClick.AddListener(() => ShowPath(productionTab));
        pathButtons.transform.Find("explorationPath").GetComponent<Button>().onClick.AddListener(() => ShowPath(explorationTab));

    }

    /* Show the corresponding path tab upon clicking the path button                             */
    private void ShowPath(GameObject tab)
    {
        initialTab.gameObject.SetActive(false);
        tab.gameObject.SetActive(true);
        /*tab.transform.Find("branch/tierNodeOneContainer").OnMouseEnter();*/
        tab.transform.Find("backArrow").GetComponent<Button>().onClick.AddListener(() => BackToInitialTab(tab));
        tab.transform.Find("buttonContainer/tierOneButton").GetComponent<Button>().onClick.AddListener(() => HandleInnovation(tab, TIER_ONE));
        tab.transform.Find("buttonContainer/tierTwoButton").GetComponent<Button>().onClick.AddListener(() => HandleInnovation(tab, TIER_TWO));
        tab.transform.Find("buttonContainer/tierThreeButton").GetComponent<Button>().onClick.AddListener(() => HandleInnovation(tab, TIER_THREE));
    }

   private void HandleInnovation(GameObject tab, int tier)
   {
      int pearlCost = 0, itemCost = 0;
      string requiredItem = "";

      if (tab == commerceTab)
      {
         requiredItem = "Crude Tool"; // Put your commerce item requirement here if needed
         switch (tier)
         {
            case TIER_ONE: pearlCost = TIER_ONE_PEARL_COST; itemCost = TIER_ONE_ITEM_COST; break;
            case TIER_TWO: pearlCost = TIER_TWO_PEARL_COST; itemCost = TIER_TWO_ITEM_COST; break;
            case TIER_THREE: pearlCost = TIER_THREE_PEARL_COST; itemCost = TIER_THREE_ITEM_COST; break;
         }
      }
      else if (tab == productionTab)
      {
         requiredItem = "Precision Lens";
         switch (tier)
         {
            case TIER_ONE: pearlCost = PROD_T1_PEARL; itemCost = PROD_T1_LENS; break;
            case TIER_TWO: pearlCost = PROD_T2_PEARL; itemCost = PROD_T2_LENS; break;
            case TIER_THREE: pearlCost = PROD_T3_PEARL; itemCost = PROD_T3_LENS; break;
         }
      }
      else if (tab == explorationTab)
      {
         requiredItem = "";
         switch (tier)
         {
            case TIER_ONE: pearlCost = EXPL_T1_PEARL; itemCost = EXPL_T1_ITEM; break;
            case TIER_TWO: pearlCost = EXPL_T2_PEARL; itemCost = EXPL_T2_ITEM; break;
            case TIER_THREE: pearlCost = EXPL_T3_PEARL; itemCost = EXPL_T3_ITEM; break;
         }
      }

      // Attempt to unlock tiers with the corresponding cost
      if (PerformBuy(pearlCost, itemCost, requiredItem))
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
         };
      }
   }

   /* Spend certain amount of resources and give corresponding innovations                     */
   public bool PerformBuy(int pearlCost, int itemCost, string itemName)
   {
      // Check to see if there is enough pearls
      if (InventoryManager.Instance.pearlCount < pearlCost)
      {
         Debug.Log("Not enough pearls!");
         return false;
      }

      // Try to spend the item if necessary
      if (!string.IsNullOrEmpty(itemName) && itemCost > 0)
      {
         // Link to inventory to spend the item
         if (!InventoryManager.Instance.TrySpendItem(itemName, itemCost))
         {
            Debug.Log($"Not enough {itemName} to spend!");
            return false;
         }
      }

      // Spend the pearls
      InventoryManager.Instance.TrySpendPearl(pearlCost);
      return true;
   }

   public void ImplementTierOneInnovation(GameObject tabType)
    {
        /* Permanently increase base sale price of all items by 10%                              */
        if (tabType == commerceTab)
        { 
           currentCommerceTier = TIER_ONE;
           tradeHutManager.marketShiftMin = 1;
           tradeHutManager.marketShiftMax = 2;

           foreach(Transform item in tradeHutManager.SellItems)
              item.Find("NextValue").GetComponent<TextMeshProUGUI>().gameObject.SetActive(true);

           ApplyDiscountToBuyItems(.2f);
        }
      /* Permanently reduce gold spent on refinery upkeep by 50%                               */
      else if (tabType == productionTab)
      {
         Debug.Log("Reduce ore jamming percentage by 5%");
         if (OreRefinery_Manager.Instance != null)
         {
            OreRefinery_Manager.Instance.ReduceJamming(5);
         }
      }
      /* Ships have health and fuel increased                                                    */
      else if (tabType == explorationTab)
      {
         Debug.Log("Tier 1 missions increased by 25%");
      }
      else
      {
          Debug.Log("There is no tab");
      }
    }

    public void ImplementTierTwoInnovation(GameObject tabType)
    {
        /* Grant action to gameple 50 gold for 60% chance to get 250 back                        */
        if (tabType == commerceTab)
        {
           TradeHutManager.Instance.marketShiftMin = 3;
           TradeHutManager.Instance.marketShiftMax = 5;

           tradeHutManager.RecycleButton.gameObject.SetActive(true);
        }

      /* Unlock tier 2 item (reinforces component); forge now has 5% chance to produce a       */
      /*    bonus item upon crafting a single item                                             */
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
        {
            Debug.Log("Permanently increase gold by +15 per turn");
        }
        else
        {
            Debug.Log("There is no tab");
        }
    }

    public void ImplementTierThreeInnovation(GameObject tabType)
    {
       int crudeToolSellValueIncrease     = Mathf.CeilToInt(GetItemValue(ItemType.CrudeTool) * 1.2f) - GetItemValue(ItemType.CrudeTool),
           harpoonSellValueIncrease       = Mathf.CeilToInt(GetItemValue(ItemType.Harpoon) * 1.2f) - GetItemValue(ItemType.Harpoon),
           pressureValveSellValueIncrease = Mathf.CeilToInt(GetItemValue(ItemType.PressureValve) * 1.2f) - GetItemValue(ItemType.PressureValve),
           engineSellValueIncrease        = Mathf.CeilToInt(GetItemValue(ItemType.Engine) * 1.2f) - GetItemValue(ItemType.Engine);

        if (tabType == commerceTab)
        {
           tradeHutManager.marketShiftMin = 5;
           tradeHutManager.marketShiftMax = 10;

           // Removes the negative world events
           tradeHutManager.isTier3BuffACtive = true;

           // Raises sell items base price by 1.2 
           TryIncreaseCrudeToolSellValue(crudeToolSellValueIncrease);
           TryIncreaseHarpoonSellValue(harpoonSellValueIncrease);
           TryIncreasePressureValveValue(pressureValveSellValueIncrease);
           TryIncreaseEngineSellValue(engineSellValueIncrease);
        }
      /* Unlock tier 3 itme (Artifact); Crafting results in two items being made               */
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
            Debug.Log("Decrease search costs by 50%");
        }
        else
        {
            Debug.Log("There is no tab");
        }
    }

    /* Unlock the next tier node upon buying the previous tier node                              */
    public void UnlockNextNode(GameObject tab, int tier)
    {
        Color currentColor;

        /* Get rid of the tier 2 lock and turn on buttons and text                               */
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
        /* Get ride of the tier 3 lock and turn on buttons and text                              */
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

    /* Return to the initial tab upon clicking the back arrow button                             */
    private void BackToInitialTab(GameObject tab)
    {
        tab.gameObject.SetActive(false);
        initialTab.gameObject.SetActive(true);
    }

    /* Open up the info panel                                                                    */
    private void ShowInfoPanel() 
    {
        infoPanel.gameObject.SetActive(true);
    }

    /* Close the research panel                                                                  */
    private void CloseInnovatePanel() 
    {
        commerceTab.gameObject.SetActive(false);
        productionTab.gameObject.SetActive(false);
        explorationTab.gameObject.SetActive(false);
        initialTab.gameObject.SetActive(true);
        innovatePanel.gameObject.SetActive(false);
    }

    /* Close the info panel                                                                      */
    private void CloseInfoPanel()
    {
        infoPanel.gameObject.SetActive(false);
    }
}