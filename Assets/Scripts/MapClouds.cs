using UnityEngine;

public class MapClouds : MonoBehaviour
{
   [Header("Which node does this cloud cover?")]
   public MapNode targetNode; // The specific map node this cloud is hiding

   // When map UI is opened, hides cloud if node is already explored
   private void OnEnable()
   {
      if (targetNode != null)
      {
         if (targetNode.isExplored)
            gameObject.SetActive(false);
         else
            targetNode.OnRevealed += HideCloud;
      }
   }

   // Unsubscribes from the event when map is closed to prevent memory leaks
   private void OnDisable()
   {
      if (targetNode != null)
         targetNode.OnRevealed -= HideCloud;
   }

   // Deactivates a cloud from a map node
   private void HideCloud()
   {
      gameObject.SetActive(false);
   }
}