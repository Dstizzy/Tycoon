/* Libraries and references                                                         */
using NUnit;
using NUnit.Framework;

using System;
using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using static Item;
using static TickerSystem;

public class InventoryManager : MonoBehaviour
{
   // Constants
   public const int MIN_PEARL_COUNT = 0,
                    MIN_CRYSTAL_COUNT = 0,
                    MIN_ORE_COUNT = 0,
                    MAX_PEARL_COUNT = 100000,
                    MAX_CRYSTAL_COUNT = 10000,
                    MAX_ORE_COUNT = 10000;


   public const int MAX_CRUDE_TOOL_COUNT = 100,
                    MAX_HARPOON_COUNT = 100,
                    MAX_PRESSURE_VALVE_COUNT = 100,
                    MAX_ENGINE_COUNT = 100,
                    MAX_RAW_ORE_COUNT = 100,
                    MAX_PATCH_KIT_COUNT = 100,
                    MAX_DIVING_BELL_COUNT = 100,
                    MAX_PRECISION_LENS_COUNT = 100,
                    MAX_MERCENARY_ENGINEER_COUNT = 3,

                    MIN_CRUDE_TOOL_COUNT = 0,
                    MIN_HARPOON_COUNT = 0,
                    MIN_PRESSURE_VALVE_COUNT = 0,
                    MIN_ENGINE_COUNT = 0,
                    MIN_RARE_ORE_CHUNK_COUNT = 0,
                    MIN_PATCH_KIT_COUNT = 0,
                    MIN_DIVING_BELL_COUNT = 0,
                    MIN_PRECISION_LENS_COUNT = 0,
                    MIN_MERCENARY_ENGINEER_COUNT = 0;

   public const int RESOURCE_SPACING = 35,
                    CRAFT_SPACING    = 33,
                    PEARL_POSITION   = 0,
                    ORE_POSITION     = PEARL_POSITION + 13,

                    CRUDE_TOOL_POSITION         = 0,
                    HARPOON_POSITION            = CRUDE_TOOL_POSITION + 10,
                    DIVING_BELL_POSITION        = HARPOON_POSITION + 10,
                    PRESSURE_VALVE_POSITION     = DIVING_BELL_POSITION + 10,
                    PATCH_KIT_POSITION          = CRUDE_TOOL_POSITION,
                    PRECISION_LENS_POSITION     = HARPOON_POSITION,
                    ENGINE_POSITION             = DIVING_BELL_POSITION,
                    MERCENARY_ENGINEER_POSITION = PRESSURE_VALVE_POSITION;

   public const string PEARL_TAG              = "Pearl",
                       CRYSTAL_TAG            = "Crystal",
                       ORE_TAG                = "Ore",
                       CRUDE_TOOL_TAG         = "Crude Tool",
                       HARPOON_TAG            = "Harpoon",
                       PATCH_KIT_TAG          = "Patch Kit",
                       PRESSURE_VALVE_TAG     = "Pressure Valve",
                       DIVING_BELL_TAG        = "Diving Bell",
                       ENGINE_TAG             = "Engine",
                       PRECISION_LENS_TAG     = "Precision Lens",
                       RAW_ORE_CHUNK_TAG      = "Raw Ore Chunk",
                       MERCENARY_ENGINEER_TAG = "Mercenary Engineer";

   // Holds a reference to the singleton instance of this class.
   public static InventoryManager Instance { get; set; }

   private TickerSystem ticker;

   // Inspector variables for UI elements.
   [SerializeField]
   private Transform InventoryPanel,
                     ResourcePanel,
                     ResourceWindow,
                     CraftsPanel,
                     CraftWindow;

   public Image ForgeUpgradeIcon,
                OreRefineryUpgradeIcon,
                ExplorationUnitUpgradeIcon;

   private TextMeshProUGUI PearlCountText,
                           CrystalCountText,
                           OreCountText,
                           CrudeToolCountText,
                           HarpoonCountText,
                           PatchKitCountText,
                           PressureValveCountText,
                           DivingBellCountText,
                           EngineCountText,
                           PrecisionLensCountText,
                           RaWOreChunkCountText,
                           MercenaryEngineerCountText;
   public PanelManager     panelManager;


   /* Public properties                               ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½   ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½  */
   public int pearlCount { get; private set; }
   public int oreCount { get; private set; }

   public int crudeToolCount { get; private set; }
   public int harpoonCount { get; private set; }
   public int patchKitCount { get; private set; }
   public int pressureValveCount { get; private set; }
   public int divingBellCount { get; private set; }
   public int engineCount { get; private set; }
   public int precisionLensCount { get; private set; }
   public int rawOreChunkCount { get; private set; }
   public int mercenaryEngineerCount { get; private set; }

   public int tempPearlCount { get; private set; }
   public int tempOreCount { get; private set; }

   public int tempCrudeToolCount { get; private set; }
   public int tempHarpoonCount { get; private set; }
   public int tempPatchKitCount   { get; private set; }
   public int tempPressureValveCount { get; private set; }
   public int tempDivingBellCount { get; private set; }
   public int tempEngineCount { get; private set; }
   public int tempPrecisionLensCount { get; private set; }
   public int tempRawOreChunkCount { get; private set; }
   public int tempMercenaryEngineerCount { get; private set; }

   /* Private variables ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½      */
   private Transform currentResource,
                     currentCraft;

   public bool tutorialFunction = false;

   /* Delegate for when the pearl count changes. ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½   */
   public Action<int> OnPearlCountChanged;

   /* Delegate for when the crystal count changes. ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½   */
   public Action<int> OnCrystalCountChanged;

   /* Delegate for when the crystal count changes. ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½   */
   public Action<int> OnOreCountChanged;

   public List<Transform> InventoryItems { get; private set; }


   /* Sets up the singleton instance and initializes the inventory panel state.    */
   private void Awake()
   {
      if (Instance != null && Instance != this)
      {
         // If an instance already exists, tell it to update its UI for the new scene
         Instance.RefreshUIReferences();
         Destroy(this.gameObject);
         return;
      }

      Instance = this;
      DontDestroyOnLoad(this.gameObject);

      // Initial data setup...
      InventoryItems = new List<Transform>();

      if (InventoryPanel == null)
         Debug.LogError("Inventory Panel is not assigned in the Inspector!");
      else
         InventoryPanel.gameObject.SetActive(false);

      if (CraftsPanel == null)
         Debug.LogError("Crafts Panel is not assigned in the Inspector!");
      else
         CraftsPanel.gameObject.SetActive(false);

      if (ResourcePanel == null)
         Debug.LogError("Resource Panel is not assigned in the Inspector!");
      else
         ResourcePanel.gameObject.SetActive(false);

      if (ResourceWindow == null)
         Debug.LogError("Resource Info Window is not assigned in the Inspector!");
      else
         ResourceWindow.gameObject.SetActive(false);

      if (CraftWindow == null)
         Debug.Log("Craft window is ont assigned in the inspector");
      else
         CraftWindow.gameObject.SetActive(false);

      pearlCount         = 0;
      oreCount           = 10;
      crudeToolCount     = 0;
      harpoonCount       = 1;
      patchKitCount      = 1;
      pressureValveCount = 0;
      divingBellCount    = 0;
      precisionLensCount = 0;
      engineCount        = 0;
   }

   public void RefreshUIReferences()
   {
      // Find the new PanelManager in the current scene
      panelManager = FindFirstObjectByType<PanelManager>();

      // You need to find the new UI root (the Inventory Canvas/Panel) 
      // and re-assign the child references.
      GameObject uiRoot = GameObject.Find("InventoryPanel"); // Make sure your object name matches
      if (uiRoot != null)
      {
         InventoryPanel = uiRoot.transform;
         ResourcePanel = InventoryPanel.Find("ResourcePanel");
         CraftsPanel = InventoryPanel.Find("CraftsPanel");
         // ... and so on for all Inspector variables
      }
   }

   /* Creates the display elements for Pearls and Crystals on the inventory panel. */
   private void Start()
   {
      CreateResource(Resources.GetResourceSprite(Resources.ResourceType.Pearl), PEARL_POSITION, PEARL_TAG);
      CreateResource(Resources.GetResourceSprite(Resources.ResourceType.Ore), ORE_POSITION, ORE_TAG);

      CreateCraft(GetItemSprite(ItemType.CrudeTool), CRUDE_TOOL_POSITION, CRUDE_TOOL_TAG);
      CreateCraft(GetItemSprite(ItemType.Harpoon), HARPOON_POSITION, HARPOON_TAG);
      CreateCraft(GetItemSprite(ItemType.DivingBell), DIVING_BELL_POSITION, DIVING_BELL_TAG);
      
      //CreateCraft(GetItemSprite(ItemType.PressureValve), PRESSURE_VALVE_POSITION, PRESSURE_VALVE_TAG);
      //CreateCraft(GetItemSprite(ItemType.PatchKit), PATCH_KIT_POSITION, PATCH_KIT_TAG, -430);
      //
      //CreateCraft(GetItemSprite(ItemType.PrecisionLens), PRECISION_LENS_POSITION, PRECISION_LENS_TAG, -430);
      //CreateCraft(GetItemSprite(ItemType.Engine), ENGINE_POSITION, ENGINE_TAG, -430);
      //CreateCraft(GetItemSprite(ItemType.MercenaryEngineer), MERCENARY_ENGINEER_POSITION, MERCENARY_ENGINEER_TAG, -430);

      //if (PatchKitCountText != null)
      //   PatchKitCountText.transform.parent.gameObject.SetActive(false);

      if (TickerSystem.Instance)
      {
         Debug.Log("Instance is set");
         ticker = TickerSystem.Instance;
      }
      else
         Debug.LogError("No ticker");
   }

   public void ChangeToWalkthrough()
   {
      tempPearlCount = pearlCount;
      tempOreCount = oreCount;
      tempCrudeToolCount = crudeToolCount;
      tempHarpoonCount = harpoonCount;
      tempPatchKitCount = patchKitCount;
      tempPressureValveCount = pressureValveCount;
      tempDivingBellCount = divingBellCount;
      tempEngineCount = engineCount;
      tempPrecisionLensCount = precisionLensCount;
      tempRawOreChunkCount = rawOreChunkCount;
      tempMercenaryEngineerCount = mercenaryEngineerCount;

      pearlCount = 0;
      oreCount = 0;
      crudeToolCount = 0;
      harpoonCount = 0;
      patchKitCount = 0;
      pressureValveCount = 0;
      divingBellCount = 0;
      engineCount = 0;
      precisionLensCount = 0;
      rawOreChunkCount = 0;
      mercenaryEngineerCount = 0;

      OnPearlCountChanged?.Invoke(pearlCount);
      OnOreCountChanged?.Invoke(oreCount);

   }

   public void ChangeToNormal()
   {
      pearlCount = tempPearlCount;
      oreCount = tempOreCount;
      crudeToolCount = tempCrudeToolCount;
      harpoonCount = tempHarpoonCount;
      patchKitCount = tempPatchKitCount;
      pressureValveCount = tempPressureValveCount;
      divingBellCount = tempDivingBellCount;
      engineCount = tempEngineCount;
      precisionLensCount = tempPrecisionLensCount;
      rawOreChunkCount = tempRawOreChunkCount;
      mercenaryEngineerCount = tempMercenaryEngineerCount;
   }
   /* Creates and positions a resource display element in the inventory panel. ï¿½   */
   public void CreateResource(Sprite resourceSprite, int positionIndex, string resourceTag)
   {
      Transform resourceTransform,
                resourceContainer = ResourcePanel.Find("ResourceContainer").GetComponent<Transform>(),
                resourceTemplate = resourceContainer.Find("ResourceTemplate").GetComponent<Transform>();

      Button resourceWindowButton;
      RectTransform resourceRectTransform;
      int resourceCount;


      resourceTemplate.gameObject.SetActive(false);

      switch (resourceTag)
      {
         case PEARL_TAG:
            resourceCount = pearlCount;
            break;
         case ORE_TAG:
            resourceCount = oreCount;
            break;
         case RAW_ORE_CHUNK_TAG:
            resourceCount = rawOreChunkCount;
            break;
         default:
            Debug.LogError("Unknown resource tag: " + resourceTag);
            return;
      }

      /* Instantiate the resource template and set its position in the container   */
      /* Transform of the newly created resource UI element. ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½     */
      resourceTransform = Instantiate(resourceTemplate, resourceContainer);
      if (resourceTransform == null)
         Debug.LogError("Error 2");

      /* RectTransform for positioning the new resource UI element. ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ï¿½     */
      resourceRectTransform = resourceTransform.GetComponent<RectTransform>();

      resourceTransform.tag = resourceTag;

      /* Places the new resource entry in a horizontal row inside the inventory    */
      resourceRectTransform.anchoredPosition = new Vector2(RESOURCE_SPACING * positionIndex, 0);

      Transform visualGroup = resourceTransform.Find("VisualGroup");

      /* Populate the resource components with item-specific data                  */
      visualGroup.Find("ResourceCount").GetComponent<TextMeshProUGUI>().text = " x" + resourceCount.ToString();
      resourceWindowButton = visualGroup.Find("ResourceButton").GetComponent<Button>();
      resourceWindowButton.image.sprite = resourceSprite;

      /* Dynamically add listeners to the button, which creates the resource       */
      /* information window                                                        */
      resourceWindowButton.onClick.AddListener(() => {
         AudioManager.Instance.PlayClick(); 
         CreateResourceWindow(resourceSprite, resourceTag);
      });

      switch (resourceTag)
      {
         case PEARL_TAG:
            PearlCountText = visualGroup.Find("ResourceCount").GetComponent<TextMeshProUGUI>();
            break;
         case CRYSTAL_TAG:
            CrystalCountText = visualGroup.Find("ResourceCount").GetComponent<TextMeshProUGUI>();
            break;
         case ORE_TAG:
            OreCountText = visualGroup.Find("ResourceCount").GetComponent<TextMeshProUGUI>();
            break;
         case RAW_ORE_CHUNK_TAG:
            RaWOreChunkCountText = visualGroup.Find("ResourceCount").GetComponent<TextMeshProUGUI>();
            break;
         default:
            Debug.LogError("Unknown resource tag: " + resourceTag);
            break;
      }

      InventoryItems.Add(resourceTransform);

      resourceTransform.gameObject.SetActive(true);
   }

   public void CreateCraft(Sprite craftSprite, float positionIndex, string craftTag, int verticalIndex = 0)
   {
      Transform craftsContainer = CraftsPanel.Find("CraftContainer").GetComponent<Transform>(),
                craftTemplate = craftsContainer.Find("CraftTemplate").GetComponent<Transform>();
      
      Button craftWindowButton;
      int    craftCount;

      craftTemplate.gameObject.SetActive(false);

      switch (craftTag)
      {
         case CRUDE_TOOL_TAG:
            craftCount = crudeToolCount;
            break;
         case HARPOON_TAG:
            craftCount = harpoonCount;
            break;
         case PATCH_KIT_TAG:
            craftCount = patchKitCount;
            break;
         case PRESSURE_VALVE_TAG:
            craftCount = pressureValveCount;
            break;
         case DIVING_BELL_TAG:
            craftCount = divingBellCount;
            break;
         case ENGINE_TAG:
            craftCount = engineCount;
            break;
         case PRECISION_LENS_TAG:
            craftCount = precisionLensCount;
            break;
         case MERCENARY_ENGINEER_TAG:
            craftCount = mercenaryEngineerCount;
            break;
         default:
            craftCount = 0;
            Debug.LogError("Unknown craft tag: " + craftTag);
            break;
      }

      /* Instantiate the craft template and set its position in the container.     */
      /* Transform of the newly created resource UI element. ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½     */
      Transform craftTransform = Instantiate(craftTemplate, craftsContainer);

      craftTemplate.gameObject.SetActive(false);

      /* RectTransform for positioning the new resource UI element.                */
      RectTransform craftRectTransform = craftTransform.GetComponent<RectTransform>();

      craftTransform.tag = craftTag;

      /* Places the new resource entry in a horizontal row inside the inventory    */
      craftRectTransform.anchoredPosition = new Vector2(CRAFT_SPACING * positionIndex, verticalIndex);

      Transform visualGroup = craftTransform.Find("VisualGroup");

      /* Populate the resource components with item-specific data                  */
      visualGroup.Find("CraftCount").GetComponent<TextMeshProUGUI>().text = " x" + craftCount.ToString();
      craftWindowButton = visualGroup.Find("CraftButton").GetComponent<Button>();
      craftWindowButton.image.sprite = craftSprite;

      switch (craftTag)
      {
         case CRUDE_TOOL_TAG:
            CrudeToolCountText = visualGroup.Find("CraftCount").GetComponent<TextMeshProUGUI>();
            break;
         case HARPOON_TAG:
            HarpoonCountText = visualGroup.Find("CraftCount").GetComponent<TextMeshProUGUI>();
            break;
         case PATCH_KIT_TAG:
            PatchKitCountText = visualGroup.Find("CraftCount").GetComponent<TextMeshProUGUI>();
            break;
         case PRESSURE_VALVE_TAG:
            PressureValveCountText = visualGroup.Find("CraftCount").GetComponent<TextMeshProUGUI>();
            break;
         case DIVING_BELL_TAG:
            DivingBellCountText = visualGroup.Find("CraftCount").GetComponent<TextMeshProUGUI>();
            break;
         case ENGINE_TAG:
            EngineCountText = visualGroup.Find("CraftCount").GetComponent<TextMeshProUGUI>();
            break;
         case PRECISION_LENS_TAG:
            PrecisionLensCountText = visualGroup.Find("CraftCount").GetComponent<TextMeshProUGUI>();
            break;
         case MERCENARY_ENGINEER_TAG:
            MercenaryEngineerCountText = visualGroup.Find("CraftCount").GetComponent<TextMeshProUGUI>();
            break;
         default:
            Debug.LogError("Unknown craft");
            break;
      }

      /* Dynamically add listeners to the buttons, which creates the craft window  */
      craftWindowButton.onClick.AddListener(() => {
         //AudioManager.Instance.PlayClick(); 
         CreateCraftWindow(craftSprite, craftTag);
      });

      InventoryItems.Add(craftTransform);
      craftTransform.gameObject.SetActive(true);
   }

   /* Creates and populates the resource information window                        */
   private void CreateResourceWindow(Sprite resourceSprite, string resourceTag)
   {
      Transform resourceWindowContainer = ResourceWindow.Find("ResourceWindowContainer").GetComponent<Transform>(),
                resourceWindowTemplate = resourceWindowContainer.Find("ResourceWindowTemplate").GetComponent<Transform>();
      int resourceCount = 0;
      string resourceInfo = "";

      resourceWindowTemplate.gameObject.SetActive(false);

      /* Instantiate the resource template and set its position in the container.  */
      /* Transform of the newly created resource UI element.                       */
      Transform resourceTransform = Instantiate(resourceWindowTemplate, resourceWindowContainer);
      RectTransform resourceRectTransform = resourceTransform.GetComponent<RectTransform>();

      /* Destroys the current resource in the window if it exists.                 */
      if (currentResource != null)
      {
         Destroy(currentResource.gameObject);
         currentResource = null;
      }

      resourceTransform.tag = resourceTag;

      switch (resourceTag)
      {
         case PEARL_TAG:
            resourceCount = pearlCount;
            resourceInfo = Resources.GetResourceDescription(Resources.ResourceType.Pearl);
            break;
         case ORE_TAG:
            resourceCount = oreCount;
            resourceInfo = Resources.GetResourceDescription(Resources.ResourceType.Ore);
            break;
         case RAW_ORE_CHUNK_TAG:
            resourceCount = rawOreChunkCount;
            resourceInfo = GetItemDescription(ItemType.RawOreChunk);
            break;
         default:
            Debug.LogError("Unknown item tag for resource window.");
            break;
      }

      /* Populate the resource properties (value, name, sprite).                   */
      resourceTransform.Find("ResourceImage").GetComponent<Image>().sprite = resourceSprite;
      resourceTransform.Find("ResourceCount").GetComponent<TextMeshProUGUI>().text = "  x" + resourceCount.ToString();
      resourceTransform.Find("ResourceName").GetComponent<TextMeshProUGUI>().text = resourceTag;
      resourceTransform.Find("ResourceInfo").GetComponent<TextMeshProUGUI>().text = resourceInfo;

      currentResource = resourceTransform;
      resourceTransform.gameObject.SetActive(true);
      ShowResourceWindow();
   }

   /* Creates and populates the craft information window                           */
   private void CreateCraftWindow(Sprite crafteSprite, string craftTag)
   {
      Transform craftWindowContainer = CraftWindow.Find("CraftWindowContainer").GetComponent<Transform>(),
                craftWindowTemplate = craftWindowContainer.Find("CraftWindowTemplate").GetComponent<Transform>();
      int craftCount = 0;
      string craftInfo = "";

      craftWindowTemplate.gameObject.SetActive(false);

      /* Instantiate the resource template and set its position in the container.  */
      /* Transform of the newly created resource UI element.                       */
      Transform craftTransform = Instantiate(craftWindowTemplate, craftWindowContainer);
      RectTransform craftRectTransform = craftTransform.GetComponent<RectTransform>();

      /* Destroys the current craft in the window if it exists.                    */
      if (currentCraft != null)
      {
         Destroy(currentCraft.gameObject);
         currentCraft = null;
      }

      if (tutorialFunction && craftTag == CRUDE_TOOL_TAG)
      {
         InventoryPanel.transform.Find("Arrow3").gameObject.SetActive(true);
         InventoryPanel.transform.Find("Arrow2").gameObject.SetActive(false);
         InventoryPanel.transform.Find("TutorialText").gameObject.SetActive(false);
      }

      craftTransform.tag = craftTag;

      switch (craftTag)
      {
         case CRUDE_TOOL_TAG:
            craftCount = crudeToolCount;
            craftInfo = GetItemDescription(ItemType.CrudeTool);
            break;
         case HARPOON_TAG:
            craftCount = harpoonCount;
            craftInfo = GetItemDescription(ItemType.Harpoon);
            break;
         case PATCH_KIT_TAG:
            craftCount = patchKitCount;
            craftInfo = GetItemDescription(ItemType.PatchKit);
            break;
         case PRESSURE_VALVE_TAG:
            craftCount = pressureValveCount;
            craftInfo = GetItemDescription(ItemType.PressureValve);
            break;
         case DIVING_BELL_TAG:
            craftCount = divingBellCount;
            craftInfo = GetItemDescription(ItemType.DivingBell);
            break;
         case ENGINE_TAG:
            craftCount = engineCount;
            craftInfo = GetItemDescription(ItemType.Engine);
            break;
         case PRECISION_LENS_TAG:
            craftCount = precisionLensCount;
            craftInfo = GetItemDescription(ItemType.PrecisionLens);
            break;
         case RAW_ORE_CHUNK_TAG:
            craftCount = rawOreChunkCount;
            craftInfo = GetItemDescription(ItemType.RawOreChunk);
            break;
         case MERCENARY_ENGINEER_TAG:
            craftCount = mercenaryEngineerCount;
            craftInfo = GetItemDescription(ItemType.MercenaryEngineer);
            break;
         default:
            Debug.LogError("Unknown item tag for resource window.");
            break;
      }

      /* Populate craft components with item-specific data (value, name, sprite).  */
      craftTransform.Find("CraftImage").GetComponent<Image>().sprite = crafteSprite;
      craftTransform.Find("CraftCount").GetComponent<TextMeshProUGUI>().text = "  x" + craftCount.ToString();
      craftTransform.Find("CraftName").GetComponent<TextMeshProUGUI>().text = craftTag;
      craftTransform.Find("CraftInfo").GetComponent<TextMeshProUGUI>().text = craftInfo;

      currentCraft = craftTransform;
      craftTransform.gameObject.SetActive(true);
      ShowCraftWindow();
   }

   public bool TryAddPearl(int pearlAmount)
   {
      bool isSuccess = false;

      if (pearlCount >= MAX_PEARL_COUNT)
      {
         ticker.ShowTicker("Pearl count is at maximum!", Color.red, MessageTypes.ResultMessage);
         Debug.LogError("Pearl count is at maximum!");
      }
      else
      {
         if ((pearlCount + pearlAmount) > MAX_PEARL_COUNT)
         {
            ticker.ShowTicker("Cannot add pearls - would exceed maximum!", Color.red, MessageTypes.ResultMessage);
            Debug.LogError("Pearl count is at maximum!");
         }
         else
         {
            pearlCount += pearlAmount;
            isSuccess = true;
         }
      }

      CheckUpgradeResources();
      OnPearlCountChanged?.Invoke(pearlCount);
      PearlCountText.text = " x" + pearlCount.ToString();

      return isSuccess;
   }

   public bool TrySpendPearl(int pearlAmount)
   {
      bool isSuccess = false;

      if (pearlCount <= MIN_PEARL_COUNT)
      {
         ticker.ShowTicker("Pearl count is at minimum! Sell items in the Trade Hut.", Color.red, MessageTypes.ResultMessage);
         Debug.LogError("Pearl count is at minimum!");
      }
      else
      {
         if (pearlCount < pearlAmount)
         {
            Debug.LogError("Not enough pearls to spend!");
            ticker.ShowTicker($"Cannot spend pearls, only {pearlCount} available! Sell items in the Trade Hut.", Color.red, MessageTypes.ResultMessage);
         }
         else
         {
            pearlCount -= pearlAmount;
            isSuccess = true;
         }
      }

      CheckUpgradeResources();
      OnPearlCountChanged?.Invoke(pearlCount);
      PearlCountText.text = " x" + pearlCount.ToString();

      return isSuccess;
   }

   public bool TryAddOre(int oreAmount)
   {
      bool isSuccess = false;

      if (oreCount >= MAX_ORE_COUNT)
      {
         Debug.LogError("Ore count is at maximum!");
         ticker.ShowTicker($"Ore count is at maximum!", Color.red, MessageTypes.ResultMessage);
      }
      else
         if ((oreCount + oreAmount) > MAX_ORE_COUNT)
      {
         Debug.LogError("Ore count is at maximum!");
         ticker.ShowTicker($"Cannot add ore - would exceed the maximum!", Color.red, MessageTypes.ResultMessage);
      }
      else
      {
         oreCount += oreAmount;
         isSuccess = true;
      }

      CheckUpgradeResources();
      OnOreCountChanged?.Invoke(oreCount);
      OreCountText.text = " x" + oreCount.ToString();

      return isSuccess;
   }

   public bool TrySpendOre(int oreAmount)
   {
      bool isSuccess = false;

      if (oreCount <= MIN_ORE_COUNT)
      {
         Debug.LogError("Ore count is at minimum!");
         ticker.ShowTicker($"Ore count is at minimum!", Color.red, MessageTypes.ResultMessage);

      }
      else
      {
         if (oreCount < oreAmount)
         {
            Debug.LogError("Not enough ore to spend!");
            ticker.ShowTicker($"Cannot spend ore, only {oreCount} avaliable!", Color.red, MessageTypes.ResultMessage);
         }
         else
         {
            oreCount -= oreAmount;
            isSuccess = true;
         }
      }

      CheckUpgradeResources();
      OnOreCountChanged?.Invoke(oreCount);
      OreCountText.text = " x" + oreCount.ToString();

      return isSuccess;
   }
   public bool TryAddCrudeTool(int crudeToolAmount)
   {
      TextMeshProUGUI newCrudeToolCount;
      bool isSuccess = false;

      if (crudeToolCount >= MAX_CRUDE_TOOL_COUNT)
      {
         Debug.LogError("Crude Tool count is at maximum!");
         ticker.ShowTicker($"Crude Tool count is at maximum!", Color.red, MessageTypes.ResultMessage);
         return isSuccess;
      }
      else
         if ((crudeToolCount + crudeToolAmount) > MAX_CRUDE_TOOL_COUNT)
      {
         Debug.LogError("Crystal count is at maximum!");
         ticker.ShowTicker($"Cannot add crude crude tools - would exceed maximum!", Color.red, MessageTypes.ResultMessage);
         return isSuccess;
      }
      else
      {
         isSuccess = true;
         crudeToolCount += crudeToolAmount;
      }

      newCrudeToolCount = TradeHutManager.Instance.SellItems.Find(item => item.CompareTag(CRUDE_TOOL_TAG)).Find("ItemCount").GetComponent<TextMeshProUGUI>();
      newCrudeToolCount.text = " x" + crudeToolCount.ToString();
      CrudeToolCountText.text = " x" + crudeToolCount.ToString();

      return isSuccess;
   }

   public bool TryUseCrudeTool(int crudeToolAmount)
   {
      TextMeshProUGUI newCrudeToolCount;
      bool isSuccess = false;

      if (crudeToolCount <= MIN_CRUDE_TOOL_COUNT)
      {
         Debug.LogError("Crude tool count is at minimum!");
         ticker.ShowTicker($"Crude Tool count is at minimum!", Color.red, MessageTypes.ResultMessage);

         return isSuccess;
      }
      else
         if (crudeToolCount < crudeToolAmount)
      {
         Debug.LogError("Not enough crude tools!");
         ticker.ShowTicker($"Cannot use crude tools, only {crudeToolCount} avaliable! Craft Crude Tools in Forge (Tier 1).", Color.red, MessageTypes.ResultMessage);

         return isSuccess;
      }
      else
      {
         isSuccess = true;
         crudeToolCount -= crudeToolAmount;

      }

      newCrudeToolCount = TradeHutManager.Instance.SellItems.Find(item => item.CompareTag(CRUDE_TOOL_TAG)).Find("ItemCount").GetComponent<TextMeshProUGUI>();
      newCrudeToolCount.text  = " x" + crudeToolCount.ToString();
      CrudeToolCountText.text = " x" + crudeToolCount.ToString();

      return isSuccess;
   }

   public bool TryAddHarpoon(int harpoonAmount)
   {
      TextMeshProUGUI newHarpoonCount;
      bool isSuccess = false;

      if (harpoonCount >= MAX_HARPOON_COUNT)
      {
         Debug.LogError("Harpoon count is at maximum!");
         ticker.ShowTicker($"Harpoon count is at maximum!", Color.red, MessageTypes.ResultMessage);
         return isSuccess;
      }
      else
         if ((harpoonCount + harpoonAmount) > MAX_HARPOON_COUNT)
      {
         Debug.LogError("Harpoon count is at maximum!");
         ticker.ShowTicker($"Cannot add harpoons - would exceed maximum!", Color.red, MessageTypes.ResultMessage);
         return isSuccess;
      }
      else
      {
         isSuccess = true;
         harpoonCount += harpoonAmount;
      }

      newHarpoonCount = TradeHutManager.Instance.SellItems.Find(item => item.CompareTag(HARPOON_TAG)).Find("ItemCount").GetComponent<TextMeshProUGUI>();
      newHarpoonCount.text = " x" + harpoonCount.ToString();
      HarpoonCountText.text = " x" + harpoonCount.ToString();

      return isSuccess;
   }

   public bool TryUseHarpoon(int harpoonAmount)
   {
      TextMeshProUGUI newHarpoonCount;
      bool isSuccess = false;

      if (harpoonCount <= MIN_HARPOON_COUNT)
      {
         Debug.LogError("Harpoon count is at minimum!");
         ticker.ShowTicker($"Harpoon count is at minimum! Craft a harpoon in Forge (Tier 1).", Color.red, MessageTypes.ResultMessage);
         return isSuccess;
      } 
      else 
      { 
         if (harpoonCount < harpoonAmount)
         {
            Debug.LogError("Not enough harpoons!");
            ticker.ShowTicker($"Cannot use harpoons, only {harpoonCount} available! Craft a harpoon in Forge (Tier 1).", Color.red, MessageTypes.ResultMessage);
            return isSuccess;
         }
         else
         {
            harpoonCount -= harpoonAmount;
            isSuccess = true;
         }
      }

      newHarpoonCount = TradeHutManager.Instance.SellItems.Find(item => item.CompareTag(HARPOON_TAG)).Find("ItemCount").GetComponent<TextMeshProUGUI>();
      newHarpoonCount.text = " x" + harpoonCount.ToString();
      HarpoonCountText.text = " x" + harpoonCount.ToString();

      return isSuccess;
   }

   public bool TryAddDivingBell(int divingBellAmount)
   {
      TextMeshProUGUI newDivingBellCount;
      bool isSuccess = false;

      if (divingBellCount >= MAX_DIVING_BELL_COUNT)
      {
         ticker.ShowTicker($"Diving Bell count is at maximum!", Color.red, MessageTypes.ResultMessage);
         return isSuccess;
      }

      isSuccess = true;
      divingBellCount += divingBellAmount;

      if (DivingBellCountText != null)
      {
         newDivingBellCount       = TradeHutManager.Instance.SellItems.Find(item => item.CompareTag(DIVING_BELL_TAG)).Find("ItemCount").GetComponent<TextMeshProUGUI>();
         newDivingBellCount.text  = " x" + divingBellCount.ToString();
         DivingBellCountText.text = " x" + divingBellCount.ToString();
      }

      return isSuccess;
   }

   public bool TryUseDivingBell(int divingBellAmount)
   {
      TextMeshProUGUI newDivingBellCount;
      bool isSuccess = false;

      if (divingBellCount <= MIN_DIVING_BELL_COUNT)
      {
         Debug.LogError("Diving Bell count is at minimum!");
         ticker.ShowTicker($"Diving Bell count is at minimum! Craft a diving bell in Forge (Tier 1).", Color.red, MessageTypes.ResultMessage);
         return isSuccess;
      }
      else
      {
         if (divingBellCount < divingBellAmount)
         {
            Debug.LogError("Not enough diving bells!");
            ticker.ShowTicker($"Cannot use diving bells, only {divingBellCount} available! Craft a diving bell in Forge (Tier 1).", Color.red, MessageTypes.ResultMessage);
            return isSuccess;
         }
         else
         {
            isSuccess = true;
            divingBellCount -= divingBellAmount;
         }
      }

      if (DivingBellCountText != null)
      {
         newDivingBellCount       = TradeHutManager.Instance.SellItems.Find(item => item.CompareTag(DIVING_BELL_TAG)).Find("ItemCount").GetComponent<TextMeshProUGUI>();
         newDivingBellCount.text  = " x" + divingBellCount.ToString();
         DivingBellCountText.text = " x" + divingBellCount.ToString();
      }

      return isSuccess;
   }

  public bool TryAddPatchKit(int patchKitAmount)
   {
      bool isSuccess = false;

      if (!ForgeManager.Instance.hasTier2Blueprint)
      {
         Debug.LogError("Tier 2 Blueprint required!");
         ticker.ShowTicker("Tier 2 Blueprint required for Patch Kits!", Color.red, MessageTypes.ResultMessage);
         return isSuccess;
      }

      if (patchKitCount >= MAX_PATCH_KIT_COUNT)
      {
         Debug.LogError("patch kit count is at maximum!");
         ticker.ShowTicker($"Patch Kit count is at maximum!", Color.red, MessageTypes.ResultMessage);
         return isSuccess;
      }
      else
         if ((patchKitCount + patchKitAmount) > MAX_PATCH_KIT_COUNT)
      {
         Debug.LogError("Patch Kit count is at maximum!");
         ticker.ShowTicker($"Cannot add patch kits - would exceed maximum!", Color.red, MessageTypes.ResultMessage);
         return isSuccess;
      }
      else
      {
         isSuccess = true;
         patchKitCount += patchKitAmount;

         if (PatchKitCountText != null && !PatchKitCountText.transform.parent.gameObject.activeSelf)
            PatchKitCountText.transform.parent.gameObject.SetActive(true);
      }

      PatchKitCountText.text = " x" + patchKitCount.ToString();

      return isSuccess;
   }

   public bool TryUsePatchKit(int patchKitAmount)
   {
      bool isSuccess = false;

      if (!ForgeManager.Instance.hasTier2Blueprint)
      {
         Debug.LogError("Tier 2 Blueprint required!");
         ticker.ShowTicker("Tier 2 Blueprint required to use Patch Kits!", Color.red, MessageTypes.ResultMessage);
         return isSuccess;
      }

      if (patchKitCount <= MIN_PATCH_KIT_COUNT)
      {
         Debug.LogError("Patch Kit count is at minimum!");
         ticker.ShowTicker($"Patch Kit count is at minimum! Craft a patch kit in Forge (Tier 2).", Color.red, MessageTypes.ResultMessage);
         return isSuccess;
      }
      else
      {
         if (patchKitCount < patchKitAmount)
         {
            Debug.LogError("Not enough Patch Kit!");
            ticker.ShowTicker($"Cannot use patch kits, only {patchKitCount} available! Craft a pathc kit in Forge (Tier 2).", Color.red, MessageTypes.ResultMessage);
            return isSuccess;
         }
         else
         {
            isSuccess = true;
            patchKitCount -= patchKitAmount;
         }
      }

      PatchKitCountText.text = " x" + patchKitCount.ToString();

      return isSuccess;
   }

   public bool TryAddPressureValve(int pressureValveAmount)
   {
      TextMeshProUGUI pressureValveValue;
      bool isSuccess = false;

      if (!ForgeManager.Instance.hasTier2Blueprint)
      {
         Debug.LogError("Tier 2 Blueprint required!");
         ticker.ShowTicker("Tier 2 Blueprint required for Pressure Valves!", Color.red, MessageTypes.ResultMessage);
         return isSuccess;
      }

      if (pressureValveCount >= MAX_PRESSURE_VALVE_COUNT)
      {
         Debug.LogError("Pressure valve count is at maximum!");
         ticker.ShowTicker($"Pressure valve count is at maximum!", Color.red, MessageTypes.ResultMessage);
         return isSuccess;
      }
      else
      {
         if ((pressureValveCount + pressureValveAmount) > MAX_PRESSURE_VALVE_COUNT)
         {
            Debug.LogError("Pressure valve count is at maximum!");
            ticker.ShowTicker($"Cannot add pressure valves - would exceed maximum!", Color.red, MessageTypes.ResultMessage);
            return isSuccess;
         }
         else
         {
            isSuccess = true;
            pressureValveCount += pressureValveAmount;
         }
      }

      pressureValveValue = TradeHutManager.Instance.SellItems.Find(item => item.CompareTag(PRESSURE_VALVE_TAG)).Find("ItemCount").GetComponent<TextMeshProUGUI>();
      pressureValveValue.text = " x" + pressureValveCount.ToString();
      PressureValveCountText.text = " x" + pressureValveCount.ToString();

      return isSuccess;
   }

   public bool TryUsePressureValve(int pressureValveAmount)
   {
      TextMeshProUGUI pressureValveValue;
      bool isSuccess = false;

      if (!ForgeManager.Instance.hasTier2Blueprint)
      {
         Debug.LogError("Tier 2 Blueprint required!");
         ticker.ShowTicker("Tier 2 Blueprint required to use Pressure Valves!", Color.red, MessageTypes.ResultMessage);
         return isSuccess;
      }

      if (pressureValveAmount <= MIN_PRESSURE_VALVE_COUNT)
      {
         Debug.LogError("Pressure valve count is at minimum!");
         ticker.ShowTicker($"Pressure valve count is at minimum! Craft a pressure valve in Forge (Tier 2).", Color.red, MessageTypes.ResultMessage);
         return isSuccess;
      }
      else
      {
         if (pressureValveCount < pressureValveAmount)
         {
            Debug.LogError("Not enough pressure valves!");
            ticker.ShowTicker($"Cannot use pressure valves, only {pressureValveCount} available! Craft a pressure valve in Forge (Tier 2).", Color.red, MessageTypes.ResultMessage);
            return isSuccess;
         }
         else
         {
            isSuccess = true;
            pressureValveCount -= pressureValveAmount;
         }
      }

      pressureValveValue = TradeHutManager.Instance.SellItems.Find(item => item.CompareTag(PRESSURE_VALVE_TAG)).Find("ItemCount").GetComponent<TextMeshProUGUI>();
      pressureValveValue.text = " x" + pressureValveCount.ToString();
      PressureValveCountText.text = " x" + pressureValveCount.ToString();

      return isSuccess;
   }

   public bool TryAddPrecisionLens(int precisionLensAmount)
   {
      TextMeshProUGUI newPrecisionLensCount;
      bool isSuccess = false;

      if (!ForgeManager.Instance.hasTier3Blueprint)
      {
         Debug.LogError("Tier 3 Blueprint required!");
         ticker.ShowTicker("Tier 3 Blueprint required for Precision Lenses!", Color.red, MessageTypes.ResultMessage);
         return isSuccess;
      }

      if (precisionLensCount >= MAX_PRECISION_LENS_COUNT)
      {
         Debug.LogError("Precision Lens count is at maximum!");
         ticker.ShowTicker($"Precision Lens count is at maximum!", Color.red, MessageTypes.ResultMessage);
         return isSuccess;
      }
      else
      {
         if ((precisionLensCount + precisionLensAmount) > MAX_PRECISION_LENS_COUNT)
         {
            Debug.LogError("Precision Lens count is at maximum!");
            ticker.ShowTicker($"Cannot add precision lenses - would exceed maximum!", Color.red, MessageTypes.ResultMessage);
            return isSuccess;
         }
         else
         {
            isSuccess = true;
            precisionLensCount += precisionLensAmount;
         }
      }

      if (PrecisionLensCountText != null)
      {
         newPrecisionLensCount = TradeHutManager.Instance.SellItems.Find(item => item.CompareTag(PRECISION_LENS_TAG)).Find("ItemCount").GetComponent<TextMeshProUGUI>();
         newPrecisionLensCount.text  = " x" + precisionLensCount.ToString();
         PrecisionLensCountText.text = " x" + precisionLensCount.ToString();
      }

      return isSuccess;
   }

   public bool TryUsePrecisionLens(int precisionLensAmount)
   {
      TextMeshProUGUI newPrecisionLensCount;
      bool isSuccess = false;

      if (!ForgeManager.Instance.hasTier3Blueprint)
      {
         Debug.LogError("Tier 3 Blueprint required!");
         ticker.ShowTicker("Tier 3 Blueprint required to use Precision Lenses!", Color.red, MessageTypes.ResultMessage);
         return isSuccess;
      }

      if (precisionLensCount <= MIN_PRECISION_LENS_COUNT)
      {
         Debug.LogError("Precision Lens count is at minimum!");
         ticker.ShowTicker($"Precision Lens count is at minimum! Craft a precision lens in Forge (Tier 3).", Color.red, MessageTypes.ResultMessage);
         return isSuccess;
      }
      else
      {
         if (precisionLensCount < precisionLensAmount)
         {
            Debug.LogError("Not enough Precision Lens!");
            ticker.ShowTicker($"Cannot use precision lenses, only {precisionLensCount} available! Craft a precision lens in Forge (Tier 3).", Color.red, MessageTypes.ResultMessage);
            return isSuccess;
         }
         else
         {
            isSuccess = true;
            precisionLensCount -= precisionLensAmount;
         }
      }

      if (PrecisionLensCountText != null)
      {
         newPrecisionLensCount = TradeHutManager.Instance.SellItems.Find(item => item.CompareTag(PRECISION_LENS_TAG)).Find("ItemCount").GetComponent<TextMeshProUGUI>();
         newPrecisionLensCount.text  = " x" + precisionLensCount.ToString();
         PrecisionLensCountText.text = " x" + precisionLensCount.ToString();
      }

      return isSuccess;
   }

   public bool TryAddEngine(int engineAmount)
   {
      TextMeshProUGUI engineValue;
      bool isSuccess = false;

      if (!ForgeManager.Instance.hasTier3Blueprint)
      {
         Debug.LogError("Tier 3 Blueprint required!");
         ticker.ShowTicker("Tier 3 Blueprint required for Engines!", Color.red, MessageTypes.ResultMessage);
         return isSuccess;
      }

      if (engineCount >= MAX_ENGINE_COUNT)
      {
         Debug.LogError("Engine count is at minimum!");
         ticker.ShowTicker($"Engine count is at maximum!", Color.red, MessageTypes.ResultMessage);
         return isSuccess;
      }
      else
      {
         if ((engineCount + engineAmount) > MAX_ENGINE_COUNT)
         {
            Debug.LogError("Engine count is at maximum!");
            ticker.ShowTicker($"Cannot add engines - would exceed maximum!", Color.red, MessageTypes.ResultMessage);
            return isSuccess;
         }
         else
         {
            isSuccess = true;
            engineCount += engineAmount;
         }
      }

      engineValue = TradeHutManager.Instance.SellItems.Find(item => item.CompareTag(ENGINE_TAG)).Find("ItemCount").GetComponent<TextMeshProUGUI>();
      engineValue.text = " x" + engineCount.ToString();
      EngineCountText.text = " x" + engineCount.ToString();

      return isSuccess;
   }

   public bool TryUseEngine(int engineAmount)
   {
      TextMeshProUGUI engineValue;
      bool isSuccess = false;

      if (!ForgeManager.Instance.hasTier3Blueprint)
      {
         Debug.LogError("Tier 3 Blueprint required!");
         ticker.ShowTicker("Tier 3 Blueprint required to use Engines!", Color.red, MessageTypes.ResultMessage);
         return isSuccess;
      }

      if (engineCount <= MIN_ENGINE_COUNT)
      {
         Debug.LogError("Engine count is at minimum!");
         ticker.ShowTicker($"Engine count is at minimum! Craft a clockwork engine in Forge (Tier 3).", Color.red, MessageTypes.ResultMessage);
         return isSuccess;
      }
      else
      {
         if (engineCount < engineAmount)
         {
            Debug.LogError("Not enough engines!");
            ticker.ShowTicker($"Cannot use engines, only {engineCount} available! Craft a clockwork engine in Forge (Tier 3).", Color.red, MessageTypes.ResultMessage);
            return isSuccess;
         }
         else
         {
            isSuccess = true;
            engineCount -= engineAmount;
         }
      }

      engineValue = TradeHutManager.Instance.SellItems.Find(item => item.CompareTag(ENGINE_TAG)).Find("ItemCount").GetComponent<TextMeshProUGUI>();
      engineValue.text = " x" + engineCount.ToString();
      EngineCountText.text = " x" + engineCount.ToString();

      return isSuccess;
   }

   public bool TryAddMercenaryEngineer(int amount)
   {
      bool isSuccess = false;

      if (mercenaryEngineerCount >= MAX_MERCENARY_ENGINEER_COUNT)
      {
         Debug.LogError("Mercenary Engineer count is at maximum!");
         ticker.ShowTicker($"Mercenary Engineer count is at maximum!", Color.red, MessageTypes.ResultMessage);
         return isSuccess;
      }
      else
      {
         if ((mercenaryEngineerCount + amount) > MAX_MERCENARY_ENGINEER_COUNT)
         {
            Debug.LogError("Mercenary Engineer count would exceed maximum!");
            ticker.ShowTicker($"Cannot add Mercenary Engineers - would exceed maximum!", Color.red, MessageTypes.ResultMessage);
            return isSuccess;
         }
         else
         {
            isSuccess = true;
            mercenaryEngineerCount += amount;
         }
      }

      // Update any UI if present
      if (MercenaryEngineerCountText != null)
         MercenaryEngineerCountText.text = " x" + mercenaryEngineerCount.ToString();


      return isSuccess;
   }

   public bool TryUseMercenaryEngineer(int amount)
   {
      bool isSuccess = false;

      if (mercenaryEngineerCount <= MIN_MERCENARY_ENGINEER_COUNT)
      {
         Debug.LogError("Mercenary Engineer count is at minimum!");
         ticker.ShowTicker($"Mercenary Engineer count is at minimum!", Color.red, MessageTypes.ResultMessage);
         return isSuccess;
      }
      else
      {
         if (mercenaryEngineerCount < amount)
         {
            Debug.LogError("Not enough Mercenary Engineers!");
            ticker.ShowTicker($"Cannot use Mercenary Engineers, only {mercenaryEngineerCount} available!", Color.red, MessageTypes.ResultMessage);
            return isSuccess;
         }
         else
         {
            isSuccess = true;
            mercenaryEngineerCount -= amount;
         }
      }

      if (MercenaryEngineerCountText != null)
         MercenaryEngineerCountText.text = " x" + mercenaryEngineerCount.ToString();

      return isSuccess;
   }

   public void ShowInventoryPanel()
   {
      panelManager.OpenPanel(InventoryPanel.gameObject);
      CraftsPanel.gameObject.SetActive(false);
      ResourcePanel.gameObject.SetActive(true);

      if (tutorialFunction)
      {
         InventoryPanel.transform.Find("Arrow").gameObject.SetActive(true);
      }

      // Added for camera fix
      PopUpManager.Instance.DisablePlayerInput();

      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(false);
   }

   public void CloseInventoryPanel()
   {
      // Added for camera fix
      PopUpManager.Instance.EnablePlayerInput();

      panelManager.ClosePanel(InventoryPanel.gameObject);

      if (currentCraft != null)
      {
         Destroy(currentCraft.gameObject);
         currentCraft = null;
      }

      if (currentResource != null)
      {
         Destroy(currentResource.gameObject);
         currentResource = null;
      }

      //if (ResourceWindow.gameObject.activeSelf)
      //   CloseResourcePanel();

      //if (CraftWindow.gameObject.activeSelf)
      //   CloseCraftsPanel();

      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(true);
   }

   public void ShowResourcePanel()
   {
      if (CraftsPanel.gameObject.activeSelf) 
      { 
         CloseCraftsPanel();
         CloseCraftWindow();
      }

      ResourcePanel.gameObject.SetActive(true);
   }

   public void ShowCraftsPanel()
   {
      if (ResourcePanel.gameObject.activeSelf) 
      { 
         CloseResourcePanel();
         CloseResourceWindow();
      }

      CraftsPanel.gameObject.SetActive(true);
      if(tutorialFunction)
      {
         InventoryPanel.transform.Find("Arrow").gameObject.SetActive(false);
         InventoryPanel.transform.Find("Arrow2").gameObject.SetActive(true);
         InventoryPanel.transform.Find("TutorialText").gameObject.SetActive(true);
      }
   }

   private void ShowResourceWindow()
   {
      panelManager.OpenPanel(ResourceWindow.gameObject);
      //ResourceWindow.gameObject.SetActive(true);
   }
   private void ShowCraftWindow()
   {
      panelManager.OpenPanel(CraftWindow.gameObject);
      //CraftWindow.gameObject.SetActive(true);
   }

   
   private void CloseResourcePanel()
   {
      ResourcePanel.gameObject.SetActive(false);
   }
   
   private void CloseCraftsPanel()
   {
      CraftsPanel.gameObject.SetActive(false);
      if(tutorialFunction)
      {
         InventoryPanel.transform.Find("Arrow3").gameObject.SetActive(false);
         tutorialFunction = false;
      }
   }
   public void CloseResourceWindow()
   {
      ResourceWindow.gameObject.SetActive(false);
   }

   public void CloseCraftWindow()
   {
      CraftWindow.gameObject.SetActive(false);
   }

   public void CheckUpgradeResources()
   {
      // Safety check: if the icons are destroyed/missing, don't try to access them
      if (OreRefineryUpgradeIcon == null || ForgeUpgradeIcon == null || ExplorationUnitUpgradeIcon == null)
      {
         Debug.LogWarning("InventoryManager: Upgrade icons are missing. Need to re-link UI for this scene.");
         return;
      }
      if (OreRefinery_Manager.Instance.oreLevel >= 3)
         OreRefineryUpgradeIcon.gameObject.SetActive(false);
      else
      {
         bool canAffordRefinery = pearlCount >= OreRefinery_Manager.Instance.NextUpgradeCostInPearls && 
                                  oreCount >= OreRefinery_Manager.Instance.NextUpgradeCostInOre;
         OreRefineryUpgradeIcon.gameObject.SetActive(canAffordRefinery);
      }

      if (ForgeManager.forgeLevel >= 3)
         ForgeUpgradeIcon.gameObject.SetActive(false);
      else 
      { 
         if (ForgeManager.forgeLevel == 1)
         {
            ForgeUpgradeIcon.gameObject.SetActive(pearlCount >= ForgeManager.LEVEL_2_PEARL_COST);
         }
         else 
         {
            if (ForgeManager.forgeLevel == 2) 
               ForgeUpgradeIcon.gameObject.SetActive(pearlCount >= ForgeManager.LEVEL_3_PEARL_COST);
         }
      }

      if (ExplorationUnitManager.Instance.shipManager.ShipLevel >= 3)
         ExplorationUnitUpgradeIcon.gameObject.SetActive(false);
      else 
      { 
         if (ExplorationUnitManager.Instance.shipManager.ShipLevel == 1) 
            ExplorationUnitUpgradeIcon.gameObject.SetActive(pearlCount >= ExplorationUnitManager.LEVEL2_PEARL_COST);
         else 
         { 
            if (ExplorationUnitManager.Instance.shipManager.ShipLevel == 2) 
               ExplorationUnitUpgradeIcon.gameObject.SetActive(pearlCount >= ExplorationUnitManager.LEVEL3_PEARL_COST);
         }
      }
   }
}