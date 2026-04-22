using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WalkthroughTurnManager : MonoBehaviour
{
   [Header("Turn Setting")]
   public int currentTurn = 0;
   public int maxTurns = 5;
   public TextMeshProUGUI walkthroughTurnText;
   public TextMeshProUGUI turnText;                        // The UI text element to display the current turn.
   public int eventCountdown = 1;              // Turn countdown until next world event

   [Header("UI/Game Status")]
   public Button endTurnButton;                     // The button to disable when the game ends.
   private bool _isGameActive = true;             // Tracks if the game is currently in progress.
   [SerializeField] private GameObject progressBar; // Turn changing progress bar UI element.

   private bool isAdvancingTurn;  // Flag to prevent multiple turn advancements at the same time

   public static WalkthroughTurnManager Instance { get; private set; }

   // Start is called once before the first execution of Update after the MonoBehaviour is created
   public void Awake()
   {
      if (walkthroughTurnText != null)
      {
         walkthroughTurnText.text = currentTurn.ToString() + " / " + maxTurns.ToString();
      }

      endTurnButton.onClick.AddListener(WalkthroughEndTurn);

      if (Instance != null && Instance != this)
      {
         Destroy(gameObject); // Kills the new "duplicate" immediately
         return;
      }
      Instance = this;
      DontDestroyOnLoad(gameObject);
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
}
