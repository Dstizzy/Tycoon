using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ShipManager : MonoBehaviour
{
   // Symbolic Constants
   public const int LAB_T1_HEALTH_BONUS = 10; // Health added to ship from tier 1 lab upgrade
   public const int LAB_T1_FUEL_BONUS = 2; // Fuel added to ship from tier 1 lab upgrade
   public const int LAB_T3_LOOT_MULTIPLIER = 2; // How much loot is multiplied from tier 3 lab upgrade

   [Header("UI References")]
   [SerializeField] private ExplorationUnitManager explorationUnitManager;
   [SerializeField] private Transform fuelPanel; // UI panel informing user that ship's fuel is empty
   [SerializeField] private Transform healthPanel; // UI panel informing user that ship's health is gone
   [SerializeField] private Transform confirmReturnPanel; // UI panel asking user if they want to send ship back
   [SerializeField] private Transform finalRewardsPanel; // UI panel that shows total rewards at the end of exploration
   [SerializeField] private TextMeshProUGUI finalRewards; // Text of all the final exploration rewards
   [SerializeField] private TextMeshProUGUI decisionFuelText; // Text showing current fuel on decision panel
   [SerializeField] private TextMeshProUGUI decisionHealthText; // Text showing current health on decision panel
   [SerializeField] private TextMeshProUGUI exploreFuelText; // Text showing current fuel on explore panel
   [SerializeField] private TextMeshProUGUI exploreHealthText; // Text showing current health on explore panel

   [Header("Ship Level Settings")]
   public int shipLevel { get; private set; } = 1; // Current level of the ship
   private int[] maxHealthByLevel = { 0, 40, 60, 90 }; // Ship's health based on its current level
   private int[] maxFuelByLevel = { 0, 6, 9, 12 }; // Ship's fuel based on its current level

   // Permanent lab upgrade bonuses
   private int labBonusHealth = 0; // Current health added by current lab upgrade status
   private int labBonusFuel = 0; // Current fuel added by lab upgrade status
   private int labLootMultiplier = 1; // How much loot is currently multiplied based on current lab upgrade status

   [Header("Current Stats")]
   // The current stats of the ship
   int currentFuel;
   int currentHealth;
   int maxHealth;
   int maxFuel;
   int currentPearl;
   int currentOre;
   int currentDepth = 1;

   [Header("Ship Inventory - Crafts")]
   public bool isTier2Unlocked = false; // Grants access to event choices resulting in crafts
   // Current inventory of crafts
   int currentPatchKit;
   int currentHarpoon;
   int currentCrudeTool;
   int currentPressureValve;
   int currentDivingBell;
   int currentClockworkEngine;
   int currentPrecisionLens;

   // Data package to send results of an event choice to exploration UI manager to be displayed
   public struct RoundResults
   {
      public int pearlChanged;
      public int oreChanged;
      public int patchKitChanged;
      public int harpoonChanged;
      public int crudeToolChanged;
      public int pressureValveChanged;
      public int divingBellChanged;
      public int clockworkEngineChanged;
      public int precisionLensChanged;
      public int healthChanged;
      public int fuelChanged;

      // Tracks if event changed status or inventory of the ship
      public bool HasChanges()
      {
         return pearlChanged != 0 || oreChanged != 0 || patchKitChanged != 0 || harpoonChanged != 0 ||
                crudeToolChanged != 0 || pressureValveChanged != 0 || divingBellChanged != 0 ||
                clockworkEngineChanged != 0 || precisionLensChanged != 0 || healthChanged != 0 || fuelChanged != 0;
      }
   }

   // Event triggered globally when ship is reset
   public static event Action OnShipDeath;

   void Awake()
   {
      UpdateStatsToLevel();
      currentHealth = maxHealth;
      currentFuel = maxFuel;
   }

   private void Start()
   {
      UpdateShipUI();
   }

   public void SetDepth(int newDepth)
   {
      currentDepth = newDepth;
   }

   // Unlocks ability to select Lab Tier 2 event choices
   public void UnlockTier2Choices()
   {
      isTier2Unlocked = true;
   }

   // Get functions for ship state and inventory
   public int GetDepth() { return currentDepth; }
   public int GetPearl() { return currentPearl; }
   public int GetOre() { return currentOre; }
   public int GetPatchKit() { return currentPatchKit; }
   public int GetHarpoon() { return currentHarpoon; }
   public int GetCrudeTool() { return currentCrudeTool; }
   public int GetPressureValve() { return currentPressureValve; }
   public int GetDivingBell() { return currentDivingBell; }
   public int GetClockworkEngine() { return currentClockworkEngine; }
   public int GetPrecisionLens() { return currentPrecisionLens; }

   // Checks if ship currently has enough resources in inventory to pay for event choices that have a cost
   public bool CanAfford(EventChoice choice)
   {
      if (choice.oreChange < 0 && GetOre() < Mathf.Abs(choice.oreChange)) { return false; }
      if (choice.pearlChange < 0 && GetPearl() < Mathf.Abs(choice.pearlChange)) { return false; }
      return true;
   }

   // Upgrade ship's level
   public void UpgradeShip()
   {
      if (shipLevel < 3)
      {
         shipLevel += 1;
         UpdateStatsToLevel();
      }
   }

   // Recalculates ship's stats based on level and lab bonuses
   public void UpdateStatsToLevel()
   {
      maxHealth = maxHealthByLevel[shipLevel] + labBonusHealth;
      currentHealth = maxHealth;
      maxFuel = maxFuelByLevel[shipLevel] + labBonusFuel;
      currentFuel = maxFuel;

      UpdateShipUI();
   }

   // Update the ship's fuel and health in the decision and explore panels
   private void UpdateShipUI()
   {
      if (decisionFuelText != null)
         decisionFuelText.text = $"{currentFuel}/{maxFuel}";
      if (decisionHealthText != null)
         decisionHealthText.text = $"{currentHealth}/{maxHealth}";
      if (exploreFuelText != null)
         exploreFuelText.text = $"{currentFuel}/{maxFuel}";
      if (exploreHealthText != null)
         exploreHealthText.text = $"{currentHealth}/{maxHealth}";
   }

   // Applies permanent stat boosts to the ship
   public void ApplyLabShipBonus()
   {
      labBonusHealth += LAB_T1_HEALTH_BONUS;
      labBonusFuel += LAB_T1_FUEL_BONUS;
      UpdateStatsToLevel();
   }

   // Turns on the loot multiplier bonus
   public void ApplyLabRewardBonus()
   {
      labLootMultiplier = LAB_T3_LOOT_MULTIPLIER;
   }

   // Calulates all rewards for an event choice
   public RoundResults ApplyEventResult(EventChoice results)
   {
      RoundResults finalResults = new RoundResults();

      // Calculate base and random loot for pearls and ore
      int actualPearl = results.pearlChange + UnityEngine.Random.Range(results.minPearl, results.maxPearl + 1);
      int actualOre = 0;
      if (results.loseOre)
         actualOre = -currentOre;
      else
         actualOre = results.oreChange + UnityEngine.Random.Range(results.minOre, results.maxOre + 1);

      // Apply tier 3 loot multiplier for all positive pearl and ore loot
      if (actualPearl > 0)
         actualPearl *= labLootMultiplier;
      if (actualOre > 0)
         actualOre *= labLootMultiplier;

      // Get random craftable item if lab tier 2 events choice
      int foundPatchKits = 0, foundHarpoons = 0, foundCrudeTools = 0, foundPressureValves = 0, foundDivingBells = 0, foundClockworkEngines = 0, foundPrecisionLenses = 0;
      if (results.requiresLabTier)
      {
         int roll = UnityEngine.Random.Range(0, 7);
         switch (roll)
         {
            case 0: foundPatchKits += 1; break;
            case 1: foundHarpoons += 1; break;
            case 2: foundCrudeTools += 1; break;
            case 3: foundPressureValves += 1; break;
            case 4: foundDivingBells += 1; break;
            case 5: foundClockworkEngines += 1; break;
            case 6: foundPrecisionLenses += 1; break;
         }
      }

      if (results.fuelChance > 0 && UnityEngine.Random.value <= results.fuelChance)
         results.fuelChange += results.fuelGain;
      if (results.damageChance > 0 && UnityEngine.Random.value <= results.damageChance)
         results.healthChange -= results.healthDamage;

      // Apply all event changes to ship's stats and inventory
      currentPearl += actualPearl;
      currentOre += actualOre;
      currentHealth += results.healthChange;
      currentFuel += results.fuelChange;
      currentPatchKit += foundPatchKits;
      currentHarpoon += foundHarpoons;
      currentCrudeTool += foundCrudeTools;
      currentPressureValve += foundPressureValves;
      currentDivingBell += foundDivingBells;
      currentClockworkEngine += foundClockworkEngines;
      currentPrecisionLens += foundPrecisionLenses;

      // Put exact changes into struct for UI manager display
      finalResults.pearlChanged = actualPearl;
      finalResults.oreChanged = actualOre;
      finalResults.patchKitChanged = foundPatchKits;
      finalResults.harpoonChanged = foundHarpoons;
      finalResults.crudeToolChanged = foundCrudeTools;
      finalResults.pressureValveChanged = foundPressureValves;
      finalResults.divingBellChanged = foundDivingBells;
      finalResults.clockworkEngineChanged = foundClockworkEngines;
      finalResults.precisionLensChanged = foundPrecisionLenses;
      finalResults.healthChanged = results.healthChange;
      finalResults.fuelChanged = results.fuelChange;

      // Check for ship fail states
      if (currentFuel <= 0)
      {
         LowFuel();
      }

      if (currentHealth <= 0)
      {
         ShipDestruction();
      }

      UpdateShipUI();

      return finalResults;
   }

   // Calculates damage ship will take if beyond ship's depth level
   public int GetDamage(int depthCheck)
   {
      int damage = 0;
      if (shipLevel == 1 && depthCheck == 2)
         damage = 30;
      else if (shipLevel == 1 && depthCheck == 3)
         damage = 60;
      else if (shipLevel == 2 && depthCheck == 3)
         damage = 40;

      return damage;
   }

   // Handles a new turn in the ship
   public void NewTurn()
   {
      // Burn one fuel and check if empty
      currentFuel -= 1;
      if (currentFuel <= 0)
         LowFuel();

      // Apply potential hull damage and check for destruction
      int hullDamage = GetDamage(currentDepth);
      if (hullDamage > 0)
         currentHealth -= hullDamage;
      if (currentHealth <= 0)
         ShipDestruction();

      UpdateShipUI();
   }

   // Clears ship's inventory, resets health and fuel to max, and resets map position
   public void ResetShip()
   {
      currentFuel = maxFuel;
      currentHealth = maxHealth;
      currentDepth = 1;

      currentPearl = 0;
      currentOre = 0;
      currentPatchKit = 0;
      currentHarpoon = 0;
      currentCrudeTool = 0;
      currentPressureValve = 0;
      currentDivingBell = 0;
      currentClockworkEngine = 0;
      currentPrecisionLens = 0;

      MapManager.Instance.MoveToNode(MapManager.Instance.startingNode);

      UpdateShipUI();
      OnShipDeath?.Invoke();
   }

   // Triggers the fail-state UI sequence when fuel is empty
   public void LowFuel()
   {
      if (currentFuel <= 0)
      {
         OpenFuelPanel();
         Button confirmFuelButton = fuelPanel.Find("OkButton").GetComponent<Button>();
         confirmFuelButton.onClick.RemoveAllListeners();
         confirmFuelButton.onClick.AddListener(() =>
         {
            ClosePanels();
            FinishExploration();
         });
      }
   }

   // Triggers the fail-state UI sequence when health is empty
   public void ShipDestruction()
   {
      OpenHealthPanel();
      Button confirmHealthButton = healthPanel.Find("OkButton").GetComponent<Button>();
      confirmHealthButton.onClick.RemoveAllListeners();
      confirmHealthButton.onClick.AddListener(() =>
      {
         ClosePanels();
         explorationUnitManager.CloseDecisionPanel();
         ResetShip();
      });
   }

   // Opens the panel that tells ship fuel is empty
   private void OpenFuelPanel()
   {
      fuelPanel.gameObject.SetActive(true);
      explorationUnitManager.SetDecisionInteractable(false);
   }

   // Opens the panel that tells ship has been destroyed
   private void OpenHealthPanel()
   {
      healthPanel.gameObject.SetActive(true);
      explorationUnitManager.SetDecisionInteractable(false);
   }

   // Opens panel to confirm ship to return to base
   public void OpenConfirmReturnPanel()
   {
      confirmReturnPanel.gameObject.SetActive(true);
      explorationUnitManager.SetDecisionInteractable(false);

      Button returnShip = confirmReturnPanel.Find("Return").GetComponent<Button>();
      returnShip.onClick.RemoveAllListeners();
      returnShip.onClick.AddListener(() => {
         FinishExploration();
         ClosePanels();
         explorationUnitManager.SetDecisionInteractable(true);
      });
      Button stayOut = confirmReturnPanel.Find("KeepGoing").GetComponent<Button>();
      stayOut.onClick.RemoveAllListeners();
      stayOut.onClick.AddListener(() =>
      {
         ClosePanels();
         explorationUnitManager.SetDecisionInteractable(true);
      });
   }

   // Closes health, fuel, and return panels
   private void ClosePanels()
   {
      healthPanel.gameObject.SetActive(false);
      fuelPanel.gameObject.SetActive(false);
      confirmReturnPanel.gameObject.SetActive(false);
      explorationUnitManager.SetDecisionInteractable(false);
   }

   // Ends a successful exploration, shows total rewards and trasfers inventory to main game inventory
   public void FinishExploration()
   {
      ClosePanels();
      explorationUnitManager.CloseDecisionPanel();

      // Activate and populate total rewards panel
      finalRewardsPanel.gameObject.SetActive(true);
      Button confirmRewards = finalRewardsPanel.Find("Confirm").GetComponent<Button>();
      confirmRewards.onClick.RemoveAllListeners();
      confirmRewards.onClick.AddListener(() =>
      {
         finalRewardsPanel.gameObject.SetActive(false);
         AddRewards();
      });

      // Build summary text for final rewards panel based on ship's current inventory
      string totalRewards = "";
      if (currentPearl > 0)
         totalRewards += $"Pearl: {currentPearl}\n";
      if (currentOre > 0)
         totalRewards += $"Ore: {currentOre}\n";
      if (currentPatchKit > 0)
         totalRewards += $"Patch Kits: {currentPatchKit}\n";
      if (currentHarpoon > 0)
         totalRewards += $"Harpoons: {currentHarpoon}\n";
      if (currentCrudeTool > 0)
         totalRewards += $"Crude Tools: {currentCrudeTool}\n";
      if (currentPressureValve > 0)
         totalRewards += $"Pressure Valve: {currentPressureValve}\n";
      if (currentDivingBell > 0)
         totalRewards += $"Diving Bells: {currentDivingBell}\n";
      if (currentClockworkEngine > 0)
         totalRewards += $"Clockwork Engines: {currentClockworkEngine}\n";
      if (currentPrecisionLens > 0)
         totalRewards += $"Precision Lenses: {currentPrecisionLens}\n";
      finalRewards.text = totalRewards;

      ResetShip();
   }

   // Moves rewards from ship inventory to main game inventory
   private void AddRewards()
   {
      if (InventoryManager.Instance != null)
      {
         if (currentPearl > 0)
            InventoryManager.Instance.TryAddPearl(currentPearl);
         if (currentOre > 0)
            InventoryManager.Instance.TryAddOre(currentOre);
         if (currentPatchKit > 0)
            InventoryManager.Instance.TryAddPatchKit(currentPatchKit);
         if (currentHarpoon > 0)
            InventoryManager.Instance.TryAddHarpoon(currentHarpoon);
         if (currentCrudeTool > 0)
            InventoryManager.Instance.TryAddCrudeTool(currentCrudeTool);
         if (currentPressureValve > 0)
            InventoryManager.Instance.TryAddPressureValve(currentPressureValve);
         if (currentDivingBell > 0)
            InventoryManager.Instance.TryAddDivingBell(currentDivingBell);
         if (currentClockworkEngine > 0)
            InventoryManager.Instance.TryAddEngine(currentClockworkEngine);
         if (currentPrecisionLens > 0)
            InventoryManager.Instance.TryAddPrecisionLens(currentPrecisionLens);
      }
   }
}