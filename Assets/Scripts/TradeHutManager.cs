
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

using static Resources;

public class TradeHutManager : MonoBehaviour 
{
   /* Inspector variables                                                                             */
   [SerializeField] private Transform TradePanels;            
                    public  Transform BuyPanel;               
   [SerializeField] private Transform BuyItemContainer;       
   [SerializeField] private Transform BuyItemTemplate;        
   [SerializeField] private Transform BuyWindow;              
   [SerializeField] private Transform BuyWindowContainer;     
   [SerializeField] private Transform BuyWindowTemplate;      
   [SerializeField] private Transform SellPanel;              
   [SerializeField] private Transform SellItemContainer;              
   [SerializeField] private Transform SellItemTemplate;       
   [SerializeField] private Transform SellWindow;
   [SerializeField] private Transform SellWindowContainer;
   [SerializeField] private Transform SellWindowTemplate;     
   [SerializeField] private Transform InfoPanel;                   
   [SerializeField] private Transform UpgradePanel;           
   [SerializeField] private Transform MysteryBoxPanel;        

   public TextMeshProUGUI tradeHutLevelText;

   private readonly static System.Random Rng = new System.Random();


   /* Private variables                                                                               */
   private int artifactSellCount     = 0; 
   private int crudeToolSellCount    = 0; 
   private int refinedToolSellCount  = 0; 
   private int swordBuyCount         = 0;

   /* Transform                                                                                       */
   private Transform currentBuyItem;      
   private Transform currentSellItem;
   private Transform currentMysteryBoxResult;

   /* Constants                                                                                       */
   const int ENDING_LEVEL        = 5;     
   const int INFO_BUTTON         = 2;     
   const int MAX_BUY_ITEM_COUNT  = 100;   
   const int MAX_SELL_ITEM_COUNT = 100;   
   const int MIN_BUY_ITEM_COUNT  = 0;     
   const int MIN_SELL_ITEM_COUNT = 0;     
   const int STARTING_LEVEL      = 1;     
   const int TRADE_BUTTON        = 1;     
   const int BUY_ITEM_SPACING    = 30;    
   const int UPGRADE_BUTTON      = 3;     

   public static int tradeHutLevel;
   public static TradeHutManager Instance;

   private void Awake() 
   {
      tradeHutLevel = STARTING_LEVEL;

      if (Instance != null && Instance != this)
         Destroy(this.gameObject);
      else 
      {
         Instance = this;
         DontDestroyOnLoad(this.gameObject);
      }

      if (SellItemContainer == null)
         Debug.LogError("Container is not assigned in the Inspector!");

      if (SellItemTemplate != null)
         SellItemTemplate.gameObject.SetActive(false);

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

      if (BuyItemContainer == null)
         Debug.LogError("Buy Container is not assigned in the Inspector!");

      if (BuyWindow == null)
         Debug.LogError("Buy Window is not assigned in the Inspector!");
      else
         BuyWindow.gameObject.SetActive(false);

      if (BuyItemTemplate == null)
         Debug.LogError("Buy Item Template is not assigned in the Inspector!");
      else
         BuyItemTemplate.gameObject.SetActive(false);

      if(MysteryBoxPanel == null)
         Debug.LogError("Mystery Box Panel is not assigned in the Inspector!");
      else
         MysteryBoxPanel.gameObject.SetActive(false);
   }

   private void Start() 
   {
      CreateSellItem(Item.GetItemSprite(Item.ItemType.CrudeTool), Item.GetItemValue(Item.ItemType.CrudeTool), -1.0f, "Crude Tool");
      CreateSellItem(Item.GetItemSprite(Item.ItemType.RefinedTool), Item.GetItemValue(Item.ItemType.RefinedTool), 0.0f, "Refined Tool");
      CreateSellItem(Item.GetItemSprite(Item.ItemType.Artifact), Item.GetItemValue(Item.ItemType.Artifact), 1.0f, "Artifact");

      CreateBuyItem(Item.GetItemSprite(Item.ItemType.Sword), Item.GetItemPrice(Item.ItemType.Sword), 0, "Sword");
   }

   private void CreateSellItem(Sprite itemSprite, int itemValue, float positionIndex, string itemTag) 
   {
      /* Instantiate the template and set its position in the container                               */
      Transform     tradeItemTransform     = Instantiate(SellItemTemplate, SellItemContainer);
      RectTransform tradeItemRectTransform = tradeItemTransform.GetComponent<RectTransform>();

      tradeItemTransform.tag = itemTag;
      tradeItemRectTransform.anchoredPosition = new Vector2(BUY_ITEM_SPACING * positionIndex, 0);

      /* Populate the the item properties                                                             */
      tradeItemTransform.Find("ItemValue").GetComponent<TextMeshProUGUI>().text = itemValue.ToString();
      tradeItemTransform.Find("ItemName").GetComponent<TextMeshProUGUI>().text  = itemTag.Equals("Artifact") ? "   Artifact" : itemTag;
      Button itemButton       = tradeItemTransform.Find("ItemButton").GetComponent<Button>();
      itemButton.image.sprite = itemSprite;

      /* Dynamically add a listener to the button, which creates a sell window when clicked           */
      itemButton.onClick.AddListener(() => CreateSellWindow(itemSprite, Resources.GetResourceSprite(Resources.ResourceType.Pearl), itemValue, itemTag));

      tradeItemTransform.gameObject.SetActive(true);
   }

   private void CreateBuyItem(Sprite itemSprite, int itemValue, float positionIndex, string itemTag) 
   {
      /* Instantiate the template and set its position in the container                               */
      Transform     tradeItemTransform     = Instantiate(BuyItemTemplate, BuyItemContainer);
      RectTransform tradeItemRectTransform = tradeItemTransform.GetComponent<RectTransform>();

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
      int totalSellValue = 0;

      if (crudeToolSellCount > MIN_SELL_ITEM_COUNT)
         totalSellValue += crudeToolSellCount * Item.GetItemValue(Item.ItemType.CrudeTool);

      if (refinedToolSellCount > MIN_SELL_ITEM_COUNT)
         totalSellValue += refinedToolSellCount * Item.GetItemValue(Item.ItemType.RefinedTool);

      if (artifactSellCount > MIN_SELL_ITEM_COUNT)
         totalSellValue += artifactSellCount * Item.GetItemValue(Item.ItemType.Artifact);

      crudeToolSellCount   = MIN_SELL_ITEM_COUNT;
      refinedToolSellCount = MIN_SELL_ITEM_COUNT;
      artifactSellCount    = MIN_SELL_ITEM_COUNT;

      /* Destroy the instantiated sell window and remove the reference                                */
      Destroy(currentSellItem.gameObject);
      currentSellItem = null;

      InventoryManager.Instance.TryAddPearl(totalSellValue);

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
            if (refinedToolSellCount < MAX_SELL_ITEM_COUNT) 
            {
               refinedToolSellCount += 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text      = "   " + refinedToolSellCount.ToString();
               item.Find("currencyGained").GetComponent<TextMeshProUGUI>().text = (refinedToolSellCount * Item.GetItemValue(Item.ItemType.RefinedTool)).ToString();
            }
            break;
         case "Artifact":
            if (artifactSellCount < MAX_SELL_ITEM_COUNT) 
            {
               artifactSellCount += 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text      = "   " + artifactSellCount.ToString();
               item.Find("currencyGained").GetComponent<TextMeshProUGUI>().text = (artifactSellCount * Item.GetItemValue(Item.ItemType.Artifact)).ToString();
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
            if (refinedToolSellCount > MIN_SELL_ITEM_COUNT) 
            {
               refinedToolSellCount -= 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "   " + refinedToolSellCount.ToString();
               item.Find("currencyGained").GetComponent<TextMeshProUGUI>().text = (crudeToolSellCount * Item.GetItemValue(Item.ItemType.RefinedTool)).ToString();
            }
            break;
         case "Artifact":
            if (artifactSellCount > MIN_SELL_ITEM_COUNT) 
            {
               artifactSellCount -= 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "   " + artifactSellCount.ToString();
               item.Find("currencyGained").GetComponent<TextMeshProUGUI>().text = (artifactSellCount * Item.GetItemValue(Item.ItemType.Artifact)).ToString();
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
      refinedToolSellCount = MIN_SELL_ITEM_COUNT;
      artifactSellCount    = MIN_SELL_ITEM_COUNT;
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