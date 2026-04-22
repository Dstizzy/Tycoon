using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using TMPro;

using UnityEngine;
using UnityEngine.UI;

using static InventoryManager;
using static Item;
using static Resources;
using static TickerSystem;
using static WorldEvents;

public class TradeHutManager : MonoBehaviour 
{

   public bool recycleOpened = false;
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

   public string currentNewsTickerMessage;

   // Random number generator
   private readonly static System.Random Rng = new System.Random(); 

   // Keeps track of the last item that had a world event
   private readonly Dictionary<ItemType, bool> lastResetTurn = new Dictionary<ItemType, bool>();

   private Vector3 TradePanelScale;
   private Vector3 InfoPanelScale;

   // Transforms
   private Transform currentBuyItem,    
                     currentSellItem;

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

               mercenaryEngineerAvailable;

   public Image  worldEventChange;
   public Transform worldEventIcon;
   public TextMeshProUGUI mercenaryEngineersAvailableText;


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
   public int recycleCounter = 0;


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

                    SELL_ITEM_SPACING = 28,
                    BUY_ITEM_SPACING  = 25,

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

   public static event Action<int> HandleTutorial;

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
      // 1. Initialize lists for this specific scene instance
      SellItems = new List<Transform>();
      BuyItems = new List<Transform>();

      // 2. Assign the static Instance so other scripts can still find THIS scene's manager
      Instance = this;

      // 3. Cache initial scales (Ensure these are assigned in the Inspector!)
      if (TradePanels != null) TradePanelScale = TradePanels.transform.localScale;
      if (InfoPanel != null) InfoPanelScale = InfoPanel.transform.localScale;

      // 4. Initialize logic
      foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
         lastResetTurn[itemType] = false;

      OnItemValueChange = ChangeItemValueText;
      mercenaryEngineerAvailable = MAX_MERCENARY_ENGINEER_COUNT;

      // 5. Safety Checks & Initial State (Simplified)
      if (TradePanels != null) CloseTradePanel();
      if (InfoPanel != null) CloseInfoPanel();
      if (SellWindow != null) CloseSellWindow();
      if (SellPanel != null) CloseSellPanel();
      if (BuyPanel != null) CloseBuyPanel();
      if (BuyWindow != null) BuyWindow.gameObject.SetActive(false);
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
      inv    = InventoryManager.Instance;
      ticker = TickerSystem.Instance;

      CreateSellItem(GetItemSprite(ItemType.CrudeTool), GetItemValue(ItemType.CrudeTool), 0.0f, CRUDE_TOOL_TAG);
      CreateSellItem(GetItemSprite(ItemType.Harpoon), GetItemValue(ItemType.Harpoon), 2.5f, HARPOON_TAG);
      CreateSellItem(GetItemSprite(ItemType.DivingBell), GetItemValue(ItemType.DivingBell), 0.0f, DIVING_BELL_TAG, -85);

      CreateSellItem(GetItemSprite(ItemType.PressureValve), GetItemValue(ItemType.PressureValve), 2.5f, PRESSURE_VALVE_TAG, -85);

      CreateSellItem(GetItemSprite(ItemType.PrecisionLens), GetItemValue(ItemType.PrecisionLens), 0.0f, PRECISION_LENS_TAG, -160);
      CreateSellItem(GetItemSprite(ItemType.Engine), GetItemValue(ItemType.Engine), 2.5f, ENGINE_TAG, -160);

      CreateBuyItem(GetItemSprite(ItemType.RawOreChunk), GetItemPrice(ItemType.RawOreChunk), 0.0f, RAW_ORE_CHUNK_TAG);
      CreateBuyItem(GetItemSprite(ItemType.IndustrialBlueprint), GetItemPrice(ItemType.IndustrialBlueprint), 1.2f, INDUSTRIAL_BLUEPRINT_TAG);
      CreateBuyItem(GetItemSprite(ItemType.ClockworkBlueprint), GetItemPrice(ItemType.ClockworkBlueprint), 0.0f, CLOCKWORK_BLUEPRINT_TAG, -30);
      CreateBuyItem(GetItemSprite(ItemType.MercenaryEngineer), GetItemPrice(ItemType.MercenaryEngineer), 1.2f, MERCENARY_ENGINEER_TAG, -30);
      CreateBuyItem(GetItemSprite(ItemType.InsurancePolicy), GetItemPrice(ItemType.InsurancePolicy), 0.0f, INSURANCE_POLICY_TAG, -60);
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
      tradeItemRectTransform.anchoredPosition = new Vector2(SELL_ITEM_SPACING * positionIndex, verticalIndex);

      // Populate the the item properties                                                           
      sellValueText      = tradeItemTransform.Find("ItemValue").GetComponent<TextMeshProUGUI>();
      sellValueText.text = itemValue.ToString();

      tradeItemTransform.Find("ItemShadow").GetComponent<Image>().sprite = itemSprite;
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

      if (itemTag != CRUDE_TOOL_TAG && itemTag != HARPOON_TAG && itemTag != DIVING_BELL_TAG) 
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

      if(itemTag == MERCENARY_ENGINEER_TAG) 
      {
         mercenaryEngineersAvailableText.text = "Availability: " + mercenaryEngineerAvailable.ToString();
         mercenaryEngineersAvailableText.gameObject.SetActive(true);
      }

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

      if(tutorialFunctionTwo && itemTag == CRUDE_TOOL_TAG)
      {
         HandleTutorial?.Invoke(1);
         TradePanels.transform.Find("Screen1").gameObject.SetActive(false);
         TradePanels.transform.Find("Screen2").gameObject.SetActive(false);
         TradePanels.transform.Find("Circle1").gameObject.SetActive(false);
         TradePanels.transform.Find("Circle2").gameObject.SetActive(true);
         TradePanels.transform.Find("Screen3").gameObject.SetActive(true);
         TradePanels.transform.Find("Screen4").gameObject.SetActive(true);
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

      buyWindowTemplate.gameObject.SetActive(false);

      // Destroy the previously opened buy window instance before creating a new one
      if (currentBuyItem != null) 
      {
         Destroy(currentBuyItem.gameObject);
         currentBuyItem = null;
      }

      Transform     buyWindowItemTransfrom              = Instantiate(buyWindowTemplate, buyWindowContainer);
      RectTransform buyItemTransfromRectTransform = buyWindowItemTransfrom.GetComponent<RectTransform>();

      buyWindowItemTransfrom.tag = itemTag;

      // Populate item properties
      buyWindowItemTransfrom.Find("ItemImage").GetComponent<Image>().sprite        = itemSprite;
      buyWindowItemTransfrom.Find("ItemName").GetComponent<TextMeshProUGUI>().text = itemTag;
      buyWindowItemTransfrom.Find("currencyIcon").GetComponent<Image>().sprite     = currencySprite;

      switch(itemTag) 
      {
         case RAW_ORE_CHUNK_TAG:
            rawOreExchange = 0;
            buyWindowItemTransfrom.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = rawOreExchange.ToString();
            break;
         case MERCENARY_ENGINEER_TAG:
            mercenaryEngineerBuyCount = 0;
            buyWindowItemTransfrom.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = mercenaryEngineerBuyCount.ToString();
            break;
         default:
            buyWindowItemTransfrom.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = 1.ToString();
            break;
      }

      switch (itemTag) 
      {
         case INDUSTRIAL_BLUEPRINT_TAG:
            buyWindowItemTransfrom.Find("currencySpent").GetComponent<TextMeshProUGUI>().text = GetItemPrice(ItemType.IndustrialBlueprint).ToString();
            break;
         case CLOCKWORK_BLUEPRINT_TAG:
            buyWindowItemTransfrom.Find("currencySpent").GetComponent<TextMeshProUGUI>().text = GetItemPrice(ItemType.ClockworkBlueprint).ToString();
            break;
         case MERCENARY_ENGINEER_TAG:
            buyWindowItemTransfrom.Find("currencySpent").GetComponent<TextMeshProUGUI>().text = "0";
            break;
         case RAW_ORE_CHUNK_TAG:
            buyWindowItemTransfrom.Find("currencySpent").GetComponent<TextMeshProUGUI>().text = "0";
            break;
         case INSURANCE_POLICY_TAG:
            buyWindowItemTransfrom.Find("currencySpent").GetComponent<TextMeshProUGUI>().text = GetItemPrice(ItemType.InsurancePolicy).ToString();
            break;
         default:
            Debug.LogError("Unkown item: " + itemTag);
            break;
      }

      buyWindowItemTransfrom.Find("QuantityButtons/IncreaseButton").gameObject.SetActive(false);
      buyWindowItemTransfrom.Find("QuantityButtons/DecreaseButton").gameObject.SetActive(false);

      if (itemTag  == RAW_ORE_CHUNK_TAG || itemTag == MERCENARY_ENGINEER_TAG)
      {
         buyWindowItemTransfrom.Find("QuantityButtons/IncreaseButton").gameObject.SetActive(true);
         buyWindowItemTransfrom.Find("QuantityButtons/DecreaseButton").gameObject.SetActive(true);

         // Get references to the increase and decrease buttons
         Button increaseButton = buyWindowItemTransfrom.Find("QuantityButtons/IncreaseButton").GetComponent<Button>();
         Button decreaseButton = buyWindowItemTransfrom.Find("QuantityButtons/DecreaseButton").GetComponent<Button>();
         
         // Dynamically add listeners to the buttons, which increases or decreases the buy item count
         increaseButton.onClick.AddListener(() => IncreaseBuyItemsCount(buyWindowItemTransfrom));
         decreaseButton.onClick.AddListener(() => DecreaseBuyItemsCount(buyWindowItemTransfrom));
      }

      // Store the reference to the newly created buy window instance
      currentBuyItem = buyWindowItemTransfrom;
      buyWindowItemTransfrom.gameObject.SetActive(true);
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

               if(tutorialFunctionTwo)
               {
                  HandleTutorial?.Invoke(1);
                  TradePanels.transform.Find("Screen5").gameObject.SetActive(false);
                  TradePanels.transform.Find("Screen6").gameObject.SetActive(false);
                  TradePanels.transform.Find("Circle3").gameObject.SetActive(false);
                  TradePanels.transform.Find("Circle4").gameObject.SetActive(true);
                  TradePanels.transform.Find("Screen7").gameObject.SetActive(true);
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

         case DIVING_BELL_TAG:
            if (divingBellSellCount > MIN_SELL_ITEM_COUNT) 
            {
               
               if (inv.TryUseDivingBell(divingBellSellCount))
               {
                  soldCount = divingBellSellCount;
                  totalSellValue += soldCount * GetItemValue(ItemType.DivingBell);
                  successMessage = $"Sold {soldCount} Diving Bell{(soldCount > 1 ? "s" : "")} for {totalSellValue} pearls.";
               }
               else
                  divingBellSellCount = MIN_SELL_ITEM_COUNT;
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
         case PRECISION_LENS_TAG:
            if (precisionLensSellCount > MIN_SELL_ITEM_COUNT) 
            {
               
               if (inv.TryUsePrecisionLens(precisionLensSellCount))
               {
                  soldCount = precisionLensSellCount;
                  totalSellValue += soldCount * GetItemValue(ItemType.PrecisionLens);
                  successMessage = $"Sold {soldCount} Precision Lens{(soldCount > 1 ? "es" : "")} for {totalSellValue} pearls.";
               }
               else
                  precisionLensSellCount = MIN_SELL_ITEM_COUNT;
            }
            break;

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
      divingBellSellCount    = MIN_SELL_ITEM_COUNT;
      pressureValveSellCount = MIN_SELL_ITEM_COUNT;
      precisionLensSellCount = MIN_SELL_ITEM_COUNT;
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
            ticker.ShowTicker("Purchased Tier 2 Blueprint. Pressure Valve unlocked.", Color.green, MessageTypes.ResultMessage);
         }

         // Tier 3 Blueprint purchase flow
         if (currentBuyItem.CompareTag(CLOCKWORK_BLUEPRINT_TAG) && inv.TrySpendPearl(GetItemPrice(ItemType.ClockworkBlueprint)))
         {
            // Grants player access to tier 3 blueprint content
            ForgeManager.Instance.hasTier3Blueprint = true;

            // Removes the tier 3 blueprint from the buy panel
            BuyItems.Find(item => item.CompareTag(CLOCKWORK_BLUEPRINT_TAG)).gameObject.SetActive(false);

            // Adds new craftable items (engine, precision lens) to inventory/craft list
            inv.CreateCraft(GetItemSprite(ItemType.Engine), ENGINE_POSITION, ENGINE_TAG, -450);
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
           
            ticker.ShowTicker("Purchased Tier 3 Blueprint. Engine and Precision Lens unlocked.", Color.green, MessageTypes.ResultMessage);
         }

         // Mercenary Engineer purchase flow
         if (currentBuyItem.CompareTag(MERCENARY_ENGINEER_TAG) && 
             inv.TrySpendPearl(mercenaryEngineerBuyCount * GetItemPrice(ItemType.MercenaryEngineer)) &&
             inv.TryAddMercenaryEngineer(mercenaryEngineerBuyCount)) 
         {
            mercenaryEngineerAvailable -= mercenaryEngineerBuyCount;
            mercenaryEngineersAvailableText.text =  "Availability: " + mercenaryEngineerAvailable.ToString();

            if (inv.InventoryItems?.Find(item => item.CompareTag(MERCENARY_ENGINEER_TAG)) == null) 
               inv.CreateCraft(GetItemSprite(ItemType.MercenaryEngineer), MERCENARY_ENGINEER_POSITION, MERCENARY_ENGINEER_TAG, -450);

            ForgeManager.Instance.hasMercenaryEngineer = inv.mercenaryEngineerCount > 0 ? true: false;

            if(mercenaryEngineerAvailable == 0) 
            { 
               BuyItems.Find(item => item.CompareTag(MERCENARY_ENGINEER_TAG)).gameObject.SetActive(false);
               mercenaryEngineersAvailableText.gameObject.SetActive(false);
            }
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
      if(tutorialFunctionTwo)
      {
         HandleTutorial?.Invoke(1);
         TradePanels.transform.Find("Screen3").gameObject.SetActive(false);
         TradePanels.transform.Find("Screen4").gameObject.SetActive(false);
         TradePanels.transform.Find("Circle2").gameObject.SetActive(false);
         TradePanels.transform.Find("Circle3").gameObject.SetActive(true);
         TradePanels.transform.Find("Screen5").gameObject.SetActive(true);
         TradePanels.transform.Find("Screen6").gameObject.SetActive(true);
      }
   }

   // Decrements the count for the item being sold and updates the UI
   public void DecreaseSellItemCount(Transform item) 
   {
      AdjustSellQuantity(item, -1);
   }

  public void AdjustSellQuantity(Transform item, int quantityChange) 
   {
      int multiplier = 1;

      // MODIFIER KEYS logic (Shift = 10x, Ctrl = 50x)
      if (Keyboard.current != null)
      {
          if (Keyboard.current.ctrlKey.isPressed) multiplier = 50;
          else if (Keyboard.current.shiftKey.isPressed) multiplier = 10;
      }

      int targetChange = quantityChange * multiplier;

      ItemType itemType = ItemType.CrudeTool;
      int      current  = 0;
      int      owned    = 0;

      switch (item.tag) 
      {
         case CRUDE_TOOL_TAG:
            current  = crudeToolSellCount;
            owned    = inv.crudeToolCount;
            itemType = ItemType.CrudeTool;
            break;
         case HARPOON_TAG:
            current  = harpoonSellCount;
            owned    = inv.harpoonCount;
            itemType = ItemType.Harpoon;
            break;
         case DIVING_BELL_TAG:
            current  = divingBellSellCount;
            owned    = inv.divingBellCount;
            itemType = ItemType.DivingBell;
            break;
         case PRESSURE_VALVE_TAG:
            current  = pressureValveSellCount;
            owned    = inv.pressureValveCount;
            itemType = ItemType.PressureValve;
            break;
         case PRECISION_LENS_TAG:
            current  = precisionLensSellCount;
            owned    = inv.precisionLensCount;
            itemType = ItemType.PrecisionLens;
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
      
      // Calculate additions/subtractions safely respecting bounds
      if (targetChange > 0)
      {
         int maxCanAdd = Mathf.Min(MAX_SELL_ITEM_COUNT - current, owned - current);

         if (maxCanAdd > 0)
            current += Mathf.Min(targetChange, maxCanAdd);
         else
         {
            if (owned > 0)
               ticker.ShowTicker($"Cannot select that many {item.tag}s. You only have {owned} {item.tag}{(owned == 1 ? "" : "s")}.", Color.red, MessageTypes.ResultMessage);
            else
               ticker.ShowTicker($"You have no {item.tag}s to sell. Craft {item.tag}s before selling.", Color.red, MessageTypes.ResultMessage);
         }
      } 
      else 
      {
         if (targetChange < 0) 
         {
            int maxCanSub = current - MIN_SELL_ITEM_COUNT;
            if (maxCanSub > 0)
               current -= Mathf.Min(Mathf.Abs(targetChange), maxCanSub);
            else 
            {
               if (owned > 0)
                  ticker.ShowTicker($"Nothing selected to remove. You own {owned} {item.tag}{(owned == 1 ? "" : "s")}. Use the + button to select an amount.", Color.red, MessageTypes.ResultMessage);
               else
                  ticker.ShowTicker($"You have no {item.tag}s to sell. Craft {item.tag}s before selling.", Color.red, MessageTypes.ResultMessage);
            }
         }
      }

      switch (item.tag)
      {
         case CRUDE_TOOL_TAG: 
            crudeToolSellCount = current; break;
         case HARPOON_TAG: 
            harpoonSellCount = current; break;
         case DIVING_BELL_TAG: 
            divingBellSellCount = current; break;
         case PRESSURE_VALVE_TAG: 
            pressureValveSellCount = current; break;
         case PRECISION_LENS_TAG: 
            precisionLensSellCount = current; break;
         case ENGINE_TAG: 
            engineSellCount = current; break;
         default: 
            Debug.LogError("Unknown item tag: " + item.tag); break;
      }
       
      item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text     = "   " + current.ToString();
      item.Find("currencyGained").GetComponent<TextMeshProUGUI>().text = (current * GetItemValue(itemType)).ToString();

      return;
   }

   public void IncreaseBuyItemsCount(Transform item) 
   {
      int amountToAdd = 1;

      // MODIFIER KEYS logic (Shift = +10, Ctrl = +50)
      if (Keyboard.current != null)
      {
          if (Keyboard.current.ctrlKey.isPressed) amountToAdd = 50;
          else if (Keyboard.current.shiftKey.isPressed) amountToAdd = 10;
      }

      switch (item.tag) 
      {
         case RAW_ORE_CHUNK_TAG:
            int oreToAdd = Mathf.Min(amountToAdd, MAX_BUY_ITEM_COUNT - rawOreExchange);
            if (oreToAdd > 0) 
            {
               rawOreExchange += oreToAdd;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text     = rawOreExchange.ToString();
               item.Find("currencySpent").GetComponent<TextMeshProUGUI>().text = (rawOreExchange * GetItemPrice(ItemType.RawOreChunk)).ToString();
            }
            else 
               ticker.ShowTicker($"Maximum limit of {MAX_BUY_ITEM_COUNT} reached.", Color.red, MessageTypes.ResultMessage);
            break;

         case MERCENARY_ENGINEER_TAG:
            int engToAdd = Mathf.Min(amountToAdd, MAX_MERCENARY_ENGINEER_COUNT - mercenaryEngineerBuyCount);
            if (engToAdd > 0) 
            {
               mercenaryEngineerBuyCount += engToAdd;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text     = mercenaryEngineerBuyCount.ToString();
               item.Find("currencySpent").GetComponent<TextMeshProUGUI>().text = (mercenaryEngineerBuyCount * GetItemPrice(ItemType.MercenaryEngineer)).ToString();
            }
            else 
               ticker.ShowTicker($"Maximum limit of {MAX_MERCENARY_ENGINEER_COUNT} reached.", Color.red, MessageTypes.ResultMessage);
            break;

         default:
            Debug.LogError("Unknown item tag: " + item.tag);
            break;
      }
   }

   public void DecreaseBuyItemsCount(Transform item) 
   {
      int amountToSub = 1;

      // MODIFIER KEYS logic (Shift = -10, Ctrl = -50)
      if (Keyboard.current != null)
      {
          if (Keyboard.current.ctrlKey.isPressed) amountToSub = 50;
          else if (Keyboard.current.shiftKey.isPressed) amountToSub = 10;
      }

      switch (item.tag) 
      {
         case RAW_ORE_CHUNK_TAG:
            int oreToSub = Mathf.Min(amountToSub, rawOreExchange - MIN_BUY_ITEM_COUNT);
            if (oreToSub > 0) 
            {
               rawOreExchange -= oreToSub;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text     = rawOreExchange.ToString();
               item.Find("currencySpent").GetComponent<TextMeshProUGUI>().text = (rawOreExchange * GetItemPrice(ItemType.RawOreChunk)).ToString();
            }
            else 
               ticker.ShowTicker($"Minimum limit of {MIN_BUY_ITEM_COUNT} reached.", Color.red, MessageTypes.ResultMessage);
            break;

         case MERCENARY_ENGINEER_TAG:
            int engToSub = Mathf.Min(amountToSub, mercenaryEngineerBuyCount - MIN_BUY_ITEM_COUNT);
            if (engToSub > 0) 
            {
               mercenaryEngineerBuyCount -= engToSub;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text     = mercenaryEngineerBuyCount.ToString();
               item.Find("currencySpent").GetComponent<TextMeshProUGUI>().text = (mercenaryEngineerBuyCount * GetItemPrice(ItemType.MercenaryEngineer)).ToString();
            }
            else 
               ticker.ShowTicker($"Minimum limit of {MIN_BUY_ITEM_COUNT} reached.", Color.red, MessageTypes.ResultMessage);
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

         if (inv.TrySpendOre(ORE_EXCHANGE_COST))
         {
            inv.TryAddPearl(pearlsReceived);
            recycleCounter += 1;
         }

         if (recycleCounter == 3)
            RecycleButton.gameObject.SetActive(false);

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

      crudeToolEvent =
         (worldEvent == (int)WorldEventTypes.IndustrialGoldRushEvent) 
         ? WorldEventTypes.IndustrialGoldRushEvent
         : (worldEvent == (int)WorldEventTypes.ScavengersHolidayEvent)
         ? WorldEventTypes.ScavengersHolidayEvent
         : WorldEventTypes.CrudeToolEvent;

      harpoonEvent =
         (worldEvent == (int) WorldEventTypes.DeepSeaWarEvent) 
         ? WorldEventTypes.DeepSeaWarEvent
         : (worldEvent == (int)WorldEventTypes.IndustrialGoldRushEvent) 
         ? WorldEventTypes.IndustrialGoldRushEvent
         : (worldEvent == (int)WorldEventTypes.ScavengersHolidayEvent)
         ? WorldEventTypes.ScavengersHolidayEvent
         : WorldEventTypes.HarpoonEvent;

      divingBellEvent =
         (worldEvent == (int)WorldEventTypes.IndustrialGoldRushEvent)
         ? WorldEventTypes.IndustrialGoldRushEvent
         : (worldEvent == (int)WorldEventTypes.ScavengersHolidayEvent) 
         ? WorldEventTypes.ScavengersHolidayEvent
         : WorldEventTypes.DivingBellEvent;

      pressureValveEvent =
         (worldEvent == (int)WorldEventTypes.ScavengersHolidayEvent) 
         ? WorldEventTypes.ScavengersHolidayEvent
         : WorldEventTypes.PressureValveEvent;

      precisionLensEvent =
         (worldEvent == (int)WorldEventTypes.ScavengersHolidayEvent) 
         ? WorldEventTypes.ScavengersHolidayEvent
         : WorldEventTypes.PrecisionLensEvent;

      engineEvent =
         (worldEvent == (int)WorldEventTypes.ScavengersHolidayEvent) 
         ? WorldEventTypes.ScavengersHolidayEvent
         : WorldEventTypes.ClockworkEngineEvent;

   
      // 2. Update UI Previews based on these exact rolls
      UpdateMarketPreviewUI(
         ItemType.CrudeTool, crudeToolEvent, CRUDE_TOOL_TAG,
         MIN_CRUDE_TOOL_VALUE, MAX_CRUDE_TOOL_VALUE, base_crude_tool_value,
         crudeToolChance, crudeToolFluctuation, TIER_ONE);
       
      UpdateMarketPreviewUI(
         ItemType.Harpoon, harpoonEvent, HARPOON_TAG,
         MIN_HARPOON_VALUE, MAX_HARPOON_VALUE, base_harpoon_value,
         harpoonChance, harpoonFluctuation, TIER_ONE);

      UpdateMarketPreviewUI(
         ItemType.DivingBell, divingBellEvent, DIVING_BELL_TAG,
         MIN_DIVING_BELL_VALUE, MAX_DIVING_BELL_VALUE, base_diving_bell_value,
         divingBellChance, divingBellFluctuation, TIER_ONE);

       if (ForgeManager.Instance.hasTier2Blueprint) 
       {
           UpdateMarketPreviewUI(
              ItemType.PressureValve, pressureValveEvent, PRESSURE_VALVE_TAG,
              MIN_PRESSURE_VALVE_VALUE, MAX_PRESSURE_VALVE_VALUE, base_pressure_valve_value,
              pressureValveChance, pressureValveFluctuation, TIER_TWO);
       }
   
       if (ForgeManager.Instance.hasTier3Blueprint) 
       {
           UpdateMarketPreviewUI(
              ItemType.PrecisionLens, precisionLensEvent, PRECISION_LENS_TAG,
              MIN_PRECISION_LENS_VALUE, MAX_PRECISION_LENS_VALUE, base_precision_lens_value,
              precisionLensChance, precisionLensFluctuation, TIER_THREE);

           UpdateMarketPreviewUI(
              ItemType.Engine, engineEvent, ENGINE_TAG,
              MIN_ENGINE_VALUE, MAX_ENGINE_VALUE, base_engine_value,
              engineChance, engineFluctuation, TIER_THREE);
       }
   }

   // Gives a up to a 1.2% increase and decrease to item sell values
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
       int currentVal = GetItemValue(itemType),
           preview    = currentVal;

       int marketTrendChance = 
         itemTier == TIER_ONE 
         ? TIER_ONE_CHANCE
         : itemTier == TIER_TWO 
         ? TIER_TWO_CHANCE 
         : itemTier == TIER_THREE 
         ? TIER_THREE_CHANCE 
         : TIER_ONE_CHANCE;

       // 1. SPIKE PREVIEW: If we are on Turn 4, predict the massive Event Spike happening next on Turn 5!
       if (worldEvent == (int)eventType && TurnManager.Instance.eventCountdown == 4)
       {
          switch(worldEvent) 
          { 
             case (int)WorldEventTypes.IndustrialGoldRushEvent:
                preview = currentVal * 2;
                break;
             case (int)WorldEventTypes.DeepSeaWarEvent:
                preview += (currentVal * 3) - currentVal;
                break;
             case (int) WorldEventTypes.ScavengersHolidayEvent:
                preview -=  Mathf.RoundToInt(currentVal * .25f);
                break;
             default:
                if (shiftDirection <= marketTrendChance)
                   preview += currentVal;
                else
                   preview -= currentVal;
                break;
          }
       }
       // 2. RESET PREVIEW
       else if (worldEvent == (int)eventType && TurnManager.Instance.eventCountdown == 5) 
       {
           preview = baseValue;
       }
       // 3. STANDARD PREVIEW: Normal daily fluctuations
       else
       { 
          if (chance <= 30) 
             preview += fluctuation;
          else if (chance <= 60) 
             preview -= fluctuation;
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
        crudeToolChance, crudeToolFluctuation, TIER_ONE,
        TryIncreaseCrudeToolSellValue, TryDecreaseCrudeToolSellValue);
             
     ApplyStoredShift(
        ItemType.Harpoon, harpoonEvent, 
        harpoonChance, harpoonFluctuation, TIER_ONE,
        TryIncreaseHarpoonSellValue, TryDecreaseHarpoonSellValue);

      ApplyStoredShift(
         ItemType.DivingBell, divingBellEvent,
         divingBellChance, divingBellFluctuation, TIER_ONE,
         TryIncreaseDivingBellValue, TryDecreaseDivingBellValue);

      // Apply Tier 2
      if (ForgeManager.Instance.hasTier2Blueprint) 
      {
         ApplyStoredShift(
            ItemType.PressureValve, pressureValveEvent, 
            pressureValveChance, pressureValveFluctuation, TIER_TWO,
            TryIncreasePressureValveValue, TryDecreasePressureValveValue);
      }
   
       // Apply Tier 3
       if (ForgeManager.Instance.hasTier3Blueprint) 
       {
           ApplyStoredShift(
              ItemType.PrecisionLens, precisionLensEvent, 
              precisionLensChance, precisionLensFluctuation, TIER_THREE,
              TryIncreasePrecisionLensValue, TryDecreasePrecisionLensValue);

           ApplyStoredShift(
              ItemType.Engine, engineEvent, 
              engineChance, engineFluctuation, TIER_THREE,
              TryIncreaseEngineSellValue, TryDecreaseEngineSellValue);
       }
   }
   
   // Applies the market shift
   private void ApplyStoredShift(
      ItemType itemType, WorldEventTypes eventType,
      int change, int fluctuation, int itemTier,
      Action<int> increaseSellValueMethod, Action<int> decreaseSellValueMethod)
   {
      int marketTrendChance = itemTier == TIER_ONE ? TIER_ONE_CHANCE : 
                              itemTier == TIER_TWO ? TIER_TWO_CHANCE : 
                              itemTier == TIER_THREE ? TIER_THREE_CHANCE : TIER_ONE_CHANCE;

      // APPLY SPIKE: Executes exactly on Turn 5
      if (worldEvent == (int)eventType && TurnManager.Instance.eventCountdown == 5)
       {
           switch (worldEvent) 
           {
              case (int) WorldEventTypes.IndustrialGoldRushEvent:
                 increaseSellValueMethod((GetItemValue(itemType) * 2) - GetItemValue(itemType));
                 break;
              case (int) WorldEventTypes.DeepSeaWarEvent:
                 // WorldEventSideEffect(eventType); <-- Ensure this exists or remove it!
                 increaseSellValueMethod((GetItemValue(itemType) * 3) - GetItemValue(itemType));
                 break;
              case (int) WorldEventTypes.ScavengersHolidayEvent:
                 decreaseSellValueMethod((int)(GetItemValue(itemType) * .25f));
                 break;
              default:
                 if (shiftDirection <= marketTrendChance) 
                    increaseSellValueMethod(GetItemValue(itemType));
                 else 
                    decreaseSellValueMethod(GetItemValue(itemType));
                 break;
           }
       }
       else 
       { 
          // SKIP FLUCTUATION: If we just reset the values on Turn 1, don't fluctuate them immediately
          if (lastResetTurn.ContainsKey(itemType) && lastResetTurn[itemType] && TurnManager.Instance.eventCountdown == 1)
              lastResetTurn[itemType] = false; 
          // STANDARD FLUCTUATION: Apply to Turns 2, 3, and 4
          else
          {
              if (change <= 30) 
                 increaseSellValueMethod(fluctuation);
              else if (change <= 60) 
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
         if (ForgeManager.Instance.hasTier2Blueprint) 
            finalWorldEvent = (int)(WorldEventTypes.DeepSeaWarEvent);
         else
            finalWorldEvent = (int)(WorldEventTypes.IndustrialGoldRushEvent);

      worldEvent = Rng.Next(worldEvent1, finalWorldEvent + 1);
      WorldEventItemVisual(worldEvent);
     

      shiftDirection = isTier3BuffACtive ? 1 : Rng.Next(MARKET_CHANCE_MIN, MARKET_CHANCE_MAX);
   }

   public void WorldEventItemVisual(int worldEvent) 
   {
      Image itemSprite = worldEventIcon.Find("WorldEventItem").GetComponent<Image>();

      itemSprite.gameObject.SetActive(true);

      switch (worldEvent) 
      {
         // Tier 1
         case (int) WorldEventTypes.CrudeToolEvent:
            itemSprite.sprite = GetItemSprite(ItemType.CrudeTool);
            break;
         case (int) WorldEventTypes.HarpoonEvent:
            itemSprite.sprite = GetItemSprite(ItemType.Harpoon);
            break;
         case (int) WorldEventTypes.DivingBellEvent:
            itemSprite.sprite = GetItemSprite(ItemType.DivingBell);
            break;
         case (int) WorldEventTypes.IndustrialGoldRushEvent:
            itemSprite.sprite = WorldEventSprites.worldEventSprites.GetWorldEventSprite(WorldEventTypes.IndustrialGoldRushEvent);
            break;

         // Tier 2
         case (int) WorldEventTypes.PressureValveEvent:
            itemSprite.sprite = GetItemSprite(ItemType.PressureValve);
            break;
         case (int) WorldEventTypes.DeepSeaWarEvent:
            itemSprite.sprite = GetItemSprite(ItemType.Harpoon);
            break;

         // Tier 3
         case (int) WorldEventTypes.PrecisionLensEvent:
            itemSprite.sprite = GetItemSprite(ItemType.PrecisionLens);
            break;
         case (int) WorldEventTypes.ClockworkEngineEvent:
            itemSprite.sprite = GetItemSprite(ItemType.Engine);
            break;
         case (int) WorldEventTypes.ScavengersHolidayEvent:
            itemSprite.gameObject.SetActive(false);
            break;
         default:
            Debug.LogError("Unkown Item: " +  worldEvent);
            break;
      }
   }

   public void DisplayWorldEventVisual(
      bool isWorldEventVisualAcive, 
      bool isWorldEventChangeVisualActive) 
   {
      int marketTrendChance =
         worldEvent <= 3
        ? TIER_ONE_CHANCE
        : worldEvent <= 5
        ? TIER_TWO_CHANCE
        : worldEvent <= 8
        ? TIER_THREE_CHANCE
        : TIER_ONE_CHANCE;

      if (worldEvent == (int) WorldEventTypes.DeepSeaWarEvent || worldEvent == (int) WorldEventTypes.IndustrialGoldRushEvent)
            worldEventChange.sprite = worldEventSymbols.Find("increaseSymbol").GetComponent<Image>().sprite;
      else 
      { 
         if(worldEvent == (int)WorldEventTypes.ScavengersHolidayEvent)
            worldEventChange.sprite = worldEventSymbols.Find("decreaseSymbol").GetComponent<Image>().sprite;
         else 
         { 
            worldEventChange.sprite = 
               shiftDirection < marketTrendChance
               ? worldEventSymbols.Find("increaseSymbol").GetComponent<Image>().sprite
               : worldEventSymbols.Find("decreaseSymbol").GetComponent<Image>().sprite;
         }
      }

      if (worldEvent == (int) WorldEventTypes.ScavengersHolidayEvent)
         isWorldEventVisualAcive = false;

      worldEventIcon.gameObject.SetActive(isWorldEventVisualAcive);
      worldEventChange.gameObject.SetActive(isWorldEventChangeVisualActive);
   }

   public void WorldEventNewsTickerText() 
   {
      switch (worldEvent) 
      { 
         case (int)WorldEventTypes.CrudeToolEvent:
            currentNewsTickerMessage = GetCrudeToolTickerMessage(shiftDirection);
            break;
         case (int)WorldEventTypes.HarpoonEvent:
            currentNewsTickerMessage = GetHarpoonTickerMessage(shiftDirection);
            break;
         case (int)WorldEventTypes.IndustrialGoldRushEvent:
            currentNewsTickerMessage = GetIndustrialGoldRushTickerMessage();
            break;
         case (int)WorldEventTypes.PressureValveEvent:
            currentNewsTickerMessage = GetPressureValveMessage(shiftDirection);
            break;
         case (int)WorldEventTypes.DivingBellEvent:
            currentNewsTickerMessage = GetDivingBellTickerMessage(shiftDirection);
            break;
         case (int)WorldEventTypes.DeepSeaWarEvent:
            currentNewsTickerMessage = GetDeepSeaWarTickerMessage();
            break;
         case (int)WorldEventTypes.PrecisionLensEvent:
            currentNewsTickerMessage = GetPrecisionLensTickerMessage(shiftDirection);
            break;
         case (int)WorldEventTypes.ClockworkEngineEvent:
            currentNewsTickerMessage = GetClockWorkEngineMessage(shiftDirection);
            break;
         case (int)WorldEventTypes.ScavengersHolidayEvent:
            currentNewsTickerMessage = GetScavengersHolidayTickerMessage();
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
             RevertItemToBaseSellValue(ItemType.CrudeTool);
   
             // Flag the reset for crude tool
             lastResetTurn[ItemType.CrudeTool] = true;
             break;
   
          // Undo the crude tool event shift based on previous direction
          case (int)WorldEventTypes.HarpoonEvent:
            RevertItemToBaseSellValue(ItemType.Harpoon);

            // Flag the reset for harpoon
            lastResetTurn[ItemType.Harpoon] = true;
             break;
   
          // Undo the diving bell event shift based on previous shift
          case (int)WorldEventTypes.DivingBellEvent:
            RevertItemToBaseSellValue(ItemType.DivingBell);

            lastResetTurn[ItemType.DivingBell] = true;
             break;
   
          case (int)WorldEventTypes.IndustrialGoldRushEvent:
             RevertItemToBaseSellValue(ItemType.CrudeTool);
             RevertItemToBaseSellValue(ItemType.Harpoon);
             RevertItemToBaseSellValue(ItemType.DivingBell);

   
             // Flag the reset for harpoon
             lastResetTurn[ItemType.CrudeTool]  = true;
             lastResetTurn[ItemType.Harpoon]    = true;
             lastResetTurn[ItemType.DivingBell] = true;
             break;
   
          // Undo the pressure valve event shift based on previous direction
          case (int)WorldEventTypes.PressureValveEvent:
            RevertItemToBaseSellValue(ItemType.PressureValve);

            // Flag the reset for Pressure Valve
            lastResetTurn[ItemType.PressureValve] = true;
            break;
   
   
          case (int)WorldEventTypes.DeepSeaWarEvent:
             RevertItemToBaseSellValue(ItemType.Harpoon);


             // Flag the reset for harpoon
             lastResetTurn[ItemType.Harpoon] = true;
             break;
   
   
          // Undo the precision lens event shift based on previous shift
          case (int)WorldEventTypes.PrecisionLensEvent:
             RevertItemToBaseSellValue(ItemType.PrecisionLens);
            
            lastResetTurn[ItemType.PrecisionLens] = true;
             break;
   
          // Undo the engine event shift based on previous direction
          case (int)WorldEventTypes.ClockworkEngineEvent:
            RevertItemToBaseSellValue(ItemType.Engine);

            // Flag the reset for Engine
            lastResetTurn[ItemType.Engine] = true;
            break;
   
          // Undo Scavenger Holiday event shift
          case (int)WorldEventTypes.ScavengersHolidayEvent:
             RevertMarketToBase();
             ExplorationUnitManager.Instance.explorationCost = 1;
   
             // Flag the reset for Scavenger Holiday Event
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

   public void InsurancePolicyCheck() 
   {
      int currentWorldEventMarketTrendChance = GetCurrentEventCrashThreshold();
      
      if (isInsurancePolicyActive && (shiftDirection > currentWorldEventMarketTrendChance))
         InventoryManager.Instance.TryAddPearl(INSURANCE_POLICY_PAYOUT);
      
      isInsurancePolicyActive = false;
      
      return;
   }

   private int GetCurrentEventCrashThreshold()
   {
      switch (worldEvent)
      {
         // Tier 3 Events 
         case (int)WorldEventTypes.ClockworkEngineEvent:
         case (int)WorldEventTypes.PrecisionLensEvent:
         case (int)WorldEventTypes.ScavengersHolidayEvent:
            return TIER_THREE_CHANCE;

         // Tier 2 Events 
         case (int)WorldEventTypes.PressureValveEvent:
         case (int)WorldEventTypes.DeepSeaWarEvent: 
            return TIER_TWO_CHANCE;

         // Tier 1 Events & Base Defaults 
         case (int)WorldEventTypes.CrudeToolEvent:
         case (int)WorldEventTypes.HarpoonEvent:
         case (int)WorldEventTypes.DivingBellEvent:
         case (int)WorldEventTypes.IndustrialGoldRushEvent:
         default:
            return TIER_ONE_CHANCE;
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
   public static void ClearEvents() { HandleTutorial = null; }

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
         BuyWindow.gameObject.SetActive(false);

         SellPanel.gameObject.SetActive(true);
      } 
      else
         SellPanel.gameObject.SetActive(true);

      // Destroy the instantiated buy item/window instance if it exists
      if (currentBuyItem != null) 
      {
         Destroy(currentBuyItem.gameObject);
         currentBuyItem = null;
      }

      if (tutorialFunctionTwo)
      {      
         HandleTutorial?.Invoke(1);
         TradePanels.transform.Find("Screen1").gameObject.SetActive(true);
         TradePanels.transform.Find("Screen2").gameObject.SetActive(true);
         TradePanels.transform.Find("Circle1").gameObject.SetActive(true);
      }
   }

   public void ShowBuyPanel() 
   {
      if(SellPanel.gameObject.activeSelf)
      {
         SellPanel.gameObject.SetActive(false);
         SellWindow.gameObject.SetActive(false);

         BuyPanel.gameObject.SetActive(true);
      }

      // Destroy the instantiated sell item/window instance if it exists
      if (currentSellItem != null) 
      {
         Destroy(currentSellItem.gameObject);
         currentSellItem = null;
      }
   }

   public void ShowBuyWindow() 
   {
      panelManager.OpenPanel(BuyWindow.gameObject);
   }

   public void ShowSellWindow() 
   {
      panelManager.OpenPanel(SellWindow.gameObject);
   }

   private void CloseTradePanel() 
   {
      if(tutorialFunctionTwo)
      {
         tutorialFunctionTwo = false;
         HandleTutorial?.Invoke(2);
         TradePanels.transform.Find("Screen7").gameObject.SetActive(false);
         TradePanels.transform.Find("Circle4").gameObject.SetActive(false);
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
}