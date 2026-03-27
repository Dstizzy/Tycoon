using System;
using System.Collections;
using UnityEngine;

// Controls the new narrative tutorial flow.
// Each step can:
// - show dialogue
// - move the tutorial panel to a preset layout
// - highlight one or more targets
// - optionally reward the player
public class NarrativeTutorialManager : MonoBehaviour
{
   public enum TutorialAdvanceMode
   {
      NextOnly,
      WaitForHoverTag
   }

   [System.Serializable]
   public class TutorialStep
   {
      public NPCEncounterSystem.NPCPersonality speaker;
      [TextArea] public string note;
      public NarrativeOverlayUI.DialogueLayoutMode layoutMode = NarrativeOverlayUI.DialogueLayoutMode.Center;
      public NPCEncounterSystem.DialogueLine[] lines;
      public HighlightTarget[] highlightTargets;
      public TutorialAdvanceMode advanceMode = TutorialAdvanceMode.NextOnly;
      public string requiredHoverTag;
      public int pearlRewardOnComplete;
   }

   [SerializeField] private TutorialStep[] tutorialSteps;

   private bool isTutorialRunning;

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

         NPCEncounterSystem.NPCProfile speakerProfile = null;
         if (NPCEncounterSystem.Instance != null)
            speakerProfile = NPCEncounterSystem.Instance.GetProfileByPersonality(currentStep.speaker);

         ShowHighlights(currentStep.highlightTargets);

         bool dialogueFinished = false;

         NarrativeOverlayUI.Instance.SetGameplayBlocked(true);
         NarrativeOverlayUI.Instance.PlaySequence(
            speakerProfile,
            currentStep.lines,
            currentStep.layoutMode,
            false,
            () => dialogueFinished = true);

         yield return new WaitUntil(() => dialogueFinished);

         if (currentStep.advanceMode == TutorialAdvanceMode.WaitForHoverTag &&
             string.IsNullOrEmpty(currentStep.requiredHoverTag) == false)
         {
            bool hoverSatisfied = false;

            void HandleHover(string currentTag)
            {
               if (currentTag == currentStep.requiredHoverTag)
                  hoverSatisfied = true;
            }

            PopUpManager.OnHoverTagChanged += HandleHover;
            NarrativeOverlayUI.Instance.SetGameplayBlocked(false);

            yield return new WaitUntil(() => hoverSatisfied);

            PopUpManager.OnHoverTagChanged -= HandleHover;
            NarrativeOverlayUI.Instance.SetGameplayBlocked(true);
         }

         if (currentStep.pearlRewardOnComplete > 0 && InventoryManager.Instance != null)
            InventoryManager.Instance.TryAddPearl(currentStep.pearlRewardOnComplete);

         HideHighlights(currentStep.highlightTargets);
      }

      isTutorialRunning = false;
      Debug.Log("[NarrativeTutorialManager] Tutorial finished.");
      onTutorialFinished?.Invoke();
   }

   private void ShowHighlights(HighlightTarget[] highlightTargets)
   {
      if (highlightTargets == null) return;

      foreach (HighlightTarget currentTarget in highlightTargets)
      {
         if (currentTarget != null)
            currentTarget.ShowHighlight();
      }
   }

   private void HideHighlights(HighlightTarget[] highlightTargets)
   {
      if (highlightTargets == null) return;

      foreach (HighlightTarget currentTarget in highlightTargets)
      {
         if (currentTarget != null)
            currentTarget.HideHighlight();
      }
   }

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
            NarrativeOverlayUI.DialogueLayoutMode.Right,
            new NPCEncounterSystem.DialogueLine[]
            {
               new NPCEncounterSystem.DialogueLine("First, your bag.", NPCEncounterSystem.ExpressionType.Neutral),
               new NPCEncounterSystem.DialogueLine("That’s where you check what you're carrying.", NPCEncounterSystem.ExpressionType.Thinking),
               new NPCEncounterSystem.DialogueLine("If you're ever unsure what you have, open the inventory before improvising. Please.", NPCEncounterSystem.ExpressionType.Special)
            }),

         CreateStep(
            "Resources",
            NarrativeOverlayUI.DialogueLayoutMode.Right,
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
            NarrativeOverlayUI.DialogueLayoutMode.Left,
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
         advanceMode = TutorialAdvanceMode.NextOnly,
         requiredHoverTag = string.Empty,
         highlightTargets = Array.Empty<HighlightTarget>(),
         pearlRewardOnComplete = pearlRewardOnComplete
      };
   }

   private void Awake()
   {
      if (tutorialSteps == null || tutorialSteps.Length == 0)
      {
         Debug.LogWarning("[NarrativeTutorialManager] tutorialSteps is empty. Creating default steps.");
         CreateSimplifiedTutorialSteps();
      }
   }
}