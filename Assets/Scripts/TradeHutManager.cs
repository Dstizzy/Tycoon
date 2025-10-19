using TMPro;

using UnityEditor.Search;

using UnityEngine;
using UnityEngine.UI;

public class TradeHutManager : MonoBehaviour {
    public int crudeToolCount = 0;
    public static int tradeHutLevel = 1;
    public GameObject upgradeContainer;
    public Transform tradeContainer;
    public Transform tradeItemTemplate;
    public Transform tradePanel;
    public Transform infoPanel;
    public Transform upgradePanel;
    public TextMeshProUGUI tradeHutLevelText;

    public static TradeHutManager Instance { get; private set; }

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }

        if (tradeContainer == null) {
            Debug.LogError("Container is not assigned in the Inspector!");
        } 
        
        if(tradeItemTemplate == null) {

        } else {
            tradeItemTemplate.gameObject.SetActive(false);
        }
        if(tradePanel == null) {
            Debug.LogError("Trade Panel is not assigned in the Inspector!");
        }
        else
            tradePanel.gameObject.SetActive(false);
    }

    private void Start() {
        CreateItem(Item.GetItemSprite(Item.ItemType.CrudeTool), "Crude Tool", Item.GetItemValue(Item.ItemType.CrudeTool), -1.0f);
        CreateItem(Item.GetItemSprite(Item.ItemType.CrudeTool), "Crude Tool", Item.GetItemValue(Item.ItemType.CrudeTool), 0.0f);
        CreateItem(Item.GetItemSprite(Item.ItemType.CrudeTool), "Crude Tool", Item.GetItemValue(Item.ItemType.CrudeTool), 1.0f);

        Button[] buttons = upgradeContainer.GetComponentsInChildren<Button>();
        buttons[0].onClick.AddListener(() => UpgradeButtonClick("yes"));
        buttons[1].onClick.AddListener(() => UpgradeButtonClick("cancel"));
    }

    private void CreateItem(Sprite itemSprite, string itemName, int itemvalue, float positionIndex) {
        Transform tradeItemTransform = Instantiate(tradeItemTemplate, tradeContainer);
        RectTransform tradeItemRectTransform = tradeItemTransform.GetComponent<RectTransform>();

        float tradeItemWidth = 30f;
        tradeItemRectTransform.anchoredPosition = new Vector2(tradeItemWidth * positionIndex, 0);

        tradeItemTransform.Find("ItemValue").GetComponent<TextMeshProUGUI>().text = itemvalue.ToString();
        tradeItemTransform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = itemName;
        tradeItemTransform.Find("ItemImage").GetComponent<Image>().sprite = itemSprite;

        tradeItemTransform.gameObject.SetActive(true);
    }

    public int GetTradeHutLevel()
    {
                return tradeHutLevel;
    }

    public void UpgradeButtonClick(string buttonType)
    {
        if(buttonType == "yes")
        {
            tradeHutLevel++;
            tradeHutLevelText.text = $"lvl. {tradeHutLevel}";
            Debug.Log("Yes button clicked");
        }
        else if (buttonType == "cancel")
        {
            upgradePanel.gameObject.SetActive(false);
            Debug.Log("Upgrade cancelled.");
        }
        else
        {
            Debug.LogWarning("Invalid button type received");
        }

    }

    public void TradeHutButtonClick(int buttonId) {
        // make sure all panels are inactive
        tradePanel.gameObject.SetActive(false);
        infoPanel.gameObject.SetActive(false);
        upgradePanel.gameObject.SetActive(false);

        switch (buttonId)
        {
            case 0:
                tradePanel.gameObject.SetActive(true);
                break;
            case 1:
                infoPanel.gameObject.SetActive(true);
                break;
            case 2:
                upgradePanel.gameObject.SetActive(true);
                break;
            default:
                Debug.LogWarning("Invalid button ID received");
                break;
        }
    }
}
