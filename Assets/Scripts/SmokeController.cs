using UnityEngine;

public class SmokeController : MonoBehaviour
{
   private void OnMouseEnter()
   {
      if (ForgeManager.Instance != null)
      {
         ForgeManager.Instance.ShowQueueInTicker();
      }
   }
}
