using System.Collections.Generic;
using UnityEngine;

// Shows one-time building explanations after the player opens a building panel.
// This guide only runs when the narrative tutorial toggle is enabled.
public class BuildingTutorialGuideManager : MonoBehaviour
{
   public static BuildingTutorialGuideManager Instance { get; private set; }

   private readonly HashSet<string> explainedBuildings = new HashSet<string>();

   private void Awake()
   {
      if (Instance != null && Instance != this)
      {
         Destroy(gameObject);
         return;
      }

      Instance = this;
   }

   public void TryShowBuildingGuide(string buildingTag)
   {
      if (TutorialFlowSettings.NarrativeTutorialEnabled == false)
         return;

      if (string.IsNullOrEmpty(buildingTag))
         return;

      if (explainedBuildings.Contains(buildingTag))
         return;

      if (NarrativeOverlayUI.Instance == null || NarrativeOverlayUI.Instance.IsBusy())
         return;

      NPCEncounterSystem.NPCProfile speakerProfile = GetSpeakerProfile(buildingTag);
      NPCEncounterSystem.DialogueLine[] guideLines = GetGuideLines(buildingTag);

      if (speakerProfile == null || guideLines == null || guideLines.Length == 0)
         return;

      explainedBuildings.Add(buildingTag);

      NarrativeOverlayUI.Instance.PlaySequence(
         speakerProfile,
         guideLines,
         GetLayoutMode(buildingTag),
         false,
         () => { });
   }

   private NPCEncounterSystem.NPCProfile GetSpeakerProfile(string buildingTag)
   {
      if (NPCEncounterSystem.Instance == null)
         return null;

      return buildingTag switch
      {
         "Trade Hut" => NPCEncounterSystem.Instance.GetProfileByPersonality(NPCEncounterSystem.NPCPersonality.HermitCrab),
         "Lab" => NPCEncounterSystem.Instance.GetProfileByPersonality(NPCEncounterSystem.NPCPersonality.Turtle),
         "Ore Refinery" => NPCEncounterSystem.Instance.GetProfileByPersonality(NPCEncounterSystem.NPCPersonality.Jellyfish),
         "Exploration Unit" => NPCEncounterSystem.Instance.GetProfileByPersonality(NPCEncounterSystem.NPCPersonality.Seahorse),
         "Forge" => NPCEncounterSystem.Instance.GetProfileByPersonality(NPCEncounterSystem.NPCPersonality.Octopus),
         _ => null
      };
   }

   private NarrativeOverlayUI.DialogueLayoutMode GetLayoutMode(string buildingTag)
   {
      return buildingTag switch
      {
         "Trade Hut" => NarrativeOverlayUI.DialogueLayoutMode.Right,
         "Lab" => NarrativeOverlayUI.DialogueLayoutMode.Left,
         "Ore Refinery" => NarrativeOverlayUI.DialogueLayoutMode.Right,
         "Exploration Unit" => NarrativeOverlayUI.DialogueLayoutMode.Left,
         "Forge" => NarrativeOverlayUI.DialogueLayoutMode.Top,
         _ => NarrativeOverlayUI.DialogueLayoutMode.Center
      };
   }

   private NPCEncounterSystem.DialogueLine[] GetGuideLines(string buildingTag)
   {
      return buildingTag switch
      {
         "Trade Hut" => new NPCEncounterSystem.DialogueLine[]
         {
            new NPCEncounterSystem.DialogueLine("Ah. A customer.", NPCEncounterSystem.ExpressionType.Neutral),
            new NPCEncounterSystem.DialogueLine("Welcome to the Trade Hut. Keep your numbers tidy and your expectations realistic.", NPCEncounterSystem.ExpressionType.Thinking),
            new NPCEncounterSystem.DialogueLine("If profit is possible, I'll notice before you do.", NPCEncounterSystem.ExpressionType.Happy)
         },

         "Lab" => new NPCEncounterSystem.DialogueLine[]
         {
            new NPCEncounterSystem.DialogueLine("Oh! Great timing!", NPCEncounterSystem.ExpressionType.Happy),
            new NPCEncounterSystem.DialogueLine("Welcome to the Lab! Things explode less often than the rumors say.", NPCEncounterSystem.ExpressionType.Special),
            new NPCEncounterSystem.DialogueLine("No promises, though.", NPCEncounterSystem.ExpressionType.Happy)
         },

         "Ore Refinery" => new NPCEncounterSystem.DialogueLine[]
         {
            new NPCEncounterSystem.DialogueLine("...Hi...", NPCEncounterSystem.ExpressionType.Neutral),
            new NPCEncounterSystem.DialogueLine("...This place is warm... and shiny...", NPCEncounterSystem.ExpressionType.Happy),
            new NPCEncounterSystem.DialogueLine("...I like it here...", NPCEncounterSystem.ExpressionType.Thinking)
         },

         "Exploration Unit" => new NPCEncounterSystem.DialogueLine[]
         {
            new NPCEncounterSystem.DialogueLine("You're here.", NPCEncounterSystem.ExpressionType.Neutral),
            new NPCEncounterSystem.DialogueLine("Good. This is where we plan routes and pretend uncertainty is strategy.", NPCEncounterSystem.ExpressionType.Thinking),
            new NPCEncounterSystem.DialogueLine("If you're lucky, it works.", NPCEncounterSystem.ExpressionType.Special)
         },

         "Forge" => new NPCEncounterSystem.DialogueLine[]
         {
            new NPCEncounterSystem.DialogueLine("Hey! Welcome!", NPCEncounterSystem.ExpressionType.Happy),
            new NPCEncounterSystem.DialogueLine("This is the Forge — loud, hot, and productive. My kind of place.", NPCEncounterSystem.ExpressionType.Special),
            new NPCEncounterSystem.DialogueLine("If it can be made better, I’ll probably make it better.", NPCEncounterSystem.ExpressionType.Happy)
         },

         _ => null
      };
   }
}