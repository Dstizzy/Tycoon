using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnManager : MonoBehaviour
{
   // A public static instance of this class, following the 
   // Singleton pattern. This allows other scripts to access
   // it easily via 'TurnManager.Instance'.
   public static TurnManager Instance { get; private set; }

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

   [Header("Crystal Setting")]
   public int currentResource = 0;      // The player's current total amount of Crystal.
   public int resourcePerTurn = 50;     // The amount of resource gained per turn.
   public TextMeshProUGUI resourceText; // The UI text element that displays the resource count. 

   [Header("Turn Setting")]
   public int currentTurn = 1;      // The current turn number, starting from 1.
   public int maxTurns    = 20;     // The maximum number of turns before the game ends.
   public TextMeshProUGUI turnText; // The UI text element to display the current turn.

   [Header("UI/Game Status")]
   public Button endTurnButton;       // The button to disable when the game ends.
   private bool _isGameActive = true; // Tracks if the game is currently in progress.

   /*************************************************/
   /* Initializes the UI elements with the starting */
   /* values when the game begins.                  */
   /*************************************************/
   void Start()
   {
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

      currentResource += resourcePerTurn;

      currentTurn++;

      // Check if the game should end
      if (currentTurn > maxTurns)
      {
         EndGame();
      }
      else
      {
         UpdateTurnUI();
         Debug.Log("Turn" + currentTurn + "Start");

         // Add logic for the next turn here (e.g., start
         // enemy turn, reset unit actions, etc.)        
      }
   }

   /***************************************************/
   /* Called last every frame after all Update().     */
   /* Ensures UI text overrides other scripts that    */
   /* might change it during the Update() phase       */
   /***************************************************/
   void LateUpdate()
   {
      UpdateResourceUI();
   }

   /***************************************************/
   /* Updates the resource text UI element to display */
   /* the current value of 'currentResource'.         */
   /***************************************************/
   void UpdateResourceUI()
   {
      if (resourceText != null)
      {
         resourceText.text = currentResource.ToString();
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