using UnityEngine;
using System.Collections;

public class UIFade : MonoBehaviour
{
   private CanvasGroup canvasGroup;
   private Coroutine currentFade;

   void Awake()
   {
      canvasGroup = GetComponent<CanvasGroup>();
   }

   public void Appear(float duration)
   {
      gameObject.SetActive(true);

      // Stop any fade-outs currently happening
      if (currentFade != null) StopCoroutine(currentFade);
      currentFade = StartCoroutine(FadeTo(1f, duration));
   }

   public void Disappear(float duration)
   {
      if (!gameObject.activeInHierarchy) return;

      if (currentFade != null) StopCoroutine(currentFade);
      currentFade = StartCoroutine(FadeTo(0f, duration));
   }

   IEnumerator FadeTo(float targetAlpha, float duration)
   {
      float startAlpha = canvasGroup.alpha;
      float elapsed = 0f;

      while (elapsed < duration)
      {
         elapsed += Time.deltaTime;
         canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
         yield return null;
      }

      canvasGroup.alpha = targetAlpha;

      if (targetAlpha == 0f)
      {
         gameObject.SetActive(false);
      }
   }
}