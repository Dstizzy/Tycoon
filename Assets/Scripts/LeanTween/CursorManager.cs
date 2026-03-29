using UnityEngine;
using UnityEngine.EventSystems; // Crucial for Pointer Events

// This script forces the GameObject to have an EventTrigger and Button component
[RequireComponent(typeof(UnityEngine.UI.Button))]
[RequireComponent(typeof(EventTrigger))]
public class CursorManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
   [Header("Cursor Settings")]
   [Tooltip("The cursor to show when NOT hovering (default).")]
   public Texture2D normalCursor;
   public Vector2 normalHotspot = Vector2.zero;

   [Tooltip("The cursor to show when HOVERING (e.g., Gauntlet).")]
   public Texture2D hoverCursor;
   [Tooltip("Try (20, 12) for your gauntlet pointer finger.")]
   public Vector2 hoverHotspot = new Vector2(20f, 12f);

   public void OnPointerEnter(PointerEventData eventData)
   {
      // Debug.Log("Mouse Entered " + gameObject.name); // Uncomment this to verify detection in console
      Cursor.SetCursor(hoverCursor, hoverHotspot, CursorMode.Auto);
   }

   public void OnPointerExit(PointerEventData eventData)
   {
      // Debug.Log("Mouse Exited " + gameObject.name); // Uncomment this to verify detection in console
      Cursor.SetCursor(normalCursor, normalHotspot, CursorMode.Auto);
   }
}