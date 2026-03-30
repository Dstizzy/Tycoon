using System.Collections;
using UnityEngine;

// Entry point for the main-scene narrative flow.
// Order:
// 1. Main scene loads
// 2. Black screen intro with Dolphin
// 3. Tutorial on/off toggle
// 4. Optional narrative tutorial
// 5. Gameplay starts
public class NarrativeFlowManager : MonoBehaviour
{
   [Header("Dependencies")]
   [SerializeField] private NarrativeTutorialManager narrativeTutorialManager;

   private readonly NPCEncounterSystem.DialogueLine[] introLines =
   {
      new NPCEncounterSystem.DialogueLine("...Hello? Can you hear me?", NPCEncounterSystem.ExpressionType.Neutral),
      new NPCEncounterSystem.DialogueLine("The line crackles softly.", NPCEncounterSystem.ExpressionType.Special, isAction: true),
      new NPCEncounterSystem.DialogueLine("Right. Good. We finally got through.", NPCEncounterSystem.ExpressionType.Thinking),
      new NPCEncounterSystem.DialogueLine("I'm Dolphin, your on-site coordinator.", NPCEncounterSystem.ExpressionType.Happy),
      new NPCEncounterSystem.DialogueLine("Things are already... a little behind schedule.", NPCEncounterSystem.ExpressionType.Special),
      new NPCEncounterSystem.DialogueLine("I'll walk you through the basics if you want.", NPCEncounterSystem.ExpressionType.Happy),
   };

   private IEnumerator Start()
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

      NarrativeOverlayUI.Instance.PlaySequence(
         dolphinProfile,
         introLines,
         NarrativeOverlayUI.DialogueLayoutMode.StoryBottom,
         true,
         () => introFinished = true);

      yield return new WaitUntil(() => introFinished);

      bool toggleAnswered = false;
      bool playTutorial = TutorialFlowSettings.NarrativeTutorialEnabled;

      NarrativeOverlayUI.Instance.ShowTutorialToggle(currentChoice =>
      {
         playTutorial = currentChoice;
         toggleAnswered = true;
      });

      yield return new WaitUntil(() => toggleAnswered);

      bool fadeFinished = false;
      NarrativeOverlayUI.Instance.FadeTo(0f, 0.4f, () => fadeFinished = true);
      yield return new WaitUntil(() => fadeFinished);

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

      NarrativeOverlayUI.Instance.HideAll();
      NarrativeOverlayUI.Instance.SetGameplayBlocked(false);
   }
}