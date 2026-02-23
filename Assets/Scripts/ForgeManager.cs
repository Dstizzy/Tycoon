using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class CraftingJob
{
   public Item.ItemType itemType;
   public int           amount;
   public int           turnsRemaining;
   public string        itemName;
}

public class ForgeManager : MonoBehaviour
{
   /* Constants */
   const int CRAFT_BUTTON = 1;
   const int INFO_BUTTON = 2;
   const int UPGRADE_BUTTON = 3;
   const int STARTING_LEVEL = 1;
   const int CRUDE_TOOL_COST = 10;
   const int HARPOON_COST = 25;
   const int PATCH_KIT_COST = 15;
   const int PRESSUREV_VALVE_COST = 50;
   const int DIVING_BELL_COST = 75;
   const int ENGINE_COST = 150;
   const int PRECISION_LENS_COST = 200;
   const int TIER_1 = 1;
   const int TIER_2 = 2;
   const int TIER_3 = 3;
   public const int LEVEL_2_PEARL_COST = 500;
   public const int LEVEL_3_PEARL_COST = 800;
   const int ENDING_LEVEL = 3;
   private const int MIN_CRAFT_AMOUNT = 0;
   private const int MAX_CRAFT_AMOUNT = 99;
   public bool isOverclockUnlocked = false;
   public bool isLabTier3Unlocked = false;
   public bool hasTier3Blueprint = false;
   public bool hasTier2Blueprint = false;
   public bool hasMercenaryEngineer = false;
   public bool isMercenaryEngineerActive = false;


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
   [SerializeField] private Transform levelCanvas;

   [Header("Crafting Queue")]
   public List<CraftingJob> activeJobs = new List<CraftingJob>();
   [SerializeField] private GameObject errorPanel;

   [Header("Active Crafting Popup")]
   [SerializeField] private GameObject activeQueuePanel;
   [SerializeField] private TextMeshProUGUI queueText;

   /* Private state variables */
   private Transform currentCraftWindow;
   private List<Item.ItemType> stagingItems = new List<Item.ItemType>();
   private Toggle currentOverclockToggle;
   private Image craftSlot1;
   private Image craftSlot2;
   private GameObject craftButtonObject;
   

   public static ForgeManager Instance { get; private set; }
   public static int forgeLevel = STARTING_LEVEL;
   TickerSystem ticker;

   private bool hasCraftedThisTurn = false;

   private void Start()
   {
      SetCraftItemButtons();

      if (craftWindowTemplate.gameObject.scene.name != null)
         craftWindowTemplate.gameObject.SetActive(false);

      // Link to turn system
      if (TurnManager.Instance != null)
         TurnManager.OnTurnEnded += ProcessCraftingQueue;
   }

   private void OnDestroy()
   {
      // Unlink to turn system to prevent errors
      if (TurnManager.Instance != null)
         TurnManager.OnTurnEnded -= ProcessCraftingQueue;
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

      ticker = TickerSystem.Instance;

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
      if (craftPanel != null)
      {
         // Find the ErrorPanel inside the CraftPanel
         Transform errorTrans = craftPanel.Find("ErrorPanel");
         if (errorTrans != null)
         {
            errorPanel = errorTrans.gameObject;
            errorPanel.SetActive(false); // Hide it by default
         }
         else
         {
            Debug.LogError("ForgeManager: Could not find 'ErrorPanel' inside CraftPanel!");
         }
         if (activeQueuePanel != null)
         {
            activeQueuePanel.SetActive(false);
         }
      }
   }

   private void CreateCraftWindow(Transform container)
   {
      Debug.Log("CreateCraftWindow: Spawning...");

      if (container == null) return;


      if (currentCraftWindow != null) Destroy(currentCraftWindow.gameObject);

      Transform windowTransform = Instantiate(craftWindowTemplate, container);
      currentCraftWindow = windowTransform;
      windowTransform.localPosition = Vector3.zero;
      windowTransform.localScale = Vector3.one;

      // FIND IMAGES
      Transform img1 = FindChildByName(windowTransform, "ItemImage1");
      Transform img2 = FindChildByName(windowTransform, "ItemImage2");

      if (img1 != null) craftSlot1 = img1.GetComponent<Image>();
      if (img2 != null) craftSlot2 = img2.GetComponent<Image>();

      //FIND BUTTONS
      Transform craftBtn = FindChildByName(windowTransform, "CraftButton");
      if (craftBtn != null)
      {
         craftButtonObject = craftBtn.gameObject; // Store the object
         craftButtonObject.GetComponent<Button>().onClick.AddListener(() => CraftStagedItems());
      }

      //FIND TOGGLE
      Transform toggleTrans = FindChildByName(windowTransform, "OverclockToggle");
      if (toggleTrans != null)
      {
         currentOverclockToggle = toggleTrans.GetComponent<Toggle>();
         currentOverclockToggle.gameObject.SetActive(isOverclockUnlocked);
         currentOverclockToggle.isOn = false;
      }

      // Refresh Visuals based on Staging List
      UpdateStagingUI();

      windowTransform.gameObject.SetActive(true);
   }


   /* Open the craft window when a item is selected*/
   public void OnCraftItemSelected(int tier, Item.ItemType itemType)
   {
      // 1. Determine Capacity based on Level
      int maxStagingSlots = (forgeLevel >= 2) ? 2 : 1;

      // 2. Logic: If list is full, clear it and start new. Otherwise, add to it.
      if (stagingItems.Count >= maxStagingSlots)
      {
         stagingItems.Clear();
      }

      // 3. Add the item
      stagingItems.Add(itemType);

      // 4. Determine Container
      Transform targetContainer = null;
      switch (tier)
      {
         case TIER_1: targetContainer = tier1Container; break;
         case TIER_2: targetContainer = tier2Container; break;
         case TIER_3: targetContainer = tier3Container; break;
      }

      // 5. Open/Refresh the Window
      if (targetContainer != null) CreateCraftWindow(targetContainer);
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
            btn.onClick.AddListener(() =>
            {
               Debug.Log($"CLICKED: {type} in Tier {tier}");
               OnCraftItemSelected(tier, type);
            });
         }
         else if (btn.name == "ExitButton")
         {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
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
      int upgradeCost = 0;

      // Determine the cost needed for current level be upgraded
      if (forgeLevel == 1)
      {
         upgradeCost = LEVEL_2_PEARL_COST;
      }
      else if (forgeLevel == 2)
      {
         upgradeCost = LEVEL_3_PEARL_COST;
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

      if (errorPanel != null)
         errorPanel.SetActive(false);
      if (activeQueuePanel != null)
         activeQueuePanel.SetActive(false);

      Transform t1 = craftPanel.transform.Find("TierButtons/Tier1");
      Transform t2 = craftPanel.transform.Find("TierButtons/Tier2");
      Transform t3 = craftPanel.transform.Find("TierButtons/Tier3");

      UpdateTierButtonState(t1, TIER_1, true);
      UpdateTierButtonState(t2, TIER_2, hasTier2Blueprint);
      UpdateTierButtonState(t3, TIER_3, hasTier3Blueprint);

      RefreshTurnValues();

      if (t1 != null)
      {
         t1.GetComponent<Button>().onClick.RemoveAllListeners();
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
      int upgradeCost = 0;
      string upgradeExplanation = "";

      upgradePanel.gameObject.SetActive(true);

      Transform mainTextTransform = upgradePanel.Find("UpgradePanelText");
      Transform explanationTransform = upgradePanel.Find("ExplanationText");

      TextMeshProUGUI upgradeText = mainTextTransform != null ? mainTextTransform.GetComponent<TextMeshProUGUI>() : null;
      TextMeshProUGUI expText = explanationTransform != null ? explanationTransform.GetComponent<TextMeshProUGUI>() : null;

      if (forgeLevel == 1)
      {
         upgradeCost = 500;
         upgradeExplanation = "Bonus: Unlocks a 2nd simultaneous crafting slot!";
      }
      else if (forgeLevel == 2)
      {
         upgradeCost = 800;
         upgradeExplanation = "Bonus: Reduces all crafting times by 1 turn!";
      }

      if (forgeLevel < ENDING_LEVEL)
      {
         if (upgradeText != null)
            upgradeText.text = $"Would you like to upgrade\nto next level for {upgradeCost} pearls?";

         if (expText != null)
         {
            expText.gameObject.SetActive(true);
            expText.text = $"<color=black>{upgradeExplanation}</color>";
         }

         Transform yesBtn = upgradePanel.Find("YesButton");
         if (yesBtn != null) yesBtn.gameObject.SetActive(true);
      }
      else
      {
         if (upgradeText != null)
            upgradeText.text = "Max Level Reached!";

         if (expText != null)
            expText.gameObject.SetActive(false); // Hide explanation if max level

         Transform yesBtn = upgradePanel.Find("YesButton");
         if (yesBtn != null) yesBtn.gameObject.SetActive(false);
      }
   }
   private void CloseCraftPanel()
   {
      craftPanel.gameObject.SetActive(false);
      if (errorPanel != null) errorPanel.SetActive(false);

      if (activeQueuePanel != null) activeQueuePanel.SetActive(false);

      stagingItems.Clear();

      if (currentCraftWindow != null) Destroy(currentCraftWindow.gameObject);
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

   public void UnlockOverclock()
   {
      isOverclockUnlocked = true;
      Debug.Log("Overclocking Unlocked in Forge!");
   }

   private int GetTurnsNeeded(Item.ItemType type)
   {
      int turns = 1;

      // 1. Determine Base Turns
      switch (type)
      {
         case Item.ItemType.CrudeTool:
            turns = isLabTier3Unlocked ? 1 : 2;
            break;
         case Item.ItemType.Harpoon:
            turns = isLabTier3Unlocked ? 1 : 2;
            break;
         case Item.ItemType.PatchKit:
            turns = isLabTier3Unlocked ? 1 : 2;
            break;
         case Item.ItemType.PressureValve:
            turns = isLabTier3Unlocked ? 1 : 2;
            break;
         case Item.ItemType.DivingBell:
            turns = 3;
            break;
         case Item.ItemType.Engine:
            turns = 4;
            break;
         case Item.ItemType.PrecisionLens:
            turns = 5;
            break;
         default:
            turns = 1;
            break;
      }

      // 2. Level 3 Bonus: Reduce turn cost by 1
      if (forgeLevel >= 3)
      {
         turns -= 1;
      }

      // 3. Allow minimum 1 turn
      if (turns < 1)
         turns = 1;

      return turns;
   }

   private void ProcessCraftingQueue()
   {
      hasCraftedThisTurn = false;
      int jobCount;

      int maxParallelSlots = (forgeLevel >= 2) ? 2 : 1;

      for (jobCount = activeJobs.Count - 1; jobCount >= 0; jobCount--)
      {

         if (jobCount < maxParallelSlots)
         {
            CraftingJob job = activeJobs[jobCount];
            job.turnsRemaining--;

            if (job.turnsRemaining <= 0)
            {
               DeliverItem(job);
               activeJobs.RemoveAt(jobCount);
               Debug.Log($"Crafting Complete: {job.itemName}");
            }
         }
      }
   }

   private void DeliverItem(CraftingJob job)
   {
      switch (job.itemType)
      {
         case Item.ItemType.CrudeTool:
            InventoryManager.Instance.TryAddCrudeTool(job.amount);
            break;
         case Item.ItemType.Harpoon:
            InventoryManager.Instance.TryAddHarpoon(job.amount);
            break;
         case Item.ItemType.PatchKit:
            InventoryManager.Instance.TryAddPatchKit(job.amount);
            break;
         case Item.ItemType.PressureValve:
            InventoryManager.Instance.TryAddPressureValve(job.amount);
            break;
         case Item.ItemType.DivingBell:
            InventoryManager.Instance.TryAddDivingBell(job.amount);
            break;
         case Item.ItemType.Engine:
            InventoryManager.Instance.TryAddEngine(job.amount);
            break;
         case Item.ItemType.PrecisionLens:
            InventoryManager.Instance.TryAddPrecisionLens(job.amount);
            break;
      }
   }

   public void UnlockReduceCraftingTime()
   {
      isLabTier3Unlocked = true;
      Debug.Log("Node 3 in production path unlocked: crafting times reduced!");
   }
   private Transform FindChildByName(Transform parent, string name)
   {
      foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
      {
         if (child.name.Trim() == name) return child;
      }
      return null;
   }
   private void UpdateStagingUI()
   {
      if (craftButtonObject != null)
      {
         craftButtonObject.SetActive(stagingItems.Count > 0);
      }
      // SLOT 1: Shows the first item in the staging list
      if (craftSlot1 != null)
      {
         if (stagingItems.Count > 0)
         {
            craftSlot1.gameObject.SetActive(true);
            craftSlot1.sprite = Item.GetItemSprite(stagingItems[0]);
         }
         else
         {
            craftSlot1.gameObject.SetActive(false);
         }
      }

      // SLOT 2: Shows the second item 
      if (craftSlot2 != null)
      {
         if (forgeLevel >= 2 && stagingItems.Count > 1)
         {
            craftSlot2.gameObject.SetActive(true);
            craftSlot2.sprite = Item.GetItemSprite(stagingItems[1]);
         }
         else
         {
            craftSlot2.gameObject.SetActive(false);
         }
      }
   }

   public void CraftStagedItems()
   {
      if (hasCraftedThisTurn)
      {
         Debug.Log("Already crafted this turn!");
         if (errorPanel != null)
         {
            errorPanel.SetActive(true);
            Debug.Log("Warning activated for craft more than 1 item this turn.");
         }
         if (craftButtonObject != null)
            craftButtonObject.SetActive(false);
         return;
      }
      if (stagingItems.Count == 0) return;

      int totalCost = 0;
      bool isOverclocked = (currentOverclockToggle != null && currentOverclockToggle.isOn);

      // 1. Calculate Total Cost
      foreach (var type in stagingItems)
      {
         totalCost += GetItemCost(type);
      }

      // 2. Check Affordability
      if (InventoryManager.Instance.TrySpendOre(totalCost))
      {
         hasCraftedThisTurn = true;

         string popupMessage = "<b>Successfully Queued!</b>\n\n";

         // 3. Process Each Item
         foreach (var type in stagingItems)
         {
            int amount = isOverclocked ? 2 : 1;
            int turns = isMercenaryEngineerActive ? 0 : GetTurnsNeeded(type);

            isMercenaryEngineerActive = false;

            CraftingJob job = new CraftingJob();
            job.itemType = type;
            job.amount = amount;
            job.itemName = type.ToString();
            job.turnsRemaining = turns;

            activeJobs.Add(job);
            Debug.Log($"[Queued] {job.itemName} - {turns} turns remaining.");

            // Add the item to our popup text
            popupMessage += $"- {job.itemName} x {amount} in {turns} turns.\n";
         }

         if (activeQueuePanel != null && queueText != null)
         {
            queueText.text = popupMessage;
            activeQueuePanel.SetActive(true);
         }
         stagingItems.Clear();
         UpdateStagingUI();

         if (currentOverclockToggle != null) currentOverclockToggle.isOn = false;
      }
      else
      {
         Debug.Log("Not enough ore for all items!");
      }
   }
   private int GetItemCost(Item.ItemType type)
   {
      switch (type)
      {
         case Item.ItemType.CrudeTool:
            return CRUDE_TOOL_COST;
         case Item.ItemType.Harpoon:
            return HARPOON_COST;
         case Item.ItemType.PatchKit:
            return PATCH_KIT_COST;
         case Item.ItemType.PressureValve:
            return PRESSUREV_VALVE_COST;
         case Item.ItemType.DivingBell:
            return DIVING_BELL_COST;
         case Item.ItemType.Engine:
            return ENGINE_COST;
         case Item.ItemType.PrecisionLens:
            return PRECISION_LENS_COST;
         default:
            return 0;
      }
   }
   private void RefreshTurnValues()
   {
      int turnCount;

      // Get all the itemUI in the craft panel
      ItemUI[] allItems = craftPanel.GetComponentsInChildren<ItemUI>(true);

      foreach (ItemUI item in allItems)
      {
         Transform turnValueTransform = item.transform.parent.Find("TurnValue");

         if (turnValueTransform != null)
         {
            TextMeshProUGUI turnValue = turnValueTransform.GetComponent<TextMeshProUGUI>();
            turnCount = GetTurnsNeeded(item.itemType);
            turnValue.text = $"{turnCount}";
         }
      }
   }
   public void CloseQueuePopup()
   {
      if (activeQueuePanel != null)
      {
         activeQueuePanel.SetActive(false);
      }
   }

   public void TryActivateMercenaryEngineer() 
   {

      if (!isMercenaryEngineerActive && InventoryManager.Instance.TryUseMercenaryEngineer(1)) 
      {
         isMercenaryEngineerActive = true;
         ticker.ShowTicker("Mercenary engineer is active.", Color.green, TickerSystem.MessageTypes.ResultMessage);
      }
      else
         if(isMercenaryEngineerActive)
            ticker.ShowTicker("Mercenary engineer already active.", Color.red, TickerSystem.MessageTypes.ResultMessage);


      return;
   }
}