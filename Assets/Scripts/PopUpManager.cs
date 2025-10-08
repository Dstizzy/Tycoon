using NUnit.Framework;

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

public class PopUpManager : MonoBehaviour {
    [SerializeField] Camera cam;
    public GameObject[] buildingButtonsPreFab;
    private List<GameObject> popUps;

    PlayerActions playerActions;

    private void Awake() {
        playerActions = new PlayerActions();
        playerActions.PlayerInput.Enable();
        playerActions.PlayerInput.BuildingActions.performed += BuildingActions;
    }

    private void OnDestroy() {
        if (playerActions != null) {
            playerActions.PlayerInput.BuildingActions.performed -= BuildingActions;
            playerActions.PlayerInput.Disable();
            playerActions.Dispose();
        }
    }

    private void BuildingActions(InputAction.CallbackContext context) {

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

        Vector3 worldPos = cam.ScreenToWorldPoint(mouseScreenPos);

        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        worldPos.z = 0;

        if (hit.collider != null) {
            Transform buildingTransform = hit.collider.transform;
            Vector3 offset = new Vector3(-6.0f, 3.0f, 0f);
            Vector3 fixedPopUpPosition = buildingTransform.position + offset;

            if (popUps == null) {

                popUps = new List<GameObject>();

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
        }
    }
}
