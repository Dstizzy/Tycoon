using UnityEngine;
using UnityEngine.InputSystem;

public class PopUpManager : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] GameObject popUpPrefab;
    private GameObject popUp;

    PlayerActions playerActions;

    private void Awake() {
        // This MUST be here and must initialize the object!
        playerActions = new PlayerActions();
        playerActions.PlayerInput.Enable();
        playerActions.PlayerInput.Click.performed += Click;
    }

    // Best practice: Clean up subscriptions and resources
    private void OnDestroy() {
        if (playerActions != null) {
            playerActions.PlayerInput.Click.performed -= Click;
            playerActions.PlayerInput.Disable();
            playerActions.Dispose();
        }
    }

    // Update is called once per frame
    void Click(InputAction.CallbackContext context) {
        // FIX: Get the mouse position directly from the current Mouse device.
        // This is the correct way to get Vector2 screen coordinates when the action 
        // itself is bound to a simple button.
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

        // 1. Convert screen position to world position for raycasting
        Vector3 worldPos = cam.ScreenToWorldPoint(mouseScreenPos);

        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        // 2. IMPORTANT: For 2D spawning, ensure Z is 0 so it appears on the plane.
        worldPos.z = 0;

        // 4. Check if the raycast hit anything and if it's the specific object (e.g., the house)
        if (hit.collider != null) {
            if(popUp == null) { 
                // 3. Instantiate the Pop-up
                popUp = Instantiate(popUpPrefab, worldPos, Quaternion.identity);

                // 4. Call the new setter method
                // This is safe assuming the PopUp script and its assignment are correct.
                popUp.GetComponent<PopUp>().SetText("Hello There");
            }
            else {
                Destroy(popUp);
                popUp = null;
            }
        }
    }
}
