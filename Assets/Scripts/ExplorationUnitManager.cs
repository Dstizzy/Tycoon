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
   [SerializeField] private GameObject vesselIcon;

   const int EXPLORE_BUTTON = 1;
   const int INFO_BUTTON = 2;
   const int UPGRADE_BUTTON = 3;

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
      TurnManager.OnTurnEnded += HandleNewTurn;
   }

   //
   private void OnDisable()
   {
      TurnManager.OnTurnEnded -= HandleNewTurn;
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
      // set isExploring boolean to true
      // activate ship icon???
   }

   public void ConfirmUpgrade()
   {
      //if (inventory has enough resources to upgrade)
      //{
      //   spend resources needed to upgrade
      shipManager.UpgradeShip();
      upgradePanel.gameObject.SetActive(false);

   }

   //
   public void HandleNewTurn()
   {
      decisionPanel.GameObject().SetActive(true);
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
}
