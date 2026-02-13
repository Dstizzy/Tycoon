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
   private int[] maxHealthByLevel = { 0, 60, 80, 100 };
   private int[] maxFuelByLevel = { 0, 7, 10, 15 };

   [Header("Current Stats")]
   int currentFuel;
   int currentHealth;
   int maxHealth;
   int maxFuel;
   int currentPearl;
   int currentOre;
   int currentCrystal;
   int currentHarpoon;
   int currentDepth = 1;

   public struct RoundResults
   {
      public int pearlChanged;
      public int oreChanged;
      public int crystalChanged;
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
   public int GetCrystal() {  return currentCrystal; }
   public int GetHarpoon() {  return currentHarpoon; }


   public void UpgradeShip()
   {
      if (shipLevel < 3)
      {
         shipLevel += 1;
         UpdateStatsToLevel();
      }
   }

   public void UpdateStatsToLevel()
   {
      maxHealth = maxHealthByLevel[shipLevel];
      currentHealth = maxHealth;
      maxFuel = maxFuelByLevel[shipLevel];
      currentFuel = maxFuel;

      UpdateShipUI();
   }

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

   public RoundResults ApplyEventResult(EventChoice results)
   {
      RoundResults finalResults = new RoundResults();

      int actualPearl = results.pearlChange + UnityEngine.Random.Range(results.minPearl, results.maxPearl + 1);
      int actualOre = results.oreChange + UnityEngine.Random.Range(results.minOre, results.maxOre + 1);
      int actualCrystal = results.crystalChange;

      currentPearl += actualPearl;
      currentOre += actualOre;
      currentCrystal += actualCrystal;
      currentHealth += results.healthChange;
      currentFuel += results.fuelChange;
      currentHarpoon += results.harpoonChange;

      finalResults.pearlChanged = actualPearl;
      finalResults.oreChanged = actualOre;
      finalResults.crystalChanged = actualCrystal;
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

   public void NewTurn()
   {
      currentFuel -= 1;
      if (currentFuel <= 0)
         LowFuel();

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
      currentCrystal = 0;
      currentHarpoon = 0;
      MapManager.Instance.MoveToNode(MapManager.Instance.startingNode);

      UpdateShipUI();
      OnShipDeath?.Invoke();
   }

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

   private void OpenFuelPanel()
   {
      fuelPanel.gameObject.SetActive(true);
   }

   private void OpenHealthPanel()
   {
      healthPanel.gameObject.SetActive(true);
   }

   public void OpenConfirmReturnPanel()
   {
      confirmReturnPanel.gameObject.SetActive(true);
      Button returnShip = confirmReturnPanel.Find("Return").GetComponent<Button>();
      returnShip.onClick.RemoveAllListeners();
      returnShip.onClick.AddListener(() => {
         FinishExploration();
         ClosePanels();
      });
      Button stayOut = confirmReturnPanel.Find("KeepGoing").GetComponent<Button>();
      stayOut.onClick.RemoveAllListeners();
      stayOut.onClick.AddListener(() => ClosePanels());
   }

   private void ClosePanels()
   {
      healthPanel.gameObject.SetActive(false);
      fuelPanel.gameObject.SetActive(false);
      confirmReturnPanel.gameObject.SetActive(false);
   }

   public void FinishExploration()
   {
      ClosePanels();
      explorationUnitManager.CloseDecisionPanel();

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
      if (currentCrystal > 0)
         totalRewards += $"Crystal: {currentCrystal}\n";
      if (currentHarpoon > 0)
         totalRewards += $"Harpoons: {currentHarpoon}\n";
      finalRewards.text = totalRewards;

      ResetShip();
   }

   private void AddRewards()
   {
      if(InventoryManager.Instance != null)
      {
         if (currentPearl > 0)
            InventoryManager.Instance.TryAddPearl(currentPearl);
         if (currentOre > 0)
            InventoryManager.Instance.TryAddOre(currentOre);
         if (currentCrystal > 0)
            InventoryManager.Instance.TryAddCrystal(currentCrystal);
         if (currentHarpoon > 0)
            InventoryManager.Instance.TryAddHarpoon(currentHarpoon);
      }
   }
}