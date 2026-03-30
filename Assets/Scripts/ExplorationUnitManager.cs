using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ExplorationUnitManager : MonoBehaviour
{
   public PanelManager panelManager;
   [SerializeField] private EventDatabase eventDatabase;       // Holds all random events that can occur on nodes
   [SerializeField] private EventUIController eventController; // Manages UI of current event
   [SerializeField] private ShipManager shipManager;           // Handles ship's health, fuel, and inventory

   [SerializeField] private Transform explorePanel; // Panel to start an expedition
   [SerializeField] private Transform upgradePanel; // Panel to upgrade exploration unit
   [SerializeField] private Transform infoPanel;    // Panel to show info
   [SerializeField] private Transform decisionPanel; // Pop-up panel during exploration where user makes choices
   [SerializeField] private Transform decisionResultsPanel; // Shows results from a event decision
   [SerializeField] private Transform newDepthPanel; // Warns that a dangerous depth tier is being entered
   [SerializeField] private Transform inventoryPanel; // Shows the ship's current inventory

   [SerializeField] private TextMeshProUGUI decisionResults; // Describes an event choice's results
   [SerializeField] private TextMeshProUGUI shipInventory; // Lists the ship's current inventory
   [SerializeField] private TextMeshProUGUI depthWarningText; // Displays predicted depth damage

   [Header("Exploration Visuals")]
   [SerializeField] private SpriteRenderer buildingSpriteRenderer;
   [SerializeField] private List<Sprite> explorationLevelSprites;
   [SerializeField] private TextMeshProUGUI explorationLevelText;

   private MapNode nextTurnDestination; // Map node ship is scheduled to move to next turn

   // ID constants for the base menu buttons
   const int EXPLORE_BUTTON = 1;
   const int INFO_BUTTON = 2;
   const int UPGRADE_BUTTON = 3;
   const int LEVEL2_PEARL_COST = 200;
   const int LEVEL3_PEARL_COST = 500;
   const int MAX_SHIP_LEVEL = 3;
   public bool isExploring = false; // Determines if exploration is currently ongoing
   private bool isWaiting = false;  // Triggered when an event causes user to lose an exploration turn
   private int lastProcessedTurn = 0;

   public static ExplorationUnitManager Instance { get; private set; }
   private void Awake()
   {
      // Verify all panels are assigned and disable them at startup
      if (infoPanel == null)
         Debug.LogError("Info Panel is not assigned in the Inspector!");
      else
         infoPanel.gameObject.SetActive(false);

      if (explorePanel == null)
         Debug.LogError("Explore Panel is not assigned");
      else
         explorePanel.gameObject.SetActive(false);
   }
   private void Start()
   {
      UpdateExplorationSprites();
      if (explorationLevelText != null && shipManager != null)
      {
         explorationLevelText.text = "Level " + shipManager.shipLevel.ToString();
      }
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
                  Debug.Log("Exploration Start button clicked");
               }
            }
            explorePanel.transform.Find("ExitButton").GetComponent<Button>().onClick.AddListener(() => CloseExplorationPanel());
            break;
         case INFO_BUTTON:
            ShowInfoPanel();
            infoPanel.transform.Find("ExitButton").GetComponent<Button>().onClick.AddListener(() => CloseInfoPanel());
            break;
         case UPGRADE_BUTTON:
            ShowUpgradePanel();
            Button yesButton = upgradePanel.Find("YesButton").GetComponent<Button>();
            if (yesButton != null)
            {
               yesButton.onClick.RemoveAllListeners();
               yesButton.onClick.AddListener(() => ConfirmUpgrade());
            }
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
      lastProcessedTurn = TurnManager.Instance.currentTurn;
      if (MapManager.Instance.startingNode != null)
      {
         MapManager.Instance.MoveToNode(MapManager.Instance.startingNode);
         nextTurnDestination = null;
      }
      CloseExplorationPanel();
      StartCoroutine(InitializeFirstTurnRoutine());
   }

   //
   private System.Collections.IEnumerator InitializeFirstTurnRoutine()
   {
      yield return new WaitForEndOfFrame();
      HandleNewTurn();
   }

   //
   public void ConfirmUpgrade()
   {
      int upgradeCost = GetUpgradeCost();

      if (shipManager.shipLevel < MAX_SHIP_LEVEL && InventoryManager.Instance.TrySpendPearl(upgradeCost))
      {
         shipManager.UpgradeShip();
         UpdateExplorationSprites();

         if (explorationLevelText != null)
            explorationLevelText.text = "Level " + shipManager.shipLevel.ToString();

         if (shipManager.shipLevel == MAX_SHIP_LEVEL)
         {
            if (InventoryManager.Instance.ExplorationUnitUpgradeIcon != null)
            {
               InventoryManager.Instance.ExplorationUnitUpgradeIcon.gameObject.SetActive(false);
            }
            else
            {
               Debug.LogWarning("ExplorationUnitUpgradeIcon is not assigned in the InventoryManager!");
            }
         }

         Debug.Log($"Exploration Unit upgraded to level {shipManager.shipLevel}!");
         if (TickerSystem.Instance != null)
         {
            TickerSystem.Instance.ShowTicker($"Exploration Unit upgraded to level {shipManager.shipLevel}!", Color.green, TickerSystem.MessageTypes.ResultMessage);
         }

         CloseUpgradePanel();
         PopUpManager.Instance.EnablePlayerInput();
      }
      else
      {
         Debug.Log("Not enough pearls to upgrade!");
      }
   }

   // Checks if exploration is entering a new depth tier when ship is not currently at safe level
   private bool CheckForDepthIncrease(MapNode targetNode)
   {
      // If target depth is greater than current depth and ship level is lower whan target depth level
      if (targetNode != null && targetNode.nodeDepth > shipManager.GetDepth() && shipManager.shipLevel < targetNode.nodeDepth)
      {
         newDepthPanel.gameObject.SetActive(true);

         // Calculates how much damage it will take
         int predictedDamage = shipManager.GetDamage(targetNode.nodeDepth);
         if (depthWarningText != null)
            depthWarningText.text = $"Ship entering new depth.\nPressure will exceed hull rating.\nTaking {predictedDamage} health per turn";

         Button sendHome = newDepthPanel.Find("Return").GetComponent<Button>();
         Button keepGoing = newDepthPanel.Find("KeepGoing").GetComponent<Button>();

         sendHome.onClick.RemoveAllListeners();
         keepGoing.onClick.RemoveAllListeners();

         // Option 1: Send ship back to the base
         sendHome.onClick.AddListener(() =>
         {
            nextTurnDestination = null;
            newDepthPanel.gameObject.SetActive(false);
            shipManager.FinishExploration();
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

   // Configures up to 3 choice buttons on the decision panel
   private void SetupButtons(string textA, UnityAction actionA, bool interactableA,
                             string textB, UnityAction actionB, bool interactableB,
                             string textC, UnityAction actionC, bool interactableC)
   {
      Debug.Log("Sets up buttons");
      Transform container = decisionPanel.Find("ButtonContainer");

      ExploreEvents currentEvent = eventController.currentEvent;

      // Sets up choice 1 (always exists)
      Button button1 = container.Find("Choice1").GetComponent<Button>();
      if (button1 != null)
      {
         Debug.Log("Choice1");
         button1.gameObject.SetActive(true);
         button1.GetComponentInChildren<TextMeshProUGUI>().text = textA;
         button1.onClick.RemoveAllListeners();

         bool isLockedA = currentEvent != null && currentEvent.choiceA.requiresLabTier && !shipManager.isTier2Unlocked;
         button1.interactable = interactableA && !isLockedA;
         if (button1.interactable && actionA != null)
            button1.onClick.AddListener(actionA);
      }
      // Sets up choice 2 if there is text for it
      Button button2 = container.Find("Choice2").GetComponent<Button>();
      if (button2 != null)
      {
         if (!string.IsNullOrEmpty(textB))
         {
            button2.gameObject.SetActive(true);
            button2.GetComponentInChildren<TextMeshProUGUI>().text = textB;
            button2.onClick.RemoveAllListeners();

            bool isLockedB = currentEvent != null && currentEvent.choiceB.requiresLabTier && !shipManager.isTier2Unlocked;
            button2.interactable = interactableB && !isLockedB;

            if (button2.interactable && actionB != null)
               button2.onClick.AddListener(actionB);
         }
         else
            button2.gameObject.SetActive(false);
      }
      // Sets up choice 3 if there is text for it
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

   // Processes the user's choice on an event node
   private void ProcessDecision(EventChoice choice, MapNode currentNode)
   {
      // Send choice to the ShipManager to calculate and apply all damage and loot
      ShipManager.RoundResults results = shipManager.ApplyEventResult(choice);

      // If event results in lost turn, flags to skip the next turn
      if (choice.waitTurn)
         isWaiting = true;

      // If anything changed, shows the results of the decision
      if (results.HasChanges())
         ShowResultsPanel(results, currentNode, choice.resultText);
      // Otherwise, move on to the next turn
      else
      {
         if (currentNode != null)
         {
            nextTurnDestination = currentNode.nextNode;
            if (!CheckForDepthIncrease(nextTurnDestination))
               CloseDecisionPanel();
         }
         else
            CloseDecisionPanel();
      }
   }

   // Handles map movements and spawning new events
   public void HandleNewTurn()
   {
      if (!isExploring) return;
      Debug.Log("Processing HandleNewTurn...");
      // Handle if user lost a turn
      if (isWaiting)
      {
         shipManager.NewTurn();
         if (!isExploring) return;
         isWaiting = false;
         return;
      }
      // If destination exists, move the ship on the map
      if (nextTurnDestination != null)
      {
         MapManager.Instance.MoveToNode(nextTurnDestination);
         nextTurnDestination = null;
      }
      // Burn fuel and depth damage for current turn
      shipManager.NewTurn();
      if (!isExploring) return;

      // Determine node type landed on
      MapNode current = MapManager.Instance.currentNode;

      if(current == null)
      {
         Debug.LogError("ExplorationManager: Current Node is NULL");
         return;
      }   

      // Trigger finale and stop if current node is an end node
      if (current.isFinalNode)
      {
         HandleFinalNode(current);
         return;
      }
      // Open the decision panel UI
      decisionPanel.gameObject.SetActive(true);

      // Set up inventory button on decisionPanel
      Button inventoryButton = decisionPanel.Find("CargoTab").GetComponent<Button>();
      inventoryButton.onClick.RemoveAllListeners();
      inventoryButton.onClick.AddListener(() =>
      {
         ShowInventoryPanel();
         CloseDecisionPanel();
      });

      // Set up return ship button on decisionPanel
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
         // Pull random event from database based on current depth
         ExploreEvents randomEvent = eventDatabase.GetRandomEvent(current.nodeDepth);

         if (randomEvent != null)
         {
            eventController.SetEventPanel(randomEvent);
            // 1. Check if they can afford it AND if they meet the Tier 2 requirement
            bool canAffordA = shipManager.CanAfford(randomEvent.choiceA) && (!randomEvent.choiceA.requiresLabTier || shipManager.isTier2Unlocked);
            bool canAffordB = shipManager.CanAfford(randomEvent.choiceB) && (!randomEvent.choiceB.requiresLabTier || shipManager.isTier2Unlocked);

            // 2. Format the text (Add [LOCKED] if they don't have the upgrade)
            string textA = randomEvent.choiceA.buttonText;
            if (randomEvent.choiceA.requiresLabTier && !shipManager.isTier2Unlocked)
               textA = "[LOCKED] " + textA;

            string textB = !string.IsNullOrEmpty(randomEvent.choiceB.buttonText) ? randomEvent.choiceB.buttonText : null;
            if (textB != null && randomEvent.choiceB.requiresLabTier && !shipManager.isTier2Unlocked)
               textB = "[LOCKED] " + textB;

            // 3. Set up the event choices
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
         decisionResultsPanel.gameObject.SetActive(true);
         Button confirmEnd = decisionResultsPanel.transform.Find("ConfirmButton").GetComponent<Button>();
         confirmEnd.onClick.RemoveAllListeners();
         confirmEnd.onClick.AddListener(() =>
         {
            shipManager.FinishExploration();
            decisionResultsPanel.gameObject.SetActive(false);
         });
      }
      else
      {
         EventChoice consolationPrize = new EventChoice();
         consolationPrize.pearlChange = 200;
         consolationPrize.oreChange = 200;
         ProcessDecision(consolationPrize, null);

         decisionResults.text = "DEAD END\n\nThe vessel piece is not here, but the chest is not empty!\nPearl: +200\nOre: +200";
      }
   }

   //
   private void ShowExplorationPanel()
   {
      panelManager.OpenPanel(explorePanel.gameObject);

      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(false);
   }

   //
   private void ShowInfoPanel()
   {
      panelManager.OpenPanel(infoPanel.gameObject);

      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(false);
   }

   //
   private void ShowUpgradePanel()
   {
      panelManager.OpenPanel(upgradePanel.gameObject);

      int upgradeCost = GetUpgradeCost();
      Transform mainTextTransform = upgradePanel.Find("UpgradePanelText");
      TextMeshProUGUI upgradeText = mainTextTransform != null ? mainTextTransform.GetComponent<TextMeshProUGUI>() : upgradePanel.GetComponentInChildren<TextMeshProUGUI>();
      Button yesButton = upgradePanel.Find("YesButton").GetComponent<Button>();

      Transform pearlTextObj = upgradePanel.Find("PearlCostText");
      if (pearlTextObj != null)
      {
         pearlTextObj.gameObject.SetActive(true);
         pearlTextObj.GetComponent<TextMeshProUGUI>().text = upgradeCost.ToString();
      }

      if (shipManager.shipLevel < MAX_SHIP_LEVEL)
      {
         if (upgradeText != null)
         {
            int targetLevel = shipManager.shipLevel + 1;
            upgradeText.text = $"Would you like to upgrade to lvl {targetLevel}?";
         }
         yesButton.gameObject.SetActive(true);
         yesButton.interactable = !isExploring && (InventoryManager.Instance.pearlCount >= upgradeCost);
      }
      else
      {
         if (upgradeText != null)
            upgradeText.text = "Max Level Reached!";

         if (pearlTextObj != null) pearlTextObj.gameObject.SetActive(false);
         Transform imagesObj = upgradePanel.Find("UpgradePanelImages");
         if (imagesObj != null) imagesObj.gameObject.SetActive(false);

         yesButton.gameObject.SetActive(false);
      }

      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(false);
   }

   // Shows the event results panel with the all results from an event
   private void ShowResultsPanel(ShipManager.RoundResults results, MapNode currentNode, string resultMessage)
   {
      decisionResultsPanel.gameObject.SetActive(true);
      SetDecisionInteractable(false);
      string resultsText = $"{resultMessage}\n\n";

      // Checks to see if event resulted in any ship or inventory changes, and show changes on panel
      if (results.pearlChanged != 0)
      {
         string sign = results.pearlChanged > 0 ? "+" : "";
         resultsText += $"Pearl: {sign}{results.pearlChanged}\n";
      }
      if (results.oreChanged != 0)
      {
         string sign = results.oreChanged > 0 ? "+" : "";
         resultsText += $"Ore: {sign}{results.oreChanged}\n";
      }
      if (results.patchKitChanged > 0)
         resultsText += $"Patch Kit: +{results.patchKitChanged}\n";
      if (results.harpoonChanged > 0)
         resultsText += $"Harpoon: +{results.harpoonChanged}\n";
      if (results.crudeToolChanged > 0)
         resultsText += $"Crude Tool: +{results.crudeToolChanged}\n";
      if (results.pressureValveChanged > 0)
         resultsText += $"Pressure Valve: +{results.pressureValveChanged}\n";
      if (results.divingBellChanged > 0)
         resultsText += $"Diving Bell: +{results.divingBellChanged}\n";
      if (results.clockworkEngineChanged > 0)
         resultsText += $"Clockwork Engine: +{results.clockworkEngineChanged}\n";
      if (results.precisionLensChanged > 0)
         resultsText += $"Precision Lens: +{results.precisionLensChanged}\n";
      if (results.healthChanged != 0)
      {
         string sign = results.healthChanged > 0 ? "+" : "";
         resultsText += $"Health: {sign}{results.healthChanged}\n";
      }
      if (results.fuelChanged != 0)
      {
         string sign = results.fuelChanged > 0 ? "+" : "";
         resultsText += $"Fuel: {sign}{results.fuelChanged}";
      }

      decisionResults.text = resultsText;

      // Sets up confirm button for results panel
      Button continueButton = decisionResultsPanel.transform.Find("ConfirmButton").GetComponent<Button>();
      continueButton.onClick.RemoveAllListeners();
      continueButton.onClick.AddListener(() =>
      {
         decisionResultsPanel.gameObject.SetActive(false);
         SetDecisionInteractable(true);

         if (currentNode != null)
         {
            nextTurnDestination = currentNode.nextNode;
            if (!CheckForDepthIncrease(nextTurnDestination))
               CloseDecisionPanel();
         }
         else
            CloseDecisionPanel();
      });
   }

   // Shows the inventory panel with ship's current resources
   private void ShowInventoryPanel()
   {
      inventoryPanel.gameObject.SetActive(true);
      SetDecisionInteractable(false);
      Button closeInventoryPanel = inventoryPanel.Find("ChartsTab").GetComponent<Button>();
      closeInventoryPanel.onClick.RemoveAllListeners();
      closeInventoryPanel.onClick.AddListener(() =>
      {
         inventoryPanel.gameObject.SetActive(false);
         decisionPanel.gameObject.SetActive(true);
         SetDecisionInteractable(true);
      });

      string currentInventory = "";

      if (shipManager.GetPearl() > 0)
         currentInventory += $"Pearl: {shipManager.GetPearl()}\n";
      if (shipManager.GetOre() > 0)
         currentInventory += $"Ore: {shipManager.GetOre()}\n";
      if (shipManager.GetPatchKit() > 0)
         currentInventory += $"Patch Kit: {shipManager.GetPatchKit()}\n";
      if (shipManager.GetHarpoon() > 0)
         currentInventory += $"Harpoons: {shipManager.GetHarpoon()}\n";
      if (shipManager.GetCrudeTool() > 0)
         currentInventory += $"Crude Tool: {shipManager.GetCrudeTool()}\n";
      if (shipManager.GetPressureValve() > 0)
         currentInventory += $"Pressure Valve: {shipManager.GetPressureValve()}\n";
      if (shipManager.GetDivingBell() > 0)
         currentInventory += $"Diving Bell: {shipManager.GetDivingBell()}\n";
      if (shipManager.GetClockworkEngine() > 0)
         currentInventory += $"Clockwork Engine: {shipManager.GetClockworkEngine()}\n";
      if (shipManager.GetPrecisionLens() > 0)
         currentInventory += $"Precision Lens: {shipManager.GetPrecisionLens()}\n";

      shipInventory.text = currentInventory;
   }

   public void SetDecisionInteractable(bool isInteractable)
   {
      CanvasGroup cg = decisionPanel.GetComponent<CanvasGroup>();
      if (cg != null)
         cg.interactable = isInteractable;
      else
         Debug.LogWarning("DecisionPanel is missing a canvas group component");
   }

   // closes the exploration panel
   private void CloseExplorationPanel()
   {
      panelManager.ClosePanel(explorePanel.gameObject);

      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(true);
      PopUpManager.Instance.EnablePlayerInput();
   }

   // closes the info panel
   private void CloseInfoPanel()
   {
      panelManager.ClosePanel(infoPanel.gameObject);

      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(true);
      PopUpManager.Instance.EnablePlayerInput();
   }

   // closes the upgrade panel
   private void CloseUpgradePanel()
   {
      panelManager.ClosePanel(upgradePanel.gameObject);
      PopUpManager.Instance.EnablePlayerInput();

      if (MainUIManager.mainUI != null)
         MainUIManager.mainUI.SetMainButtonsInteractable(true);
   }

   // closes the decision panel
   public void CloseDecisionPanel()
   {
      Debug.Log("Closing decision panel");
      decisionPanel.gameObject.SetActive(false);
   }

   private void UpdateExplorationSprites()
   {
      if (shipManager == null) return;

      // Calculate index based on current ship level (Level 1 = Index 0)
      int index = shipManager.shipLevel - 1;

      if (buildingSpriteRenderer != null && index >= 0 && index < explorationLevelSprites.Count)
      {
         buildingSpriteRenderer.sprite = explorationLevelSprites[index];
         Debug.Log($"Exploration Visuals Updated to Level {shipManager.shipLevel}");
      }
   }

   private int GetUpgradeCost()
   {
      if (shipManager.shipLevel == 1) return LEVEL2_PEARL_COST;
      if (shipManager.shipLevel == 2) return LEVEL3_PEARL_COST;
      return 0;
   }
}