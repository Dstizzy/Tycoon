using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using JetBrains.Annotations;

public class ShipManager : MonoBehaviour
{
   [Header("UI References")]
   [SerializeField] private ExplorationUnitManager explorationUnitManager;
   [SerializeField] private Transform fuelPanel;
   [SerializeField] private Transform healthPanel;
   [SerializeField] private Transform confirmReturnPanel;
   [SerializeField] private Transform finalRewardsPanel;
   [SerializeField] private TextMeshProUGUI finalRewards;
   public TextMeshProUGUI decisionFuelText;
   public TextMeshProUGUI decisionHealthText;
   public TextMeshProUGUI exploreFuelText;
   public TextMeshProUGUI exploreHealthText;

   [Header("Ship Level Settings")]
   public int shipLevel = 1;
   private int[] maxHealthByLevel = { 0, 40, 60, 90 };
   private int[] maxFuelByLevel = { 0, 6, 9, 12 };

   // Lab bonuses
   public int labBonusHealth = 0;
   public int labBonusFuel = 0;

   [Header("Current Stats")]
   int currentFuel;
   int currentHealth;
   int maxHealth;
   int maxFuel;
   int currentPearl;
   int currentOre;
   int currentHarpoon;
   int currentDepth = 1;

   public struct RoundResults
   {
      public int pearlChanged;
      public int oreChanged;
      public int healthChanged;
      public int fuelChanged;
   }

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

   public int GetDepth() { return currentDepth; }
   public int GetPearl() { return currentPearl; }
   public int GetOre() { return currentOre; }
   public int GetHarpoon() {  return currentHarpoon; }

   public bool CanAfford(EventChoice choice)
   {
      if(choice.oreChange < 0 && GetOre() < Mathf.Abs(choice.oreChange)) { return false; }
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

   // Update the ship's stats to the ship's current level
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
         decisionFuelText.text = $"fuel: {currentFuel}/{maxFuel}";
      if (decisionHealthText != null)
         decisionHealthText.text = $"health: {currentHealth}/{maxHealth}";
      if (exploreFuelText != null)
         exploreFuelText.text = $"fuel: {currentFuel}/{maxFuel}";
      if (exploreHealthText != null)
         exploreHealthText.text = $"health: {currentHealth}/{maxHealth}";
   }

   public void ApplyLabShipBonus(int healthBonus, int fuelBonus)
   {
      labBonusHealth += healthBonus;
      labBonusFuel += fuelBonus;
      UpdateStatsToLevel();
   }

   public RoundResults ApplyEventResult(EventChoice results)
   {
      RoundResults finalResults = new RoundResults();

      int actualPearl = results.pearlChange + UnityEngine.Random.Range(results.minPearl, results.maxPearl + 1);
      int actualOre = 0;
      if (results.loseOre)
         actualOre = -currentOre;
      else
         actualOre = results.oreChange + UnityEngine.Random.Range(results.minOre, results.maxOre + 1);

      currentPearl += actualPearl;
      currentOre += actualOre;
      currentHealth += results.healthChange;
      currentFuel += results.fuelChange;
      currentHarpoon += results.harpoonChange;

      finalResults.pearlChanged = actualPearl;
      finalResults.oreChanged = actualOre;
      finalResults.healthChanged = results.healthChange;
      finalResults.fuelChanged = results.fuelChange;

      if(currentFuel <= 0)
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
      // burn one fuel and check if empty
      currentFuel -= 1;
      if (currentFuel <= 0)
         LowFuel();

      // take out potential hull damage and check for destruction
      int hullDamage = GetDamage(currentDepth);
      if (hullDamage > 0)
         currentHealth -= hullDamage;
      if (currentHealth <= 0)
         ShipDestruction();

      UpdateShipUI();
   }

   // Reset ship health, fuel, depth, inventory, and map location
   public void ResetShip()
   {
      currentFuel = maxFuel;
      currentHealth = maxHealth;
      currentDepth = 1;
      currentPearl = 0;
      currentOre = 0;
      currentHarpoon = 0;
      MapManager.Instance.MoveToNode(MapManager.Instance.startingNode);

      UpdateShipUI();
      OnShipDeath?.Invoke();
   }

   // Tells that fuel is too low to continue
   public void LowFuel()
   {
      if(currentFuel <= 0)
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

   // Tells that ship has lost all its health
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

   // Ends a successful exploration and shows total rewards
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

      string totalRewards = "";

      if (currentPearl > 0)
         totalRewards += $"Pearl: {currentPearl}\n";
      if (currentOre > 0)
         totalRewards += $"Ore: {currentOre}\n";
      if (currentHarpoon > 0)
         totalRewards += $"Harpoons: {currentHarpoon}\n";
      finalRewards.text = totalRewards;

      ResetShip();
   }

   // Moves rewards from ship inventory to main game inventory
   private void AddRewards()
   {
      if(InventoryManager.Instance != null)
      {
         if (currentPearl > 0)
            InventoryManager.Instance.TryAddPearl(currentPearl);
         if (currentOre > 0)
            InventoryManager.Instance.TryAddOre(currentOre);
         if (currentHarpoon > 0)
            InventoryManager.Instance.TryAddHarpoon(currentHarpoon);
      }
   }
}