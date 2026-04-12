using UnityEngine;

// Temporary debug shortcut for testing ending scenes without satisfying gameplay conditions.
public class EndingDebugShortcut : MonoBehaviour
{
   [Header("Debug Keys")]
   [SerializeField] private KeyCode triggerSuccessKey = KeyCode.F8;
   [SerializeField] private KeyCode triggerFailureKey = KeyCode.F9;
   [SerializeField] private bool enableDebugShortcut = true;

   private void Update()
   {
      if (!enableDebugShortcut)
         return;

      if (Input.GetKeyDown(triggerSuccessKey))
      {
         Debug.Log("[EndingDebugShortcut] Triggering success ending.");
         GameEndingState.LoadSuccessEnding();
      }

      if (Input.GetKeyDown(triggerFailureKey))
      {
         Debug.Log("[EndingDebugShortcut] Triggering failure ending.");
         GameEndingState.LoadFailureEnding();
      }
   }
}