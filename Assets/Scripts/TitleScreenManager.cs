using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
   public Animator FadePanelAnimator;   /* The Animator for the FadePanel */
   public float    FadeDuration = 0.5f; /* The duration of the fade-out animation in seconds */

   // A function that executes when the user clicks the "START" button.
   public void OnStartButtonClick()
   {
      StartCoroutine(FadeAndLoadScene("MainScene"));
   }

   // A function that executes when the user clicks the "INFO" button.
   public void OnInfoButtonClick()
   {
      Debug.Log("Info button clicked!");
      // TODO: Code to display the information (Info) pop-up window
   }

   // A function that executes when the user clicks the "SETTINGS" button.
   public void OnSettingsButtonClick()
   {
      Debug.Log("Settings button clicked!");
      // TODO: Code to display the settings (Settings) pop-up window
   }

   private IEnumerator FadeAndLoadScene(string sceneName)
   {
      // 1. Run the animation trigger
      FadePanelAnimator.SetTrigger("StartFadeOut");

      // 2. Wait for the animation to finish
      yield return new WaitForSeconds(FadeDuration);

      // 3. Load the scene
      SceneManager.LoadScene(sceneName);
   }
}