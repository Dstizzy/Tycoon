using System;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour {

    public static InventoryManager Instance { get; private set; }
    public Transform InventoryPanel;
    public Transform ResourceContainer;
    public Transform ResourceTemplate;

    const int MIN_PEARL_COUNT = 0;
    const int MAX_PEARL_COUNT = 1000;
    const int MIN_CRYSTAL_COUNT = MIN_PEARL_COUNT;
    const int MAX_CRYSTAL_COUNT = MAX_PEARL_COUNT;
    const int RESOURCE_SPACING = 30;

    public int pearlCount;
    public int crystalCount;

    private void Awake() {

        if (Instance != null && Instance != this)
            Destroy(this.gameObject);
        else {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        if (InventoryPanel == null)
            Debug.LogError("Inventory Panel is not assigned in the Inspector!");
        else {
            InventoryPanel.gameObject.SetActive(false);
        }

        pearlCount = MIN_PEARL_COUNT;
        crystalCount = MIN_CRYSTAL_COUNT;
    }

    private void Start() {
        CreateResource(Resources.Load<Sprite>("Sprites/PearlIcon"), "Pearl", 0, "Pearl");
        CreateResource(Resources.Load<Sprite>("Sprites/CrystalIcon"), "Crystal", 1, "Crystal");
    }
    private void CreateResource(Sprite ResourceSprite, string ResourceName, float positionIndex, string ResourceTag) {

        int ResourceCount = ResourceName switch {
            "Pearl" => pearlCount,
            "Crystal" => crystalCount,
            _ => 0
        };

        // Instantiate the template and set its position in the container.
        Transform ResourceTransform = Instantiate(ResourceTemplate, ResourceContainer);
        RectTransform ResourceRectTransform = ResourceTransform.GetComponent<RectTransform>();

        ResourceTransform.tag = ResourceTag;

        ResourceRectTransform.anchoredPosition = new Vector2(RESOURCE_SPACING * positionIndex, 0);

        // Populate the TextMeshPro and Image components with item-specific data (value, name, sprite).
        ResourceTransform.Find("ResourceSprite").GetComponent<Image>().sprite = ResourceSprite;
        ResourceTransform.Find("ResourceCount").GetComponent<TextMeshProUGUI>().text = "  x" + ResourceCount.ToString();

        ResourceTransform.gameObject.SetActive(true);
    }

    public void TryAddPearl(int pearlAmount) {
        if (pearlCount > MAX_PEARL_COUNT) {
            Debug.LogError("Pearl count is at maximum!");
            return;
        } else if ((pearlCount + pearlAmount) > MAX_PEARL_COUNT) {
            Debug.LogError("Pearl count is at maximum!");
        } else
            pearlCount += pearlAmount;

        OnPearlCountChanged?.Invoke(pearlCount);

        return;
    }

    public void TrySpendPearl(int pearlAmount) {
        if (pearlCount < MIN_PEARL_COUNT) {
            Debug.LogError("Pearl count is at minimum!");
            return;
        } else if (pearlCount < pearlAmount) {
            Debug.LogError("Not enough pearls to spend!");
            return;
        } else
            pearlCount -= pearlAmount;

        OnPearlCountChanged?.Invoke(pearlCount);

        return;
    }

    public void TryAddCrystal(int pearlAmount) {
        if (pearlCount > MAX_PEARL_COUNT) {
            Debug.LogError("Pearl count is at maximum!");
            return;
        } else if ((pearlCount + pearlAmount) > MAX_PEARL_COUNT) {
            Debug.LogError("Pearl count is at maximum!");
        } else
            pearlCount += pearlAmount;

        OnCrystalCountChanged?.Invoke(crystalCount);

        return;
    }

    public void TrySpendCrystal(int pearlAmount) {
        if (pearlCount < MIN_PEARL_COUNT) {
            Debug.LogError("Pearl count is at minimum!");
            return;
        } else if (pearlCount < pearlAmount) {
            Debug.LogError("Not enough pearls to spend!");
            return;
        } else
            pearlCount -= pearlAmount;

        OnCrystalCountChanged?.Invoke(crystalCount);

        return;
    }

    

    public void ActivateInventoryPanel() {
        InventoryPanel.gameObject.SetActive(true);
    }

    public Action<int> OnPearlCountChanged;
    public Action<int> OnCrystalCountChanged;
}
