using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using Unity.Plastic.Antlr3.Runtime.Tree;

public class ShipManager : MonoBehaviour
{
   // Symbolic Constants
   public const int MAX_LEVEL              = 3;    // Maximum ship level to be reached
   public const int NUM_OF_CRAFTS          = 7;    // Number of crafts that can be discovered
   public const int LAB_T1_HEALTH_BONUS    = 10;   // Additional health from first Laboratory upgrade
   public const int LAB_T1_FUEL_BONUS      = 2;    // Additional fuel from first laboratory upgrade
   public const int LAB_T3_LOOT_MULTIPLIER = 2;    // Scalar applied to positive loot draws after third lab upgrade
   public const int LEVEL_1_IN_DEPTH_2     = 30;   // Damage taken by level 1 ship in depth 2
   public const int LEVEL_1_IN_DEPTH_3     = 60;   // Damage taken by level 1 ship in depth 3
   public const int LEVEL_2_IN_DEPTH_3     = 40;   // Damage taken by level 2 ship in depth 3
   public const float ITEM_DELAY           = 0.3f; // Duration of the delay between final reward panel entries
   public const float POP_DURATION         = 0.5f; // Duration of the item pop animation in final rewards panel

   public static ShipManager Instance { get; private set; }

   public PanelManager panelManager; // Reference to global UI panel controller
   [SerializeField] private ExplorationUnitManager explorationUnitManager;


   [Header("UI References")]
   [SerializeField] private Transform fuelPanel;          // UI panel displayed when ship runs out of fuel
   [SerializeField] private Transform healthPanel;        // UI panel displayed when ship hull health reaches zero
   [SerializeField] private Transform confirmReturnPanel; // UI panel asking user to confirm request to end exloration
   [SerializeField] private Transform finalRewardsPanel;  // UI panel showing total rewards at the end of an exploration

   [Header("Ship Stat Texts")]
   [SerializeField] private TextMeshProUGUI decisionFuelText;    // Text showing current fuel on decision panel
   [SerializeField] private TextMeshProUGUI decisionHealthText;  // Text showing current health on decision panel
   [SerializeField] private TextMeshProUGUI inventoryFuelText;   // Text showing current fuel on inventory panel
   [SerializeField] private TextMeshProUGUI inventoryHealthText; // Text showing current health on inventory panel
   [SerializeField] private TextMeshProUGUI exploreFuelText;     // Text showing current fuel on explore panel
   [SerializeField] private TextMeshProUGUI exploreHealthText;   // Text showing current health on explore panel

   [Header("Final Rewards Panel UI")]
   [SerializeField] private Transform  rewardsContainer; // Parent transform holding all parent icon prefabs
   [SerializeField] private GameObject noRewardsText;    // Message shown if exploration returns with no resources

   [Header("Ship Level Settings")]
   public           int ShipLevel { get; private set; } = 1;    // Current level of the ship
   private readonly int[] maxHealthByLevel = { 0, 40, 60, 90 }; // Health ceiling for each level (index 0 is unused placeholder)
   private readonly int[] maxFuelByLevel   = { 0, 6, 9, 12 };   // Fuel ceiling for each level (index 0 is unused placeholder)

   // Permanent lab upgrade bonuses
   private int labBonusHealth    = 0;   // Total health gained from permanent lab upgrades
   private int labBonusFuel      = 0;   // Total fuel gained from permanent fuel upgrades
   private int labLootMultiplier = 1;   // Multiplier applied to exedition rewards (default 1)
   public bool isTier2Unlocked = false; // Determines if second lab upgrade is unlocked and crafted items can be discovered


   [Header("Current Stats")]
   // The current stats of the ship
   int currentFuel;
   int currentHealth;
   int maxHealth; // Ship's max health at its current level
   int maxFuel;   // Ship's max fuel at its current level
   int currentPearl;
   int currentOre;
   int currentDepth = 1; // The current depth zone the ship is in

   [Header("Ship Inventory - Crafts")]
   // Current inventory of crafts
   int currentPatchKit;
   int currentHarpoon;
   int currentCrudeTool;
   int currentPressureValve;
   int currentDivingBell;
   int currentClockworkEngine;
   int currentPrecisionLens;

   // Data structure used to pass exploration state changes to UI system
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
      public int healthBefore; // Health value prior to processing (used for animation)
      public int fuelBefore;   // Fuel value prior to processing (used for animation)
      public int maxHealth;
      public int maxFuel;

      // Determines if any values were altered
      public readonly bool HasChanges()
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
      if (Instance != null && Instance != this)
      {
         Destroy(this.gameObject);
         return;
      }
      Instance = this;

      // Initialize ship's base stats and ensure ship has full health and fuel
      UpdateStatsToLevel();
      currentHealth = maxHealth;
      currentFuel   = maxFuel;
   }

   private void Start()
   { 
      UpdateShipUI();
   }

   // Updates currrent depth zone when ship reaches new depth
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
   public int GetDepth()           { return currentDepth; }
   public int GetPearl()           { return currentPearl; }
   public int GetOre()             { return currentOre; }
   public int GetPatchKit()        { return currentPatchKit; }
   public int GetHarpoon()         { return currentHarpoon; }
   public int GetCrudeTool()       { return currentCrudeTool; }
   public int GetPressureValve()   { return currentPressureValve; }
   public int GetDivingBell()      { return currentDivingBell; }
   public int GetClockworkEngine() { return currentClockworkEngine; }
   public int GetPrecisionLens()   { return currentPrecisionLens; }

   // Verifies if ship has resources required to select a decision option
   public bool CanAfford(EventChoice choice)
   {
      if (choice.oreChange   < 0 && GetOre()   < Mathf.Abs(choice.oreChange))   { return false; }
      if (choice.pearlChange < 0 && GetPearl() < Mathf.Abs(choice.pearlChange)) { return false; }
      return true;
   }

   // Increments ship level and recalculates stat ceilings
   public void UpgradeShip()
   {
      if (ShipLevel < MAX_LEVEL)
      {
         ShipLevel += 1;
         UpdateStatsToLevel();
      }
   }

   // Recalculates ship's stats based on level and lab bonuses
   public void UpdateStatsToLevel()
   {
      maxHealth     = maxHealthByLevel[ShipLevel] + labBonusHealth;
      currentHealth = maxHealth;
      maxFuel       = maxFuelByLevel[ShipLevel] + labBonusFuel;
      currentFuel   = maxFuel;

      UpdateShipUI();
   }

   // Update the ship's fuel and health in all panels
   private void UpdateShipUI()
   {
      if (exploreHealthText != null)
         exploreHealthText.text = $"{currentHealth}/{maxHealth}";
      if (exploreFuelText != null)
         exploreFuelText.text = $"{currentFuel}/{maxFuel}";
      if (decisionFuelText != null)
         decisionFuelText.text = $"{currentFuel}/{maxFuel}";
      if (decisionHealthText != null)
         decisionHealthText.text = $"{currentHealth}/{maxHealth}";
      if (inventoryFuelText != null)
         inventoryFuelText.text = $"{currentFuel}/{maxFuel}";
      if (inventoryHealthText != null)
         inventoryHealthText.text = $"{currentHealth}/{maxHealth}";
   }

   // Applies permanent Lab stat boosts to the ship
   public void ApplyLabShipBonus()
   {
      labBonusHealth += LAB_T1_HEALTH_BONUS;
      labBonusFuel   += LAB_T1_FUEL_BONUS;
      UpdateStatsToLevel();
   }

   // Activates the Lab tier 3 loot multiplier for all future rewards
   public void ApplyLabRewardBonus()
   {
      labLootMultiplier = LAB_T3_LOOT_MULTIPLIER;
   }

   // Calulates mathematical outcome of a exploration event decision
   public RoundResults ApplyEventResult(EventChoice results)
   {
      RoundResults finalResults = new();

      // Set decision results panel health and fuel before decision changes
      finalResults.healthBefore = currentHealth;
      finalResults.fuelBefore   = currentFuel;
      finalResults.maxHealth    = maxHealth;
      finalResults.maxFuel      = maxFuel;

      // Calculate base and random loot for pearls and ore
      int actualPearl = results.pearlChange + UnityEngine.Random.Range(results.minPearl, results.maxPearl + 1);
      int actualOre = 0;
      if (results.loseOre)
         actualOre = -currentOre; // Special case where all ore is lost
      else
         actualOre = results.oreChange + UnityEngine.Random.Range(results.minOre, results.maxOre + 1);

      // Apply tier 3 Lab multiplier for all positive pearl and ore loot
      if (actualPearl > 0)
         actualPearl *= labLootMultiplier;
      if (actualOre > 0)
         actualOre *= labLootMultiplier;

      // Get random craftable item if lab tier 2 events choice
      int foundPatchKits        = 0, 
          foundHarpoons         = 0, 
          foundCrudeTools       = 0, 
          foundPressureValves   = 0, 
          foundDivingBells      = 0, 
          foundClockworkEngines = 0, 
          foundPrecisionLenses  = 0;

      if (results.requiresLabTier)
      {
         int roll = UnityEngine.Random.Range(0, NUM_OF_CRAFTS);
         switch (roll)
         {
            case 0: foundPatchKits        += 1; break;
            case 1: foundHarpoons         += 1; break;
            case 2: foundCrudeTools       += 1; break;
            case 3: foundPressureValves   += 1; break;
            case 4: foundDivingBells      += 1; break;
            case 5: foundClockworkEngines += 1; break;
            case 6: foundPrecisionLenses  += 1; break;
         }
      }

      // Process random chance fuel and health modifiers
      if (results.fuelChance > 0 && UnityEngine.Random.value <= results.fuelChance)
         results.fuelChange += results.fuelGain;
      if (results.damageChance > 0 && UnityEngine.Random.value <= results.damageChance)
         results.healthChange -= results.healthDamage;

      // Apply all event changes to ship's stats and inventory
      currentPearl           += actualPearl;
      currentOre             += actualOre;
      currentHealth          += results.healthChange;
      currentFuel            += results.fuelChange;
      currentPatchKit        += foundPatchKits;
      currentHarpoon         += foundHarpoons;
      currentCrudeTool       += foundCrudeTools;
      currentPressureValve   += foundPressureValves;
      currentDivingBell      += foundDivingBells;
      currentClockworkEngine += foundClockworkEngines;
      currentPrecisionLens   += foundPrecisionLenses;

      // Populate results package for UI manager
      finalResults.pearlChanged           = actualPearl;
      finalResults.oreChanged             = actualOre;
      finalResults.patchKitChanged        = foundPatchKits;
      finalResults.harpoonChanged         = foundHarpoons;
      finalResults.crudeToolChanged       = foundCrudeTools;
      finalResults.pressureValveChanged   = foundPressureValves;
      finalResults.divingBellChanged      = foundDivingBells;
      finalResults.clockworkEngineChanged = foundClockworkEngines;
      finalResults.precisionLensChanged   = foundPrecisionLenses;
      finalResults.healthChanged          = results.healthChange;
      finalResults.fuelChanged            = results.fuelChange;

      // Check for ship fail states and update the UI
      if (currentFuel <= 0)
         LowFuel();
      if (currentHealth <= 0)
         ShipDestruction();
      UpdateShipUI();

      return finalResults;
   }

   // Calculates damage ship will take if beyond ship's depth level
   public int GetDamage(int depthCheck)
   {
      int damage = 0; // Ship damage as a result of high depth
      if (ShipLevel == 1 && depthCheck == 2)
         damage = LEVEL_1_IN_DEPTH_2;
      else if (ShipLevel == 1 && depthCheck == 3)
         damage = LEVEL_1_IN_DEPTH_3;
      else if (ShipLevel == 2 && depthCheck == 3)
         damage = LEVEL_2_IN_DEPTH_3;
      return damage;
   }

   // Handles a new turn in the ship
   public void NewTurn()
   {
      // Burn one fuel and check for empty
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
      // Replenish ship stats
      currentFuel   = maxFuel;
      currentHealth = maxHealth;
      currentDepth  = 1;
      // Wipe away the ship's cargo
      currentPearl           = 0;
      currentOre             = 0;
      currentPatchKit        = 0;
      currentHarpoon         = 0;
      currentCrudeTool       = 0;
      currentPressureValve   = 0;
      currentDivingBell      = 0;
      currentClockworkEngine = 0;
      currentPrecisionLens   = 0;

      // Reset ship's position on the map
      MapManager.Instance.MoveToNode(MapManager.Instance.startingNode);

      UpdateShipUI();
      OnShipDeath?.Invoke(); // Broadcast ship reset to other systems
   }

   // Triggers the fail-state UI sequence when fuel is empty
   public void LowFuel()
   {
      if (currentFuel <= 0)
      {
         OpenFuelPanel();
         // Setup "OK" button to initiate expedition conclusion sequence
         Button confirmFuelButton = fuelPanel.Find("OkButton").GetComponent<Button>();
         confirmFuelButton.onClick.RemoveAllListeners();
         confirmFuelButton.onClick.AddListener(() =>
         {
            ClosePanels();
            StartCoroutine(FinishExploration());
         });
      }
   }

   // Triggers the fail-state UI sequence when health is empty
   public void ShipDestruction()
   {
      OpenHealthPanel();
      // Setup "OK" button to initiate full exploration reset
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
      panelManager.OpenPanel(fuelPanel.gameObject);
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

      // Setup "return" button to voluntarily end exploration and secure loot
      Button returnShip = confirmReturnPanel.Find("Return").GetComponent<Button>();
      returnShip.onClick.RemoveAllListeners();
      returnShip.onClick.AddListener(() => {
         StartCoroutine(FinishExploration());
         ClosePanels();
         explorationUnitManager.SetDecisionInteractable(true);
      });
      // Setup "keep going" button to continue exploration
      Button stayOut = confirmReturnPanel.Find("KeepGoing").GetComponent<Button>();
      stayOut.onClick.RemoveAllListeners();
      stayOut.onClick.AddListener(() =>
      {
         ClosePanels();
         explorationUnitManager.SetDecisionInteractable(true);
      });
   }

   // Hides health, fuel, and return panels and restores interaction
   private void ClosePanels()
   {
      healthPanel.gameObject.SetActive(false);
      fuelPanel.gameObject.SetActive(false);
      confirmReturnPanel.gameObject.SetActive(false);
      explorationUnitManager.SetDecisionInteractable(false);
   }

   // Ends a successful exploration, shows total rewards and trasfers inventory to main game inventory
   public IEnumerator FinishExploration()
   {
      Button confirmRewards = finalRewardsPanel.Find("Confirm").GetComponent<Button>();
      confirmRewards.interactable = false;
      explorationUnitManager.isExploring = false;
      ClosePanels();
      explorationUnitManager.CloseDecisionPanel();
      // Prepare rewards container by hiding all previously visible icons
      if (rewardsContainer != null)
      {
         foreach (Transform child in rewardsContainer)
         {
            child.gameObject.SetActive(false);
            child.localScale = Vector3.zero;
         }
      }
      // Run animation to show collected loot
      yield return StartCoroutine(ShowRewardsSequence());
      explorationUnitManager.SetDecisionInteractable(true);
      confirmRewards.interactable = true;

      // Set up confirm button to finalize exploration
      confirmRewards.onClick.RemoveAllListeners();
      confirmRewards.onClick.AddListener(() =>
      {
         CloseFinalRewardsPanel();
         AddRewards(); // Add loot to main inventory
         ResetShip();  // Prepare ship for new exploration
      });
   }

   // Controls pop-in animation for each reward type collected
   private IEnumerator ShowRewardsSequence()
   {
      // Sum of all collected items to check for empty exploration
      int totalRewardCount = currentPearl + currentOre + currentPatchKit + currentHarpoon +
                             currentCrudeTool + currentPressureValve + currentDivingBell +
                             currentClockworkEngine + currentPrecisionLens;

      // Wait for previous panels to fully fade out
      yield return new WaitForSeconds(0.5f);
      panelManager.OpenPanel(finalRewardsPanel.gameObject);

      // If no rewards were found, show empty exploraion message
      if (totalRewardCount <= 0 && noRewardsText != null)
      {
         noRewardsText.SetActive(true);
         yield break;
      }

      yield return new WaitForSeconds(ITEM_DELAY); // Brief pause before rewards start popping in

      // Define data map for iterative reward processing
      var rewards = new (string Name, int Amount)[]
      {
        ("Pearl", currentPearl),
        ("Ore", currentOre),
        ("Patch Kit", currentPatchKit),
        ("Harpoon", currentHarpoon),
        ("Crude Tool", currentCrudeTool),
        ("Pressure Valve", currentPressureValve),
        ("Diving Bell", currentDivingBell),
        ("Clockwork Engine", currentClockworkEngine),
        ("Precision Lens", currentPrecisionLens)
      };

      // Search for each reward type, and if present then animate it into the panel
      foreach (var reward in rewards)
      {
         Transform itemRow = TrySpawnRewardRow(reward.Name, reward.Amount);
         if (itemRow != null && itemRow.gameObject.activeSelf)
         {
            StartCoroutine(AnimatePop(itemRow));
            yield return new WaitForSeconds(ITEM_DELAY);
         }
      }
   }

   // Searches reward container for a reward by name and updates its quantity
   private Transform TrySpawnRewardRow(string itemName, int amount)
   {
      Transform slotTransform = rewardsContainer.Find(itemName);
      if (slotTransform == null) 
         return null;

      // If given item is in inventory, activate item's object in panel and set the quantity found
      if (amount > 0)
      {
         slotTransform.gameObject.SetActive(true);
         slotTransform.localScale = Vector3.zero; // Preparing for pop animation
         TextMeshProUGUI txt = slotTransform.Find("Count").GetComponent<TextMeshProUGUI>();
         txt.text = $"x{amount}";
         return slotTransform;
      }
      else
      {
         slotTransform.gameObject.SetActive(false);
         return null;
      }
   }

   // Elastic pop-in scale animation for UI elements
   private IEnumerator AnimatePop(Transform target)
   {
      float elapsed  = 0.0f; // Keeps track of time passed in animation
      Vector3 startScale = Vector3.zero; // The starting scale of the reward entry
      Vector3 endScale   = Vector3.one;  // The final scale of the reward entry

      while (elapsed < POP_DURATION)
      {
         elapsed += Time.deltaTime;
         float percent = elapsed / POP_DURATION;
         float curve = Mathf.Sin(percent * Mathf.PI * 1.2f) / 1.2f;
         target.localScale = Vector3.LerpUnclamped(startScale, endScale, percent + (1f - percent) * curve);

         yield return null;
      }
      target.localScale = endScale;
   }

   // Permanently transfers ship inventory to main game inventory
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

   // Closes the final rewards panel
   public void CloseFinalRewardsPanel()
   {
      noRewardsText.SetActive(false);
      panelManager.ClosePanel(finalRewardsPanel.gameObject);
      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(true);
   }
}