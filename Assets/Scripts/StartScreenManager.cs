using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
   // Must assign these slots in the Inspector.
   public Animator FadePanelAnimator;
   public float FadeDuration = 0.5f;

   // Called by the START button.
   public void OnStartButtonClick()
   {
      // Change "MainScene" to your actual game scene name. 
      StartCoroutine(FadeAndLoadScene("MainScene"));
   }

   // Info button (Not implemented yet)
   public void OnInfoButtonClick()
   {
      Debug.Log("Info button clicked!");
   }

   // Settings button (Not implemented yet)
   public void OnSettingsButtonClick()
   {
      Debug.Log("Settings button clicked!");
   }

   // Coroutine to execute fade-out and load the scene.
   private IEnumerator FadeAndLoadScene(string sceneName)
   {
      // 1. Trigger the "StartFadeOut" animation.
      FadePanelAnimator.SetTrigger("StartFadeOut");

      // 2. Wait for the animation to finish (FadeDuration seconds).
      yield return new WaitForSeconds(FadeDuration);

      // 3. Load the scene.
      SceneManager.LoadScene(sceneName);
   }
}