using System;
using System.Security.Cryptography;
using UnityEngine;

[CreateAssetMenu(fileName = "NewNode", menuName = "Map/Node")]
public class MapNode : ScriptableObject
{
   public string nodeName;
   public Vector2 mapPosition;
   public bool isExplored = false;
   public int nodeDepth;
   public event Action OnRevealed;

   public enum NodeType { Event, Directional }
   public NodeType type;

   [Header("Directional Settings (If Type = Directional)")]
   [TextArea] public string navigationStory;
   public string choiceAText;
   public MapNode pathA;
   public string choiceBText;
   public MapNode pathB;
   public string choiceCText;
   public MapNode pathC;

   [Header("Event Settings (If Type = Event)")]
   public MapNode nextNode;

   [Header("End Game Settings")]
   public bool isFinalNode;
   public bool isLeftPath;

   public void RevealNode()
   {
      if (!isExplored)
      {
         isExplored = true;
         OnRevealed?.Invoke();
      }
   }
}