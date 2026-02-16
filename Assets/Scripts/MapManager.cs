using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct NodeVisual
{
   public MapNode nodeData;
   public GameObject cloudObject;
}

public class MapManager : MonoBehaviour
{
   public static MapManager Instance;
   public MapNode startingNode;
   public MapNode currentNode;
   public RectTransform exploreShipIcon;
   public RectTransform decisionShipIcon;
   public bool winningPathIsLeft;
   [SerializeField] private ShipManager shipManager;

   [Header("Game Reset")]
   public List<MapNode> allNodesInGame;

   private void Awake()
   {
      Instance = this;
      winningPathIsLeft = (Random.Range(0, 2) == 0);

      if (allNodesInGame != null)
         foreach (MapNode node in allNodesInGame)
            if (node != null)
               node.isExplored = false;
   }

   public void MoveToNode(MapNode newNode)
   {
      if (newNode == null) return;

      currentNode = newNode;
      shipManager.SetDepth(currentNode.nodeDepth);

      if(exploreShipIcon != null && decisionShipIcon != null)
      {
         exploreShipIcon.anchoredPosition = currentNode.mapPosition;
         decisionShipIcon.anchoredPosition = currentNode.mapPosition;
      }
      currentNode.RevealNode();
   }
}
