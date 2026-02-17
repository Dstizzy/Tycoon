using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ExplorationUnitManager : MonoBehaviour
{
   [SerializeField] private EventDatabase eventDatabase;
   [SerializeField] private EventUIController eventController;
   [SerializeField] private ShipManager shipManager;
   [SerializeField] private Transform explorePanel;
   [SerializeField] private Transform upgradePanel;
   [SerializeField] private Transform infoPanel;
   [SerializeField] private Transform decisionPanel;
   [SerializeField] private Transform decisionResultsPanel;
   [SerializeField] private Transform newDepthPanel;
   [SerializeField] private Transform inventoryPanel;
   [SerializeField] private TextMeshProUGUI decisionResults;
   [SerializeField] private TextMeshProUGUI shipInventory;
   [SerializeField] private TextMeshProUGUI depthWarningText;
   [SerializeField] private GameObject exploreShipIcon;

   public int lastProcessedTurn = -1;
   private MapNode nextTurnDestination;

   const int EXPLORE_BUTTON = 1;
   const int INFO_BUTTON = 2;
   const int UPGRADE_BUTTON = 3;

   public bool isExploring = false;
   private bool isWaiting = false;

   //
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

   //
   private void OnEnable()
   {
      ShipManager.OnShipDeath += HandleExplorationDone;
   }

   //
   private void OnDisable()
   {
      ShipManager.OnShipDeath -= HandleExplorationDone;
   }

   private void Update()
   {
      if(TurnManager.Instance != null)
         if(TurnManager.Instance.currentTurn > lastProcessedTurn)
         {
            lastProcessedTurn = TurnManager.Instance.currentTurn;
            HandleNewTurn();
         }
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
               exploreButton.interactable = !isExploring;
               if (!isExploring)
                  exploreButton.onClick.AddListener(() => StartExploration());
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
               yesButton.interactable = !isExploring;
               if(!isExploring)
                  yesButton.onClick.AddListener(() => ConfirmUpgrade());
            }
            upgradePanel.transform.Find("CancelButton").GetComponent<Button>().onClick.AddListener(() => CloseUpgradePanel());
            break;
         default:
            Debug.Log("Building Panel: Unknown button ID.");
            break;
      }
   }

   // starts an exploration
   public void StartExploration()
   {
      isExploring = true;
      if (MapManager.Instance.startingNode != null)
      { 
         nextTurnDestination = MapManager.Instance.startingNode.nextNode;

      }
      CloseExplorationPanel();
   }

   //
   public void ConfirmUpgrade()
   {
      //if (inventory has enough resources to upgrade)
      //{
      //   spend resources needed to upgrade
      shipManager.UpgradeShip();
      upgradePanel.gameObject.SetActive(false);

   }

   // checks if exploration is entering a new depth tier when ship is not currently at safe level
   private bool CheckForDepthIncrease(MapNode targetNode)
   {
      if (targetNode != null && targetNode.nodeDepth > shipManager.GetDepth() && shipManager.shipLevel < targetNode.nodeDepth)
      {
         newDepthPanel.gameObject.SetActive(true);

         int predictedDamage = shipManager.GetDamage(targetNode.nodeDepth);
         if (depthWarningText != null)
            depthWarningText.text = $"Ship entering new depth.\nPressure will exceed hull rating.\nTaking {predictedDamage} health per turn";

         Button sendHome = newDepthPanel.Find("Return").GetComponent<Button>();
         Button keepGoing = newDepthPanel.Find("KeepGoing").GetComponent<Button>();

         sendHome.onClick.RemoveAllListeners();
         keepGoing.onClick.RemoveAllListeners();

         sendHome.onClick.AddListener(() =>
         {
            nextTurnDestination = null;
            newDepthPanel.gameObject.SetActive(false);
            shipManager.FinishExploration();
         });
         keepGoing.onClick.AddListener(() =>
         {
            newDepthPanel.gameObject.SetActive(false);
            CloseDecisionPanel();
         });
         return true;
      }
      return false;
   }

   //
   private void SetupButtons(string textA, UnityAction actionA,
                          string textB, UnityAction actionB,
                          string textC, UnityAction actionC)
   {
      Transform container = decisionPanel.Find("ButtonContainer");

      Button button1 = container.Find("Choice1").GetComponent<Button>();
      if (button1 != null)
      {
         button1.gameObject.SetActive(true);
         button1.GetComponentInChildren<TextMeshProUGUI>().text = textA;
         button1.onClick.RemoveAllListeners();
         if (actionA != null)
            button1.onClick.AddListener(actionA);
      }

      Button button2 = container.Find("Choice2").GetComponent<Button>();
      if (button2 != null)
      {
         if (!string.IsNullOrEmpty(textB))
         {
            button2.gameObject.SetActive(true);
            button2.GetComponentInChildren<TextMeshProUGUI>().text = textB;
            button2.onClick.RemoveAllListeners();
            if (actionB != null)
               button2.onClick.AddListener(actionB);
         }
         else
            button2.gameObject.SetActive(false);
      }

      Button button3 = container.Find("Choice3").GetComponent<Button>();
      if (button3 != null)
      {
         if (!string.IsNullOrEmpty(textC))
         {
            button3.gameObject.SetActive(true);
            button3.GetComponentInChildren<TextMeshProUGUI>().text = textC;
            button3.onClick.RemoveAllListeners();
            if (actionC != null)
               button3.onClick.AddListener(actionC);
         }
         else
            button3.gameObject.SetActive(false);
      }
   }

   // 
   private void ProcessDecision(EventChoice choice, MapNode currentNode)
   {
      ShipManager.RoundResults results = shipManager.ApplyEventResult(choice);

      if (choice.waitTurn)
         isWaiting = true;
      if (results.pearlChanged != 0 || results.oreChanged != 0 || results.healthChanged != 0 || results.fuelChanged != 0 || results.crystalChanged != 0)
         ShowResultsPanel(results, currentNode);
      else
      {
         if (currentNode != null)
         {
            nextTurnDestination = currentNode.nextNode;
            if(!CheckForDepthIncrease(nextTurnDestination))
               CloseDecisionPanel();
         }
         else
            CloseDecisionPanel();
      }
   }

   //
   public void HandleNewTurn()
   {
      if (isExploring)
      {
         // handle losing a turn
         if(isWaiting)
         {
            shipManager.NewTurn();
            if (!isExploring) return;
            isWaiting = false;
            return;
         }

         MapNode nextNode = nextTurnDestination;
         if (nextNode != null)
         {
            MapManager.Instance.MoveToNode(nextNode);
            nextTurnDestination = null;
         }

         shipManager.NewTurn();
         if (!isExploring)
            return;

         MapNode current = MapManager.Instance.currentNode;

         // check if current node is one of the end of map nodes
         if(current.isFinalNode)
         {
            HandleFinalNode(current);
            return;
         }

         decisionPanel.gameObject.SetActive(true);

         // set up inventory button on decision panel
         Button inventoryButton = decisionPanel.Find("ShipInventory").GetComponent<Button>();
         inventoryButton.onClick.RemoveAllListeners();
         inventoryButton.onClick.AddListener(() => ShowInventoryPanel());

         // set up return ship button on decision panel
         Button returnShip = decisionPanel.Find("ReturnButton").GetComponent<Button>();
         returnShip.onClick.RemoveAllListeners();
         returnShip.onClick.AddListener(() => shipManager.OpenConfirmReturnPanel());

         // handle directional decision
         if(current.type == MapNode.NodeType.Directional)
         {
            eventController.scenarioText.text = current.navigationStory;

            SetupButtons(
               current.choiceAText, () => { nextTurnDestination = current.pathA; if (!CheckForDepthIncrease(nextTurnDestination)) CloseDecisionPanel(); },
               current.choiceBText, () => { nextTurnDestination = current.pathB; if (!CheckForDepthIncrease(nextTurnDestination)) CloseDecisionPanel(); },
               current.choiceCText, () => { nextTurnDestination = current.pathC; if (!CheckForDepthIncrease(nextTurnDestination)) CloseDecisionPanel(); }
            );
         }
         // handle event decision
         else
         {
            ExploreEvents randomEvent = eventDatabase.GetRandomEvent(current.nodeDepth);

            if(randomEvent != null)
            {
               eventController.SetEventPanel(randomEvent);
               string textB = !string.IsNullOrEmpty(randomEvent.choiceB.buttonText) ? randomEvent.choiceB.buttonText : null;
               SetupButtons(
                  randomEvent.choiceA.buttonText, () => ProcessDecision(randomEvent.choiceA, current),
                  textB, () => ProcessDecision(randomEvent.choiceB, current),
                  null, null
               );
            }
         }
      }
   }

   // tells game explorationis done and resets the next turn node
   public void HandleExplorationDone()
   {
      isExploring = false;
      nextTurnDestination = null;
   }

   //
   public void HandleFinalNode(MapNode current)
   {
      bool isWinner = false;

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
      explorePanel.gameObject.SetActive(true);
   }

   //
   private void ShowInfoPanel()
   {
      infoPanel.gameObject.SetActive(true);
   }

   //
   private void ShowUpgradePanel()
   {
      upgradePanel.gameObject.SetActive(true);
   }

   // shows the event results panel with the all results from an event
   private void ShowResultsPanel(ShipManager.RoundResults results, MapNode currentNode)
   {
      decisionResultsPanel.gameObject.SetActive(true);
      string resultsText = "";

      // check to see if event resulted in any ship or inventory changes, and show changes on panel
      if(results.pearlChanged != 0)
      {
         string sign = results.pearlChanged > 0 ? "+" : "";
         resultsText += $"Pearl: {sign}{results.pearlChanged}\n";
      }
      if(results.oreChanged != 0)
      {
         string sign = results.oreChanged > 0 ? "+" : "";
         resultsText += $"Ore: {sign}{results.oreChanged}\n";
      }
      if(results.crystalChanged != 0)
      {
         string sign = results.crystalChanged > 0 ? "+" : "";
         resultsText += $"Crystal: {sign}{results.crystalChanged}\n";
      }
      if(results.healthChanged != 0)
      {
         string sign = results.healthChanged > 0 ? "+" : "";
         resultsText += $"Health: {sign}{results.healthChanged}\n";
      }
      if(results.fuelChanged != 0)
      {
         string sign = results.fuelChanged > 0 ? "+" : "";
         resultsText += $"Fuel: {sign}{results.fuelChanged}";
      }

      decisionResults.text = resultsText;

      // set up confirm button for results panel
      Button continueButton = decisionResultsPanel.transform.Find("ConfirmButton").GetComponent<Button>();
      continueButton.onClick.RemoveAllListeners();
      continueButton.onClick.AddListener(() =>
      {
         decisionResultsPanel.gameObject.SetActive(false);

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

   // shows the inventory panel with ship's current inventory
   private void ShowInventoryPanel()
   {
      inventoryPanel.gameObject.SetActive(true);
      Button closeInventoryPanel = inventoryPanel.Find("ClosePanelButton").GetComponent<Button>();
      closeInventoryPanel.onClick.RemoveAllListeners();
      closeInventoryPanel.onClick.AddListener(() => inventoryPanel.gameObject.SetActive(false));

      string currentInventory = "";

      if (shipManager.GetPearl() > 0)
         currentInventory += $"Pearl: {shipManager.GetPearl()}\n";
      if (shipManager.GetOre() > 0)
         currentInventory += $"Ore: {shipManager.GetOre()}\n";
      if (shipManager.GetCrystal() > 0)
         currentInventory += $"Cystal: {shipManager.GetCrystal()}\n";
      if (shipManager.GetHarpoon() > 0)
         currentInventory += $"Harpoons: {shipManager.GetHarpoon()}\n";

      shipInventory.text = currentInventory;
   }

   // closes the exploration panel
   private void CloseExplorationPanel()
   {
      explorePanel.gameObject.SetActive(false);
   }

   // closes the info panel
   private void CloseInfoPanel()
   {
      infoPanel.gameObject.SetActive(false);
   }

   // closes the upgrade panel
   private void CloseUpgradePanel()
   {
      upgradePanel.gameObject.SetActive(false);
   }

   // closes the decision panel
   public void CloseDecisionPanel()
   {
      decisionPanel.gameObject.SetActive(false);
   }
}