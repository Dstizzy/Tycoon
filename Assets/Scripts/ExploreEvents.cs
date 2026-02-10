using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "New Event", menuName = "Exploration/Event")]
public class ExploreEvents : ScriptableObject
{
   public string description;
   public EventChoice choiceA;
   public EventChoice choiceB;
}

[System.Serializable]
public struct EventChoice
{
   public string buttonText;

   [Header("Guaranteed Result")]
   public int goldChange;
   public int oreChange;
   public int healthChange;
   public int fuelChange;
   public int inventoryChange;
   public int harpoonChange;
   public int artifactChange;
   public bool waitTurn;

   [Header("Range Result")]
   public int minGold;
   public int maxGold;
   public int minOre;
   public int maxOre;

   [Header("Chance Results")]
   [Range(0, 1)] public float fuelChance;
   public int fuelGain;
   [Range(0, 1)] public float damageChance;
   public int healthDamage;
}