using System.Collections.Generic;

using TMPro;

using UnityEditor.Search;

using UnityEngine;
using UnityEngine.UI;

public class TradeHutManager : MonoBehaviour {
    public int crudeToolCount = 0;
    public int refinedToolCount = 0;
    public int artifact = 0;

    public Transform container;
    public Transform tradeItemTemplate;
    public Transform tradePanel;
    public Transform infoPanel;

    private List<Transform> tradeItems = new();
    //public static TradeHutManager Instance { get; private set; }

    private void Awake() {
        //if (Instance != null && Instance != this) {
        //    Destroy(gameObject);
        //} else {
        //    Instance = this;
        //}

        if (container == null) {
            Debug.LogError("Container is not assigned in the Inspector!");
        }

        if (tradeItemTemplate == null) {

        } else {
            tradeItemTemplate.gameObject.SetActive(false);
        }
        if (tradePanel == null) {
            Debug.LogError("Trade Panel is not assigned in the Inspector!");
        } else
            tradePanel.gameObject.SetActive(false);
    }

    private void Start() {
        CreateItem(Item.GetItemSprite(Item.ItemType.CrudeTool), "Crude Tool", Item.GetItemValue(Item.ItemType.CrudeTool), -1.0f, "Crude Tool");
        CreateItem(Item.GetItemSprite(Item.ItemType.CrudeTool), "Refined Tool", Item.GetItemValue(Item.ItemType.CrudeTool), 0.0f, "Refined Tool");
        CreateItem(Item.GetItemSprite(Item.ItemType.CrudeTool), "Artifact", Item.GetItemValue(Item.ItemType.CrudeTool), 1.0f, "Artifact");
    }

    private void CreateItem(Sprite itemSprite, string itemName, int itemvalue, float positionIndex, string ItemTag) {
        int itemCount = 0;

        Transform tradeItemTransform = Instantiate(tradeItemTemplate, container);
        RectTransform tradeItemRectTransform = tradeItemTransform.GetComponent<RectTransform>();

        tradeItemTransform.tag = ItemTag;

        float tradeItemWidth = 30f;
        tradeItemRectTransform.anchoredPosition = new Vector2(tradeItemWidth * positionIndex, 0);

        tradeItemTransform.Find("ItemValue").GetComponent<TextMeshProUGUI>().text = itemvalue.ToString();
        tradeItemTransform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = itemName;
        tradeItemTransform.Find("ItemImage").GetComponent<Image>().sprite = itemSprite;
        tradeItemTransform.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "    " + itemCount.ToString();

        Button increaseButton = tradeItemTransform.Find("QuantityButtons/IncreaseButton").GetComponent<Button>();
        Button decreaseButton = tradeItemTransform.Find("QuantityButtons/DecreaseButton").GetComponent<Button>();

        increaseButton.onClick.AddListener(() => OnClickIncreaseButton(tradeItemTransform));
        decreaseButton.onClick.AddListener(() => OnClickDecreaseButton(tradeItemTransform));

        tradeItemTransform.gameObject.SetActive(true);
        tradeItems.Add(tradeItemTransform);
    }

    public void ShowTradePanel() {
        tradePanel.gameObject.SetActive(true);
    }

    public void OnClickIncreaseButton(Transform Item) {
        switch (Item.tag) {
            case "Crude Tool":
                crudeToolCount += 1;
                Item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "    " + crudeToolCount.ToString();
                break;
            case "Refined Tool":
                refinedToolCount += 1;
                Item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "    " + refinedToolCount.ToString();
                break;
            case "Artifact":
                artifact += 1;
                Item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "    " + artifact.ToString();
                break;
            default:
                Debug.Log("Unknown item tag: " + Item.tag);
                break;
        }
    }
    public void OnClickDecreaseButton(Transform Item) {
        switch (Item.tag) {
            case "Crude Tool":
                crudeToolCount -= 1;
                Item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "    " + crudeToolCount.ToString();
                break;
            case "Refined Tool":
                refinedToolCount -= 1;
                Item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "    " + refinedToolCount.ToString();
                break;
            case "Artifact":
                artifact -= 1;
                Item.Find("ItemCount").GetComponent<TextMeshProUGUI>().text = "    " + artifact.ToString();
                break;
            default:
                Debug.Log("Unknown item tag: " + Item.tag);
                break;
        }
    }
}
