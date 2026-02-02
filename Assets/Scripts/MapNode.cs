using System.Runtime.CompilerServices;
using UnityEngine;

[CreateAssetMenu(fileName = "NewNode", menuName = "Map/Node")]
public class MapNode : ScriptableObject
{
   public string nodeName;
   public Vector2 mapPosition;
   public bool isExplored = false;
   public int nodeDepth;

   public enum NodeType { Event, Directional }
   public NodeType type;

   [Header("Directional Settings (If Type = Directional)")]
   [TextArea] public string navigationStory;
   public string choiceAText;
   public MapNode pathA;
   public string choiceBText;
   public MapNode pathB;

   [Header("Event Settings (If Type = Event)")]
   public MapNode nextNode;
}