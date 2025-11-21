
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
   [SerializeField] private Transform BuyWindowContainer;     
   [SerializeField] private Transform BuyWindowTemplate;      
   [SerializeField] private Transform SellPanel;              
   [SerializeField] private Transform SellWindow;
   [SerializeField] private Transform SellWindowContainer;
   [SerializeField] private Transform SellWindowTemplate;     
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
   private TextMeshProUGUI crudeToolValueText,
                           weaponValueText,
                           engineValueText;

   /* Private variables                                                                               */
   private int crudeToolSellCount = 0,
               weaponSellCount    = 0, 
               engineSellCount    = 0, 
               swordBuyCount      = 0;

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

      if (SellWindowTemplate == null)
         Debug.LogError("Sell window template is not assigned in the Inspector");
      else
         SellWindowTemplate.gameObject.SetActive(false);

      if (SellPanel == null)
         Debug.LogError("Sell Panel is not assigned in the Inspector!");
      else
         CloseSellPanel();

      if (BuyPanel == null)
         Debug.LogError("Buy Panel is not assigned in the Inspector!");
      else
         CloseBuyPanel();

      if (BuyWindowTemplate == null)
         Debug.LogError("Buy Item Template is not assigned in the Inspector!");
      else
         BuyWindowTemplate.gameObject.SetActive(false);

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
      CreateSellItem(Item.GetItemSprite(Item.ItemType.CrudeTool), Item.GetItemValue(Item.ItemType.CrudeTool), -1.0f, "Crude Tool");
      CreateSellItem(Item.GetItemSprite(Item.ItemType.Harpoon), Item.GetItemValue(Item.ItemType.Harpoon), 0.0f, "Harpoon");
      CreateSellItem(Item.GetItemSprite(Item.ItemType.Engine), Item.GetItemValue(Item.ItemType.Engine), 1.0f, "Engine");

      CreateBuyItem(Item.GetItemSprite(Item.ItemType.Sword), Item.GetItemPrice(Item.ItemType.Sword), 0, "Sword");
   }

   private void CreateSellItem(Sprite itemSprite, int itemValue, float positionIndex, string itemTag) 
   {
      Transform       SellItemContainer = SellPanel.Find("sellItemContainer").GetComponent<Transform>(),
                      SellItemTemplate  = SellItemContainer.Find("SellItemTemplate").GetComponent<Transform>(),
                      tradeItemTransform;
      RectTransform   tradeItemRectTransform;
      Button          itemButton;
      TextMeshProUGUI sellValueText;

      SellItemTemplate.gameObject.SetActive(false);

      /* Instantiate the template and set its position in the container                               */
      tradeItemTransform     = Instantiate(SellItemTemplate, SellItemContainer);
      tradeItemRectTransform = tradeItemTransform.GetComponent<RectTransform>();

      tradeItemTransform.tag = itemTag;
      tradeItemRectTransform.anchoredPosition = new Vector2(BUY_ITEM_SPACING * positionIndex, 0);

      //* Populate the the item properties                                                           */
      //switch(tradeItemTransform.tag) {
      //   case "Crude Tool":
      //      crudeToolValueText = tradeItemTransform.Find("ItemValue").GetComponent<TextMeshProUGUI>();
      //   break;

      //}
      sellValueText = tradeItemTransform.Find("ItemValue").GetComponent<TextMeshProUGUI>();
      sellValueText.text = itemValue.ToString();
      Items.Add(tradeItemTransform);

      tradeItemTransform.Find("ItemName").GetComponent<TextMeshProUGUI>().text  = itemTag.Equals("Artifact") ? "   Artifact" : itemTag;
      itemButton       = tradeItemTransform.Find("ItemButton").GetComponent<Button>();
      itemButton.image.sprite = itemSprite;

      /* Dynamically add a listener to the button, which creates a sell window when clicked           */
      itemButton.onClick.AddListener(() => CreateSellWindow(itemSprite, Resources.GetResourceSprite(Resources.ResourceType.Pearl), itemValue, itemTag));

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
      itemButton.onClick.AddListener(() => CreateBuyWindow(itemSprite, Resources.GetResourceSprite(Resources.ResourceType.Pearl), itemValue, itemTag));

      tradeItemTransform.gameObject.SetActive(true);
   }

   /* Creates and populates the single buy transaction window                                         */
   private void CreateBuyWindow(Sprite itemSprite, Sprite currencySprite, int itemValue, string itemTag) 
   {
      int itemCount = 0;

      /* Destroy the previously opened buy window instance before creating a new one                  */
      if (currentBuyItem != null) 
      {
         Destroy(currentBuyItem.gameObject);
         currentBuyItem = null;
      }

      Transform     buyItemTransfrom              = Instantiate(BuyWindowTemplate, BuyWindowContainer);
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

   /* Creates and populates the single sell transaction window                                        */
   private void CreateSellWindow(Sprite itemSprite, Sprite currencySprite, int itemValue, string itemTag) 
   {
      int itemCount = 0;

      /* Destroy the previously opened sell window instance before creating a new one                 */
      if (currentSellItem != null) 
      {
         Destroy(currentSellItem.gameObject);
         currentSellItem = null;
      }

      Transform     sellItemTransform     = Instantiate(SellWindowTemplate, SellWindowContainer);
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

   public void SellItem() 
   {
      int totalSellValue  = 0;

      if (crudeToolSellCount > MIN_SELL_ITEM_COUNT) 
      {
         totalSellValue += crudeToolSellCount * Item.GetItemValue(Item.ItemType.CrudeTool);
         InventoryManager.Instance.TryUseCrudeTool(crudeToolSellCount);
      }

      if (weaponSellCount > MIN_SELL_ITEM_COUNT) 
      {
         totalSellValue += weaponSellCount * Item.GetItemValue(Item.ItemType.Harpoon);
         InventoryManager.Instance.TryUseHarpoon(weaponSellCount);
      }

      if (engineSellCount > MIN_SELL_ITEM_COUNT) 
      {
         totalSellValue += engineSellCount * Item.GetItemValue(Item.ItemType.Engine);
         InventoryManager.Instance.TryUseEngine(engineSellCount);
      }

      InventoryManager.Instance.TryAddPearl(totalSellValue);

      crudeToolSellCount = MIN_SELL_ITEM_COUNT;
      weaponSellCount    = MIN_SELL_ITEM_COUNT;
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
      if (swordBuyCount > MIN_BUY_ITEM_COUNT)
         InventoryManager.Instance.TrySpendPearl(swordBuyCount * Item.GetItemValue(Item.ItemType.Sword));

      swordBuyCount = MIN_BUY_ITEM_COUNT;

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
         case "Crude Tool":
            if (crudeToolSellCount < MAX_SELL_ITEM_COUNT) 
            {
               crudeToolSellCount += 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text      = "   " + crudeToolSellCount.ToString();
               item.Find("currencyGained").GetComponent<TextMeshProUGUI>().text = (crudeToolSellCount * Item.GetItemValue(Item.ItemType.CrudeTool)).ToString();
            }
            break;
         case "Refined Tool":
            if (weaponSellCount < MAX_SELL_ITEM_COUNT) 
            {
               weaponSellCount += 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text      = "   " + weaponSellCount.ToString();
               item.Find("currencyGained").GetComponent<TextMeshProUGUI>().text = (weaponSellCount * Item.GetItemValue(Item.ItemType.Harpoon)).ToString();
            }
            break;
         case "Artifact":
            if (engineSellCount < MAX_SELL_ITEM_COUNT) 
            {
               engineSellCount += 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text      = "   " + engineSellCount.ToString();
               item.Find("currencyGained").GetComponent<TextMeshProUGUI>().text = (engineSellCount * Item.GetItemValue(Item.ItemType.Engine)).ToString();
            }
            break;
         default:
            Debug.Log("Unknown item tag: " + item.tag);
            break;
      }
   }

   /* Decrements the count for the item being sold and updates the UI                                 */
   public void DecreaseSellItemCount(Transform item) 
   {
      switch (item.tag) {
         case "Crude Tool":
            if (crudeToolSellCount > MIN_SELL_ITEM_COUNT) 
            {
               crudeToolSellCount -= 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "   " + crudeToolSellCount.ToString();
               item.Find("currencyGained").GetComponent<TextMeshProUGUI>().text = (crudeToolSellCount * Item.GetItemValue(Item.ItemType.CrudeTool)).ToString();
            }
            break;
         case "Refined Tool":
            if (weaponSellCount > MIN_SELL_ITEM_COUNT) 
            {
               weaponSellCount -= 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "   " + weaponSellCount.ToString();
               item.Find("currencyGained").GetComponent<TextMeshProUGUI>().text = (crudeToolSellCount * Item.GetItemValue(Item.ItemType.Harpoon)).ToString();
            }
            break;
         case "Artifact":
            if (engineSellCount > MIN_SELL_ITEM_COUNT) 
            {
               engineSellCount -= 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "   " + engineSellCount.ToString();
               item.Find("currencyGained").GetComponent<TextMeshProUGUI>().text = (engineSellCount * Item.GetItemValue(Item.ItemType.Engine)).ToString();
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
         case "Sword":
            if (swordBuyCount < MAX_BUY_ITEM_COUNT) 
            {
               swordBuyCount += 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text     = "   " + swordBuyCount.ToString();
               item.Find("currencySpent").GetComponent<TextMeshProUGUI>().text = (swordBuyCount * Item.GetItemPrice(Item.ItemType.Sword)).ToString();
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
         case "Sword":
            if (swordBuyCount > MIN_BUY_ITEM_COUNT) 
            {
               swordBuyCount -= 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text     = "   " + swordBuyCount.ToString();
               item.Find("currencySpent").GetComponent<TextMeshProUGUI>().text = (swordBuyCount * Item.GetItemPrice(Item.ItemType.Sword)).ToString();
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

      ResultTransform.Find("CurrencyIcon").GetComponent<Image>().sprite = Resources.GetResourceSprite(resource);
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
      int crudeToolValue      = Item.GetItemValue(Item.ItemType.CrudeTool),
          harpoonValue        = Item.GetItemValue(Item.ItemType.Harpoon),
          pressureValveValue  = Item.GetItemValue(Item.ItemType.PressureValve),
          engineValue         = Item.GetItemValue(Item.ItemType.Engine),
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
         Item.TryIncreaseCrudeToolSellValue(pearlAmount);
      }
      else 
         if(crudeToolChance <= 60) 
         {
            //fluctuationPercent =  (.01f *  (float) Math.Round((double) Rng.Next(50, 101)));
            //pearlAmount = (int) (crudeToolValue * fluctuationPercent);
            pearlAmount = Rng.Next(marketShiftMin, marketShiftMax + 1);
            Debug.Log("Pearl Amount +" + pearlAmount);
            Item.TryDecreaseCrudeToolSellValue(pearlAmount);
         }

      if (harpoonChance <= 30) 
      {
         //fluctuationPercent =  (.01f *  (float) Math.Round((double) Rng.Next(50, 101)));
         //pearlAmount = (int) (weaponValue * fluctuationPercent);
         pearlAmount = Rng.Next(marketShiftMin, marketShiftMax + 1);
         Debug.Log("Pearl Amount +" + pearlAmount);
         Item.TryIncreaseWeaponsSellValue(pearlAmount);
      }
      else 
         if(harpoonChance <= 60) 
         {
            //fluctuationPercent =  (.01f *  (float) Math.Round((double) Rng.Next(50, 101)));
            //pearlAmount = (int) (weaponValue * fluctuationPercent);
            pearlAmount = Rng.Next(marketShiftMin, marketShiftMax + 1);
            Debug.Log("Pearl Amount -" + pearlAmount);
            Item.TryDecreaseWeaponsSellValue(pearlAmount);
         }

      if (pressureValveChance <= 30) 
      {
         //fluctuationPercent =  (.01f *  (float) Math.Round((double) Rng.Next(50, 101)));
         //pearlAmount = (int) (weaponValue * fluctuationPercent);
         pearlAmount = Rng.Next(marketShiftMin, marketShiftMax + 1);
         Debug.Log("Pearl Amount +" + pearlAmount);
         Item.TryIncreaseWeaponsSellValue(pearlAmount);
      }
      else 
         if(pressureValveChance <= 60) 
         {
            //fluctuationPercent =  (.01f *  (float) Math.Round((double) Rng.Next(50, 101)));
            //pearlAmount = (int) (weaponValue * fluctuationPercent);
            pearlAmount = Rng.Next(marketShiftMin, marketShiftMax + 1);
            Debug.Log("Pearl Amount -" + pearlAmount);
            Item.TryDecreaseWeaponsSellValue(pearlAmount);
         }

      if (engineChance <= 30) 
      {
         //fluctuationPercent = (.01f * (float)Math.Round((float)Rng.Next(50, 101)));
         //pearlAmount = (int) (engineValue * fluctuationPercent);
         pearlAmount = Rng.Next(marketShiftMin, marketShiftMax + 1);
         Debug.Log("Pearl Amount +" + pearlAmount);
         Item.TryIncreaseEnginesSellValue(pearlAmount);
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
      weaponSellCount = MIN_SELL_ITEM_COUNT;
      engineSellCount    = MIN_SELL_ITEM_COUNT;
      swordBuyCount        = MIN_BUY_ITEM_COUNT;

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