using UnityEngine;

public class PanelManager : MonoBehaviour
{
   // We'll use a Dictionary to store the original scales of multiple panels
   // since your LabManager passes in different GameObjects.
   private System.Collections.Generic.Dictionary<GameObject, Vector3> originalScales =
       new System.Collections.Generic.Dictionary<GameObject, Vector3>();

   public float animationTime = 0.4f;

   public void OpenPanel(GameObject panel)
   {
      if (panel == null) return;

      // 1. If we haven't recorded this panel's "correct" size yet, do it now
      if (!originalScales.ContainsKey(panel))
      {
         originalScales.Add(panel, panel.transform.localScale);
      }

      // 2. Stop any current animations and reset to 0
      LeanTween.cancel(panel);
      panel.transform.localScale = Vector3.zero;
      panel.SetActive(true);

      // 3. Scale up to the ORIGINAL scale we recorded
      LeanTween.scale(panel, originalScales[panel], animationTime)
          .setEaseOutBack()
          .setIgnoreTimeScale(true);
   }

   public void ClosePanel(GameObject panel)
   {
      if (panel == null) return;

      LeanTween.cancel(panel);

      LeanTween.scale(panel, Vector3.zero, animationTime)
          .setEaseInBack()
          .setIgnoreTimeScale(true)
          .setOnComplete(() => {
             panel.SetActive(false);
          });
   }
}