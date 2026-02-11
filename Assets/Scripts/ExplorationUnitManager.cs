using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

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

   //
   private void Awake()
   {
      /* Verify all panels are assigned and disable them at startup                              */
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
     // TurnManager.OnTurnEnded += HandleNewTurn;
      ShipManager.OnShipDeath += HandleExplorationDone;
   }

   //
   private void OnDisable()
   {
     // TurnManager.OnTurnEnded -= HandleNewTurn;
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

   public void StartExploration()
   {
      isExploring = true;
      nextTurnDestination = MapManager.Instance.startingNode.nextNode;
      CloseExplorationPanel();
   }

   public void ConfirmUpgrade()
   {
      //if (inventory has enough resources to upgrade)
      //{
      //   spend resources needed to upgrade
      shipManager.UpgradeShip();
      upgradePanel.gameObject.SetActive(false);

   }

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

   private void ProcessDecision(EventChoice choice, MapNode currentNode)
   {
      ShipManager.RoundResults results = shipManager.ApplyEventResult(choice);
      if (results.goldChanged != 0 || results.oreChanged != 0 || results.healthChanged != 0 || results.fuelChanged != 0)
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
         decisionPanel.gameObject.SetActive(true);

         Button inventoryButton = decisionPanel.Find("ShipInventory").GetComponent<Button>();
         inventoryButton.onClick.RemoveAllListeners();
         inventoryButton.onClick.AddListener(() => ShowInventoryPanel());

         Button returnShip = decisionPanel.Find("ReturnButton").GetComponent<Button>();
         returnShip.onClick.RemoveAllListeners();
         returnShip.onClick.AddListener(() => shipManager.OpenConfirmReturnPanel());

         Button choice1 = decisionPanel.Find("Choice1").GetComponent<Button>();
         Button choice2 = decisionPanel.Find("Choice2").GetComponent<Button>();
         choice1.onClick.RemoveAllListeners();
         choice2.onClick.RemoveAllListeners();

         if(current.type == MapNode.NodeType.Directional)
         {
            eventController.scenarioText.text = current.navigationStory;
            eventController.choiceAText.text = current.choiceAText;
            eventController.choiceBText.text = current.choiceBText;

            choice1.onClick.AddListener(() => 
            {
               nextTurnDestination = current.pathA;
               if(!CheckForDepthIncrease(nextTurnDestination))
                  CloseDecisionPanel();
            });
            choice2.onClick.AddListener(() =>
            {
               nextTurnDestination = current.pathB;
               if (!CheckForDepthIncrease(nextTurnDestination))
                  CloseDecisionPanel();
            });
         }
         else
         {
            ExploreEvents randomEvent = eventDatabase.GetRandomEvent(current.nodeDepth);

            if(randomEvent != null)
            {
               eventController.SetEventPanel(randomEvent);
               choice1.onClick.AddListener(() => ProcessDecision(randomEvent.choiceA, current));
               choice2.onClick.AddListener(() => ProcessDecision(randomEvent.choiceB, current));
            }
         }
      }
   }

   public void HandleExplorationDone()
   {
      isExploring = false;
      nextTurnDestination = null;
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

   private void ShowResultsPanel(ShipManager.RoundResults results, MapNode currentNode)
   {
      decisionResultsPanel.gameObject.SetActive(true);
      string resultsText = "";

      if(results.goldChanged != 0)
      {
         string sign = results.goldChanged > 0 ? "+" : "";
         resultsText += $"Gold: {sign}{results.goldChanged}\n";
      }

      if(results.oreChanged != 0)
      {
         string sign = results.oreChanged > 0 ? "+" : "";
         resultsText += $"Ore: {sign}{results.oreChanged}\n";
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

   private void ShowInventoryPanel()
   {
      inventoryPanel.gameObject.SetActive(true);
      Button closeInventoryPanel = inventoryPanel.Find("ClosePanelButton").GetComponent<Button>();
      closeInventoryPanel.onClick.RemoveAllListeners();
      closeInventoryPanel.onClick.AddListener(() => inventoryPanel.gameObject.SetActive(false));

      string currentInventory = "";

      if (shipManager.GetGold() > 0)
         currentInventory += $"Gold: {shipManager.GetGold()}\n";
      if (shipManager.GetOre() > 0)
         currentInventory += $"Ore: {shipManager.GetOre()}\n";
      if (shipManager.GetHarpoon() > 0)
         currentInventory += $"Harpoons: {shipManager.GetHarpoon()}\n";
      if (shipManager.GetArtifact() > 0)
         currentInventory += $"Artifacts: {shipManager.GetArtifact()}";

      shipInventory.text = currentInventory;
   }

   //
   private void CloseExplorationPanel()
   {
      explorePanel.gameObject.SetActive(false);
   }

   //
   private void CloseInfoPanel()
   {
      infoPanel.gameObject.SetActive(false);
   }

   //
   private void CloseUpgradePanel()
   {
      upgradePanel.gameObject.SetActive(false);
   }

   //
   public void CloseDecisionPanel()
   {
      decisionPanel.gameObject.SetActive(false);
   }
}