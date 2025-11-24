
using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;

using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.UI;

using static Item;
using static Resources;

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
   public TextMeshProUGUI tradeHutLevelText;

   private readonly static System.Random Rng = new System.Random();

   /* Transforms                                                                                      */
   private Transform       currentBuyItem,    
                           currentSellItem,
                           currentMysteryBoxResult;

   /* Private variables                                                                               */
   private int crudeToolSellCount = 0,
               harpoonSellCount   = 0, 
               pressureValveCount = 0, 
               engineSellCount    = 0, 
               rareOreCount      = 0;

   /* Public variables                                                                                */
   public int marketShiftMax = 2,
              marketShiftMin = 1;

   /* Constants                                                                                       */
   const int ENDING_LEVEL        = 5,  
             INFO_BUTTON         = 2,     
             MAX_BUY_ITEM_COUNT  = 100,   
             MAX_SELL_ITEM_COUNT = 100,   
             MIN_BUY_ITEM_COUNT  = 0,   
             MIN_SELL_ITEM_COUNT = 0,     
             STARTING_LEVEL      = 1,     
             TRADE_BUTTON        = 1,     
             BUY_ITEM_SPACING    = 30,    
             UPGRADE_BUTTON      = 3;

   const string CRUDE_TOOL_TAG            = "Crude Tool",
                HARPOON_TAG               = "Harpoon",
                PRESSURE_VALVE_TAG        = "Pressure Valve",
                ENGINE_TAG                = "Engine",
                RARE_ORE                  = "Rare Ore",
                INDUSTRIAL_BLUE_PRINT_TAG = "Industrial Blue Print",
                CLOCKWORK_BLUEPRINT_TAG   = "Clockwork Blue Print";

   public static int tradeHutLevel;


   public static TradeHutManager Instance;

   private void Awake() 
   {
      tradeHutLevel = STARTING_LEVEL;
      Items = new();

      Item.OnItemValueChange = ChangeItemValueText;

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
      CreateSellItem(GetItemSprite(ItemType.CrudeTool),GetItemValue(ItemType.CrudeTool), -1.0f, CRUDE_TOOL_TAG);
      CreateSellItem(GetItemSprite(ItemType.Harpoon), GetItemValue(ItemType.Harpoon), 0.0f, HARPOON_TAG);
      CreateSellItem(GetItemSprite(ItemType.PressureValve), GetItemValue(ItemType.PressureValve), 1.0f, PRESSURE_VALVE_TAG);
      CreateSellItem(GetItemSprite(ItemType.Engine), GetItemValue(ItemType.Engine), 2.0f, ENGINE_TAG);

      CreateBuyItem(GetItemSprite(ItemType.RareOre), GetItemPrice(ItemType.RareOre), 0.0f, RARE_ORE);
      CreateBuyItem(GetItemSprite(ItemType.IndustrialBluePrint), GetItemPrice(ItemType.IndustrialBluePrint), 1.0f, INDUSTRIAL_BLUE_PRINT_TAG);
      CreateBuyItem(GetItemSprite(ItemType.ClockworkBlueprint), GetItemPrice(ItemType.ClockworkBlueprint), 2.0f, CLOCKWORK_BLUEPRINT_TAG);
   }

   private void CreateSellItem(Sprite itemSprite, int itemValue, float positionIndex, string itemTag) 
   {
      Transform       SellItemContainer = SellPanel.Find("sellItemContainer").GetComponent<Transform>(),
                      SellItemTemplate  = SellItemContainer.Find("SellItemTemplate").GetComponent<Transform>(),
                      tradeItemTransform;
      TextMeshProUGUI sellValueText;
      RectTransform   tradeItemRectTransform;
      Button          itemButton;

      SellItemTemplate.gameObject.SetActive(false);

      /* Instantiate the template and set its position in the container                               */
      tradeItemTransform     = Instantiate(SellItemTemplate, SellItemContainer);
      tradeItemRectTransform = tradeItemTransform.GetComponent<RectTransform>();

      tradeItemTransform.tag = itemTag;
      tradeItemRectTransform.anchoredPosition = new Vector2(BUY_ITEM_SPACING * positionIndex, 0);

      /* Populate the the item properties                                                           */
      sellValueText = tradeItemTransform.Find("ItemValue").GetComponent<TextMeshProUGUI>();
      sellValueText.text = itemValue.ToString();
      Items.Add(tradeItemTransform);

      tradeItemTransform.Find("ItemName").GetComponent<TextMeshProUGUI>().text  = itemTag.Equals(ENGINE_TAG) ? "   " + ENGINE_TAG : itemTag;
      itemButton = tradeItemTransform.Find("ItemButton").GetComponent<Button>();
      itemButton.image.sprite = itemSprite;

      /* Dynamically add a listener to the button, which creates a sell window when clicked           */
      itemButton.onClick.AddListener(() => CreateSellWindow(itemSprite, GetResourceSprite(ResourceType.Pearl), itemValue, itemTag));

      tradeItemTransform.gameObject.SetActive(true);
   }

   private void CreateBuyItem(Sprite itemSprite, int itemValue, float positionIndex, string itemTag) 
   {
      Transform     BuyItemContainer = BuyPanel.Find("BuyItemContainer").GetComponent<Transform>(),
                    BuyItemTemplate  = BuyItemContainer.Find("BuyItemTemplate").GetComponent<Transform>(),
                    tradeItemTransform;
      RectTransform tradeItemRectTransform;

      /* Instantiate the template and set its position in the container                               */
      tradeItemTransform     = Instantiate(BuyItemTemplate, BuyItemContainer);
      tradeItemRectTransform = tradeItemTransform.GetComponent<RectTransform>();

      BuyItemTemplate.gameObject.SetActive(false);

      tradeItemTransform.tag = itemTag;

      tradeItemRectTransform.anchoredPosition = new Vector2(BUY_ITEM_SPACING * positionIndex, 0);     

      /* Populate the item properties                                                                 */
      tradeItemTransform.Find("ItemValue").GetComponent<TextMeshProUGUI>().text = itemValue.ToString();
      Button itemButton = tradeItemTransform.Find("ItemButton").GetComponent<Button>();

      itemButton.image.sprite = itemSprite;

      /* Dynamically add a listener to the button, which creates the buy window                       */
      itemButton.onClick.AddListener(() => CreateBuyWindow(itemSprite, GetResourceSprite(ResourceType.Pearl), itemValue, itemTag));

      tradeItemTransform.gameObject.SetActive(true);
   }

   /* Creates and populates the single buy transaction window                                         */
   /* Creates and populates the single sell transaction window                                        */
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
         if(InventoryManager.Instance.TryUseCrudeTool(crudeToolSellCount))
            totalSellValue += crudeToolSellCount * GetItemValue(ItemType.CrudeTool);
         else
            crudeToolSellCount = MIN_SELL_ITEM_COUNT;
      }

      if (harpoonSellCount > MIN_SELL_ITEM_COUNT) 
      {
         if(InventoryManager.Instance.TryUseHarpoon(harpoonSellCount))
            totalSellValue += harpoonSellCount * GetItemValue(ItemType.Harpoon);
         else
            harpoonSellCount = MIN_SELL_ITEM_COUNT;
      }

      if (pressureValveCount > MIN_SELL_ITEM_COUNT) 
      {
         if(InventoryManager.Instance.TryUseHarpoon(pressureValveCount))
            totalSellValue += pressureValveCount * GetItemValue(ItemType.PressureValve);
         else
            pressureValveCount = MIN_SELL_ITEM_COUNT;
      }

      if (engineSellCount > MIN_SELL_ITEM_COUNT) 
      {
         if(InventoryManager.Instance.TryUseEngine(engineSellCount))
            totalSellValue += engineSellCount * GetItemValue(ItemType.Engine);
         else
            engineSellCount = MIN_SELL_ITEM_COUNT;
      }

      InventoryManager.Instance.TryAddPearl(totalSellValue);

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
      if (rareOreCount > MIN_BUY_ITEM_COUNT)
         InventoryManager.Instance.TrySpendPearl(rareOreCount * GetItemValue(ItemType.RareOre));

      rareOreCount = MIN_BUY_ITEM_COUNT;

      Destroy(currentBuyItem.gameObject);
      currentBuyItem = null;

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

   /* Decrements the count for the item being sold and updates the UI                                 */
   public void DecreaseSellItemCount(Transform item) 
   {
      switch (item.tag) {
         case CRUDE_TOOL_TAG:
            if (crudeToolSellCount > MIN_SELL_ITEM_COUNT) 
            {
               crudeToolSellCount -= 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "   " + crudeToolSellCount.ToString();
               item.Find("currencyGained").GetComponent<TextMeshProUGUI>().text = (crudeToolSellCount * GetItemValue(ItemType.CrudeTool)).ToString();
            }
            break;
         case HARPOON_TAG:
            if (harpoonSellCount > MIN_SELL_ITEM_COUNT) 
            {
               harpoonSellCount -= 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "   " + harpoonSellCount.ToString();
               item.Find("currencyGained").GetComponent<TextMeshProUGUI>().text = (crudeToolSellCount * GetItemValue(ItemType.Harpoon)).ToString();
            }
            break;
         case PRESSURE_VALVE_TAG:
            if (pressureValveCount > MIN_SELL_ITEM_COUNT) 
            {
               pressureValveCount -= 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "   " + pressureValveCount.ToString();
               item.Find("currencyGained").GetComponent<TextMeshProUGUI>().text = (pressureValveCount * GetItemValue(ItemType.PressureValve)).ToString();
            }
            break;
         case ENGINE_TAG:
            if (engineSellCount > MIN_SELL_ITEM_COUNT) 
            {
               engineSellCount -= 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "   " + engineSellCount.ToString();
               item.Find("currencyGained").GetComponent<TextMeshProUGUI>().text = (engineSellCount * GetItemValue(ItemType.Engine)).ToString();
            }
            break;
         default:
            Debug.Log("Unknown item tag: " + item.tag);
            break;
      }
   }

   /* Increments the count for the item being bought and updates the UI                               */
   public void IncreaseBuyItemsCount(Transform item) {
      switch (item.tag) 
      {
         case RARE_ORE:
            if (rareOreCount < MAX_BUY_ITEM_COUNT) 
            {
               rareOreCount += 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text     = "   " + rareOreCount.ToString();
               item.Find("currencySpent").GetComponent<TextMeshProUGUI>().text = (rareOreCount * GetItemPrice(ItemType.RareOre)).ToString();
            }
            break;
         default:
            Debug.Log("Unknown item tag: " + item.tag);
            break;
      }
   }

   /* Decrements the count for the item being bought and updates the UI                               */
   public void DecreaseBuyItemsCount(Transform item) 
   {
      switch (item.tag) 
      {
         case RARE_ORE:
            if (rareOreCount > MIN_BUY_ITEM_COUNT) 
            {
               rareOreCount -= 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text     = "   " + rareOreCount.ToString();
               item.Find("currencySpent").GetComponent<TextMeshProUGUI>().text = (rareOreCount * GetItemPrice(ItemType.RareOre)).ToString();
            }
            break;
         default:
            Debug.Log("Unknown item tag: " + item.tag);
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

   public void MarketFluctuate() {
      //float fluctuationPercent;
      int crudeToolValue      = GetItemValue(ItemType.CrudeTool),
          harpoonValue        = GetItemValue(ItemType.Harpoon),
          pressureValveValue  = GetItemValue(ItemType.PressureValve),
          engineValue         = GetItemValue(ItemType.Engine),
          crudeToolChance     = Rng.Next(1, 101),
          harpoonChance       = Rng.Next(1, 101),
          pressureValveChance = Rng.Next(1, 101),
          engineChance        = Rng.Next(1, 101),
          pearlAmount;

      if (crudeToolChance <= 30) 
      {
         
         //fluctuationPercent =  (.01f * Rng.Next(50, 101));
         //pearlAmount = (int) (crudeToolValue * fluctuationPercent);
         pearlAmount = Rng.Next(marketShiftMin, marketShiftMax + 1);
         Debug.Log("Pearl Amount +" + pearlAmount);
         TryIncreaseCrudeToolSellValue(pearlAmount);
      }
      else 
         if(crudeToolChance <= 60) 
         {
            //fluctuationPercent =  (.01f *  (float) Math.Round((double) Rng.Next(50, 101)));
            //pearlAmount = (int) (crudeToolValue * fluctuationPercent);
            pearlAmount = Rng.Next(marketShiftMin, marketShiftMax + 1);
            Debug.Log("Pearl Amount +" + pearlAmount);
            TryDecreaseCrudeToolSellValue(pearlAmount);
         }

      if (harpoonChance <= 30) 
      {
         //fluctuationPercent =  (.01f *  (float) Math.Round((double) Rng.Next(50, 101)));
         //pearlAmount = (int) (weaponValue * fluctuationPercent);
         pearlAmount = Rng.Next(marketShiftMin, marketShiftMax + 1);
         Debug.Log("Pearl Amount +" + pearlAmount);
         TryIncreaseWeaponsSellValue(pearlAmount);
      }
      else 
         if(harpoonChance <= 60) 
         {
            //fluctuationPercent =  (.01f *  (float) Math.Round((double) Rng.Next(50, 101)));
            //pearlAmount = (int) (weaponValue * fluctuationPercent);
            pearlAmount = Rng.Next(marketShiftMin, marketShiftMax + 1);
            Debug.Log("Pearl Amount -" + pearlAmount);
            TryDecreaseWeaponsSellValue(pearlAmount);
         }

      if (pressureValveChance <= 30) 
      {
         //fluctuationPercent =  (.01f *  (float) Math.Round((double) Rng.Next(50, 101)));
         //pearlAmount = (int) (weaponValue * fluctuationPercent);
         pearlAmount = Rng.Next(marketShiftMin, marketShiftMax + 1);
         Debug.Log("Pearl Amount +" + pearlAmount);
         TryIncreaseWeaponsSellValue(pearlAmount);
      }
      else 
         if(pressureValveChance <= 60) 
         {
            //fluctuationPercent =  (.01f *  (float) Math.Round((double) Rng.Next(50, 101)));
            //pearlAmount = (int) (weaponValue * fluctuationPercent);
            pearlAmount = Rng.Next(marketShiftMin, marketShiftMax + 1);
            Debug.Log("Pearl Amount -" + pearlAmount);
            TryDecreaseWeaponsSellValue(pearlAmount);
         }

      if (engineChance <= 30) 
      {
         //fluctuationPercent = (.01f * (float)Math.Round((float)Rng.Next(50, 101)));
         //pearlAmount = (int) (engineValue * fluctuationPercent);
         pearlAmount = Rng.Next(marketShiftMin, marketShiftMax + 1);
         Debug.Log("Pearl Amount +" + pearlAmount);
         TryIncreaseEnginesSellValue(pearlAmount);
      }
      else 
         if(engineChance <= 60) 
         {
            //fluctuationPercent = (.01f * (float)Math.Round((float)Rng.Next(50, 101)));
            //pearlAmount = (int) (engineValue * fluctuationPercent);
            pearlAmount = Rng.Next(marketShiftMin, marketShiftMax + 1);
            Debug.Log("Pearl Amount +" + pearlAmount);
            Item.TryDecreaseEnginesSellValue(pearlAmount);
         }

      return;
   }

   public void ChangeItemValueText(int newAmount, ItemType itemType) {
      Transform currentItem;

      switch (itemType) {
         case ItemType.CrudeTool:
            currentItem = Items.Find(d => d.CompareTag("Crude Tool"));
            break;
         case ItemType.Harpoon:
            currentItem = Items.Find(d => d.CompareTag("Weapon"));
            break;
         case ItemType.Engine:
            currentItem = Items.Find(d => d.CompareTag("Engine"));
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

      crudeToolSellCount   = MIN_SELL_ITEM_COUNT;
      harpoonSellCount = MIN_SELL_ITEM_COUNT;
      engineSellCount    = MIN_SELL_ITEM_COUNT;
      rareOreCount        = MIN_BUY_ITEM_COUNT;

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