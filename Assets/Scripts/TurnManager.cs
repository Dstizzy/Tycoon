﻿using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Rendering.UI;
using static TickerSystem;
using static InventoryManager; 

public class TurnManager : MonoBehaviour
{

   // Constants
   const int MANUALRESETWAIT = 3;
   const int STARTINGTURN    = 1;
   const int ENDINGTURN      = 80;


   // Static fields
   public static System.Random random = new System.Random(); // Random number generator
   public static int       randomNumber;                     // Random number for various calculations
   public static int       jamTurnCounter;                   // Counter for turns during a jam
   public static bool      manualResetOption     = false;    // Flag for manual reset option
   public static int       heatLevel;                        // Current heat level
   public static int       enemyAttackPercentage = 0;        // Percentage chance of enemy attack each turn
   public static bool      userDefends;                      // Flag indicating if the user defends against enemy attacks 
   private TradeHutManager tradeHutManager;                  // Trade hut manager instance 
   public TickerSystem     newsTicker;                       // The wolrd event news ticker panel


   // Public Unity fields
   [Header("Turn Setting")]
   public int             currentTurn    = STARTINGTURN;   // The current turn number, starting from 1.
   public int             maxTurns       = ENDINGTURN;     // The maximum number of turns before the game ends.
   public TextMeshProUGUI turnText;                        // The UI text element to display the current turn.
   public int             eventCountdown = 1;              // Turn countdown until next world event

   [Header("UI/Game Status")]
   public Button          endTurnButton;                   // The button to disable when the game ends.
   private bool           _isGameActive  = true;           // Tracks if the game is currently in progress.

   [Header("Enemy Settings")]
   [SerializeField] private GameObject optionalEnemyPanel; // The enemy panel UI element.
   [SerializeField] private GameObject forcedEnemyPanel;   // The forced enemy panel UI element.
   [SerializeField] private GameObject heatProgressBar;    // The heat level progress bar UI element.


   // public propertries
   public static TurnManager Instance { get; private set; } // Singleton instance


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

      if(newsTicker == null)
         Debug.Log("Ticker is not assigned in the Inspector");
   }

   // Initializes the UI elements with the starting values when the game begins.                  
   void Start()
   {
      tradeHutManager = TradeHutManager.Instance;
      UpdateTurnUI();
      tradeHutManager.WorldEventChance();
   }

   // Advances the game to the next turn and updates the UI,
   public void EndTurn()
   {
      Debug.Log("### TurnManager Start() ###");

      // Do nothing if the game is already over
      if (!_isGameActive) return;

      currentTurn++;
      eventCountdown++;

      // Check if the game should end                 */
      if (currentTurn > maxTurns)
      {
         EndGame();
      }
      else
      {
         UpdateTurnUI();

         if (eventCountdown == 5) 
         {
            tradeHutManager.WorldEventNewsTickerText();
            newsTicker.ShowTicker(tradeHutManager.currrentNewsTickerMessage, Color.white, MessageTypes.WorldEvent);
            
            // World Event fluctuation
            tradeHutManager.MarketFluctuate();

            eventCountdown = 0;
         } 
         else 
         {
            if (eventCountdown == 1) 
            {
               tradeHutManager.ResetWorldEventShifts();
               tradeHutManager.WorldEventChance();
               tradeHutManager.MarketFluctuate();
            } 
            else 
            {
               if (eventCountdown >= 3 && eventCountdown <= 5) 
               {
                  newsTicker.gameObject.SetActive(true);
                  tradeHutManager.WorldEventNewsTickerText();
                  newsTicker.ShowTicker(tradeHutManager.currrentNewsTickerMessage, Color.white, MessageTypes.WorldEvent);
                  tradeHutManager.MarketFluctuate();
               }
               else
                  tradeHutManager.MarketFluctuate();
            }
         }

         tradeHutManager.CraftMarketForesight();

         if (currentTurn == 2)
            tradeHutManager.CraftMarketForesight();

         //// Natural flucuations
         //if(eventCountdown > 0 && eventCountdown != 5) 
         //{
         //   TradeHutManager.Instance.MarketFluctuate();
         //   TradeHutManager.Instance.CraftMarketForesight();
         //}

         Debug.Log("Turn" + currentTurn + "Start");


         // Add logic for the next turn here (e.g., start
         // enemy turn, reset unit actions, etc.)

         OnTurnEnded?.Invoke();
      }
   }

   // Updates the turn text UI element to display the current
   // turn and the maximum turn limit.        
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
            JamRefinery();
         }
      }
      else
      {
         if(manualResetOption == true)
         {
            if(jamTurnCounter > 0)
               jamTurnCounter--;
            else
            {
               OreRefinery_Manager.Instance.IsBlocked = false;
               OreRefinery_Manager.Instance.DeactivateJamSymbol();
               manualResetOption = false;
            }
         }      
      }
   }

   // Allows the player to manually reset the jammed Ore Refinery
   public void ManualResetUnjam()
   {
      manualResetOption = true;
   }

   // Jams the Ore Refinery and sets the jam turn counter
   public void JamRefinery()
   {
      OreRefinery_Manager.Instance.IsBlocked = true;
      OreRefinery_Manager.Instance.ActivateJamSymbol();
      jamTurnCounter = MANUALRESETWAIT;
   }

   // Handles enemy attack logic based on the current heat level
   public void HandleEnemy()
   {

      HandleHeat();
      HandleProgressBar();
      if(heatLevel <= 40)
      {
         enemyAttackPercentage = 0;
      }
      else if(heatLevel >= 41 && heatLevel < 70)
      {
         enemyAttackPercentage = 5;
      }
      else if(heatLevel >= 71 && heatLevel < 99)
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
         if (InventoryManager.Instance.harpoonCount > 0)
         {
            optionalEnemyPanel.SetActive(true);
            optionalEnemyPanel.transform.Find("Buttons/OptionOneButton").GetComponent<Button>().onClick.AddListener(() => 
            {
               userDefends = true;
               optionalEnemyPanel.SetActive(false);
            });
            optionalEnemyPanel.transform.Find("Buttons/OptionTwoButton").GetComponent<Button>().onClick.AddListener(() => 
            {
               userDefends = false;
               optionalEnemyPanel.SetActive(false);
               PopUpManager.Instance.EnablePlayerInput();
            });
         }
         else
         {
            forcedEnemyPanel.SetActive(true);
            forcedEnemyPanel.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() => 
            {
               userDefends = false;
               forcedEnemyPanel.SetActive(false);
               PopUpManager.Instance.EnablePlayerInput();
            });
         }
         if (userDefends)
         {
            heatLevel = 0;
            DeactivateHeatNodes();
         }
         else
         {
            InventoryManager.Instance.TrySpendOre((int)(InventoryManager.Instance.oreCount / 2));
            InventoryManager.Instance.TrySpendPearl((int)(InventoryManager.Instance.pearlCount / 4));
            JamRefinery();
            heatLevel = 10;
            DeactivateHeatNodes();
            heatProgressBar.transform.Find("Node1").gameObject.SetActive(true);
         }


      }
   }

   // Increases the heat level each turn
   public void HandleHeat()
   {
      heatLevel += 5;
      if(OreRefinery_Manager.Instance.IsBlocked || manualResetOption)
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
      if(heatProgressBar != null)
      {
         if (heatLevel >= 8)
         {
            heatProgressBar.transform.Find("Node1").gameObject.SetActive(true);
         }
         if(heatLevel >= 16)
         {
            heatProgressBar.transform.Find("Node2").gameObject.SetActive(true);
         }
         if(heatLevel >= 24)
         {
            heatProgressBar.transform.Find("Node3").gameObject.SetActive(true);
         }
         if(heatLevel >= 32)
         {
            heatProgressBar.transform.Find("Node4").gameObject.SetActive(true);
         }
         if(heatLevel >= 40)
         {
            heatProgressBar.transform.Find("Node5").gameObject.SetActive(true);
         }
         if(heatLevel >= 48)
         {
            heatProgressBar.transform.Find("Node6").gameObject.SetActive(true);
         }
         if(heatLevel >= 56)
         {
            heatProgressBar.transform.Find("Node7").gameObject.SetActive(true);
         }
         if(heatLevel >= 64)
         {
            heatProgressBar.transform.Find("Node8").gameObject.SetActive(true);
         }
         if(heatLevel >= 72)
         {
            heatProgressBar.transform.Find("Node9").gameObject.SetActive(true);
         }
         if(heatLevel >= 80)
         {
            heatProgressBar.transform.Find("Node10").gameObject.SetActive(true);
         }
         if(heatLevel >= 88)
         {
            heatProgressBar.transform.Find("Node11").gameObject.SetActive(true);
         }
         if(heatLevel >= 96)
         {
            heatProgressBar.transform.Find("Node12").gameObject.SetActive(true);
         }
         if(heatLevel >= 100)
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
}