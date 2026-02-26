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
   /* Holds a reference to the singleton instance of this class. ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½      */
   public static InventoryManager Instance { get; private set; }

   private TickerSystem ticker;

   /* Inspector variables for UI elements. ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½              */
   [SerializeField]
   private Transform InventoryPanel,
                                      ResourcePanel,
                                      ResourceWindow,
                                      CraftsPanel,
                                      CraftWindow;

   [SerializeField]
   private Image ForgeUpgradeIcon,
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


   /* Constants                                                                     */
   public const int MIN_PEARL_COUNT = 0,
                    MIN_CRYSTAL_COUNT = 0,
                    MIN_ORE_COUNT = 0,
                    MAX_PEARL_COUNT = 10000,
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


   public const int RESOURCE_SPACING = 30,
                    PEARL_POSITION = 0,
                    CRYSTAL_POSITION = PEARL_POSITION + 10,
                    ORE_POSITION = CRYSTAL_POSITION + 10,
                    CRUDE_TOOL_POSITION = 0,
                    HARPOON_POSITION = CRUDE_TOOL_POSITION + 10,
                    PATCH_KIT_POSITION = CRUDE_TOOL_POSITION,
                    PRESSURE_VALVE_POSITION = HARPOON_POSITION + 10,
                    DIVING_BELL_POSITION = CRUDE_TOOL_POSITION + 10,
                    ENGINE_POSITION = PRESSURE_VALVE_POSITION + 10,
                    PRECISION_LENS_POSITION = PRESSURE_VALVE_POSITION,
                    MERCENARY_ENGINEER_POSITION = PRESSURE_VALVE_POSITION + 10;

   public const string PEARL_TAG = "Pearl",
                       CRYSTAL_TAG = "Crystal",
                       ORE_TAG = "Ore",
                       CRUDE_TOOL_TAG = "Crude Tool",
                       HARPOON_TAG = "Harpoon",
                       PATCH_KIT_TAG = "Patch Kit",
                       PRESSURE_VALVE_TAG = "Pressure Valve",
                       DIVING_BELL_TAG = "Diving Bell",
                       ENGINE_TAG = "Engine",
                       PRECISION_LENS_TAG = "Precision Lens",
                       RAW_ORE_CHUNK_TAG = "Raw Ore Chunk",
                       MERCENARY_ENGINEER_TAG = "Mercenary Engineer";


   /* Public properties                               ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½   ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½  */
   public int pearlCount { get; private set; }
   public int crystalCount { get; private set; }
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

   /* Private variables ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½      */
   private Transform currentResource,
                     currentCraft;

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
         Destroy(this.gameObject);
      else
      {
         Instance = this;
         DontDestroyOnLoad(this.gameObject);
      }

      InventoryItems = new();

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

      pearlCount = 500;
      crystalCount = MIN_CRYSTAL_COUNT;
      oreCount = 100;
      crudeToolCount = MIN_CRUDE_TOOL_COUNT;
      harpoonCount = MIN_HARPOON_COUNT;
      engineCount = MIN_ENGINE_COUNT;
   }

   /* Creates the display elements for Pearls and Crystals on the inventory panel. */
   private void Start()
   {
      CreateResource(Resources.GetResourceSprite(Resources.ResourceType.Pearl), PEARL_POSITION, PEARL_TAG);
      CreateResource(Resources.GetResourceSprite(Resources.ResourceType.Crystal), CRYSTAL_POSITION, CRYSTAL_TAG);
      CreateResource(Resources.GetResourceSprite(Resources.ResourceType.Ore), ORE_POSITION, ORE_TAG);
      //CreateResource(GetItemSprite(ItemType.RawOreChunk), ORE_POSITION + 10, RAW_ORE_CHUNK_TAG); 

      CreateCraft(GetItemSprite(ItemType.CrudeTool), CRUDE_TOOL_POSITION, CRUDE_TOOL_TAG);
      CreateCraft(GetItemSprite(ItemType.Harpoon), HARPOON_POSITION, HARPOON_TAG);
      //CreateCraft(GetItemSprite(ItemType.PatchKit), PATCH_KIT_POSITION, PATCH_KIT_TAG, -250);
      //CreateCraft(GetItemSprite(ItemType.PressureValve), PRESSURE_VALVE_POSITION, PRESSURE_VALVE_TAG);
      //CreateCraft(GetItemSprite(ItemType.Engine), ENGINE_POSITION, ENGINE_TAG);
      //CreateCraft(GetItemSprite(ItemType.DivingBell), DIVING_BELL_POSITION, DIVING_BELL_TAG, -250);
      //CreateCraft(GetItemSprite(ItemType.PrecisionLens), PRECISION_LENS_POSITION, PRECISION_LENS_TAG, -250);
      //CreateCraft(GetItemSprite(ItemType.MercenaryEngineer), MERCENARY_ENGINEER_POSITION, TradeHutManager.MERCENARY_ENGINEER_TAG, -250);

      if (PatchKitCountText != null)
         PatchKitCountText.transform.parent.gameObject.SetActive(false);

      if (TickerSystem.Instance)
      {
         Debug.Log("Instance is set");
         ticker = TickerSystem.Instance;
      }
      else
         Debug.LogError("No ticker");
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

      switch (resourceTag)
      {
         case PEARL_TAG:
            resourceCount = pearlCount;
            break;
         case CRYSTAL_TAG:
            resourceCount = crystalCount;
            break;
         case ORE_TAG:
            resourceCount = oreCount;
            break;
         case RAW_ORE_CHUNK_TAG:
            resourceCount = rawOreChunkCount;
            break;
         default:
            Debug.LogError("Unknown resource tag: " + resourceTag);
            resourceCount = 0;
            break;
      }

      /* Instantiate the resource template and set its position in the container   */
      /* Transform of the newly created resource UI element. ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½     */
      resourceTransform = Instantiate(resourceTemplate, resourceContainer);

      /* RectTransform for positioning the new resource UI element. ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ï¿½     */
      resourceRectTransform = resourceTransform.GetComponent<RectTransform>();

      resourceTransform.tag = resourceTag;

      /* Places the new resource entry in a horizontal row inside the inventory    */
      resourceRectTransform.anchoredPosition = new Vector2(RESOURCE_SPACING * positionIndex, 0);

      /* Populate the resource components with item-specific data                  */
      resourceTransform.Find("ResourceCount").GetComponent<TextMeshProUGUI>().text = " x" + resourceCount.ToString();
      resourceWindowButton = resourceTransform.Find("ResourceButton").GetComponent<Button>();
      resourceWindowButton.image.sprite = resourceSprite;

      /* Dynamically add listeners to the button, which creates the resource       */
      /* information window                                                        */
      resourceWindowButton.onClick.AddListener(() => CreateResourceWindow(resourceSprite, resourceTag));

      switch (resourceTag)
      {
         case PEARL_TAG:
            PearlCountText = resourceTransform.Find("ResourceCount").GetComponent<TextMeshProUGUI>();
            break;
         case CRYSTAL_TAG:
            CrystalCountText = resourceTransform.Find("ResourceCount").GetComponent<TextMeshProUGUI>();
            break;
         case ORE_TAG:
            OreCountText = resourceTransform.Find("ResourceCount").GetComponent<TextMeshProUGUI>();
            break;
         case RAW_ORE_CHUNK_TAG:
            RaWOreChunkCountText = resourceTransform.Find("ResourceCount").GetComponent<TextMeshProUGUI>();
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
      int craftCount;

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
      craftRectTransform.anchoredPosition = new Vector2(RESOURCE_SPACING * positionIndex, verticalIndex);

      /* Populate the resource components with item-specific data                  */
      craftTransform.Find("CraftCount").GetComponent<TextMeshProUGUI>().text = " x" + craftCount.ToString();
      craftWindowButton = craftTransform.Find("CraftButton").GetComponent<Button>();
      craftWindowButton.image.sprite = craftSprite;

      switch (craftTag)
      {
         case CRUDE_TOOL_TAG:
            CrudeToolCountText = craftTransform.Find("CraftCount").GetComponent<TextMeshProUGUI>();
            break;
         case HARPOON_TAG:
            HarpoonCountText = craftTransform.Find("CraftCount").GetComponent<TextMeshProUGUI>();
            break;
         case PATCH_KIT_TAG:
            PatchKitCountText = craftTransform.Find("CraftCount").GetComponent<TextMeshProUGUI>();
            break;
         case PRESSURE_VALVE_TAG:
            PressureValveCountText = craftTransform.Find("CraftCount").GetComponent<TextMeshProUGUI>();
            break;
         case DIVING_BELL_TAG:
            DivingBellCountText = craftTransform.Find("CraftCount").GetComponent<TextMeshProUGUI>();
            break;
         case ENGINE_TAG:
            EngineCountText = craftTransform.Find("CraftCount").GetComponent<TextMeshProUGUI>();
            break;
         case PRECISION_LENS_TAG:
            PrecisionLensCountText = craftTransform.Find("CraftCount").GetComponent<TextMeshProUGUI>();
            break;
         case MERCENARY_ENGINEER_TAG:
            MercenaryEngineerCountText = craftTransform.Find("CraftCount").GetComponent<TextMeshProUGUI>();
            break;
         default:
            Debug.LogError("Unknown craft");
            break;
      }

      /* Dynamically add listeners to the buttons, which creates the craft window  */
      craftWindowButton.onClick.AddListener(() => { CreateCraftWindow(craftSprite, craftTag); });

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
         case CRYSTAL_TAG:
            resourceCount = crystalCount;
            resourceInfo = resourceInfo = Resources.GetResourceDescription(Resources.ResourceType.Crystal);
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
            craftInfo = "";
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
         ticker.ShowTicker("Pearl count is at minimum!", Color.red, MessageTypes.ResultMessage);
         Debug.LogError("Pearl count is at minimum!");
      }
      else
      {
         if (pearlCount < pearlAmount)
         {
            Debug.LogError("Not enough pearls to spend!");
            ticker.ShowTicker($"Cannot spend pearls, only {pearlCount} available!", Color.red, MessageTypes.ResultMessage);
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

   public bool TryAddCrystal(int crystalAmount)
   {
      bool isSuccess = false;

      if (crystalCount >= MAX_CRYSTAL_COUNT)
      {
         Debug.LogError("Crystal count is at maximum!");
         ticker.ShowTicker("CRystal count is at maximum!", Color.red, MessageTypes.ResultMessage);
      }
      else
         if ((crystalCount + crystalAmount) > MAX_CRYSTAL_COUNT)
         ticker.ShowTicker("Cannot add crystals - would exceed maximum!", Color.red, MessageTypes.ResultMessage);
      else
      {
         crystalCount += crystalAmount;
         isSuccess = true;
      }

      OnCrystalCountChanged?.Invoke(crystalCount);
      CrystalCountText.text = " x" + crystalCount.ToString();

      return isSuccess;
   }

   public bool TrySpendCrystal(int crystalAmount)
   {
      bool isSuccess = false;

      if (crystalCount <= MIN_CRYSTAL_COUNT)
      {
         ticker.ShowTicker("Crystal count is at minimum", Color.red, MessageTypes.ResultMessage);
         Debug.LogError("Crystal count is at minimum!");
      }
      else
         if (crystalCount < crystalAmount)
      {
         ticker.ShowTicker($"Cannot spend crystals, only {crystalCount} available!", Color.red, MessageTypes.ResultMessage);
         Debug.LogError("Not enough crystals to spend!");
      }
      else
      {
         crystalCount -= crystalAmount;
         isSuccess = true;
      }

      OnCrystalCountChanged?.Invoke(crystalCount);
      CrystalCountText.text = " x" + crystalCount.ToString();

      return isSuccess;
   }

   public bool TryAddOre(int oreAmount)
   {
      bool isSuccess = false;

      if (oreCount >= MAX_ORE_COUNT)
      {
         Debug.LogError("Not enough ore to spend!");
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
         ticker.ShowTicker($"Cannot use crude tools, only {crudeToolCount} avaliable!", Color.red, MessageTypes.ResultMessage);

         return isSuccess;
      }
      else
      {
         isSuccess = true;
         crudeToolCount -= crudeToolAmount;

      }

      newCrudeToolCount = TradeHutManager.Instance.SellItems.Find(item => item.CompareTag(CRUDE_TOOL_TAG)).Find("ItemCount").GetComponent<TextMeshProUGUI>();
      newCrudeToolCount.text = " x" + crudeToolCount.ToString();
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
         ticker.ShowTicker($"Harpoon count is at minimum!", Color.red, MessageTypes.ResultMessage);
         return isSuccess;
      }
      else
         if (harpoonCount < harpoonAmount)
      {
         Debug.LogError("Not enough harpoons!");
         ticker.ShowTicker($"Cannot use harpoons, only {harpoonCount} available!", Color.red, MessageTypes.ResultMessage);
         return isSuccess;
      }
      else
      {
         harpoonCount -= harpoonAmount;
         isSuccess = true;
      }

      newHarpoonCount = TradeHutManager.Instance.SellItems.Find(item => item.CompareTag(HARPOON_TAG)).Find("ItemCount").GetComponent<TextMeshProUGUI>();
      newHarpoonCount.text = " x" + harpoonCount.ToString();
      HarpoonCountText.text = " x" + harpoonCount.ToString();

      return isSuccess;
   }

   public bool TryAddDivingBell(int divingBellAmount)
   {
      bool isSuccess = false;


      if (divingBellCount >= MAX_DIVING_BELL_COUNT)
      {
         Debug.LogError("Diving Bell count is at maximum!");
         ticker.ShowTicker($"Diving Bell count is at maximum!", Color.red, MessageTypes.ResultMessage);
         return isSuccess;
      }
      else
         if ((divingBellCount + divingBellAmount) > MAX_DIVING_BELL_COUNT)
      {
         Debug.LogError("Diving Bell count is at maximum!");
         ticker.ShowTicker($"Cannot add diving bells - would exceed maximum!", Color.red, MessageTypes.ResultMessage);
         return isSuccess;
      }
      else
      {
         isSuccess = true;
         divingBellCount += divingBellAmount;
      }

      DivingBellCountText.text = " x" + divingBellCount.ToString();

      return isSuccess;
   }

   public bool TryUseDivingBell(int divingBellAmount)
   {
      bool isSuccess = false;

      if (divingBellCount <= MIN_DIVING_BELL_COUNT)
      {
         Debug.LogError("Diving Bell count is at minimum!");
         ticker.ShowTicker($"Diving Bell count is at minimum!", Color.red, MessageTypes.ResultMessage);
         return isSuccess;
      }
      else
      {
         if (divingBellCount < divingBellAmount)
         {
            Debug.LogError("Not enough diving bells!");
            ticker.ShowTicker($"Cannot use diving bells, only {divingBellCount} available!", Color.red, MessageTypes.ResultMessage);
            return isSuccess;
         }
         else
         {
            isSuccess = true;
            divingBellCount -= divingBellAmount;
         }
      }

      DivingBellCountText.text = " x" + divingBellCount.ToString();

      return isSuccess;
   }

   public bool TryAddPatchKit(int patchKitAmount)
   {
      bool isSuccess = false;

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

      if (patchKitCount <= MIN_PATCH_KIT_COUNT)
      {
         Debug.LogError("Patch Kit count is at minimum!");
         ticker.ShowTicker($"Patch Kit count is at minimum!", Color.red, MessageTypes.ResultMessage);
         return isSuccess;
      }
      else
      {
         if (patchKitCount < patchKitAmount)
         {
            Debug.LogError("Not enough Patch Kit!");
            ticker.ShowTicker($"Cannot use patch kits, only {patchKitCount} available!", Color.red, MessageTypes.ResultMessage);
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

   public bool TryAddPrecisionLens(int precisionLensAmount)
   {
      bool isSuccess = false;

      if (precisionLensCount >= MAX_PRECISION_LENS_COUNT)
      {
         Debug.LogError("Precision Lens count is at minimum!");
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

      PrecisionLensCountText.text = " x" + precisionLensCount.ToString();

      return isSuccess;
   }

   public bool TryUsePrecisionLens(int precisionLensAmount)
   {
      bool isSuccess = false;

      if (precisionLensCount <= MIN_PRECISION_LENS_COUNT)
      {
         Debug.LogError("Precision Lensl count is at minimum!");
         ticker.ShowTicker($"Precision Lens count is at minimum!", Color.red, MessageTypes.ResultMessage);
         return isSuccess;
      }
      else
      {
         if (precisionLensCount < precisionLensAmount)
         {
            Debug.LogError("Not enough Precision Lens!");
            ticker.ShowTicker($"Cannot use precision lenses, only {precisionLensCount} available!", Color.red, MessageTypes.ResultMessage);
            return isSuccess;
         }
         else
         {
            isSuccess = true;
            precisionLensCount -= precisionLensAmount;
         }
      }

      PrecisionLensCountText.text = " x" + precisionLensCount.ToString();

      return isSuccess;
   }

   public bool TryAddPressureValve(int pressureValveAmount)
   {
      TextMeshProUGUI pressureValveValue;
      bool isSuccess = false;

      if (pressureValveCount >= MAX_PRESSURE_VALVE_COUNT)
      {
         Debug.LogError("Pressure valve count is at minimum!");
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

      if (pressureValveAmount <= MIN_PRESSURE_VALVE_COUNT)
      {
         Debug.LogError("Pressure valve count is at minimum!");
         ticker.ShowTicker($"Pressure valve count is at minimum!", Color.red, MessageTypes.ResultMessage);
         return isSuccess;
      }
      else
      {
         if (pressureValveCount < pressureValveAmount)
         {
            Debug.LogError("Not enough pressure valves!");
            ticker.ShowTicker($"Cannot use pressure valves, only {pressureValveCount} available!", Color.red, MessageTypes.ResultMessage);
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

   public bool TryAddEngine(int engineAmount)
   {
      TextMeshProUGUI engineValue;
      bool isSuccess = false;

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

      if (engineCount <= MIN_ENGINE_COUNT)
      {
         Debug.LogError("Engine count is at minimum!");
         ticker.ShowTicker($"Engine count is at minimum!", Color.red, MessageTypes.ResultMessage);
         return isSuccess;
      }
      else
      {
         if (engineCount < engineAmount)
         {
            Debug.LogError("Not enough engines!");
            ticker.ShowTicker($"Cannot use engines, only {engineCount} available!", Color.red, MessageTypes.ResultMessage);
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
      InventoryPanel.gameObject.SetActive(true);
      ResourcePanel.gameObject.SetActive(true);
      // Added for camera fix
      PopUpManager.Instance.DisablePlayerInput();

      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(false);
   }

   public void ShowResourcePanel()
   {
      if (CraftsPanel.gameObject.activeSelf)
         CloseCraftsPanel();

      ResourcePanel.gameObject.SetActive(true);
   }
   private void ShowResourceWindow()
   {
      ResourceWindow.gameObject.SetActive(true);
   }
   private void ShowCraftWindow()
   {
      CraftWindow.gameObject.SetActive(true);
   }

   public void CloseInventoryPanel()
   {
      // Added for camera fix
      PopUpManager.Instance.EnablePlayerInput();

      InventoryPanel.gameObject.SetActive(false);

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

      if (ResourceWindow.gameObject.activeSelf)
         CloseResourcePanel();

      if (CraftWindow.gameObject.activeSelf)
         CloseCraftsPanel();

      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(true);
   }
   private void CloseResourcePanel()
   {
      ResourcePanel.gameObject.SetActive(false);
   }

   public void ShowCraftsPanel()
   {
      if (ResourcePanel.gameObject.activeSelf)
         CloseResourcePanel();

      CraftsPanel.gameObject.SetActive(true);
   }

   private void CloseCraftsPanel()
   {
      CraftsPanel.gameObject.SetActive(false);
   }

   public bool TrySpendItem(string itemName, int amount)
   {
      switch (itemName)
      {
         // Resources
         case PEARL_TAG:
            return TrySpendPearl(amount);
         case CRYSTAL_TAG:
            return TrySpendCrystal(amount);
         case ORE_TAG:
            return TrySpendOre(amount);

         // Crafted Items
         case CRUDE_TOOL_TAG:
            return TryUseCrudeTool(amount);
         case HARPOON_TAG:
            return TryUseHarpoon(amount);
         case PATCH_KIT_TAG:
            return TryUsePatchKit(amount);
         case PRESSURE_VALVE_TAG:
            return TryUsePressureValve(amount);
         case DIVING_BELL_TAG:
            return TryUseDivingBell(amount);
         case ENGINE_TAG:
            return TryUseEngine(amount);
         case PRECISION_LENS_TAG:
            return TryUsePrecisionLens(amount);
         //case RAW_ORE_CHUNK_TAG:
         //return TryUseRawOreChunk(amount);

         default:
            Debug.LogError($"TrySpendItem: Unknown item type '{itemName}'");
            return false;
      }
   }

   private void CheckUpgradeResources()
   {
      if (pearlCount >= OreRefinery_Manager.Instance.NextUpgradeCostInPearls && OreRefinery_Manager.Instance.NextUpgradeCostInOre <= oreCount)
         OreRefineryUpgradeIcon.gameObject.SetActive(true);
      else
         OreRefineryUpgradeIcon.gameObject.SetActive(false);

      if (ForgeManager.forgeLevel == 1)
      {
         if (pearlCount >= ForgeManager.LEVEL_2_PEARL_COST)
            ForgeUpgradeIcon.gameObject.SetActive(true);
         else
            ForgeUpgradeIcon.gameObject.SetActive(false);
      }
      else
      {
         if (ForgeManager.forgeLevel == 2)
            if (pearlCount >= ForgeManager.LEVEL_3_PEARL_COST)
               ForgeUpgradeIcon.gameObject.SetActive(true);
            else
               ForgeUpgradeIcon.gameObject.SetActive(true);
      }
   }

}