using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ExplorationUnitManager : MonoBehaviour
{
   [System.Serializable]
   public class ExplorationReward
   {
      public string rewardName;                /* Name of exploration reward                     */
      [Range(0f, 1f)] public float baseChance; /* Base chance of exploration finding item        */
      [Range(0f, 1f)] public float levelBonus; /* Amount chances increase at each level upgrade  */
      public int minimumAmount;                /* Min quantity that can be found per explore     */
      public int maximumAmount;                /* Max quantity that can be found per explore     */
      public Sprite rewardIcon;                /* Icon representing the reward                   */
   }

   public TextMeshProUGUI explorationUnitLevelText; /* Displays current exploration unit level   */

   [SerializeField] private Transform  explorePanel;     /* Explore panel UI                     */
   [SerializeField] private Transform  infoPanel;        /* Info panel UI                        */
   [SerializeField] private Transform  upgradePanel;     /* Upgrade panel UI                     */

   [Header("Explore Panel Tabs")]
   [SerializeField] private GameObject levelOneTab;      /* Level one tab on explore panel       */
   [SerializeField] private GameObject levelTwoTab;      /* Level two tab on explore panel       */
   [SerializeField] private GameObject levelThreeTab;    /* Level three tab on explore panel     */

   [Header("Boat Visuals")]
   [SerializeField] private GameObject levelOneBoat;     /* Boat for level one                   */
   [SerializeField] private GameObject levelTwoBoat;     /* Boat for level two                   */
   [SerializeField] private GameObject levelThreeBoat;   /* Boat for level three                 */

   [Header("Exploration Rewards")]
   [SerializeField] private ExplorationReward[] possibleRewards;
                                                         /* Data table for possible rewards      */
   [SerializeField] private Transform  rewardsPanel;     /* UI panel that displays rewards       */
   [SerializeField] private Transform  rewardsContainer; /* Container for rewards entries        */
   [SerializeField] private GameObject rewardEntry;      /* Prefab for a single reward           */

   const int EXPLORE_BUTTON       = 1;   /* ID number of the explore button                      */
   const int INFO_BUTTON          = 2;   /* ID number of the info button                         */
   const int UPGRADE_BUTTON       = 3;   /* ID number of the upgrade button                      */
   const int STARTING_LEVEL       = 1;   /* Starting level of the exploration unit               */
   const int ENDING_LEVEL         = 3;   /* Maximum level of the exploration unit                */
   const int UPGRADE_COST         = 100; /* Cost to upgrade the exploration unit                 */
   const int LEVEL_1_TAB_ID       = 11;  /* ID number of the level 1 tab in the explore panel    */
   const int LEVEL_2_TAB_ID       = 12;  /* ID number of the level 2 tab in the explore panel    */
   const int LEVEL_3_TAB_ID       = 13;  /* ID number of the level 3 tab in the explore panel    */
   const int LEVEL_1_EXPLORE_COST = 200; /* Cost of a level 1 exploration                        */
   const int LEVEL_2_EXPLORE_COST = 300; /* Cost of a level 2 exploration                        */
   const int LEVEL_3_EXPLORE_COST = 400; /* Cost of a level 3 exploration                        */
   const int EXPLORE_DURATION     = 2;   /* Number of turns an exploration lasts                 */

   private TurnManager turnManager;
   private InventoryManager inventoryManager;
   private static int explorationUnitLevel = STARTING_LEVEL; /* Current level of exploratin unit */
   private static int exploreEndTurn       = 0;              /* End turn of current exploration  */
   private int activeExploreTabId          = LEVEL_1_TAB_ID; /* The explore tab currently open   */
   private List<(string name, int amount)> pendingRewards = new();
                                                             /* Uncollected generated rewards    */
   public  bool isExploring                = false;          /* Is exploration currently ongoing */


   private void Start()
   {
      turnManager = TurnManager.Instance;
      if(turnManager == null)
         Debug.LogError("TurnManager instance not found! Make sure TurnManager exists in scene.");

      inventoryManager = InventoryManager.Instance;
      if (inventoryManager == null)
         Debug.LogError("InventoryManager instance not found! Make sure InventoryManager exists in scene.");
   }

   /*  */
   private void Update()
   {
      if(isExploring && turnManager != null)
         if(turnManager.currentTurn >= exploreEndTurn)
            FinishExploration();
   }

   /*  */
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

      if (explorationUnitLevelText == null)
         Debug.LogError("Exploration Unit Level Text is not assigned");
      else
         explorationUnitLevelText.text = "Level " + explorationUnitLevel.ToString();
   }

   /* Determines if the user has enough currency for an action                                   */
   private bool HasEnoughPearls(int cost)
   {
      if (inventoryManager == null)
      {
         Debug.LogError("InventoryManager not found!");
         return false;
      }
      return inventoryManager.pearlCount >= cost;
   }

   /* Activates the requested exploration unit panel                                             */
   public void RequestExplorationUnitPanel(int buttonID)
   {
      /* Activate the correct exploration unit panel                                             */
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

               /* Allow upgrade only if user has enough currency, exploration is not ongoing,    */
               /* and unit is not already at max level                                           */
               bool canAffordUpgrade = HasEnoughPearls(UPGRADE_COST);
               bool canUpgrade = canAffordUpgrade && !isExploring;
               bool isMaxLevel = explorationUnitLevel >= ENDING_LEVEL;
               yesButton.interactable = canUpgrade && !isMaxLevel;

               /* When fully upgraded, change upgrade button to inform user                      */
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

   /* Switch the explore tab based on the tab that has been clicked                              */
   public void SwitchExploreTab(int tabId)
   {
      activeExploreTabId = tabId;

      /* Deactivate all three tabs                                                               */
      if(levelOneTab != null) 
         levelOneTab.SetActive(false);
      if(levelTwoTab != null) 
         levelTwoTab.SetActive(false);
      if(levelThreeTab != null) 
         levelThreeTab.SetActive(false);

      /* Activate the requested tab                                                              */
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
      UpdateExploreButton(tabId, isExploring);
   }

   /* Change the explore button based on if an exploration is currently ongoing, the user has    */
   /* enough money, or if the Exploration Unit is at a high enough level                         */
   public void UpdateExploreButton(int tabId, bool isExploring)
   {
      GameObject activeTab = null; /* The explore tab currently open                             */
      int exploreCost      = 0;    /* Cost of an exploration in the current explore tab          */
      int requiredLevel    = 0;    /* Level required to explore in current explore tab           */

      /* Determine information of the active tab                                                 */
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

      /* Get the explore button within the active tab                                            */
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

      /* When exploration unit's level is not high enough for tab's exploration                  */
      if(needsUpgrade)
      {
         /* Make button open upgrade panel instead of start exploration                          */
         startButton.onClick.RemoveAllListeners();
         startButton.onClick.AddListener(() =>
         {
            if(explorePanel != null)
               explorePanel.gameObject.SetActive(false);
            RequestExplorationUnitPanel(UPGRADE_BUTTON);
         });
         startButton.interactable = explorationUnitLevel < ENDING_LEVEL;

         /* Change button to indicate upgrade panel will open                                    */
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
      /* Start an exploration                                                                    */
      else
      {
         startButton.onClick.AddListener(() => StartExploration(exploreCost));
         bool canAffordExplore = HasEnoughPearls(exploreCost);
         bool canExplore = canAffordExplore && !isExploring;
         startButton.interactable = canExplore;

         /* Update button text to indicate exploration will start                                */
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
   public void StartExploration(int cost)
   {
      if(isExploring)
      {
         Debug.LogError("Exploration is already ongoing!");
         return;
      }
      if (!HasEnoughPearls(cost))
      {
         Debug.LogError("Not enough pearls for exploration!");
         return;
      }
      inventoryManager.TrySpendPearl(cost);

      exploreEndTurn = turnManager.currentTurn + EXPLORE_DURATION;
      isExploring = true;
      HideAllBoats();
      CloseExplorationPanel();
      PopUpManager.Instance.EnablePlayerInput();
   }

   /* An exploration finishes after a certain number of turns has passed                         */
   public void FinishExploration()
   {
      isExploring = false;
      exploreEndTurn = 0;
      pendingRewards = GenerateExplorationRewards(explorationUnitLevel);
      UpdateBoatVisuals();
      ShowRewardsPanel();
   }

   /* Generates a random set of rewards based on exploration level                               */
   private List<(string name, int amount)> GenerateExplorationRewards(int level)
   {
      List<(string name, int amount)> rewards = new();

      foreach(var r in possibleRewards)
      {
         float chance = r.baseChance + r.levelBonus * (level - 1);
         chance = Mathf.Clamp01(chance);
         int totalFound = 0;
         for(int i = 0; i < r.maximumAmount; i++)
            if (Random.value <= chance)
               totalFound++;
         if(totalFound < r.minimumAmount)
            totalFound = r.minimumAmount;
         rewards.Add((r.rewardName, totalFound));
      }
      return rewards;
   }

   /* Show the rewards found on an exploration                                                   */
   public void ShowRewardsPanel()
   {
      /* Enable the rewards panel                                                                */
      rewardsPanel.gameObject.SetActive(true);
      /* Clear old entries                                                                       */
      foreach (Transform child in rewardsContainer)
         Destroy(child.gameObject);

      foreach(var reward in pendingRewards)
      {
         if(reward.amount <= 0)
            continue;
         GameObject entry = Instantiate(rewardEntry, rewardsContainer);
         var tmp = entry.GetComponentInChildren<TextMeshProUGUI>();
         if (tmp != null)
            tmp.text = $"{reward.name} x{reward.amount}";

         var image = entry.GetComponentInChildren<Image>();
         if(image != null)
         {
            var rewardData = System.Array.Find(possibleRewards, r => r.rewardName == reward.name);
            if(rewardData != null && rewardData.rewardIcon != null)
               image.sprite = rewardData.rewardIcon;
         }
      }

      Button collectButton = rewardsPanel.Find("CollectButton").GetComponent<Button>();
      collectButton.onClick.RemoveAllListeners();
      collectButton.onClick.AddListener(() => CollectRewards());
   }

   /* Collect all rewards found on the exploration                                               */
   public void CollectRewards()
   {
//TODO ------------------------------------------------------------------    add all rewards to player's inventory

      Debug.Log("rewards collected");
      foreach (var reward in pendingRewards)
         Debug.Log($"{reward.name} x{reward.amount}");
      pendingRewards.Clear();
      rewardsPanel.gameObject.SetActive(false);
   }

   /* Upgrade the exploration unit to the next level                                             */
   public void UpgradeExplorationUnit()
   {
      if(explorationUnitLevel < ENDING_LEVEL && HasEnoughPearls(UPGRADE_COST))
      {
         explorationUnitLevel += 1;
         UpdateBoatVisuals();
         inventoryManager.TrySpendPearl(UPGRADE_COST);
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
      if (isExploring)
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
         SwitchExploreTab(LEVEL_1_TAB_ID + explorationUnitLevel - 1);
      else
         SwitchExploreTab(activeExploreTabId);
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