using System.Collections.Generic;

using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PopUpManager : MonoBehaviour {
    [SerializeField] private GameObject[] buildingButtonsPreFab;
    [SerializeField] private Camera cam;
    [SerializeField] private TradeHutManager tradeHutManager;

    private List<GameObject> popUps;
    private PlayerActions playerActions;
    private Transform buildingTransform;
    private List<RaycastResult> raycastResults = new List<RaycastResult>();

    public static PopUpManager Instance { get; private set; }

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }

        playerActions = new PlayerActions();
        playerActions.PlayerInput.Enable();
        playerActions.PlayerInput.OnBuildingClick.performed += OnBuildingClick;
    }
    private void OnDestroy() {
        if (playerActions != null) {
            playerActions.PlayerInput.OnBuildingClick.performed -= OnBuildingClick;
            playerActions.PlayerInput.Disable();
            playerActions.Dispose();
        }
    }
    private void OnBuildingClick(InputAction.CallbackContext context) {

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Mouse.current.position.ReadValue();

        // 2. CLEAR and CHECK FOR UI HITS using RaycastAll
        raycastResults.Clear();
        EventSystem.current.RaycastAll(eventData, raycastResults);

        // If the list is not empty, a UI element was hit.
        if (raycastResults.Count > 0) return;

        Vector2 mouseScreenPos = eventData.position;

        Vector3 worldPos = cam.ScreenToWorldPoint(mouseScreenPos);

        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null) {
            if (popUps == null || (buildingTransform != null && hit.collider.transform.tag != buildingTransform.tag)) {
                ClosePopUps();
                buildingTransform = hit.collider.transform;
                CreateBuildingButtons(buildingTransform);
            } else
                ClosePopUps();
        } else {
            ClosePopUps();
        }
    }
    private void CreateBuildingButtons(Transform buildingTransform) {
        Vector3 offset = new Vector3(-6.0f, 3.0f, 0f);
        Vector3 fixedPopUpPosition = buildingTransform.position + offset;
        float buttonSpacing = 2.0f;

        popUps = new();

        int buttonCount = (buildingTransform.CompareTag("Lab")) ? 2 : buildingButtonsPreFab.Length;

        for (int buttonIndex = 0; buttonIndex < buttonCount; buttonIndex++) {

            GameObject buttonPreFab = buildingButtonsPreFab[buttonIndex];
            GameObject newButton = Instantiate(buttonPreFab, fixedPopUpPosition, Quaternion.identity);
                        GameObject newButton = Instantiate(button, fixedPopUpPosition, Quaternion.identity);
            popUps.Add(newButton);

            string uniqueButtonName = buildingTransform.tag switch {
                "Trade Hut" => "Trade",
                "Lab" => "Research",
                "Ore Refinory" => "Refine",
                "Exploration Unit" => "Explore",
                "Forge" => "Craft",
                _ => "BuildingButton"
            };

            string buttonText = buttonIndex switch {
                0 => uniqueButtonName,
                1 => "Info",
                2 => "Upgrade",
                _ => "Button"
            };

            int buttonId = buttonIndex + 1;

            newButton.GetComponentInChildren<ButtonsPopUp>().SetText(buttonText);
            newButton.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() => OnBuildingButtonClick(buttonId));

            fixedPopUpPosition.y -= buttonSpacing;
        }
    }
    public void OnBuildingButtonClick(int buttonId) {
        switch (buildingTransform.tag) {
            case "Trade Hut":
                tradeHutManager.RequestTradeHutPanel(buttonId);
                break;
            //case "Lab":
            //    labManaager.RequestTradeHutPanel(buttonId);
            //    break;
            //case "Forge":
            //    forgeManager.RequestForgePanel(buttonId);
            //    break;
            //case "Ore Refinory":
            //    oreRefinoryManager.RequestOreRefinoryPanel(buttonId);
            //    break;
            //case "Exploration Unit":
            //    explorationUnitManager.RequestExplorationUnitPanel(buttonId);
            //    break;
            default:
                Debug.Log("Building Panel: Unknown building type.");
                break;
        }
        DisablePlayerInput();
    }
    //public void ClosePanel(int buttonId) {
    //    switch (buildingTransform.tag) {
    //        case "Trade Hut":
    //            tradeHutManager.CloseTradeHutPanel(buttonId);
    //            break;
    //        //case "Lab":
    //        //    labManaager.RequestTradeHutPanel(buttonId);
    //        //    break;
    //        //case "Forge":
    //        //    forgeManager.RequestForgePanel(buttonId);
    //        //    break;
    //        //case "Ore Refinory":
    //        //    oreRefinoryManager.RequestOreRefinoryPanel(buttonId);
    //        //    break;
    //        //case "Exploration Unit":
    //        //    explorationUnitManager.RequestExplorationUnitPanel(buttonId);
    //        //    break;
    //        default:
    //            Debug.Log("Building Panel: Unknown building type.");
    //            break;
    //    }
    //    EnablePlayerInput();
    //}
   

    public void DisablePlayerInput() {
        playerActions.PlayerInput.Disable();
        HoverScript.Instance.DisbaleHover();
    }
    public void EnablePlayerInput() {
        playerActions.PlayerInput.Enable();
        HoverScript.Instance.EnableHover();
        ClosePopUps();
    }

    private void ClosePopUps() {
        if (popUps != null) {
            foreach (GameObject currentPopUp in popUps) {
                if (currentPopUp != null)
                    Destroy(currentPopUp);
            }
            popUps = null;
            buildingTransform = null;
        }
        TradeHutManager.Instance.ShowTradePanel();
    }
}
