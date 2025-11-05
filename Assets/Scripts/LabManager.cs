/* libraries                                                                                     */
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

    /* Check if all required game objects exist and are in there required states                 */
    private void Awake() 
    {
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
                tab.transform.Find("branch/firstConnector/unfilledConnector").gameObject.SetActive(false);
                tab.transform.Find("branch/firstConnector/filledConnector").gameObject.SetActive(true);
                tab.transform.Find("branch/tierNodeOneContainer/tierNodeOneUnfilled").gameObject.SetActive(false);
                tab.transform.Find("branch/tierNodeOneContainer/tierNodeOneFilled").gameObject.SetActive(true);
                tab.transform.Find("buttonContainer/tierOneButton").gameObject.SetActive(false);
                tab.transform.Find("costContainer/tierOneCost").gameObject.SetActive(false);
                break;
            case TIER_TWO:
                PerformBuy(TIER_TWO_PEARL_COST, TIER_TWO_ITEM_COST);
                tab.transform.Find("branch/secondConnector/unfilledConnector").gameObject.SetActive(false);
                tab.transform.Find("branch/secondConnector/filledConnector").gameObject.SetActive(true);
                tab.transform.Find("branch/tierNodeTwoContainer/tierNodeTwoUnfilled").gameObject.SetActive(false);
                tab.transform.Find("branch/tierNodeTwoContainer/tierNodeTwoFilled").gameObject.SetActive(true);
                tab.transform.Find("buttonContainer/tierTwoButton").gameObject.SetActive(false);
                tab.transform.Find("costContainer/tierTwoCost").gameObject.SetActive(false);
                break;
            case TIER_THREE:
                PerformBuy(TIER_THREE_PEARL_COST, TIER_THREE_ITEM_COST);
                tab.transform.Find("branch/thirdConnector/unfilledConnector").gameObject.SetActive(false);
                tab.transform.Find("branch/thirdConnector/filledConnector").gameObject.SetActive(true);
                tab.transform.Find("branch/tierNodeThreeContainer/TierNodeThreeUnfilled").gameObject.SetActive(false);
                tab.transform.Find("branch/tierNodeThreeContainer/TierNodeThreeFilled").gameObject.SetActive(true);
                tab.transform.Find("buttonContainer/tierThreeButton").gameObject.SetActive(false);
                tab.transform.Find("costContainer/tierThreeCost").gameObject.SetActive(false);
                break;
        };
    }

    /* Spend certain amount of resources and give corresponding innovations                      */
    public void PerformBuy(int pearlCost, int itemCost)
    {
        InventoryManager.Instance.TrySpendPearl(pearlCost);
        InventoryManager.Instance.TrySpendItem("Crude Tool", itemCost);

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
    private void CloseInfoPanel() {
        infoPanel.gameObject.SetActive(false);
    }

}
