using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIFlash : MonoBehaviour
{
   private Image overlayImage;

   void Awake()
   {
      overlayImage = GetComponent<Image>();
   }

   public IEnumerator FlashRed(float duration, float maxAlpha)
   {
      float elapsed = 0f;
      float halfDuration = duration / 2f;

      // Fade In
      while (elapsed < halfDuration)
      {
         elapsed += Time.deltaTime;
         float alpha = Mathf.Lerp(0, maxAlpha, elapsed / halfDuration);
         overlayImage.color = new Color(1, 0, 0, alpha);
         yield return null;
      }

      elapsed = 0f;

      // Fade Out
      while (elapsed < halfDuration)
      {
         elapsed += Time.deltaTime;
         float alpha = Mathf.Lerp(maxAlpha, 0, elapsed / halfDuration);
         overlayImage.color = new Color(1, 0, 0, alpha);
         yield return null;
      }

      // Ensure it's fully transparent at the end
      overlayImage.color = new Color(1, 0, 0, 0);
   }
}