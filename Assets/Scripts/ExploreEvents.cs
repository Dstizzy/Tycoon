using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "New Event", menuName = "Exploration/Event")]
public class ExploreEvents : ScriptableObject
{
   public string description; // The story narrative text for an event
   public EventChoice choiceA; // The first event choice a player can make
   public EventChoice choiceB; // The second event choice a player can make (may be empty for some events)
}

// A structure that hold the information of exactly what happens when a specific event choice button is clicked
[System.Serializable]
public struct EventChoice
{
   public string buttonText; // Text displayed on the physical button

   [Header("Guaranteed Result")]
   // Flat rate resource changes applied to ship's inventory when button is clicked (use negative values for costs/damage)
   public int pearlChange;
   public int oreChange;
   public int healthChange;
   public int fuelChange;
   public bool waitTurn; // If choice will result in waiting a turn
   public bool loseOre; // If choice will result in losing all ship's ore inventory

   [Header("Range Result")]
   // The minimum and maximums for a choice that results in a random value within a range
   public int minPearl;
   public int maxPearl;
   public int minOre;
   public int maxOre;

   [Header("Tier 2 Settings")]
   public bool requiresLabTier;

   [Header("Chance Results")]
   // A percent chance for fuel to change, and the amount of fuel that would be gained if successful
   [Range(0, 1)] public float fuelChance;
   public int fuelGain;
   // A percent chance for ship's health to change, and the amount of damage that would be taken if occurrs
   [Range(0, 1)] public float damageChance;
   public int healthDamage;
}