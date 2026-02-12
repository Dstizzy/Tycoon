using JetBrains.Annotations;
using UnityEngine;

public class MapManager : MonoBehaviour
{
   public static MapManager Instance;
   public MapNode startingNode;
   public MapNode currentNode;
   public RectTransform exploreShipIcon;
   public RectTransform decisionShipIcon;
   public bool winningPathIsLeft;
   [SerializeField] private ShipManager shipManager;

   private void Awake()
   {
      Instance = this;

      winningPathIsLeft = (Random.Range(0, 2) == 0);
   }

   public void MoveToNode(MapNode newNode)
   {
      if (newNode == null) return;

      currentNode = newNode;
      currentNode.isExplored = true;
      shipManager.SetDepth(currentNode.nodeDepth);

      if(exploreShipIcon != null && decisionShipIcon != null)
      {
         exploreShipIcon.anchoredPosition = currentNode.mapPosition;
         decisionShipIcon.anchoredPosition = currentNode.mapPosition;
      }
   }
}
