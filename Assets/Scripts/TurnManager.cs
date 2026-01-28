using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TurnManager : MonoBehaviour
{

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

   [Header("Turn Setting")]
   public int currentTurn = 1;      // The current turn number, starting from 1.
   public int maxTurns    = 20;     // The maximum number of turns before the game ends.
   public TextMeshProUGUI turnText; // The UI text element to display the current turn.
   public int eventCountdown = 1;   // Turn countdown until next world event

   [Header("UI/Game Status")]
   public Button endTurnButton;       // The button to disable when the game ends.
   private bool _isGameActive = true; // Tracks if the game is currently in progress.

   /*************************************************/
   /* Initializes the UI elements with the starting */
   /* values when the game begins.                  */
   /*************************************************/
   void Start()
   {
      tradeHutManager = TradeHutManager.Instance;
      UpdateTurnUI();
   }

   /***************************************************/
   /* This function is called by the End Turn button. */
   /* It processes the end-of-turn logic, including   */
   /* resource gains and advancing the turn counter.  */
   /***************************************************/
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

         randomNumber = random.Next(1,100);
         if(randomNumber < OreRefinery_Manager.Instance.JammingPercentage)
         {
            OreRefinery_Manager.Instance.IsBlocked = true;
            OreRefinery_Manager.Instance.ActivateJamSymbol();
         }

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
}