using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TradeHutManager : MonoBehaviour 
{
    const int MIN_SELL_ITEM_COUNT = 0;
    const int MAX_SELL_ITEM_COUNT = 100;
    const int TRADE_ITEM_SPACING = 30;
    const int TRADE_BUTTON = 1;
    const int INFO_BUTTON = 2;
    const int UPGRADE_BUTTON = 3;
    const int STARTING_LEVEL = 1;
    const int ENDING_LEVEL = 5;

    public int crudeToolCount = 0;
    public static int tradeHutLevel = STARTING_LEVEL;
    public Transform tradeContainer;
    public Transform tradeItemTemplate;
    public Transform tradePanel;
    public Transform infoPanel;
    public Transform upgradePanel;
    public TextMeshProUGUI tradeHutLevelText;

    //public int crudeToolCount = 0;
    public int refinedToolCount = 0;
    public int artifactCount = 0;

    // A list to hold references to all the instantiated trade item UI elements.
    private List<Transform> tradeItems = new();

    private void Awake() {
        if (tradeContainer == null) {
            Debug.LogError("Container is not assigned in the Inspector!");
        }

        // Hides the trade item template and the main trade panel when the game starts.
        if (tradeItemTemplate != null) {
            tradeItemTemplate.gameObject.SetActive(false);
        }
        if (tradePanel == null) {
            Debug.LogError("Trade Panel is not assigned in the Inspector!");
        } else {
            tradePanel.gameObject.SetActive(false);
        }

        if (infoPanel == null) {
            Debug.LogError("Info Panel is not assigned in the Inspector!");
        } else {
            infoPanel.gameObject.SetActive(false);
        }

        if (upgradePanel == null) {
            Debug.LogError("Upgrade Panel is not assigned in the Inspector!");
        } else {
            upgradePanel.gameObject.SetActive(false);
        }
    }

    private void Start() {
        // Creates the individual trade item entries for Crude Tools, Refined Tools, and Artifacts
        CreateItem(Item.GetItemSprite(Item.ItemType.CrudeTool), "Crude Tool", Item.GetItemValue(Item.ItemType.CrudeTool), -1.0f, "Crude Tool");
        CreateItem(Item.GetItemSprite(Item.ItemType.RefinedTool), "Refined Tool", Item.GetItemValue(Item.ItemType.RefinedTool), 0.0f, "Refined Tool");
        CreateItem(Item.GetItemSprite(Item.ItemType.Articfatct), "Artifact", Item.GetItemValue(Item.ItemType.Articfatct), 1.0f, "Artifact");
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
        tradeItems.Add(tradeItemTransform);
    }

    public void OnClickIncreaseButton(Transform Item) {
        // Uses the item's Tag to determine which counter variable to update.
        switch (Item.tag) {
            case "Crude Tool":
                // Check against the maximum selling limit before incrementing.
                if (crudeToolCount < MAX_SELL_ITEM_COUNT) {
                    crudeToolCount += 1;
                    Item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "  " + crudeToolCount.ToString();
                }
                break;
            case "Refined Tool":
                if (refinedToolCount < MAX_SELL_ITEM_COUNT) {
                    refinedToolCount += 1;
                    Item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "  " + refinedToolCount.ToString();
                }
                break;
            case "Artifact":
                if (artifactCount < MAX_SELL_ITEM_COUNT) {
                    artifactCount += 1;
                    Item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "  " + artifactCount.ToString();
                }
                break;
            default:
                Debug.Log("Unknown item tag: " + Item.tag);
                break;
        }
    }

    public void OnClickDecreaseButton(Transform Item) {
        // Uses the item's Tag to determine which counter variable to update.
        switch (Item.tag) {
            case "Crude Tool":
                // Check against the minimum selling limit (0) before decrementing.
                if (crudeToolCount > MIN_SELL_ITEM_COUNT) {
                    crudeToolCount -= 1;
                    Item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "  " + crudeToolCount.ToString();
                }
                break;
            case "Refined Tool":
                if (refinedToolCount > MIN_SELL_ITEM_COUNT) {
                    refinedToolCount -= 1;
                    Item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "  " + refinedToolCount.ToString();
                }
                break;
            case "Artifact":
                if (artifactCount > MIN_SELL_ITEM_COUNT) {
                    artifactCount -= 1;
                    Item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "  " + artifactCount.ToString();
                }
                break;
            default:
                Debug.Log("Unknown item tag: " + Item.tag);
                break;
        }
    }

    public void RequestTradeHutPanel(int buttonID) {
        switch (buttonID) {
            case TRADE_BUTTON:
                ShowTradePanel();
                tradePanel.transform.Find("ExitButton").GetComponent<Button>().onClick.AddListener(() => CloseTradeHutPanel(TRADE_BUTTON));
                break;
            case INFO_BUTTON:
                ShowInfoPanel();
                infoPanel.transform.Find("ExitButton").GetComponent<Button>().onClick.AddListener(() => CloseTradeHutPanel(INFO_BUTTON));
                break;
            case UPGRADE_BUTTON:
                ShowUpgradePanel();
                upgradePanel.transform.Find("YesButton").GetComponent<Button>().onClick.AddListener(() => UpgradeTradeHut());
                upgradePanel.transform.Find("CancelButton").GetComponent<Button>().onClick.AddListener(() => CloseTradeHutPanel(UPGRADE_BUTTON));
                break;
            default:
                Debug.Log("Building Panel: Unknown button ID.");
                break;
        }
    }

    private void UpgradeTradeHut()
    {
        // Check if the trade hut can be upgraded
        if (tradeHutLevel < 5)
        {
            tradeHutLevel += 1;
        }
        else
        {
            Debug.Log("Trade Hut is already at max level.");
        }
        //tradeHutLevelText.text = "Level " + tradeHutLevel.ToString();
        // Close the upgrade panel after upgrading
        CloseUpgradePanel();
    }

    public int GetLevel()
    {
        return tradeHutLevel;
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
        tradePanel.gameObject.SetActive(true);
    }
    private void ShowInfoPanel() {
        infoPanel.gameObject.SetActive(true);
    }
    private void ShowUpgradePanel() {
        upgradePanel.gameObject.SetActive(true);
    }
    private void CloseTradePanel() {
        tradePanel.gameObject.SetActive(false);
    }
    private void CloseInfoPanel() {
        infoPanel.gameObject.SetActive(false);
    }
    private void CloseUpgradePanel() {
        upgradePanel.gameObject.SetActive(false);
    }

}