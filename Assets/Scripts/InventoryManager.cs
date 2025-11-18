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
   [SerializeField] private Transform InventoryPanel;
   [SerializeField] private Transform ResourcePanel;
   [SerializeField] private Transform ResourceWindow;
   [SerializeField] private Transform CraftsPanel;
   [SerializeField] private Transform ResourceContainer;                                             
   [SerializeField] private Transform ResourceTemplate;                                              
   [SerializeField] private Transform CraftContainer;                                             
   [SerializeField] private Transform CraftTemplate;                                             
   [SerializeField] private Transform ResourceWindowContainer;                                             
   [SerializeField] private Transform ResourceWindowTemplate;                                             
   [SerializeField] private Transform CraftWindow;                                             
   [SerializeField] private Transform CraftWindowContainer;                                             
   [SerializeField] private Transform CraftWindowTemplate;                                             
                                                                                   
   private TextMeshProUGUI PearlCountText;                                         
   private TextMeshProUGUI CrystalCountText;
   private TextMeshProUGUI OreCountText;
   private TextMeshProUGUI CrudeToolCountText;                                       
   private TextMeshProUGUI RefinedToolCountText;                                       
   private TextMeshProUGUI ArtifactCountText;                                       
                                                                                   
   /* Constants                                                                     */
   const int MIN_PEARL_COUNT        = 0;                                                
   const int MIN_CRYSTAL_COUNT      = MIN_PEARL_COUNT;
   const int MIN_ORE_COUNT          = MIN_PEARL_COUNT;

   const int MAX_CRUDE_TOOL_COUNT   = 100;
   const int MAX_REFINED_TOOL_COUNT = 100;
   const int MAX_ARTIFACT_COUNT     = 100;
   const int MIN_CRUDE_TOOL_COUNT   = 0;
   const int MIN_REFINED_TOOL_COUNT = 0;
   const int MIN_ARTIFACT_COUNT     = 0;
   const int MAX_PEARL_COUNT        = 1000;                                             
   const int MAX_CRYSTAL_COUNT      = MAX_PEARL_COUNT;
   const int MAX_ORE_COUNT          = MAX_PEARL_COUNT;
   const int RESOURCE_SPACING       = 30;
   const int PEARL_POSITION         = 0;
   const int CRYSTAL_POSITION       = PEARL_POSITION + 10;
   const int ORE_POSITION           = CRYSTAL_POSITION + 10;
   const int CRUDE_TOOL_POSITION    = 0;
   const int REFINED_TOOL_POSITION  = CRUDE_TOOL_POSITION+ 10;
   const int ARTIFACT_POSITION      = REFINED_TOOL_POSITION + 10;

   /* Public properties                               ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½   ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½  */
   public int pearlCount       { get; private set;  }
   public int crystalCount     { get; private set;  }
   public int oreCount         { get; private set;  }
   public int crudeToolCount   { get; private set;  }
   public int refinedToolCount { get; private set;  }
   public int artifactCount    { get; private set;  }

   /* Private variables ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½      */
   private Transform currentResource;  
   private Transform currentCraft;
   
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

      if(ResourceTemplate == null)
         Debug.LogError("Resource Template is not assigned in the Inspector!");
      else
         ResourceTemplate.gameObject.SetActive(false);

      if(ResourceWindowTemplate == null)
         Debug.LogError("Resource Window Template is not assigned in the Inspector!");
      else
         ResourceWindowTemplate.gameObject.SetActive(false);

      if (CraftTemplate == null || CraftContainer == null) 
      {
         Debug.LogError("CraftTemplate or CraftContainer is not assigned in the Inspector in InventoryManager!", this);
         return;
      }

      if (CraftWindowTemplate == null)
         Debug.LogError("Craft window templateis not assigned in the Inspector");
      else
         CraftWindowTemplate.gameObject.SetActive(false);

      if(CraftWindow == null) 
         Debug.Log("Craft window is ont assigned in the inspector");
      else
         CraftWindow.gameObject.SetActive(false);

      pearlCount       = MIN_PEARL_COUNT;
      crystalCount     = MIN_CRYSTAL_COUNT;
      oreCount         = MIN_ORE_COUNT;
      crudeToolCount   = MIN_CRUDE_TOOL_COUNT;
      refinedToolCount = MIN_REFINED_TOOL_COUNT;
      artifactCount    = MIN_ARTIFACT_COUNT;
   }
   
   /* Creates the display elements for Pearls and Crystals on the inventory panel. */
   private void Start() 
   {
      CreateResource(Resources.GetResourceSprite(Resources.ResourceType.Pearl), PEARL_POSITION, "Pearl");
      CreateResource(Resources.GetResourceSprite(Resources.ResourceType.Crystal), CRYSTAL_POSITION, "Crystal");
      CreateResource(Resources.GetResourceSprite(Resources.ResourceType.Ore), ORE_POSITION, "Ore");

      CreateCraft(Item.GetItemSprite(Item.ItemType.CrudeTool), CRUDE_TOOL_POSITION, "Crude Tool");
      CreateCraft(Item.GetItemSprite(Item.ItemType.RefinedTool), REFINED_TOOL_POSITION, "Refined Tool");
      CreateCraft(Item.GetItemSprite(Item.ItemType.Artifact), ARTIFACT_POSITION, "Artifact");
   }
   
   /* Creates and positions a resource display element in the inventory panel. ï¿½   */
   private void CreateResource(Sprite resourceSprite, float positionIndex,  string resourceTag)           
   {                                                                                
      int           resourceCount;
      Button        resourceWindowButton;
      Transform     resourceTransform;
      RectTransform resourceRectTransform;

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
      resourceTransform = Instantiate(ResourceTemplate, ResourceContainer);
      
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
          case "Pearl":
             PearlCountText   = resourceTransform.Find("ResourceCount").GetComponent<TextMeshProUGUI>();
             break;
          case "Crystal":
             CrystalCountText = resourceTransform.Find("ResourceCount").GetComponent<TextMeshProUGUI>();
             break;
          case "Ore":
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
      int    craftCount;
      Button craftWindowButton;
      switch (craftTag) 
      {
         case "Crude Tool":
            craftCount = crudeToolCount;
            break;
         case "Refined Tool":
            craftCount = crystalCount;
            break;
         case "Artifact":
            craftCount = artifactCount;
            break;
         default:
            craftCount = 0;
            Debug.LogError("Unknown craft tag: " + craftTag);
            break;
      }

      /* Instantiate the craft template and set its position in the container.     */
      /* Transform of the newly created resource UI element. ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½     */
      Transform craftTransform = Instantiate(CraftTemplate, CraftContainer);
      
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
         case "Crude Tool":
            CrudeToolCountText   = craftTransform.Find("CraftCount").GetComponent<TextMeshProUGUI>();
            break;
         case "Refined Tool":
            RefinedToolCountText = craftTransform.Find("CraftCount").GetComponent<TextMeshProUGUI>();
            break;
         case "Artifact":
            ArtifactCountText    = craftTransform.Find("CraftCount").GetComponent<TextMeshProUGUI>();
            break;
         default:
            Debug.LogError("Unkown craft");
            break;
      }

      /* Dynamically add listeners to the buttons, which creates the craft window  */
      craftWindowButton.onClick.AddListener(() => { CreateCraftWindow(craftSprite, craftTag); } );
     
      craftTransform.gameObject.SetActive(true);
   }

   /* Creates and populates the resource information window                        */
   private void CreateResourceWindow(Sprite resourceSprite, string resourceTag) 
   {
      int    resourceCount = 0;
      string resourceInfo  = "";

      /* Instantiate the resource template and set its position in the container.  */
      /* Transform of the newly created resource UI element. ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½ ï¿½     */
      Transform     resourceTransform     = Instantiate(ResourceWindowTemplate, ResourceWindowContainer);
      RectTransform resourceRectTransform = resourceTransform.GetComponent<RectTransform>();

      /* Destroys the current resource in the window if it exists.                 */
      if (currentResource != null) 
      {
         Destroy(currentResource.gameObject);
         currentResource = null;
      }

      resourceTransform.tag = resourceTag;

      switch (resourceTag) {
         case "Pearl":
            resourceCount = pearlCount;
            resourceInfo  = Resources.GetResourceDescription(Resources.ResourceType.Pearl);
            break;
         case "Crystal":
            resourceCount = crystalCount;
            resourceInfo  = resourceInfo = Resources.GetResourceDescription(Resources.ResourceType.Crystal);
            break;
         case "Ore":
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
      Debug.Log("In CreateCraft Method");

      int craftCount = 0;
      string craftInfo  = "";

      /* Instantiate the resource template and set its position in the container.  */
      /* Transform of the newly created resource UI element.                       */
      Transform     craftTransform     = Instantiate(CraftWindowTemplate, CraftWindowContainer);
      RectTransform craftRectTransform = craftTransform.GetComponent<RectTransform>();

      /* Destroys the current craft in the window if it exists.                    */
      if (currentCraft != null) {
         Destroy(currentCraft.gameObject);
         currentCraft = null;
      }

      craftTransform.tag = craftTag;

      switch (craftTag) 
      {
         case "Crude Tool":
            craftCount = pearlCount;
            craftInfo  = Item.GetItemDescription(Item.ItemType.CrudeTool);
            break;
         case "Refined Tool":
            craftCount = crystalCount;
            craftInfo  = Item.GetItemDescription(Item.ItemType.RefinedTool);
            break;
         case "Artifact":
            craftCount = artifactCount;
            craftInfo  = Item.GetItemDescription(Item.ItemType.Artifact);
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
   public void TryAddCrudeTool(int crudeToolAmount)
   {
      if (crudeToolCount >= MAX_CRUDE_TOOL_COUNT)
      {
         Debug.LogError("Crude tool count is at minimum!");
         return;
      }
      else
          if ((crudeToolCount + crudeToolAmount) > MAX_CRUDE_TOOL_COUNT)
             Debug.LogError("Crystal count is at maximum!");
          else
             crudeToolCount += crudeToolAmount;
      
      CrudeToolCountText.text = " x" + crudeToolCount.ToString();

      return;
   }
   public void TryUseCrudeTool(int crudeToolAmount)
   {
      if (crudeToolCount < MIN_CRUDE_TOOL_COUNT)
      {
         Debug.LogError("Crude tool count is at minimum!");
         return;
      }
      else
         if (crudeToolCount < crudeToolAmount)
         {
            Debug.LogError("Not enough crude tools!");
            return;
         }
         else
            crudeToolCount -= crudeToolAmount;
        
         CrudeToolCountText.text = " x" + crudeToolCount.ToString();

      return;
   }

   public void TryAddRefinedTool(int refinedToolAmount)
   {
      if (refinedToolCount >= MAX_REFINED_TOOL_COUNT)
      {
         Debug.LogError("Refined tool count is at minimum!");
         return;
      }
      else
         if ((refinedToolCount + refinedToolAmount) > MAX_REFINED_TOOL_COUNT)
            Debug.LogError("Refined Tool count is at maximum!");
         else
            refinedToolCount += refinedToolAmount;

      RefinedToolCountText.text = " x" + refinedToolCount.ToString();

      return;
   }
   public void TryUseRefinedTool(int refinedToolAmount)
   {
      if (refinedToolCount < MIN_REFINED_TOOL_COUNT)
      {
         Debug.LogError("Refined tool count is at minimum!");
         return;
      }
      else
         if (refinedToolCount < refinedToolAmount)
         {
            Debug.LogError("Not enough refined tools!");
            return;
         }
         else
         refinedToolCount -= refinedToolAmount;

      RefinedToolCountText.text = " x" + refinedToolCount.ToString();

      return;
   }

   public void TryAddArtifact(int artifactAmount)
   {
      if (artifactCount >= MAX_ARTIFACT_COUNT)
      {
         Debug.LogError("Artifact count is at minimum!");
         return;
      }
      else
         if ((artifactCount + artifactAmount) > MAX_ARTIFACT_COUNT)
            Debug.LogError("Refined Tool count is at maximum!");
         else
            artifactCount += artifactAmount;

      ArtifactCountText.text = " x" + artifactCount.ToString();

      return;
   }
   public void TryUseArtifacts(int artifactAmount)
   {
      if (artifactCount < MIN_ARTIFACT_COUNT)
      {
         Debug.LogError("Artifact count is at minimum!");
         return;
      }
      else
         if (artifactCount < artifactAmount)
         {
            Debug.LogError("Not enough artifacts!");
            return;
         }
         else
         artifactCount -= artifactAmount;

      ArtifactCountText.text = " x" + artifactCount.ToString();

      return;
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