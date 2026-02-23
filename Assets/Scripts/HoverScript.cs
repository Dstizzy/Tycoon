using UnityEngine;
using UnityEngine.InputSystem;

public class HoverScript : MonoBehaviour {
    public Camera mainCam;
    RaycastHit2D raycastHit2D;

    [SerializeField] private Transform ForgeCanvas;
    [SerializeField] private Transform OreRefineryCanvas;
    [SerializeField] private Transform ExplorationUnitCanvas;
    [SerializeField] private ShipManager shipManager;
    
    private Transform prevHoverObject;
    private Transform currentHoverObject;
    private PlayerActions playerActions;

    public static HoverScript Instance { get; private set; }

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return; // Exit to prevent duplicate initialization
        }
        Instance = this;

        HideAllLevels(ForgeCanvas);
        HideAllLevels(OreRefineryCanvas);
        HideAllLevels(ExplorationUnitCanvas);

        playerActions = new PlayerActions();
    }

    private void OnEnable() {
        // Subscribe here to ensure callbacks only run while the object is active
        playerActions.PlayerInput.Enable();
        playerActions.PlayerInput.Hover.performed += Hover;
    }

    private void OnDisable() {
        // CRITICAL: Unsubscribe to prevent "MissingReferenceException" after scene load
        playerActions.PlayerInput.Hover.performed -= Hover;
        playerActions.PlayerInput.Disable();
    }

    private void HideAllLevels(Transform canvas) {
        if (canvas == null) return;
        for (int oreLevel = 1; oreLevel <= 4; oreLevel++) {
            Transform level = canvas.Find("LVL" + oreLevel);
            if (level != null) level.gameObject.SetActive(false);
        }
    }

    private void SetLevelPanel(Transform canvas, int level) {
        if (canvas == null) return;
        HideAllLevels(canvas);
        Transform targetLevel = canvas.Find("LVL" + level);
        if (targetLevel != null) targetLevel.gameObject.SetActive(true);
    }

    public void Hover(InputAction.CallbackContext context) {
        // 1. Safety check: Ensure the camera reference is valid for the current scene
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return;

        Vector2 mouseScreenPos = context.ReadValue<Vector2>();
        Vector2 mouseWorldPos = mainCam.ScreenToWorldPoint(mouseScreenPos);

        raycastHit2D = Physics2D.Raycast(mouseWorldPos, Vector2.zero);
        currentHoverObject = raycastHit2D.collider ? raycastHit2D.collider.transform : null;

        // Case A: Moved OFF object
        if (prevHoverObject != null && prevHoverObject != currentHoverObject) {
            SpriteRenderer prevRenderer = prevHoverObject.GetComponentInChildren<SpriteRenderer>();
            if (prevRenderer != null) {
                switch (prevHoverObject.tag) {
                    case "Forge": HideAllLevels(ForgeCanvas); break;
                    case "Ore Refinery": HideAllLevels(OreRefineryCanvas); break;
                    case "Exploration Unit": HideAllLevels(ExplorationUnitCanvas); break;
                }
                prevRenderer.color = Color.white;
            }
        }

        // Case B: Moved ONTO new object
        if (currentHoverObject != null && currentHoverObject != prevHoverObject) {
            SpriteRenderer currentRenderer = currentHoverObject.GetComponentInChildren<SpriteRenderer>();
            if (currentRenderer != null) {
                currentRenderer.color = Color.red;
                switch (currentHoverObject.tag) {
                    case "Forge": SetLevelPanel(ForgeCanvas, ForgeManager.forgeLevel); break;
                    case "Ore Refinery": SetLevelPanel(OreRefineryCanvas, OreRefinery_Manager.Instance.oreLevel); break;
                    case "Exploration Unit": 
                        if (shipManager != null) SetLevelPanel(ExplorationUnitCanvas, shipManager.shipLevel); 
                        break;
                }
            }
        }

        prevHoverObject = currentHoverObject;
    }

   public void DisbaleHover() {
      playerActions.PlayerInput.Disable();
   }

   public void EnableHover() {
      playerActions.PlayerInput.Enable();
   }
}