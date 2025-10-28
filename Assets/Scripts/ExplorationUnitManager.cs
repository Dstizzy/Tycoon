using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ExplorationUnitManager : MonoBehaviour
{
   [SerializeField] private Transform explorePanel;
   [SerializeField] private Transform infoPanel;
   [SerializeField] private Transform upgradePanel;
   public TextMeshProUGUI explorationUnitLevelText;

   [Header("Explore Panel Tabs")]
   [SerializeField] private GameObject levelOneTab;
   [SerializeField] private GameObject levelTwoTab;
   [SerializeField] private GameObject levelThreeTab;

   [Header("Boat Visuals")]
   [SerializeField] private GameObject levelOneBoat;
   [SerializeField] private GameObject levelTwoBoat;
   [SerializeField] private GameObject levelThreeBoat;

   const int EXPLORE_BUTTON       = 1;
   const int INFO_BUTTON          = 2;
   const int UPGRADE_BUTTON       = 3;
   const int STARTING_LEVEL       = 1;
   const int ENDING_LEVEL         = 3;
   const int UPGRADE_COST         = 100;
   const int LEVEL_1_TAB_ID       = 11;
   const int LEVEL_2_TAB_ID       = 12;
   const int LEVEL_3_TAB_ID       = 13;
   const int LEVEL_1_EXPLORE_COST = 200;
   const int LEVEL_2_EXPLORE_COST = 300;
   const int LEVEL_3_EXPLORE_COST = 400;
   const int EXPLORE_DURATION     = 2;

   private static int explorationUnitLevel = STARTING_LEVEL;
   private static int exploreEndTurn       = 0;

   private int activeExploreTabId = LEVEL_1_TAB_ID;

   private void Awake()
   {
      if (infoPanel == null)
         Debug.LogError("Info Panel is not assigned in the Inspector!");
      else
         infoPanel.gameObject.SetActive(false);

      if (explorePanel == null)
         Debug.LogError("Explore Panel is not assigned");
      else
         explorePanel.gameObject.SetActive(false);

      if (explorationUnitLevelText == null)
         Debug.LogError("Exploration Unit Level Text is not assigned");
      else
         explorationUnitLevelText.text = "Level " + explorationUnitLevel.ToString();
   }

   /* Determines if an Exploration is currently ongoing                                          */
   private bool IsExplorationOngoing()
   {
      return false; // PopUpManager.CurrentTurn < exploreEndTurn;
   }

   /* Determines if the user has enough currency for an action                                   */
   private bool HasEnoughCurrency(int cost)
   {
      return true; // CurrencyManager.Instance.GetPearls() >= cost;
   }

   public void RequestExplorationUnitPanel(int buttonID)
   {
      bool isExploring = IsExplorationOngoing();

      switch (buttonID)
      {
         case EXPLORE_BUTTON:
            ShowExplorationPanel();
            UpdateExploreButton(activeExploreTabId, isExploring);
            explorePanel.transform.Find("ExitButton").GetComponent<Button>().onClick.AddListener(() => CloseExplorationUnitPanel(EXPLORE_BUTTON));
            break;
         case INFO_BUTTON:
            ShowInfoPanel();
            infoPanel.transform.Find("ExitButton").GetComponent<Button>().onClick.AddListener(() => CloseExplorationUnitPanel(INFO_BUTTON));
            break;
         case UPGRADE_BUTTON:
            ShowUpgradePanel();
            Transform yesButtonTransform = upgradePanel.transform.Find("YesButton");
            Button yesButton = yesButtonTransform.GetComponent<Button>();
            if (yesButton != null)
            {
               yesButton.onClick.RemoveAllListeners();
               yesButton.onClick.AddListener(() => UpgradeExplorationUnit());

               bool canAffordUpgrade = HasEnoughCurrency(UPGRADE_COST);
               bool canUpgrade = canAffordUpgrade && !isExploring;
               bool isMaxLevel = explorationUnitLevel >= ENDING_LEVEL;
               yesButton.interactable = canUpgrade && !isMaxLevel;

               Transform yesText = yesButtonTransform.Find("Text (TMP)");
               if (isMaxLevel && yesText != null)
               {
                  var tmp = yesText.GetComponent<TextMeshProUGUI>();
                  if (tmp != null)
                     tmp.text = "Max Level";
               }
            }
            else
               Debug.LogError("YesButton not found on upgrade panel.");

            upgradePanel.transform.Find("CancelButton").GetComponent<Button>().onClick.AddListener(() => CloseExplorationUnitPanel(UPGRADE_BUTTON));
            break;
         default:
            Debug.Log("Building Panel: Unknown button ID.");
            break;
      }
   }

   /* Switche the explore tab based on the tab that has been clicked                             */
   public void switchExploreTab(int tabId)
   {
      activeExploreTabId = tabId;

      // Deactivate all three tabs
      if(levelOneTab != null) 
         levelOneTab.SetActive(false);
      if(levelTwoTab != null) 
         levelTwoTab.SetActive(false);
      if(levelThreeTab != null) 
         levelThreeTab.SetActive(false);

      // Activate the requested panel
      switch(tabId)
      {
         case LEVEL_1_TAB_ID:
            if(levelOneTab != null)
               levelOneTab.SetActive(true);
            break;
         case LEVEL_2_TAB_ID:
            if(levelTwoTab != null)
               levelTwoTab.SetActive(true);
            break;
         case LEVEL_3_TAB_ID:
            if(levelThreeTab != null)
               levelThreeTab.SetActive(true);
            break;
         default:
            Debug.LogWarning("Unknown explore tab ID requested: " + tabId);
            break;
      }
      UpdateExploreButton(tabId, IsExplorationOngoing());
   }

   /* Change the explore button based on if an exploration is currently ongoing, the user has    */
   /* enough money, or if the Exploration Unit is at a high enough level                         */
   public void UpdateExploreButton(int tabId, bool isExploring)
   {
      GameObject activeTab = null; /* The explore tab currently open                             */
      int exploreCost      = 0; /* Cost of an exploration in the current explore tab             */
      int requiredLevel    = 0; /* Level required to explore in current explore tab              */

      switch (tabId)
      {
         case LEVEL_1_TAB_ID:
            activeTab     = levelOneTab;
            exploreCost   = LEVEL_1_EXPLORE_COST;
            requiredLevel = 1;
            break;
         case LEVEL_2_TAB_ID:
            activeTab     = levelTwoTab;
            exploreCost   = LEVEL_2_EXPLORE_COST;
            requiredLevel = 2;
            break;
         case LEVEL_3_TAB_ID:
            activeTab     = levelThreeTab;
            exploreCost   = LEVEL_3_EXPLORE_COST;
            requiredLevel = 3;
            break;
         default:
            Debug.LogWarning("Invalid tab ID provided in updateExploreButton().");
            return;
      }

      if(activeTab == null)
         return;
      if(activeTab.activeSelf)
         activeTab.SetActive(true);

      Transform startButtonTransform = activeTab.transform.Find("ExploreButton");
      if(startButtonTransform == null)
      {
         Debug.LogWarning("ExploreButton child not found in the tab");
         return;
      }

      Button startButton = startButtonTransform.GetComponent<Button>();
      if(startButton == null)
      {
         Debug.LogWarning("Button component not found on ExploreButton of child tab");
         return;
      }

      startButton.onClick.RemoveAllListeners();
      bool needsUpgrade = explorationUnitLevel < requiredLevel;

      Transform costTextTransform = startButtonTransform.transform.Find("ExploreCost");
      TextMeshProUGUI costText = (costTextTransform != null) ? costTextTransform.GetComponent<TextMeshProUGUI>() : null;

      if(needsUpgrade)
      {
         startButton.onClick.RemoveAllListeners();
         startButton.onClick.AddListener(() =>
         {
            if(explorePanel != null)
               explorePanel.gameObject.SetActive(false);
            RequestExplorationUnitPanel(UPGRADE_BUTTON);
         });

         startButton.interactable = explorationUnitLevel < ENDING_LEVEL;
         if(costText != null)
         {
            costText.text = "Upgrade";
            costText.fontSize = 7.0f;
            RectTransform costTextRect = costText.GetComponent<RectTransform>();
            if(costTextRect != null)
            {
               Vector3 pos = costTextRect.anchoredPosition;
               pos.x = 0f;
               costTextRect.anchoredPosition = pos;
            }
            Transform parent = costText.transform.parent;
            if(parent != null)
               foreach(Transform sibling in parent)
                  if(sibling != costText.transform && sibling.GetComponent<Image>() != null)
                     sibling.gameObject.SetActive(false);
         }
      }
      else
      {
         startButton.onClick.AddListener(startExploration);
         bool canAffordExplore = HasEnoughCurrency(exploreCost);
         bool canExplore = canAffordExplore && !isExploring;
         startButton.interactable = canExplore;
         if(costText != null)
         {
            costText.text = exploreCost.ToString();
            costText.fontSize = 10.0f;
            RectTransform costTextRect = costText.GetComponent<RectTransform>();
            if(costTextRect != null)
            {
               Vector2 pos = costTextRect.anchoredPosition;
               pos.x = 6.0f;
               costTextRect.anchoredPosition = pos;
            }
            Transform parent = costText.transform.parent;
            if(parent != null)
               foreach(Transform sibling in parent)
                  if(sibling != costText.transform && sibling.GetComponent<Image>() != null)
                     sibling.gameObject.SetActive(true);
         }
      }
   }

   /* Starts an exploration                                                                      */
   public void startExploration()
   {
      if(IsExplorationOngoing()) 
         return;
      Debug.Log("Exploration started!");

      // add exploration duration to current turn and assign to the exploration's ending turn
      //exploreEndTurn = PopUpManager.CurrentTurn + EXPLORE_DURATION;

      //deduct exploration cost here

      HideAllBoats();
      CloseExplorationPanel();
      PopUpManager.Instance.EnablePlayerInput();
   }

   /* An exploration finishes after a certain number of turns has passed                         */
   public void FinishExploration()
   {
      exploreEndTurn = 0;
      UpdateBoatVisuals();
   }

   /* Upgrade the exploration unit to the next level                                             */
   public void UpgradeExplorationUnit()
   {
      if(explorationUnitLevel < ENDING_LEVEL)
      {
         explorationUnitLevel += 1;
         UpdateBoatVisuals();
      }
      else
      {
         Transform yesButtonTransform = upgradePanel.transform.Find("YesButton");
         if(yesButtonTransform != null)
         {
            Button yesButton = yesButtonTransform.GetComponent<Button>();
            if(yesButton != null)
               yesButton.interactable = false;
         }
      }
      explorationUnitLevelText.text = "Level " + explorationUnitLevel.ToString();

      // Close the upgrade panel after upgrading
      CloseUpgradePanel();
      PopUpManager.Instance.EnablePlayerInput();
   }

   /* Hide all exploration boats                                                                 */
   private void HideAllBoats()
   {
      if(levelOneBoat != null) 
         levelOneBoat.SetActive(false);
      if(levelTwoBoat != null) 
         levelTwoBoat.SetActive(false);
      if(levelThreeBoat != null) 
         levelThreeBoat.SetActive(false);
   }

   /* Update the exploration boats based on Exploration Unit level or if they are exploring      */
   private void UpdateBoatVisuals()
   {
      if (IsExplorationOngoing())
      {
         HideAllBoats();
         return;
      }

      if(levelOneBoat != null) 
         levelOneBoat.SetActive(explorationUnitLevel >= 1);
      if(levelTwoBoat != null) 
         levelTwoBoat.SetActive(explorationUnitLevel >= 2);
      if(levelThreeBoat != null) 
         levelThreeBoat.SetActive(explorationUnitLevel >= 3);
   }

   /* Get the level of the exploration unit                                                      */
   public int GetLevel() 
   { 
      return explorationUnitLevel;
   }

   /* Close an exploration unit panel                                                            */
   public void CloseExplorationUnitPanel(int buttonID)
   {
      switch (buttonID)
      {
         case EXPLORE_BUTTON:
            CloseExplorationPanel();
            break;
         case INFO_BUTTON:
            CloseInfoPanel();
            Debug.Log("Building Panel: Sell requested.");
            break;
         case UPGRADE_BUTTON:
            CloseUpgradePanel();
            break;
         default:
            Debug.Log("Building Panel: Unknown button ID.");
            break;
      }
      PopUpManager.Instance.EnablePlayerInput();
   }

   /* Show the explore panel                                                                     */
   private void ShowExplorationPanel()
   {
      explorePanel.gameObject.SetActive(true);
      int requiredLevel = activeExploreTabId - LEVEL_1_TAB_ID + 1;
      if(explorationUnitLevel < requiredLevel)
         switchExploreTab(LEVEL_1_TAB_ID + explorationUnitLevel - 1);
      else
         switchExploreTab(activeExploreTabId);
   }

   /* Show the exploration unit's information panel                                              */
   private void ShowInfoPanel()
   {
      infoPanel.gameObject.SetActive(true);
   }

   /* Show the exploration unit's upgrade panel                                                  */
   private void ShowUpgradePanel()
   {
      upgradePanel.gameObject.SetActive(true);
   }

   /* Close the explore panel                                                                    */
   private void CloseExplorationPanel()
   {
      explorePanel.gameObject.SetActive(false);
   }
   
   /* Close the exploration unit's information panel                                             */
   private void CloseInfoPanel()
   {
      infoPanel.gameObject.SetActive(false);
   }

   /* Close the exploration unit's upgrade panel                                                 */
   private void CloseUpgradePanel()
   {
      upgradePanel.gameObject.SetActive(false);
   }
}