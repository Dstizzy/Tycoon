using System.Collections.Generic;

using UnityEditor.Search;

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

        foreach (GameObject button in buildingButtonsPreFab) {
            if (buildingTransform.tag != "Lab" || popUps.Count < 2) {
                GameObject newButton = Instantiate(button, fixedPopUpPosition, Quaternion.identity);
                popUps.Add(newButton);
                if (popUps.Count == 1) {
                    switch (buildingTransform.tag) {
                        case "Trade Hut":
                            newButton.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() => OnBuildingButtonClick(buildingTransform));
                            newButton.GetComponentInChildren<ButtonsPopUp>().SetText("Trade");
                            break;
                        default:
                            newButton.GetComponentInChildren<ButtonsPopUp>().SetText("Test");
                            break;
                    }
                } else if (popUps.Count == 2) {
                    newButton.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() => OnBuildingButtonClick(buildingTransform));
                    newButton.GetComponentInChildren<ButtonsPopUp>().SetText("Info");
                } else {
                    newButton.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() => OnBuildingButtonClick(buildingTransform));
                    newButton.GetComponentInChildren<ButtonsPopUp>().SetText("Upgrade");
                }
            }
            fixedPopUpPosition.y -= buttonSpacing;
        }
    }
    public void OnBuildingButtonClick(Transform buildingTransform) {
        switch (buildingTransform.tag) {
            case "Trade Hut":
                tradeHutManager.ShowTradePanel();
                DisablePlayerInput();
                break;
        }
    }
    public void DisablePlayerInput() {
        playerActions.PlayerInput.Disable();
        HoverScript.Instance.DisbaleHover();
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
    }
}
