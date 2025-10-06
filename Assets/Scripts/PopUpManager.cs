using UnityEngine;
using UnityEngine.InputSystem;

public class PopUpManager : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] GameObject popUpPrefab;
    private GameObject popUp;

    PlayerActions playerActions;

    private void Awake() {
        playerActions = new PlayerActions();
        playerActions.PlayerInput.Enable();
        playerActions.PlayerInput.Click.performed += Click;
    }

    private void OnDestroy() {
        if (playerActions != null) {
            playerActions.PlayerInput.Click.performed -= Click;
            playerActions.PlayerInput.Disable();
            playerActions.Dispose();
        }
    }

    void Click(InputAction.CallbackContext context) {
     
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

        Vector3 worldPos = cam.ScreenToWorldPoint(mouseScreenPos);

        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        worldPos.z = 0;

        if (hit.collider != null) {

            Transform buildingTransform = hit.collider.transform;

            Vector3 offset = new  Vector3(0f, 2.0f, 0f);

            Vector3 fixedPopUpPosition = buildingTransform.position;

            if (popUp == null) { 
                popUp = Instantiate(popUpPrefab, fixedPopUpPosition, Quaternion.identity);

                popUp.GetComponent<PopUp>().SetText("Test");
            }
            else {
                Destroy(popUp);
                popUp = null;
            }
        }
    }
}
