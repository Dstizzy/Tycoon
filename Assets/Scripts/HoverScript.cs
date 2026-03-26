using UnityEngine;
using UnityEngine.InputSystem;

public class HoverScript : MonoBehaviour
{
   public Camera mainCam;
   RaycastHit2D raycastHit2D;

   [Header("Building Canvases")]
   [SerializeField] private Transform ForgeCanvas;
   [SerializeField] private Transform OreRefineryCanvas;
   [SerializeField] private Transform ExplorationUnitCanvas;
   [SerializeField] private ShipManager shipManager;

   [Header("Hover Visuals")]
   [SerializeField] private Material outlineMaterial;
   private Material defaultMaterial;

   private Transform prevHoverObject;
   private Transform currentHoverObject;
   private PlayerActions playerActions;

   public static HoverScript Instance { get; private set; }

   private void Awake()
   {
      if (Instance != null && Instance != this)
      {
         Destroy(gameObject);
         return;
      }
      Instance = this;

      HideAllLevels(ForgeCanvas);
      HideAllLevels(OreRefineryCanvas);
      HideAllLevels(ExplorationUnitCanvas);

      playerActions = new PlayerActions();
   }

   private void OnEnable()
   {
      if (playerActions != null)
         playerActions.PlayerInput.Enable();
   }

   private void OnDisable()
   {
      if (playerActions != null)
         playerActions.PlayerInput.Disable();
   }

   private void Update()
   {
      HandleHoverSensitivity();
   }

   private void HandleHoverSensitivity()
   {
      // 1. Camera Safety Check
      if (mainCam == null) mainCam = Camera.main;
      if (mainCam == null) return;

      // 2. Get mouse position directly from Input System
      Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
      Vector2 mouseWorldPos = mainCam.ScreenToWorldPoint(mouseScreenPos);

      // 3. Raycast to find buildings
      raycastHit2D = Physics2D.Raycast(mouseWorldPos, Vector2.zero, Mathf.Infinity, Physics2D.AllLayers);
      currentHoverObject = raycastHit2D.collider ? raycastHit2D.collider.transform : null;

      // 4. Logic: If the object under the mouse changed...
      if (currentHoverObject != prevHoverObject)
      {
         // RESET the old object
         if (prevHoverObject != null)
         {
            ResetVisuals(prevHoverObject);
         }

         // APPLY to the new object
         if (currentHoverObject != null)
         {
            ApplyVisuals(currentHoverObject);
         }

         prevHoverObject = currentHoverObject;
      }
   }

   private void ApplyVisuals(Transform obj)
   {
      SpriteRenderer renderer = obj.GetComponentInChildren<SpriteRenderer>();

      if (renderer != null && obj.tag != "IdleIndicator")
      {
         // Store original material so we can revert later
         defaultMaterial = renderer.material;

         // Swap to outline
         if (outlineMaterial != null)
            renderer.material = outlineMaterial;
      }

      // Trigger the Level Panels (LVL 1, LVL 2 etc)
      switch (obj.tag)
      {
         case "Forge":
            SetLevelPanel(ForgeCanvas, ForgeManager.forgeLevel);
            break;
         case "Ore Refinery":
            SetLevelPanel(OreRefineryCanvas, OreRefinery_Manager.Instance.oreLevel);
            break;
         case "Exploration Unit":
            if (shipManager != null)
               SetLevelPanel(ExplorationUnitCanvas, shipManager.shipLevel);
            break;
         case "IdleIndicator":
            if (TickerSystem.Instance != null)
               TickerSystem.Instance.ShowTicker("Take a break: No item is currently being crafted!", Color.white, TickerSystem.MessageTypes.ResultMessage);
            break;
      }
   }

   private void ResetVisuals(Transform obj)
   {
      SpriteRenderer renderer = obj.GetComponentInChildren<SpriteRenderer>();
      if (renderer != null)
      {
         // Put the original material back
         if (defaultMaterial != null)
            renderer.material = defaultMaterial;

         // Hide the UI panels
         switch (obj.tag)
         {
            case "Forge": HideAllLevels(ForgeCanvas); break;
            case "Ore Refinery": HideAllLevels(OreRefineryCanvas); break;
            case "Exploration Unit": HideAllLevels(ExplorationUnitCanvas); break;
         }
      }
   }

   private void HideAllLevels(Transform canvas)
   {
      if (canvas == null) return;
      for (int i = 1; i <= 4; i++)
      {
         Transform level = canvas.Find("LVL" + i);
         if (level != null) level.gameObject.SetActive(false);
      }
   }

   private void SetLevelPanel(Transform canvas, int level)
   {
      if (canvas == null) return;
      HideAllLevels(canvas);
      Transform targetLevel = canvas.Find("LVL" + level);
      if (targetLevel != null) targetLevel.gameObject.SetActive(true);
   }

   public void DisableHover() { if (playerActions != null) playerActions.PlayerInput.Disable(); }
   public void EnableHover() { if (playerActions != null) playerActions.PlayerInput.Enable(); }
}