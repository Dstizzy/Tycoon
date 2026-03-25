
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PopUpManager : MonoBehaviour
{
   [SerializeField] private GameObject[] buildingButtonsPreFab;
   [SerializeField] private Camera cam;
   [SerializeField] private TradeHutManager tradeHutManager;
   [SerializeField] private ExplorationUnitManager explorationUnitManager;
   [SerializeField] private OreRefinery_Manager oreRefineryManager;
   [SerializeField] private ForgeManager forgeManager;
   [SerializeField] private LabManager labManager;

   private Transform prevHoverObject;
   private Transform currentHoverObject;

   private List<GameObject> popUps;
   private PlayerActions playerActions;
   private List<RaycastResult> raycastResults = new List<RaycastResult>();
   public static Transform buildingTransform;
   public static bool IsBuildingBlocked = false;

   public static event Action<string> OnHoverTagChanged;

   public static PopUpManager Instance { get; private set; }

   // Added for CameraDragPan update(off when the pop up window is open)
   /* This flag is used by the Camera script to disable */
   /* panning and zooming while a window is open.       */
   public bool IsWindowOpen { get; private set; } = false;

   private void Awake()
   {
      if (Instance != null && Instance != this)
      {
         Destroy(this.gameObject);
         return;
      }
      else
         Instance = this;

      playerActions = new PlayerActions();
      playerActions.PlayerInput.Enable();
      playerActions.PlayerInput.OnBuildingHover.performed += OnBuildingHover;
   }

   private void OnDestroy()
   {
      if (playerActions != null)
      {
         playerActions.PlayerInput.OnBuildingHover.performed -= OnBuildingHover;
         playerActions.PlayerInput.Disable();
         playerActions.Dispose();
      }
   }
   private void OnBuildingHover(InputAction.CallbackContext context)
   {
      if (cam == null)
         return;

      PointerEventData eventData = new PointerEventData(EventSystem.current);
      eventData.position = context.ReadValue<Vector2>();

      /* 2. CLEAR and CHECK FOR UI HITS using RaycastAll                                                                                              */
      // Assuming 'raycastResults' is a List<RaycastResult> and 'EventSystem.current' is accessible
      raycastResults.Clear();
      EventSystem.current.RaycastAll(eventData, raycastResults);

      /* If the list is not empty, a UI element was hit. Ignore hover logic.                                                                          */
      if (raycastResults.Count > 0) return;

      Vector2 mouseScreenPos = eventData.position;

      Vector3 worldPos = cam.ScreenToWorldPoint(mouseScreenPos);

      // Perform a point raycast in 2D physics
      RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

      /* 3. Determine the object hit this frame, or null if nothing was hit                                                                           */
      Transform currentHoverObject = hit.collider ? hit.collider.transform : null;

      if (currentHoverObject != null && currentHoverObject.CompareTag("ForgeSmoke"))
      {
         ClosePopUps();
         return;
      }

      /* =========================================================
       * 4. CORE HOVER LOGIC: Manage Pop-up State
       * =========================================================*/

      /* Case A: Mouse moved OFF the previous object (either to empty space or a new object)                                                          */
      if (prevHoverObject != null && prevHoverObject != currentHoverObject)
      {
         // The mouse is leaving an object. Close the pop-up related to the object we just left.
         ClosePopUps();
         if (prevHoverObject.tag == "Ore Refinery" && OreRefinery_Manager.Instance.IsBlocked)
            //  OreRefinery_Manager.Instance.DeactivateJamButton();
            if (prevHoverObject.tag == "Ore Refinery" && OreRefinery_Manager.Instance.IsBlocked && TurnManager.manualResetOption)
            {
               // OreRefinery_Manager.Instance.DeactivateManualResetCounter();
            }
         buildingTransform = null; // Clear the reference to the old building
      }

      /* Case B: Mouse moved ONTO a new object (a building)                                                                                           */
      if (currentHoverObject != null && currentHoverObject != prevHoverObject)
      {
         // The mouse is entering a new object. Open the pop-up for the new object.
         // We don't need to call ClosePopUps() here because it was handled in Case A.
         buildingTransform = currentHoverObject;
         OnHoverTagChanged?.Invoke(currentHoverObject.tag);

         if (OreRefinery_Manager.Instance.IsBlocked && currentHoverObject.tag == "Ore Refinery")
         {
            if (TurnManager.manualResetOption)
            {
               //OreRefinery_Manager.Instance.ActivateManualResetCounter();
            }
            else
            {
               //OreRefinery_Manager.Instance.ActivateJamButton();
            }
         }
         else
         {
            CreateBuildingButtons(buildingTransform);
         }
      }

      /* 5. Update the state for the next frame                                                                                                       */
      prevHoverObject = currentHoverObject;


      //if (hit.collider != null) {
      //      if (popUps == null || (buildingTransform != null && hit.collider.transform.tag != buildingTransform.tag)) {
      //          ClosePopUps();
      //          buildingTransform = hit.collider.transform;
      //          CreateBuildingButtons(buildingTransform);
      //      } else {
      //          ClosePopUps();
      //      }
      //  } else {
      //      ClosePopUps();
      //  }
   }
   private void CreateBuildingButtons(Transform buildingTransform)
   {
      Vector3 offset = new Vector3(-6.0f, 3.0f, 0f);
      Vector3 fixedPopUpPosition = buildingTransform.position + offset;
      float buttonSpacing = 2.0f;

      popUps = new();

      int buttonCount = (buildingTransform.CompareTag("Exploration Unit") || buildingTransform.CompareTag("Forge")) ? buildingButtonsPreFab.Length : 2;

      for (int buttonIndex = 0; buttonIndex < buttonCount; buttonIndex++)
      {

         GameObject buttonPreFab = buildingButtonsPreFab[buttonIndex];
         GameObject newButton = Instantiate(buttonPreFab, fixedPopUpPosition, Quaternion.identity);
         newButton.tag = "BuildingButton";

         popUps.Add(newButton);

         string uniqueButtonName = buildingTransform.tag switch
         {
            "Trade Hut" => "Trade",
            "Lab" => "Research",
            "Exploration Unit" => "Explore",
            "Forge" => "Craft",
            _ => "BuildingButton"
         };

         string buttonText;

         switch (buttonIndex)
         {
            case 0:
               buttonText = buildingTransform.CompareTag("Ore Refinery") ? "Info" : uniqueButtonName;
               break;
            case 1:
               buttonText = buildingTransform.CompareTag("Ore Refinery") ? "Upgrade" : "Info";
               break;
            case 2:
               buttonText = "Upgrade";
               break;
            default:
               buttonText = "Button";
               break;
         }
         ;

         int buttonId = buttonIndex + 1;

         newButton.GetComponentInChildren<ButtonsPopUp>().SetText(buttonText);
         newButton.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() => OnBuildingButtonClick(buttonId));

         fixedPopUpPosition.y -= buttonSpacing;
      }
   }

   public void OnBuildingButtonClick(int buttonId)
   {
      switch (buildingTransform.tag)
      {
         case "Trade Hut":
            tradeHutManager.RequestTradeHutPanel(buttonId);
            break;
         case "Lab":
            labManager.RequestLabPanel(buttonId);
            break;
         case "Forge":
            forgeManager.RequestForgePanel(buttonId);
            break;
         case "Ore Refinery":
            oreRefineryManager.RequestOreRefinoryPanel(buttonId);
            break;
         case "Exploration Unit":
            explorationUnitManager.RequestExplorationUnitPanel(buttonId);
            break;
         default:
            Debug.Log("Building Panel: Unknown building type.");
            break;
      }
      DisablePlayerInput();
   }

   public void DisablePlayerInput()
   {
      IsWindowOpen = true; // Added for Drag/Pan update; Find the function(s) that OPEN popups
      playerActions.PlayerInput.Disable();
      HoverScript.Instance.DisableHover();
   }
   public void EnablePlayerInput()
   {
      IsWindowOpen = false; // Added for Drag/Pan update; Find the function(s) that CLOSE popups
      playerActions.PlayerInput.Enable();
      HoverScript.Instance.EnableHover();
      ClosePopUps();
   }

   private void ClosePopUps()
   {
      if (popUps != null)
      {
         foreach (GameObject currentPopUp in popUps)
         {
            if (currentPopUp != null)
               Destroy(currentPopUp);
         }
         popUps = null;
         buildingTransform = null;
      }
   }
}
