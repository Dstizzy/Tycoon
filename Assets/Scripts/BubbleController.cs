using UnityEngine;

public class BubbleController : MonoBehaviour 
{
   private void OnMouseEnter()
   {
      if (LabManager.labManager != null)
      {
         LabManager.labManager.CheckAffordableNodes();
      }
   }
}

