using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ForgeManager : MonoBehaviour
{
   /* Constants */
   const int CRAFT_BUTTON = 1;
   const int INFO_BUTTON = 2;
   const int UPGRADE_BUTTON = 3;
   const int STARTING_LEVEL = 1;
   const int TIER_1 = 1;
   const int TIER_2 = 2;
   const int TIER_3 = 3;
   const int ENDING_LEVEL = 5;
   private const int MIN_CRAFT_AMOUNT = 0;
   private const int MAX_CRAFT_AMOUNT = 99;


   /* Inspector Variables */
   [Header("Main Panels")]
   [SerializeField] private Transform craftPanel;
   [SerializeField] private Transform infoPanel;
   [SerializeField] private Transform upgradePanel;

   [Header("Tier Panels")]
   [SerializeField] private GameObject tier1Panel;
   [SerializeField] private GameObject tier2Panel;
   [SerializeField] private GameObject tier3Panel;

   [Header("Window Containers")]
   [SerializeField] private Transform tier1Container;
   [SerializeField] private Transform tier2Container;
   [SerializeField] private Transform tier3Container;

   [Header("Windw Template")]
   [SerializeField] private Transform craftWindowTemplate;


   public TextMeshProUGUI forgeLevelText;

   /* Private state variables */
   private Transform currentCraftWindow;
   private int selectedCraftAmount = 0;
   private Item.ItemType selectedItemType;
   private static int forgeLevel = STARTING_LEVEL;

   private void Start()
   {
      SetCraftItemButtons();

      if (craftWindowTemplate.gameObject.scene.name != null)
         craftWindowTemplate.gameObject.SetActive(false);
   }

   private void Awake()
   {
      craftPanel.gameObject.SetActive(false);
      infoPanel.gameObject.SetActive(false);
      upgradePanel.gameObject.SetActive(false);

      if (tier1Container)
         tier1Container.gameObject.SetActive(true);

      if (tier2Container)
         tier3Container.gameObject.SetActive(true);

      if (tier3Container)
         tier3Container.gameObject.SetActive(true);

      CloseAllTierPanels();

      forgeLevelText.text = "Level" + forgeLevel;
   }

   private void CreateCraftWindow(Transform container, Item.ItemType itemType)
   {
      Debug.Log("CreateCraftWindow: Attempting to spawn window...");

      // 1. Safe Container Check
      if (container == null) { Debug.LogError("CRITICAL: Container is null!"); return; }
      container.gameObject.SetActive(true);
      if (container.parent != null) container.parent.gameObject.SetActive(true);

      // 2. Clear Old Window
      if (currentCraftWindow != null)
      {
         Destroy(currentCraftWindow.gameObject);
         currentCraftWindow = null;
      }

      // 3. Instantiate and Verify
      if (craftWindowTemplate == null) { Debug.LogError("CRITICAL: CraftWindowTemplate is not assigned in Inspector!"); return; }

      Transform windowTransform = Instantiate(craftWindowTemplate, container);
      currentCraftWindow = windowTransform;

      windowTransform.localPosition = Vector3.zero;
      windowTransform.localScale = Vector3.one;

      selectedItemType = itemType;
      selectedCraftAmount = 0;

      // ---------------------------------------------------------
      //  SAFE FINDING OF UI ELEMENTS
      // ---------------------------------------------------------

      // CHECK 1: Item Image
      Transform imageTrans = windowTransform.Find("ItemImage");
      if (imageTrans != null)
      {
         imageTrans.GetComponent<Image>().sprite = Item.GetItemSprite(itemType);
      }
      else Debug.LogError("MISSING: Could not find object named 'ItemImage' in prefab!");

      // CHECK 2: Increase Button
      Transform incBtn = windowTransform.Find("QuantityButtons/IncreaseButton");
      if (incBtn != null)
      {
         incBtn.GetComponent<Button>().onClick.AddListener(() => IncreaseCraftAmount(windowTransform));
      }
      else Debug.LogError("MISSING: Could not find 'QuantityButtons/IncreaseButton' in prefab!");

      // CHECK 3: Decrease Button
      Transform decBtn = windowTransform.Find("QuantityButtons/DecreaseButton");
      if (decBtn != null)
      {
         decBtn.GetComponent<Button>().onClick.AddListener(() => DecreaseCraftAmount(windowTransform));
      }
      else Debug.LogError("MISSING: Could not find 'QuantityButtons/DecreaseButton' in prefab!");

      // CHECK 4: Craft Button
      Transform craftBtn = windowTransform.Find("CraftButton");
      if (craftBtn != null)
      {
         craftBtn.GetComponent<Button>().onClick.AddListener(() => CraftSelectedItem());
      }
      else Debug.LogError("MISSING: Could not find 'CraftButton' in prefab!");

      // ---------------------------------------------------------

      UpdateCraftAmountUI(windowTransform);
      windowTransform.gameObject.SetActive(true);

      Debug.Log("Success! Window should be visible.");
   }

   /* Open the craft window when a item is selected*/
   public void OnCraftItemSelected(int tier, Item.ItemType itemType)
   {
      /* Decide which container to used based on the selected tier */
      Transform targetContainer = null;

      switch (tier)
      {
         case TIER_1:
            targetContainer = tier1Container;
            break;

         case TIER_2:
            targetContainer = tier2Container;
            break;

         case TIER_3:
            targetContainer = tier3Container;
            break;
      }

      if (targetContainer != null)
         CreateCraftWindow(targetContainer, itemType);
   }

   public void IncreaseCraftAmount(Transform window)
   {
      if (selectedCraftAmount < MAX_CRAFT_AMOUNT)
         selectedCraftAmount++;
      UpdateCraftAmountUI(window);
   }

   public void DecreaseCraftAmount(Transform window)
   {
      if (selectedCraftAmount > MIN_CRAFT_AMOUNT)
         selectedCraftAmount--;
      UpdateCraftAmountUI(window);
   }

   public void UpdateCraftAmountUI(Transform window)
   {
      int costPerItem = 0,
          totalCost;

      TextMeshProUGUI amountText = window.Find("ItemCount").GetComponent<TextMeshProUGUI>();
      if (amountText != null)
         amountText.text = selectedCraftAmount.ToString();

      Transform costText = window.Find("currencyNeeded");

      if (costText == null)
         costText = window.Find("ItemValue");

      if (costText != null)
      {
         
         switch (selectedItemType)
         {
            case Item.ItemType.CrudeTool:
               costPerItem = 15;
               break;

            case Item.ItemType.RefinedTool:
               costPerItem = 25;
               break;

            case Item.ItemType.Artifact:
               costPerItem = 50;
               break;
         }

         totalCost = selectedCraftAmount * costPerItem;
         costText.GetComponent<TextMeshProUGUI>().text = totalCost.ToString();
      }
     
   }

   public void CraftSelectedItem()
   {
      switch (selectedItemType)
      {
         case Item.ItemType.CrudeTool:
            InventoryManager.Instance.TryAddCrudeTool(selectedCraftAmount);
            break;

         case Item.ItemType.RefinedTool:
            InventoryManager.Instance.TryAddCrudeTool(selectedCraftAmount);
            break;

         case Item.ItemType.Artifact:
            InventoryManager.Instance.TryAddCrudeTool(selectedCraftAmount);
            break;
      }
   }

   private void SetCraftItemButtons()
   {
      SetupTierButtons(tier1Panel, TIER_1);
      SetupTierButtons(tier2Panel, TIER_2);
      SetupTierButtons(tier3Panel, TIER_3);

   }

   private void SetupTierButtons(GameObject tierPanel, int tier)
   {
      if (tierPanel == null) return;

      // Debug: Tell us we are looking for buttons
      Debug.Log($"SetupTierButtons: Scanning {tierPanel.name}...");

      Button[] buttons = tierPanel.GetComponentsInChildren<Button>(true);

      foreach (var btn in buttons)
      {
         if (btn.name.Contains("ItemButton"))
         {
            Debug.Log($"-- Found ItemButton: {btn.name}"); // Spy 1: Did we find it?

            btn.onClick.RemoveAllListeners();
            ItemUI itemUI = btn.GetComponent<ItemUI>();

            if (itemUI == null)
            {
               Debug.LogError($"-- ERROR: {btn.name} has no ItemUI script!");
               continue;
            }

            Item.ItemType type = itemUI.itemType;

            // Wire up the click
            btn.onClick.AddListener(() => {
               Debug.Log($"CLICKED: {type} in Tier {tier}"); // Spy 2: Did the click happen?
               OnCraftItemSelected(tier, type);
            });
         }
         else if (btn.name == "ExitButton")
         {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => {
               CloseAllTierPanels();
               if (currentCraftWindow != null) Destroy(currentCraftWindow.gameObject);
            });
         }
      }
   }


   public void RequestForgePanel(int buttonID)
   {
      switch (buttonID)
      {
         case CRAFT_BUTTON:
            ShowCraftPanel();
            craftPanel.transform.Find("ExitButton").GetComponent<Button>().onClick.AddListener(() => CloseForgePanel(CRAFT_BUTTON));
            break;
         case INFO_BUTTON:
            ShowInfoPanel();
            infoPanel.transform.Find("ExitButton").GetComponent<Button>().onClick.AddListener(() => CloseForgePanel(INFO_BUTTON));
            break;
         case UPGRADE_BUTTON:
            ShowUpgradePanel();
            upgradePanel.transform.Find("YesButton").GetComponent<Button>().onClick.AddListener(() => UpgradeForge());
            upgradePanel.transform.Find("CancelButton").GetComponent<Button>().onClick.AddListener(() => CloseForgePanel(UPGRADE_BUTTON));
            break;
         default:
            Debug.Log("Building Panel: Unknown button ID.");
            break;
      }
   }

   public void UpgradeForge()
   {
      // Check if the forge can be upgraded
      if (forgeLevel < ENDING_LEVEL)
         forgeLevel += 1;

      forgeLevelText.text = "Level " + forgeLevel.ToString();
      CloseUpgradePanel();
      PopUpManager.Instance.EnablePlayerInput();
   }

   public void CloseForgePanel(int buttonID)
   {
      switch (buttonID)
      {
         case CRAFT_BUTTON:
            CloseCraftPanel();
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

   private void ShowCraftPanel()
   {
      craftPanel.gameObject.SetActive(true);

      Transform t1 = craftPanel.transform.Find("TierButtons/Tier1");
      Transform t2 = craftPanel.transform.Find("TierButtons/Tier2");
      Transform t3 = craftPanel.transform.Find("TierButtons/Tier3");

      if (t1 != null)
      {
         t1.GetComponent<Button>().onClick.RemoveAllListeners(); // Clean up old clicks
         t1.GetComponent<Button>().onClick.AddListener(() => OpenTierPanel(1));
      }
      else Debug.LogError("Could not find button 'Tier1' inside TierButtons!");

      if (t2 != null)
      {
         t2.GetComponent<Button>().onClick.RemoveAllListeners();
         t2.GetComponent<Button>().onClick.AddListener(() => OpenTierPanel(2));
      }
      else Debug.LogError("Could not find button 'Tier2' inside TierButtons!");

      if (t3 != null)
      {
         t3.GetComponent<Button>().onClick.RemoveAllListeners();
         t3.GetComponent<Button>().onClick.AddListener(() => OpenTierPanel(3));
      }
      else Debug.LogError("Could not find button 'Tier3' inside TierButtons!");
   }

   private void ShowInfoPanel()
   {
      infoPanel.gameObject.SetActive(true);
   }
   private void ShowUpgradePanel()
   {
      upgradePanel.gameObject.SetActive(true);
   }
   private void CloseCraftPanel()
   {
      craftPanel.gameObject.SetActive(false);

      /* Destroy the craft window when closing the main panel*/
      if (currentCraftWindow != null)
         Destroy(currentCraftWindow.gameObject);
   }
   private void CloseInfoPanel()
   {
      infoPanel.gameObject.SetActive(false);
   }
   private void CloseUpgradePanel()
   {
      upgradePanel.gameObject.SetActive(false);
   }

   public void CloseAllTierPanels()
   {
      tier1Panel.SetActive(false);
      tier2Panel.SetActive(false);
      tier3Panel.SetActive(false);
   }

   public void OpenTierPanel(int tier)
   {
      CloseAllTierPanels();

      /* Destroy any opened craft windows from other tiers*/
      if (currentCraftWindow != null)
         Destroy(currentCraftWindow.gameObject);

      switch (tier)
      {
         case TIER_1:
            tier1Panel.SetActive(true);
            break;

         case TIER_2:
            tier2Panel.SetActive(true);
            break;

         case TIER_3:
            tier3Panel.SetActive(true);
            break;
      }
   }
}
   
