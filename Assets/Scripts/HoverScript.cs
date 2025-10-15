using UnityEngine;
using UnityEngine.InputSystem;

public class HoverScript : MonoBehaviour {

    RaycastHit2D raycastHit2D;

    private Transform prevHoverObject;
    private Transform currentHoverObject;

    private PlayerActions playerActions;
    public static HoverScript Instance { get; private set; }

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }

        playerActions = new PlayerActions();
        playerActions.PlayerInput.Enable();
        playerActions.PlayerInput.Hover.performed += Hover;
    }

    public void Hover(InputAction.CallbackContext context) {
        // 1. Get mouse position converted to World Space (Vector2 is sufficient for 2D physics)
        Vector2 mouseScreenPos = context.ReadValue<Vector2>();

        // Then convert to world space:
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        // 2. Perform the 2D Raycast (a single-point check at the mouse's location)
        raycastHit2D = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        // 3. Determine the object hit this frame, or null if nothing was hit
        currentHoverObject = raycastHit2D.collider ? raycastHit2D.collider.transform : null;

        // =========================================================
        // 4. CORE HOVER LOGIC: Compare current state to previous state
        // =========================================================

        // Case A: Mouse moved OFF the previous object (either to empty space or a new object)
        if (prevHoverObject != null && prevHoverObject != currentHoverObject) {
            // Revert the color/state of the object we just left
            prevHoverObject.GetComponentInChildren<SpriteRenderer>().color = Color.white;
        }

        // Case B: Mouse moved ONTO a new object
        if (currentHoverObject != null && currentHoverObject != prevHoverObject) {
            // Set the color/state of the new object
            currentHoverObject.GetComponentInChildren<SpriteRenderer>().color = Color.red;
        }

        // 5. Update the state for the next frame
        // What we hit this frame becomes the 'previous' object for the next frame's comparison
        prevHoverObject = currentHoverObject;
    }

    public void DisbaleHover() {
        playerActions.PlayerInput.Disable();
    }

}


