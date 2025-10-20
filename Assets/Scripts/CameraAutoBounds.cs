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

    void Start()
    {
        if (autoCalculate) CalculateBounds();
    }

    public void CalculateBounds()
    {
        if (targetPanScript == null)
        {
            targetPanScript = GetComponent<CameraDragPan>();
            if (targetPanScript == null)
            {
                Debug.LogWarning("CameraAutoBounds: CameraDragPan script not found.");
                return;
            }
        }

        // Get all renderers in the scene (SpriteRenderer, TilemapRenderer, etc.)
        Renderer[] renderers = FindObjectsOfType<Renderer>();
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

        // Add extra space
        min += Vector2.one * margin;
        max -= Vector2.one * margin;


        // Passing values to the CameraDragPan script
        targetPanScript.minWorld = min;
        targetPanScript.maxWorld = max;
        targetPanScript.clampToBounds = true;

        Debug.Log($"CameraAutoBounds: Automatic boundary calculation complete → min: {min}, max: {max}");
    }
}