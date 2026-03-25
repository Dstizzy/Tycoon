using UnityEngine;

public class SmokeHoverAnimator : MonoBehaviour
{
   private void OnMouseEnter()
   {
      if (ForgeManager.Instance != null)
      {
         ForgeManager.Instance.ShowQueueInTicker();
      }
   }
}