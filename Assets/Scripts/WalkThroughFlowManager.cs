using System.Collections;
using UnityEngine;

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
   [SerializeField] private NarrativeTutorialManager narrativeTutorialManager;

   // Runs automatically when the scene starts.
   // Handles intro dialogue, tutorial toggle, optional tutorial handoff, and cleanup.
   private IEnumerator Start()
   {
      yield return null;

      if (NarrativeOverlayUI.Instance == null)
      {
         Debug.LogError("[WalkThroughFlowManager] NarrativeOverlayUI instance is missing.");
         yield break;
      }

      NarrativeOverlayUI.Instance.SetGameplayBlocked(true);


      NPCEncounterSystem.NPCProfile dolphinProfile = null;
      if (NPCEncounterSystem.Instance != null)
         dolphinProfile = NPCEncounterSystem.Instance.GetProfileByPersonality(NPCEncounterSystem.NPCPersonality.Dolphin);

      bool introFinished = false;


      yield return new WaitUntil(() => introFinished);

      bool toggleAnswered = false;
      bool playTutorial = TutorialFlowSettings.NarrativeTutorialEnabled;

      // Ask the player whether they want the tutorial enabled.
      NarrativeOverlayUI.Instance.ShowTutorialToggle(currentChoice =>
      {
         playTutorial = currentChoice;
         toggleAnswered = true;
      });

      yield return new WaitUntil(() => toggleAnswered);

      bool fadeFinished = false;
      NarrativeOverlayUI.Instance.FadeTo(0f, 0.4f, () => fadeFinished = true);
      yield return new WaitUntil(() => fadeFinished);

      // Hand off to the main narrative tutorial if enabled.
      if (playTutorial)
      {
         if (narrativeTutorialManager != null)
         {
            Debug.Log("[WalkThroughFlowManager] Starting narrative tutorial.");

            bool tutorialFinished = false;
            narrativeTutorialManager.BeginTutorial(() => tutorialFinished = true);
            yield return new WaitUntil(() => tutorialFinished);
         }
         else
         {
            Debug.LogWarning("[WalkThroughFlowManager] narrativeTutorialManager reference is missing.");
         }
      }

      // Final cleanup: hide overlay UI and restore gameplay input.
      NarrativeOverlayUI.Instance.HideAll();
      NarrativeOverlayUI.Instance.SetGameplayBlocked(false);
   }
}