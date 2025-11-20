/* libraries                                                                                     */
using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LabManager : MonoBehaviour
{
    /* Symbolic Constants                                                                        */
    const int INNOVATE_BUTTON = 1;
    const int INFO_BUTTON = 2;
    const int UPGRADE_BUTTON = 3;
    const int TIER_ONE = 1;
    const int TIER_TWO = 2;
    const int TIER_THREE = 3;
    const int TIER_ONE_PEARL_COST = 100;
    const int TIER_ONE_ITEM_COST = 10;
    const int TIER_TWO_PEARL_COST = 350;
    const int TIER_TWO_ITEM_COST = 25;
    const int TIER_THREE_PEARL_COST = 700;
    const int TIER_THREE_ITEM_COST = 50;

    /* Inspector Variables                                                                       */
    [SerializeField] private Transform innovatePanel;
    [SerializeField] private Transform infoPanel;

    [SerializeField] private GameObject pathButtons;
    [SerializeField] private GameObject initialTab;
    [SerializeField] private GameObject commerceTab;
    [SerializeField] private GameObject productionTab;
    [SerializeField] private GameObject explorationTab;
    [SerializeField] private CraftingController craftingController;
   
    TradeHutManager tradeHutManager;

    /* Check if all required game objects exist and are in there required states                 */
    private void Awake() 
    {
      tradeHutManager = TradeHutManager.Instance;

      if (tradeHutManager == null)
            Debug.LogError("Insance is not initialized");


        /* Set the info panel to inactive if it exists                                           */
        if (infoPanel == null) 
        {
            Debug.LogError("Info Panel is not assigned in the Inspector!");
        } else 
        {
            infoPanel.gameObject.SetActive(false);
        }

        /* Set the research panel to inactive if it exists                                       */
        if (innovatePanel == null) 
        {
            Debug.LogError("Innovate Panel is not assigned");
        } else 
        {
            innovatePanel.gameObject.SetActive(false);
        }

        /* Set the research panel to inactive if it exists                                       */
        if (initialTab == null)
        {
            Debug.LogError("Commerce Tab is not assigned");
        }
        else
        {
            initialTab.gameObject.SetActive(true);
        }

        /* Set the research panel to inactive if it exists                                       */
        if (commerceTab == null)
        {
            Debug.LogError("Commerce Tab is not assigned");
        }
        else
        {
            commerceTab.gameObject.SetActive(false);
        }

        /* Set the research panel to inactive if it exists                                       */
        if (productionTab == null)
        {
            Debug.LogError("Production Tab is not assigned");
        }
        else
        {
            productionTab.gameObject.SetActive(false);
        }

        /* Set the research panel to inactive if it exists                                       */
        if (explorationTab == null)
        {
            Debug.LogError("Commerce Tab is not assigned");
        }
        else
        {
            explorationTab.gameObject.SetActive(false);
        } 
    }

    /* Open up a lab panel upon clicking the corresponding button                                */
    public void RequestLabPanel(int buttonID) 
    {
        switch (buttonID) {
            case INNOVATE_BUTTON:
                ShowInnovatePanel();
                innovatePanel.transform.Find("ExitButton").GetComponent<Button>().onClick.AddListener(() => CloseLabPanel(INNOVATE_BUTTON));
                break;
            case INFO_BUTTON:
                ShowInfoPanel();
                infoPanel.transform.Find("ExitButton").GetComponent<Button>().onClick.AddListener(() => CloseLabPanel(INFO_BUTTON));
                break;
            default:
                Debug.Log("Building Panel: Unknown button ID.");
                break;
        }
    }

    /* Close the lab panel upon clicking the exit button                                         */
    public void CloseLabPanel(int buttonID) 
    {
        switch (buttonID) {
            case INNOVATE_BUTTON:
                CloseInnovatePanel();
                break;
            case INFO_BUTTON:
                CloseInfoPanel();
                break;
            case UPGRADE_BUTTON:
                Debug.Log("Building Panel: Info requested.");
                break;
            default:
                Debug.Log("Building Panel: Unknown button ID.");
                break;
        }
        PopUpManager.Instance.EnablePlayerInput();
    }

    /* Open up the research panel and assign the buttons in the initial panel                    */
    private void ShowInnovatePanel() 
    {
        innovatePanel.gameObject.SetActive(true);
        pathButtons.transform.Find("commercePath").GetComponent<Button>().onClick.AddListener(() => ShowPath(commerceTab));
        pathButtons.transform.Find("productionPath").GetComponent<Button>().onClick.AddListener(() => ShowPath(productionTab));
        pathButtons.transform.Find("explorationPath").GetComponent<Button>().onClick.AddListener(() => ShowPath(explorationTab));

    }

    /* Show the corresponding path tab upon clicking the path button                             */
    private void ShowPath(GameObject tab)
    {
        initialTab.gameObject.SetActive(false);
        tab.gameObject.SetActive(true);
        /*tab.transform.Find("branch/tierNodeOneContainer").OnMouseEnter();*/
        tab.transform.Find("backArrow").GetComponent<Button>().onClick.AddListener(() => BackToInitialTab(tab));
        tab.transform.Find("buttonContainer/tierOneButton").GetComponent<Button>().onClick.AddListener(() => HandleInnovation(tab, TIER_ONE));
        tab.transform.Find("buttonContainer/tierTwoButton").GetComponent<Button>().onClick.AddListener(() => HandleInnovation(tab, TIER_TWO));
        tab.transform.Find("buttonContainer/tierThreeButton").GetComponent<Button>().onClick.AddListener(() => HandleInnovation(tab, TIER_THREE));
    }

    /* Handle different changes upon clicking a buy button                                       */
    private void HandleInnovation(GameObject tab, int tier)
    {
        switch (tier)
        {
            case TIER_ONE:
                PerformBuy(TIER_ONE_PEARL_COST, TIER_ONE_ITEM_COST);
                ImplementTierOneInnovation(tab);
                UnlockNextNode(tab, 2);
                tab.transform.Find("branch/firstConnector/unfilledConnector").gameObject.SetActive(false);
                tab.transform.Find("branch/firstConnector/filledConnector").gameObject.SetActive(true);
                tab.transform.Find("branch/tierNodeOneContainer/tierNodeOneUnfilled").gameObject.SetActive(false);
                tab.transform.Find("branch/tierNodeOneContainer/tierNodeOneFilled").gameObject.SetActive(true);
                tab.transform.Find("buttonContainer/tierOneButton").gameObject.SetActive(false);
                tab.transform.Find("costContainer/tierOneCost").gameObject.SetActive(false);
                ImplementTierOneInnovation(tab);
                break;
            case TIER_TWO:
                PerformBuy(TIER_TWO_PEARL_COST, TIER_TWO_ITEM_COST);
                ImplementTierTwoInnovation(tab);
                UnlockNextNode(tab, 3);
                tab.transform.Find("branch/secondConnector/unfilledConnector").gameObject.SetActive(false);
                tab.transform.Find("branch/secondConnector/filledConnector").gameObject.SetActive(true);
                tab.transform.Find("branch/tierNodeTwoContainer/tierNodeTwoUnfilled").gameObject.SetActive(false);
                tab.transform.Find("branch/tierNodeTwoContainer/tierNodeTwoFilled").gameObject.SetActive(true);
                tab.transform.Find("buttonContainer/tierTwoButton").gameObject.SetActive(false);
                tab.transform.Find("costContainer/tierTwoCost").gameObject.SetActive(false);
                ImplementTierTwoInnovation(tab);
                break;
            case TIER_THREE:
                PerformBuy(TIER_THREE_PEARL_COST, TIER_THREE_ITEM_COST);
                ImplementTierThreeInnovation(tab);
                tab.transform.Find("branch/thirdConnector/unfilledConnector").gameObject.SetActive(false);
                tab.transform.Find("branch/thirdConnector/filledConnector").gameObject.SetActive(true);
                tab.transform.Find("branch/tierNodeThreeContainer/TierNodeThreeUnfilled").gameObject.SetActive(false);
                tab.transform.Find("branch/tierNodeThreeContainer/TierNodeThreeFilled").gameObject.SetActive(true);
                tab.transform.Find("buttonContainer/tierThreeButton").gameObject.SetActive(false);
                tab.transform.Find("costContainer/tierThreeCost").gameObject.SetActive(false);
                ImplementTierThreeInnovation(tab);
                break;
        };
    }

    /* Spend certain amount of resources and give corresponding innovations                      */
    public void PerformBuy(int pearlCost, int itemCost)
    {
        InventoryManager.Instance.TrySpendPearl(pearlCost);
        //InventoryManager.Instance.TrySpendItem("Crude Tool", itemCost);

    }

    public void ImplementTierOneInnovation(GameObject tabType)
    {
        /* Permanently increase base sale price of all items by 10%                              */
        if (tabType == commerceTab)
            Console.WriteLine();
        /* Permanently reduce gold spent on refinery upkeep by 50%                               */
        else if (tabType == productionTab)
        {
            Debug.Log("Ore upkeep reduced by 50%");
        }
        /* Tier 1 missions have succession increased by 25%                                      */
        else if (tabType == explorationTab)
        {
            Debug.Log("Tier 1 missions increased by 25%");
        }
        else
        {
            Debug.Log("There is no tab");
        }
    }

    public void ImplementTierTwoInnovation(GameObject tabType)
    {
        /* Grant action to gameple 50 gold for 60% chance to get 250 back                        */
        if (tabType == commerceTab)
        {
            Button mysteryBox = tradeHutManager.BuyPanel.Find("Mystery Box").GetComponent<Button>();
            Image  chainImage = tradeHutManager.BuyPanel.Find("Chain").GetComponent<Image>();

            chainImage.gameObject.SetActive(false);
            
            mysteryBox.interactable = true;
        }
        /* Unlock tier 2 item (reinforces component); forge now has 5% chance to produce a       */
        /*    bonus item upon crafting a single item                                             */
        else if (tabType == productionTab)
        {
            Debug.Log("Unlock reinforced tool and add 5% chance of bonus item");
            craftingController.UnlockRefinedToolFromLab();
            craftingController.ApplyLockStateToUI();
        }
        /* Permanently increase gold by +15 per turn                                             */
        else if (tabType == explorationTab)
        {
            Debug.Log("Permanently increase gold by +15 per turn");
        }
        else
        {
            Debug.Log("There is no tab");
        }
    }

    public void ImplementTierThreeInnovation(GameObject tabType)
    {
        /* Allows all items in storage to be sold for 5x multiplier                              */
        if (tabType == commerceTab)
        {
            Debug.Log("All items in storage sold for 5x");
        }
        /* Unlock tier 3 itme (Artifact); Crafting results in two items being made               */
        else if (tabType == productionTab)
        {
            Debug.Log("Unlock Artifact and crafting results in double item");
            craftingController.UnlockArtifactToolFromLab();
            craftingController.ApplyLockStateToUI();
        }
        /* Decrease search costs by 50%                                                          */
        else if (tabType == explorationTab)
        {
            Debug.Log("Decrease search costs by 50%");
        }
        else
        {
            Debug.Log("There is no tab");
        }
    }

    /* Unlock the next tier node upon buying the previous tier node                              */
    public void UnlockNextNode(GameObject tab, int tier)
    {
        Color currentColor;

        /* Get rid of the tier 2 lock and turn on buttons and text                               */
        if (tier == 2)
        {
            tab.transform.Find("lockContainer/tierTwoLock").gameObject.SetActive(false);

            currentColor = tab.transform.Find("buttonContainer/tierTwoButton").GetComponent<Image>().color;
            currentColor.a = 1.0f;
            tab.transform.Find("buttonContainer/tierTwoButton").GetComponent<Image>().color = currentColor;
            tab.transform.Find("buttonContainer/tierTwoButton").GetComponent<Button>().interactable = true;

            currentColor = tab.transform.Find("costContainer/tierTwoCost").GetComponent<TextMeshProUGUI>().color;
            currentColor.a = 1.0f;
            tab.transform.Find("costContainer/tierTwoCost").GetComponent<TextMeshProUGUI>().color = currentColor;
        }
        /* Get ride of the tier 3 lock and turn on buttons and text                              */
        else
        {
            if (tier != 3)
            {
                Debug.Log("Accessing wrong tier node");
                return;
            }
            tab.transform.Find("lockContainer/tierThreeLock").gameObject.SetActive(false);

            currentColor = tab.transform.Find("buttonContainer/tierThreeButton").GetComponent<Image>().color;
            currentColor.a = 255;
            tab.transform.Find("buttonContainer/tierThreeButton").GetComponent<Image>().color = currentColor;
            tab.transform.Find("buttonContainer/tierThreeButton").GetComponent<Button>().interactable = true;

            currentColor = tab.transform.Find("costContainer/tierThreeCost").GetComponent<TextMeshProUGUI>().color;
            currentColor.a = 255;
            tab.transform.Find("costContainer/tierThreeCost").GetComponent<TextMeshProUGUI>().color = currentColor;

        }
    }

    /* Return to the initial tab upon clicking the back arrow button                             */
    private void BackToInitialTab(GameObject tab)
    {
        tab.gameObject.SetActive(false);
        initialTab.gameObject.SetActive(true);
    }

    /* Open up the info panel                                                                    */
    private void ShowInfoPanel() 
    {
        infoPanel.gameObject.SetActive(true);
    }

    /* Close the research panel                                                                  */
    private void CloseInnovatePanel() 
    {
        commerceTab.gameObject.SetActive(false);
        productionTab.gameObject.SetActive(false);
        explorationTab.gameObject.SetActive(false);
        initialTab.gameObject.SetActive(true);
        innovatePanel.gameObject.SetActive(false);
    }

    /* Close the info panel                                                                      */
    private void CloseInfoPanel()
    {
        infoPanel.gameObject.SetActive(false);
    }


}
