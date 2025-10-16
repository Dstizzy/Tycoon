using System.Collections.Generic;

using UnityEditor.Search;

using UnityEngine;
using UnityEngine.InputSystem;

public class PopUpManager : MonoBehaviour {
    [SerializeField] private GameObject[] buildingButtonsPreFab;
    [SerializeField] private Camera cam;
    [SerializeField] private TradeHutManager tradeHutManager;

    private List<GameObject> popUps;
    private PlayerActions playerActions;
    private Transform buildingTransform;

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

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

        Vector3 worldPos = cam.ScreenToWorldPoint(mouseScreenPos);

        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        worldPos.z = 0;

        if (hit.collider != null) {
            if (buildingTransform == null || hit.collider.transform.tag != buildingTransform.tag) {
                buildingTransform = hit.collider.transform;
                Vector3 offset = new Vector3(-6.0f, 3.0f, 0f);
                Vector3 fixedPopUpPosition = buildingTransform.position + offset;



                popUps = new();

                float buttonSpacing = 2.0f;

                foreach (GameObject button in buildingButtonsPreFab) {
                    if (buildingTransform.tag != "Lab" || popUps.Count < 2) {
                        GameObject newButton = Instantiate(button, fixedPopUpPosition, Quaternion.identity);
                        popUps.Add(newButton);
                        switch (buildingTransform.tag) {
                            case "Trade Hut":
                                if (popUps.Count == 1)
                                    newButton.GetComponentInChildren<ButtonsPopUp>().SetText("Trade");
                                else if (popUps.Count == 2)
                                    newButton.GetComponentInChildren<ButtonsPopUp>().SetText("Info");
                                else
                                    newButton.GetComponentInChildren<ButtonsPopUp>().SetText("Upgrade");
                                break;
                            default:
                                newButton.GetComponentInChildren<ButtonsPopUp>().SetText("Test");
                                break;
                        }
                        fixedPopUpPosition.y -= buttonSpacing;
                    }
                }
            } else {
                foreach (GameObject currentPopUp in popUps) {
                    if (currentPopUp != null)
                        Destroy(currentPopUp);
                }
                popUps = null;
            }
        } else {
            foreach (GameObject currentPopUp in popUps) {
                if (currentPopUp != null)
                    Destroy(currentPopUp);
            }
            popUps = null;
        }
    }

    public void OnBuildingButtonClick() {
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
}
