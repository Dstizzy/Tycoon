using UnityEngine;

// Controls tutorial highlight objects that are already placed in the scene.
// Recommended usage:
// - Add child objects such as HighlightFrame / GlowRing under the target object
// - Assign them in the Inspector
// - Turn them on/off through this component during tutorial steps
public class HighlightTarget : MonoBehaviour
{
   [SerializeField] private GameObject[] highlightObjects;

   public void ShowHighlight()
   {
      if (highlightObjects == null) return;

      foreach (GameObject currentObject in highlightObjects)
      {
         if (currentObject != null)
            currentObject.SetActive(true);
      }
   }

   public void HideHighlight()
   {
      if (highlightObjects == null) return;

      foreach (GameObject currentObject in highlightObjects)
      {
         if (currentObject != null)
            currentObject.SetActive(false);
      }
   }
}