using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using JetBrains.Annotations;

public class ExplorationUnitManager : MonoBehaviour
{
   public LabManager   labManager;
   public PanelManager panelManager;
   [SerializeField] private EventDatabase     eventDatabase;   // Contains all potential node events
   [SerializeField] private EventUIController eventController; // Manages UI of current event
   [SerializeField] private ShipManager       shipManager;     // Tracks the ship's stats and inventory

   [SerializeField] private Transform explorePanel;         // Panel to start an expedition
   [SerializeField] private Transform upgradePanel;         // Panel to upgrade the exploration unit
   [SerializeField] private Transform infoPanel;            // Panel to show general exploration info
   [SerializeField] private Transform decisionPanel;        // Pop-up panel during exploration where user makes choices
   [SerializeField] private Transform decisionResultsPanel; // Shows results from a event decision
   [SerializeField] private Transform newDepthPanel;        // Warns that a dangerous depth tier is being entered
   [SerializeField] private Transform inventoryPanel;       // Shows the ship's current inventory

   [SerializeField] private TextMeshProUGUI decisionResults;  // Displays the narrative outcome of an event choice
   [SerializeField] private TextMeshProUGUI depthWarningText; // Displays the calculated pressure damage when new depth is entered

   [Header("Icon UI Settings")]
   [SerializeField] private Transform inventoryIconContainer; // Holds item slots in cargo panel
   [SerializeField] private Transform resultsIconContainer;   // Holds item slots in decision results panel
   [Header("Results Animation Settings")]
   [SerializeField] private TextMeshProUGUI resultsHealthText;     // Health fraction displayed on decision results panel
   [SerializeField] private TextMeshProUGUI resultsFuelText;       // Fuel fraction displayed on decision resullts panel
   [SerializeField] private TextMeshProUGUI floatingResultsHealth; // Fading floating text in decision results panel for health change
   [SerializeField] private TextMeshProUGUI floatingResultsFuel;   // Fading floating text in decision results panel for fuel change

   [Header("Exploration Visuals")]
   [SerializeField] private SpriteRenderer  buildingSpriteRenderer;
   [SerializeField] private List<Sprite>    explorationLevelSprites;
   [SerializeField] private TextMeshProUGUI explorationLevelText;

   [Header("Level UI")]
   [SerializeField] private Transform levelCanvas; // Canvas showing current exploration unit level


   private MapNode nextTurnDestination; // Map node ship is scheduled to move to on next turn

   // ID constants for the base menu buttons
   const int EXPLORE_BUTTON    = 1;
   const int INFO_BUTTON       = 2;
   const int UPGRADE_BUTTON    = 3;
   const int LEVEL2_PEARL_COST = 200; // Pearl cost to reach level 2
   const int LEVEL3_PEARL_COST = 500; // Pearl cost to reach level 3
   const int MAX_SHIP_LEVEL    = 3;   // The maximum level the exploration unit can reach

   public  bool isExploring       = false; // Determines if exploration is currently ongoing
   private bool isWaiting         = false; // Triggered when an event causes user to lose an exploration turn
   private int  lastProcessedTurn = 0;     // Tracks the last turn that has been processed

   public static ExplorationUnitManager Instance { get; private set; }

   private void Awake()
   {
      // Verify all panels are assigned and disable them at startup
      if (infoPanel != null)
         infoPanel.gameObject.SetActive(false);
      if (explorePanel != null)
         explorePanel.gameObject.SetActive(false);
      if (upgradePanel != null)
         upgradePanel.gameObject.SetActive(false);
   }
   private void Start()
   {
      UpdateExplorationSprites();
      UpdateLevel();
      if (explorationLevelText != null && shipManager != null)
         explorationLevelText.text = "Level " + shipManager.ShipLevel.ToString();
   }

   void Update()
   {
      if (isExploring && TurnManager.Instance.currentTurn > lastProcessedTurn)
      {
         // A new turn has started, but we wait until the UI is clear
         lastProcessedTurn = TurnManager.Instance.currentTurn;
         HandleNewTurn();
      }
   }

   // Event Adder
   private void OnEnable()
   {
      ShipManager.OnShipDeath += HandleExplorationDone;
   }

   // Event destroyer
   private void OnDisable()
   {
      ShipManager.OnShipDeath -= HandleExplorationDone;
   }

   // Activates the requested exploration unit panel
   public void RequestExplorationUnitPanel(int buttonID)
   {
      switch (buttonID)
      {
         case EXPLORE_BUTTON:
            ShowExplorationPanel();
            // Dynamically configure start button interactability
            Button exploreButton = explorePanel.Find("ExploreButton").GetComponent<Button>();
            if (exploreButton != null)
            {
               exploreButton.onClick.RemoveAllListeners();
               //             bool hasDivingBell = InventoryManager.Instance.divingBellCount > 0;
               bool canExplore = !isExploring; //&& hasDivingBell;
               exploreButton.interactable = canExplore;
               if (canExplore)
               {
                  exploreButton.onClick.AddListener(() => StartExploration());
               }
            }
            // Setup exit button
            explorePanel.transform.Find("ExitButton").GetComponent<Button>().onClick.AddListener(() => CloseExplorationPanel());
            break;
         case INFO_BUTTON:
            ShowInfoPanel();
            infoPanel.transform.Find("ExitButton").GetComponent<Button>().onClick.AddListener(() => CloseInfoPanel());
            break;
         case UPGRADE_BUTTON:
            ShowUpgradePanel();
            // Setup upgrade confirmation button
            Button yesButton = upgradePanel.Find("YesButton").GetComponent<Button>();
            if (yesButton != null)
            {
               yesButton.onClick.RemoveAllListeners();
               yesButton.onClick.AddListener(() => ConfirmUpgrade());
            }
            // Setup exit button
            upgradePanel.transform.Find("CancelButton").GetComponent<Button>().onClick.AddListener(() => CloseUpgradePanel());
            break;
         default:
            Debug.Log("Building Panel: Unknown button ID.");
            break;
      }
   }

   // Starts exploration, gets the starting node, and queues the first move
   public void StartExploration()
   {
      isExploring = true;
      SetDecisionInteractable(true);
      lastProcessedTurn = TurnManager.Instance.currentTurn;

      if (MapManager.Instance.startingNode != null)
      {
         MapManager.Instance.MoveToNode(MapManager.Instance.startingNode);
         nextTurnDestination = null;
      }

      CloseExplorationPanel();
      StartCoroutine(InitializeFirstTurnRoutine());
   }

   // Initialize the first decision (with a slight delay for UI transitions)
   private IEnumerator InitializeFirstTurnRoutine()
   {
      yield return new WaitForSeconds(0.5f);
      HandleNewTurn();
   }

   // Upgrades the ship if it is currently able to be upgraded
   public void ConfirmUpgrade()
   {
      int upgradeCost = GetUpgradeCost();
      
      // Upgrade ship if is not at max level and if it can be afforded
      if (shipManager.ShipLevel < MAX_SHIP_LEVEL && InventoryManager.Instance.TrySpendPearl(upgradeCost))
      {
         shipManager.UpgradeShip();
         UpdateExplorationSprites();
         UpdateLevel();

         if (explorationLevelText != null)
            explorationLevelText.text = "Level " + shipManager.ShipLevel.ToString();

         // Disable upgrade button if max level has been reached
         if (shipManager.ShipLevel == MAX_SHIP_LEVEL)
         {
            if (InventoryManager.Instance.ExplorationUnitUpgradeIcon != null)
               InventoryManager.Instance.ExplorationUnitUpgradeIcon.gameObject.SetActive(false);
            else
               Debug.LogWarning("ExplorationUnitUpgradeIcon is not assigned in the InventoryManager!");
         }

         Debug.Log($"Exploration Unit upgraded to level {shipManager.ShipLevel}!");
         if (TickerSystem.Instance != null)
            TickerSystem.Instance.ShowTicker($"Exploration Unit upgraded to level {shipManager.ShipLevel}!", Color.green, TickerSystem.MessageTypes.ResultMessage);

         CloseUpgradePanel();
         PopUpManager.Instance.EnablePlayerInput();
      }
   }

   // Interrupts movement with warning panel if ship is moving into depth beyond current ship level
   private bool CheckForDepthIncrease(MapNode targetNode)
   {
      // If target depth is greater than current depth and ship level is lower whan target depth level
      if (targetNode != null && targetNode.nodeDepth > shipManager.GetDepth() && shipManager.ShipLevel < targetNode.nodeDepth)
      {
         newDepthPanel.gameObject.SetActive(true);

         // Calculates how much damage it will take
         int predictedDamage = shipManager.GetDamage(targetNode.nodeDepth);
         if (depthWarningText != null)
            depthWarningText.text = $"Ship entering new depth.\nPressure will exceed hull rating.\nTaking {predictedDamage} health per turn";

         Button sendHome  = newDepthPanel.Find("Return").GetComponent<Button>();
         Button keepGoing = newDepthPanel.Find("KeepGoing").GetComponent<Button>();

         sendHome.onClick.RemoveAllListeners();
         keepGoing.onClick.RemoveAllListeners();

         // Option 1: Abort current exploration to secure loot
         sendHome.onClick.AddListener(() =>
         {
            nextTurnDestination = null;
            newDepthPanel.gameObject.SetActive(false);
            StartCoroutine(shipManager.FinishExploration());
         });
         // Option 2: Keep the ship on its exploration
         keepGoing.onClick.AddListener(() =>
         {
            newDepthPanel.gameObject.SetActive(false);
            CloseDecisionPanel();
         });
         return true;
      }
      return false;
   }

   // Populates up to 3 choice buttons on the decision panel with scenario data
   private void SetupButtons(string textA, UnityAction actionA, bool interactableA,
                             string textB, UnityAction actionB, bool interactableB,
                             string textC, UnityAction actionC, bool interactableC)
   {
      Transform container = decisionPanel.Find("ButtonContainer");
      MapNode currentNode = MapManager.Instance.currentNode;

      // Reset button lock and hover logic for new decision
      foreach (Transform child in container)
      {
         NodeHover hover = child.GetComponent<NodeHover>();
         if (hover != null) hover.isTierLocked = false;
         child.gameObject.SetActive(true);
         Button btn = child.GetComponent<Button>();
         if (btn != null) btn.interactable = true;
      }

      ExploreEvents currentEvent = eventController.currentEvent;

      // Sets up choice 1 (always exists)
      Button button1 = container.Find("Choice1").GetComponent<Button>();
      if (button1 != null)
      {
         button1.gameObject.SetActive(true);
         button1.GetComponentInChildren<TextMeshProUGUI>().text = textA;
         button1.onClick.RemoveAllListeners();
         if (actionA != null) button1.onClick.AddListener(actionA);

         // Check if choice is locked by Lab upgrades
         bool isLockedA = (currentNode  != null && currentNode.type != MapNode.NodeType.Directional) && 
                           currentEvent != null && currentEvent.choiceA.requiresLabTier && !shipManager.isTier2Unlocked;

         NodeHover hover1 = button1.GetComponent<NodeHover>();
         if (hover1 != null) hover1.isTierLocked = isLockedA;
         button1.interactable = interactableA && !isLockedA;
      }
      // Sets up choice 2 if it exists for current scenario
      Button button2 = container.Find("Choice2").GetComponent<Button>();
      if (button2 != null)
      {
         if (!string.IsNullOrEmpty(textB))
         {
            button2.gameObject.SetActive(true);
            button2.GetComponentInChildren<TextMeshProUGUI>().text = textB;
            button2.onClick.RemoveAllListeners();
            if (actionB != null) button2.onClick.AddListener(actionB);

            // Check if choice is locked by Lab upgrades
            bool isLockedB = (currentNode  != null && currentNode.type != MapNode.NodeType.Directional) && 
                              currentEvent != null && currentEvent.choiceB.requiresLabTier && !shipManager.isTier2Unlocked;
            NodeHover hover2 = button2.GetComponent<NodeHover>();
            if (hover2 != null)
               hover2.isTierLocked = isLockedB;
            button2.interactable = interactableB && !isLockedB;
         }
         else
            button2.gameObject.SetActive(false);
      }
      // Sets up choice 3 if it exists for current scenario
      Button button3 = container.Find("Choice3").GetComponent<Button>();
      if (button3 != null)
      {
         if (!string.IsNullOrEmpty(textC))
         {
            button3.gameObject.SetActive(true);
            button3.GetComponentInChildren<TextMeshProUGUI>().text = textC;
            button3.interactable = interactableC;
            button3.onClick.RemoveAllListeners();
            if (actionC != null)
               button3.onClick.AddListener(actionC);
         }
         else
            button3.gameObject.SetActive(false);
      }
   }

   // Calculates mathematical outcome of an event choice and displays the summary
   private void ProcessDecision(EventChoice choice, MapNode currentNode)
   {
      // Send choice to the ShipManager to calculate and apply all damage and loot
      ShipManager.RoundResults results = shipManager.ApplyEventResult(choice);

      // If event results in lost turn, flags to skip the next turn
      if (choice.waitTurn)
         isWaiting = true;

      // Show choice summary if ship stats were changed or loot was gained
      if (results.HasChanges())
         ShowResultsPanel(results, currentNode, choice.resultText);
      // Otherwise, move on to the next turn
      else
         if (currentNode != null)
         {
            nextTurnDestination = currentNode.nextNode;
            if (!CheckForDepthIncrease(nextTurnDestination))
               CloseDecisionPanel();
         }
         else
            CloseDecisionPanel();
   }

   // Handles logic for new turn starting (movement, ship stat losses, event generation)
   public void HandleNewTurn()
   {
      if (!isExploring) return;
      // Handle if user has to skip a turn
      if (isWaiting)
      {
         shipManager.NewTurn();
         if (!isExploring) return;
         isWaiting = false;
         return;
      }
      // Execute ship map movement if next destination exists
      if (nextTurnDestination != null)
      {
         MapManager.Instance.MoveToNode(nextTurnDestination);
         nextTurnDestination = null;
      }
      // Decrement fuel and apply potential pressure hull damage
      shipManager.NewTurn();
      if (!isExploring) return;

      MapNode current = MapManager.Instance.currentNode;
      if(current == null) return;

      // Check for exploration end condition
      if (current.isFinalNode)
      {
         HandleFinalNode(current);
         return;
      }

      ShowDecisionPanel();

      // Set up cargo tab button on decision panel
      Button inventoryButton = decisionPanel.Find("CargoTab").GetComponent<Button>();
      inventoryButton.onClick.RemoveAllListeners();
      inventoryButton.onClick.AddListener(() =>
      {
         ShowInventoryPanel();
         decisionPanel.gameObject.SetActive(false);
      });

      // Set up return ship button on decision panel
      Button returnShip = decisionPanel.Find("ReturnButton").GetComponent<Button>();
      returnShip.onClick.RemoveAllListeners();
      returnShip.onClick.AddListener(() => shipManager.OpenConfirmReturnPanel());

      // Handle a directional node decision
      if (current.type == MapNode.NodeType.Directional)
      {
         eventController.scenarioText.text = current.navigationStory;

         SetupButtons(
            current.choiceAText, () => { nextTurnDestination = current.pathA; if (!CheckForDepthIncrease(nextTurnDestination)) CloseDecisionPanel(); }, true,
            current.choiceBText, () => { nextTurnDestination = current.pathB; if (!CheckForDepthIncrease(nextTurnDestination)) CloseDecisionPanel(); }, true,
            current.choiceCText, () => { nextTurnDestination = current.pathC; if (!CheckForDepthIncrease(nextTurnDestination)) CloseDecisionPanel(); }, true
         );
      }
      // Handle event node decision
      else
      {
         // Generate a random event from current depth table
         ExploreEvents randomEvent = eventDatabase.GetRandomEvent(current.nodeDepth);
         if (randomEvent != null)
         {
            eventController.SetEventPanel(randomEvent);
            bool canAffordA = shipManager.CanAfford(randomEvent.choiceA) && (!randomEvent.choiceA.requiresLabTier || shipManager.isTier2Unlocked);
            bool canAffordB = shipManager.CanAfford(randomEvent.choiceB) && (!randomEvent.choiceB.requiresLabTier || shipManager.isTier2Unlocked);

            string textA = randomEvent.choiceA.buttonText;
            string textB = !string.IsNullOrEmpty(randomEvent.choiceB.buttonText) ? randomEvent.choiceB.buttonText : null;

            SetupButtons(
               textA, () => ProcessDecision(randomEvent.choiceA, current), canAffordA,
               textB, () => ProcessDecision(randomEvent.choiceB, current), canAffordB,
               null, null, false
            );
         }
      }
   }

   // Tells game exploration is done and resets the next turn node
   public void HandleExplorationDone()
   {
      isExploring = false;
      nextTurnDestination = null;
      SetDecisionInteractable(true);
   }

   // Handles the end-of-map sequence
   public void HandleFinalNode(MapNode current)
   {
      bool isWinner = false;

      // Check if current final node is the winning final node
      if (current.isLeftPath == MapManager.Instance.winningPathIsLeft)
         isWinner = true;

      if (isWinner)
      {
         decisionResults.text = "MISSION ACCOMPLISHED!\nYou have found the vessel piece.\nYou will now return.";
         panelManager.OpenPanel(decisionResultsPanel.gameObject);
         Button confirmEnd = decisionResultsPanel.transform.Find("ConfirmButton").GetComponent<Button>();
         confirmEnd.onClick.RemoveAllListeners();
         confirmEnd.onClick.AddListener(() =>
         {
            labManager.ActivateTail();
            StartCoroutine(shipManager.FinishExploration());
            panelManager.ClosePanel(decisionResultsPanel.gameObject);
         });
      }
      else
      {
         EventChoice consolationPrize = new EventChoice();
         consolationPrize.pearlChange = 200;
         consolationPrize.oreChange   = 200;
         ProcessDecision(consolationPrize, null);

         decisionResults.text = "DEAD END\n\nThe vessel piece is not here, but the chest is not empty!\nPearl: +200\nOre: +200";
      }
   }

   // Show the main exploration panel
   private void ShowExplorationPanel()
   {
      panelManager.OpenPanel(explorePanel.gameObject);
      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(false);
   }

   // Show the exploration information panel
   private void ShowInfoPanel()
   {
      panelManager.OpenPanel(infoPanel.gameObject);
      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(false);
   }

   // Show and dynamically update the exploration upgrade panel
   private void ShowUpgradePanel()
   {
      int pearlUpgradeCost = 0;
      //int oreUpgradeCost = 0;

      // Locate and update text fields on upgrade panel
      string upgradeTitle       = "";
      string upgradeExplanation = "";

      panelManager.OpenPanel(upgradePanel.gameObject);

      Transform titleTextTransform = upgradePanel.Find("Title");
      Transform mainTextTransform = upgradePanel.Find("UpgradePanelText");
      Transform pearlCostTransform = upgradePanel.Find("PearlCostText");
      Transform oreCostTransform = upgradePanel.Find("OreCostText");
      Transform explanationTransform = upgradePanel.Find("ExplanationText");

      TextMeshProUGUI title         = titleTextTransform   != null ? titleTextTransform.GetComponent<TextMeshProUGUI>()   : null;
      TextMeshProUGUI upgradeText   = mainTextTransform    != null ? mainTextTransform.GetComponent<TextMeshProUGUI>()    : null;
      TextMeshProUGUI pearlCostText = pearlCostTransform   != null ? pearlCostTransform.GetComponent<TextMeshProUGUI>()   : null;
      TextMeshProUGUI oreCostText   = oreCostTransform     != null ? oreCostTransform.GetComponent<TextMeshProUGUI>()     : null;
      TextMeshProUGUI expText       = explanationTransform != null ? explanationTransform.GetComponent<TextMeshProUGUI>() : null;

      if (shipManager.ShipLevel == 1)
      {
         pearlUpgradeCost   = LEVEL2_PEARL_COST;
         upgradeTitle       = "REWARD: ";
         upgradeExplanation = "";
      }
      else if (shipManager.ShipLevel == 2)
      {
         pearlUpgradeCost   = LEVEL3_PEARL_COST;
         upgradeTitle       = "REWARD:";
         upgradeExplanation = "";
      }

      if (shipManager.ShipLevel < MAX_SHIP_LEVEL)
      {
         if (title != null)
            title.text = $"LEVEL {shipManager.ShipLevel + 1} UPGRADE";
         if (upgradeText != null)
            upgradeText.text = upgradeTitle;

         if (pearlCostText != null)
            pearlCostText.text = $"{pearlUpgradeCost}";

         if (expText != null)
         {
            expText.gameObject.SetActive(true);
            expText.text = $"<color=black>{upgradeExplanation}</color>";
         }

         Transform yesBtn = upgradePanel.Find("YesButton");
         if (yesBtn != null) yesBtn.gameObject.SetActive(true);
      }
      else
      {
         if (upgradeText != null)
            upgradeText.text = "Max Level Reached!";

         if (expText != null)
            expText.gameObject.SetActive(false); // Hide explanation if max level

         Transform yesBtn = upgradePanel.Find("YesButton");
         if (yesBtn != null) yesBtn.gameObject.SetActive(false);
      }

      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(false);
   }

   // Shows the exploration event decision panel
   public void ShowDecisionPanel()
   {
      panelManager.OpenPanel(decisionPanel.gameObject);
      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(false);
   }

   // Prepares decision results panel and triggers stat and reward animations
   private void ShowResultsPanel(ShipManager.RoundResults results, MapNode currentNode, string resultMessage)
   {
      panelManager.OpenPanel(decisionResultsPanel.gameObject);
      SetDecisionInteractable(false);
      decisionResults.text = resultMessage;

      // Clear previous decision results
      if (resultsIconContainer != null)
         foreach (Transform child in resultsIconContainer)
         {
            child.gameObject.SetActive(false);
            child.localScale = Vector3.zero;
         }

      // Start animation sequence for results
      StartCoroutine(ShowDecisionResultsSequence(results, currentNode));
   }

   // Manages the sequential animation of stat changes and loot icons in decision results panel
   private IEnumerator ShowDecisionResultsSequence(ShipManager.RoundResults results,  MapNode currentNode)
   {
      // Set up the health and fuel text
      resultsHealthText.text = $"{results.healthBefore}/{results.maxHealth}";
      resultsFuelText.text   = $"{results.fuelBefore}/{results.maxFuel}";
      floatingResultsHealth.gameObject.SetActive(false);
      floatingResultsFuel.gameObject.SetActive(false);

      // Run ship health or fuel animation if either stat was changed by decision result
      if(results.healthChanged != 0)
         StartCoroutine(AnimateStatChange(resultsHealthText, floatingResultsHealth, results.healthBefore, results.healthChanged, results.maxHealth));
      if (results.fuelChanged != 0)
         StartCoroutine(AnimateStatChange(resultsFuelText, floatingResultsFuel, results.fuelBefore, results.fuelChanged, results.maxFuel));

      float delayBetweenItems = 0.2f; // The time delay between items popping in to panel
      var resultData = new (string Name, int Amount)[]
      {
         ("Pearl",            results.pearlChanged),
         ("Ore",              results.oreChanged),
         ("Patch Kit",        results.patchKitChanged),
         ("Harpoon",          results.harpoonChanged),
         ("Crude Tool",       results.crudeToolChanged),
         ("Pressure Valve",   results.pressureValveChanged),
         ("Diving Bell",      results.divingBellChanged),
         ("Clockwork Engine", results.clockworkEngineChanged),
         ("Precision Lens",   results.precisionLensChanged),
         ("Health",           results.healthChanged),
         ("Fuel",             results.fuelChanged)
      };

      // Check for each loot item in the decision results
      foreach (var item in resultData)
      {
         if (item.Amount != 0)
         {
            Transform iconRow = TrySpawnResultIcon(item.Name, item.Amount);
            if (iconRow != null)
            {
               // Trigger the "Pop" animation
               yield return new WaitForSeconds(delayBetweenItems);
               StartCoroutine(AnimatePop(iconRow));
            }
         }
      }
      // Configure the button to move forward
      Button continueButton = decisionResultsPanel.transform.Find("ConfirmButton").GetComponent<Button>();
      continueButton.onClick.RemoveAllListeners();
      continueButton.onClick.AddListener(() =>
      {
         StartCoroutine(ResultsDelay(currentNode));
      });
   }

   // Animates floating fuel and health results, and updates stat fractions
   private IEnumerator AnimateStatChange(TextMeshProUGUI mainText, TextMeshProUGUI deltaText, int startVal, int change, int max)
   {
      // Update the stat fraction with the post-result values
      int newVal     = startVal + change;
      mainText.text  = $"{newVal}/{max}";

      // Set up floating stat with correct sign, value, color, and starting/ending position
      string sign    = change > 0 ? "+" : "" ;
      deltaText.text = $"{sign}{change}";
      deltaText.color = change > 0 ? Color.green : Color.red;
      deltaText.gameObject.SetActive(true);
      Vector3 startPos = deltaText.transform.localPosition;
      Vector3 endPos   = startPos + (change > 0 ? new Vector3(0, 30, 0) : new Vector3(0, -30, 0));

      float duration = 2.0f; // How long the floating text animation lasts
      float elapsed  = 0.0f;

      // Run the floating stat animation
      while (elapsed < duration)
      {
         elapsed += Time.deltaTime;
         float t  = elapsed / duration;
         deltaText.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
         deltaText.alpha                   = Mathf.Lerp(1, 0, t);
         yield return null;
      }
      // Hide floating stat and reset back to original position
      deltaText.gameObject.SetActive(false);
      deltaText.transform.localPosition = startPos;
   }

   // Searches for specific icon object and configures its quantity
   private Transform TrySpawnResultIcon(string itemName, int amount)
   {
      Transform slotTransform = resultsIconContainer.Find(itemName);
      if (slotTransform == null) return null;

      slotTransform.gameObject.SetActive(true);

      TextMeshProUGUI txt = slotTransform.Find("Count").GetComponent<TextMeshProUGUI>();
      if (txt != null)
      {
         string sign = amount > 0 ? "+" : "-";
         txt.text = $"{sign}{amount}";
      }
      return slotTransform;
   }

   // Elastic pop animation for rewards icon entries
   private IEnumerator AnimatePop(Transform target)
   {
      float duration     = 0.4f;
      float elapsed      = 0.0f;
      Vector3 startScale = Vector3.zero;
      Vector3 endScale   = Vector3.one;

      while (elapsed < duration)
      {
         elapsed += Time.deltaTime;
         float percent = elapsed / duration;
         float curve   = Mathf.Sin(percent * Mathf.PI * 1.2f) / 1.2f;
         target.localScale = Vector3.LerpUnclamped(startScale, endScale, percent + (1f - percent) * curve);
         yield return null;
      }
      target.localScale = endScale;
   }

   // Closes results panel and moves to next map node
   private IEnumerator ResultsDelay(MapNode currentNode)
   {
      panelManager.ClosePanel(decisionResultsPanel.gameObject);
      SetDecisionInteractable(true);
      yield return new WaitForSeconds(0.2f);
      if (currentNode != null)
      {
         nextTurnDestination = currentNode.nextNode;
         if (!CheckForDepthIncrease(nextTurnDestination))
            CloseDecisionPanel();
      }
      else
         CloseDecisionPanel();
   }

   // Shows and populates cargo tab panel with all resources currently held
   private void ShowInventoryPanel()
   {
      inventoryPanel.gameObject.SetActive(true);
      SetDecisionInteractable(false);

      SpawnRewardIcon("Pearl",            shipManager.GetPearl());
      SpawnRewardIcon("Ore",              shipManager.GetOre());
      SpawnRewardIcon("Patch Kit",        shipManager.GetPatchKit());
      SpawnRewardIcon("Harpoon",          shipManager.GetHarpoon());
      SpawnRewardIcon("Crude Tool",       shipManager.GetCrudeTool());
      SpawnRewardIcon("Pressure Valve",   shipManager.GetPressureValve());
      SpawnRewardIcon("Diving Bell",      shipManager.GetDivingBell());
      SpawnRewardIcon("Clockwork Engine", shipManager.GetClockworkEngine());
      SpawnRewardIcon("Precision Lens",   shipManager.GetPrecisionLens());

      // Set up the charts tab button to return to the decision panel
      Button closeInventoryPanel = inventoryPanel.Find("ChartsTab").GetComponent<Button>();
      closeInventoryPanel.onClick.RemoveAllListeners();
      closeInventoryPanel.onClick.AddListener(() =>
      {
         inventoryPanel.gameObject.SetActive(false);
         decisionPanel.gameObject.SetActive(true);
         SetDecisionInteractable(true);
      });
   }

   // Updates the cargo tab panel to show current ship inventory
   private void SpawnRewardIcon(string itemName, int amount)
   {
      Transform slotTransform = inventoryIconContainer.Find(itemName);
      if (slotTransform == null) return;

      if (amount > 0)
      {
         slotTransform.gameObject.SetActive(true);
         TextMeshProUGUI txt = slotTransform.Find("Count").GetComponent<TextMeshProUGUI>();
         txt.text = $"x{amount}";
      }
      else
         slotTransform.gameObject.SetActive(false);
   }

   // Controls if buttons on decision panel are interactable
   public void SetDecisionInteractable(bool isInteractable)
   {
      CanvasGroup cg = decisionPanel.GetComponent<CanvasGroup>();
      if (cg != null)
         cg.interactable = isInteractable;
      else
         Debug.LogWarning("DecisionPanel is missing a canvas group component");
   }

   // Closes the exploration panel
   private void CloseExplorationPanel()
   {
      panelManager.ClosePanel(explorePanel.gameObject);
      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(true);
      PopUpManager.Instance.EnablePlayerInput();
   }

   // Closes the info panel
   private void CloseInfoPanel()
   {
      panelManager.ClosePanel(infoPanel.gameObject);
      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(true);
      PopUpManager.Instance.EnablePlayerInput();
   }

   // Closes the upgrade panel
   private void CloseUpgradePanel()
   {
      panelManager.ClosePanel(upgradePanel.gameObject);
      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(true);
      PopUpManager.Instance.EnablePlayerInput();
   }

   // Closes the decision panel
   public void CloseDecisionPanel()
   {
      panelManager.ClosePanel(decisionPanel.gameObject);
      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(true);
   }

   // Changes exploration building sprite based on current level
   private void UpdateExplorationSprites()
   {
      if (shipManager == null) return;

      // Calculate index based on current ship level (Level 1 = Index 0)
      int index = shipManager.ShipLevel - 1;

      if (buildingSpriteRenderer != null && index >= 0 && index < explorationLevelSprites.Count)
      {
         buildingSpriteRenderer.sprite = explorationLevelSprites[index];
         Debug.Log($"Exploration Visuals Updated to Level {shipManager.ShipLevel}");
      }
   }

   // Returns the cost of the next exploraiton unit upgrade
   private int GetUpgradeCost()
   {
      if (shipManager.ShipLevel == 1) return LEVEL2_PEARL_COST;
      if (shipManager.ShipLevel == 2) return LEVEL3_PEARL_COST;
      return 0;
   }

   // Syncs the level text label with the current level
   private void UpdateLevel()
   {
      if (explorationLevelText != null && shipManager != null)
         explorationLevelText.text = "Level " + shipManager.ShipLevel.ToString();
   }
}