using UnityEngine;

public class ShipManager : MonoBehaviour
{
   [Header("Ship Level Settings")]
   public  int shipLevel = 1;
   private int[] maxHealthByLevel = { 0, 60, 80, 100 };
   private int[] maxFuelByLevel = { 7, 10, 15 };

   [Header("Current Stats")]
   int currentFuel;
   int currentHealth;
   int maxHealth;
   int maxFuel;
   int currentGold;
   int currentOre;
   int currentHarpoon;
   int currentArtifact;

   void Awake()
   {
      UpdateStatsToLevel();
      currentHealth = maxHealth;
      currentFuel = maxFuel;
   }
   public void UpgradeShip()
   {
      if(shipLevel < 3)
      {
         shipLevel += 1;
         UpdateStatsToLevel();
      }
   }

   public void UpdateStatsToLevel()
   {
      maxHealth = maxHealthByLevel[shipLevel];
      maxFuel = maxFuelByLevel[shipLevel];
   }

   public void ApplyEventResult(EventChoice results)
   {
      currentGold += results.goldChange;
      currentOre += results.oreChange; 
      currentHealth += results.healthChange;
      currentFuel += results.fuelChange;
      currentHarpoon += results.harpoonChange;
      currentArtifact += results.artifactChange;

      //handle randomized rewards
      currentGold += Random.Range(results.minGold, results.maxGold + 1);
      currentOre  += Random.Range(results.minOre,  results.maxOre  + 1);

      if(currentHealth > maxHealth)
      {
         ShipDestruction();
      }
   }

   public void ShipDestruction()
   {
      Debug.Log("Your ship has been destroyed.");
   }
}