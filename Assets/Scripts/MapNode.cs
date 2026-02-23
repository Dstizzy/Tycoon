using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewNode", menuName = "Map/Node")]
public class MapNode : ScriptableObject
{
   public string nodeName; // The name of the node
   public Vector2 mapPosition; // The exact X/Y coordinates of the UI canvas map
   public bool isExplored = false; // Tracks if the node has been visited
   public int nodeDepth; // The depth tier level of the node
   public event Action OnRevealed; // Event signal that fires the moment the node is discovered

   // Determines whether the node is an event or directional node
   public enum NodeType { Event, Directional }
   public NodeType type;

   // Used when the node is a directional decision at a fork in the path
   [Header("Directional Settings (If Type = Directional)")]
   [TextArea] public string navigationStory; // The narrative text of the decision
   public string choiceAText; // Text for the first path button
   public MapNode pathA; // Node the ship moves to if first button is selected
   public string choiceBText; // Text for the second path button
   public MapNode pathB; // Node the ship moves to if the second button is selected
   public string choiceCText; // Text for the third path button
   public MapNode pathC; // Node the ship moves to if the third button is selected

   // Used when the node is a straight path that brings a random narrative event
   [Header("Event Settings (If Type = Event)")]
   public MapNode nextNode; // The single node that the ship will move to after event decision

   [Header("End Game Settings")]
   public bool isFinalNode;
   public bool isLeftPath;

   // Reveals node on map
   public void RevealNode()
   {
      if (!isExplored)
      {
         isExplored = true;
         OnRevealed?.Invoke();
      }
   }
}