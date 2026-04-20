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
public class NarrativeFlowManager : MonoBehaviour
{
   [Header("Dependencies")]
   [SerializeField] private NarrativeTutorialManager narrativeTutorialManager;

   private static bool tutorialEnabled = true;

   // Intro dialogue played once at the start of the scene.
   private readonly NPCEncounterSystem.DialogueLine[] introLines =
   {
      new NPCEncounterSystem.DialogueLine("...Hello? Can you hear me?", NPCEncounterSystem.ExpressionType.Neutral),
      new NPCEncounterSystem.DialogueLine("The line crackles softly.", NPCEncounterSystem.ExpressionType.Special, isAction: true),
      new NPCEncounterSystem.DialogueLine("Right. Good. We finally got through.", NPCEncounterSystem.ExpressionType.Thinking),
      new NPCEncounterSystem.DialogueLine("I'm Steaman, your on-site coordinator.", NPCEncounterSystem.ExpressionType.Happy),
      new NPCEncounterSystem.DialogueLine("We have a serious problem. The volcano is close to blowing, and when it does, this whole place goes with it.", NPCEncounterSystem.ExpressionType.Special),
      new NPCEncounterSystem.DialogueLine("That gives us 80 days to finish the submarine and get everyone out before this turns into a cautionary tale.", NPCEncounterSystem.ExpressionType.Thinking),
      new NPCEncounterSystem.DialogueLine("Things are already... a little behind schedule.", NPCEncounterSystem.ExpressionType.Special),
      new NPCEncounterSystem.DialogueLine("I'll walk you through the basics if you want.", NPCEncounterSystem.ExpressionType.Happy),
   };

   public void OnEnable()
   {
      WalkThroughFlowManager.disableTutorial += DisableTutorial;
   }

   public void OnDisable()
   {
      WalkThroughFlowManager.disableTutorial -= DisableTutorial;
   }

   public void Start()
   {
      if(tutorialEnabled)
         StartCoroutine(StartTutorial());
   }

   public void DisableTutorial()
   {
      tutorialEnabled = false;
      NarrativeOverlayUI.Instance.SetFadeImmediate(0f);
   }

   // Runs automatically when the scene starts.
   // Handles intro dialogue, tutorial toggle, optional tutorial handoff, and cleanup.
   private IEnumerator StartTutorial()
   {
      if(tutorialEnabled)
      {
      yield return null;

      if (NarrativeOverlayUI.Instance == null)
      {
         Debug.LogError("[NarrativeFlowManager] NarrativeOverlayUI instance is missing.");
         yield break;
      }

      NarrativeOverlayUI.Instance.SetGameplayBlocked(true);
      NarrativeOverlayUI.Instance.SetFadeImmediate(1f);


      NPCEncounterSystem.NPCProfile dolphinProfile = null;
      if (NPCEncounterSystem.Instance != null)
         dolphinProfile = NPCEncounterSystem.Instance.GetProfileByPersonality(NPCEncounterSystem.NPCPersonality.Dolphin);

      bool introFinished = false;

      // Play the opening intro sequence.
      NarrativeOverlayUI.Instance.PlaySequence(
         dolphinProfile,
         introLines,
         NarrativeOverlayUI.DialogueLayoutMode.StoryBottom,
         true,
         () => introFinished = true);

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
            Debug.Log("[NarrativeFlowManager] Starting narrative tutorial.");

            bool tutorialFinished = false;
            narrativeTutorialManager.BeginTutorial(() => tutorialFinished = true);
            yield return new WaitUntil(() => tutorialFinished);
         }
         else
         {
            Debug.LogWarning("[NarrativeFlowManager] narrativeTutorialManager reference is missing.");
         }
      }

      // Final cleanup: hide overlay UI and restore gameplay input.
      NarrativeOverlayUI.Instance.HideAll();
      NarrativeOverlayUI.Instance.SetGameplayBlocked(false);
      tutorialEnabled = false;
   }
   }
}