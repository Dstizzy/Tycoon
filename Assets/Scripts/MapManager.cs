using JetBrains.Annotations;
using UnityEngine;

public class MapManager : MonoBehaviour
{
   public MapNode currentNode;
   public RectTransform shipIcon;

   public void MoveToNode()
   {
      
      currentNode = currentNode.nextNode;
   }
}
