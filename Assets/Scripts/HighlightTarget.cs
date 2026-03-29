using UnityEngine;

// Reuses the same outline effect already used by HoverScript.
// Add this to a building object and assign the exact sprite transform or sprite renderer target.
public class HighlightTarget : MonoBehaviour
{
   [SerializeField] private Transform outlineTarget;

   public bool IsForcedHighlightActive { get; private set; }

   public Transform OutlineTarget => outlineTarget != null ? outlineTarget : transform;

   public SpriteRenderer GetOutlineRenderer()
   {
      SpriteRenderer renderer = OutlineTarget.GetComponent<SpriteRenderer>();
      if (renderer != null)
         return renderer;

      return OutlineTarget.GetComponentInChildren<SpriteRenderer>();
   }

   public void ShowHighlight()
   {
      IsForcedHighlightActive = true;

      if (HoverScript.Instance != null)
         HoverScript.Instance.ApplyTutorialOutline(this);
   }

   public void HideHighlight()
   {
      IsForcedHighlightActive = false;

      if (HoverScript.Instance != null)
         HoverScript.Instance.RemoveTutorialOutline(this);
   }
}