using System.Collections;
using UnityEngine;
using TMPro;
using System;

public class TickerSystem : MonoBehaviour {
   [Header("UI References")]
   [SerializeField] private CanvasGroup     wortldEventCanvasGroup;
   [SerializeField] private TextMeshProUGUI worldEventMessageText;
   [SerializeField] private CanvasGroup     resultCanvasGroup;
   [SerializeField] private TextMeshProUGUI resultMessageText;

   [Header("Settings")]
   [SerializeField] private float timeVisible  = 20.0f;
   [SerializeField] private float fadeDuration = .5f; // Used for both Fade In and Fade Out

   private Coroutine activeRoutine;

   public static TickerSystem Instance { get; set; }

   public enum MessageTypes {
      WorldEvent,
      ResultMessage
   }

   private void Awake() 
   {
      if (Instance != null && Instance != this)
         Destroy(this.gameObject);
      else 
      {
         Instance = this;
         DontDestroyOnLoad(this.gameObject);
      }

      wortldEventCanvasGroup.gameObject.SetActive(true);
      resultCanvasGroup.gameObject.SetActive(true);

      // Ensure alpha is 0 when the game boots up
      if (wortldEventCanvasGroup != null) 
         wortldEventCanvasGroup.alpha = 0;

      if (resultCanvasGroup!= null)
         resultCanvasGroup.alpha = 0;
      else
         Debug.Log("resultCanvasGroup is not set in the inspector");

      if(resultMessageText == null)
         Debug.Log("resultMessageText is not set in the inspector");

      gameObject.SetActive(false);
   }

   public void ShowTicker(string message, Color textColor,  MessageTypes currentMessageType) 
   {
      CanvasGroup     currentCanvasGroup;
      TextMeshProUGUI currentText;
        
      switch(currentMessageType) 
      {
         case MessageTypes.WorldEvent:
            currentCanvasGroup = wortldEventCanvasGroup;
            currentText        = worldEventMessageText; 
            break;
         case MessageTypes.ResultMessage:
            currentCanvasGroup = resultCanvasGroup;
            currentText        = resultMessageText;
            break;
         default:
            Debug.LogError("Unkown message type: " + currentMessageType);
            throw new Exception("Unkown message type: " + currentMessageType);
      }


      // 1. Force the object on so the coroutine runs
      gameObject.SetActive(true);

      currentText.text  = message;
      currentText.color = textColor;

      // 2. Start fully invisible
      wortldEventCanvasGroup.alpha = 0;

      // 3. Stop existing routines to prevent conflicts
      if (activeRoutine != null)
         StopCoroutine(activeRoutine);

      // 4. Start the full sequence (In -> Wait -> Out)
      activeRoutine = StartCoroutine(FadeSequence(currentCanvasGroup));
   }

   private IEnumerator FadeSequence(CanvasGroup currentCanvasGroup) 
   {
      float timer = 0;

      // --- STEP 1: FADE IN ---
      while (timer < fadeDuration) 
      {
         timer += Time.deltaTime;

         // Lerp from 0 to 1
         currentCanvasGroup.alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
         yield return null;
      }
      currentCanvasGroup.alpha = 1; // Ensure it ends at exactly 1

      // --- STEP 2: WAIT ---
      yield return new WaitForSeconds(timeVisible);

      // --- STEP 3: FADE OUT ---
      timer = 0; // Reset timer
      while (timer < fadeDuration) 
      {
         timer += Time.deltaTime;

         // Lerp from 1 to 0
         currentCanvasGroup.alpha = Mathf.Lerp(1, 0, timer / fadeDuration);
         yield return null;
      }
      currentCanvasGroup.alpha = 0; // Ensure it ends at exactly 0

      // Optional: Turn object off again
      gameObject.SetActive(false);
   }
}