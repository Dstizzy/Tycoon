using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Entry point for the main-scene narrative onboarding flow.
//
// Execution order:
// 1. Main scene finishes loading.
// 2. Dolphin intro dialogue plays over a black screen.
// 3. The player chooses whether to enable the narrative tutorial.
// 4. If enabled, NarrativeTutorialManager runs the step-by-step tutorial.
// 5. Gameplay input is restored.
//
// This class does not implement tutorial content itself.
// It only coordinates the startup flow and hands off to other systems.
public class WalkThroughFlowManager : MonoBehaviour
{
   [Header("Dependencies")]
   [SerializeField] private TutorialManager tutorialManager;

   // Runs automatically when the scene starts.
   // Handles intro dialogue, tutorial toggle, optional tutorial handoff, and cleanup.
   private IEnumerator Start()
   {
      yield return null;

      //NarrativeOverlayUI.Instance.SetGameplayBlocked(true);


      NPCEncounterSystem.NPCProfile dolphinProfile = null;
      if (NPCEncounterSystem.Instance != null)
         dolphinProfile = NPCEncounterSystem.Instance.GetProfileByPersonality(NPCEncounterSystem.NPCPersonality.Dolphin);

      bool playTutorial = true;

      // Hand off to the main narrative tutorial if enabled.
      if (playTutorial)
      {
         if (tutorialManager != null)
         {
            Debug.Log("[WalkThroughFlowManager] Starting narrative tutorial.");

            bool walkthroughFinished = false;
            tutorialManager.BeginWalkthrough(() => walkthroughFinished = true);
            yield return new WaitUntil(() => walkthroughFinished);
         }
         else
         {
            Debug.LogWarning("[WalkThroughFlowManager] narrativeTutorialManager reference is missing.");
         }
      }

      // Final cleanup: hide overlay UI and restore gameplay input.
      NarrativeOverlayUI.Instance.HideAll();
      TutorialManager.Instance.SetGameplayBlocked(false);
      SceneManager.LoadScene("MainScene");
   }
}