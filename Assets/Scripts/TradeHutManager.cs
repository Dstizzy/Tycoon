using System;
using System.Collections.Generic;

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
   // Inspector variables
   [SerializeField] private Transform TradePanels;            
   [SerializeField] private Transform BuyWindow;    
   [SerializeField] private Transform SellPanel;              
   [SerializeField] private Transform SellWindow;     
   [SerializeField] private Transform InfoPanel;                   
   [SerializeField] private Transform UpgradePanel;           
   [SerializeField] private Transform MysteryBoxPanel;
   [SerializeField] private TextMeshProUGUI tradeHutLevelText;
                    public  Transform BuyPanel;       
                    public  Transform RecycleButton;
   public List<Transform> SellItems { get; private set; }
   public List<Transform> BuyItems  { get; private set; }

   private TickerSystem ticker;

   public string currrentNewsTickerMessage;

   // Random number generator
   private readonly static System.Random Rng = new System.Random(); 

   // Keeps track of the last item that had a world event
   private readonly Dictionary<ItemType, bool> lastResetTurn = new Dictionary<ItemType, bool>();

   // Transforms
   private Transform currentBuyItem,    
                     currentSellItem,
                     currentMysteryBoxResult;

   // Private variables
   private int crudeToolSellCount = 0,
               harpoonSellCount   = 0, 
               pressureValveCount = 0, 
               engineSellCount    = 0, 

               rawOreExchange     = 0,
       
               crudeToolFluctuation,
               harpoonFluctuation,
               pressureValveFluctuation,
               engineFluctuation,

               crudeToolChance,
               harpoonChance,
               pressureValveChance,
               engineChance,

               shiftDirection,
               worldEvent;

   // Public variables
   public int marketShiftMax = 0,
              marketShiftMin = 0;

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
                    ORE_EXCHANGE_COST    = 10;
                    
      
   public const string RAW_ORE_CHUNK_TAG      = "Raw Ore Chunk",
                       TIER_2_BLUEPRINT       = "Tier 2 Blueprint",
                       TIER_3_BLUEPRINT       = "Tier 3 Blueprint",
                       MERCENARY_ENGINEER_TAG = "Mercenary Engineer";
   
   public bool isTier3BuffACtive  = false;

   private InventoryManager inv;

   public static TradeHutManager Instance;

   private void Awake() 
   {
      SellItems = new();
      BuyItems  = new();
      ticker    = TickerSystem.Instance;

      // Initialize lastResetTurn for every ItemType so lookups are safe
      foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
         lastResetTurn[itemType] = false;

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

      if (UpgradePanel == null)
         Debug.LogError("Upgrade Panel is not assigned in the Inspector!");
      else
         CloseUpgradePanel();

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
      inv = InventoryManager.Instance;

      CreateSellItem(GetItemSprite(ItemType.CrudeTool),GetItemValue(ItemType.CrudeTool), -1.0f, CRUDE_TOOL_TAG);
      CreateSellItem(GetItemSprite(ItemType.Harpoon), GetItemValue(ItemType.Harpoon), 0.0f, HARPOON_TAG);
      CreateSellItem(GetItemSprite(ItemType.PressureValve), GetItemValue(ItemType.PressureValve), 1.0f, PRESSURE_VALVE_TAG);
      CreateSellItem(GetItemSprite(ItemType.Engine), GetItemValue(ItemType.Engine), 2.0f, ENGINE_TAG);

      CreateBuyItem(GetItemSprite(ItemType.RawOreChunk), GetItemPrice(ItemType.RawOreChunk), 0.0f, RAW_ORE_CHUNK_TAG);
      CreateBuyItem(GetItemSprite(ItemType.Tier2BluePrint), GetItemPrice(ItemType.Tier2BluePrint), 1.0f, TIER_2_BLUEPRINT);
      CreateBuyItem(GetItemSprite(ItemType.Tier3BluePrint), GetItemPrice(ItemType.Tier3BluePrint), 2.0f, TIER_3_BLUEPRINT);
      CreateBuyItem(GetItemSprite(ItemType.MercenaryEngineer), GetItemPrice(ItemType.MercenaryEngineer), 0.0f, MERCENARY_ENGINEER_TAG, -35);
   }

   public void CreateSellItem(Sprite itemSprite, int itemValue, float positionIndex, string itemTag) 
   {
      Transform       sellItemContainer = SellPanel.Find("sellItemContainer").GetComponent<Transform>(),
                      sellItemTemplate  = sellItemContainer.Find("SellItemTemplate").GetComponent<Transform>(),
                      tradeItemTransform;

      TextMeshProUGUI sellValueText;
      RectTransform   tradeItemRectTransform;
      Button          itemButton;

      sellItemTemplate.gameObject.SetActive(false);

      // Instantiate the template and set its position in the container                               
      tradeItemTransform     = Instantiate(sellItemTemplate, sellItemContainer);
      tradeItemRectTransform = tradeItemTransform.GetComponent<RectTransform>();

      tradeItemTransform.tag = itemTag;
      tradeItemRectTransform.anchoredPosition = new Vector2(BUY_ITEM_SPACING * positionIndex, 0);

      // Populate the the item properties                                                           
      sellValueText      = tradeItemTransform.Find("ItemValue").GetComponent<TextMeshProUGUI>();
      sellValueText.text = itemValue.ToString();
      
      tradeItemTransform.Find("ItemName").GetComponent<TextMeshProUGUI>().text  = itemTag.Equals(ENGINE_TAG) ? "   " + ENGINE_TAG : itemTag;
      tradeItemTransform.Find("ItemShadow").GetComponent<Image>().sprite        = itemSprite;
      tradeItemTransform.Find("ItemShadow").gameObject.SetActive(false);

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
            case ENGINE_TAG:
               tradeItemTransform.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = " x" + inv.engineCount.ToString();
               break;
            default:
               Debug.LogError("Unkown item: " +  itemTag);
               break;
         }

      if (LabManager.currentCommerceTier < LabManager.TIER_ONE)
         tradeItemTransform.Find("NextValue").GetComponent<TextMeshProUGUI>().gameObject.SetActive(false);

     if(itemTag != CRUDE_TOOL_TAG && itemTag != HARPOON_TAG) 
     {
         tradeItemTransform.Find("ItemButton").gameObject.SetActive(false);
         tradeItemTransform.Find("ItemName").gameObject.SetActive(false);
         tradeItemTransform.Find("ItemCount").gameObject.SetActive(false);
         tradeItemTransform.Find("ItemValue").gameObject.SetActive(false);
         tradeItemTransform.Find("Pearl_Icon").gameObject.SetActive(false);
         tradeItemTransform.Find("ItemShadow").gameObject.SetActive(true);
     }

      SellItems.Add(tradeItemTransform);

      // Dynamically add a listener to the button, which creates a sell window when clicked         
      itemButton.onClick.AddListener(() => CreateSellWindow(itemSprite, GetResourceSprite(ResourceType.Pearl), itemValue, itemTag));

      tradeItemTransform.gameObject.SetActive(true);
   }

   public void CreateBuyItem(Sprite itemSprite, int itemValue, float positionIndex, string itemTag, int verticalIndex = 0) 
   {
      Transform     buyItemContainer = BuyPanel.Find("BuyItemContainer").GetComponent<Transform>(),
                    buyItemTemplate  = buyItemContainer.Find("BuyItemTemplate").GetComponent<Transform>(),
                    tradeItemTransform;
      
      RectTransform tradeItemRectTransform;
      Button        itemButton;

      // Instantiate the template and set its position in the container
      tradeItemTransform     = Instantiate(buyItemTemplate, buyItemContainer);
      buyItemTemplate.gameObject.SetActive(false);
      tradeItemRectTransform = tradeItemTransform.GetComponent<RectTransform>();
      tradeItemRectTransform.anchoredPosition = new Vector2(BUY_ITEM_SPACING * positionIndex, verticalIndex);     

      // Populate the item properties
      tradeItemTransform.tag = itemTag;
      tradeItemTransform.Find("ItemName").GetComponent<TextMeshProUGUI>().text  = itemTag.ToString();
      tradeItemTransform.Find("ItemValue").GetComponent<TextMeshProUGUI>().text = itemValue.ToString();
      itemButton = tradeItemTransform.Find("ItemButton").GetComponent<Button>();

      itemButton.image.sprite = itemSprite;

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
      int       itemCount           = 0;

      sellWindowTemplate.gameObject.SetActive(false);

      // Destroy the previously opened sell window instance before creating a new one
      if (currentSellItem != null) 
      {
         Destroy(currentSellItem.gameObject);
         currentSellItem = null;
      }

      Transform     sellItemTransform     = Instantiate(sellWindowTemplate, sellWindowContainer);
      RectTransform sellItemRectTransform = sellItemTransform.GetComponent<RectTransform>();

      sellItemTransform.tag = itemTag;

      sellItemRectTransform.anchoredPosition = new Vector2(BUY_ITEM_SPACING * 0, 0);

      // Populate item properties
      sellItemTransform.Find("ItemImage").GetComponent<Image>().sprite              = itemSprite;
      sellItemTransform.Find("ItemCount").GetComponent<TextMeshProUGUI>().text      = "   " + itemCount.ToString();
      sellItemTransform.Find("currencyIcon").GetComponent<Image>().sprite           = currencySprite;
      sellItemTransform.Find("currencyGained").GetComponent<TextMeshProUGUI>().text = "0";

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
      buyItemTransfrom.Find("ItemImage").GetComponent<Image>().sprite             = itemSprite;
      buyItemTransfrom.Find("ItemCount").GetComponent<TextMeshProUGUI>().text     = (itemCount + 1).ToString();
      buyItemTransfrom.Find("currencyIcon").GetComponent<Image>().sprite          = currencySprite;

      switch (itemTag) 
      {
         case TIER_2_BLUEPRINT:
            buyItemTransfrom.Find("currencySpent").GetComponent<TextMeshProUGUI>().text = GetItemPrice(ItemType.Tier2BluePrint).ToString();
            break;
         case TIER_3_BLUEPRINT:
            buyItemTransfrom.Find("currencySpent").GetComponent<TextMeshProUGUI>().text = GetItemPrice(ItemType.Tier3BluePrint).ToString();
            break;
         case MERCENARY_ENGINEER_TAG:
            buyItemTransfrom.Find("currencySpent").GetComponent<TextMeshProUGUI>().text = GetItemPrice(ItemType.MercenaryEngineer).ToString();
            break;
         case RAW_ORE_CHUNK_TAG:
            buyItemTransfrom.Find("currencySpent").GetComponent<TextMeshProUGUI>().text = "0";
            break;
         default:
            Debug.LogError("Unkown item: " + itemTag);
            break;
      }

      buyItemTransfrom.Find("QuantityButtons/IncreaseButton").gameObject.SetActive(false);
      buyItemTransfrom.Find("QuantityButtons/DecreaseButton").gameObject.SetActive(false);

      if (itemTag  == RAW_ORE_CHUNK_TAG) 
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

      switch (itemTag) 
      {
         // Tier 1 items
         case CRUDE_TOOL_TAG:
            if (crudeToolSellCount > MIN_SELL_ITEM_COUNT)
            {
               soldCount = crudeToolSellCount;
               
               if (inv.TryUseCrudeTool(crudeToolSellCount)) 
               {
                  totalSellValue += soldCount * GetItemValue(ItemType.CrudeTool);
                  successMessage = $"Sold {soldCount} Crude Tool{(soldCount > 1 ? "s" : "")} for {totalSellValue} pearls.";
               }
               else 
                  crudeToolSellCount = MIN_SELL_ITEM_COUNT;
            }
            break;
      
         case HARPOON_TAG:
            if (harpoonSellCount > MIN_SELL_ITEM_COUNT) 
            {
               soldCount = harpoonSellCount;
               
               if (inv.TryUseHarpoon(harpoonSellCount))
               {
                  totalSellValue += soldCount * GetItemValue(ItemType.Harpoon);
                  successMessage = $"Sold {soldCount} Harpoon{(soldCount > 1 ? "s" : "")} for {totalSellValue} pearls.";
               }
               else
                  harpoonSellCount = MIN_SELL_ITEM_COUNT;
            }
            break;
      
         // Tier 2 item
         case PRESSURE_VALVE_TAG:
            if (pressureValveCount > MIN_SELL_ITEM_COUNT) 
            {
               soldCount = pressureValveCount;
               
               if (inv.TryUsePressureValve(pressureValveCount))
               {
                  totalSellValue += soldCount * GetItemValue(ItemType.PressureValve);
                  successMessage = $"Sold {soldCount} Pressure Valve{(soldCount > 1 ? "s" : "")} for {totalSellValue} pearls.";
               }
               else
                  pressureValveCount = MIN_SELL_ITEM_COUNT;
            }
            break;
      
         // Tier 3 item
         case ENGINE_TAG:
            if (engineSellCount > MIN_SELL_ITEM_COUNT) 
            {
               soldCount = engineSellCount;
               
               if (inv.TryUseEngine(engineSellCount))
               {
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
         ticker.ShowTicker(successMessage ?? $"Sold items for {totalSellValue} pearls.", Color.green, MessageTypes.ResultMessage);
      } 
      else 
      { 
         if(soldCount == MIN_SELL_ITEM_COUNT) 
            ticker.ShowTicker("No items have been selected.", Color.red, MessageTypes.ResultMessage);
         else
            soldCount = 0;
      }

      crudeToolSellCount = MIN_SELL_ITEM_COUNT;
      harpoonSellCount   = MIN_SELL_ITEM_COUNT;
      pressureValveCount = MIN_SELL_ITEM_COUNT;
      engineSellCount    = MIN_SELL_ITEM_COUNT;
      
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
         if (currentBuyItem.CompareTag(TIER_2_BLUEPRINT) && inv.TrySpendPearl(GetItemPrice(ItemType.Tier2BluePrint))) 
         {
            // Grants player access to tier 2 blueprint content
            ForgeManager.Instance.hasTier2Blueprint = true;

            // Adds new craftable items to the inventory/craft list (pressure valve, diving bell)
            InventoryManager.Instance.CreateCraft(GetItemSprite(ItemType.PressureValve), PRESSURE_VALVE_POSITION, PRESSURE_VALVE_TAG);
            InventoryManager.Instance.CreateCraft(GetItemSprite(ItemType.DivingBell), DIVING_BELL_POSITION, DIVING_BELL_TAG, -250);

            // Removes the tier 2 blueprint from the buy panel
            BuyItems.Find(item => item.CompareTag(TIER_2_BLUEPRINT)).gameObject.SetActive(false);

            // Reveal the pressure valve on the sell panel and enable its UI controls
            SellItems.Find(item => item.CompareTag(PRESSURE_VALVE_TAG)).Find("ItemButton").gameObject.SetActive(true);
            SellItems.Find(item => item.CompareTag(PRESSURE_VALVE_TAG)).Find("ItemName").gameObject.SetActive(true);
            SellItems.Find(item => item.CompareTag(PRESSURE_VALVE_TAG)).Find("ItemCount").gameObject.SetActive(true);
            SellItems.Find(item => item.CompareTag(PRESSURE_VALVE_TAG)).Find("ItemValue").gameObject.SetActive(true);
            SellItems.Find(item => item.CompareTag(PRESSURE_VALVE_TAG)).Find("Pearl_Icon").gameObject.SetActive(true);
            SellItems.Find(item => item.CompareTag(PRESSURE_VALVE_TAG)).Find("ItemShadow").gameObject.SetActive(false);
      
            ticker.ShowTicker("Purchased Tier 2 Blueprint — Pressure Valve and Diving Bell unlocked.", Color.green, MessageTypes.ResultMessage);
         }

         // Tier 3 Blueprint purchase flow
         if (currentBuyItem.CompareTag(TIER_3_BLUEPRINT) && inv.TrySpendPearl(GetItemPrice(ItemType.Tier3BluePrint)))
         {
            // Grants player access to tier 3 blueprint content
            ForgeManager.Instance.hasTier3Blueprint = true;

            // Removes the tier 3 blueprint from the buy panel
            BuyItems.Find(item => item.CompareTag(TIER_3_BLUEPRINT)).gameObject.SetActive(false);

            // Adds new craftable items (engine, precision lens) to inventory/craft list
            inv.CreateCraft(GetItemSprite(ItemType.Engine), ENGINE_POSITION, ENGINE_TAG);
            inv.CreateCraft(GetItemSprite(ItemType.PrecisionLens), PRECISION_LENS_POSITION, PRECISION_LENS_TAG, -250);

            // Reveals the engine on the sell panel and enable its UI controls
            SellItems.Find(item => item.CompareTag(ENGINE_TAG)).Find("ItemButton").gameObject.SetActive(true);
            SellItems.Find(item => item.CompareTag(ENGINE_TAG)).Find("ItemName").gameObject.SetActive(true);
            SellItems.Find(item => item.CompareTag(ENGINE_TAG)).Find("ItemCount").gameObject.SetActive(true);
            SellItems.Find(item => item.CompareTag(ENGINE_TAG)).Find("ItemValue").gameObject.SetActive(true);
            SellItems.Find(item => item.CompareTag(ENGINE_TAG)).Find("Pearl_Icon").gameObject.SetActive(true);
            SellItems.Find(item => item.CompareTag(ENGINE_TAG)).Find("ItemShadow").gameObject.SetActive(false);
      
            ticker.ShowTicker("Purchased Tier 3 Blueprint — Engine and Precision Lens unlocked.", Color.green, MessageTypes.ResultMessage);
         }

         // Mercenary Engineer purchase flow
         if (currentBuyItem.CompareTag(MERCENARY_ENGINEER_TAG) && inv.TrySpendPearl(GetItemPrice(ItemType.MercenaryEngineer))) 
         {
            ForgeManager.Instance.hasMercenaryEngineer = true;

            if(inv.mercenaryEngineerCount == MAX_MERCENARY_ENGINEER_COUNT)
               BuyItems.Find(item => item.CompareTag(MERCENARY_ENGINEER_TAG)).gameObject.SetActive(false);
            ticker.ShowTicker("Purchased Mercenary Engineer.", Color.green, MessageTypes.ResultMessage);
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
      switch (item.tag) 
      {
         case CRUDE_TOOL_TAG:
            if (crudeToolSellCount < MAX_SELL_ITEM_COUNT) 
            {
               crudeToolSellCount += 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text      = "   " + crudeToolSellCount.ToString();
               item.Find("currencyGained").GetComponent<TextMeshProUGUI>().text = (crudeToolSellCount * GetItemValue(ItemType.CrudeTool)).ToString();
            }
            break;
         case HARPOON_TAG:
            if (harpoonSellCount < MAX_SELL_ITEM_COUNT) 
            {
               harpoonSellCount += 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text      = "   " + harpoonSellCount.ToString();
               item.Find("currencyGained").GetComponent<TextMeshProUGUI>().text = (harpoonSellCount * GetItemValue(ItemType.Harpoon)).ToString();
            }
            break;
         case PRESSURE_VALVE_TAG:
            if (pressureValveCount < MAX_SELL_ITEM_COUNT) 
            {
               pressureValveCount += 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text      = "   " + pressureValveCount.ToString();
               item.Find("currencyGained").GetComponent<TextMeshProUGUI>().text = (pressureValveCount * GetItemValue(ItemType.PressureValve)).ToString();
            }
            break;
         case ENGINE_TAG:
            if (engineSellCount < MAX_SELL_ITEM_COUNT) 
            {
               engineSellCount += 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text      = "   " + engineSellCount.ToString();
               item.Find("currencyGained").GetComponent<TextMeshProUGUI>().text = (engineSellCount * GetItemValue(ItemType.Engine)).ToString();
            }
            break;
         default:
            Debug.LogError("Unknown item tag: " + item.tag);
            break;
      }
   }

   // Decrements the count for the item being sold and updates the UI
   public void DecreaseSellItemCount(Transform item) 
   {
      switch (item.tag) 
      {
         case CRUDE_TOOL_TAG:
            if (crudeToolSellCount > MIN_SELL_ITEM_COUNT) 
            {
               crudeToolSellCount -= 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text      = "   " + crudeToolSellCount.ToString();
               item.Find("currencyGained").GetComponent<TextMeshProUGUI>().text = (crudeToolSellCount * GetItemValue(ItemType.CrudeTool)).ToString();
            }
            break;
         case HARPOON_TAG:
            if (harpoonSellCount > MIN_SELL_ITEM_COUNT) 
            {
               harpoonSellCount -= 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text      = "   " + harpoonSellCount.ToString();
               item.Find("currencyGained").GetComponent<TextMeshProUGUI>().text = (crudeToolSellCount * GetItemValue(ItemType.Harpoon)).ToString();
            }
            break;
         case PRESSURE_VALVE_TAG:
            if (pressureValveCount > MIN_SELL_ITEM_COUNT) 
            {
               pressureValveCount -= 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text      = "   " + pressureValveCount.ToString();
               item.Find("currencyGained").GetComponent<TextMeshProUGUI>().text = (pressureValveCount * GetItemValue(ItemType.PressureValve)).ToString();
            }
            break;
         case ENGINE_TAG:
            if (engineSellCount > MIN_SELL_ITEM_COUNT) 
            {
               engineSellCount -= 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text      = "   " + engineSellCount.ToString();
               item.Find("currencyGained").GetComponent<TextMeshProUGUI>().text = (engineSellCount * GetItemValue(ItemType.Engine)).ToString();
            }
            break;
         default:
            Debug.LogError("Unknown item tag: " + item.tag);
            break;
      }
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

   //public void MysterBoxResult(ResourceType resource, int resourceAmount) 
   //{
   //   MysteryBoxPanel.Find("StartingView").gameObject.SetActive(false);

   //   Transform ResultContainer = MysteryBoxPanel.Find("ResultContainer");
   //   Transform ResultTemplate  = ResultContainer.Find("ResultTemplate");
   //   Transform ResultTransform = Instantiate(ResultTemplate, ResultContainer);

   //   ResultTransform.Find("CurrencyIcon").GetComponent<Image>().sprite = GetResourceSprite(resource);
   //   ResultTransform.Find("CurrencyObtained").GetComponent<TextMeshProUGUI>().text = resourceAmount.ToString();

   //   currentMysteryBoxResult = ResultTransform;
   //   ResultTransform.gameObject.SetActive(true);
   //}

   //public void OpenMysterBox() 
   //{

   //   int successChance = Rng.Next(1, 101);

   //   if(successChance <= 60) 
   //   { 
   //      MysterBoxResult(ResourceType.Pearl, 200);
   //      InventoryManager.Instance.TryAddPearl(200);
   //   }
   //   else 
   //   {
   //      MysterBoxResult(ResourceType.Crystal, 50);
   //      InventoryManager.Instance.TryAddPearl(50);
   //   }
   //}

   // Shows market shifts for next turn
   public void CraftMarketForesight() 
   {
  
      crudeToolChance     = Rng.Next(1, 101);
      harpoonChance       = Rng.Next(1, 101);
      pressureValveChance = Rng.Next(1, 101);
      engineChance        = Rng.Next(1, 101);

      crudeToolFluctuation     = Rng.Next(marketShiftMin, marketShiftMax + 1);
      harpoonFluctuation       = Rng.Next(marketShiftMin, marketShiftMax + 1);
      pressureValveFluctuation = Rng.Next(marketShiftMin, marketShiftMax + 1);
      engineFluctuation        = Rng.Next(marketShiftMin, marketShiftMax + 1);

      TextMeshProUGUI crudeToolValueText     = SellItems.Find(d => d.CompareTag(CRUDE_TOOL_TAG)).Find("NextValue").GetComponent<TextMeshProUGUI>(),
                      harpoonValueText       = SellItems.Find(d => d.CompareTag(HARPOON_TAG)).Find("NextValue").GetComponent<TextMeshProUGUI>(),
                      pressureValveValueText = SellItems.Find(d => d.CompareTag(PRESSURE_VALVE_TAG)).Find("NextValue").GetComponent<TextMeshProUGUI>(),
                      engineValueText        = SellItems.Find(d => d.CompareTag(ENGINE_TAG)).Find("NextValue").GetComponent<TextMeshProUGUI>();

      int baseVal,
          preview;

      // Displays the crude tool sell value for the next turn
      if (worldEvent == (int) WorldEventTypes.CrudeToolEvent  && TurnManager.Instance.eventCountdown == 4) 
         WorldEventForesight(crudeToolValueText);
      else 
      {
         baseVal = GetItemValue(ItemType.CrudeTool);
         preview = baseVal;

         if (TurnManager.Instance.eventCountdown == 0 && worldEvent == (int)WorldEventTypes.CrudeToolEvent) 
         {
            if (shiftDirection <= 50)
               preview = baseVal - ((int)(BASE_CRUDE_TOOL_SELL_VALUE));
            else
               preview = baseVal + ((int)(BASE_CRUDE_TOOL_SELL_VALUE));
         } 
         else 
         {
            if (crudeToolChance <= 30)
               preview = baseVal + crudeToolFluctuation;
            else
               if (crudeToolChance <= 60)
                  preview = baseVal - crudeToolFluctuation;
         }

         // clamp so UI never shows out-of-range values
         preview = Mathf.Clamp(preview, MIN_CRUDE_TOOL_VALUE, MAX_CRUDE_TOOL_VALUE);

         // always write the UI (avoids leaving stale negative text when conditions skip)
         crudeToolValueText.text = "Next Value: " + preview.ToString();
      }

      // Displays the harpoon sell value for the next turn
      if (worldEvent == (int)WorldEventTypes.HarpoonEvent && TurnManager.Instance.eventCountdown == 4)
         WorldEventForesight(harpoonValueText);
      else 
      {
         baseVal = GetItemValue(ItemType.Harpoon);
         preview = baseVal; // default to current

         if(TurnManager.Instance.eventCountdown == 0 && worldEvent == (int)WorldEventTypes.HarpoonEvent) 
         {
            if (shiftDirection <= 50)
               preview = baseVal - ((int)(BASE_HARPON_SELL_VALUE));
            else
               preview = baseVal + ((int)(BASE_HARPON_SELL_VALUE));
         }
          else 
          {
            if (harpoonChance <= 30)
                   preview = baseVal + harpoonFluctuation;
                else 
                   if (harpoonChance <= 60)
                      preview = baseVal - harpoonFluctuation;
          }
          
          // clamp so UI never shows out-of-range values
          preview = Mathf.Clamp(preview, MIN_HARPOON_VALUE, MAX_HARPOON_VALUE);

         // always write the UI (avoids leaving stale negative text when conditions skip)
         harpoonValueText.text = "Next Value: " + preview.ToString();
      }

      if (ForgeManager.Instance.hasTier2Blueprint) 
      {
         // Displays the pressure valve sell value for the next turn
         if (worldEvent == (int)WorldEventTypes.PressureValveEvent && TurnManager.Instance.eventCountdown == 4)
            WorldEventForesight(pressureValveValueText);
         else 
         {
            baseVal = GetItemValue(ItemType.PressureValve);
            preview = baseVal; // default to current

            if (TurnManager.Instance.eventCountdown == 0 && worldEvent == (int)WorldEventTypes.PressureValveEvent) 
            {
               if (shiftDirection <= 50)
                  preview = baseVal - ((int)(BASE_PRESSURE_VALVE_SELL_VALUE));
               else
                  preview = baseVal + ((int)(BASE_PRESSURE_VALVE_SELL_VALUE));
            }
            else 
            {
               if (pressureValveChance <= 30)
                  preview = baseVal + pressureValveFluctuation;
               else
                  if (pressureValveChance <= 60)
                     preview = baseVal - pressureValveFluctuation;
            }

            // clamp so UI never shows out-of-range values
            preview = Mathf.Clamp(preview, MIN_PRESSURE_VALVE_VALUE, MAX_PRESSURE_VALVE_VALUE);

            // always write the UI (avoids leaving stale negative text when conditions skip)
            pressureValveValueText.text = "Next Value: " + preview.ToString();
         }
      }

      if (ForgeManager.Instance.hasTier3Blueprint) 
      {
         // Displays the engine sell value for the next turn
         if (worldEvent == (int)WorldEventTypes.ClockworkEngineEvent && TurnManager.Instance.eventCountdown == 4)
            WorldEventForesight(engineValueText);
         else 
         {
            baseVal = GetItemValue(ItemType.Engine);
            preview = baseVal; // default to current

            if (TurnManager.Instance.eventCountdown == 0 && worldEvent == (int)WorldEventTypes.ClockworkEngineEvent) 
            {
               if (shiftDirection <= 50)
                  preview = baseVal - ((int)(BASE_ENGINE_VALUE));
               else
                  preview = baseVal + ((int)(BASE_ENGINE_VALUE));
            } 
            else 
            {
               if (engineChance <= 30)
                  preview = baseVal + engineFluctuation;
               else
                  if (engineChance <= 60)
                     preview = baseVal - engineFluctuation;
            }

            // clamp so UI never shows out-of-range values
            preview = Mathf.Clamp(preview, MIN_ENGINE_VALUE, MAX_ENGINE_VALUE);

            // always write the UI (avoids leaving stale negative text when conditions skip)
            engineValueText.text = "Next Value: " + preview.ToString();
         }
      }
   }

   // Shifts the sell market each turn and during world events
   public void MarketFluctuate() 
   {
      // Crude Tool fluctuations
      if (worldEvent == (int)WorldEventTypes.CrudeToolEvent && TurnManager.Instance.eventCountdown == 5) 
      {
         if(shiftDirection <= 50)
            TryIncreaseCrudeToolSellValue(crudeToolFluctuation);
         else 
            TryDecreaseCrudeToolSellValue(crudeToolFluctuation);
      }
      else 
      {
         if (lastResetTurn[ItemType.CrudeTool] && TurnManager.Instance.eventCountdown == 1)
            lastResetTurn[ItemType.CrudeTool] = false;
         else 
         {
            if (crudeToolChance <= 30)
               TryIncreaseCrudeToolSellValue(crudeToolFluctuation);
            else
               if(crudeToolChance <= 60) 
                  TryDecreaseCrudeToolSellValue(crudeToolFluctuation);
         }
      }

      // Harpoon fluctuations
      if (worldEvent == (int)WorldEventTypes.HarpoonEvent && TurnManager.Instance.eventCountdown == 5) 
      {
         if (shiftDirection <= 50)
            TryIncreaseHarpoonSellValue(harpoonFluctuation);
         else
            TryDecreaseHarpoonSellValue(harpoonFluctuation);
      } 
      else 
      {
         if (lastResetTurn[ItemType.Harpoon] && TurnManager.Instance.eventCountdown == 1)
            lastResetTurn[ItemType.Harpoon] = false;
         else 
         {
            if (harpoonChance <= 30) 
               TryIncreaseHarpoonSellValue(harpoonFluctuation);
            else 
               if(harpoonChance <= 60) 
                  TryDecreaseHarpoonSellValue(harpoonFluctuation);
         }
      }
     
      // Pressure Valve fluctuations
      if (ForgeManager.Instance.hasTier2Blueprint) 
      {
         if (worldEvent == (int)WorldEventTypes.PressureValveEvent && TurnManager.Instance.eventCountdown == 5) 
         {
            if (shiftDirection <= 50)
               TryIncreasePressureValveValue(pressureValveFluctuation);
            else
               TryDecreasePressureValveValue(pressureValveFluctuation);
         } 
         else 
         {
            if (pressureValveChance <= 30) 
               TryIncreasePressureValveValue(pressureValveFluctuation);
            else 
               if(pressureValveChance <= 60) 
                  TryDecreasePressureValveValue(pressureValveFluctuation);
         }
      }

      // Clockwork engine fluctuations
      if (ForgeManager.Instance.hasTier3Blueprint) 
      {
         if (worldEvent == (int)WorldEventTypes.ClockworkEngineEvent && TurnManager.Instance.eventCountdown == 5) 
         {
            if (shiftDirection <= 50) 
               TryDecreaseEnginesSellValue(engineFluctuation);
            else 
               TryDecreaseEnginesSellValue(engineFluctuation);
         } 
         else 
         {
            if (engineChance <= 30) 
               TryIncreaseEngineSellValue(engineFluctuation);
            else 
               if(engineChance <= 60) 
                  TryDecreaseEnginesSellValue(engineFluctuation);
         }
         return;
      }
   }

   // Determines world event selection and shift direction for the next cycle.
   public void WorldEventChance() 
   {
      int worldEvent1 = (int)WorldEventTypes.CrudeToolEvent,
          finalWorldEvent;

      if(ForgeManager.Instance.hasTier3Blueprint)
         finalWorldEvent = (int)WorldEventTypes.ClockworkEngineEvent;
      else
         if(ForgeManager.Instance.hasTier2Blueprint)
            finalWorldEvent = (int)(WorldEventTypes.PressureValveEvent);
      else
         finalWorldEvent = (int)(WorldEventTypes.HarpoonEvent);

      worldEvent     = Rng.Next(worldEvent1, finalWorldEvent + 1);
      shiftDirection = isTier3BuffACtive ?  50 : Rng.Next(1, 101);
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
         case (int)WorldEventTypes.PressureValveEvent:
            currrentNewsTickerMessage = GetPressureValveMessage(shiftDirection);
            break;
         case (int)WorldEventTypes.ClockworkEngineEvent:
            currrentNewsTickerMessage = GetClockWorkEngineMessage(shiftDirection);
            break;
         default:
            Debug.LogError("Unknown Event");
            break;
      }
   }

   // Displays the next world event's market fluctuations
   public void WorldEventForesight(TextMeshProUGUI sellValueText) 
   {
      int preview; 

      switch (worldEvent) 
      { 
         // Crude tool world event foresight
         case (int) WorldEventTypes.CrudeToolEvent:
            crudeToolFluctuation = GetItemValue(ItemType.CrudeTool);

            if (shiftDirection <= 50)
               preview = GetItemValue(ItemType.CrudeTool) + crudeToolFluctuation;
            else
               preview = GetItemValue(ItemType.CrudeTool) - crudeToolFluctuation;

            preview = Mathf.Clamp(preview, MIN_CRUDE_TOOL_VALUE, MAX_CRUDE_TOOL_VALUE);
            sellValueText.text = "Next Value: " + preview.ToString();
            break;

         // Harpoon world event foresight
         case (int) WorldEventTypes.HarpoonEvent:
            harpoonFluctuation = GetItemValue(ItemType.Harpoon);

            if (shiftDirection <= 50)
               preview = GetItemValue(ItemType.Harpoon) + harpoonFluctuation;
            else
               preview = GetItemValue(ItemType.Harpoon) - harpoonFluctuation;

            preview = Mathf.Clamp(preview, MIN_HARPOON_VALUE, MAX_HARPOON_VALUE);
            sellValueText.text = "Next Value: " + preview.ToString();
            break;

         // Pressure valve world event foresight
         case (int)WorldEventTypes.PressureValveEvent:
            pressureValveFluctuation = GetItemValue(ItemType.PressureValve);

            if (shiftDirection <= 50)
               preview = GetItemValue(ItemType.PressureValve) + pressureValveFluctuation;
            else
               preview = GetItemValue(ItemType.PressureValve) - pressureValveFluctuation;

            preview = Mathf.Clamp(preview, MIN_PRESSURE_VALVE_VALUE, MAX_PRESSURE_VALVE_VALUE);
            sellValueText.text = "Next Value: " + preview.ToString();
            break;

         // Engine world event foresight
         case (int)WorldEventTypes.ClockworkEngineEvent:
            engineFluctuation = GetItemValue(ItemType.Engine);

            if (shiftDirection <= 50)
               preview = GetItemValue(ItemType.Engine) + engineFluctuation;
            else
               preview = GetItemValue(ItemType.Engine) - engineFluctuation;

            preview = Mathf.Clamp(preview, MIN_ENGINE_VALUE, MAX_ENGINE_VALUE);
            sellValueText.text = "Next Value: " + preview.ToString();
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
           if (shiftDirection <= 50) 
              TryDecreaseCrudeToolSellValue((int)(BASE_CRUDE_TOOL_SELL_VALUE));
           else 
              TryIncreaseCrudeToolSellValue((int)(BASE_CRUDE_TOOL_SELL_VALUE));

            // Flag the reset for crude tool
            lastResetTurn[ItemType.CrudeTool] = true;
           break;

         // Undo the crude tool event shift based on previous direction
         case (int)WorldEventTypes.HarpoonEvent:
            if (shiftDirection <= 50) 
               TryDecreaseHarpoonSellValue((int)(BASE_HARPON_SELL_VALUE));
            else 
               TryIncreaseHarpoonSellValue((int)(BASE_HARPON_SELL_VALUE));

            // Flag the reset for harpoon
            lastResetTurn[ItemType.Harpoon] = true;
            break;

         // Undo the pressure valve event shift based on previous direction
         case (int)WorldEventTypes.PressureValveEvent:
           if (shiftDirection <= 50)
              TryDecreasePressureValveValue((int)(BASE_PRESSURE_VALVE_SELL_VALUE));
           else
              TryIncreasePressureValveValue((int)(BASE_PRESSURE_VALVE_SELL_VALUE));
  
           // Flag the reset for Pressure Valve
           lastResetTurn[ItemType.PressureValve] = true;
           break;

         // Undo the engine event shift based on previous direction
         case (int)WorldEventTypes.ClockworkEngineEvent:
           if (shiftDirection <= 50)
              TryDecreaseEnginesSellValue((int)(BASE_ENGINE_VALUE));
           else
              TryIncreaseEngineSellValue((int)(BASE_ENGINE_VALUE));
  
           // Flag the reset for Engine
           lastResetTurn[ItemType.Engine] = true;
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
            SellWindow.Find("SellButton").GetComponent<Button>().onClick.AddListener(() => SellItem());
            BuyWindow.Find("BuyButton").GetComponent<Button>().onClick.AddListener(() => BuyItem());
            TradePanels.Find("ExitButton").GetComponent<Button>().onClick.AddListener(() => CloseTradeHutPanel(TRADE_BUTTON));
            break;
         case INFO_BUTTON:
            ShowInfoPanel();
            InfoPanel.Find("ExitButton").GetComponent<Button>().onClick.AddListener(() => CloseTradeHutPanel(INFO_BUTTON));
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
         case UPGRADE_BUTTON:
            CloseUpgradePanel();
            break;
         default:
            Debug.Log("Building Panel: Unknown button ID.");
            break;
      }
      PopUpManager.Instance.EnablePlayerInput();
   }

   private void ShowTradePanel() 
   {
      TradePanels.gameObject.SetActive(true);
      ShowSellPanel();
   }

   private void ShowInfoPanel() 
   {
      InfoPanel.gameObject.SetActive(true);
   }

   public void ShowSellPanel() 
   {
      if (BuyPanel.gameObject.activeSelf) 
      {
         if (BuyWindow.gameObject.activeSelf)
            CloseBuyWindow();

         CloseBuyPanel();
      }

      // Destroy the instantiated buy item/window instance if it exists
      if (currentBuyItem != null) 
      {
         Destroy(currentBuyItem.gameObject);
         currentBuyItem = null;
      }
      SellPanel.gameObject.SetActive(true);
   }

   public void ShowBuyPanel() 
   {
      if (SellPanel.gameObject.activeSelf) 
      {
         if (SellWindow.gameObject.activeSelf)
            CloseSellWindow();

         CloseSellPanel();
      }

      // Destroy the instantiated sell item/window instance if it exists
      if (currentSellItem != null) 
      {
         Destroy(currentSellItem.gameObject);
         currentSellItem = null;
      }
      BuyPanel.gameObject.SetActive(true);
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
      TradePanels.gameObject.SetActive(false);

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
      rawOreExchange       = MIN_BUY_ITEM_COUNT;

      if (SellWindow.gameObject.activeSelf)
         CloseSellWindow();

      if (BuyWindow.gameObject.activeSelf)
         CloseBuyWindow();
   }

   private void CloseInfoPanel() 
   {
      InfoPanel.gameObject.SetActive(false);
   }

   private void CloseUpgradePanel() 
   {
      UpgradePanel.gameObject.SetActive(false);
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