using UnityEngine;
using UnityEngine.InputSystem;

public class HoverScript : MonoBehaviour {
   /* Inspector variables                                                                                                                              */
   public Camera mainCam;

   /* Private variables                                                                                                                                */
   RaycastHit2D raycastHit2D;

   private Transform prevHoverObject;
   private Transform currentHoverObject;

   private PlayerActions playerActions;

   /* Public static variables                                                                                                                          */
   public static HoverScript Instance { get; private set; }

   private void Awake() {
      if (Instance != null && Instance != this)
         Destroy(gameObject);
      else
         Instance = this;

      playerActions = new PlayerActions();
      playerActions.PlayerInput.Enable();
      playerActions.PlayerInput.Hover.performed += Hover;
   }

   public void Hover(InputAction.CallbackContext context) 
   {
      /* 1. Get mouse position converted to World Space (Vector2 is sufficient for 2D physics)                                                      */
      Vector2 mouseScreenPos = context.ReadValue<Vector2>();

      /* Then convert to world space:                                                                                                                 */
      Vector2 mouseWorldPos = mainCam.ScreenToWorldPoint(mouseScreenPos);

      /* 2. Perform the 2D Raycast (a single-point check at the mouse's location)                                                                     */
      raycastHit2D = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

      /* 3. Determine the object hit this frame, or null if nothing was hit                                                                           */
      currentHoverObject = raycastHit2D.collider ? raycastHit2D.collider.transform : null;

      /* =========================================================
       * 4. CORE HOVER LOGIC: Compare current state to previous state
       * =========================================================*/

      /* Case A: Mouse moved OFF the previous object (either to empty space or a new object)                                                          */
      if (prevHoverObject != null && prevHoverObject != currentHoverObject) 
      {
         /* Retrieve the SpriteRenderer and check if it exists before using it                                                                       */
         SpriteRenderer prevRenderer = prevHoverObject.GetComponentInChildren<SpriteRenderer>();

         if (prevRenderer != null)
            /* Revert the color/state of the object we just left                                                                                    */
            prevRenderer.color = Color.white;
      }

      /* Case B: Mouse moved ONTO a new object                                                                                                        */
      if (currentHoverObject != null && currentHoverObject != prevHoverObject) {
         /* Retrieve the SpriteRenderer and check if it exists before using it                                                                       */
         SpriteRenderer currentRenderer = currentHoverObject.GetComponentInChildren<SpriteRenderer>();

         if (currentRenderer != null) 
         {
            /* Set the color/state of the new object                                                                                                */
            currentRenderer.color = Color.red;
         }
      }

      /* Update the state for the next frame
      /* What we hit this frame becomes the 'previous' object for the next frame's comparison                                                         */
      prevHoverObject = currentHoverObject;
   }

   public void DisbaleHover() {
      playerActions.PlayerInput.Disable();
   }

   public void EnableHover() {
      playerActions.PlayerInput.Enable();
   }
}