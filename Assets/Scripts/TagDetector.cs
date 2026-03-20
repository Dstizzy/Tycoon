using UnityEngine;

public class TagDetector : MonoBehaviour
{
   private void OnEnable()
   {
      // Subscribe to the event
      //PopUpManager.OnHoverTagChanged += HandleHoverChanged;
   }

   private void OnDisable()
   {
      // Unsubscribe to prevent memory leaks
      //PopUpManager.OnHoverTagChanged -= HandleHoverChanged;
   }

   private void HandleHoverChanged(string tag)
   {
      Debug.Log("The user is now hovering over: " + tag);

      if (tag == "Ore Refinery")
      {
         // Do something specific for Ore Refinery
      }
      else if (tag == "None")
      {
         // The mouse left a building and is over empty space
      }
   }
}