using System.Collections;
using UnityEngine;
using TMPro;

public class TickerSystem : MonoBehaviour {
   [Header("UI References")]
   [SerializeField] private TextMeshProUGUI messageText;
   [SerializeField] private CanvasGroup     canvasGroup;

   [Header("Settings")]
   [SerializeField] private float timeVisible = 3.0f;
   [SerializeField] private float fadeDuration = .5f; // Used for both Fade In and Fade Out

   private Coroutine activeRoutine;

   private void Awake() {
      // Ensure alpha is 0 when the game boots up
      if (canvasGroup != null) 
         canvasGroup.alpha = 0;
     
      gameObject.SetActive(false);
   }

   public void ShowTicker(string message, Color textColor) 
   {
      // 1. Force the object on so the coroutine runs
      gameObject.SetActive(true);

      messageText.text  = message;
      messageText.color = textColor;

      // 2. Start fully invisible
      canvasGroup.alpha = 0;

      // 3. Stop existing routines to prevent conflicts
      if (activeRoutine != null)
         StopCoroutine(activeRoutine);

      // 4. Start the full sequence (In -> Wait -> Out)
      activeRoutine = StartCoroutine(FadeSequence());
   }

   private IEnumerator FadeSequence() {
      float timer = 0;

      // --- STEP 1: FADE IN ---
      while (timer < fadeDuration) 
      {
         timer += Time.deltaTime;

         // Lerp from 0 to 1
         canvasGroup.alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
         yield return null;
      }
      canvasGroup.alpha = 1; // Ensure it ends at exactly 1

      // --- STEP 2: WAIT ---
      yield return new WaitForSeconds(timeVisible);

      // --- STEP 3: FADE OUT ---
      timer = 0; // Reset timer
      while (timer < fadeDuration) 
      {
         timer += Time.deltaTime;

         // Lerp from 1 to 0
         canvasGroup.alpha = Mathf.Lerp(1, 0, timer / fadeDuration);
         yield return null;
      }
      canvasGroup.alpha = 0; // Ensure it ends at exactly 0

      // Optional: Turn object off again
      gameObject.SetActive(false);
   }
}