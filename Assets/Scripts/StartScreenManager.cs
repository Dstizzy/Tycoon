using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
   /* These slots must be assigned in the Inspector */
   public Animator FadePanelAnimator;
   public float FadeDuration = 0.5f;

   // A function that executes when the START button is clicked.
   public void OnStartButtonClick()
   {
      // The "MainScene" string must be changed to the actual scene name.
      StartCoroutine(FadeAndLoadScene("MainScene"));
   }

   // Info button (Not implemented)
   public void OnInfoButtonClick()
   {
      Debug.Log("Info button clicked!");
   }

   // Settings button (Not implemented)
   public void OnSettingsButtonClick()
   {
      Debug.Log("Settings button clicked!");
   }

   // Coroutine to execute the fade-out and load the scene.
   private IEnumerator FadeAndLoadScene(string sceneName)
   {
      // 1. Send the "StartFadeOut" signal to the animator.
      FadePanelAnimator.SetTrigger("StartFadeOut");

      // 2. Wait until the animation finishes (FadeDuration seconds).
      yield return new WaitForSeconds(FadeDuration);

      // 3. Load the scene.
      SceneManager.LoadScene(sceneName);
   }
}