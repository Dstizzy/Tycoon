//using Codice.Client.BaseCommands.Import;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[System.Serializable]
public class CraftingJob
{
   public Item.ItemType itemType;
   public int amount;
   public int turnsRemaining;
   public string itemName;
   public int index;
}

public class ForgeManager : MonoBehaviour
{

   TickerSystem ticker;

   /* Constants */
   const int CRAFT_BUTTON = 1;
   const int INFO_BUTTON = 2;
   const int UPGRADE_BUTTON = 3;
   const int STARTING_LEVEL = 1;
   const int CRUDE_TOOL_COST = 10;
   const int HARPOON_COST = 25;
   const int PATCH_KIT_COST = 75;
   const int PRESSUREV_VALVE_COST = 50;
   const int DIVING_BELL_COST = 15;
   const int ENGINE_COST = 250;
   const int PRECISION_LENS_COST = 200;
   const int TIER_1 = 1;
   const int TIER_2 = 2;
   const int TIER_3 = 3;
   public const int LEVEL_2_PEARL_COST = 300;
   public const int LEVEL_3_PEARL_COST = 700;
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
   public PanelManager panelManager;
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

   [Header("Idle Indicator")]
   [SerializeField] private GameObject craftIdleIndicator;

   [Header("Forge Visuals")]
   [SerializeField] private SpriteRenderer buildingSpriteRenderer;
   [SerializeField] private List<Sprite> forgeLevelSprites;

   [Header("Crafting Progress")]
   [SerializeField] private ParticleSystem forgeSmoke;

   [Header("Tier 1 Backgrounds")]
   [SerializeField] private GameObject t1_bg_lvl1;
   [SerializeField] private GameObject t1_bg_lvl2;
   [SerializeField] private GameObject t1_bg_lvl3;

   [Header("Tier 2 Backgrounds")]
   [SerializeField] private GameObject t2_bg_lvl1;
   [SerializeField] private GameObject t2_bg_lvl2;
   [SerializeField] private GameObject t2_bg_lvl3;

   [Header("Tier 3 Backgrounds")]
   [SerializeField] private GameObject t3_bg_lvl1;
   [SerializeField] private GameObject t3_bg_lvl2;
   [SerializeField] private GameObject t3_bg_lvl3;

   [Header("Tier Panel Modifiers")]
   [SerializeField] private List<Toggle> overclockToggles;
   [SerializeField] private List<Toggle> mercenaryToggles;

   /* Private state variables */
   private Transform currentCraftWindow;
   private List<Item.ItemType> stagingItems = new List<Item.ItemType>();
   private List<bool> craftingItems = new List<bool>();
   private Toggle currentOverclockToggle;
   private Image craftSlot1;
   private Image craftSlot2;
   private Image craftSlot3;
   private GameObject craftButtonObject;


   public static event Action HandleTutorial;
   public static ForgeManager Instance { get; private set; }

   public static int forgeLevel = STARTING_LEVEL;
   public bool tutorialFunction = false; // Checks if the forge function has been explained in the tutorial
   private bool hasCraftedThisTurn = false;

   // Subscribe to the tutorial event when enabled to trigger the tutorial state change when the player reaches the forge tutorial step
   public void OnEnable()
   {
      TutorialManager.HandleForgeTutorial += ChangeTutorialState;
   }

   // Unsubscribe from the tutorial event when disabled to prevent memory leaks
   public void OnDisable()
   {
      TutorialManager.HandleForgeTutorial -= ChangeTutorialState;
   }

   // Changes the tutorial state to true, allowing the tutorial to progress and certain UI elements to appear in the forge
   private void ChangeTutorialState()
   {
      tutorialFunction = true;
   }

   private void Start()
   {
      SetCraftItemButtons();

      if (craftWindowTemplate.gameObject.scene.name != null)
         craftWindowTemplate.gameObject.SetActive(false);

      // Link to turn system
      if (TurnManager.Instance != null)
         TurnManager.OnTurnEnded += ProcessCraftingQueue;

      UpdateProgressVisuals();
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

      UpdateForgeSprites();
      UpdateSlotBackgrounds();
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
      Transform img3 = FindChildByName(windowTransform, "ItemImage3");

      if (img1 != null) craftSlot1 = img1.GetComponent<Image>();
      if (img2 != null) craftSlot2 = img2.GetComponent<Image>();
      if (img3 != null) craftSlot3 = img3.GetComponent<Image>();

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

      if(stagingItems.Count != activeJobs.Count)
         windowTransform.gameObject.SetActive(true);
   }


   /* Open the craft window when a item is selected*/
   public void OnCraftItemSelected(int tier, Item.ItemType itemType)
   {
      // 1. Determine Capacity based on Level
      int maxStagingSlots = forgeLevel;


      // 2. Logic: If list is full, clear it and start new. Otherwise, add to it.
      if (stagingItems.Count >= maxStagingSlots)
      {
         Debug.Log("You are at max capacity");
      }

      // 3. Add the item
      if (tutorialFunction && itemType == Item.ItemType.CrudeTool)
      {
         craftPanel.transform.Find("TutorialPart2").gameObject.SetActive(false);
         craftPanel.transform.Find("TutorialPart3").gameObject.SetActive(true);
      }

      if(stagingItems.Count < maxStagingSlots)
         stagingItems.Add(itemType);

      for(int i = 0; i < stagingItems.Count; i++)
      {
         if(i == stagingItems.Count - 1)
         {
            switch(i)
            {
               case 0:
                  AddXButton(0);
                  break;
               case 1:
                  AddXButton(1);
                  break;
               case 2:
                  AddXButton(2);
                  break;
            }
         }
      }


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
               if (currentCraftWindow != null)
                  Destroy(currentCraftWindow.gameObject);

               CloseForgePanel(CRAFT_BUTTON);
            });
         }
         else if (btn.name == "BackButton")
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
      if (InventoryManager.Instance.TrySpendPearl(upgradeCost))
      {
         // Perform the upgrade
         if (forgeLevel < ENDING_LEVEL)
         {
            forgeLevel += 1;
            UpdateForgeSprites();
            UpdateSlotBackgrounds();
         }

         if (forgeLevel == ENDING_LEVEL)
            InventoryManager.Instance.ForgeUpgradeIcon.gameObject.SetActive(false);

         forgeLevelText.text = "Level " + forgeLevel.ToString();
         Debug.Log($"Forge upgraded to level {forgeLevel}!");
         ticker.ShowTicker($"Forge upgraded to level {forgeLevel}!", Color.green, TickerSystem.MessageTypes.ResultMessage);
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

   private void UpdateForgeSprites()
   {
      int index = forgeLevel - 1;

      if (buildingSpriteRenderer != null && index < forgeLevelSprites.Count)
      {
         buildingSpriteRenderer.sprite = forgeLevelSprites[index];
         Debug.Log($"Forge Visuals Updated to Level {forgeLevel}");
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
      panelManager.OpenPanel(craftPanel.gameObject);
      if (tutorialFunction)
         craftPanel.transform.Find("TutorialPart1").gameObject.SetActive(true);

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
      RefreshModifiers();

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

      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(false);
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

      EventTrigger trigger = btnTransform.GetComponent<EventTrigger>();
      if (trigger == null) trigger = btnTransform.gameObject.AddComponent<EventTrigger>();

      trigger.triggers.Clear();

      if (!isUnlocked)
      {
         string blueprintName = "";
         if (requiredLevel == 2)
         {
            blueprintName = "Industrial Blueprint"; 
         }
         else if (requiredLevel == 3)
         {
            blueprintName = "Clockwork Blueprint"; 
         }

         EventTrigger.Entry enterEntry = new EventTrigger.Entry();
         enterEntry.eventID = EventTriggerType.PointerEnter;
         enterEntry.callback.AddListener((data) =>
         {
            if (ticker != null)
            {
               ticker.ShowTicker($"Needs to be unlocked by {blueprintName} in tradehut.", Color.yellow, TickerSystem.MessageTypes.ResultMessage);
            }
         });

         trigger.triggers.Add(enterEntry);
      }

      btn.onClick.RemoveAllListeners();
      if (isUnlocked)
      {
         btn.onClick.AddListener(() => OpenTierPanel(requiredLevel));
      }
   }

   private void ShowInfoPanel()
   {
      panelManager.OpenPanel(infoPanel.gameObject);

      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(false);
   }

   private void ShowUpgradePanel()
   {
      int upgradeCost = 0;
      string upgradeTitle = "";
      string upgradeExplanation = "";

      panelManager.OpenPanel(upgradePanel.gameObject);

      Transform titleTextTransform = upgradePanel.Find("Title");
      Transform mainTextTransform = upgradePanel.Find("UpgradePanelText");
      Transform pearlCostTransform = upgradePanel.Find("PearlCostText");
      Transform oreCostTransform = upgradePanel.Find("OreCostText");
      Transform explanationTransform = upgradePanel.Find("ExplanationText");

      TextMeshProUGUI title = titleTextTransform != null ? titleTextTransform.GetComponent<TextMeshProUGUI>() : null;
      TextMeshProUGUI upgradeText = mainTextTransform != null ? mainTextTransform.GetComponent<TextMeshProUGUI>() : null;
      TextMeshProUGUI pearlCostText = pearlCostTransform != null ? pearlCostTransform.GetComponent<TextMeshProUGUI>() : null;
      TextMeshProUGUI oreCostText = oreCostTransform != null ? oreCostTransform.GetComponent<TextMeshProUGUI>() : null;
      TextMeshProUGUI expText = explanationTransform != null ? explanationTransform.GetComponent<TextMeshProUGUI>() : null;

      if (forgeLevel == 1)
      {
         upgradeCost = 300;
         upgradeTitle = "REWARD: Unlock 2nd Crafting Slot";
         upgradeExplanation = "This upgrade increases your simultaneous crafting capacity to 2";
      }
      else if (forgeLevel == 2)
      {
         upgradeCost = 700;
         upgradeTitle = "REWARD: Unlock 3nd Crafting Slot";
         upgradeExplanation = "This upgrade increases your simultaneous crafting capacity to 2";
      }

      if (forgeLevel < ENDING_LEVEL)
      {
         if (title != null)
            title.text = $"LEVEL {forgeLevel + 1} UPGRADE";
         if (upgradeText != null)
            upgradeText.text = upgradeTitle;

         if (pearlCostText != null)
            pearlCostText.text = $"{upgradeCost}";

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

      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(false);
   }

   private void CloseCraftPanel()
   {
      int itemCheck = 0;

      panelManager.ClosePanel(craftPanel.gameObject);
      if (errorPanel != null) errorPanel.SetActive(false);

      if (activeQueuePanel != null)
         activeQueuePanel.SetActive(false);

      Debug.Log(craftingItems.Count);
      if (craftingItems.Count <= 0)
         stagingItems.Clear();
      else
      {
         foreach(var item in activeJobs)
         {
            switch(item.index)
            {
               case 0:
                  itemCheck += 1;
                  break;
               case 1:
                  itemCheck += 3;
                  break;
               case 2:
                  itemCheck += 5;
                  break;
            }
         }
         switch(itemCheck)
         {
            case 1:
               if(stagingItems.Count > 1)
                  stagingItems.RemoveAt(1);
               if(stagingItems.Count > 2)
                  stagingItems.RemoveAt(2);
               break;
            case 3:
               stagingItems.RemoveAt(0);
               if(stagingItems.Count > 2)
                  stagingItems.RemoveAt(2);
               break;
            case 4:
               if (stagingItems.Count > 2)
                  stagingItems.RemoveAt(2);
               break;
            case 5:
               stagingItems.RemoveAt(0);
               stagingItems.RemoveAt(1);
               break;
            case 6:
               stagingItems.RemoveAt(1);
               break;
            case 8:
               stagingItems.RemoveAt(0);
               break;
         }
      }
      craftPanel.transform.Find("xButton1").gameObject.SetActive(false);
      craftPanel.transform.Find("xButton1").gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
      craftPanel.transform.Find("xButton2").gameObject.SetActive(false);
      craftPanel.transform.Find("xButton2").gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
      craftPanel.transform.Find("xButton3").gameObject.SetActive(false);
      craftPanel.transform.Find("xButton3").gameObject.GetComponent<Button>().onClick.RemoveAllListeners();

      if (currentCraftWindow != null)
         Destroy(currentCraftWindow.gameObject);

      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(true);
   }
   private void CloseInfoPanel()
   {
      panelManager.ClosePanel(infoPanel.gameObject);

      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(true);
   }
   private void CloseUpgradePanel()
   {
      panelManager.ClosePanel(upgradePanel.gameObject);

      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(true);
   }

   public void CloseAllTierPanels()
   {
      craftPanel.transform.Find("xButton1").gameObject.SetActive(false);
      craftPanel.transform.Find("xButton2").gameObject.SetActive(false);
      craftPanel.transform.Find("xButton3").gameObject.SetActive(false);
      craftPanel.transform.Find("GearWheel1").gameObject.SetActive(false);
      craftPanel.transform.Find("GearWheel2").gameObject.SetActive(false);
      craftPanel.transform.Find("GearWheel3").gameObject.SetActive(false);
      tier1Panel.SetActive(false);
      tier2Panel.SetActive(false);
      tier3Panel.SetActive(false);
   }

   public void OpenTierPanel(int tier)
   {
      CloseAllTierPanels();

      if (currentCraftWindow != null)
         Destroy(currentCraftWindow.gameObject);

      Transform targetContainer = null;

      switch (stagingItems.Count)
      {
         case 1:
            if(activeJobs.Count < 1)
               craftPanel.transform.Find("xButton1").gameObject.SetActive(true);
            break;
         case 2:
            if (activeJobs.Count < 1)
               craftPanel.transform.Find("xButton1").gameObject.SetActive(true);
            if(activeJobs.Count < 2)
               craftPanel.transform.Find("xButton2").gameObject.SetActive(true);
            break;
         case 3:
            if (activeJobs.Count < 1)
               craftPanel.transform.Find("xButton1").gameObject.SetActive(true);
            if (activeJobs.Count < 2)
               craftPanel.transform.Find("xButton2").gameObject.SetActive(true);
            if (activeJobs.Count < 3)
               craftPanel.transform.Find("xButton3").gameObject.SetActive(true);
            break;
      }

      switch (craftingItems.Count)
      {
         case 1:
            craftPanel.transform.Find("GearWheel1").gameObject.SetActive(true);
            break;
         case 2:
            craftPanel.transform.Find("GearWheel1").gameObject.SetActive(true);
            craftPanel.transform.Find("GearWheel2").gameObject.SetActive(true);
            break;
         case 3:
            craftPanel.transform.Find("GearWheel1").gameObject.SetActive(true);
            craftPanel.transform.Find("GearWheel2").gameObject.SetActive(true);
            craftPanel.transform.Find("GearWheel3").gameObject.SetActive(true);
            break;
      }

      switch (tier)
      {
         case TIER_1:
            tier1Panel.SetActive(true);
            targetContainer = tier1Container; 
            if (tutorialFunction)
            {
               craftPanel.transform.Find("TutorialPart1").gameObject.SetActive(false);
               craftPanel.transform.Find("TutorialPart2").gameObject.SetActive(true);
            }
            break;

         case TIER_2:
            tier2Panel.SetActive(true);
            targetContainer = tier2Container; 
            break;

         case TIER_3:
            tier3Panel.SetActive(true);
            targetContainer = tier3Container; 
            break;
      }

      if (stagingItems.Count > 0 && targetContainer != null)
      {
         CreateCraftWindow(targetContainer);
      }
      RefreshModifiers();
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
            turns = 1;
            break;
         case Item.ItemType.Harpoon:
            turns = 1;
            break;
         case Item.ItemType.DivingBell:
            turns = 1;
            break;
         case Item.ItemType.PressureValve:
            turns = isLabTier3Unlocked ? 1 : 2;
            break;
         case Item.ItemType.PatchKit:
            turns = 1;
            break;
         case Item.ItemType.Engine:
            turns = 4;
            break;
         case Item.ItemType.PrecisionLens:
            turns = 3;
            break;
         default:
            turns = 1;
            break;
      }

      // 2. Level 3 Bonus: Reduce turn cost by 1
      //if (forgeLevel >= 3)
      //{
         //turns -= 1;
     // }

      // 3. Allow minimum 1 turn
      if (turns < 1)
         turns = 1;

      return turns;
   }

   private void ProcessCraftingQueue()
   {
      hasCraftedThisTurn = false;
      int jobCount;

      int maxParallelSlots = forgeLevel;

      List<string> finishedItems = new List<string>();

      for (jobCount = activeJobs.Count - 1; jobCount >= 0; jobCount--)
      {
         Debug.Log(activeJobs.Count);
         if (jobCount < maxParallelSlots)
         {
            CraftingJob job = activeJobs[jobCount];
            job.turnsRemaining--;

            if (job.turnsRemaining <= 0)
            {
               Debug.Log(stagingItems.Count);
               Debug.Log($"Crafting Complete: {job.itemName}");
               DeliverItem(job);
               RemoveCraftedItem(job);
               activeJobs.RemoveAt(jobCount);
               UpdateStagingUI();
               Debug.Log($"Crafting Complete: {job.itemName}");

               finishedItems.Add($"{job.amount}x {job.itemName}");
            }
         }
      }

      if (finishedItems.Count > 0)
      {
         string finalMessage = "Crafting Complete: " + string.Join(", ", finishedItems);

         if (ticker != null)
         {
            ticker.ShowTicker(finalMessage, Color.green, TickerSystem.MessageTypes.ResultMessage);
         }
      }

      UpdateProgressVisuals();
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
         if (activeJobs.Count > 0)
         {
            craftSlot1.gameObject.SetActive(true);
            craftSlot1.sprite = Item.GetItemSprite(activeJobs[0].itemType);
         }
         else
         {
            craftSlot1.gameObject.SetActive(false);
         }
      }

      // SLOT 2: Shows the second item 
      if (craftSlot2 != null)
      {
         if (forgeLevel >= 2 && activeJobs.Count > 1)
         {
            craftSlot2.gameObject.SetActive(true);
            craftSlot2.sprite = Item.GetItemSprite(activeJobs[1].itemType);
         }
         else
         {
            craftSlot2.gameObject.SetActive(false);
         }
      }

      // SLOT 3: Shows the third item
      if (craftSlot3 != null)
      {
         if (forgeLevel >= 3 && activeJobs.Count > 2)
         {
            craftSlot3.gameObject.SetActive(true);
            craftSlot3.sprite = Item.GetItemSprite(activeJobs[2].itemType);
         }
         else
         {
            craftSlot3.gameObject.SetActive(false);
         }
      }
   }

   public void CraftStagedItems()
   {
      List<int> activeIndexes = new List<int>();
      foreach(var job in activeJobs)
      {
         Debug.Log($"Active Job - Item: {job.itemName}, Turns Remaining: {job.turnsRemaining}, Index: {job.index}");
         activeIndexes.Add(job.index);
      }
      /*if (hasCraftedThisTurn)
      {
         Debug.Log("Already crafted this turn!");
         ticker.ShowTicker("You can only craft once per turn!", Color.red, TickerSystem.MessageTypes.ResultMessage);

         if (craftButtonObject != null)
            craftButtonObject.SetActive(false);

         CloseAllTierPanels();

         if (currentCraftWindow != null)
            Destroy(currentCraftWindow.gameObject);

         CloseForgePanel(CRAFT_BUTTON);

         return;
      }*/

      if (stagingItems.Count == 0) return;

      int totalCost = 0;

      // Check if ANY Overclock toggle is currently checked
      bool isOverclocked = false;
      foreach (Toggle t in overclockToggles) { if (t != null && t.isOn) isOverclocked = true; }

      // Check if ANY Mercenary toggle is currently checked
      bool useMercenary = false;
      foreach (Toggle t in mercenaryToggles) { if (t != null && t.isOn) useMercenary = true; }

      // Calculate Total Cost
      for(int i = 0; i < stagingItems.Count; i++)
      {
         if(!activeIndexes.Contains(i))
            totalCost += GetItemCost(stagingItems[i]);
      }

      // Check Affordability
      if (InventoryManager.Instance.TrySpendOre(totalCost))
      {
         //hasCraftedThisTurn = true;

         // NEW: Deduct the mercenary from inventory if used
         if (useMercenary)
         {
            InventoryManager.Instance.TryUseMercenaryEngineer(1);
         }

         string successMessage = "Successfully Queued: ";
         List<string> itemNames = new List<string>();

         Debug.Log(stagingItems.Count);
         // Process Each Item
         for (int i = 0; i < stagingItems.Count; i++)
         {
            int amount = isOverclocked ? 2 : 1;

            if (useMercenary)
            {
               CraftingJob instantJob = new CraftingJob { itemType = stagingItems[i], amount = amount, itemName = stagingItems[i].ToString() };
               DeliverItem(instantJob);

               Debug.Log($"[Instant Craft] {instantJob.itemName}");
               itemNames.Add($"{amount}x {instantJob.itemName}");
            }
            else
            {
               int turns = GetTurnsNeeded(stagingItems[i]);
               CraftingJob job = new CraftingJob { itemType = stagingItems[i], amount = amount, itemName = stagingItems[i].ToString(), turnsRemaining = turns };

               activeJobs.Add(job);
               craftingItems.Add(true);
               Debug.Log($"[Queued] {job.itemName} - {turns} turns remaining.");
               itemNames.Add($"{amount}x {job.itemName}");
            }
         }

         if (useMercenary)
         {
            successMessage = "Instant Craft Complete: " + string.Join(", ", itemNames);
         }
         else
         {
            successMessage += string.Join(", ", itemNames);
         }

         ticker.ShowTicker(successMessage, Color.green, TickerSystem.MessageTypes.ResultMessage);


         switch (craftingItems.Count)
         {
            case 1:
               craftPanel.transform.Find("GearWheel1").gameObject.SetActive(true);
               break;
            case 2:
               craftPanel.transform.Find("GearWheel1").gameObject.SetActive(true);
               craftPanel.transform.Find("GearWheel2").gameObject.SetActive(true);
               break;
            case 3:
               craftPanel.transform.Find("GearWheel1").gameObject.SetActive(true);
               craftPanel.transform.Find("GearWheel2").gameObject.SetActive(true);
               craftPanel.transform.Find("GearWheel3").gameObject.SetActive(true);
               break;
         }
         UpdateStagingUI();

         // Turn all toggles back off after crafting
         foreach (Toggle t in overclockToggles) { if (t != null) t.isOn = false; }
         foreach (Toggle t in mercenaryToggles) { if (t != null) t.isOn = false; }

         UpdateProgressVisuals();
         // ... (rest of method stays the same)


         CloseAllTierPanels();
         if (currentCraftWindow != null) Destroy(currentCraftWindow.gameObject);
         CloseForgePanel(CRAFT_BUTTON);
      }
      else
      {
         Debug.Log("Not enough ore for all items!");
         ticker.ShowTicker("Not enough ore to craft!", Color.red, TickerSystem.MessageTypes.ResultMessage);
      }
      if (tutorialFunction)
      {
         craftPanel.transform.Find("TutorialPart3").gameObject.SetActive(false);
         CloseCraftPanel();
         tutorialFunction = false;
         HandleTutorial?.Invoke();
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
         if (isMercenaryEngineerActive)
         ticker.ShowTicker("Mercenary engineer already active.", Color.red, TickerSystem.MessageTypes.ResultMessage);


      return;
   }
   public void UpdateProgressVisuals()
   {
      bool isWorking = activeJobs.Count > 0;

      if (craftIdleIndicator != null)
         craftIdleIndicator.SetActive(!isWorking);

      if (forgeSmoke != null)
      {
         forgeSmoke.gameObject.SetActive(isWorking);

         if (isWorking)
         {
            if (!forgeSmoke.isPlaying) forgeSmoke.Play();
         }
         else
         {
            forgeSmoke.Stop();
         }
      }
   }

   public void ShowQueueInTicker()
   {
      if (activeJobs.Count == 0 || ticker == null) return;

      string message = "CRAFTING: ";
      List<string> jobDetails = new List<string>();

      foreach (var job in activeJobs)
      {
         jobDetails.Add($"{job.amount}x {job.itemName} ({job.turnsRemaining}Turn left)");
      }

      message += string.Join(" | ", jobDetails);
      ticker.ShowTicker(message, Color.cyan, TickerSystem.MessageTypes.ResultMessage);
   }

   private void UpdateSlotBackgrounds()
   {
      if (t1_bg_lvl1) t1_bg_lvl1.SetActive(false);
      if (t1_bg_lvl2) t1_bg_lvl2.SetActive(false);
      if (t1_bg_lvl3) t1_bg_lvl3.SetActive(false);

      if (t2_bg_lvl1) t2_bg_lvl1.SetActive(false);
      if (t2_bg_lvl2) t2_bg_lvl2.SetActive(false);
      if (t2_bg_lvl3) t2_bg_lvl3.SetActive(false);

      if (t3_bg_lvl1) t3_bg_lvl1.SetActive(false);
      if (t3_bg_lvl2) t3_bg_lvl2.SetActive(false);
      if (t3_bg_lvl3) t3_bg_lvl3.SetActive(false);

      if (forgeLevel == 1)
      {
         if (t1_bg_lvl1) t1_bg_lvl1.SetActive(true);
         if (t2_bg_lvl1) t2_bg_lvl1.SetActive(true);
         if (t3_bg_lvl1) t3_bg_lvl1.SetActive(true);
      }
      else if (forgeLevel == 2)
      {
         if (t1_bg_lvl2) t1_bg_lvl2.SetActive(true);
         if (t2_bg_lvl2) t2_bg_lvl2.SetActive(true);
         if (t3_bg_lvl2) t3_bg_lvl2.SetActive(true);
      }
      else if (forgeLevel >= 3)
      {
         if (t1_bg_lvl3) t1_bg_lvl3.SetActive(true);
         if (t2_bg_lvl3) t2_bg_lvl3.SetActive(true);
         if (t3_bg_lvl3) t3_bg_lvl3.SetActive(true);
      }
   }
   private void RefreshModifiers()
   {
      bool hasMercenary = InventoryManager.Instance.mercenaryEngineerCount > 0;

  
      foreach (Toggle toggle in overclockToggles)
      {
         if (toggle != null)
         {
            toggle.gameObject.SetActive(isOverclockUnlocked);
            toggle.isOn = false; 
         }
      }

      foreach (Toggle t in mercenaryToggles)
      {
         if (t != null)
         {
            t.gameObject.SetActive(hasMercenary);
            t.isOn = false; 
         }
      }
   }

   public void AddXButton(int index)
   {
      switch (index)
      {
         case 0:
            craftPanel.transform.Find("xButton1").gameObject.SetActive(true);
            craftPanel.transform.Find("xButton1").gameObject.GetComponent<Button>().onClick.AddListener(() =>
            {
               stagingItems.RemoveAt(index);
               UpdateStagingUI();
               craftPanel.transform.Find("xButton1").gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
               if (stagingItems.Count > 0)
                  AddXButton(0);
               else
                  craftPanel.transform.Find("xButton1").gameObject.SetActive(false);

               if (stagingItems.Count > 1)
               {
                  AddXButton(1);
                  craftPanel.transform.Find("xButton3").gameObject.SetActive(false);
               }

               else
                  craftPanel.transform.Find("xButton2").gameObject.SetActive(false);
            });
            break;
         case 1:
            craftPanel.transform.Find("xButton2").gameObject.SetActive(true);
            craftPanel.transform.Find("xButton2").gameObject.GetComponent<Button>().onClick.AddListener(() =>
            {
               stagingItems.RemoveAt(index);
               UpdateStagingUI();
               craftPanel.transform.Find("xButton2").gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
               if (stagingItems.Count > 1)
               {
                  AddXButton(1);
                  craftPanel.transform.Find("xButton3").gameObject.SetActive(false);
               }
               else
                  craftPanel.transform.Find("xButton2").gameObject.SetActive(false);
            });
            break;
         case 2:
            craftPanel.transform.Find("xButton3").gameObject.SetActive(true);
            craftPanel.transform.Find("xButton3").gameObject.GetComponent<Button>().onClick.AddListener(() =>
            {
               stagingItems.RemoveAt(index);
               UpdateStagingUI();
               craftPanel.transform.Find("xButton3").gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
               craftPanel.transform.Find("xButton3").gameObject.SetActive(false);
            });
            break;
      }
   }

   public void RemoveCraftedItem(CraftingJob job)
   {
      Debug.Log($"This is the job index: {job.index}");
      Debug.Log($"This is the staging count before removal: {stagingItems.Count}");
      stagingItems.RemoveAt(job.index);
      craftingItems.RemoveAt(craftingItems.Count - 1);
      foreach (var item in activeJobs)
      {
         if (item.index > job.index)
            item.index -= 1;
      }
      Debug.Log(stagingItems.Count);
      switch (job.index)
      {
         case 0:
            if (stagingItems.Count > 1)
               craftPanel.transform.Find("GearWheel3").gameObject.SetActive(false);
            if (stagingItems.Count > 0)
               craftPanel.transform.Find("GearWheel2").gameObject.SetActive(false);
            if (stagingItems.Count >= 0)
               craftPanel.transform.Find("GearWheel1").gameObject.SetActive(false);
            break;
         case 1:
            if (stagingItems.Count > 1)
               craftPanel.transform.Find("GearWheel3").gameObject.SetActive(false);
            if (stagingItems.Count > 0)
               craftPanel.transform.Find("GearWheel2").gameObject.SetActive(false);
            break;
         case 2:
            craftPanel.transform.Find("GearWheel3").gameObject.SetActive(false);
            break;
      }
   }
}