using System.Runtime.CompilerServices;
using UnityEngine;

public class MapClouds : MonoBehaviour
{
   [Header("Which node does this cloud cover?")]
   public MapNode targetNode;

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

   private void OnDisable()
   {
      if (targetNode != null)
         targetNode.OnRevealed -= HideCloud;
   }

   private void HideCloud()
   {
      gameObject.SetActive(false);
   }
}
