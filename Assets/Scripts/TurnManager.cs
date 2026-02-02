using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TurnManager : MonoBehaviour
{

   // Constants
   int TRADEHUTCAPTURED = 1;
   int EXPLORATIONCAPTURED = 2;
   int FORGECAPTURED = 3;
   int LABCAPTURED = 4;
   int OREFINERYCAPTURED = 5;

   public static System.Random random = new System.Random();
   public static int randomNumber;
    // A public static instance of this class, following the 
    // Singleton pattern. This allows other scripts to access
    // it easily via 'TurnManager.Instance'.
    public static TurnManager Instance { get; private set; }

   // --- ADDED: Event System ---
   /* This event is broadcast to all other scripts */
   /* when the EndTurn() function is called. */
   public static event Action OnTurnEnded;

   public TradeHutManager tradeHutManager;

   [Header("Turn Setting")]
   public int currentTurn = 1;      // The current turn number, starting from 1.
   public int maxTurns = 80;     // The maximum number of turns before the game ends.
   public TextMeshProUGUI turnText; // The UI text element to display the current turn.
   public int eventCountdown = 1;   // Turn countdown until next world event

   [Header("UI/Game Status")]
   public Button endTurnButton;       // The button to disable when the game ends.
   private bool _isGameActive = true; // Tracks if the game is currently in progress.


   public static int  jamTurnCounter;                // Counter for turns during a jam
   public static bool isTradeHutCaptured    = false; // Flag to indicate if the Trade Hut is captured
   public static bool isOreRefineryCaptured = false; // Flag to indicate if the Ore Refinery is captured
   public static bool isExplorationCaptured = false; // Flag to indicate if the Exploration Unit is captured
   public static bool isForgeCaptured       = false; // Flag to indicate if the Forge is captured
   public static bool isLabCaptured         = false; // Flag to indicate if the Lab is captured
   public static int  enemyAttackPercentage = 5;     // Percentage chance of enemy attack each turn
   public static int  whichBuildingCaptured = 0;     // Indicates which building was captured
   public static bool manualResetOption     = false; // Flag for manual reset option


   // Enforces the Singleton pattern to ensure only one 
   // instance of TurnManager exists. 
   void Awake()
   {
      // Enforce the singleton pattern
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
   }

   // Initializes the UI elements with the starting values when the game begins.                  
   void Start()
   {
      tradeHutManager = TradeHutManager.Instance;
      UpdateTurnUI();
   }

   // Advances the game to the next turn and updates the UI,
   public void EndTurn()
   {
      Debug.Log("### TurnManager Start() ###");

      // Do nothing if the game is already over
      if (!_isGameActive) return;

      currentTurn++;
      if ((eventCountdown % 5) == 0) 
      {
         tradeHutManager.WorldEventNewsTickerText();
         tradeHutManager.WorldEvent();
         tradeHutManager.WorldEventChance();

         eventCountdown = 0;
      }
      else 
      {
         if(eventCountdown >= 3)
            tradeHutManager.WorldEventNewsTickerText();

         eventCountdown++;
      }

      HandleJamming();
      HandleEnemy();


      // Check if the game should end
      if (currentTurn > maxTurns)
      {
         EndGame();
      }
      else
      {
         UpdateTurnUI();

         if(currentTurn == 2)
            TradeHutManager.Instance.CraftMarketForesight();

         TradeHutManager.Instance.MarketFluctuate();
         TradeHutManager.Instance.CraftMarketForesight();
         Debug.Log("Turn" + currentTurn + "Start");


         // Add logic for the next turn here (e.g., start
         // enemy turn, reset unit actions, etc.)

         OnTurnEnded?.Invoke();
      }
   }

   /***************************************************/
   /* Updates the turn text UI element to display the */
   /* current turn and the maximum turn limit.        */
   /***************************************************/
   void UpdateTurnUI()
   {
      if (turnText != null)
      {
         turnText.text = currentTurn.ToString() + " / " + maxTurns.ToString();
      }
   }

   /***************************************************/
   /* Called when the 'maxTurns' limit is reached.    */
   /* It stops the game logic and updates the UI.     */
   /***************************************************/
   void EndGame()
   {
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
   }

   // Handles the jamming logic for the Ore Refinery at the start of each turn
   public void HandleJamming()
   {
      if(OreRefinery_Manager.Instance.IsBlocked == false)
      {
         randomNumber = random.Next(1, 100);
         if (randomNumber < OreRefinery_Manager.Instance.JammingPercentage)
         {
            OreRefinery_Manager.Instance.IsBlocked = true;
            OreRefinery_Manager.Instance.ActivateJamSymbol();
            jamTurnCounter = 0;
         }
      }
      else
      {
         if(manualResetOption == true)
         {
            if(jamTurnCounter <= 3)
               jamTurnCounter++;
            else
            {
               OreRefinery_Manager.Instance.IsBlocked = false;
               OreRefinery_Manager.Instance.DeactivateJamSymbol();
               jamTurnCounter = 0;
               manualResetOption = false;
            }
         }      
      }
   }

   public void ManualResetUnjam()
   {
      manualResetOption = true;
      jamTurnCounter = 0;
   }

   public void HandleEnemy()
   {
      enemyAttackPercentage += InventoryManager.Instance.pearlCount;
      if(whichBuildingCaptured == 0)
      {
         randomNumber = random.Next(1,100);
         if(randomNumber < enemyAttackPercentage)
         {
            if(OreRefinery_Manager.Instance.IsBlocked == false)
            {
               randomNumber = random.Next(1, 4);
            }
            else
            {
               randomNumber = random.Next(1, 5);
            }
            switch(randomNumber)
            {
               case 1:
                  isTradeHutCaptured = true;
                  whichBuildingCaptured = TRADEHUTCAPTURED;
                  break;
               case 2:
                  isExplorationCaptured = true;
                  whichBuildingCaptured = EXPLORATIONCAPTURED;
                  break;
               case 3:
                  isForgeCaptured = true;
                  whichBuildingCaptured = FORGECAPTURED;
                  break;
               case 4:
                  isLabCaptured = true;
                  whichBuildingCaptured = LABCAPTURED;
                  break;
               case 5:
                  isOreRefineryCaptured = true;
                  whichBuildingCaptured = OREFINERYCAPTURED;
                  break;
               default:
                  break;
            }
         }
      }
   }
}