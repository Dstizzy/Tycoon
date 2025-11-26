/* Libraries and references                                                         */
using System;

using TMPro;                                                                        

using UnityEngine;                                                                  
using UnityEngine.UI;                                                               
                                                                                    
public class InventoryManager : MonoBehaviour 
{                                     
   /* Holds a reference to the singleton instance of this class. ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½      */
   public static InventoryManager Instance { get; private set; }                   
                                                                                    
   /* Inspector variables for UI elements. ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½              */
   [SerializeField] private Transform InventoryPanel,
                                      ResourcePanel,
                                      ResourceWindow,
                                      CraftsPanel,                                          
                                      CraftWindow;                              
                                                                                   
   private TextMeshProUGUI PearlCountText,                      
                           CrystalCountText,
                           OreCountText,
                           PressureValveCountText,                                  
                           CrudeToolCountText,                                       
                           HarpoonCountText,
                           EngineCountText;
   
   /* Constants                                                                     */
   const int MIN_PEARL_COUNT        = 0,                                              
             MIN_CRYSTAL_COUNT      = MIN_PEARL_COUNT,
             MIN_ORE_COUNT          = MIN_PEARL_COUNT,
             MAX_PEARL_COUNT        = 1000,
             MAX_CRYSTAL_COUNT      = MAX_PEARL_COUNT,
             MAX_ORE_COUNT          = MAX_PEARL_COUNT;

   const int MAX_CRUDE_TOOL_COUNT     = 100,
             MAX_HARPOON_COUNT        = 100,
             MAX_PRESSURE_VALVE_COUNT = 100,
             MAX_ENGINE_COUNT         = 100,
             MIN_CRUDE_TOOL_COUNT     = 0,
             MIN_HARPOON_COUNT        = 0,
             MIN_PRESSURE_VALVE_COUNT = 0,
             MIN_ENGINE_COUNT         = 0;


   const int RESOURCE_SPACING        = 30,
             PEARL_POSITION          = 0,
             CRYSTAL_POSITION        = PEARL_POSITION + 10,
             ORE_POSITION            = CRYSTAL_POSITION + 10,
             CRUDE_TOOL_POSITION     = 0,
             HARPOON_POSITION        = CRUDE_TOOL_POSITION + 10,
             PRESSURE_VALVE_POSITION = HARPOON_POSITION + 10,
             ENGINE_POSITION         = PRESSURE_VALVE_POSITION + 10;

   const string PEARL_TAG         =  "Pearl",
                CRYSTAL_TAG       =  "Crystal",
                ORE_TAG           =  "Ore",
                CRUDE_TOOL_TAG     =  "Crude Tool",
                HARPOON_TAG        =  "Harpoon",
                PRESSURE_VALVE_TAG =  "Pressure Valve",
                ENGINE_TAG         =  "Engine";


   /* Public properties                               ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½   ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½  */
   public int pearlCount         { get; private set; }
   public int crystalCount       { get; private set; }
   public int oreCount           { get; private set; }
   public int crudeToolCount     { get; private set; }
   public int harpoonCount       { get; private set; }
   public int pressureValveCount { get; private set; }
   public int engineCount        { get; private set; }

   /* Private variables ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½      */
   private Transform currentResource,  
                     currentCraft;
   
   /* Delegate for when the pearl count changes. ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½   */
   public Action<int> OnPearlCountChanged;                                         

   /* Delegate for when the crystal count changes. ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½   */
   public Action<int> OnCrystalCountChanged;

   /* Delegate for when the crystal count changes. ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½   */
   public Action<int> OnOreCountChanged;
                                                                                   
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
      
      if (InventoryPanel == null) 
          Debug.LogError("Inventory Panel is not assigned in the Inspector!");
      else 
          InventoryPanel.gameObject.SetActive(false);

      if(CraftsPanel == null)
         Debug.LogError("Crafts Panel is not assigned in the Inspector!");
      else
         CraftsPanel.gameObject.SetActive(false);

      if(ResourcePanel == null) 
         Debug.LogError("Resource Panel is not assigned in the Inspector!");
      else
          ResourcePanel.gameObject.SetActive(false);

      if(ResourceWindow == null)
         Debug.LogError("Resource Info Window is not assigned in the Inspector!");
      else
         ResourceWindow.gameObject.SetActive(false);

      if(CraftWindow == null)
         Debug.Log("Craft window is ont assigned in the inspector");
      else
         CraftWindow.gameObject.SetActive(false);

      pearlCount     = MIN_PEARL_COUNT;
      crystalCount   = MIN_CRYSTAL_COUNT;
      oreCount       = MIN_ORE_COUNT;
      crudeToolCount = MIN_CRUDE_TOOL_COUNT;
      harpoonCount   = MIN_HARPOON_COUNT;
      engineCount    = MIN_ENGINE_COUNT;
   }
   
   /* Creates the display elements for Pearls and Crystals on the inventory panel. */
   private void Start() 
   {
      CreateResource(Resources.GetResourceSprite(Resources.ResourceType.Pearl), PEARL_POSITION,PEARL_TAG);
      CreateResource(Resources.GetResourceSprite(Resources.ResourceType.Crystal), CRYSTAL_POSITION, CRYSTAL_TAG);
      CreateResource(Resources.GetResourceSprite(Resources.ResourceType.Ore), ORE_POSITION, ORE_TAG);

      CreateCraft(Item.GetItemSprite(Item.ItemType.CrudeTool), CRUDE_TOOL_POSITION, CRUDE_TOOL_TAG);
      CreateCraft(Item.GetItemSprite(Item.ItemType.Harpoon), HARPOON_POSITION, HARPOON_TAG);
      CreateCraft(Item.GetItemSprite(Item.ItemType.PressureValve), PRESSURE_VALVE_POSITION, PRESSURE_VALVE_TAG);
      CreateCraft(Item.GetItemSprite(Item.ItemType.Engine), ENGINE_POSITION, ENGINE_TAG);
   }
   
   /* Creates and positions a resource display element in the inventory panel. ï¿½   */
   private void CreateResource(Sprite resourceSprite, float positionIndex,  string resourceTag)           
   {                                                                                
      Transform     resourceTransform,
                    resourceContainer = ResourcePanel.Find("ResourceContainer").GetComponent<Transform>(),
                    resourceTemplate  = resourceContainer.Find("ResourceTemplate").GetComponent<Transform>();
      Button        resourceWindowButton;
      RectTransform resourceRectTransform;
      int           resourceCount;

      switch (resourceTag) 
      {
         case "Pearl":
            resourceCount = pearlCount;
            break;
         case "Crystal":
            resourceCount = crystalCount;
            break;
         case "Ore":
            resourceCount = oreCount;
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
             PearlCountText   = resourceTransform.Find("ResourceCount").GetComponent<TextMeshProUGUI>();
             break;
          case CRYSTAL_TAG:
             CrystalCountText = resourceTransform.Find("ResourceCount").GetComponent<TextMeshProUGUI>();
             break;
          case ORE_TAG:
             OreCountText     = resourceTransform.Find("ResourceCount").GetComponent<TextMeshProUGUI>();
             break;
         default:
            Debug.LogError("Unknown resource tag: " + resourceTag);
            break;
      }

      resourceTransform.gameObject.SetActive(true);
   }

   private void CreateCraft(Sprite craftSprite, float positionIndex, string craftTag)           
   {                           
      Transform craftsContainer = CraftsPanel.Find("CraftContainer").GetComponent<Transform>(),
                craftTemplate   = craftsContainer.Find("CraftTemplate").GetComponent<Transform>();
      Button    craftWindowButton;
      int       craftCount;

      switch (craftTag) 
      {
         case CRUDE_TOOL_TAG:
            craftCount = crudeToolCount;
            break;
         case HARPOON_TAG:
            craftCount = harpoonCount;
            break;
         case PRESSURE_VALVE_TAG:
            craftCount = pressureValveCount;
            break;
         case ENGINE_TAG:
            craftCount = engineCount;
            break;
         default:
            craftCount = 0;
            Debug.LogError("Unknown craft tag: " + craftTag);
            break;
      }

      /* Instantiate the craft template and set its position in the container.     */
      /* Transform of the newly created resource UI element. ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½     */
      Transform craftTransform = Instantiate(craftTemplate, craftsContainer);
      
      /* RectTransform for positioning the new resource UI element.                */
       RectTransform craftRectTransform = craftTransform.GetComponent<RectTransform>();
       
      craftTransform.tag = craftTag;
      
      /* Places the new resource entry in a horizontal row inside the inventory    */
      craftRectTransform.anchoredPosition = new Vector2(RESOURCE_SPACING * positionIndex, 0);
      
      /* Populate the resource components with item-specific data                  */
      craftTransform.Find("CraftCount").GetComponent<TextMeshProUGUI>().text = " x" + craftCount.ToString();
      craftWindowButton = craftTransform.Find("CraftButton").GetComponent<Button>();
      craftWindowButton.image.sprite = craftSprite;

      switch (craftTag)
      { 
         case CRUDE_TOOL_TAG:
            CrudeToolCountText     = craftTransform.Find("CraftCount").GetComponent<TextMeshProUGUI>();
            break;
         case HARPOON_TAG:
            HarpoonCountText       = craftTransform.Find("CraftCount").GetComponent<TextMeshProUGUI>();
            break;
         case PRESSURE_VALVE_TAG:
            PressureValveCountText = craftTransform.Find("CraftCount").GetComponent<TextMeshProUGUI>();
            break;
         case ENGINE_TAG:
            EngineCountText        = craftTransform.Find("CraftCount").GetComponent<TextMeshProUGUI>();
            break;
         default:
            Debug.LogError("Unknown craft");
            break;
      }

      /* Dynamically add listeners to the buttons, which creates the craft window  */
      craftWindowButton.onClick.AddListener(() => { CreateCraftWindow(craftSprite, craftTag); });
      craftTransform.gameObject.SetActive(true);
   }

   /* Creates and populates the resource information window                        */
   private void CreateResourceWindow(Sprite resourceSprite, string resourceTag) 
   {
      Transform resourceWindowContainer = ResourceWindow.Find("ResourceContainer").GetComponent<Transform>(),
                resourceWindowTemplate  = resourceWindowContainer.Find("ResourceWindowTemplate").GetComponent<Transform>();
      int       resourceCount           = 0;
      string    resourceInfo            = "";

      /* Instantiate the resource template and set its position in the container.  */
      /* Transform of the newly created resource UI element.                       */
      Transform     resourceTransform     = Instantiate(resourceWindowTemplate, resourceWindowContainer);
      RectTransform resourceRectTransform = resourceTransform.GetComponent<RectTransform>();

      /* Destroys the current resource in the window if it exists.                 */
      if (currentResource != null) 
      {
         Destroy(currentResource.gameObject);
         currentResource = null;
      }

      resourceTransform.tag = resourceTag;

      switch (resourceTag) {
         case PEARL_TAG:
            resourceCount = pearlCount;
            resourceInfo  = Resources.GetResourceDescription(Resources.ResourceType.Pearl);
            break;
         case CRYSTAL_TAG:
            resourceCount = crystalCount;
            resourceInfo  = resourceInfo = Resources.GetResourceDescription(Resources.ResourceType.Crystal);
            break;
         case ORE_TAG:
            resourceCount = oreCount;
            resourceInfo = resourceInfo = Resources.GetResourceDescription(Resources.ResourceType.Ore);
            break;
         default: 
            Debug.Log("Unknown item tag for resource window.");
            break;
      }

      /* Populate the resource properties (value, name, sprite).                   */
      resourceTransform.Find("ResourceImage").GetComponent<Image>().sprite         = resourceSprite;
      resourceTransform.Find("ResourceCount").GetComponent<TextMeshProUGUI>().text = "  x" + resourceCount.ToString();
      resourceTransform.Find("ResourceName").GetComponent<TextMeshProUGUI>().text  = resourceTag;
      resourceTransform.Find("ResourceInfo").GetComponent<TextMeshProUGUI>().text  = resourceInfo;

      currentResource = resourceTransform;
      resourceTransform.gameObject.SetActive(true);
      ShowResourceWindow();
   }

   /* Creates and populates the craft information window                           */
   private void CreateCraftWindow(Sprite crafteSprite, string craftTag) 
   {
      Transform craftWindowContainer = CraftWindow.Find("CraftContainer").GetComponent<Transform>(),
                craftWindowTemplate  = craftWindowContainer.Find("CraftWindowTemplate").GetComponent<Transform>();
      int       craftCount           = 0;
      string    craftInfo            = "";

      /* Instantiate the resource template and set its position in the container.  */
      /* Transform of the newly created resource UI element.                       */
      Transform     craftTransform     = Instantiate(craftWindowTemplate, craftWindowContainer);
      RectTransform craftRectTransform = craftTransform.GetComponent<RectTransform>();

      /* Destroys the current craft in the window if it exists.                    */
      if (currentCraft != null) {
         Destroy(currentCraft.gameObject);
         currentCraft = null;
      }

      craftTransform.tag = craftTag;

      switch (craftTag) 
      {
         case CRUDE_TOOL_TAG:
            craftCount = crudeToolCount;
            craftInfo  = Item.GetItemDescription(Item.ItemType.CrudeTool);
            break;
         case HARPOON_TAG:
            craftCount = harpoonCount;
            craftInfo  = Item.GetItemDescription(Item.ItemType.Harpoon);
            break;
         case PRESSURE_VALVE_TAG:
            craftCount = pressureValveCount;
            craftInfo  = Item.GetItemDescription(Item.ItemType.PressureValve);
            break;
         case ENGINE_TAG:
            craftCount = engineCount;
            craftInfo  = Item.GetItemDescription(Item.ItemType.Engine);
            break;
         default:
            Debug.Log("Unknown item tag for resource window.");
            break;
      }

      /* Populate craft components with item-specific data (value, name, sprite).  */
      craftTransform.Find("CraftImage").GetComponent<Image>().sprite         = crafteSprite;
      craftTransform.Find("CraftCount").GetComponent<TextMeshProUGUI>().text = "  x" + craftCount.ToString();
      craftTransform.Find("CraftName").GetComponent<TextMeshProUGUI>().text  = craftTag;
      craftTransform.Find("CraftInfo").GetComponent<TextMeshProUGUI>().text  = craftInfo;

      currentCraft = craftTransform;
      craftTransform.gameObject.SetActive(true);
      ShowCraftWindow();
   }

   public void TryAddPearl(int pearlAmount)
   {
      if (pearlCount > MAX_PEARL_COUNT) 
      {
          Debug.LogError("Pearl count is at maximum!");
          return;
      } 
      else
         if ((pearlCount + pearlAmount) > MAX_PEARL_COUNT) 
             Debug.LogError("Pearl count is at maximum!");
         else
             pearlCount += pearlAmount;
      
      OnPearlCountChanged?.Invoke(pearlCount);
      PearlCountText.text = " x" + pearlCount.ToString();
      
      return;
   }
   
   public void TrySpendPearl(int pearlAmount) 
   {
      if (pearlCount <= MIN_PEARL_COUNT)
      {
         Debug.LogError("Pearl count is at minimum!");
         return;
      } 
      else
         if (pearlCount < pearlAmount) 
         {
            Debug.LogError("Not enough pearls to spend!");
            return;
         } 
         else
            pearlCount -= pearlAmount;
      
      OnPearlCountChanged?.Invoke(pearlCount);
      PearlCountText.text = " x" + pearlCount.ToString();
      
      return;
   }
   
   public void TryAddCrystal(int crystalAmount) 
   {
      if (crystalCount > MAX_CRYSTAL_COUNT) 
      {
          Debug.LogError("Crystal count is at maximum!");
          return;
      } 
      else
         if ((crystalCount + crystalAmount) > MAX_CRYSTAL_COUNT) 
             Debug.LogError("Crystal count is at maximum!");
          else
             crystalCount += crystalAmount;
      
      OnCrystalCountChanged?.Invoke(crystalCount);
      CrystalCountText.text = " x" + crystalCount.ToString();
      
      return;
   }
   
   public void TrySpendCrystal(int crystalAmount) 
   {
      if (crystalCount < MIN_CRYSTAL_COUNT) 
      {
         Debug.LogError("Crystal count is at minimum!");
         return;
      } 
      else
         if (crystalCount < crystalAmount) 
         {
            Debug.LogError("Not enough crystals to spend!");
            return;
         } 
         else
            crystalCount -= crystalAmount;
      
      OnCrystalCountChanged?.Invoke(crystalCount);
      CrystalCountText.text = " x" + crystalCount.ToString();
      
      return;
   }

   public void TryAddOre(int oreAmount)
   {
      if (oreCount > MAX_ORE_COUNT)
      {
         Debug.LogError("Ore count is at maximum!");
         return;
      }
      else
         if ((oreCount + oreAmount) > MAX_ORE_COUNT)
         Debug.LogError("Ore count is at maximum!");
      else
         oreCount += oreAmount;

      OnOreCountChanged?.Invoke(oreCount);
      OreCountText.text = " x" + oreCount.ToString();

      return;
   }

   public void TrySpendOre(int oreAmount)
   {
      if (oreCount <= MIN_ORE_COUNT)
      {
         Debug.LogError("Ore count is at minimum!");
         return;
      }
      else
         if (oreCount < oreAmount)
      {
         Debug.LogError("Not enough ore to spend!");
         return;
      }
      else
         oreCount -= oreAmount;

      OnOreCountChanged?.Invoke(oreCount);
      OreCountText.text = " x" + oreCount.ToString();

      return;
   }
   public bool TryAddCrudeTool(int crudeToolAmount)
   {
      bool isSuccess = false;

      if (crudeToolCount >= MAX_CRUDE_TOOL_COUNT)
      {
         Debug.LogError("Crude tool count is at minimum!");
         return isSuccess;
      }
      else
          if ((crudeToolCount + crudeToolAmount) > MAX_CRUDE_TOOL_COUNT) 
          {
             Debug.LogError("Crystal count is at maximum!");
             return isSuccess;
          }
          else 
              isSuccess = true;
      
      crudeToolCount += crudeToolAmount;
      
      CrudeToolCountText.text = " x" + crudeToolCount.ToString();

      return isSuccess;
   }

   public bool TryUseCrudeTool(int crudeToolAmount)
   {
      bool isSuccess = false;

      if (crudeToolCount <= MIN_CRUDE_TOOL_COUNT)
      {
         Debug.LogError("Crude tool count is at minimum!");
         return isSuccess;
      }
      else
         if (crudeToolCount < crudeToolAmount)
         {
            Debug.LogError("Not enough crude tools!");
            return isSuccess;
         } 
         else 
            isSuccess = true;

      crudeToolCount -= crudeToolAmount;

      CrudeToolCountText.text = " x" + crudeToolCount.ToString();

      return isSuccess;
   }

   public bool TryAddHarpoon(int harpoonAmount)
   {
      bool isSuccess = false;

      if (harpoonCount >= MAX_HARPOON_COUNT)
      {
         Debug.LogError("Refined tool count is at minimum!");
         return isSuccess;
      }
      else
         if ((harpoonCount + harpoonAmount) > MAX_HARPOON_COUNT)
         {    
            Debug.LogError("Refined Tool count is at maximum!");
            return isSuccess;
         } 
         else 
            isSuccess = true;
            
      harpoonCount += harpoonAmount;

      HarpoonCountText.text = " x" + harpoonCount.ToString();

      return isSuccess;
   }
   public bool TryUseHarpoon(int harpoonAmount)
   {
      bool isSuccess = false;

      if (harpoonCount <= MIN_HARPOON_COUNT)
      {
         Debug.LogError("Refined tool count is at minimum!");
         return isSuccess;
      }
      else
         if (harpoonCount < harpoonAmount)
         {
            Debug.LogError("Not enough refined tools!");
            return isSuccess;
         } 
         else 
            harpoonCount -= harpoonAmount;
      
      isSuccess = true;
      HarpoonCountText.text = " x" + harpoonCount.ToString();

      return isSuccess;
   }

   public bool TryAddPressureValve(int pressureValveAmount)
   {
      bool isSuccess = false;

      if (pressureValveCount >= MAX_PRESSURE_VALVE_COUNT)
      {
         Debug.LogError("Pressure valve count is at minimum!");
         return isSuccess;
      }
      else
         if ((pressureValveCount + pressureValveAmount) > MAX_PRESSURE_VALVE_COUNT)
         {
            isSuccess = false;
            Debug.LogError("Refined Tool count is at maximum!");
         }
         else 
            isSuccess = true;
       
      pressureValveCount += pressureValveAmount;

      PressureValveCountText.text = " x" + pressureValveCount.ToString();
      
      return isSuccess;
   }

   public bool TryUsePressureValve(int pressureValveAmount) 
   {
      bool isSuccess = false;

      if (pressureValveAmount <= MIN_PRESSURE_VALVE_COUNT) 
      {
         Debug.LogError("Pressure valve count is at minimum!");
         return isSuccess;
      }
      else 
         if (pressureValveCount < pressureValveAmount)
         {
            Debug.LogError("Not enough pressure valves!");
            return isSuccess;
         }
         else
            isSuccess = true;
      
      pressureValveCount -= pressureValveAmount;

      PressureValveCountText.text = " x" + pressureValveCount.ToString();

      return isSuccess;
   }

   public bool TryAddEngine(int artifactAmount)
   {
      bool isSuccess = false;

      if (engineCount >= MAX_ENGINE_COUNT)
      {
         Debug.LogError("Artifact count is at minimum!");
         return isSuccess;
      }
      else
         if ((engineCount + artifactAmount) > MAX_ENGINE_COUNT)
         { 
            Debug.LogError("Refined Tool count is at maximum!"); 
            return isSuccess;
         }
         else
           isSuccess = true;

      engineCount += artifactAmount;
      EngineCountText.text = " x" + engineCount.ToString();

      return isSuccess;
   }
   public bool TryUseEngine(int engineAmount)
   {
      bool isSuccess = false;

      if (engineCount <= MIN_ENGINE_COUNT)
      {
         Debug.LogError("Engine count is at minimum!");
         return isSuccess;
      }
      else
         if (engineCount < engineAmount)
         {
            Debug.LogError("Not enough engines!");
            return isSuccess;
         }
         else
            isSuccess = true;

      engineCount -= engineAmount;

      EngineCountText.text = " x" + engineCount.ToString();

      return isSuccess;
   }

   public void ShowInventoryPanel() 
   {
      InventoryPanel.gameObject.SetActive(true);
      ResourcePanel.gameObject.SetActive(true);
      // Added for camera fix
      PopUpManager.Instance.DisablePlayerInput();
   }
   
   public void ShowResourcePanel()
   {
      if(CraftsPanel.gameObject.activeSelf)
         CloseCraftsPanel();

      ResourcePanel.gameObject.SetActive(true);
   }
   private void ShowResourceWindow() {
      ResourceWindow.gameObject.SetActive(true);
   }
   private void ShowCraftWindow() {
      CraftWindow.gameObject.SetActive(true);
   }

   public void CloseInventoryPanel() 
   {
      // Added for camera fix
      PopUpManager.Instance.EnablePlayerInput();

      InventoryPanel.gameObject.SetActive(false);

      if(currentCraft != null) 
      {
         Destroy(currentCraft.gameObject);
         currentCraft = null;
      }

      if(currentResource != null) 
      {
         Destroy(currentResource.gameObject);
         currentResource = null;
      }

      if(ResourceWindow.gameObject.activeSelf)
         CloseResourcePanel(); 

      if(CraftWindow.gameObject.activeSelf)
         CloseCraftsPanel();
   }
   private void CloseResourcePanel()
   {
      ResourcePanel.gameObject.SetActive(false);
   }

   public void ShowCraftsPanel()
   {
      if(ResourcePanel.gameObject.activeSelf)
         CloseResourcePanel();

      CraftsPanel.gameObject.SetActive(true);
   }

   private void CloseCraftsPanel()
   {
      CraftsPanel.gameObject.SetActive(false);
   }
}