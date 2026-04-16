using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Runs the optional narrative tutorial after the intro flow finishes.
//
// High-level flow:
// 1. NarrativeFlowManager decides whether the player wants the tutorial.
// 2. If enabled, BeginTutorial() starts this manager.
// 3. Each TutorialStep is played in order using NarrativeOverlayUI.
// 4. Optional highlight targets are shown while the current step is active.
// 5. When all steps finish, control returns to normal gameplay.
//
// This class is intentionally focused on "scripted onboarding":
// - It does not manage building logic directly.
// - It does not create UI itself.
// - It simply sequences dialogue + highlights + optional rewards.
public class NarrativeTutorialManager : MonoBehaviour
{
   // Represents one tutorial step in the sequence.
   //
   // A step can contain:
   // - who is speaking
   // - which dialogue layout to use
   // - the actual dialogue lines
   // - optional world-space highlight targets
   // - optional UI highlight targets
   // - an optional pearl reward granted after the step finishes
   [System.Serializable]
   public class TutorialStep
   {
      public NPCEncounterSystem.NPCPersonality speaker;

      // Internal note for designers / developers.
      // Useful in the Inspector to identify the purpose of the step quickly.
      [TextArea] public string note;

      // Controls whether the dialogue appears at the bottom or top of the screen.
      public NarrativeOverlayUI.DialogueLayoutMode layoutMode = NarrativeOverlayUI.DialogueLayoutMode.StoryBottom;

      // Dialogue lines shown for this tutorial step.
      public NPCEncounterSystem.DialogueLine[] lines;

      // Optional world objects to highlight during this step.
      public HighlightTarget[] highlightTargets;

      // Optional UI elements to highlight during this step.
      public UIHighlightTarget[] uiHighlightTargets;

      // Optional pearl reward granted after this step completes.
      public int pearlRewardOnComplete;
   }

   // The ordered list of tutorial steps shown when the narrative tutorial is enabled.
   [SerializeField] private TutorialStep[] tutorialSteps;

   // Prevents the tutorial from being started multiple times simultaneously.
   private bool isTutorialRunning;

   // Ensures the tutorial always has content.
   // If no steps were configured in the Inspector, default steps are generated automatically.
   private void Awake()
   {
      if (tutorialSteps == null || tutorialSteps.Length == 0)
      {
         Debug.LogWarning("[NarrativeTutorialManager] tutorialSteps is empty. Creating default steps.");
         CreateSimplifiedTutorialSteps();
      }
   }

   // Public entry point used by NarrativeFlowManager.
   //
   // onTutorialFinished is invoked when:
   // - the full tutorial sequence finishes normally, or
   // - there are no valid tutorial steps to play.
   public void BeginTutorial(Action onTutorialFinished)
   {
      if (isTutorialRunning)
         return;

      if (tutorialSteps == null || tutorialSteps.Length == 0)
      {
         Debug.LogWarning("[NarrativeTutorialManager] No tutorial steps found. Completing immediately.");
         onTutorialFinished?.Invoke();
         return;
      }

      Debug.Log($"[NarrativeTutorialManager] Starting tutorial with {tutorialSteps.Length} steps.");
      StartCoroutine(RunTutorial(onTutorialFinished));
   }

   /*public void BeginWalkthrough(Action onWalkthroughFinished)
   {
      if (tutorialSteps == null || tutorialSteps.Length == 0)
      {
         Debug.LogWarning("[NarrativeTutorialManager] No tutorial steps found. Completing immediately.");
         onWalkthroughFinished?.Invoke();
         return;
      }

      Debug.Log($"[NarrativeTutorialManager] Starting tutorial with {tutorialSteps.Length} steps.");
      StartCoroutine(RunWalkthrough(onWalkthroughFinished));
   }*/

   /*public IEnumerator RunWalkthrough(Action onWalkthroughFinished)
   {
      isTutorialRunning = true;

      for (int stepIndex = 0; stepIndex < tutorialSteps.Length; stepIndex++)
      {
         TutorialStep currentStep = tutorialSteps[stepIndex];
         GameObject objectType = null;

         switch(currentStep.lines[currentStep.lines.Length - 1].ToString())
         {
            case "Click the next day button to get 10 of those beautiful pieces of ore":
               objectType = NextDayButton;
               objectType.GetComponent<Button>().interactable = true;
               break;
            case "Hover over this building and click the craft button that pops up.":
               objectType = ForgePanel;
               break;
            case "TradeHut":
               objectType = TradeHutPanel;
               break;
            case "ExplorationUnit":
               objectType = ExplorationUnitPanel;
               break;
            case "Lab":
               objectType = LabPanel;
               break;
         }

         if(objectType != null)
            Debug.Log(objectType.ToString());

         if (currentStep == null)
         {
            Debug.LogWarning($"[NarrativeTutorialManager] Step {stepIndex} is null. Skipping.");
            continue;
         }

         Debug.Log($"[NarrativeTutorialManager] Playing tutorial step {stepIndex}: {currentStep.note}");

         // Resolve the speaker portrait/profile from the NPC encounter database.
         NPCEncounterSystem.NPCProfile speakerProfile = null;
         if (NPCEncounterSystem.Instance != null)
            speakerProfile = NPCEncounterSystem.Instance.GetProfileByPersonality(currentStep.speaker);

         // Turn on any requested highlights before the dialogue starts.
         ShowHighlights(currentStep.highlightTargets);
         ShowUIHighlights(currentStep.uiHighlightTargets);

         bool dialogueFinished = false;

         // While tutorial dialogue is playing, gameplay input should remain blocked.
         NarrativeOverlayUI.Instance.SetGameplayBlocked(true);
         NarrativeOverlayUI.Instance.PlayWalkthroughSequence(
            speakerProfile,
            currentStep.lines,
            currentStep.layoutMode,
            false,
            objectType != null,
            objectType,
            () => dialogueFinished = true);

         // Wait until the overlay reports that the dialogue has finished.
         yield return new WaitUntil(() => dialogueFinished);

         // Optional step reward.
         if (currentStep.pearlRewardOnComplete > 0 && InventoryManager.Instance != null)
            InventoryManager.Instance.TryAddPearl(currentStep.pearlRewardOnComplete);

         // Clean up highlights before moving to the next tutorial step.
         HideHighlights(currentStep.highlightTargets);
         HideUIHighlights(currentStep.uiHighlightTargets);

         NarrativeOverlayUI.Instance.SetGameplayBlocked(false);
      }

      isTutorialRunning = false;
      Debug.Log("[NarrativeTutorialManager] Tutorial finished.");
      onWalkthroughFinished?.Invoke();
   }*/

   // Main tutorial coroutine.
   //
   // Plays each step in order:
   // - resolve speaker profile
   // - show highlights
   // - block gameplay
   // - play dialogue
   // - wait until dialogue completes
   // - grant optional rewards
   // - hide highlights
   //
   // When all steps are done, the completion callback is fired.
   private IEnumerator RunTutorial(Action onTutorialFinished)
   {
      isTutorialRunning = true;

      for (int stepIndex = 0; stepIndex < tutorialSteps.Length; stepIndex++)
      {
         TutorialStep currentStep = tutorialSteps[stepIndex];
         if (currentStep == null)
         {
            Debug.LogWarning($"[NarrativeTutorialManager] Step {stepIndex} is null. Skipping.");
            continue;
         }

         Debug.Log($"[NarrativeTutorialManager] Playing tutorial step {stepIndex}: {currentStep.note}");

         // Resolve the speaker portrait/profile from the NPC encounter database.
         NPCEncounterSystem.NPCProfile speakerProfile = null;
         if (NPCEncounterSystem.Instance != null)
            speakerProfile = NPCEncounterSystem.Instance.GetProfileByPersonality(currentStep.speaker);

         // Turn on any requested highlights before the dialogue starts.
         ShowHighlights(currentStep.highlightTargets);
         ShowUIHighlights(currentStep.uiHighlightTargets);

         bool dialogueFinished = false;

         // While tutorial dialogue is playing, gameplay input should remain blocked.
         NarrativeOverlayUI.Instance.SetGameplayBlocked(true);
         NarrativeOverlayUI.Instance.PlaySequence(
            speakerProfile,
            currentStep.lines,
            currentStep.layoutMode,
            false,
            () => dialogueFinished = true);

         // Wait until the overlay reports that the dialogue has finished.
         yield return new WaitUntil(() => dialogueFinished);

         // Optional step reward.
         if (currentStep.pearlRewardOnComplete > 0 && InventoryManager.Instance != null)
            InventoryManager.Instance.TryAddPearl(currentStep.pearlRewardOnComplete);

         // Clean up highlights before moving to the next tutorial step.
         HideHighlights(currentStep.highlightTargets);
         HideUIHighlights(currentStep.uiHighlightTargets);
         NarrativeOverlayUI.Instance.SetGameplayBlocked(false);
      }

      isTutorialRunning = false;
      Debug.Log("[NarrativeTutorialManager] Tutorial finished.");
      onTutorialFinished?.Invoke();
   }

   // Shows all world highlight targets for the current step.
   private void ShowHighlights(HighlightTarget[] highlightTargets)
   {
      if (highlightTargets == null) return;

      foreach (HighlightTarget currentTarget in highlightTargets)
      {
         if (currentTarget != null)
            currentTarget.ShowHighlight();
      }
   }

   // Hides all world highlight targets for the current step.
   private void HideHighlights(HighlightTarget[] highlightTargets)
   {
      if (highlightTargets == null) return;

      foreach (HighlightTarget currentTarget in highlightTargets)
      {
         if (currentTarget != null)
            currentTarget.HideHighlight();
      }
   }

   // Generates a default tutorial sequence directly in code.
   //
   // This is useful when:
   // - no tutorial steps were set up in the Inspector yet
   // - the scene is missing serialized tutorial content
   // - developers need a reliable fallback during iteration
   [ContextMenu("Create Simplified Narrative Tutorial Steps")]
   private void CreateSimplifiedTutorialSteps()
   {
      tutorialSteps = new TutorialStep[]
      {
         CreateStep(
            "Welcome",
            NarrativeOverlayUI.DialogueLayoutMode.StoryBottom,
            new NPCEncounterSystem.DialogueLine[]
            {
               new NPCEncounterSystem.DialogueLine("Hi. Welcome. Sorry, I should sound calmer than this.", NPCEncounterSystem.ExpressionType.Happy),
               new NPCEncounterSystem.DialogueLine("A quick breath. A strained but professional smile.", NPCEncounterSystem.ExpressionType.Special, isAction: true),
               new NPCEncounterSystem.DialogueLine("I'm Dolphin. I keep this place running, or at least politely prevent it from collapsing.", NPCEncounterSystem.ExpressionType.Thinking),
               new NPCEncounterSystem.DialogueLine("Here. Take a little emergency fund before we start.", NPCEncounterSystem.ExpressionType.Happy)
            },
            pearlRewardOnComplete: 20),

         CreateStep(
            "Inventory",
            NarrativeOverlayUI.DialogueLayoutMode.Top,
            new NPCEncounterSystem.DialogueLine[]
            {
               new NPCEncounterSystem.DialogueLine("First, your bag.", NPCEncounterSystem.ExpressionType.Neutral),
               new NPCEncounterSystem.DialogueLine("That’s where you check what you're carrying.", NPCEncounterSystem.ExpressionType.Thinking),
               new NPCEncounterSystem.DialogueLine("If you're ever unsure what you have, open the inventory before improvising. Please.", NPCEncounterSystem.ExpressionType.Special)
            }),

         CreateStep(
            "Resources",
            NarrativeOverlayUI.DialogueLayoutMode.Top,
            new NPCEncounterSystem.DialogueLine[]
            {
               new NPCEncounterSystem.DialogueLine("Up there, you can read your current resources.", NPCEncounterSystem.ExpressionType.Neutral),
               new NPCEncounterSystem.DialogueLine("Pearls are the premium asset. Ore matters too, but pearls are what really push things forward.", NPCEncounterSystem.ExpressionType.Thinking),
               new NPCEncounterSystem.DialogueLine("If something turns ore into pearls, it deserves your attention.", NPCEncounterSystem.ExpressionType.Happy)
            }),

         CreateStep(
            "Turn Counter",
            NarrativeOverlayUI.DialogueLayoutMode.StoryBottom,
            new NPCEncounterSystem.DialogueLine[]
            {
               new NPCEncounterSystem.DialogueLine("That counter tracks how far you've gone.", NPCEncounterSystem.ExpressionType.Neutral),
               new NPCEncounterSystem.DialogueLine("And how close everything is to becoming a very expensive volcanic cautionary tale.", NPCEncounterSystem.ExpressionType.Special),
               new NPCEncounterSystem.DialogueLine("So yes, time matters.", NPCEncounterSystem.ExpressionType.Thinking)
            }),

         CreateStep(
            "Enemy Gauge",
            NarrativeOverlayUI.DialogueLayoutMode.Top,
            new NPCEncounterSystem.DialogueLine[]
            {
               new NPCEncounterSystem.DialogueLine("That gauge is bad news, in a nicely organized format.", NPCEncounterSystem.ExpressionType.Neutral),
               new NPCEncounterSystem.DialogueLine("It means a troublesome visitor is getting closer.", NPCEncounterSystem.ExpressionType.Thinking),
               new NPCEncounterSystem.DialogueLine("When that fills up, the day tends to become memorable in the wrong way.", NPCEncounterSystem.ExpressionType.Special)
            }),

         CreateStep(
            "Next Day Button",
            NarrativeOverlayUI.DialogueLayoutMode.Top,
            new NPCEncounterSystem.DialogueLine[]
            {
               new NPCEncounterSystem.DialogueLine("And that button sends you into the next day.", NPCEncounterSystem.ExpressionType.Neutral),
               new NPCEncounterSystem.DialogueLine("Useful, dangerous, unavoidable — just like scheduling.", NPCEncounterSystem.ExpressionType.Special),
               new NPCEncounterSystem.DialogueLine("Try not to press it before you're ready.", NPCEncounterSystem.ExpressionType.Happy)
            }),

         CreateStep(
            "Buildings Overview",
            NarrativeOverlayUI.DialogueLayoutMode.StoryBottom,
            new NPCEncounterSystem.DialogueLine[]
            {
               new NPCEncounterSystem.DialogueLine("Now, the buildings.", NPCEncounterSystem.ExpressionType.Neutral),
               new NPCEncounterSystem.DialogueLine("These five keep the whole settlement moving.", NPCEncounterSystem.ExpressionType.Happy),
               new NPCEncounterSystem.DialogueLine("Go in, look around, and say hello to the residents when you can.", NPCEncounterSystem.ExpressionType.Thinking),
               new NPCEncounterSystem.DialogueLine("They all have opinions. Some of them are even useful.", NPCEncounterSystem.ExpressionType.Special)
            }),

         CreateStep(
            "NPC Hint",
            NarrativeOverlayUI.DialogueLayoutMode.StoryBottom,
            new NPCEncounterSystem.DialogueLine[]
            {
               new NPCEncounterSystem.DialogueLine("One last thing.", NPCEncounterSystem.ExpressionType.Thinking),
               new NPCEncounterSystem.DialogueLine("If you spot someone out on the map and stop to talk, something unusual might happen.", NPCEncounterSystem.ExpressionType.Happy),
               new NPCEncounterSystem.DialogueLine("Could be helpful. Could be expensive. That's community life.", NPCEncounterSystem.ExpressionType.Special),
               new NPCEncounterSystem.DialogueLine("Anyway, I really do need to get back to work. Good luck.", NPCEncounterSystem.ExpressionType.Happy)
            })
      };
   }

   // Convenience factory used when generating the default tutorial steps in code.
   // All generated steps currently use Dolphin as the tutorial narrator.
   private TutorialStep CreateStep(
      string note,
      NarrativeOverlayUI.DialogueLayoutMode layoutMode,
      NPCEncounterSystem.DialogueLine[] lines,
      int pearlRewardOnComplete = 0)
   {
      return new TutorialStep
      {
         speaker = NPCEncounterSystem.NPCPersonality.Dolphin,
         note = note,
         layoutMode = layoutMode,
         lines = lines,
         highlightTargets = Array.Empty<HighlightTarget>(),
         uiHighlightTargets = Array.Empty<UIHighlightTarget>(),
         pearlRewardOnComplete = pearlRewardOnComplete
      };
   }

   // Shows all UI highlight targets for the current step.
   private void ShowUIHighlights(UIHighlightTarget[] uiHighlightTargets)
   {
      if (uiHighlightTargets == null) return;

      foreach (UIHighlightTarget currentTarget in uiHighlightTargets)
      {
         if (currentTarget != null)
            currentTarget.ShowHighlight();
      }
   }

   // Hides all UI highlight targets for the current step.
   private void HideUIHighlights(UIHighlightTarget[] uiHighlightTargets)
   {
      if (uiHighlightTargets == null) return;

      foreach (UIHighlightTarget currentTarget in uiHighlightTargets)
      {
         if (currentTarget != null)
            currentTarget.HideHighlight();
      }
   }
}