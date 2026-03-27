using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
   public static void RestartGame() 
   {
      // =========================================================
      // STEP 1: RESET ALL STATIC VARIABLES
      // Static variables survive scene reloads, so we must manually 
      // set them back to their default values.
      // =========================================================

      // TurnManager Statics
      TurnManager.jamTurnCounter        = 0;
      TurnManager.manualResetOption     = false;
      TurnManager.heatLevel             = 0;
      TurnManager.enemyAttackPercentage = 0;
      TurnManager.userDefends           = false;

      // ForgeManager Statics
      ForgeManager.forgeLevel = 1;

      // LabManager Statics
      LabManager.headUnlocked        = false;
      LabManager.bodyUnlocked        = false;
      LabManager.tailUnlocked        = false;
      LabManager.currentCommerceTier = 0;

      Item.ResetPrices();

      // =========================================================
      // STEP 2: DESTROY PERSISTENT MANAGERS
      // Destroying the GameObjects wipes out all the non-static 
      // lists, dictionaries, and counts (like your inventory).
      // =========================================================
      if (TradeHutManager.Instance != null) 
         Destroy(TradeHutManager.Instance.gameObject);
      if (InventoryManager.Instance != null) 
         Destroy(InventoryManager.Instance.gameObject);
      if (ForgeManager.Instance != null) 
         Destroy(ForgeManager.Instance.gameObject);
      if (TurnManager.Instance != null) 
         Destroy(TurnManager.Instance.gameObject);
      if (LabManager.labManager != null) 
         Destroy(LabManager.labManager.gameObject);
      if (TickerSystem.Instance != null) 
         Destroy(TickerSystem.Instance.gameObject);
      // Add any other managers here (like OreRefinery_Manager, ShipManager, PopUpManager)
      if (OreRefinery_Manager.Instance != null) 
         Destroy(OreRefinery_Manager.Instance.gameObject);
      if(ExplorationUnitManager.Instance != null)
         Destroy(ExplorationUnitManager.Instance.gameObject);


      // =========================================================
      // STEP 3: CLEAR THE SINGLETON REFERENCES
      // This ensures the new instances spawned in the reloaded 
      // scene can safely take over the 'Instance' variable.
      // =========================================================
      TradeHutManager.Instance           = null;
      //ForgeManager.Instance              = null;
      TurnManager.Instance               = null;
      LabManager.labManager              = null;
   }
}