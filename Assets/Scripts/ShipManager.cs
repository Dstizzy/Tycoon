using UnityEngine;

public class ShipManager : MonoBehaviour
{
   [Header("Ship Level Settings")]
   public  int shipLevel = 1;
   private int[] maxHealthByLevel = { 0, 60, 80, 100 };
   private int[] maxFuelByLevel = { 0, 7, 10, 15 };

   [Header("Current Stats")]
   int currentFuel;
   int currentHealth;
   int maxHealth;
   int maxFuel;
   int currentGold;
   int currentOre;
   int currentHarpoon;
   int currentArtifact;
   int currentDepth = 1;

   void Awake()
   {
      UpdateStatsToLevel();
      currentHealth = maxHealth;
      currentFuel = maxFuel;
   }

   public void SetDepth(int newDepth)
   {
      currentDepth = newDepth;
   }

   public int GetDepth()
   {
      return currentDepth;
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
      Debug.Log("gold added to ship inventory: " + results.goldChange);
      currentOre += results.oreChange;
      Debug.Log("ore added to ship inventory: " + results.oreChange);
      currentHealth += results.healthChange;
      Debug.Log("Change to ships health: " + results.healthChange);
      currentFuel += results.fuelChange;
      Debug.Log("Change to ship fuel: " + results.fuelChange);
      currentHarpoon += results.harpoonChange;
      Debug.Log("Ship harpoon inventory change: " + results.harpoonChange);
      currentArtifact += results.artifactChange;
      Debug.Log("Ship artifact inventory change: " + results.artifactChange);

      //handle randomized rewards
      currentGold += Random.Range(results.minGold, results.maxGold + 1);
      currentOre  += Random.Range(results.minOre,  results.maxOre  + 1);

      Debug.Log($"Current ship status - Health: {currentHealth}, Fuel: {currentFuel}");
      Debug.Log($"Gold: {currentGold}, Ore: {currentOre}");
      Debug.Log($"Harpoons: {currentHarpoon}, Artifacts: {currentArtifact}");

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