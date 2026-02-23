using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
   public static MapManager Instance; // Allows other scripts to easily access the map
   public MapNode startingNode; // Map node that the launches from
   public MapNode currentNode; // Node the shp is currently resting on
   public bool winningPathIsLeft; // Determines which final node path holds vessel piece
   public RectTransform exploreShipIcon; // UI ship icon on explorePanel map
   public RectTransform decisionShipIcon; // UI ship icon on decisionPanel map
   [SerializeField] private ShipManager shipManager; // Reference to update ship's depth when it moves

   [Header("Game Reset")]
   public List<MapNode> allNodesInGame; // List of all map nodes to reset them

   private void Awake()
   {
      Instance = this;

      // Randomly decide on the winning end node
      winningPathIsLeft = (Random.Range(0, 2) == 0);

      // Resets all nodes to unexplored
      if (allNodesInGame != null)
         foreach (MapNode node in allNodesInGame)
            if (node != null)
               node.isExplored = false;
   }

   // Moves user's ship on map and removes cloud if needed
   public void MoveToNode(MapNode newNode)
   {
      if (newNode == null) return;

      // Update game's state to new location, tell ship manage how deep current node is
      currentNode = newNode;
      shipManager.SetDepth(currentNode.nodeDepth);

      // Move ship icon to new node on map in both explore and decision panels
      if(exploreShipIcon != null && decisionShipIcon != null)
      {
         exploreShipIcon.anchoredPosition = currentNode.mapPosition;
         decisionShipIcon.anchoredPosition = currentNode.mapPosition;
      }
      // Remove the node's cloud from the map
      currentNode.RevealNode();
   }
}
