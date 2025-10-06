using UnityEngine;
using UnityEngine.InputSystem;

public class PopUpManager : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] GameObject buttonsPreFab;
    private GameObject popUp;

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
            
            Vector3 offset = new  Vector3(-10.0f, 0.0f, 0f);

            Vector3 fixedPopUpPosition = buildingTransform.position + offset;

            if (popUp == null) { 
                popUp = Instantiate(buttonsPreFab, fixedPopUpPosition, Quaternion.identity);
                switch (buildingTransform.tag) {
                    case "Trade Hut":
                        popUp.GetComponent<ButtonsPopUp>().SetText("Trade", "Info", "Upgrade");
                        break;
                    default:
                        popUp.GetComponent<ButtonsPopUp>().SetText("Test", "Test 2", "Test 3");
                        break;
                }
            } else {
                Destroy(popUp);
                popUp = null;
            }
        }
    }
}
