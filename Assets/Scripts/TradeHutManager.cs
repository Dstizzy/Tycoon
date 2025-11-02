using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using static UnityEditor.Progress;

public class TradeHutManager : MonoBehaviour {
   public Transform tradeContainer;
   public Transform tradeItemTemplate;
   public Transform BuyItemContainer;
   public Transform BuyItemTemplate;
   public Transform BuyWindowContainer;
   public Transform BuyWindowTemplate;
   public Transform tradePanels;
   public Transform sellPanel;
   public Transform buyPanel;
   public Transform buyWindow;
   public Transform infoPanel;
   public Transform upgradePanel;
   public TextMeshProUGUI tradeHutLevelText;

   private int crudeToolSellCount = 0;
   private int refinedToolSellCount = 0;
   private int artifactSellCount = 0;
   private int swordBuyCount = 0;

   const int MIN_SELL_ITEM_COUNT = 0;
   const int MIN_BUY_ITEM_COUNT = 0;
   const int MAX_SELL_ITEM_COUNT = 100;
   const int MAX_BUY_ITEM_COUNT = 100;
   const int TRADE_ITEM_SPACING = 30;
   const int TRADE_BUTTON = 1;
   const int INFO_BUTTON = 2;
   const int UPGRADE_BUTTON = 3;
   const int STARTING_LEVEL = 1;
   const int ENDING_LEVEL = 5;

   public static int tradeHutLevel = STARTING_LEVEL;

   // A list to hold references to all the instantiated trade item UI elements.
   private List<Transform> sellItems;
   private List<Transform> buyItems;

   private void Awake() {
      sellItems = new();
      buyItems = new();

      if (tradeContainer == null) {
         Debug.LogError("Container is not assigned in the Inspector!");
      }

      // Hides the trade item template and the main trade panel when the game starts.
      if (tradeItemTemplate != null) {
         tradeItemTemplate.gameObject.SetActive(false);
      }
      if (tradePanels == null) {
         Debug.LogError("Trade Panel is not assigned in the Inspector!");
      } else {
         CloseTradePanel();
      }

      if (sellPanel == null)
         Debug.LogError("Sell Panel is not assigned in the Inspector!");
      else
         CloseSellPanel();

      if (buyPanel == null)
         Debug.LogError("Buy Panel is not assigned in the Inspector!");
      else
         CloseBuyPanel();

      if (infoPanel == null) {
         Debug.LogError("Info Panel is not assigned in the Inspector!");
      } else {
         CloseInfoPanel();
      }

      if (upgradePanel == null) {
         Debug.LogError("Upgrade Panel is not assigned in the Inspector!");
      } else {
         CloseUpgradePanel();
      }

      if (tradeHutLevelText != null) {
         tradeHutLevelText.text = "Level " + tradeHutLevel.ToString();
      } else {
         Debug.LogError("Trade Hut Level Text is not assigned in the Inspector!");
      }

      if (BuyWindowTemplate == null) {
         Debug.LogError("Buy Item Template is not assigned in the Inspector!");
      }
      else
         BuyWindowTemplate.gameObject.SetActive(false);


      if (BuyItemContainer == null) {
         Debug.LogError("Buy Container is not assigned in the Inspector!");
      }

      if (BuyItemContainer == null)
         Debug.LogError("Buy Container is not assigned in the Inspector!");

      if(buyWindow == null)
         Debug.LogError("Buy Window is not assigned in the Inspector!");
      else
         buyWindow.gameObject.SetActive(false);

      if(BuyItemTemplate == null)
         Debug.LogError("Buy Item Template is not assigned in the Inspector!");
      else
         BuyItemTemplate.gameObject.SetActive(false);
   }

   private void Start() {
      // Creates the individual trade item entries for Crude Tools, Refined Tools, and Artifacts
      CreateItem(Item.GetItemSprite(Item.ItemType.CrudeTool), "Crude Tool", Item.GetItemValue(Item.ItemType.CrudeTool), -1.0f, "Crude Tool");
      CreateItem(Item.GetItemSprite(Item.ItemType.RefinedTool), "Refined Tool", Item.GetItemValue(Item.ItemType.RefinedTool), 0.0f, "Refined Tool");
      CreateItem(Item.GetItemSprite(Item.ItemType.Artifact), "Artifact", Item.GetItemValue(Item.ItemType.Artifact), 1.0f, "Artifact");

      CreateBuyItem(Item.GetItemSprite(Item.ItemType.Sword), "Sword", Item.GetItemPrice(Item.ItemType.Sword), 0, "Sword");
   }

   // Instantiates a new trade item UI element, sets its visual data, and configures its buttons.
   private void CreateItem(Sprite itemSprite, string itemName, int itemvalue, float positionIndex, string ItemTag) {
      int itemCount = 0;

      // Instantiate the template and set its position in the container.
      Transform tradeItemTransform = Instantiate(tradeItemTemplate, tradeContainer);
      RectTransform tradeItemRectTransform = tradeItemTransform.GetComponent<RectTransform>();

      tradeItemTransform.tag = ItemTag;

      tradeItemRectTransform.anchoredPosition = new Vector2(TRADE_ITEM_SPACING * positionIndex, 0);

      // Populate the TextMeshPro and Image components with item-specific data (value, name, sprite).
      tradeItemTransform.Find("ItemValue").GetComponent<TextMeshProUGUI>().text = itemvalue.ToString();
      tradeItemTransform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = itemName.Equals("Artifact") ? "    Artifact" : itemName;
      tradeItemTransform.Find("ItemImage").GetComponent<Image>().sprite = itemSprite;
      tradeItemTransform.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "  " + itemCount.ToString();

      // Get references to the increase and decrease buttons.
      Button increaseButton = tradeItemTransform.Find("QuantityButtons/IncreaseButton").GetComponent<Button>();
      Button decreaseButton = tradeItemTransform.Find("QuantityButtons/DecreaseButton").GetComponent<Button>();

      // Dynamically add listeners to the buttons, passing the specific item's Transform.
      // This ensures the button click only affects its corresponding item entry.
      increaseButton.onClick.AddListener(() => OnClickIncreaseButton(tradeItemTransform));
      decreaseButton.onClick.AddListener(() => OnClickDecreaseButton(tradeItemTransform));

      tradeItemTransform.gameObject.SetActive(true);
      sellItems.Add(tradeItemTransform);
   }

   // Instantiates a new trade item UI element, sets its visual data, and configures its buttons.
   private void CreateBuyItem(Sprite itemSprite,  string itemName, int itemValue, float positionIndex, string itemTag) 
   {

      // Instantiate the template and set its position in the container.
      Transform tradeItemTransform = Instantiate(BuyItemTemplate, BuyItemContainer);
      RectTransform tradeItemRectTransform = tradeItemTransform.GetComponent<RectTransform>();

      tradeItemTransform.tag = itemTag;

      //tradeItemRectTransform.anchoredPosition = new Vector2(TRADE_ITEM_SPACING * positionIndex, 0);

      // Populate the TextMeshPro and Image components with item-specific data (value, name, sprite).
      tradeItemTransform.Find("ItemValue").GetComponent<TextMeshProUGUI>().text = itemValue.ToString();
      //tradeItemTransform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = itemName.Equals("Artifact") ? "    Artifact" : itemName;
      Button itemButton = tradeItemTransform.Find("ItemButton").GetComponent<Button>();

      itemButton.image.sprite = itemSprite;

      // Dynamically add listeners to the buttons, passing the specific item's Transform.
      // This ensures the button click only affects its corresponding item entry.
      itemButton.onClick.AddListener(() => CreateBuyWindow(itemSprite, Resources.Load<Sprite>("00-removebg-preview"), itemName, itemValue, itemTag));

      tradeItemTransform.gameObject.SetActive(true);
   }

   private void CreateBuyWindow(Sprite itemSprite, Sprite currencySprite, string itemName, int itemValue, string itemTag) {
      int itemCount = 0;

      Transform buyItemTransfrom = Instantiate(BuyWindowTemplate, BuyWindowContainer);

      buyItemTransfrom.tag = itemTag;

      RectTransform buyItemTransfromRectTransform = buyItemTransfrom.GetComponent<RectTransform>();

      buyItemTransfromRectTransform.anchoredPosition = new Vector2(TRADE_ITEM_SPACING * 0, 0);

      // Populate the TextMeshPro and Image components with item-specific data (value, name, sprite).
      buyItemTransfrom.Find("ItemImage").GetComponent<Image>().sprite = itemSprite;
      buyItemTransfrom.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "  " + itemCount.ToString();
      buyItemTransfrom.Find("currencyIcon").GetComponent<Image>().sprite = currencySprite;
      buyItemTransfrom.Find("currencySpent").GetComponent<TextMeshProUGUI>().text = "0";
      //tradeItemTransform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = itemName.Equals("Artifact") ? "    Artifact" : itemName;


      // Get references to the increase and decrease buttons.
      Button increaseButton = buyItemTransfrom.Find("QuantityButtons/IncreaseButton").GetComponent<Button>();
      Button decreaseButton = buyItemTransfrom.Find("QuantityButtons/DecreaseButton").GetComponent<Button>();

      // Dynamically add listeners to the buttons, passing the specific item's Transform.
      // This ensures the button click only affects its corresponding item entry.
      increaseButton.onClick.AddListener(() => OnClickIncreaseBuyItemsCount(buyItemTransfrom));
      decreaseButton.onClick.AddListener(() => OnClickDecreaseBuyItemsCount(buyItemTransfrom));

      buyItems.Add(buyItemTransfrom);
      buyItemTransfrom.gameObject.SetActive(true);
      ShowBuyWindow();
   }

   public void SellItem() {
      int totalSellValue = 0;

      if (crudeToolSellCount > MIN_SELL_ITEM_COUNT)
         totalSellValue += crudeToolSellCount * Item.GetItemValue(Item.ItemType.CrudeTool);

      if (refinedToolSellCount > MIN_SELL_ITEM_COUNT)
         totalSellValue += refinedToolSellCount * Item.GetItemValue(Item.ItemType.RefinedTool);

      if (artifactSellCount > MIN_SELL_ITEM_COUNT)
         totalSellValue += artifactSellCount * Item.GetItemValue(Item.ItemType.Artifact);

      crudeToolSellCount = MIN_SELL_ITEM_COUNT;
      refinedToolSellCount = MIN_SELL_ITEM_COUNT;
      artifactSellCount = MIN_SELL_ITEM_COUNT;

      foreach (Transform sellItem in sellItems)
         sellItem.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "  " + MIN_SELL_ITEM_COUNT.ToString();

      InventoryManager.Instance.TryAddPearl(totalSellValue);

      return;
   }
   public void BuyItem() 
   {
      if (swordBuyCount > MIN_BUY_ITEM_COUNT)
         InventoryManager.Instance.TrySpendPearl(swordBuyCount * Item.GetItemValue(Item.ItemType.Sword));

      swordBuyCount = MIN_BUY_ITEM_COUNT;

      foreach (Transform buyItem in buyItems)
         buyItem.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "  " + MIN_BUY_ITEM_COUNT.ToString();

      return;
   }

   public void OnClickIncreaseButton(Transform item) {
      // Uses the item's Tag to determine which counter variable to update.
      switch (item.tag) {
         case "Crude Tool":
            // Check against the maximum selling limit before incrementing.
            if (crudeToolSellCount < MAX_SELL_ITEM_COUNT) {
               crudeToolSellCount += 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "  " + crudeToolSellCount.ToString();
            }
            break;
         case "Refined Tool":
            if (refinedToolSellCount < MAX_SELL_ITEM_COUNT) {
               refinedToolSellCount += 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "  " + refinedToolSellCount.ToString();
            }
            break;
         case "Artifact":
            if (artifactSellCount < MAX_SELL_ITEM_COUNT) {
               artifactSellCount += 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "  " + artifactSellCount.ToString();
            }
            break;
         case "Sword":
            if (swordBuyCount < MAX_BUY_ITEM_COUNT) {
               swordBuyCount += 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "  " + swordBuyCount.ToString();
            }
            break;
         default:
            Debug.Log("Unknown item tag: " + item.tag);
            break;
      }
   }

   public void OnClickDecreaseButton(Transform Item) {
      // Uses the item's Tag to determine which counter variable to update.
      switch (Item.tag) {
         case "Crude Tool":
            // Check against the minimum selling limit (0) before decrementing.
            if (crudeToolSellCount > MIN_SELL_ITEM_COUNT) {
               crudeToolSellCount -= 1;
               Item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "  " + crudeToolSellCount.ToString();
            }
            break;
         case "Refined Tool":
            if (refinedToolSellCount > MIN_SELL_ITEM_COUNT) {
               refinedToolSellCount -= 1;
               Item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "  " + refinedToolSellCount.ToString();
            }
            break;
         case "Artifact":
            if (artifactSellCount > MIN_SELL_ITEM_COUNT) {
               artifactSellCount -= 1;
               Item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "  " + artifactSellCount.ToString();
            }
            break;
         case "Sword":
            if (swordBuyCount > MIN_SELL_ITEM_COUNT) {
               swordBuyCount -= 1;
               Item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "  " + swordBuyCount.ToString();
            }
            break;
         default:
            Debug.Log("Unknown item tag: " + Item.tag);
            break;
      }
   }

   public void OnClickIncreaseBuyItemsCount(Transform item) {
      switch (item.tag) {
         case "Sword":
            // Check against the maximum selling limit before incrementing.
            if (swordBuyCount < MAX_BUY_ITEM_COUNT) {
               swordBuyCount += 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "  " + swordBuyCount.ToString();
               item.Find("currencySpent").GetComponent<TextMeshProUGUI>().text = (swordBuyCount * Item.GetItemPrice(Item.ItemType.Sword)).ToString();
            }
            break;
         default:
            Debug.Log("Unknown item tag: " + item.tag);
            break;
      }
   }

   public void OnClickDecreaseBuyItemsCount(Transform item) {
      switch (item.tag) {
         case "Sword":
            // Check against the maximum selling limit before incrementing.
            if (swordBuyCount > MIN_BUY_ITEM_COUNT) {
               swordBuyCount -= 1;
               item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "  " + swordBuyCount.ToString();
               item.Find("currencySpent").GetComponent<TextMeshProUGUI>().text = (swordBuyCount * Item.GetItemPrice(Item.ItemType.Sword)).ToString();
            }
            break;
         default:
            Debug.Log("Unknown item tag: " + item.tag);
            break;
      }
   }

   public void RequestTradeHutPanel(int buttonID) {
      switch (buttonID) {
         case TRADE_BUTTON:
            ShowTradePanel();
            sellPanel.Find("SellButton").GetComponent<Button>().onClick.AddListener(() => SellItem());
            buyWindow.Find("BuyButton").GetComponent<Button>().onClick.AddListener(() => BuyItem());
            tradePanels.Find("ExitButton").GetComponent<Button>().onClick.AddListener(() => CloseTradeHutPanel(TRADE_BUTTON));

            break;
         case INFO_BUTTON:
            ShowInfoPanel();
            infoPanel.Find("ExitButton").GetComponent<Button>().onClick.AddListener(() => CloseTradeHutPanel(INFO_BUTTON));
            break;
         case UPGRADE_BUTTON:
            ShowUpgradePanel();
            upgradePanel.Find("YesButton").GetComponent<Button>().onClick.AddListener(() => UpgradeTradeHut());
            upgradePanel.Find("CancelButton").GetComponent<Button>().onClick.AddListener(() => CloseTradeHutPanel(UPGRADE_BUTTON));
            break;
         default:
            Debug.Log("Building Panel: Unknown button ID.");
            break;
      }
   }

   private void UpgradeTradeHut() {
      // Check if the trade hut can be upgraded
      if (tradeHutLevel < ENDING_LEVEL)
         tradeHutLevel += 1;
      else
         Debug.Log("Trade Hut is already at max level.");

      tradeHutLevelText.text = "Level " + tradeHutLevel.ToString();

      upgradePanel.transform.Find("YesButton").GetComponent<Button>().onClick.RemoveAllListeners();
      upgradePanel.transform.Find("CancelButton").GetComponent<Button>().onClick.RemoveAllListeners();

      //Close the upgrade panel after upgrading
      CloseUpgradePanel();
      PopUpManager.Instance.EnablePlayerInput();
   }

   public void CloseTradeHutPanel(int buttonID) {
      switch (buttonID) {
         case TRADE_BUTTON:
            CloseTradePanel();
            break;
         case INFO_BUTTON:
            CloseInfoPanel();
            break;
         case UPGRADE_BUTTON:
            CloseUpgradePanel();
            Debug.Log("Building Panel: Info requested.");
            break;
         default:
            Debug.Log("Building Panel: Unknown button ID.");
            break;
      }
      PopUpManager.Instance.EnablePlayerInput();
   }

   private void ShowTradePanel() {
      tradePanels.gameObject.SetActive(true);
      ShowSellPanel();
   }
   private void ShowInfoPanel() {
      infoPanel.gameObject.SetActive(true);
   }
   private void ShowUpgradePanel() {
      upgradePanel.gameObject.SetActive(true);
   }

   public void ShowSellPanel() {
      if (buyPanel.gameObject.activeSelf)
         CloseBuyPanel();

      sellPanel.gameObject.SetActive(true);
   }

   public void ShowBuyPanel() {
      if (sellPanel.gameObject.activeSelf)
         CloseSellPanel();
      buyPanel.gameObject.SetActive(true);
   }

   public void ShowBuyWindow() {
      buyWindow.gameObject.SetActive(true);
   }

   private void CloseTradePanel() {
      tradePanels.gameObject.SetActive(false);
   }
   private void CloseInfoPanel() {
      infoPanel.gameObject.SetActive(false);
   }
   private void CloseUpgradePanel() {
      upgradePanel.gameObject.SetActive(false);
   }

   private void CloseSellPanel() {
      sellPanel.gameObject.SetActive(false);
   }

   private void CloseBuyPanel() {
      buyPanel.gameObject.SetActive(false);
   }
}