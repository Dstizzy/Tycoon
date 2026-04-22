//using Codice.Client.Common.GameUI;
using JetBrains.Annotations;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using static TickerSystem;

public class TurnManager : MonoBehaviour
{

   // Constants
   const int MANUALRESETWAIT = 2;
   const int STARTINGTURN = 1;
   const int ENDINGTURN = 80;

   // Variables
   public static System.Random random = new System.Random(); // Random number generator
   public static int randomNumber;                           // Random number for various calculations
   public static int jamTurnCounter;                         // Counter for turns during a jam
   public static bool manualResetOption = false;             // Flag for manual reset option
   public static int heatLevel;                              // Current heat level
   public static int enemyAttackPercentage = 0;              // Percentage chance of enemy attack each turn
   public static bool userDefends;                           // Flag indicating if the user defends against enemy attacks 
   private TradeHutManager tradeHutManager;                  // Trade hut manager instance 
   [SerializeField] private OreRefinery_Manager oreManager;  // Ore refinery manager instance
   [SerializeField] private CameraShake cameraShake;         // Camera shake script
   [SerializeField] private UIFlash uiFlash;                 // Red flash script
   [SerializeField] private UIFade uiFade;                   // Panel fade script
   public TickerSystem newsTicker;                           // The wolrd event news ticker panel
   public static int jammingChance = 5;                      // Chance of ore refinery jamming
   public static bool isJamPrevented = false;                // Flag to check if the ore refinery is being prevented from jamming
   public static int MaintenanceCounter = 5;                 // Counter for the number of times preventative maintenance has been performed
   public static int previousJammingChance;                  // Keeps track of the jamming chance before preventative maintenance

   // Public Unity fields
   [Header("Turn Setting")]
   public int currentTurn = STARTINGTURN;   // The current turn number, starting from 1.
   public int maxTurns = ENDINGTURN;     // The maximum number of turns before the game ends.
   public TextMeshProUGUI walkthroughTurnText;
   public TextMeshProUGUI turnText;                        // The UI text element to display the current turn.
   public int eventCountdown = 1;              // Turn countdown until next world event

   [Header("UI/Game Status")]
   public Button endTurnButton;                     // The button to disable when the game ends.
   private bool _isGameActive   = true;             // Tracks if the game is currently in progress.
   [SerializeField] private GameObject progressBar; // Turn changing progress bar UI element.
   private bool walkthroughGame = false;            // Flag to indicate if the game is in walkthrough mode
   private bool normalGame      = true;             // Flag to indicate if the game is in normal mode
   private int tempTurn;                            // Temporary variable to store the turn number when switching between walkthrough and normal modes

   [Header("Enemy Settings")]
   [SerializeField] private GameObject enemyPanel;      // The enemy panel UI element.
   [SerializeField] private GameObject heatProgressBar; // The heat level progress bar UI element.
   private bool hasShownFirstEnemyAttackTutorial;  // Flag to track if the first enemy attack tutorial has been shown
   private bool isAdvancingTurn;  // Flag to prevent multiple turn advancements at the same time

   // public propertries
   public static TurnManager Instance { get; set; } // Singleton instance


   // Events
   public static event Action OnTurnEnded; // Broadcasts when a turn ends



   // Enforces the Singleton pattern to ensure only one 
   // instance of TurnManager exists. 
   void Awake()
   {
      if (Instance != null && Instance != this)
      {
         Destroy(gameObject);
      }
      else
      {
         Instance = this;
         // (Optional) Uncomment this to make the manager persist across scenes
         // DontDestroyOnLoad(gameObject); 
      }

      if (newsTicker == null)
         Debug.Log("Ticker is not assigned in the Inspector");

   }

   // Initializes the UI elements with the starting values when the game begins.                  
   void Start()
   {
      tradeHutManager = TradeHutManager.Instance;
      UpdateTurnUI();
      tradeHutManager.WorldEventChance();
   }

   public async void WalkthroughEndTurn()
   {
      if (isAdvancingTurn)
         return;
      if (!_isGameActive)
         return;
      isAdvancingTurn = true;

      if (PopUpManager.Instance != null)
         PopUpManager.Instance.ForceResetInputBlock();

      if (endTurnButton != null)
         endTurnButton.interactable = false;

      progressBar.SetActive(true);
      PopUpManager.Instance.DisablePlayerInput();
      await Task.Delay(1000);

      progressBar.SetActive(false);
      PopUpManager.Instance.EnablePlayerInput();
      progressBar.transform.rotation = Quaternion.identity;
      Debug.Log("### TurnManager Start() ###");

      currentTurn++;

      if (walkthroughTurnText != null)
      {
         walkthroughTurnText.text = currentTurn.ToString() + " / " + maxTurns.ToString();
      }

      InventoryManager.Instance.TryAddOre(10);
      ForgeManager.Instance.ProcessCraftingQueue();

      isAdvancingTurn = false;

      if (endTurnButton != null)
         endTurnButton.interactable = true;
   }


   public void SetSavedTurn()
   {
      tempTurn = currentTurn;
   }

   public void ResetToSavedTurn()
   {
      currentTurn = tempTurn;
      UpdateTurnUI();
   }
   
   public async void EndTurn()
   {
      // 1. Prevent overlapping turn advancements or running after game over
      if (isAdvancingTurn || !_isGameActive)
         return;
         
      isAdvancingTurn = true;

      // 2. Lock UI and Input before the delay
      if (PopUpManager.Instance != null)
         PopUpManager.Instance.ForceResetInputBlock();

      if (endTurnButton != null)
         endTurnButton.interactable = false;

      if (progressBar != null)
         progressBar.SetActive(true);
         
      if (PopUpManager.Instance != null)
         PopUpManager.Instance.DisablePlayerInput();
         
      await Task.Delay(1000);
      
      // =========================================================
      // 3. BULLETPROOF LOGIC BLOCK
      // =========================================================
      try 
      {
         if (progressBar != null)
         {
            progressBar.SetActive(false);
            progressBar.transform.rotation = Quaternion.identity;
         }
            
         if (PopUpManager.Instance != null)
            PopUpManager.Instance.EnablePlayerInput();
            
         Debug.Log("### TurnManager Start() ###");

         currentTurn++;
         eventCountdown++;

         // Check if the game should end                 
         if (currentTurn > maxTurns)
         {
            EndGame();
            return; // Safely exits to the 'finally' block
         }

         // --- Standard Turn Logic ---
         UpdateTurnUI();
         
         if (currentTurn > 5)
            HandleJamming();

         HandleEnemy();

         if(isJamPrevented)
            HandlePreventativeMaintenance();

         // --- World Event Reset & Initialization ---
         if (eventCountdown == 1) 
         {
            tradeHutManager.ResetWorldEventShifts();
            tradeHutManager.WorldEventChance();

            // Safely find and activate the Insurance Policy
            Transform insuranceUI = TradeHutManager.Instance?.BuyItems?.Find(item => item.CompareTag(TradeHutManager.INSURANCE_POLICY_TAG));
            if (insuranceUI != null)
               insuranceUI.gameObject.SetActive(true);

            TradeHutManager.Instance.DisplayWorldEventVisual(false, false);
         }

         // Apply the market shift
         tradeHutManager.MarketFluctuate();

         // --- World Event News Ticker & Triggers ---
         if (eventCountdown >= 3 && eventCountdown <= 5) 
         {
            // Safely show the ticker
            if (newsTicker != null)
            {
               newsTicker.gameObject.SetActive(true);
               tradeHutManager.WorldEventNewsTickerText();
               newsTicker.ShowTicker(tradeHutManager.currentNewsTickerMessage, Color.black, MessageTypes.WorldEvent);
            }
            else
            {
               Debug.LogWarning("News Ticker is missing! Skipping Ticker UI update.");
            }

            // Safely hide the Insurance Policy
            Transform insuranceUI = TradeHutManager.Instance?.BuyItems?.Find(item => item.CompareTag(TradeHutManager.INSURANCE_POLICY_TAG));
            if (insuranceUI != null)
               insuranceUI.gameObject.SetActive(false);

            // Execute the climax of the event on Turn 5
            if(eventCountdown == 5) 
            {
               TradeHutManager.Instance.InsurancePolicyCheck();
               TradeHutManager.Instance.DisplayWorldEventVisual(true, true);
            }
         }

         // Predict the next turn
         tradeHutManager.CraftMarketForesight();

         // Reset countdown if we just finished the event turn
         if (eventCountdown == 5)
            eventCountdown = 0;

         if(LabManager.currentCommerceTier == LabManager.TIER_TWO) 
         { 
            TradeHutManager.Instance.recycleCounter = 0;
            TradeHutManager.Instance.RecycleButton.gameObject.SetActive(true);
         }
         OnTurnEnded?.Invoke();
      }
      catch (Exception e)
      {
         // If a random null error happens, log it so you can fix it, but DO NOT freeze the game!
         Debug.LogError($"A fatal error occurred during EndTurn: {e.Message}\n{e.StackTrace}");
      }
      finally 
      {
         // =========================================================
         // 4. GUARANTEED CLEANUP 
         // (Runs even if an error crashes the try block above)
         // =========================================================
         isAdvancingTurn = false;

         if (endTurnButton != null)
            endTurnButton.interactable = true;
            
         if (progressBar != null)
            progressBar.SetActive(false);
            
         if (PopUpManager.Instance != null)
            PopUpManager.Instance.EnablePlayerInput();
      }
   }
   void UpdateTurnUI()
   {
      if (turnText != null)
      {
         turnText.text = currentTurn.ToString() + " / " + maxTurns.ToString();
      }
   }

   // Ends the game when the maximum number of turns is reached.
   void EndGame()
   {
      if (GameEndingState.HasEndingTriggered)
         return;

      _isGameActive = false;
      Debug.Log("Game over! Reached max turn(" + maxTurns + ").");

      if (turnText != null)
      {
         turnText.text = "Game over!";
      }

      if (endTurnButton != null)
      {
         endTurnButton.interactable = false;
      }

      GameEndingState.LoadFailureEnding();
   }

   // Handles the jamming logic for the Ore Refinery at the start of each turn
   public void HandleJamming()
   {
      if (oreManager.IsBlocked == false)
      {
         randomNumber = UnityEngine.Random.Range(1, 100);
         Debug.Log($"{randomNumber} < {jammingChance}");
         if (randomNumber < jammingChance)
         {
            Debug.Log("Jamming refinery");
            JamRefinery();
         }
      }
      else
      {
         if (oreManager.manualResetOption == true)
         {
            if (jamTurnCounter > 1)
               jamTurnCounter--;
            else
            {
               oreManager.IsBlocked = false;
               PopUpManager.IsOreRefineryBlocked = false;
               oreManager.DeactivateJamSymbol();
               oreManager.manualResetOption = false;
            }
         }
      }
   }

   // Jams the Ore Refinery and sets the jam turn counter
   public void JamRefinery()
   {
      oreManager.IsBlocked = true;
      PopUpManager.IsOreRefineryBlocked = true;
      oreManager.ActivateJamSymbol();
      oreManager.ActivateJamButton();
      jamTurnCounter = MANUALRESETWAIT;

      oreManager.TryShowFirstJamTutorial();
   }

   // Handles enemy attack logic based on the current heat level
   public void HandleEnemy()
   {
      GameObject decisionEnemyPanel;
      int harpoonAmount;

      if (currentTurn <= 20)
      {
         harpoonAmount = 1;
         enemyPanel.transform.Find("OptionalEnemyPanel/Text").GetComponent<TextMeshProUGUI>().text = "Would you like to defend with 1      ?;";
      }
      else if (currentTurn > 20 && currentTurn <= 40)
      {
         harpoonAmount = 2;
         enemyPanel.transform.Find("OptionalEnemyPanel/Text").GetComponent<TextMeshProUGUI>().text = "Would you like to defend with 2      ?;";
      }
      else if (currentTurn > 40 && currentTurn <= 60)
      {
         harpoonAmount = 3;
         enemyPanel.transform.Find("OptionalEnemyPanel/Text").GetComponent<TextMeshProUGUI>().text = "Would you like to defend with 3      ?;";
      }
      else
      {
         harpoonAmount = 4;
         enemyPanel.transform.Find("OptionalEnemyPanel/Text").GetComponent<TextMeshProUGUI>().text = "Would you like to defend with 4      ?;";
      }

      HandleHeat();
      HandleProgressBar();
      if (heatLevel <= 40)
      {
         enemyAttackPercentage = 0;
      }
      else if (heatLevel >= 41 && heatLevel < 70)
      {
         enemyAttackPercentage = 5;
      }
      else if (heatLevel >= 71 && heatLevel < 99)
      {
         enemyAttackPercentage = 15;
      }
      else
      {
         enemyAttackPercentage = 70;
      }

      randomNumber = random.Next(1, 100);
      if (randomNumber < enemyAttackPercentage)
      {
         PopUpManager.Instance.DisablePlayerInput();

         StartCoroutine(cameraShake.Shake(0.5f, 0.2f));

         uiFade.Appear(1.0f);

         if (TryShowFirstEnemyAttackTutorial(harpoonAmount))
            return;

         ShowEnemyEncounterPanel(harpoonAmount);

         StartCoroutine(cameraShake.Shake(0.5f, 0.2f));
         StartCoroutine(uiFlash.FlashRed(0.4f, 0.4f));
      }
   }

   private void ShowEnemyEncounterPanel(int harpoonAmount)
   {
      GameObject decisionEnemyPanel;

      enemyPanel.SetActive(true);

      if (InventoryManager.Instance.harpoonCount >= harpoonAmount)
      {
         decisionEnemyPanel = enemyPanel.transform.Find("OptionalEnemyPanel").gameObject;
         decisionEnemyPanel.SetActive(true);

         Button yesBtn = decisionEnemyPanel.transform.Find("Buttons/OptionOneButton").GetComponent<Button>();
         yesBtn.onClick.RemoveAllListeners();
         yesBtn.onClick.AddListener(() =>
         {
            InventoryManager.Instance.TryUseHarpoon(harpoonAmount);

            heatLevel = 0;
            DeactivateHeatNodes();

            decisionEnemyPanel.SetActive(false);
            enemyPanel.SetActive(false);
            PopUpManager.Instance.EnablePlayerInput();
         });

         Button noBtn = decisionEnemyPanel.transform.Find("Buttons/OptionTwoButton").GetComponent<Button>();
         noBtn.onClick.RemoveAllListeners();
         noBtn.onClick.AddListener(() =>
         {
            decisionEnemyPanel.SetActive(false);
            enemyPanel.SetActive(false);
            PopUpManager.Instance.EnablePlayerInput();
            ApplyKrakenPenalty();
         });
      }
      else
      {
         decisionEnemyPanel = enemyPanel.transform.Find("ForcedEnemyPanel").gameObject;
         decisionEnemyPanel.SetActive(true);

         Button forcedBtn = decisionEnemyPanel.transform.Find("Button").GetComponent<Button>();
         forcedBtn.onClick.RemoveAllListeners();
         forcedBtn.onClick.AddListener(() =>
         {
            decisionEnemyPanel.SetActive(false);
            enemyPanel.SetActive(false);
            PopUpManager.Instance.EnablePlayerInput();
            ApplyKrakenPenalty();
         });
      }
   }

   private void ApplyKrakenPenalty()
   {
      InventoryManager.Instance.TrySpendOre((int)(InventoryManager.Instance.oreCount / 2));
      InventoryManager.Instance.TrySpendPearl((int)(InventoryManager.Instance.pearlCount / 4));
      heatLevel = 10;
      DeactivateHeatNodes();
      heatProgressBar.transform.Find("Node1").gameObject.SetActive(true);
   }

   // Increases the heat level each turn
   public void HandleHeat()
   {
      heatLevel += 5;
      if (oreManager.IsBlocked || manualResetOption)
         heatLevel += 10;

      // Active Forge +2 per forge

      // Exploration Unit + 3

      // Building Count + 2

      // Trade Hut Sale + 5

      // Discovery in Exploration?

   }

   // Updates the heat level progress bar UI element
   public void HandleProgressBar()
   {
      if (heatProgressBar != null)
      {
         if (heatLevel >= 8)
         {
            heatProgressBar.transform.Find("Node1").gameObject.SetActive(true);
         }
         if (heatLevel >= 16)
         {
            heatProgressBar.transform.Find("Node2").gameObject.SetActive(true);
         }
         if (heatLevel >= 24)
         {
            heatProgressBar.transform.Find("Node3").gameObject.SetActive(true);
         }
         if (heatLevel >= 32)
         {
            heatProgressBar.transform.Find("Node4").gameObject.SetActive(true);
         }
         if (heatLevel >= 40)
         {
            heatProgressBar.transform.Find("Node5").gameObject.SetActive(true);
         }
         if (heatLevel >= 48)
         {
            heatProgressBar.transform.Find("Node6").gameObject.SetActive(true);
         }
         if (heatLevel >= 56)
         {
            heatProgressBar.transform.Find("Node7").gameObject.SetActive(true);
         }
         if (heatLevel >= 64)
         {
            heatProgressBar.transform.Find("Node8").gameObject.SetActive(true);
         }
         if (heatLevel >= 72)
         {
            heatProgressBar.transform.Find("Node9").gameObject.SetActive(true);
         }
         if (heatLevel >= 80)
         {
            heatProgressBar.transform.Find("Node10").gameObject.SetActive(true);
         }
         if (heatLevel >= 88)
         {
            heatProgressBar.transform.Find("Node11").gameObject.SetActive(true);
         }
         if (heatLevel >= 96)
         {
            heatProgressBar.transform.Find("Node12").gameObject.SetActive(true);
         }
         if (heatLevel >= 100)
         {
            heatProgressBar.transform.Find("Node13").gameObject.SetActive(true);
         }
      }
   }

   // Deactivates all heat nodes in the progress bar
   public void DeactivateHeatNodes()
   {
      heatProgressBar.transform.Find("Node1").gameObject.SetActive(false);
      heatProgressBar.transform.Find("Node2").gameObject.SetActive(false);
      heatProgressBar.transform.Find("Node3").gameObject.SetActive(false);
      heatProgressBar.transform.Find("Node4").gameObject.SetActive(false);
      heatProgressBar.transform.Find("Node5").gameObject.SetActive(false);
      heatProgressBar.transform.Find("Node6").gameObject.SetActive(false);
      heatProgressBar.transform.Find("Node7").gameObject.SetActive(false);
      heatProgressBar.transform.Find("Node8").gameObject.SetActive(false);
      heatProgressBar.transform.Find("Node9").gameObject.SetActive(false);
      heatProgressBar.transform.Find("Node10").gameObject.SetActive(false);
      heatProgressBar.transform.Find("Node11").gameObject.SetActive(false);
      heatProgressBar.transform.Find("Node12").gameObject.SetActive(false);
      heatProgressBar.transform.Find("Node13").gameObject.SetActive(false);
   }

   public void HandlePreventativeMaintenance()
   {
      if(jammingChance > 0)
      {
         jammingChance = 0;
      }
      if (isJamPrevented)
      {
         MaintenanceCounter--;
         oreManager.ChangeMaintenanceCounter(MaintenanceCounter);
         if(MaintenanceCounter <= 0)
         {
            isJamPrevented = false;
            MaintenanceCounter = 5;
            jammingChance = previousJammingChance;
            oreManager.ActivateUnjamButton();
         }
      }
   }

   private bool TryShowFirstEnemyAttackTutorial(int harpoonAmount)
   {
      if (hasShownFirstEnemyAttackTutorial)
         return false;

      if (!TutorialFlowSettings.NarrativeTutorialEnabled)
         return false;

      if (NarrativeOverlayUI.Instance == null || NPCEncounterSystem.Instance == null)
         return false;

      NPCEncounterSystem.NPCProfile dolphinProfile =
         NPCEncounterSystem.Instance.GetProfileByPersonality(NPCEncounterSystem.NPCPersonality.Dolphin);

      if (dolphinProfile == null)
         return false;

      hasShownFirstEnemyAttackTutorial = true;

      NarrativeOverlayUI.Instance.SetGameplayBlocked(true);
      NarrativeOverlayUI.Instance.PlaySequence(
         dolphinProfile,
         new NPCEncounterSystem.DialogueLine[]
         {
            new NPCEncounterSystem.DialogueLine("Right. That's an attack alert.", NPCEncounterSystem.ExpressionType.Surprised),
            new NPCEncounterSystem.DialogueLine("If you have a harpoon ready, this is where you spend it to stop the hit before it lands.", NPCEncounterSystem.ExpressionType.Thinking),
            new NPCEncounterSystem.DialogueLine($"This one costs {harpoonAmount} harpoon{(harpoonAmount > 1 ? "s" : string.Empty)}.", NPCEncounterSystem.ExpressionType.Neutral),
            new NPCEncounterSystem.DialogueLine("And yes, it gets more expensive later. The farther we go, the worse these things get.", NPCEncounterSystem.ExpressionType.Special),
            new NPCEncounterSystem.DialogueLine("So if you can block the early ones cleanly, do it. Future Dolphin would appreciate the favor.", NPCEncounterSystem.ExpressionType.Happy)
         },
         NarrativeOverlayUI.DialogueLayoutMode.StoryBottom,
         false,
         () =>
         {
            NarrativeOverlayUI.Instance.SetGameplayBlocked(false);
            ShowEnemyEncounterPanel(harpoonAmount);

         });

      return true;
   }

   public static void ClearEvents() { OnTurnEnded = null; }
}