using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using static Item;
using static Resources;
using static WorldEvents;

public class TradeHutManager : MonoBehaviour 
{
   /* Inspector variables                                                                             */
   [SerializeField] private Transform TradePanels;            
                    public  Transform BuyPanel;       
   [SerializeField] private Transform BuyWindow;    
   [SerializeField] private Transform SellPanel;              
   [SerializeField] private Transform SellWindow;     
   [SerializeField] private Transform InfoPanel;                   
   [SerializeField] private Transform UpgradePanel;           
   [SerializeField] private Transform MysteryBoxPanel;
   
   public List<Transform> Items { get; private set; }

   [SerializeField] private TextMeshProUGUI tradeHutLevelText;

   public string currrentNewsTickerMessage;

   private readonly static System.Random Rng = new System.Random();

   /* Transforms                                                                                      */
   private Transform currentBuyItem,    
                     currentSellItem,
                     currentMysteryBoxResult;

   /* Private variables                                                                               */
   private int crudeToolSellCount = 0,
               harpoonSellCount   = 0, 
               pressureValveCount = 0, 
               engineSellCount    = 0, 

               rawOreExchange           = 0,
               industrialBluePrintCount = 0,
               clockworkBluePrintCount  = 0,
               mercenaryEngineerCount   = 0,
       
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

   /* Public variables                                                                                 */
   public int marketShiftMax = 0,
              marketShiftMin = 0;

   /* Constants                                                                                        */
   public const int ENDING_LEVEL   = 5,  
                    STARTING_LEVEL = 1, 
      
                    MAX_BUY_ITEM_COUNT  = 100,   
                    MAX_SELL_ITEM_COUNT = 100,   
                    MIN_BUY_ITEM_COUNT  = 0,   
                    MIN_SELL_ITEM_COUNT = 0, 
      
                    TRADE_BUTTON    = 1,     
                    INFO_BUTTON     = 2,     
                    UPGRADE_BUTTON  = 3,

                    BUY_ITEM_SPACING = 30;
      
   public const string CRUDE_TOOL_TAG            = "Crude Tool",
                       HARPOON_TAG               = "Harpoon",
                       PRESSURE_VALVE_TAG        = "Pressure Valve",
                       ENGINE_TAG                = "Engine",
                       RAW_ORE_CHUNK_TAG         = "Raw Ore Chunk",
                       INDUSTRIAL_BLUE_PRINT_TAG = "Industrial Blue Print",
                       CLOCKWORK_BLUEPRINT_TAG   = "Clockwork Blue Print",
                       MERCENARY_ENGINEER_TAG    = "Mercenary Engineer";



   public static int tradeHutLevel;
   private InventoryManager inv;

   public static TradeHutManager Instance;

   private void Awake() 
   {
      tradeHutLevel = STARTING_LEVEL;
      Items         = new();

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

      if (tradeHutLevelText != null)
         tradeHutLevelText.text = "Level " + tradeHutLevel.ToString();
      else
         Debug.LogError("Trade Hut Level Text is not assigned in the Inspector!");

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
      
      if(MysteryBoxPanel == null)
         Debug.LogError("Mystery Box Panel is not assigned in the Inspector!");
      else
         MysteryBoxPanel.gameObject.SetActive(false);
   }

   private void Start()
   {
      inv = InventoryManager.Instance;

      CreateSellItem(GetItemSprite(ItemType.CrudeTool),GetItemValue(ItemType.CrudeTool), -1.0f, CRUDE_TOOL_TAG);
      CreateSellItem(GetItemSprite(ItemType.Harpoon), GetItemValue(ItemType.Harpoon), 0.0f, HARPOON_TAG);
      //CreateSellItem(GetItemSprite(ItemType.PressureValve), GetItemValue(ItemType.PressureValve), 1.0f, PRESSURE_VALVE_TAG);
      //CreateSellItem(GetItemSprite(ItemType.Engine), GetItemValue(ItemType.Engine), 2.0f, ENGINE_TAG);

      CreateBuyItem(GetItemSprite(ItemType.RawOreChunk), GetItemPrice(ItemType.RawOreChunk), 0.0f, RAW_ORE_CHUNK_TAG);
      CreateBuyItem(GetItemSprite(ItemType.IndustrialBluePrint), GetItemPrice(ItemType.IndustrialBluePrint), 1.0f, INDUSTRIAL_BLUE_PRINT_TAG);
      CreateBuyItem(GetItemSprite(ItemType.ClockworkBlueprint), GetItemPrice(ItemType.ClockworkBlueprint), 2.0f, CLOCKWORK_BLUEPRINT_TAG);
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

      Items.Add(tradeItemTransform);

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

      /* Instantiate the template and set its position in the container                               */
      tradeItemTransform     = Instantiate(buyItemTemplate, buyItemContainer);
      buyItemTemplate.gameObject.SetActive(false);
      tradeItemRectTransform = tradeItemTransform.GetComponent<RectTransform>();
      tradeItemRectTransform.anchoredPosition = new Vector2(BUY_ITEM_SPACING * positionIndex, verticalIndex);     

      /* Populate the item properties                                                                 */
      tradeItemTransform.tag = itemTag;
      tradeItemTransform.Find("ItemName").GetComponent<TextMeshProUGUI>().text  = itemTag.ToString();
      tradeItemTransform.Find("ItemValue").GetComponent<TextMeshProUGUI>().text = itemValue.ToString();
      itemButton = tradeItemTransform.Find("ItemButton").GetComponent<Button>();

      itemButton.image.sprite = itemSprite;

      /* Dynamically add a listener to the button, which creates the buy window                       */
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

      /* Destroy the previously opened sell window instance before creating a new one                 */
      if (currentSellItem != null) 
      {
         Destroy(currentSellItem.gameObject);
         currentSellItem = null;
      }

      Transform     sellItemTransform     = Instantiate(sellWindowTemplate, sellWindowContainer);
      RectTransform sellItemRectTransform = sellItemTransform.GetComponent<RectTransform>();

      sellItemTransform.tag = itemTag;

      sellItemRectTransform.anchoredPosition = new Vector2(BUY_ITEM_SPACING * 0, 0);

      /* Populate item properties                                                                     */
      sellItemTransform.Find("ItemImage").GetComponent<Image>().sprite              = itemSprite;
      sellItemTransform.Find("ItemCount").GetComponent<TextMeshProUGUI>().text      = "   " + itemCount.ToString();
      sellItemTransform.Find("currencyIcon").GetComponent<Image>().sprite           = currencySprite;
      sellItemTransform.Find("currencyGained").GetComponent<TextMeshProUGUI>().text = "0";

      /* Get references to the increase and decrease buttons                                          */
      Button increaseButton = sellItemTransform.Find("QuantityButtons/IncreaseButton").GetComponent<Button>();
      Button decreaseButton = sellItemTransform.Find("QuantityButtons/DecreaseButton").GetComponent<Button>();

      /* Dynamically add listeners to the buttons, increasing or decreasing the sell items            */
      increaseButton.onClick.AddListener(() => IncreaseSellItemCount(sellItemTransform));
      decreaseButton.onClick.AddListener(() => DecreaseSellItemCount(sellItemTransform));

      /* Store the reference to the newly created sell window instance                                */
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

      /* Destroy the previously opened buy window instance before creating a new one                  */
      if (currentBuyItem != null) 
      {
         Destroy(currentBuyItem.gameObject);
         currentBuyItem = null;
      }

      Transform     buyItemTransfrom              = Instantiate(buyWindowTemplate, buyWindowContainer);
      RectTransform buyItemTransfromRectTransform = buyItemTransfrom.GetComponent<RectTransform>();

      buyItemTransfrom.tag = itemTag;

      //buyItemTransfromRectTransform.anchoredPosition = new Vector2(BUY_ITEM_SPACING * 0, 0);

      /* Populate item properties                                                                     */
      buyItemTransfrom.Find("ItemImage").GetComponent<Image>().sprite             = itemSprite;
      buyItemTransfrom.Find("ItemCount").GetComponent<TextMeshProUGUI>().text     = "   " + itemCount.ToString();
      buyItemTransfrom.Find("currencyIcon").GetComponent<Image>().sprite          = currencySprite;
      buyItemTransfrom.Find("currencySpent").GetComponent<TextMeshProUGUI>().text = "0";

      /* Get references to the increase and decrease buttons                                          */
      Button increaseButton = buyItemTransfrom.Find("QuantityButtons/IncreaseButton").GetComponent<Button>();
      Button decreaseButton = buyItemTransfrom.Find("QuantityButtons/DecreaseButton").GetComponent<Button>();

      /* Dynamically add listeners to the buttons, which increases or decreases the buy item count    */
      increaseButton.onClick.AddListener(() => IncreaseBuyItemsCount(buyItemTransfrom));
      decreaseButton.onClick.AddListener(() => DecreaseBuyItemsCount(buyItemTransfrom));

      /* Store the reference to the newly created buy window instance                                 */
      currentBuyItem = buyItemTransfrom;
      buyItemTransfrom.gameObject.SetActive(true);
      ShowBuyWindow();
   }

   public void SellItem() 
   {
      int totalSellValue  = 0;

      if (crudeToolSellCount > MIN_SELL_ITEM_COUNT) 
      {
         if(inv.TryUseCrudeTool(crudeToolSellCount))
            totalSellValue += crudeToolSellCount * GetItemValue(ItemType.CrudeTool);
         else
            crudeToolSellCount = MIN_SELL_ITEM_COUNT;
      }

      if (harpoonSellCount > MIN_SELL_ITEM_COUNT) 
      {
         if(inv.TryUseHarpoon(harpoonSellCount))
            totalSellValue += harpoonSellCount * GetItemValue(ItemType.Harpoon);
         else
            harpoonSellCount = MIN_SELL_ITEM_COUNT;
      }

      if (pressureValveCount > MIN_SELL_ITEM_COUNT) 
      {
         if(inv.TryUseHarpoon(pressureValveCount))
            totalSellValue += pressureValveCount * GetItemValue(ItemType.PressureValve);
         else
            pressureValveCount = MIN_SELL_ITEM_COUNT;
      }

      if (engineSellCount > MIN_SELL_ITEM_COUNT) 
      {
         if(inv.TryUseEngine(engineSellCount))
            totalSellValue += engineSellCount * GetItemValue(ItemType.Engine);
         else
            engineSellCount = MIN_SELL_ITEM_COUNT;
      }

      inv.TryAddPearl(totalSellValue);

      crudeToolSellCount = MIN_SELL_ITEM_COUNT;
      harpoonSellCount   = MIN_SELL_ITEM_COUNT;
      pressureValveCount = MIN_SELL_ITEM_COUNT;
      engineSellCount    = MIN_SELL_ITEM_COUNT;

      /* Destroy the instantiated sell window and remove the reference                                */
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
      if (rawOreExchange > MIN_BUY_ITEM_COUNT) 
      { 
         inv.TrySpendPearl(rawOreExchange);
         inv.TryAddOre(rawOreExchange);
      }

      if(industrialBluePrintCount > MIN_BUY_ITEM_COUNT)
         inv.TrySpendPearl(industrialBluePrintCount * GetItemPrice(ItemType.IndustrialBluePrint));

      if(clockworkBluePrintCount > MIN_BUY_ITEM_COUNT)
         inv.TrySpendPearl(clockworkBluePrintCount * GetItemPrice(ItemType.ClockworkBlueprint));

      if(mercenaryEngineerCount > MIN_BUY_ITEM_COUNT)
         inv.TrySpendPearl(mercenaryEngineerCount * GetItemPrice(ItemType.MercenaryEngineer));

      rawOreExchange           = MIN_BUY_ITEM_COUNT;
      industrialBluePrintCount = MIN_BUY_ITEM_COUNT;
      clockworkBluePrintCount  = MIN_BUY_ITEM_COUNT;

      if(currentBuyItem != null) 
      {
         Destroy(currentBuyItem.gameObject);
         currentBuyItem = null;
      }

      CloseBuyWindow();

      return;
   }

   /* Increments the count for the item being sold and updates the UI                                 */
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
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text      = "   x" + pressureValveCount.ToString();
               item.Find("currencyGained").GetComponent<TextMeshProUGUI>().text = (pressureValveCount * GetItemValue(ItemType.PressureValve)).ToString();
            }
            break;
         case ENGINE_TAG:
            if (engineSellCount < MAX_SELL_ITEM_COUNT) 
            {
               engineSellCount += 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text      = "   x" + engineSellCount.ToString();
               item.Find("currencyGained").GetComponent<TextMeshProUGUI>().text = (engineSellCount * GetItemValue(ItemType.Engine)).ToString();
            }
            break;
         default:
            Debug.LogError("Unknown item tag: " + item.tag);
            break;
      }
   }

   /* Decrements the count for the item being sold and updates the UI                                 */
   public void DecreaseSellItemCount(Transform item) 
   {
      switch (item.tag) 
      {
         case CRUDE_TOOL_TAG:
            if (crudeToolSellCount > MIN_SELL_ITEM_COUNT) 
            {
               crudeToolSellCount -= 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text      = "   x" + crudeToolSellCount.ToString();
               item.Find("currencyGained").GetComponent<TextMeshProUGUI>().text = (crudeToolSellCount * GetItemValue(ItemType.CrudeTool)).ToString();
            }
            break;
         case HARPOON_TAG:
            if (harpoonSellCount > MIN_SELL_ITEM_COUNT) 
            {
               harpoonSellCount -= 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text      = "   x" + harpoonSellCount.ToString();
               item.Find("currencyGained").GetComponent<TextMeshProUGUI>().text = (crudeToolSellCount * GetItemValue(ItemType.Harpoon)).ToString();
            }
            break;
         case PRESSURE_VALVE_TAG:
            if (pressureValveCount > MIN_SELL_ITEM_COUNT) 
            {
               pressureValveCount -= 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text      = "   x" + pressureValveCount.ToString();
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

   /* Increments the count for the item being bought and updates the UI                               */
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
         case INDUSTRIAL_BLUE_PRINT_TAG:
            if (industrialBluePrintCount < MAX_BUY_ITEM_COUNT) 
            {
               industrialBluePrintCount += 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text     = "   " + industrialBluePrintCount.ToString();
               item.Find("currencySpent").GetComponent<TextMeshProUGUI>().text = (industrialBluePrintCount * GetItemPrice(ItemType.IndustrialBluePrint)).ToString();
            }
            break;
         case CLOCKWORK_BLUEPRINT_TAG:
            if (clockworkBluePrintCount < MAX_BUY_ITEM_COUNT) 
            {
               clockworkBluePrintCount += 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text     = "   " + clockworkBluePrintCount.ToString();
               item.Find("currencySpent").GetComponent<TextMeshProUGUI>().text = (clockworkBluePrintCount * GetItemPrice(ItemType.ClockworkBlueprint)).ToString();
            }
            break;
         case MERCENARY_ENGINEER_TAG:
            if (mercenaryEngineerCount < MAX_BUY_ITEM_COUNT) 
            {
               mercenaryEngineerCount += 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text     = "   " + mercenaryEngineerCount.ToString();
               item.Find("currencySpent").GetComponent<TextMeshProUGUI>().text = (mercenaryEngineerCount * GetItemPrice(ItemType.MercenaryEngineer)).ToString();
            }
            break;
         default:
            Debug.LogError("Unknown item tag: " + item.tag);
            break;
      }
   }

   /* Decrements the count for the item being bought and updates the UI                               */
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
         case INDUSTRIAL_BLUE_PRINT_TAG:
            if (industrialBluePrintCount > MIN_BUY_ITEM_COUNT) 
            {
               industrialBluePrintCount -= 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text     = "   " + industrialBluePrintCount.ToString();
               item.Find("currencySpent").GetComponent<TextMeshProUGUI>().text = (industrialBluePrintCount * GetItemPrice(ItemType.IndustrialBluePrint)).ToString();
            }
            break;
         case CLOCKWORK_BLUEPRINT_TAG:
            if(clockworkBluePrintCount  > MIN_BUY_ITEM_COUNT)
            { 
               clockworkBluePrintCount -= 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text     = "   " + clockworkBluePrintCount.ToString();
               item.Find("currencySpent").GetComponent<TextMeshProUGUI>().text = (clockworkBluePrintCount * GetItemPrice(ItemType.ClockworkBlueprint)).ToString();
            }
            break;
         case MERCENARY_ENGINEER_TAG:
            if(mercenaryEngineerCount  > MIN_BUY_ITEM_COUNT)
            { 
               mercenaryEngineerCount -= 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text     = "   " + mercenaryEngineerCount.ToString();
               item.Find("currencySpent").GetComponent<TextMeshProUGUI>().text = (mercenaryEngineerCount * GetItemPrice(ItemType.MercenaryEngineer)).ToString();
            }
            break;
         default:
            Debug.LogError("Unknown item tag: " + item.tag);
            break;
      }
   }

   public void MysterBoxResult(ResourceType resource, int resourceAmount) 
   {
      MysteryBoxPanel.Find("StartingView").gameObject.SetActive(false);

      Transform ResultContainer = MysteryBoxPanel.Find("ResultContainer");
      Transform ResultTemplate  = ResultContainer.Find("ResultTemplate");
      Transform ResultTransform = Instantiate(ResultTemplate, ResultContainer);

      ResultTransform.Find("CurrencyIcon").GetComponent<Image>().sprite = GetResourceSprite(resource);
      ResultTransform.Find("CurrencyObtained").GetComponent<TextMeshProUGUI>().text = resourceAmount.ToString();

      currentMysteryBoxResult = ResultTransform;
      ResultTransform.gameObject.SetActive(true);
   }

   public void OpenMysterBox() 
   {

      int successChance = Rng.Next(1, 101);

      if(successChance <= 60) 
      { 
         MysterBoxResult(ResourceType.Pearl, 200);
         InventoryManager.Instance.TryAddPearl(200);
      }
      else 
      {
         MysterBoxResult(ResourceType.Crystal, 50);
         InventoryManager.Instance.TryAddPearl(50);
      }
   }

   /* Shows market shifts for next turn */
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

      TextMeshProUGUI crudeToolValueText     = Items.Find(d => d.CompareTag(CRUDE_TOOL_TAG)).Find("NextValue").GetComponent<TextMeshProUGUI>(),
                      harpoonValueText       = Items.Find(d => d.CompareTag(HARPOON_TAG)).Find("NextValue").GetComponent<TextMeshProUGUI>();
                      //pressureValveValueText = Items.Find(d => d.CompareTag(PRESSURE_VALVE_TAG)).Find("NextValue").GetComponent<TextMeshProUGUI>(),
                      //engineValueText        = Items.Find(d => d.CompareTag(ENGINE_TAG)).Find("NextValue").GetComponent<TextMeshProUGUI>();


      if (crudeToolChance <= 30) 
      {
         if((crudeToolSellValue + crudeToolFluctuation) <= MAX_CRUDE_TOOL_VALUE)
            crudeToolValueText.text = "Next Value: " + (crudeToolSellValue + crudeToolFluctuation).ToString();
      }
      else
         if(crudeToolChance <= 60) 
         {
            if((crudeToolSellValue - crudeToolFluctuation) >= MIN_CRUDE_TOOL_VALUE)
               crudeToolValueText.text = "Next Value: " + (crudeToolSellValue - crudeToolFluctuation).ToString();
         }

      if(harpoonChance <= 30) 
      {
         if((harpoonSellValue + harpoonFluctuation) <= MAX_HARPOON_VALUE)
           harpoonValueText.text = "Next Value: " + (harpoonSellValue + harpoonFluctuation).ToString();
      }
      else
         if(harpoonChance <= 60) 
         {
            if((harpoonSellValue - harpoonFluctuation) >= MIN_HARPOON_VALUE)
               harpoonValueText.text = "Next Value: " + (harpoonSellValue - harpoonFluctuation).ToString();
         }

      //if(pressureValveChance <= 30) 
      //{
         //if((pressureValveSellValue + pressureValveFluctuation) <= MAX_PRESSURE_VALVE_VALUE)
            //pressureValveValueText.text = "Next Value: " + (pressureValveSellValue + pressureValveFluctuation).ToString();
      //}
         
      //else
         //if(pressureValveChance <= 60) 
         //{
            //if((pressureValveSellValue - pressureValveFluctuation) >= MIN_PRESSURE_VALVE_VALUE)
               //pressureValveValueText.text = "Next Value: " + (pressureValveSellValue - pressureValveFluctuation).ToString();
         //}
      
      //if(engineChance <= 30) 
      //{
         //if((engineSellValue + engineFluctuation) <= MAX_ENGINE_VALUE)
            //engineValueText.text = "Next Value: " + (engineSellValue + engineFluctuation).ToString();
      //}
      //else
         //if(engineChance <= 60) 
          //{
            //if((engineSellValue - engineFluctuation) >= MIN_ENGINE_VALUE)
               //engineValueText.text = "Next Value: " + (engineSellValue - engineFluctuation).ToString();
          //}
   }

   /* Shifts the sell market each turn.  */
   public void MarketFluctuate() 
   {
      //float fluctuationPercent;
      //int crudeToolValue     = GetItemValue(ItemType.CrudeTool),
      //    harpoonValue       = GetItemValue(ItemType.Harpoon),
      //    pressureValveValue = GetItemValue(ItemType.PressureValve),
      //    engineValue        = GetItemValue(ItemType.Engine);

      if (crudeToolChance <= 30) 
      {
         //fluctuationPercent =  (.01f * Rng.Next(50, 101));
         //pearlAmount = (int) (crudeToolValue * fluctuationPercent);
         Debug.Log("crude tool amount +" + crudeToolFluctuation);
         TryIncreaseCrudeToolSellValue(crudeToolFluctuation);
      }
      else 
         if(crudeToolChance <= 60) 
         {
            //fluctuationPercent =  (.01f *  (float) Math.Round((double) Rng.Next(50, 101)));
            //pearlAmount = (int) (crudeToolValue * fluctuationPercent);
            Debug.Log("crude tool amount -" + crudeToolFluctuation);
            TryDecreaseCrudeToolSellValue(crudeToolFluctuation);
         }

      if (harpoonChance <= 30) 
      {
         //fluctuationPercent =  (.01f *  (float) Math.Round((double) Rng.Next(50, 101)));
         //pearlAmount = (int) (weaponValue * fluctuationPercent);
         Debug.Log("harpoon amount +" + harpoonFluctuation);
         TryIncreaseHarpoonSellValue(harpoonFluctuation);
      }
      else 
         if(harpoonChance <= 60) 
         {
            //fluctuationPercent =  (.01f *  (float) Math.Round((double) Rng.Next(50, 101)));
            //pearlAmount = (int) (weaponValue * fluctuationPercent);
            Debug.Log("harpoon amount -" + harpoonFluctuation);
            TryDecreaseHarpoonSellValue(harpoonFluctuation);
         }

      //if (pressureValveChance <= 30) 
      //{
      //   //fluctuationPercent =  (.01f *  (float) Math.Round((double) Rng.Next(50, 101)));
      //   //pearlAmount = (int) (weaponValue * fluctuationPercent);
      //   Debug.Log("pressure valve amount +" + pressureValveFluctuation);
      //   TryIncreasePressureValveValue(pressureValveFluctuation);
      //}
      //else 
      //   if(pressureValveChance <= 60) 
      //   {
      //      //fluctuationPercent =  (.01f *  (float) Math.Round((double) Rng.Next(50, 101)));
      //      //pearlAmount = (int) (weaponValue * fluctuationPercent);
      //      Debug.Log("pressure valve amount  -" + pressureValveFluctuation);
      //      TryDecreasePressureValveValue(pressureValveFluctuation);
      //   }

      //if (engineChance <= 30) 
      //{
      //   //fluctuationPercent = (.01f * (float)Math.Round((float)Rng.Next(50, 101)));
      //   //pearlAmount = (int) (engineValue * fluctuationPercent);
      //   Debug.Log("engine amount +" + engineFluctuation);
      //   TryIncreaseEnginesSellValue(engineFluctuation);
      //}
      //else 
      //   if(engineChance <= 60) 
      //   {
      //      //fluctuationPercent = (.01f * (float)Math.Round((float)Rng.Next(50, 101)));
      //      //pearlAmount = (int) (engineValue * fluctuationPercent);
      //      Debug.Log("engine amount -" + engineFluctuation);
      //      TryDecreaseEnginesSellValue(engineFluctuation);
      //   }
      //return;
   }

   /* Determines world event selection and shift direction for the next cycle. */
   public void WorldEventChance() 
   {
      int worldEvent1 = (int)WorldEventTypes.CrudeToolEvent,
          worldEvent4 = (int)WorldEventTypes.ClockworkEngineEvent;

      worldEvent     = Rng.Next(worldEvent1, worldEvent4);
      shiftDirection = Rng.Next(1, 101);
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

   public void WorldEvent() 
   {
      switch (worldEvent) 
      { 
         case (int) WorldEventTypes.CrudeToolEvent:
            if(shiftDirection <= 50 ) 
               TryIncreaseCrudeToolSellValue(GetItemValue(ItemType.CrudeTool));
            else
               TryDecreaseCrudeToolSellValue(GetItemValue(ItemType.CrudeTool));
            break;
         case (int) WorldEventTypes.HarpoonEvent:
            if(shiftDirection <= 50 ) 
               TryIncreaseHarpoonSellValue(GetItemValue(ItemType.Harpoon));
            else 
               TryDecreaseHarpoonSellValue(GetItemValue(ItemType.Harpoon));
            break;
         case (int)WorldEventTypes.PressureValveEvent:
            if(shiftDirection <= 50 ) 
               TryIncreasePressureValveValue(GetItemValue(ItemType.PressureValve));
            else 
               TryDecreasePressureValveValue(GetItemValue(ItemType.PressureValve));
            break;
         case (int)WorldEventTypes.ClockworkEngineEvent:
            if(shiftDirection <= 50 ) 
               TryIncreaseEnginesSellValue(GetItemValue(ItemType.Engine));
            else 
               TryDecreaseEnginesSellValue(GetItemValue(ItemType.Engine));
            break;
         default:
            Debug.LogError("Unknown Event");
            break;
      }

   }

   public void ResetWorldEventShifts() 
   {
      Debug.Log($"Shift direction: {shiftDirection}");
      Debug.Log($"World Event: {worldEvent}");
      switch (worldEvent) 
      {
         case (int)WorldEventTypes.CrudeToolEvent:
            if (shiftDirection <= 50) {
               Debug.Log("Here - crude");
               TryDecreaseCrudeToolSellValue(MAX_CRUDE_TOOL_VALUE / 2);
            }
            else {
               Debug.Log("Here + crude");
               TryIncreaseCrudeToolSellValue(MAX_CRUDE_TOOL_VALUE / 2);
            }
            break;
         case (int)WorldEventTypes.HarpoonEvent:
            if (shiftDirection <= 50) {
               Debug.Log("Here - harpoon");
               TryDecreaseHarpoonSellValue(MAX_HARPOON_VALUE / 2);
            }
            else {
               Debug.Log("Here + harpoon");
               TryIncreaseHarpoonSellValue(MAX_HARPOON_VALUE / 2);
            }
            break;
         case (int)WorldEventTypes.PressureValveEvent:
            if (shiftDirection <= 50)
               TryDecreasePressureValveValue(MAX_PRESSURE_VALVE_VALUE / 2);
            else
               TryIncreasePressureValveValue(MAX_PRESSURE_VALVE_VALUE / 2);
            break;
         case (int)WorldEventTypes.ClockworkEngineEvent:
            if (shiftDirection <= 50)
               TryDecreaseEnginesSellValue(MAX_ENGINE_VALUE / 2);
            else
               TryIncreaseEnginesSellValue(MAX_ENGINE_VALUE / 2);
            break;
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
            currentItem = Items.Find(d => d.CompareTag(CRUDE_TOOL_TAG));
            break;
         case ItemType.Harpoon:
            currentItem = Items.Find(d => d.CompareTag(HARPOON_TAG));
            break;
         case ItemType.PressureValve:
            currentItem = Items.Find(d => d.CompareTag(PRESSURE_VALVE_TAG));
            break;
         case ItemType.Engine:
            currentItem = Items.Find(d => d.CompareTag(ENGINE_TAG));
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

   /* Handles the main button clicks (Trade, Info, Upgrade) to open the corresponding panel           */
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
         case UPGRADE_BUTTON:
            ShowUpgradePanel();
            UpgradePanel.Find("YesButton").GetComponent<Button>().onClick.AddListener(() => UpgradeTradeHut());
            UpgradePanel.Find("CancelButton").GetComponent<Button>().onClick.AddListener(() => CloseTradeHutPanel(UPGRADE_BUTTON));
            break;
         default:
            Debug.Log("Building Panel: Unknown button ID.");
            break;
      }
   }

   /* Increments the trade hut level                                                                  */
   private void UpgradeTradeHut() 
   {
      if (tradeHutLevel < ENDING_LEVEL)
         tradeHutLevel += 1;
      else
         Debug.Log("Trade Hut is already at max level.");

      tradeHutLevelText.text = "Level " + tradeHutLevel.ToString();

      UpgradePanel.transform.Find("YesButton").GetComponent<Button>().onClick.RemoveAllListeners();
      UpgradePanel.transform.Find("CancelButton").GetComponent<Button>().onClick.RemoveAllListeners();

      CloseUpgradePanel();
      PopUpManager.Instance.EnablePlayerInput();
   }

   /* Closes the panel corresponding to the button ID                                                 */
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

   private void ShowUpgradePanel() 
   {
      UpgradePanel.gameObject.SetActive(true);
   }

   public void ShowSellPanel() 
   {
      if (BuyPanel.gameObject.activeSelf) 
      {
         if (BuyWindow.gameObject.activeSelf)
            CloseBuyWindow();

         CloseBuyPanel();
      }

      /* Destroy the instantiated buy item/window instance if it exists                               */
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

      /* Destroy the instantiated sell item/window instance if it exists                              */
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

   public void ShowMysteryBoxPanel() 
   {
      MysteryBoxPanel.gameObject.SetActive(true);
      MysteryBoxPanel.Find("StartingView").gameObject.SetActive(true);
   }

   private void CloseTradePanel() 
   {
      TradePanels.gameObject.SetActive(false);

      /* Destroy the instantiated sell item/window instance if it exists                              */
      if (currentSellItem != null) 
      {
         Destroy(currentSellItem.gameObject);
         currentSellItem = null;
      }

      /* Destroy the instantiated buy item/window instance if it exists                               */
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

   public void CloseMysteryBoxPanel() 
   {
      if(currentMysteryBoxResult != null) 
      {
         Destroy(currentMysteryBoxResult.gameObject);
         currentMysteryBoxResult = null;
      }
      MysteryBoxPanel.gameObject.SetActive(false);
   }
}