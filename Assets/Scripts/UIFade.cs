using UnityEngine;
using System.Collections;

public class UIFade : MonoBehaviour
{
   private CanvasGroup canvasGroup;

   private float elapsed = 0f;

   void Awake()
   {
      // Get the Canvas Group component we added
      canvasGroup = GetComponent<CanvasGroup>();
   }

   // Call this function to start the fade
   public void Appear(float duration)
   {
      gameObject.SetActive(true);
      StartCoroutine(FadeIn(duration));
   }

   public void Disappear(float duration) 
   {
      StartCoroutine(FadeOut(duration));
      gameObject.SetActive(false);
   }

   IEnumerator FadeIn(float duration)
   {
      elapsed = 0f;

      while (elapsed < duration)
      {
         elapsed += Time.deltaTime;
         // Linearly move Alpha from 0 to 1 based on time
         canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
         yield return null;
      }

      canvasGroup.alpha = 1f; // Ensure it ends perfectly visible
   }

   IEnumerator FadeOut(float duration)
   {
      elapsed = 0f;

      while (elapsed < duration)
      {
         elapsed += Time.deltaTime;
         // Linearly move Alpha from 0 to 1 based on time
         canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
         yield return null;
      }

      canvasGroup.alpha = 0f; // Ensure it ends perfectly invisible
   }
}