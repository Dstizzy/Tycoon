using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ForgeManager : MonoBehaviour
{
   /* Constants */
   const int CRAFT_BUTTON         = 1;
   const int INFO_BUTTON          = 2;
   const int UPGRADE_BUTTON       = 3;
   const int STARTING_LEVEL       = 1;
   const int CRUDE_TOOL_COST      = 10;
   const int HARPOON_COST         = 25;
   const int PATCH_KIT_COST       = 15;
   const int PRESSUREV_VALVE_COST = 50;
   const int DIVING_BELL_COST     = 75;
   const int ENGINE_COST          = 150;
   const int PRECISION_LENS_COST  = 200;
   const int TIER_1 = 1;
   const int TIER_2 = 2;
   const int TIER_3 = 3;
   const int ENDING_LEVEL = 3;
   private const int MIN_CRAFT_AMOUNT = 0;
   private const int MAX_CRAFT_AMOUNT = 99;
   public bool hasClockworkBlueprint = false;
   public bool hasIndustrialBlueprint = false;


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
   public bool isMercenaryEngineerActive;

   /* Private state variables */
   private Transform currentCraftWindow;
   private int selectedCraftAmount = 0;
   private Item.ItemType selectedItemType;
   private static int forgeLevel = STARTING_LEVEL;

   public static ForgeManager Instance { get; private set; }

   private void Start()
   {
      SetCraftItemButtons();

      if (craftWindowTemplate.gameObject.scene.name != null)
         craftWindowTemplate.gameObject.SetActive(false);
   }

   private void Awake()
   {
      if (Instance != null && Instance != this)
         Destroy(this.gameObject);
      else 
      {
         Instance = this;
         DontDestroyOnLoad(this.gameObject);
      }

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
            /* Tier 1 items */
            case Item.ItemType.CrudeTool:
               costPerItem = CRUDE_TOOL_COST;
               break;
            case Item.ItemType.Harpoon:
               costPerItem = HARPOON_COST;
               break;
            case Item.ItemType.PatchKit:
               costPerItem = PATCH_KIT_COST;
               break;

            /* Tier 2 items */
            case Item.ItemType.PressureValve:
               costPerItem = PRESSUREV_VALVE_COST;
               break;
            case Item.ItemType.DivingBell:
               costPerItem = DIVING_BELL_COST;
               break;

            /* Tier 3 items */
            case Item.ItemType.Engine:
               costPerItem = ENGINE_COST;
               break;
            case Item.ItemType.PrecisionLens:
               costPerItem = PRECISION_LENS_COST;
               break;
         }

         totalCost = selectedCraftAmount * costPerItem;
         costText.GetComponent<TextMeshProUGUI>().text = totalCost.ToString();
      }
     
   }

   public void CraftSelectedItem()
   {
      Debug.Log("Craft");
      switch (selectedItemType)
      {
         /* Tier 1 items */
         case Item.ItemType.CrudeTool:
            InventoryManager.Instance.TrySpendOre(CRUDE_TOOL_COST);
            InventoryManager.Instance.TryAddCrudeTool(selectedCraftAmount);
            break;
         case Item.ItemType.Harpoon:
            InventoryManager.Instance.TrySpendOre(HARPOON_COST);
            InventoryManager.Instance.TryAddHarpoon(selectedCraftAmount);
            break;
         case Item.ItemType.PatchKit:
            InventoryManager.Instance.TrySpendOre(PATCH_KIT_COST);
            InventoryManager.Instance.TryAddPatchKit(selectedCraftAmount);
            break;

         // Tier 2 items
         case Item.ItemType.PressureValve:
            InventoryManager.Instance.TrySpendOre(PRESSUREV_VALVE_COST);
            InventoryManager.Instance.TryAddPressureValve(selectedCraftAmount);
            break;
         case Item.ItemType.DivingBell:
            InventoryManager.Instance.TrySpendOre(DIVING_BELL_COST);
            InventoryManager.Instance.TryAddDivingBell(selectedCraftAmount);
            break;

         // Tier 3 items
         case Item.ItemType.Engine:
            InventoryManager.Instance.TrySpendOre(ENGINE_COST);
            InventoryManager.Instance.TryAddEngine(selectedCraftAmount);
            break;
         case Item.ItemType.PrecisionLens:
            InventoryManager.Instance.TrySpendOre(PRECISION_LENS_COST);
            InventoryManager.Instance.TryAddPrecisionLens(selectedCraftAmount);
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
            Debug.Log($"-- Found ItemButton: {btn.name}"); 

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
               Debug.Log($"CLICKED: {type} in Tier {tier}"); 
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
            upgradePanel.transform.Find("YesButton").   GetComponent<Button>().onClick.AddListener(() => UpgradeForge());
            upgradePanel.transform.Find("CancelButton").GetComponent<Button>().onClick.AddListener(() => CloseForgePanel(UPGRADE_BUTTON));
            break;
         default:
            Debug.Log("Building Panel: Unknown button ID.");
            break;
      }
   }

   public void UpgradeForge()
   {
      int upgradeCost = 0;

      // Determine the cost needed for current level be upgraded
      if (forgeLevel == 1)
      {
         upgradeCost = 500;
      }
      else if (forgeLevel == 2)
      {
         upgradeCost = 800;
      }

      // Check if there is sufficient pearls to upgrade
      if (InventoryManager.Instance.pearlCount >= upgradeCost)
      {
         // Enough pearls, deduct the required amount
         InventoryManager.Instance.TrySpendPearl(upgradeCost);

         // Perform the upgrade
         if (forgeLevel < ENDING_LEVEL)
         {
            forgeLevel += 1;
         }
         forgeLevelText.text = "Level " + forgeLevel.ToString();
         CloseUpgradePanel();
         PopUpManager.Instance.EnablePlayerInput();
      }
      else
      {
         TextMeshProUGUI upgradeText = upgradePanel.GetComponentInChildren<TextMeshProUGUI>();

         if (upgradeText != null)
         {
            // Display the fail message
            upgradeText.text = $"Not enough pearls to upgrade!\nYou need {upgradeCost} pearls.";
            upgradePanel.transform.Find("YesButton").gameObject.SetActive(false);
         }
         Debug.Log("Not enough pearls to upgrade!");

         
      }

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

      UpdateTierButtonState(t1, TIER_1, true);
      UpdateTierButtonState(t2, TIER_2, hasClockworkBlueprint);
      UpdateTierButtonState(t3, TIER_3, hasIndustrialBlueprint);

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

   private void UpdateTierButtonState(Transform btnTransform, int requiredLevel, bool isUnlocked)
   {
      Button btn = btnTransform.GetComponent<Button>();
      Transform overlay = btnTransform.Find("Overlay");

      btn.interactable = isUnlocked;


      if (overlay != null)
      {
         overlay.gameObject.SetActive(!isUnlocked);
      }

      btn.onClick.RemoveAllListeners();
      if (isUnlocked)
      {
         btn.onClick.AddListener(() => OpenTierPanel(requiredLevel));
      }

   }

   private void ShowInfoPanel()
   {
      infoPanel.gameObject.SetActive(true);
   }
   private void ShowUpgradePanel()
   {
      int upgradeCost = 0,
          nextLevel = forgeLevel + 1;

      upgradePanel.gameObject.SetActive(true);
      TextMeshProUGUI upgradeText = upgradePanel.GetComponentInChildren<TextMeshProUGUI>();

      if (upgradeText != null)
      {
         if (forgeLevel == 1)
         {
            upgradeCost = 500;
         }
         else if (forgeLevel == 2)
         {
            upgradeCost = 800;
         }

         if (forgeLevel < ENDING_LEVEL)
         {
            upgradeText.text = $"Would you like to upgrade\nto next level for {upgradeCost}\npearls?";
         }
         else
         {
            upgradeText.text = "Max Level Reached!";
            upgradePanel.transform.Find("YesButton").gameObject.SetActive(false);
         }
      }
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
   
