using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HoverScript : MonoBehaviour
{
   public Camera mainCam;
   private RaycastHit2D raycastHit2D;

   [Header("Building Canvases")]
   [SerializeField] private Transform ForgeCanvas;
   [SerializeField] private Transform OreRefineryCanvas;
   [SerializeField] private Transform ExplorationUnitCanvas;
   [SerializeField] private ShipManager shipManager;

   [Header("Hover Visuals")]
   [SerializeField] private Material outlineMaterial;

   private readonly Dictionary<SpriteRenderer, Material> originalMaterials = new Dictionary<SpriteRenderer, Material>();
   private readonly Dictionary<SpriteRenderer, Material> outlineInstances = new Dictionary<SpriteRenderer, Material>();

   private Transform prevHoverObject;
   private Transform currentHoverObject;
   private bool isHoverEnabled = true;

   public static HoverScript Instance { get; set; }

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
   }

   private void Update()
   {
      if (isHoverEnabled == false)
         return;

      HandleHoverSensitivity();
   }

   private void HandleHoverSensitivity()
   {
      if (mainCam == null)
         mainCam = Camera.main;

      if (mainCam == null)
         return;

      // =========================================================
      // THE FIX: Sync the visual outline to the PopUpManager's state
      // If the PopUpManager has a menu open, lock the outline to that building!
      // =========================================================
      if (PopUpManager.buildingTransform != null)
      {
          currentHoverObject = PopUpManager.buildingTransform;
      }
      else
      {
          // Normal State: No menus are open, so shoot the raycast normally
          Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
          Vector2 mouseWorldPos = mainCam.ScreenToWorldPoint(mouseScreenPos);

          raycastHit2D = Physics2D.Raycast(mouseWorldPos, Vector2.zero, Mathf.Infinity, Physics2D.AllLayers);
          currentHoverObject = NormalizeHoverTarget(raycastHit2D.collider ? raycastHit2D.collider.transform : null);
      }

      if (currentHoverObject != prevHoverObject)
      {
         if (prevHoverObject != null)
            ResetVisuals(prevHoverObject);

         if (currentHoverObject != null)
            ApplyVisuals(currentHoverObject);

         prevHoverObject = currentHoverObject;
      }
   }

   private Transform NormalizeHoverTarget(Transform rawTarget)
   {
      if (rawTarget == null)
         return null;

      HighlightTarget highlightTarget = rawTarget.GetComponentInParent<HighlightTarget>();
      if (highlightTarget != null)
         return highlightTarget.transform;

      return rawTarget;
   }

   private HighlightTarget ResolveHighlightTarget(Transform obj)
   {
      if (obj == null)
         return null;

      HighlightTarget target = obj.GetComponent<HighlightTarget>();
      if (target != null)
         return target;

      return obj.GetComponentInParent<HighlightTarget>();
   }

   private SpriteRenderer ResolveOutlineRenderer(Transform obj)
   {
      if (obj == null)
         return null;

      HighlightTarget highlightTarget = ResolveHighlightTarget(obj);
      if (highlightTarget != null)
      {
         SpriteRenderer highlightRenderer = highlightTarget.GetOutlineRenderer();
         if (highlightRenderer != null)
            return highlightRenderer;
      }

      SpriteRenderer selfRenderer = obj.GetComponent<SpriteRenderer>();
      if (selfRenderer != null)
         return selfRenderer;

      return obj.GetComponentInChildren<SpriteRenderer>();
   }

   private void ApplyVisuals(Transform obj)
   {
      SpriteRenderer renderer = ResolveOutlineRenderer(obj);

      if (renderer != null && obj.tag != "IdleIndicator")
         ApplyOutlineMaterial(renderer);

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
               SetLevelPanel(ExplorationUnitCanvas, shipManager.ShipLevel);
            break;

         case "IdleIndicator":
            if (TickerSystem.Instance != null)
               TickerSystem.Instance.ShowTicker("Take a break: No item is currently being crafted!", Color.white, TickerSystem.MessageTypes.ResultMessage);
            break;
      }
   }

   private void ResetVisuals(Transform obj)
   {
      SpriteRenderer renderer = ResolveOutlineRenderer(obj);
      HighlightTarget highlightTarget = ResolveHighlightTarget(obj);

      bool keepTutorialOutline = highlightTarget != null && highlightTarget.IsForcedHighlightActive;

      if (renderer != null && keepTutorialOutline == false)
         RestoreOriginalMaterial(renderer);

      switch (obj.tag)
      {
         case "Forge":
            HideAllLevels(ForgeCanvas);
            break;

         case "Ore Refinery":
            HideAllLevels(OreRefineryCanvas);
            break;

         case "Exploration Unit":
            HideAllLevels(ExplorationUnitCanvas);
            break;
      }
   }

   private void ApplyOutlineMaterial(SpriteRenderer renderer)
   {
      if (renderer == null || outlineMaterial == null)
         return;

      if (originalMaterials.ContainsKey(renderer) == false)
         originalMaterials.Add(renderer, renderer.sharedMaterial);

      if (outlineInstances.TryGetValue(renderer, out Material outlineInstance) == false || outlineInstance == null)
      {
         outlineInstance = new Material(outlineMaterial);
         outlineInstances[renderer] = outlineInstance;
      }

      if (renderer.sprite != null && outlineInstance.HasProperty("_MainTex"))
         outlineInstance.SetTexture("_MainTex", renderer.sprite.texture);

      renderer.material = outlineInstance;
   }

   private void RestoreOriginalMaterial(SpriteRenderer renderer)
   {
      if (renderer == null)
         return;

      if (originalMaterials.TryGetValue(renderer, out Material originalMaterial))
      {
         renderer.sharedMaterial = originalMaterial;
         originalMaterials.Remove(renderer);
      }

      if (outlineInstances.TryGetValue(renderer, out Material outlineInstance))
      {
         if (outlineInstance != null)
            Destroy(outlineInstance);

         outlineInstances.Remove(renderer);
      }
   }

   public void ApplyTutorialOutline(HighlightTarget target)
   {
      if (target == null)
         return;

      SpriteRenderer renderer = target.GetOutlineRenderer();
      ApplyOutlineMaterial(renderer);
   }

   public void RemoveTutorialOutline(HighlightTarget target)
   {
      if (target == null)
         return;

      SpriteRenderer targetRenderer = target.GetOutlineRenderer();
      if (targetRenderer == null)
         return;

      SpriteRenderer currentHoverRenderer = ResolveOutlineRenderer(currentHoverObject);

      if (currentHoverRenderer == targetRenderer)
      {
         ApplyOutlineMaterial(targetRenderer);
         return;
      }

      RestoreOriginalMaterial(targetRenderer);
   }

   private void HideAllLevels(Transform canvas)
   {
      if (canvas == null)
         return;

      for (int index = 1; index <= 4; index++)
      {
         Transform level = canvas.Find("LVL" + index);
         if (level != null)
            level.gameObject.SetActive(false);
      }
   }

   public void SetLevelPanel(Transform canvas, int level)
   {
      if (canvas == null)
         return;

      HideAllLevels(canvas);

      Transform targetLevel = canvas.Find("LVL" + level);
      if (targetLevel != null)
         targetLevel.gameObject.SetActive(true);
   }

   public void DisableHover()
   {
      isHoverEnabled = false;

      if (prevHoverObject != null)
         ResetVisuals(prevHoverObject);

      prevHoverObject = null;
      currentHoverObject = null;
   }

   public void EnableHover()
   {
      isHoverEnabled = true;
   }

   private void OnDestroy()
   {
      foreach (KeyValuePair<SpriteRenderer, Material> currentPair in outlineInstances)
      {
         if (currentPair.Value != null)
            Destroy(currentPair.Value);
      }

      outlineInstances.Clear();
      originalMaterials.Clear();
   }
}