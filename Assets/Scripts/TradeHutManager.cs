using System;
using System.Collections;
using System.Collections.Generic;

using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

using static InventoryManager;
using static Item;
using static Resources;
using static TickerSystem;
using static WorldEvents;

public class TradeHutManager : MonoBehaviour 
{
   // Inspector variables
   [SerializeField] private Transform TradePanels;            
   [SerializeField] private Transform BuyWindow;    
   [SerializeField] private Transform SellPanel;              
   [SerializeField] private Transform SellWindow;     
   [SerializeField] private Transform InfoPanel;                   
                    public  Transform BuyPanel;       
                    public  Transform RecycleButton;
   [SerializeField] private RectTransform flipTarget;
   [SerializeField] private Transform worldEventSymbols;

   public PanelManager panelManager;
   public List<Transform> SellItems { get; private set; }
   public List<Transform> BuyItems  { get; private set; }

   private TickerSystem ticker;

   public string currrentNewsTickerMessage;

   // Random number generator
   private readonly static System.Random Rng = new System.Random(); 

   // Keeps track of the last item that had a world event
   private readonly Dictionary<ItemType, bool> lastResetTurn = new Dictionary<ItemType, bool>();
   private readonly Dictionary<ItemType, int> scavengerHolidayDeduction = new Dictionary<ItemType, int>();

   private Vector3 TradePanelScale;
   private Vector3 InfoPanelScale;

   // Transforms
   private Transform currentBuyItem,    
                     currentSellItem,
                     currentMysteryBoxResult;

   // Private variables
   private int crudeToolSellCount     = 0,
               harpoonSellCount       = 0, 
               pressureValveSellCount = 0, 
               divingBellSellCount    = 0,
               precisionLensSellCount = 0,
               engineSellCount        = 0,

               rawOreExchange            = 0,
               mercenaryEngineerBuyCount = 0,

               crudeToolFluctuation,
               harpoonFluctuation,
               divingBellFluctuation,
               pressureValveFluctuation,
               precisionLensFluctuation,
               engineFluctuation,

               crudeToolChance,
               harpoonChance,
               divingBellChance,
               pressureValveChance,
               precisionLensChance,
               engineChance,

               worldEvent,
               currentWorldEventChance;

   public Image worldEventItem,
                worldEventChange;


   private WorldEventTypes crudeToolEvent,
                           harpoonEvent,
                           pressureValveEvent,
                           divingBellEvent,
                           precisionLensEvent,
                           engineEvent;

   // Public variables
   public float marketShiftMax = 1.2f,
                marketShiftMin = 1.06f;
   public int shiftDirection { get; private set; } 


   // Constants
   public const int ENDING_LEVEL   = 5,  
                    STARTING_LEVEL = 1, 
      
                    MAX_BUY_ITEM_COUNT  = 100,   
                    MAX_SELL_ITEM_COUNT = 100,   
                    MIN_BUY_ITEM_COUNT  = 0,   
                    MIN_SELL_ITEM_COUNT = 0, 
      
                    TRADE_BUTTON    = 1,     
                    INFO_BUTTON     = 2,     
                    UPGRADE_BUTTON  = 3,

                    BUY_ITEM_SPACING = 30,

                    PEARL_REWARD_MINIMUM = 20,
                    PEARL_REWARD_MAXIMUM = 40,
                    ORE_EXCHANGE_COST    = 10,
                    
                    MARKET_CHANCE_MIN = 0,
                    MARKET_CHANCE_MAX = 100,
         
                    FIRST_TUTORIAL   = 1,
                    SECOND_TUTORIAL = 2,

                    INSURANCE_POLICY_PAYOUT = 500,
                    
                    TIER_ONE          = 1,
                    TIER_TWO          = 2,
                    TIER_THREE        = 3,

                    TIER_ONE_CHANCE   = 50,
                    TIER_TWO_CHANCE   = 35,
                    TIER_THREE_CHANCE = 25;
      
   public const string RAW_ORE_CHUNK_TAG        = "Raw Ore Chunk",
                       INDUSTRIAL_BLUEPRINT_TAG = "Industrial Blueprint",
                       CLOCKWORK_BLUEPRINT_TAG  = "Clockwork Blueprint",
                       INSURANCE_POLICY_TAG     = "Insurance Policy";
   
   public bool isTier3BuffACtive       = false;
   public bool tutorialFunctionOne     = false;
   public bool tutorialFunctionTwo     = false;
   public bool isInsurancePolicyActive = false;

   [Header("Submerge Settings")]
   public float submergeSpeed = 0.5f;
   public float sunkenScale = 0.6f; // How small it gets as it "sinks"

   private InventoryManager inv;

   public static event Action HandleTutorial;

   public static TradeHutManager Instance;


   /*public void ShowBuyTab()
   {
      if (isSelling) StartTransition(false);
   }

   public void ShowSellTab()
   {
      if (!isSelling) StartTransition(true);
   }*/

   private void Awake() 
   {
      SellItems = new();
      BuyItems  = new();

      TradePanelScale = TradePanels.transform.localScale;
      InfoPanelScale = InfoPanel.transform.localScale;

      // Initialize lastResetTurn for every ItemType so lookups are safe
      foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
         lastResetTurn[itemType] = false;

      foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
         scavengerHolidayDeduction[itemType] = 0;

      OnItemValueChange = ChangeItemValueText;

      if (Instance != null && Instance != this)
         Destroy(this.gameObject);
      else 
      {
         Instance = this;
         DontDestroyOnLoad(this.gameObject);
      }

      if (TradePanels == null)
         Debug.LogError("Trade Panel is not assigned in the Inspector!");
      else
         CloseTradePanel();

      if (InfoPanel == null)
         Debug.LogError("Info Panel is not assigned in the Inspector!");
      else
         CloseInfoPanel();

      if (SellWindow == null)
         Debug.LogError("Sell window is not assigned in the Inspector");
      else
         CloseSellWindow();

      if (SellPanel == null)
         Debug.LogError("Sell Panel is not assigned in the Inspector!");
      else
         CloseSellPanel();

      if (BuyPanel == null)
         Debug.LogError("Buy Panel is not assigned in the Inspector!");
      else
         CloseBuyPanel();

      if (BuyWindow == null)
         Debug.LogError("Buy Window is not assigned in the Inspector!");
      else
         BuyWindow.gameObject.SetActive(false);

   }

   private void Start()
   {
      // Ensure we have the reference
      if (panelManager == null) panelManager = FindFirstObjectByType<PanelManager>();

      inv = InventoryManager.Instance;
      ticker = TickerSystem.Instance;

      // Reset scales to 1 manually to prevent the "Zero Scale" recording bug
      TradePanels.localScale = Vector3.one;
      TradePanels.gameObject.SetActive(false);
      inv = InventoryManager.Instance;
      ticker = TickerSystem.Instance;

      CreateSellItem(GetItemSprite(ItemType.CrudeTool), GetItemValue(ItemType.CrudeTool), 0.0f, CRUDE_TOOL_TAG);
      CreateSellItem(GetItemSprite(ItemType.Harpoon), GetItemValue(ItemType.Harpoon), 2.5f, HARPOON_TAG);
      CreateSellItem(GetItemSprite(ItemType.DivingBell), GetItemValue(ItemType.DivingBell), 0.0f, DIVING_BELL_TAG, -90);
      CreateSellItem(GetItemSprite(ItemType.PressureValve), GetItemValue(ItemType.PressureValve), 2.5f, PRESSURE_VALVE_TAG, -90);
      CreateSellItem(GetItemSprite(ItemType.PrecisionLens), GetItemValue(ItemType.PrecisionLens), 0.0f, PRECISION_LENS_TAG, -165);
      CreateSellItem(GetItemSprite(ItemType.Engine), GetItemValue(ItemType.Engine), 2.5f, ENGINE_TAG, -165);

      CreateBuyItem(GetItemSprite(ItemType.RawOreChunk), GetItemPrice(ItemType.RawOreChunk), 0.0f, RAW_ORE_CHUNK_TAG);
      CreateBuyItem(GetItemSprite(ItemType.IndustrialBlueprint), GetItemPrice(ItemType.IndustrialBlueprint), 1.2f, INDUSTRIAL_BLUEPRINT_TAG);
      CreateBuyItem(GetItemSprite(ItemType.ClockworkBlueprint), GetItemPrice(ItemType.ClockworkBlueprint), 0.0f, CLOCKWORK_BLUEPRINT_TAG, -30);
      CreateBuyItem(GetItemSprite(ItemType.MercenaryEngineer), GetItemPrice(ItemType.MercenaryEngineer), 1.2f, MERCENARY_ENGINEER_TAG, -30);
      CreateBuyItem(GetItemSprite(ItemType.InsurancePolicy), GetItemPrice(ItemType.InsurancePolicy), .7f, INSURANCE_POLICY_TAG, -65);
   }

   public void OnEnable()
   {
      TutorialManager.HandleTradeHutTutorial += ChangeTutorialState;
   }

   public void OnDisable()
   {
      TutorialManager.HandleTradeHutTutorial -= ChangeTutorialState;
   }

   public void ChangeTutorialState(int tutorialType)
   {
      if (tutorialType == FIRST_TUTORIAL)
         tutorialFunctionOne = true;

      if (tutorialType == SECOND_TUTORIAL)
         tutorialFunctionTwo = true;
   }

   public void CreateSellItem(Sprite itemSprite, int itemValue, float positionIndex, string itemTag, int verticalIndex = 0) 
   {
      Transform       sellItemContainer = SellPanel.Find("sellItemContainer").GetComponent<Transform>(),
                      sellItemTemplate  = sellItemContainer.Find("SellItemTemplate").GetComponent<Transform>(),
                      tradeItemTransform;

      TextMeshProUGUI sellValueText;
      RectTransform   tradeItemRectTransform;
      Button          itemButton;

      sellItemTemplate.gameObject.SetActive(false);

      // Instantiate the template and set its position in the containerS                             
      tradeItemTransform     = Instantiate(sellItemTemplate, sellItemContainer);
      tradeItemRectTransform = tradeItemTransform.GetComponent<RectTransform>();

      tradeItemTransform.tag = itemTag;
      tradeItemRectTransform.anchoredPosition = new Vector2(BUY_ITEM_SPACING * positionIndex, verticalIndex);

      // Populate the the item properties                                                           
      sellValueText      = tradeItemTransform.Find("ItemValue").GetComponent<TextMeshProUGUI>();
      sellValueText.text = itemValue.ToString();

      tradeItemTransform.Find("ItemShadow").GetComponent<Image>().sprite        = itemSprite;
      tradeItemTransform.Find("ItemShadow").gameObject.SetActive(false);
      tradeItemTransform.Find("Chain").gameObject.SetActive(false);

      itemButton = tradeItemTransform.Find("ItemButton").GetComponent<Button>();
      itemButton.image.sprite = itemSprite;

      if (tradeItemTransform.Find("ItemCount") == null)
         Debug.LogError("Item count not found");
      else
         switch (itemTag) 
         {
            case CRUDE_TOOL_TAG:
               tradeItemTransform.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = " x" + inv.crudeToolCount.ToString();
               break;
            case HARPOON_TAG:
               tradeItemTransform.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = " x" + inv.harpoonCount.ToString();
               break;
            case PRESSURE_VALVE_TAG:
               tradeItemTransform.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = " x" + inv.pressureValveCount.ToString();
               break;
            case DIVING_BELL_TAG:
               tradeItemTransform.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = " x" + inv.divingBellCount.ToString();
               break;
            case PRECISION_LENS_TAG:
               tradeItemTransform.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = " x" + inv.precisionLensCount.ToString();
               break;
            case ENGINE_TAG:
               tradeItemTransform.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = " x" + inv.engineCount.ToString();
               break;
            default:
               Debug.LogError("Unkown item: " +  itemTag);
               break;
         }

      if (LabManager.currentCommerceTier < LabManager.TIER_ONE)
         tradeItemTransform.Find("NextValue").GetComponent<TextMeshProUGUI>().gameObject.SetActive(false);

     if(itemTag != CRUDE_TOOL_TAG && itemTag != HARPOON_TAG && itemTag != DIVING_BELL_TAG) 
     {
         tradeItemTransform.Find("ItemButton").gameObject.SetActive(false);
         tradeItemTransform.Find("ItemCount").gameObject.SetActive(false);
         tradeItemTransform.Find("ItemValue").gameObject.SetActive(false);
         tradeItemTransform.Find("Pearl_Icon").gameObject.SetActive(false);
         tradeItemTransform.Find("ItemShadow").gameObject.SetActive(true);
         tradeItemTransform.Find("Chain").gameObject.SetActive(true);
     }

      // Dynamically add a listener to the button, which creates a sell window when clicked         
      itemButton.onClick.AddListener(() => CreateSellWindow(itemSprite, GetResourceSprite(ResourceType.Pearl), itemValue, itemTag));
      
      SellItems.Add(tradeItemTransform);

      tradeItemTransform.gameObject.SetActive(true);
   }

   public void CreateBuyItem(Sprite itemSprite, int itemValue, float positionIndex, string itemTag, int verticalIndex = 0) 
   {
      Transform     buyItemContainer = BuyPanel.Find("BuyItemContainer").GetComponent<Transform>(),
                    buyItemTemplate  = buyItemContainer.Find("BuyItemTemplate").GetComponent<Transform>(),
                    tradeItemTransform;
      
      RectTransform tradeItemRectTransform;
      Button          itemButton;
      TextMeshProUGUI infoText;

      // Instantiate the template and set its position in the container
      tradeItemTransform     = Instantiate(buyItemTemplate, buyItemContainer);
      buyItemTemplate.gameObject.SetActive(false);
      tradeItemRectTransform = tradeItemTransform.GetComponent<RectTransform>();
      tradeItemRectTransform.anchoredPosition = new Vector2(BUY_ITEM_SPACING * positionIndex, verticalIndex);     

      // Populate the item properties
      tradeItemTransform.tag = itemTag;
      itemButton = tradeItemTransform.Find("ItemButton").GetComponent<Button>();

      itemButton.image.sprite = itemSprite;

      infoText = tradeItemTransform.Find("ItemInfoCanvas").Find("ItemInfoPanel").Find("InfoText").GetComponent<TextMeshProUGUI>();


      switch (itemTag) 
      {
         case RAW_ORE_CHUNK_TAG:
            infoText.text = GetItemDescription(ItemType.RawOreChunk);
            break;
         case INDUSTRIAL_BLUEPRINT_TAG:
            infoText.text = GetItemDescription(ItemType.IndustrialBlueprint);
            break;
         case CLOCKWORK_BLUEPRINT_TAG:
            infoText.text = GetItemDescription(ItemType.ClockworkBlueprint);
            break;
         case MERCENARY_ENGINEER_TAG:
            int index = GetItemDescription(ItemType.MercenaryEngineer).IndexOf('.'); 
            if (index >= 0)
               infoText.text = GetItemDescription(ItemType.MercenaryEngineer).Substring(0, index + 1).Trim();
            break;
         case INSURANCE_POLICY_TAG:
            infoText.text = GetItemDescription(ItemType.InsurancePolicy);
            break;
         default:
            Debug.LogError("Unkown item: " + itemTag);
            break;
      }

      BuyItems.Add(tradeItemTransform);

      // Dynamically add a listener to the button, which creates the buy window
      itemButton.onClick.AddListener(() => CreateBuyWindow(itemSprite, GetResourceSprite(ResourceType.Pearl), itemValue, itemTag));

      tradeItemTransform.gameObject.SetActive(true);
   }

   // Creates and populates the single sell transaction window                                       
   private void CreateSellWindow(Sprite itemSprite, Sprite currencySprite, int itemValue, string itemTag) 
   {
      Transform sellWindowContainer = SellWindow.Find("SellWindowContainer").GetComponent<Transform>(),
                sellWindowTemplate  = sellWindowContainer.Find("SellWindowTemplate").GetComponent<Transform>();

      sellWindowTemplate.gameObject.SetActive(false);

      // Destroy the previously opened sell window instance before creating a new one
      if (currentSellItem != null) 
      {
         Destroy(currentSellItem.gameObject);
         currentSellItem = null;
      }

      if(tutorialFunctionOne && itemTag == CRUDE_TOOL_TAG && SellPanel.Find("TutorialPart3").gameObject.activeSelf)
      {
         SellPanel.Find("TutorialPart3").gameObject.SetActive(false);
         SellPanel.Find("TutorialPart4").gameObject.SetActive(true);
      }

      Transform     sellItemTransform     = Instantiate(sellWindowTemplate, sellWindowContainer);
      RectTransform sellItemRectTransform = sellItemTransform.GetComponent<RectTransform>();

      sellItemRectTransform.anchoredPosition = new Vector2(BUY_ITEM_SPACING * 0, 0);

      // Populate item properties
      sellItemTransform.tag = itemTag;
      sellItemTransform.Find("ItemImage").GetComponent<Image>().sprite              = itemSprite;
      sellItemTransform.Find("SellItemName").GetComponent<TextMeshProUGUI>().text   = itemTag;
      sellItemTransform.Find("currencyIcon").GetComponent<Image>().sprite           = currencySprite;
      sellItemTransform.Find("currencyGained").GetComponent<TextMeshProUGUI>().text = "0";

      switch (itemTag) 
      {
         case CRUDE_TOOL_TAG:
            crudeToolSellCount = 0;
            sellItemTransform.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "   " + crudeToolSellCount.ToString();
            break;
         case HARPOON_TAG:
            harpoonSellCount = 0;
            sellItemTransform.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "   " + harpoonSellCount.ToString();
            break;
         case PRESSURE_VALVE_TAG:
            pressureValveSellCount = 0;
            sellItemTransform.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "   " + pressureValveSellCount.ToString();
            break;
         case DIVING_BELL_TAG:
            divingBellSellCount = 0;
            sellItemTransform.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "   " + divingBellSellCount.ToString(); 
            break;
         case PRECISION_LENS_TAG:
            precisionLensSellCount = 0;
            sellItemTransform.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "   " + precisionLensSellCount.ToString();
            break;
         case ENGINE_TAG:
            engineSellCount = 0;
            sellItemTransform.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "   " + engineSellCount.ToString();
            break;
         default:
            Debug.LogError("Unkown item: " + itemTag);
            break;
      }

      // Get references to the increase and decrease buttons
      Button increaseButton = sellItemTransform.Find("QuantityButtons/IncreaseButton").GetComponent<Button>();
      Button decreaseButton = sellItemTransform.Find("QuantityButtons/DecreaseButton").GetComponent<Button>();

      // Dynamically add listeners to the buttons, increasing or decreasing the sell items
      increaseButton.onClick.AddListener(() => IncreaseSellItemCount(sellItemTransform));
      decreaseButton.onClick.AddListener(() => DecreaseSellItemCount(sellItemTransform));

      // Store the reference to the newly created sell window instance
      currentSellItem = sellItemTransform;
      sellItemTransform.gameObject.SetActive(true);
      ShowSellWindow();
   }

   // Creates and populates the single buy transaction window                                         
   private void CreateBuyWindow(Sprite itemSprite, Sprite currencySprite, int itemValue, string itemTag) 
   {
      Transform buyWindowContainer = BuyWindow.Find("BuyWindowContainer").GetComponent<Transform>(),
                buyWindowTemplate  = buyWindowContainer.Find("BuyWindowTemplate").GetComponent<Transform>();
      int       itemCount          = 0;

      buyWindowTemplate.gameObject.SetActive(false);

      // Destroy the previously opened buy window instance before creating a new one
      if (currentBuyItem != null) 
      {
         Destroy(currentBuyItem.gameObject);
         currentBuyItem = null;
      }

      Transform     buyItemTransfrom              = Instantiate(buyWindowTemplate, buyWindowContainer);
      RectTransform buyItemTransfromRectTransform = buyItemTransfrom.GetComponent<RectTransform>();

      buyItemTransfrom.tag = itemTag;

      // Populate item properties
      buyItemTransfrom.Find("ItemImage").GetComponent<Image>().sprite        = itemSprite;
      buyItemTransfrom.Find("ItemName").GetComponent<TextMeshProUGUI>().text = itemTag;
      buyItemTransfrom.Find("currencyIcon").GetComponent<Image>().sprite     = currencySprite;

      switch(itemTag) 
      {
         case RAW_ORE_CHUNK_TAG:
            rawOreExchange = 0;
            buyItemTransfrom.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = rawOreExchange.ToString();
            break;
         case MERCENARY_ENGINEER_TAG:
            mercenaryEngineerBuyCount = 0;
            buyItemTransfrom.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = mercenaryEngineerBuyCount.ToString();
            break;
         default:
            buyItemTransfrom.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = 1.ToString();
            break;
      }

      switch (itemTag) 
      {
         case INDUSTRIAL_BLUEPRINT_TAG:
            buyItemTransfrom.Find("currencySpent").GetComponent<TextMeshProUGUI>().text = GetItemPrice(ItemType.IndustrialBlueprint).ToString();
            break;
         case CLOCKWORK_BLUEPRINT_TAG:
            buyItemTransfrom.Find("currencySpent").GetComponent<TextMeshProUGUI>().text = GetItemPrice(ItemType.ClockworkBlueprint).ToString();
            break;
         case MERCENARY_ENGINEER_TAG:
            buyItemTransfrom.Find("currencySpent").GetComponent<TextMeshProUGUI>().text = "0";
            break;
         case RAW_ORE_CHUNK_TAG:
            buyItemTransfrom.Find("currencySpent").GetComponent<TextMeshProUGUI>().text = "0";
            break;
         case INSURANCE_POLICY_TAG:
            buyItemTransfrom.Find("currencySpent").GetComponent<TextMeshProUGUI>().text = GetItemPrice(ItemType.InsurancePolicy).ToString();
            break;
         default:
            Debug.LogError("Unkown item: " + itemTag);
            break;
      }

      buyItemTransfrom.Find("QuantityButtons/IncreaseButton").gameObject.SetActive(false);
      buyItemTransfrom.Find("QuantityButtons/DecreaseButton").gameObject.SetActive(false);

      if (itemTag  == RAW_ORE_CHUNK_TAG || itemTag == MERCENARY_ENGINEER_TAG)
      {
         buyItemTransfrom.Find("QuantityButtons/IncreaseButton").gameObject.SetActive(true);
         buyItemTransfrom.Find("QuantityButtons/DecreaseButton").gameObject.SetActive(true);

         buyItemTransfrom.Find("ItemCount").GetComponent<TextMeshProUGUI>().text     = "   " + itemCount.ToString();

         // Get references to the increase and decrease buttons
         Button increaseButton = buyItemTransfrom.Find("QuantityButtons/IncreaseButton").GetComponent<Button>();
         Button decreaseButton = buyItemTransfrom.Find("QuantityButtons/DecreaseButton").GetComponent<Button>();
         
         // Dynamically add listeners to the buttons, which increases or decreases the buy item count
         increaseButton.onClick.AddListener(() => IncreaseBuyItemsCount(buyItemTransfrom));
         decreaseButton.onClick.AddListener(() => DecreaseBuyItemsCount(buyItemTransfrom));
      }

      // Store the reference to the newly created buy window instance
      currentBuyItem = buyItemTransfrom;
      buyItemTransfrom.gameObject.SetActive(true);
      ShowBuyWindow();
   }

   public void SellItem() 
   {
      int    totalSellValue = 0,
             soldCount      = 0;

      string successMessage = "",
             itemTag        = currentSellItem ? currentSellItem.tag : string.Empty;

      Debug.Log("Harpoon sell count " + harpoonSellCount.ToString());
      switch (itemTag) 
      {
         // Tier 1 items
         case CRUDE_TOOL_TAG:
            if (crudeToolSellCount > MIN_SELL_ITEM_COUNT)
            {
               
               if (inv.TryUseCrudeTool(crudeToolSellCount)) 
               {
                  soldCount = crudeToolSellCount;
                  totalSellValue += soldCount * GetItemValue(ItemType.CrudeTool);
                  successMessage = $"Sold {soldCount} Crude Tool{(soldCount > 1 ? "s" : "")} for {totalSellValue} pearls.";
               }
               else 
                  crudeToolSellCount = MIN_SELL_ITEM_COUNT;

               if(tutorialFunctionOne)
               {
                  SellPanel.Find("TutorialPart5").gameObject.SetActive(false);
                  SellPanel.Find("TutorialPart6").gameObject.SetActive(true);
               }
            }
            break;
      
         case HARPOON_TAG:
            if (harpoonSellCount > MIN_SELL_ITEM_COUNT) 
            {
               
               if (inv.TryUseHarpoon(harpoonSellCount))
               {
                  soldCount = harpoonSellCount;
                  totalSellValue += soldCount * GetItemValue(ItemType.Harpoon);
                  successMessage = $"Sold {soldCount} Harpoon{(soldCount > 1 ? "s" : "")} for {totalSellValue} pearls.";
               }
               else
                  harpoonSellCount = MIN_SELL_ITEM_COUNT;
            }
            break;
      
         // Tier 2 item
         case PRESSURE_VALVE_TAG:
            if (pressureValveSellCount > MIN_SELL_ITEM_COUNT) 
            {

               if (inv.TryUsePressureValve(pressureValveSellCount))
               {
                  soldCount = pressureValveSellCount;
                  totalSellValue += soldCount * GetItemValue(ItemType.PressureValve);
                  successMessage = $"Sold {soldCount} Pressure Valve{(soldCount > 1 ? "s" : "")} for {totalSellValue} pearls.";
               }
               else
                  pressureValveSellCount = MIN_SELL_ITEM_COUNT;
            }
            break;
      
         // Tier 3 item
         case ENGINE_TAG:
            if (engineSellCount > MIN_SELL_ITEM_COUNT) 
            {
               
               if (inv.TryUseEngine(engineSellCount))
               {
                  soldCount = engineSellCount;
                  totalSellValue += soldCount * GetItemValue(ItemType.Engine);
                  successMessage = $"Sold {soldCount} Engine{(soldCount > 1 ? "s" : "")} for {totalSellValue} pearls.";
               }
               else
                  engineSellCount = MIN_SELL_ITEM_COUNT;
            }
            break;
      }
      
      // Recieves pearls and show success ticker only if something sold
      if (totalSellValue > 0)
      {
         inv.TryAddPearl(totalSellValue);
         ticker.ShowTicker(successMessage, Color.green, MessageTypes.ResultMessage);
      } 
      else 
      {
         // Check if the user actually tried to sell something but failed 
         // or not selecting anything at all.
         ticker.ShowTicker("Transaction failed or no items selected.", Color.red, MessageTypes.ResultMessage);
      }

      crudeToolSellCount     = MIN_SELL_ITEM_COUNT;
      harpoonSellCount       = MIN_SELL_ITEM_COUNT;
      pressureValveSellCount = MIN_SELL_ITEM_COUNT;
      engineSellCount        = MIN_SELL_ITEM_COUNT;
      
      // Destroy the instantiated sell window and remove the reference                                
      if (currentSellItem != null) 
      {
         Destroy(currentSellItem.gameObject);
         currentSellItem = null;
      }
      
      CloseSellWindow();
      
      return;
   }

  public void BuyItem() 
  {
      // Checks if there is a current buy item
      if (currentBuyItem != null) 
      {
         // Handles raw ore exchange
         if (rawOreExchange > MIN_BUY_ITEM_COUNT) 
         { 
            bool pearlsSpent = inv.TrySpendPearl(rawOreExchange),
                 oreAdded    = false;
            
            if (pearlsSpent)
               oreAdded = inv.TryAddOre(rawOreExchange);
            
            if (pearlsSpent && oreAdded)
               ticker.ShowTicker($"Bought {rawOreExchange} Ore for {rawOreExchange} pearls.", Color.green, MessageTypes.ResultMessage);
            
            rawOreExchange = MIN_BUY_ITEM_COUNT;
         }
         else
            if(currentBuyItem.tag == RAW_ORE_CHUNK_TAG)
               ticker.ShowTicker("No items have been selected", Color.red, MessageTypes.ResultMessage);

         // Tier 2 Blueprint purchase flow
         if (currentBuyItem.CompareTag(INDUSTRIAL_BLUEPRINT_TAG) && inv.TrySpendPearl(GetItemPrice(ItemType.IndustrialBlueprint))) 
         {
            // Grants player access to tier 2 blueprint content
            ForgeManager.Instance.hasTier2Blueprint = true;

            // Adds new craftable items to the inventory/craft list (pressure valve, diving bell)
            InventoryManager.Instance.CreateCraft(GetItemSprite(ItemType.PressureValve), PRESSURE_VALVE_POSITION, PRESSURE_VALVE_TAG);
            InventoryManager.Instance.CreateCraft(GetItemSprite(ItemType.PatchKit), PATCH_KIT_POSITION, PATCH_KIT_TAG, -450);

            // Removes the tier 2 blueprint from the buy panel
            BuyItems.Find(item => item.CompareTag(INDUSTRIAL_BLUEPRINT_TAG)).gameObject.SetActive(false);

            // Reveal the tier 2 items on the sell panel and enable its UI controls
            SellItems.Find(item => item.CompareTag(PRESSURE_VALVE_TAG)).Find("ItemButton").gameObject.SetActive(true);
            SellItems.Find(item => item.CompareTag(PRESSURE_VALVE_TAG)).Find("ItemCount").gameObject.SetActive(true);
            SellItems.Find(item => item.CompareTag(PRESSURE_VALVE_TAG)).Find("ItemValue").gameObject.SetActive(true);
            SellItems.Find(item => item.CompareTag(PRESSURE_VALVE_TAG)).Find("Pearl_Icon").gameObject.SetActive(true);
            SellItems.Find(item => item.CompareTag(PRESSURE_VALVE_TAG)).Find("ItemShadow").gameObject.SetActive(false);
            SellItems.Find(item => item.CompareTag(PRESSURE_VALVE_TAG)).Find("Chain").gameObject.SetActive(false);
            
            SellItems.Find(item => item.CompareTag(DIVING_BELL_TAG)).Find("ItemButton").gameObject.SetActive(true);
            SellItems.Find(item => item.CompareTag(DIVING_BELL_TAG)).Find("ItemCount").gameObject.SetActive(true);
            SellItems.Find(item => item.CompareTag(DIVING_BELL_TAG)).Find("ItemValue").gameObject.SetActive(true);
            SellItems.Find(item => item.CompareTag(DIVING_BELL_TAG)).Find("Pearl_Icon").gameObject.SetActive(true);
            SellItems.Find(item => item.CompareTag(DIVING_BELL_TAG)).Find("ItemShadow").gameObject.SetActive(false);
            SellItems.Find(item => item.CompareTag(DIVING_BELL_TAG)).Find("Chain").gameObject.SetActive(false);



            if (ticker == null)
               Debug.LogError("Ticker is null");
            ticker.ShowTicker("Purchased Tier 2 Blueprint ・Pressure Valve and Diving Bell unlocked.", Color.green, MessageTypes.ResultMessage);
         }

         // Tier 3 Blueprint purchase flow
         if (currentBuyItem.CompareTag(CLOCKWORK_BLUEPRINT_TAG) && inv.TrySpendPearl(GetItemPrice(ItemType.ClockworkBlueprint)))
         {
            // Grants player access to tier 3 blueprint content
            ForgeManager.Instance.hasTier3Blueprint = true;

            // Removes the tier 3 blueprint from the buy panel
            BuyItems.Find(item => item.CompareTag(CLOCKWORK_BLUEPRINT_TAG)).gameObject.SetActive(false);

            // Adds new craftable items (engine, precision lens) to inventory/craft list
            inv.CreateCraft(GetItemSprite(ItemType.Engine), ENGINE_POSITION, ENGINE_TAG);
            inv.CreateCraft(GetItemSprite(ItemType.PrecisionLens), PRECISION_LENS_POSITION, PRECISION_LENS_TAG, -450);

            // Reveals the tier 3 items on the sell panel and enable its UI controls
            SellItems.Find(item => item.CompareTag(PRECISION_LENS_TAG)).Find("ItemButton").gameObject.SetActive(true);
            SellItems.Find(item => item.CompareTag(PRECISION_LENS_TAG)).Find("ItemCount").gameObject.SetActive(true);
            SellItems.Find(item => item.CompareTag(PRECISION_LENS_TAG)).Find("ItemValue").gameObject.SetActive(true);
            SellItems.Find(item => item.CompareTag(PRECISION_LENS_TAG)).Find("Pearl_Icon").gameObject.SetActive(true);
            SellItems.Find(item => item.CompareTag(PRECISION_LENS_TAG)).Find("ItemShadow").gameObject.SetActive(false);
            SellItems.Find(item => item.CompareTag(PRECISION_LENS_TAG)).Find("Chain").gameObject.SetActive(false);
      
            SellItems.Find(item => item.CompareTag(ENGINE_TAG)).Find("ItemButton").gameObject.SetActive(true);
            SellItems.Find(item => item.CompareTag(ENGINE_TAG)).Find("ItemCount").gameObject.SetActive(true);
            SellItems.Find(item => item.CompareTag(ENGINE_TAG)).Find("ItemValue").gameObject.SetActive(true);
            SellItems.Find(item => item.CompareTag(ENGINE_TAG)).Find("Pearl_Icon").gameObject.SetActive(true);
            SellItems.Find(item => item.CompareTag(ENGINE_TAG)).Find("ItemShadow").gameObject.SetActive(false);
            SellItems.Find(item => item.CompareTag(ENGINE_TAG)).Find("Chain").gameObject.SetActive(false);
           
            ticker.ShowTicker("Purchased Tier 3 Blueprint ・Engine and Precision Lens unlocked.", Color.green, MessageTypes.ResultMessage);
         }

         // Mercenary Engineer purchase flow
         if (currentBuyItem.CompareTag(MERCENARY_ENGINEER_TAG) && 
             inv.TrySpendPearl(mercenaryEngineerBuyCount * GetItemPrice(ItemType.MercenaryEngineer)) &&
             inv.TryAddMercenaryEngineer(mercenaryEngineerBuyCount)) 
         {
            if(inv.InventoryItems?.Find(item => item.CompareTag(MERCENARY_ENGINEER_TAG)) == null)
               inv.CreateCraft(GetItemSprite(ItemType.MercenaryEngineer), MERCENARY_ENGINEER_POSITION, MERCENARY_ENGINEER_TAG, -450);

            ForgeManager.Instance.hasMercenaryEngineer = inv.mercenaryEngineerCount > 0 ? true: false;

            if(inv.mercenaryEngineerCount == MAX_MERCENARY_ENGINEER_COUNT)
               BuyItems.Find(item => item.CompareTag(MERCENARY_ENGINEER_TAG)).gameObject.SetActive(false);
            ticker.ShowTicker("Purchased Mercenary Engineer.", Color.green, MessageTypes.ResultMessage);
         }

         if(currentBuyItem.CompareTag(INSURANCE_POLICY_TAG) && inv.TrySpendPearl(GetItemPrice(ItemType.InsurancePolicy)))
         { 
            isInsurancePolicyActive = true;
            ticker.ShowTicker("Purchased Insurance Policy.", Color.green, MessageTypes.ResultMessage);
         }
      }

      // Clean up the buy window instance if one was open
      if (currentBuyItem != null) 
      {
         Destroy(currentBuyItem.gameObject);
         currentBuyItem = null;
      }
      
      CloseBuyWindow();
      
      return;
   }

   // Increments the count for the item being sold and updates the UI
   public void IncreaseSellItemCount(Transform item) 
   {
      AdjustSellQuantity(item, 1);
      if(tutorialFunctionOne)
      {
         SellPanel.Find("TutorialPart4").gameObject.SetActive(false);
         SellPanel.Find("TutorialPart5").gameObject.SetActive(true);
      }
   }

   // Decrements the count for the item being sold and updates the UI
   public void DecreaseSellItemCount(Transform item) 
   {
      AdjustSellQuantity(item, -1);
   }

   public void AdjustSellQuantity(Transform item, int quantityChange) 
   {
      ItemType itemType = ItemType.CrudeTool;
      int      current  = 0;
      int      owned    = 0;

      switch (item.tag) 
      {
         case CRUDE_TOOL_TAG:
            current  = crudeToolSellCount;
            owned    = inv. crudeToolCount;
            itemType = ItemType.CrudeTool;
            break;
         case HARPOON_TAG:
            current  = harpoonSellCount;
            owned    = inv.harpoonCount;
            itemType = ItemType.Harpoon;
            break;
         case PRESSURE_VALVE_TAG:
            current  = pressureValveSellCount;
            owned    = inv.pressureValveCount;
            itemType = ItemType.PressureValve;
            break;
         case ENGINE_TAG:
            current  = engineSellCount;
            owned    = inv.engineCount;
            itemType = ItemType.Engine;
            break;
         default:
            Debug.LogError("Unknown item tag: " + item.tag);
            break;
      }
      
      if (quantityChange > 0)
      {
         if (current < MAX_SELL_ITEM_COUNT && current < owned)
            current += 1;
         else
         {
            if (owned > 0)
               ticker.ShowTicker($"Cannot select that many ・you only have {owned} {item.tag}{(owned == 1 ? "" : "s")}.", Color.red, MessageTypes.ResultMessage);
            else
               ticker.ShowTicker($"You have no {item.tag}s to sell. Craft {item.tag}s before selling.", Color.red, MessageTypes.ResultMessage);
         }
      } 
      else 
      {
         if (current > MIN_SELL_ITEM_COUNT) 
            current -= 1;
         else 
         {
            if (owned > 0)
               ticker.ShowTicker($"Nothing selected to remove ・you own {owned} {item.tag}{(owned == 1 ? "" : "s")}. Use the + button to select an amount.", Color.red, MessageTypes.ResultMessage);
            else
               ticker.ShowTicker($"You have no {item.tag}s to sell. Craft {item.tag}s before selling.", Color.red, MessageTypes.ResultMessage);
         }
      }

      switch (item.tag)
      {
         case CRUDE_TOOL_TAG:
           crudeToolSellCount = current; 
           break;
         case HARPOON_TAG: 
           harpoonSellCount = current; 
           break;
         case PRESSURE_VALVE_TAG: 
           pressureValveSellCount = current; 
           break;
         case DIVING_BELL_TAG: 
           divingBellSellCount = current; 
           break;
         case PRECISION_LENS_TAG: 
           precisionLensSellCount = current; 
           break;
         case ENGINE_TAG: 
           engineSellCount = current; 
           break;
         default:
            Debug.LogError("Unkown item: " + item.tag);
            break;
      }
       
      item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text      = "   " + current.ToString();
      item.Find("currencyGained").GetComponent<TextMeshProUGUI>().text = (current * GetItemValue(itemType)).ToString();

      return;
   }

   // Increments the count for the item being bought and updates the UI
   public void IncreaseBuyItemsCount(Transform item) 
   {
      switch (item.tag) 
      {
         case RAW_ORE_CHUNK_TAG:
            if (rawOreExchange < MAX_BUY_ITEM_COUNT) 
            {
               rawOreExchange += 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text     = "   " + rawOreExchange.ToString();
               item.Find("currencySpent").GetComponent<TextMeshProUGUI>().text = (rawOreExchange * GetItemPrice(ItemType.RawOreChunk)).ToString();
            }
            break;
         case MERCENARY_ENGINEER_TAG:
            if (mercenaryEngineerBuyCount < MAX_MERCENARY_ENGINEER_COUNT) 
            {
               mercenaryEngineerBuyCount += 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text     = "   " + mercenaryEngineerBuyCount.ToString();
               item.Find("currencySpent").GetComponent<TextMeshProUGUI>().text = (mercenaryEngineerBuyCount * GetItemPrice(ItemType.MercenaryEngineer)).ToString();
            }
            break;
          default:
            Debug.LogError("Unknown item tag: " + item.tag);
            break;
      }
   }

   // Decrements the count for the item being bought and updates the UI
   public void DecreaseBuyItemsCount(Transform item) 
   {
      switch (item.tag) 
      {
         case RAW_ORE_CHUNK_TAG:
            if (rawOreExchange > MIN_BUY_ITEM_COUNT) 
            {
               rawOreExchange -= 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text     = "   " + rawOreExchange.ToString();
               item.Find("currencySpent").GetComponent<TextMeshProUGUI>().text = (rawOreExchange * GetItemPrice(ItemType.RawOreChunk)).ToString();
            }
            break;
         case MERCENARY_ENGINEER_TAG:
            if (mercenaryEngineerBuyCount > MIN_BUY_ITEM_COUNT) 
            {
               mercenaryEngineerBuyCount -= 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text     = "   " + mercenaryEngineerBuyCount.ToString();
               item.Find("currencySpent").GetComponent<TextMeshProUGUI>().text = (mercenaryEngineerBuyCount * GetItemPrice(ItemType.MercenaryEngineer)).ToString();
            }
            break;
         default:
            Debug.LogError("Unknown item tag: " + item.tag);
            break;
      }
   }

   // Exchanges ores for pearls
   public void RecycleOre() 
   {
      int pearlsReceived = Rng.Next(PEARL_REWARD_MINIMUM, PEARL_REWARD_MAXIMUM + 1);

      inv.TrySpendOre(ORE_EXCHANGE_COST);
      inv.TryAddPearl(pearlsReceived);

      return;
   }
   
   public void CraftMarketForesight() 
   {
       // 1. Roll all chances and fluctuations for the upcoming turn ONCE
       crudeToolChance     = Rng.Next(MARKET_CHANCE_MIN, MARKET_CHANCE_MAX + 1);
       harpoonChance       = Rng.Next(MARKET_CHANCE_MIN, MARKET_CHANCE_MAX + 1);
       pressureValveChance = Rng.Next(MARKET_CHANCE_MIN, MARKET_CHANCE_MAX + 1);
       divingBellChance    = Rng.Next(MARKET_CHANCE_MIN, MARKET_CHANCE_MAX + 1);
       precisionLensChance = Rng.Next(MARKET_CHANCE_MIN, MARKET_CHANCE_MAX + 1);
       engineChance        = Rng.Next(MARKET_CHANCE_MIN, MARKET_CHANCE_MAX + 1);
   
       crudeToolFluctuation     = GetItemSellValueFluctuation(base_crude_tool_value);
       harpoonFluctuation       = GetItemSellValueFluctuation(base_harpoon_value);
       pressureValveFluctuation = GetItemSellValueFluctuation(base_pressure_valve_value);
       divingBellFluctuation    = GetItemSellValueFluctuation(base_diving_bell_value);
       precisionLensFluctuation = GetItemSellValueFluctuation(base_precision_lens_value);
       engineFluctuation        = GetItemSellValueFluctuation(base_engine_value);

       harpoonEvent =
          (worldEvent == (int) WorldEventTypes.DeepSeaWarEvent) 
          ? WorldEventTypes.DeepSeaWarEvent
          : (worldEvent == (int)WorldEventTypes.IndustrialGoldRushEvent) 
          ? WorldEventTypes.IndustrialGoldRushEvent
          : (worldEvent == (int)WorldEventTypes.ScavengersHolidayEvent)
          ? WorldEventTypes.ScavengersHolidayEvent
          : WorldEventTypes.HarpoonEvent;

       crudeToolEvent =
          (worldEvent == (int)WorldEventTypes.IndustrialGoldRushEvent) 
          ? WorldEventTypes.IndustrialGoldRushEvent
          : (worldEvent == (int)WorldEventTypes.ScavengersHolidayEvent)
          ? WorldEventTypes.ScavengersHolidayEvent
          : WorldEventTypes.CrudeToolEvent;

       pressureValveEvent =
          (worldEvent == (int)WorldEventTypes.ScavengersHolidayEvent) 
          ? WorldEventTypes.ScavengersHolidayEvent
          : WorldEventTypes.PressureValveEvent;

       divingBellEvent =
          (worldEvent == (int)WorldEventTypes.ScavengersHolidayEvent) 
          ? WorldEventTypes.ScavengersHolidayEvent
          : WorldEventTypes.DivingBellEvent;

       precisionLensEvent =
          (worldEvent == (int)WorldEventTypes.ScavengersHolidayEvent) 
          ? WorldEventTypes.ScavengersHolidayEvent
          : WorldEventTypes.PressureValveEvent;

       engineEvent =
          (worldEvent == (int)WorldEventTypes.ScavengersHolidayEvent) 
          ? WorldEventTypes.ScavengersHolidayEvent
          : WorldEventTypes.ClockworkEngineEvent;

   
       // 2. Update UI Previews based on these exact rolls
       UpdateMarketPreviewUI(
           ItemType.CrudeTool, crudeToolEvent, CRUDE_TOOL_TAG,
           MIN_CRUDE_TOOL_VALUE, MAX_CRUDE_TOOL_VALUE, base_crude_tool_value,
           crudeToolChance, crudeToolFluctuation, 1);
        
       UpdateMarketPreviewUI(
          ItemType.Harpoon, harpoonEvent, HARPOON_TAG,
          MIN_HARPOON_VALUE, MAX_HARPOON_VALUE, base_harpoon_value,
          harpoonChance, harpoonFluctuation, 1);

      UpdateMarketPreviewUI(
             ItemType.DivingBell, divingBellEvent, DIVING_BELL_TAG,
             MIN_DIVING_BELL_VALUE, MAX_DIVING_BELL_VALUE, base_diving_bell_value,
             divingBellChance, divingBellFluctuation, 1);

      if (ForgeManager.Instance.hasTier2Blueprint) 
       {
           UpdateMarketPreviewUI(
              ItemType.PressureValve, pressureValveEvent, PRESSURE_VALVE_TAG,
              MIN_PRESSURE_VALVE_VALUE, MAX_PRESSURE_VALVE_VALUE, base_pressure_valve_value,
              pressureValveChance, pressureValveFluctuation, 2);

          
       }
   
       if (ForgeManager.Instance.hasTier3Blueprint) 
       {
           UpdateMarketPreviewUI(
              ItemType.PrecisionLens, precisionLensEvent, PRECISION_LENS_TAG,
              MIN_PRECISION_LENS_VALUE, MAX_PRECISION_LENS_VALUE, base_precision_lens_value,
              precisionLensChance, precisionLensFluctuation, 3);

           UpdateMarketPreviewUI(
              ItemType.Engine, engineEvent, ENGINE_TAG,
              MIN_ENGINE_VALUE, MAX_ENGINE_VALUE, base_engine_value,
              engineChance, engineFluctuation, 3);
       }
   }

   private int GetItemSellValueFluctuation(int itemBaseValue) 
   {
      float fluctuation,
            currentShift;

      currentShift = (float)Rng.NextDouble() * (marketShiftMax - marketShiftMin) + marketShiftMin;

      fluctuation = Mathf.Abs(itemBaseValue - (itemBaseValue * currentShift));

      return Mathf.RoundToInt(fluctuation);
   }
   
   // Helper method to keep your UI updates clean and perfectly matched to the math
   private void UpdateMarketPreviewUI(
      ItemType itemType, WorldEventTypes eventType, string itemTag,
      int minSellCost, int maxSellCost, int baseValue,
      int chance, int fluctuation, int itemTier)
   {
       int currentVal    = GetItemValue(itemType),
           preview       = currentVal;

       currentWorldEventChance 
          = itemTier == TIER_ONE 
          ? TIER_ONE_CHANCE 
          : itemTier == TIER_TWO
          ? TIER_TWO_CHANCE
          : itemTier == TIER_THREE
          ? TIER_THREE_CHANCE
          : TIER_ONE_CHANCE;

       // World event market preview
       if (worldEvent == (int)eventType && TurnManager.Instance.eventCountdown == WORLD_EVENT_PREVIEW_TURN)
       {
          switch(worldEvent) 
          { 
             case (int)WorldEventTypes.DeepSeaWarEvent:
                preview += (GetItemValue(itemType) * 3) - GetItemValue(itemType);
                break;

             case (int)WorldEventTypes.IndustrialGoldRushEvent:
                if (itemType == ItemType.CrudeTool)
                   preview = base_pressure_valve_value;
                else
                  if (itemType == ItemType.Harpoon)
                   preview = base_diving_bell_value;
                break;

             case (int) WorldEventTypes.ScavengersHolidayEvent:
                preview -=  Mathf.RoundToInt(currentVal * .25f);
                break;

             default:
                if (shiftDirection <= currentWorldEventChance)
                   preview += currentVal;
                else
                   preview -= currentVal;
                break;
          }
       }
       else
       {
          // Market reset preview
          if (worldEvent == (int) eventType && TurnManager.Instance.eventCountdown == WORLD_EVENT_ACTIVE_TURN) 
          {
             switch(worldEvent) 
             { 
                case (int)WorldEventTypes.DeepSeaWarEvent:
                   preview = base_harpoon_value;
                   break;

                case (int)WorldEventTypes.IndustrialGoldRushEvent:
                   if (itemType == ItemType.CrudeTool)
                      preview = base_crude_tool_value;
                   else
                     if (itemType == ItemType.Harpoon)
                        preview = base_harpoon_value;
                   break;

                  case (int) WorldEventTypes.ScavengersHolidayEvent:
                     preview += Mathf.RoundToInt(currentVal * .25f);
                     break;

                default:
                   if (shiftDirection <= currentWorldEventChance)
                      preview = currentVal - baseValue;
                   else
                      preview = currentVal + baseValue;
                   break;
             }
          }
          
          // Standard Fluctuation Preview
          else
          { 
             if (chance <= 30) 
                preview += fluctuation;
             else 
               if (chance <= 60) 
                  preview -= fluctuation;
          }
       }
   
       preview = Mathf.Clamp(preview, minSellCost, maxSellCost);
       
       Transform itemUI = SellItems.Find(item => item.CompareTag(itemTag));

       if(itemUI != null)
           itemUI.Find("NextValue").GetComponent<TextMeshProUGUI>().text = "Next Value: " + preview.ToString();
       else
         Debug.LogError("Current Item is null");
   }

   // Shifts the sell market each turn and during world events
  public void MarketFluctuate() 
   { 
       ApplyStoredShift(
          ItemType.CrudeTool, crudeToolEvent, 
          crudeToolChance, crudeToolFluctuation, 
          TryIncreaseCrudeToolSellValue, TryDecreaseCrudeToolSellValue);
             
        ApplyStoredShift(
           ItemType.Harpoon, harpoonEvent, 
           harpoonChance, harpoonFluctuation, 
           TryIncreaseHarpoonSellValue, TryDecreaseHarpoonSellValue);

      ApplyStoredShift(
              ItemType.DivingBell, divingBellEvent,
              divingBellChance, divingBellFluctuation,
              TryIncreaseDivingBellValue, TryDecreaseDivingBellValue);

      // Apply Tier 2
      if (ForgeManager.Instance.hasTier2Blueprint) 
       {
           ApplyStoredShift(
              ItemType.PressureValve, pressureValveEvent, 
              pressureValveChance, pressureValveFluctuation, 
              TryIncreasePressureValveValue, TryDecreasePressureValveValue);
       }
   
       // Apply Tier 3
       if (ForgeManager.Instance.hasTier3Blueprint) 
       {
           ApplyStoredShift(
              ItemType.PrecisionLens, precisionLensEvent, 
              precisionLensChance, precisionLensFluctuation, 
              TryIncreasePrecisionLensValue, TryDecreasePrecisionLensValue);

           ApplyStoredShift(
              ItemType.Engine, engineEvent, 
              engineChance, engineFluctuation, 
              TryIncreaseEngineSellValue, TryDecreaseEngineSellValue);
       }
   }
   
   // Helper method to execute the exact math we promised the player
   private void ApplyStoredShift(
      ItemType itemType, WorldEventTypes eventType,
      int change, int fluctuation, 
      Action<int> increaseSellValueMethod, Action<int> decreaseSellValueMethod)
   {
       // Is it the active World Event turn?
       if (worldEvent == (int)eventType && TurnManager.Instance.eventCountdown == WORLD_EVENT_ACTIVE_TURN)
       {
           switch (worldEvent) 
           {
              case (int) WorldEventTypes.IndustrialGoldRushEvent:
                 if(itemType == ItemType.CrudeTool)
                    increaseSellValueMethod(base_pressure_valve_value - GetItemValue(itemType));
                 else
                    increaseSellValueMethod(base_diving_bell_value - GetItemValue(itemType));
                 break;

              case (int) WorldEventTypes.DeepSeaWarEvent:
                 WorldEventSideEffect(eventType);
                 increaseSellValueMethod((GetItemValue(itemType) * 3) - GetItemValue(itemType));
                 break;

              case (int) WorldEventTypes.ScavengersHolidayEvent:
                decreaseSellValueMethod((int)(GetItemValue(itemType) * .25f));
                scavengerHolidayDeduction[itemType] = (int) (GetItemValue(itemType) * .25f);
                break;

              default:
                 if (shiftDirection <= currentWorldEventChance) 
                    increaseSellValueMethod(GetItemValue(itemType));
                 else 
                    decreaseSellValueMethod(GetItemValue(itemType));
                 break;
           }
       }

       // Is it a post-event Reset Turn? (Skip natural fluctuation)
       else 
       { 
          if (lastResetTurn.ContainsKey(itemType) && lastResetTurn[itemType] && TurnManager.Instance.eventCountdown == WORLD_EVENT_RESET_TURN)
              lastResetTurn[itemType] = false;
          else
          {
              if (change <= 30) 
                 increaseSellValueMethod(fluctuation);
              else 
                 if (change <= 60) 
                    decreaseSellValueMethod(fluctuation);
          }
       }
   }

   // Determines world event selection and shift direction for the next cycle.
   public void WorldEventChance() 
   {

      int worldEvent1 = (int)WorldEventTypes.CrudeToolEvent,
          finalWorldEvent;

      if(ForgeManager.Instance.hasTier3Blueprint)
         finalWorldEvent = (int)WorldEventTypes.ScavengersHolidayEvent;
      else
         if(ForgeManager.Instance.hasTier2Blueprint)
            finalWorldEvent = (int)(WorldEventTypes.DeepSeaWarEvent);
         else
            finalWorldEvent = (int)(WorldEventTypes.IndustrialGoldRushEvent);


      worldEvent       = Rng.Next(worldEvent1, finalWorldEvent + 1);
      WorldEventItemVisual(worldEvent);
      shiftDirection   = isTier3BuffACtive ?  50 : Rng.Next(MARKET_CHANCE_MIN, MARKET_CHANCE_MAX);
      worldEventChange.sprite = shiftDirection < 50 ? worldEventSymbols.Find("increaseSymbol").GetComponent<Image>().sprite 
                                                    : worldEventSymbols.Find("decreaseSymbol").GetComponent<Image>().sprite;
   }

   public void WorldEventItemVisual(int worldEvent) 
   {
      switch (worldEvent) 
      {
         case (int) WorldEventTypes.CrudeToolEvent:
            worldEventItem.sprite = GetItemSprite(ItemType.CrudeTool);
            break;
         case (int) WorldEventTypes.HarpoonEvent:
            worldEventItem.sprite = GetItemSprite(ItemType.Harpoon);
            break;
         case (int) WorldEventTypes.DivingBellEvent:
            worldEventItem.sprite = GetItemSprite(ItemType.DivingBell);
            break;
         case (int) WorldEventTypes.PressureValveEvent:
            worldEventItem.sprite = GetItemSprite(ItemType.PressureValve);
            break;
         case (int) WorldEventTypes.DeepSeaWarEvent:
            worldEventItem.sprite= GetItemSprite(ItemType.Harpoon);
            break;
         case (int) WorldEventTypes.PrecisionLensEvent:
            worldEventItem.sprite = GetItemSprite(ItemType.PrecisionLens);
            break;
         case (int) WorldEventTypes.ClockworkEngineEvent:
            worldEventItem.sprite = GetItemSprite(ItemType.Engine);
            break;
         default:
            Debug.LogError("Unkown Item: " +  worldEvent);
            break;
      }
   }

   public void WorldEventNewsTickerText() 
   {
      switch (worldEvent) 
      { 
         case (int)WorldEventTypes.CrudeToolEvent:
            currrentNewsTickerMessage = GetCrudeToolTickerMessage(shiftDirection);
            break;
         case (int)WorldEventTypes.HarpoonEvent:
            currrentNewsTickerMessage = GetHarpoonTickerMessage(shiftDirection);
            break;
         case (int)WorldEventTypes.IndustrialGoldRushEvent:
            currrentNewsTickerMessage = GetIndustrialGoldRushTickerMessage();
            break;
         case (int)WorldEventTypes.PressureValveEvent:
            currrentNewsTickerMessage = GetPressureValveMessage(shiftDirection);
            break;
         case (int)WorldEventTypes.DivingBellEvent:
            currrentNewsTickerMessage = GetDivingBellTickerMessage(shiftDirection);
            break;
         case (int)WorldEventTypes.DeepSeaWarEvent:
            currrentNewsTickerMessage = GetDeepSeaWarTickerMessage();
            break;
         case (int)WorldEventTypes.PrecisionLensEvent:
            currrentNewsTickerMessage = GetPrecisionLensTickerMessage(shiftDirection);
            break;
         case (int)WorldEventTypes.ClockworkEngineEvent:
            currrentNewsTickerMessage = GetClockWorkEngineMessage(shiftDirection);
            break;
         case (int)WorldEventTypes.ScavengersHolidayEvent:
            currrentNewsTickerMessage = GetScavengersHolidayTickerMessage();
            break;
         default:
            Debug.LogError("Unknown Event");
            break;
      }
   }

  public void ResetWorldEventShifts() 
  {
     // Determine which world event was active and reverse its effect on item values
     switch (worldEvent) 
     {
         // Undo the crude tool event shift based on previous direction
         case (int)WorldEventTypes.CrudeToolEvent:
           if (shiftDirection <= TIER_ONE_CHANCE) 
              TryDecreaseCrudeToolSellValue(GetItemValue(ItemType.CrudeTool) - base_crude_tool_value);
           else
              TryIncreaseCrudeToolSellValue(base_crude_tool_value - GetItemValue(ItemType.CrudeTool));

            // Flag the reset for crude tool
            lastResetTurn[ItemType.CrudeTool] = true;
            break;

         // Undo the crude tool event shift based on previous direction
         case (int)WorldEventTypes.HarpoonEvent:
            if (shiftDirection <= TIER_ONE_CHANCE) 
               TryDecreaseHarpoonSellValue(GetItemValue(ItemType.Harpoon) - base_harpoon_value);
            else 
               TryIncreaseHarpoonSellValue(base_harpoon_value - GetItemValue(ItemType.Harpoon));

            // Flag the reset for harpoon
            lastResetTurn[ItemType.Harpoon] = true;
            break;

         // Undo the diving bell event shift based on previous shift
         case (int)WorldEventTypes.DivingBellEvent:
            if (shiftDirection <= TIER_ONE_CHANCE)
               TryDecreaseDivingBellValue(GetItemValue(ItemType.DivingBell) - base_diving_bell_value);
            else
               TryIncreaseDivingBellValue(base_diving_bell_value - GetItemValue(ItemType.DivingBell));

            lastResetTurn[ItemType.DivingBell] = true;
            break;

         case (int)WorldEventTypes.IndustrialGoldRushEvent:
            TryDecreaseCrudeToolSellValue(base_pressure_valve_value - base_crude_tool_value);
            TryDecreaseHarpoonSellValue(base_diving_bell_value - base_harpoon_value);

            // Flag the reset for harpoon
            lastResetTurn[ItemType.CrudeTool] = true;
            lastResetTurn[ItemType.Harpoon]   = true;
            break;

         // Undo the pressure valve event shift based on previous direction
         case (int)WorldEventTypes.PressureValveEvent:
           if (shiftDirection <= TIER_TWO_CHANCE)
              TryDecreasePressureValveValue(GetItemValue(ItemType.PressureValve) - base_pressure_valve_value);
           else
              TryIncreasePressureValveValue(base_pressure_valve_value - GetItemValue(ItemType.PressureValve));
  
           // Flag the reset for Pressure Valve
           lastResetTurn[ItemType.PressureValve] = true;
           break;


         case (int)WorldEventTypes.DeepSeaWarEvent:
            TryDecreaseHarpoonSellValue(GetItemValue(ItemType.Harpoon) - base_harpoon_value);

            // Flag the reset for harpoon
            lastResetTurn[ItemType.Harpoon] = true;
            break;


         // Undo the precision lens event shift based on previous shift
         case (int)WorldEventTypes.PrecisionLensEvent:
            if (shiftDirection <= TIER_THREE_CHANCE)
               TryDecreasePrecisionLensValue(GetItemValue(ItemType.PrecisionLens) - base_precision_lens_value);
            else
               TryIncreasePrecisionLensValue(base_precision_lens_value - GetItemValue(ItemType.PrecisionLens));

            lastResetTurn[ItemType.PrecisionLens] = true;
            break;

         // Undo the engine event shift based on previous direction
         case (int)WorldEventTypes.ClockworkEngineEvent:
           if (shiftDirection <= TIER_THREE_CHANCE)
              TryDecreaseEngineSellValue(GetItemValue(ItemType.Engine) - base_engine_value);
           else
              TryIncreaseEngineSellValue(base_engine_value - GetItemValue(ItemType.Engine));
  
           // Flag the reset for Engine
           lastResetTurn[ItemType.Engine] = true;
           break;

         case (int)WorldEventTypes.ScavengersHolidayEvent:
            TryIncreaseCrudeToolSellValue(scavengerHolidayDeduction[ItemType.CrudeTool]);
            TryIncreaseHarpoonSellValue(scavengerHolidayDeduction[ItemType.Harpoon]);
            TryIncreasePressureValveValue(scavengerHolidayDeduction[ItemType.PressureValve]);
            TryIncreaseDivingBellValue(scavengerHolidayDeduction[ItemType.DivingBell]);
            TryIncreasePrecisionLensValue(scavengerHolidayDeduction[ItemType.PrecisionLens]);
            TryIncreaseEngineSellValue(scavengerHolidayDeduction[ItemType.Engine]);

            // Flag the reset for Engine
            lastResetTurn[ItemType.CrudeTool]     = true;
            lastResetTurn[ItemType.Harpoon]       = true;
            lastResetTurn[ItemType.PressureValve] = true;
            lastResetTurn[ItemType.DivingBell]    = true;
            lastResetTurn[ItemType.PrecisionLens] = true;
            lastResetTurn[ItemType.Engine]        = true;
            break;
  
        // Unknown event should be logged for debugging
        default:
           Debug.LogError("Unknown Event");
           break;
     }
  }

   public void ChangeItemValueText(int newAmount, ItemType itemType) 
   {
      Transform currentItem;

      switch (itemType) 
      {
         case ItemType.CrudeTool:
            currentItem = SellItems.Find(d => d.CompareTag(CRUDE_TOOL_TAG));
            break;
         case ItemType.Harpoon:
            currentItem = SellItems.Find(d => d.CompareTag(HARPOON_TAG));
            break;
         case ItemType.PressureValve:
            currentItem = SellItems.Find(d => d.CompareTag(PRESSURE_VALVE_TAG));
            break;
         case ItemType.DivingBell:
            currentItem = SellItems.Find(d => d.CompareTag(DIVING_BELL_TAG));
            break;
         case ItemType.PrecisionLens:
            currentItem = SellItems.Find(d => d.CompareTag(PRECISION_LENS_TAG));
            break;
         case ItemType.Engine:
            currentItem = SellItems.Find(d => d.CompareTag(ENGINE_TAG));
            break;
         default:
            currentItem = null;
            Debug.LogError("Unkown item");
            break;
      }

      if (currentItem != null)
         currentItem.Find("ItemValue").GetComponent<TextMeshProUGUI>().text = newAmount.ToString();

      return;
   }

   // Handles the main button clicks (Trade, Info, Upgrade) to open the corresponding panel
   public void RequestTradeHutPanel(int buttonID) 
   {
      switch (buttonID) 
      {
         case TRADE_BUTTON:
            ShowTradePanel();

            // Handle Sell Button
            Button sellBtn = SellWindow.Find("SellButton").GetComponent<Button>();
            sellBtn.onClick.RemoveAllListeners();
            sellBtn.onClick.AddListener(() => SellItem());

            // Handle Buy Button
            Button buyBtn = BuyWindow.Find("BuyButton").GetComponent<Button>();
            buyBtn.onClick.RemoveAllListeners();
            buyBtn.onClick.AddListener(() => BuyItem());

            // Handle Exit Button
            Button exitBtnTrade = TradePanels.Find("ExitButton").GetComponent<Button>();
            exitBtnTrade.onClick.RemoveAllListeners();
            exitBtnTrade.onClick.AddListener(() => CloseTradeHutPanel(TRADE_BUTTON));
            break;

         case INFO_BUTTON:
            ShowInfoPanel();

            // Handle Info Exit Button
            Button exitBtnInfo = InfoPanel.Find("ExitButton").GetComponent<Button>();
            exitBtnInfo.onClick.RemoveAllListeners();
            exitBtnInfo.onClick.AddListener(() => CloseTradeHutPanel(INFO_BUTTON));
            break;

         default:
            Debug.Log("Building Panel: Unknown button ID.");
            break;
      }
   }

   // Closes the panel corresponding to the button ID
   public void CloseTradeHutPanel(int buttonID)    
   {
      switch (buttonID)
      {
         case TRADE_BUTTON:
            CloseTradePanel();
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

   private void ShowTradePanel() 
   {
      TradePanels.transform.localScale = TradePanelScale;
      Debug.Log(TradePanels.transform.localScale);
      panelManager.OpenPanel(TradePanels.gameObject);

      // Ensure Sell tab is visible, Buy is not
      SellPanel.gameObject.SetActive(true);
      BuyPanel.gameObject.SetActive(false);

      ShowSellPanel();

      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(false);
   }

   private void ShowInfoPanel() 
   {
      InfoPanel.transform.localScale = InfoPanelScale;
      panelManager.OpenPanel(InfoPanel.gameObject);

      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(false);
   }

   public void ShowSellPanel() 
   {
      if(BuyPanel.gameObject.activeSelf)
      {
         BuyPanel.gameObject.SetActive(false);
      }
      else
         SellPanel.gameObject.SetActive(true);


      /*if(tutorialFunctionTwo)
      {
         SellPanel.Find("Arrow").gameObject.SetActive(false);
         SellPanel.Find("Arrow2").gameObject.SetActive(false);
         SellPanel.Find("FirstText").gameObject.SetActive(false);
         SellPanel.Find("SecondText").gameObject.SetActive(false);
         SellPanel.Find("Arrow3").gameObject.SetActive(true);
         SellPanel.Find("ThirdText").gameObject.SetActive(true);
         SellPanel.Find("FourthText").gameObject.SetActive(true);
      }*/

      // Destroy the instantiated buy item/window instance if it exists
      if (currentBuyItem != null) 
      {
         Destroy(currentBuyItem.gameObject);
         currentBuyItem = null;
      }
      SellPanel.gameObject.SetActive(true);

      if (tutorialFunctionOne)
      {
         if (BuyPanel.Find("TutorialPart2").gameObject.activeSelf)
         {
            BuyPanel.Find("TutorialPart2").gameObject.SetActive(false);
            SellPanel.Find("TutorialPart3").gameObject.SetActive(true);
         }
         else
            SellPanel.Find("TutorialPart1").gameObject.SetActive(true);
      }
   }

   public void ShowBuyPanel() 
   {
      if(SellPanel.gameObject.activeSelf)
      {
         SellPanel.gameObject.SetActive(false);  
         BuyPanel.gameObject.SetActive(true);
      }

      if (tutorialFunctionOne)
      {
         SellPanel.Find("TutorialPart1").gameObject.SetActive(false);
         BuyPanel.Find("TutorialPart2").gameObject.SetActive(true);
      }

      // Destroy the instantiated sell item/window instance if it exists
      if (currentSellItem != null) 
      {
         Destroy(currentSellItem.gameObject);
         currentSellItem = null;
      }
      //BuyPanel.gameObject.SetActive(true);
   }

   public void ShowBuyWindow() 
    {
      BuyWindow.gameObject.SetActive(true);
   }

   public void ShowSellWindow() 
   {
      SellWindow.gameObject.SetActive(true);
   }

   //public void ShowMysteryBoxPanel() 
   //{
   //   MysteryBoxPanel.gameObject.SetActive(true);
   //   MysteryBoxPanel.Find("StartingView").gameObject.SetActive(true);
   //}

   private void CloseTradePanel() 
   {
      if(tutorialFunctionOne)
      {
         SellPanel.Find("TutorialPart6").gameObject.SetActive(false);
         tutorialFunctionOne = false;
         HandleTutorial?.Invoke();
      }

      if(tutorialFunctionTwo && SellPanel.Find("Arrow4").gameObject.activeSelf)
      {
         BuyPanel.Find("Arrow4").gameObject.SetActive(false);
         BuyPanel.Find("SixthText").gameObject.SetActive(false);
         BuyPanel.Find("SeventhText").gameObject.SetActive(false);
         HandleTutorial?.Invoke();
      }

      // Destroy the instantiated sell item/window instance if it exists
      if (currentSellItem != null) 
      {
         Destroy(currentSellItem.gameObject);
         currentSellItem = null;
      }

      // Destroy the instantiated buy item/window instance if it exists
      if (currentBuyItem != null) 
      {
         Destroy(currentBuyItem.gameObject);
         currentBuyItem = null;
      }

      crudeToolSellCount = MIN_SELL_ITEM_COUNT;
      harpoonSellCount   = MIN_SELL_ITEM_COUNT;
      engineSellCount    = MIN_SELL_ITEM_COUNT;
      rawOreExchange     = MIN_BUY_ITEM_COUNT;

      if (SellWindow.gameObject.activeSelf)
         CloseSellWindow();

      if (BuyWindow.gameObject.activeSelf)
         CloseBuyWindow();

      panelManager.ClosePanel(TradePanels.gameObject);

      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(true);
   }

   private void CloseInfoPanel()
   {
      panelManager.ClosePanel(InfoPanel.gameObject);

      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(true);
   }

   private void CloseSellPanel() 
   {
      SellPanel.gameObject.SetActive(false);
   }

   private void CloseBuyPanel() 
   {
      BuyPanel.gameObject.SetActive(false);
   }

   private void CloseSellWindow() 
   {
      SellWindow.gameObject?.SetActive(false);
   }

   private void CloseBuyWindow() 
   {
      BuyWindow.gameObject?.SetActive(false);
   }

   //public void CloseMysteryBoxPanel() 
   //{
   //   if(currentMysteryBoxResult != null) 
   //   {
   //      Destroy(currentMysteryBoxResult.gameObject);
   //      currentMysteryBoxResult = null;
   //   }
   //   MysteryBoxPanel.gameObject.SetActive(false);
   //}

}