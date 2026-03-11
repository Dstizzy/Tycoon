using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraAutoBounds : MonoBehaviour
{
   [Tooltip("Automatic boundary calculation based on SpriteRenderer / TilemapRenderer across the entire scene")]
   public bool autoCalculate = true;

   [Tooltip("Tag filter for objects to find boundaries (leave blank to include all)")]
   public string targetTag = "";

   [Tooltip("CameraDragPan script to apply the results")]
   public CameraDragPan targetPanScript;

   [Tooltip("Value for margin at the boundary (Unit: Unit)")]
   public float margin = 2f;

   // =====================================================================
   // [ADDED] Whether to center the camera after calculating bounds
   // =====================================================================
   [Tooltip("Center the camera on the map after calculating bounds")]
   public bool centerCameraOnStart = true;
   // =====================================================================
   // [END ADDED]
   // =====================================================================

   void Awake()
   {
      // Only do essential component lookups in Awake
      if (targetPanScript == null)
      {
         targetPanScript = GetComponent<CameraDragPan>();
      }
   }

   void Start()
   { // <-- MOVED to Start()
      if (autoCalculate)
      {
         CalculateBounds();
      }
   }

   public void CalculateBounds()
   {
      if (targetPanScript == null)
      {
         targetPanScript = GetComponent<CameraDragPan>();
         if (targetPanScript == null)
         {
            Debug.LogWarning("CameraAutoBounds: targetPanScript is still null. Cannot calculate bounds.");
            return;
         }
      }

      // Get all renderers in the scene (SpriteRenderer, TilemapRenderer, etc.)
      Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
      if (renderers.Length == 0)
      {
         Debug.LogWarning("CameraAutoBounds: There is no renderer. Please check if your scene contains a SpriteRenderer or Tilemap.");
         return;
      }

      bool first = true;
      Vector2 min = Vector2.zero;
      Vector2 max = Vector2.zero;

      foreach (Renderer r in renderers)
      {
         if (!string.IsNullOrEmpty(targetTag) && !r.CompareTag(targetTag))
            continue; // If a tag filter is set

         // =====================================================================
         // [ADDED] Exclude UI elements and particle systems
         // =====================================================================
         // Exclude renderers under UI Canvas
         if (r.GetComponentInParent<Canvas>() != null)
            continue;

         // Exclude ParticleSystemRenderer (can affect background boundaries)
         if (r is ParticleSystemRenderer)
            continue;
         // =====================================================================
         // [END ADDED]
         // =====================================================================

         Bounds b = r.bounds;

         if (first)
         {
            min = b.min;
            max = b.max;
            first = false;
         }
         else
         {
            min = Vector2.Min(min, b.min);
            max = Vector2.Max(max, b.max);
         }
      }

      // =====================================================================
      // [ADDED] Apply margin to slightly reduce the boundaries
      // =====================================================================
      // Apply margin to the boundaries to remove extra space
      min += new Vector2(margin, margin);
      max -= new Vector2(margin, margin);
      // =====================================================================
      // [END ADDED]
      // =====================================================================

      // Passing values to the CameraDragPan script
      targetPanScript.minWorld = min;
      targetPanScript.maxWorld = max;
      targetPanScript.clampToBounds = true;

      Debug.Log($"CameraAutoBounds: Automatic boundary calculation complete → min: {min}, max: {max}");

      // =====================================================================
      // [ADDED] Center the camera after setting the bounds
      // =====================================================================
      if (centerCameraOnStart)
      {
         targetPanScript.CenterCamera();
      }
      // =====================================================================
      // [END ADDED]
      // =====================================================================
   }
}
