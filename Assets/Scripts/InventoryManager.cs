/* Libraries and references                                                         */
using System;                                                                       
using TMPro;                                                                        
using UnityEngine;                                                                  
using UnityEngine.UI;                                                               
                                                                                    
public class InventoryManager : MonoBehaviour {                                     
   /* Holds a reference to the singleton instance of this class. � � � � � � �     */
   public static InventoryManager Instance { get; private set; }                   
                                                                                    
   /* Inspector variables for UI elements. � � � � � � � � � � � � � �              */
   public Transform InventoryPanel;
   public Transform ResourcePanel;
   public Transform CraftsPanel;
   public Transform ResourceContainer;                                             
   public Transform ResourceTemplate;                                              
   public Transform CraftContainer;                                             
   public Transform CraftTemplate;                                             
                                                                                   
   private TextMeshProUGUI PearlCountText;                                         
   private TextMeshProUGUI CrystalCountText;                                       
   private TextMeshProUGUI CrudeToolCountText;                                       
   private TextMeshProUGUI RefinedToolCountText;                                       
   private TextMeshProUGUI ArtifactCountText;                                       
                                                                                   
   /* Constants                                                                     */
   const int MIN_PEARL_COUNT        = 0;                                                
   const int MIN_CRYSTAL_COUNT      = MIN_PEARL_COUNT;                                  
   const int MIN_CRUDE_TOOL_COUNT   = 0;
   const int MIN_REFINED_TOOL_COUNT = 0;
   const int MIN_ARTIFACT_COUNT     = 0;
   const int MAX_PEARL_COUNT        = 1000;                                             
   const int MAX_CRYSTAL_COUNT      = MAX_PEARL_COUNT;                                  
   const int RESOURCE_SPACING       = 30;
   const int PEARL_POSITION         = 0;
   const int CRYSTAL_POSITION       = PEARL_POSITION + 10;
   const int CRUDE_TOOL_POSITION    = 0;
   const int REFINED_TOOL_POSITION  = CRUDE_TOOL_POSITION+ 10;
   const int ARTIFACT_POSITION      = REFINED_TOOL_POSITION + 10;

   /* Public properties                               � � � � � � �   � � � � � �  */
   public int pearlCount  {  get; private set; }
   public int crystalCount  { get; private set; }
   public int crudeToolCount { get; private set; }
   public int refinedToolCount { get; private set; }
   public int artifactCount { get; private set; }

   /* Delegate for when the pearl count changes. � � � � � � � � � � � � � � � �   */
   public Action<int> OnPearlCountChanged;                                         
   /* Delegate for when the crystal count changes. � � � � � � � � � � � � � � �   */
   public Action<int> OnCrystalCountChanged;                                       
                                                                                   
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

      pearlCount       = MIN_PEARL_COUNT;
      crystalCount     = MIN_CRYSTAL_COUNT;
      crudeToolCount   = MIN_CRUDE_TOOL_COUNT;
      refinedToolCount = MIN_REFINED_TOOL_COUNT;
      artifactCount    = MIN_ARTIFACT_COUNT;
   }
   
   /* Creates the display elements for Pearls and Crystals on the inventory panel. */
   private void Start() 
   {
      CreateResource(Resource.GetResourceSprite(Resource.ResourceType.Pearl), "Pearl", PEARL_POSITION, "Pearl");
      CreateResource(Resource.GetResourceSprite(Resource.ResourceType.Crystal), "Crystal", CRYSTAL_POSITION, "Crystal");

      CreateCraft(Item.GetItemSprite(Item.ItemType.CrudeTool), "CrudeTool", CRUDE_TOOL_POSITION, "Crude Tool");
      CreateCraft(Item.GetItemSprite(Item.ItemType.RefinedTool), "Refined Tool", REFINED_TOOL_POSITION, "Refined Tool");
      CreateCraft(Item.GetItemSprite(Item.ItemType.Artifact), "Artifact", ARTIFACT_POSITION, "Artifact");
   }
   
   /* Creates and positions a resource display element in the inventory panel. �   */
   private void CreateResource(Sprite resourceSprite, string resourceName,          
                                float positionIndex,  string resourceTag)           
   {                                                                                
      /* The initial count of the resource to display. � � � � � � � � � � � � �   */
      int resourceCount;
      
      switch (resourceTag) 
      {
         case "Pearl":
            resourceCount = pearlCount;
            break;
         case "Crystal":
            resourceCount = crystalCount;
            break;
         default:
            resourceCount = 0;
            break;
      }
        
      /* Instantiate the template and set its position in the container.
      /* Transform of the newly created resource UI element. � � � � � � � � �     */
      Transform ResourceTransform = Instantiate(ResourceTemplate, ResourceContainer);
      
     /* RectTransform for positioning the new resource UI element. � � � � � �     */
      RectTransform ResourceRectTransform = ResourceTransform.GetComponent<RectTransform>();
      
      ResourceTransform.tag = resourceTag;
   
     /* Places the new resource entry in a horizontal row inside the inventory     */
     ResourceRectTransform.anchoredPosition = new Vector2(RESOURCE_SPACING * positionIndex, 0);
   
     /* Populate the resource count text and Image components with item-specific   */
     /* data                                                                       */
     ResourceTransform.Find("ResourceSprite").GetComponent<Image>().sprite        = resourceSprite;
     ResourceTransform.Find("ResourceCount").GetComponent<TextMeshProUGUI>().text = " x" + resourceCount.ToString();
      
     switch (resourceTag) 
     { 
         case "Pearl":
            PearlCountText   = ResourceTransform.Find("ResourceCount").GetComponent<TextMeshProUGUI>();
            break;
         case "Crystal":
            CrystalCountText = ResourceTransform.Find("ResourceCount").GetComponent<TextMeshProUGUI>();
            break;
         default:
            break;
      }

      ResourceTransform.gameObject.SetActive(true);
   }

   private void CreateCraft(Sprite craftSprite,   string craftName,          
                             float positionIndex, string craftTag)           
   {                                                                                
      /* The initial count of the crafts to display. � � � � � � � � � � � � �   */
      int craftCount;
      
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
            break;
      }

      if (CraftTemplate == null || CraftContainer == null) {
         Debug.LogError("CraftTemplate or CraftContainer is not assigned in the Inspector in InventoryManager!", this);
         return; // Exit the method to avoid NullReferenceException
      }

      /* Instantiate the template and set its position in the container.
      /* Transform of the newly created resource UI element. � � � � � � � � �     */
      Transform craftTransform = Instantiate(CraftTemplate, CraftContainer);
      
     /* RectTransform for positioning the new resource UI element. � � � � � �     */
      RectTransform craftRectTransform = craftTransform.GetComponent<RectTransform>();
      
      craftTransform.tag = craftTag;
   
     /* Places the new resource entry in a horizontal row inside the inventory     */
     craftRectTransform.anchoredPosition = new Vector2(RESOURCE_SPACING * positionIndex, 0);
   
     /* Populate the resource count text and Image components with item-specific   */
     /* data                                                                       */
     craftTransform.Find("CraftSprite").GetComponent<Image>().sprite        = craftSprite;
     craftTransform.Find("CraftCount").GetComponent<TextMeshProUGUI>().text = " x" + craftCount.ToString();
      
     switch (craftTag) 
     { 
         case "Crude tool":
            CrudeToolCountText   = craftTransform.Find("CraftCount").GetComponent<TextMeshProUGUI>();
            break;
         case "Refined Tool":
            RefinedToolCountText = craftTransform.Find("CraftCount").GetComponent<TextMeshProUGUI>();
            break;
         case "Artifact":
            ArtifactCountText    = craftTransform.Find("CraftCount").GetComponent<TextMeshProUGUI>();
            break;
         default:
            break;
      }

      craftTransform.gameObject.SetActive(true);
   }
   
   /* Attempts to add the specified amount of pearls to the player's count. � �    */
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
   
   /* Attempts to deduct the specified amount of pearls from the player's count.   */
   public void TrySpendPearl(int pearlAmount) 
   {
      if (pearlCount < MIN_PEARL_COUNT) 
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
   
   
   /* Attempts to add the specified amount of crystals to the player's count. �    */
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
   
   /* Attempts to deduct the specified amount of crystals from the player's count. */
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
   
   /* Displays the inventory panel to the user. � � � � � � � � � � � � � � � � �  */
   public void ActivateInventoryPanel() 
   {
      InventoryPanel.gameObject.SetActive(true);
      ResourcePanel.gameObject.SetActive(true);
   }
   
   /* Hides the inventory panel from the user. � � � � � � � � � � � � � � � �  �  */
   public void DeactivateInventoryPanel() 
   {
      InventoryPanel.gameObject.SetActive(false);
   }

   public void ActivateResourcePanel()
   {
      if(CraftsPanel.gameObject.activeSelf)
         DeactivateCraftsPanel();

      ResourcePanel.gameObject.SetActive(true);
   }

   private void DeactivateResourcePanel()
   {
      ResourcePanel.gameObject.SetActive(false);
   }

   public void ActivateCraftsPanel()
   {
      if(ResourcePanel.gameObject.activeSelf)
         DeactivateResourcePanel();

      CraftsPanel.gameObject.SetActive(true);
   }

   private void DeactivateCraftsPanel()
   {
      CraftsPanel.gameObject.SetActive(false);
   }
}