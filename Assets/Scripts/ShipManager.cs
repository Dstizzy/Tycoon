using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ShipManager : MonoBehaviour
{
   [Header("UI References")]
   [SerializeField] private ExplorationUnitManager explorationUnitManager;
   [SerializeField] private Transform fuelPanel;
   [SerializeField] private Transform healthPanel;
   [SerializeField] private Transform confirmReturnPanel;
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
   int currentGold;
   int currentOre;
   int currentHarpoon;
   int currentArtifact;
   int currentDepth = 1;

   public struct RoundResults
   {
      public int goldChanged;
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

   public int GetDepth()
   {
      return currentDepth;
   }

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

      int actualGold = results.goldChange + UnityEngine.Random.Range(results.minGold, results.maxGold + 1);
      int actualOre = results.oreChange + UnityEngine.Random.Range(results.minOre, results.maxOre + 1);

      currentGold += actualGold;
      currentOre += actualOre;
      currentHealth += results.healthChange;
      currentFuel += results.fuelChange;
      currentHarpoon += results.harpoonChange;
      currentArtifact += results.artifactChange;

      finalResults.goldChanged = actualGold;
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

   public void NewTurn()
   {
      currentFuel -= 1;
      if (currentFuel <= 0)
         LowFuel();

      if (shipLevel == 1 && currentDepth == 2)
         currentHealth -= 30;
      if (shipLevel == 1 && currentDepth == 3)
         currentHealth -= 60;
      if (shipLevel == 2 && currentDepth == 3)
         currentHealth -= 40;

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
      currentGold = 0;
      currentOre = 0;
      currentHarpoon = 0;
      currentArtifact = 0;
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
            CloseFuelPanel();
            explorationUnitManager.CloseDecisionPanel();
            ResetShip();
         });
      }
      if (currentFuel > 0 && currentFuel < currentDepth)
      {
         // message fuel insufficient for return
      }
   }

   public void ShipDestruction()
   {
      OpenHealthPanel();
      Button confirmHealthButton = healthPanel.Find("OkButton").GetComponent<Button>();
      confirmHealthButton.onClick.RemoveAllListeners();
      confirmHealthButton.onClick.AddListener(() =>
      {
         CloseHealthPanel();
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
         CloseConfirmReturnPanel();
      });
      Button stayOut = confirmReturnPanel.Find("KeepGoing").GetComponent<Button>();
      stayOut.onClick.RemoveAllListeners();
      stayOut.onClick.AddListener(() => CloseConfirmReturnPanel());
   }

   private void CloseFuelPanel()
   {
      fuelPanel.gameObject.SetActive(false);
   }

   private void CloseHealthPanel()
   {
      healthPanel.gameObject.SetActive(false);
   }

   private void CloseConfirmReturnPanel()
   {
      confirmReturnPanel.gameObject.SetActive(false);
   }

   public void FinishExploration()
   {
      //ADD TO INVENTORY
      explorationUnitManager.CloseDecisionPanel();
      ResetShip();
   }
}