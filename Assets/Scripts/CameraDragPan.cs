using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Camera))]
public class CameraDragPan : MonoBehaviour
{
   [Header("Pan")]
   [Tooltip("Mouse Drag Sensitivity (Pixel→World Conversion Scale)")]
   public float panSpeed = 1.0f;
   [Tooltip("Right-click and drag (release to left-click)")]
   public bool dragWithRightMouse = true;
   [Tooltip("Reverse drag direction")]
   public bool invert = false;

   [Header("Pan Bounds")]
   [Tooltip("Restrict the camera to within the map boundaries")]
   public bool clampToBounds = true;
   [Tooltip("Minimum/Maximum World Coordinates (CameraAutoBounds set automatically)")]
   public Vector2 minWorld = new Vector2(-50, -50);
   public Vector2 maxWorld = new Vector2(50, 50);
   [Tooltip("The margin (unit) that reduces the drag-movable area further inward than the map")]
   public float panInnerMargin = 1f;

   [Header("Edge Easing (deceleration)")]
   [Tooltip("Smoothly reduce movement as closer to the boundary")]
   public bool edgeEasing = true;
   [Tooltip("Deceleration (unit) within this distance from the boundary")]
   public float edgeCushion = 2.0f;

   [Header("Zoom (Mouse Wheel)")]
   public bool enableWheelZoom = true;
   [Tooltip("Change in Orthographic Size per Wheel Slot")]
   public float zoomStep = 2f;
   public float minSize = 3f;
   public float maxSize = 25f;

   [Header("Vertical Tightness")]
   [Tooltip("Clamp slightly below half the vertical length to prevent top and bottom margins (0.95–0.99 recommended)")]
   [Range(0.9f, 1.0f)]
   public float panVerticalTightness = 0.98f;

   private Camera cam;
   private Vector3 lastMouseScreen;
   private bool dragging = false;

   void Awake()
   {
      cam = GetComponent<Camera>();
      if (!cam.orthographic) cam.orthographic = true;
   }

   void Update()
   {
      // If the popup manager exists and the IsWindowOpen flag is true,
      // exit immediately without executing the camera drag/zoom logic
      if (PopUpManager.Instance != null && PopUpManager.Instance.IsWindowOpen)
      {
         return;
      }

      HandleMousePan();
      if (enableWheelZoom) HandleWheelZoom();
   }

   bool IsPointerOverUI()
   {
      if (EventSystem.current == null) return false;
      if (Input.touchSupported && Input.touchCount > 0)
         return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
      return EventSystem.current.IsPointerOverGameObject();
   }

   void HandleMousePan()
   {
      int btn = dragWithRightMouse ? 1 : 0; // 0:left click, 1:right click

      if (Input.GetMouseButtonDown(btn) && !IsPointerOverUI())
      {
         lastMouseScreen = Input.mousePosition;
         dragging = true;
      }

      if (dragging && Input.GetMouseButton(btn))
      {
         Vector3 deltaPx = Input.mousePosition - lastMouseScreen;
         lastMouseScreen = Input.mousePosition;

         // Pixel → World Conversion (Vertical basis: size*2 == vertical world units on screen)
         float pxToWorld = cam.orthographicSize * 2f / Screen.height;
         Vector3 move = new Vector3(-deltaPx.x * pxToWorld, -deltaPx.y * pxToWorld, 0f);
         if (invert) move = -move;
         move *= panSpeed;

         if (clampToBounds)
            ApplyMoveWithClampAndEasing(move);
         else
            transform.position += move;
      }

      if (Input.GetMouseButtonUp(btn))
         dragging = false;
   }

   // Move only inside the boundary + Slow down near the boundary
   void ApplyMoveWithClampAndEasing(Vector3 move)
   {
      GetPanBounds(out float minX, out float maxX, out float minY, out float maxY);

      Vector3 pos = transform.position;

      // X-axis available distance
      float availRight = maxX - pos.x;
      float availLeft = pos.x - minX;

      // Y-axis available distance
      float availUp = maxY - pos.y;
      float availDown = pos.y - minY;

      // X-axis adjustment
      if (move.x > 0f)
      {
         float factor = Ease(availRight, edgeCushion);
         float capped = Mathf.Min(move.x, Mathf.Max(0f, availRight));
         move.x = capped * factor;
      }
      else if (move.x < 0f)
      {
         float factor = Ease(availLeft, edgeCushion);
         float capped = -Mathf.Min(-move.x, Mathf.Max(0f, availLeft));
         move.x = capped * factor;
      }

      // Y-axis adjustment
      if (move.y > 0f)
      {
         float factor = Ease(availUp, edgeCushion);
         float capped = Mathf.Min(move.y, Mathf.Max(0f, availUp));
         move.y = capped * factor;
      }
      else if (move.y < 0f)
      {
         float factor = Ease(availDown, edgeCushion);
         float capped = -Mathf.Min(-move.y, Mathf.Max(0f, availDown));
         move.y = capped * factor;
      }

      transform.position += move;

      // Final clamp against rounding error
      ClampToBounds();
   }

   float Ease(float available, float cushion)
   {
      if (!edgeEasing) return 1f;
      if (cushion <= 0f) return 1f;
      if (available >= cushion) return 1f;
      float t = Mathf.Clamp01(available / cushion);
      return Mathf.SmoothStep(0f, 1f, t);
   }

   void HandleWheelZoom()
   {
      float scroll = Input.mouseScrollDelta.y;
      if (Mathf.Abs(scroll) < 0.01f) return;

      // Location on the world map where the cursor was pointing before zooming
      Vector3 before = cam.ScreenToWorldPoint(Input.mousePosition);

      float oldSize = cam.orthographicSize;
      float wanted = Mathf.Clamp(oldSize - scroll * zoomStep, minSize, maxSize);
      float newSize = wanted;

      // Maximum zoom-out limit possible on the map
      // (calculated reflecting current margins and tightening)
      if (clampToBounds)
      {
         float mapW = maxWorld.x - minWorld.x;
         float mapH = maxWorld.y - minWorld.y;

         float maxSizeX = Mathf.Max(0.001f, (mapW - 2f * panInnerMargin) / (2f * cam.aspect));
         float maxSizeY = Mathf.Max(0.001f, (mapH - 2f * panInnerMargin) / (2f * panVerticalTightness));

         newSize = Mathf.Min(wanted, maxSizeX, maxSizeY);
         newSize = Mathf.Clamp(newSize, minSize, maxSize);
      }

      // Actual zoom reflection
      cam.orthographicSize = newSize;

      // Calculate the ‘desired’ target position based on cursor offset correction
      Vector3 after = cam.ScreenToWorldPoint(Input.mousePosition);
      Vector3 offset = before - after;
      Vector3 desired = transform.position + offset;

      // Clamp ‘desired’ directly to the center coordinate range allowed at the new zoom level
      if (clampToBounds)
      {
         GetPanBoundsForSize(newSize, out float minX, out float maxX, out float minY, out float maxY);
         desired.x = Mathf.Clamp(desired.x, minX, maxX);
         desired.y = Mathf.Clamp(desired.y, minY, maxY);
      }

      // Instant confirmation without any back-and-forth errors
      transform.position = desired;
   }

   void GetPanBounds(out float minX, out float maxX, out float minY, out float maxY)
   {
      float halfH = cam.orthographicSize * panVerticalTightness; // Tighten the vertical scale
      float halfW = (cam.orthographicSize) * cam.aspect;         // Maintain the horizontal scale

      minX = minWorld.x + halfW + panInnerMargin;
      maxX = maxWorld.x - halfW - panInnerMargin;
      minY = minWorld.y + halfH + panInnerMargin;
      maxY = maxWorld.y - halfH - panInnerMargin;
   }

   void GetPanBoundsForSize(float size, out float minX, out float maxX, out float minY, out float maxY)
   {
      float halfH = size * panVerticalTightness; // Tighten the vertical scale
      float halfW = size * cam.aspect;           // Maintain the horizontal scale

      minX = minWorld.x + halfW + panInnerMargin;
      maxX = maxWorld.x - halfW - panInnerMargin;
      minY = minWorld.y + halfH + panInnerMargin;
      maxY = maxWorld.y - halfH - panInnerMargin;
   }

   void ClampToBounds()
   {
      if (!clampToBounds) return;

      GetPanBounds(out float minX, out float maxX, out float minY, out float maxY);

      Vector3 p = transform.position;
      p.x = Mathf.Clamp(p.x, minX, maxX);
      p.y = Mathf.Clamp(p.y, minY, maxY);
      transform.position = p;
   }
}