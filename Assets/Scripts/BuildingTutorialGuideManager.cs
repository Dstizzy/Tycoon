using System.Collections.Generic;
using UnityEngine;

// Shows one-time building explanations after the player opens a building panel.
// This guide only runs when the narrative tutorial toggle is enabled.
public class BuildingTutorialGuideManager : MonoBehaviour
{
   // Singleton instance for easy access from building panels.
   public static BuildingTutorialGuideManager Instance { get; private set; }

   // Keeps track of which building tags have already shown their guide.
   // Prevents the same explanation from repeating every time the panel is opened.
   private readonly HashSet<string> explainedBuildings = new HashSet<string>();

   // Enforces a simple singleton so only one guide manager exists at runtime.
   private void Awake()
   {
      if (Instance != null && Instance != this)
      {
         Destroy(gameObject);
         return;
      }

      Instance = this;
   }

   // Tries to show a one-time guide for the given building.
   // The guide is skipped if:
   // - tutorial mode is disabled
   // - the main narrative tutorial is still running
   // - the building tag is invalid
   // - the guide was already shown once
   // - the overlay UI is unavailable or currently busy
   public void TryShowBuildingGuide(string buildingTag)
   {
      if (TutorialFlowSettings.NarrativeTutorialEnabled == false)
         return;

      // Do not show building-specific guides until the main tutorial has fully finished
      if (TutorialFlowSettings.IsNarrativeTutorialRunning)
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

      // Mark before playback so repeated requests during the same moment do not duplicate the guide.
      explainedBuildings.Add(buildingTag);

      // Temporarily block gameplay while the guide dialogue is on screen.
      NarrativeOverlayUI.Instance.SetGameplayBlocked(true);
      NarrativeOverlayUI.Instance.PlaySequence(
         speakerProfile,
         guideLines,
         GetLayoutMode(buildingTag),
         false,
         () => NarrativeOverlayUI.Instance.SetGameplayBlocked(false));
   }

   // Returns the NPC profile that should speak for a given building.
   // Each building is mapped to the character most closely associated with it.
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

   // Returns the UI layout to use for the guide.
   // All current building guides use the bottom story layout for consistency.
   private NarrativeOverlayUI.DialogueLayoutMode GetLayoutMode(string buildingTag)
   {
      return NarrativeOverlayUI.DialogueLayoutMode.StoryBottom;
   }

   // Returns the full guide dialogue set for the requested building.
   // Each block is intentionally themed to the assigned speaker's personality.
   private NPCEncounterSystem.DialogueLine[] GetGuideLines(string buildingTag)
   {
      return buildingTag switch
      {
         "Trade Hut" => new NPCEncounterSystem.DialogueLine[]
         {
         new NPCEncounterSystem.DialogueLine("Ah, welcome. A fresh face is always good for business.", NPCEncounterSystem.ExpressionType.Happy),
         new NPCEncounterSystem.DialogueLine("This is the Trade Hut.", NPCEncounterSystem.ExpressionType.Neutral),
         new NPCEncounterSystem.DialogueLine("Bring crafted goods here to sell them for pearls, and stop by the market when you need materials, plans, or a timely bargain.", NPCEncounterSystem.ExpressionType.Thinking),
         new NPCEncounterSystem.DialogueLine("Pearls keep the whole operation moving, so do try not to spend them all in one dramatic gesture.", NPCEncounterSystem.ExpressionType.Special),
         new NPCEncounterSystem.DialogueLine("Take your time and look around. Profit favors a careful eye.", NPCEncounterSystem.ExpressionType.Happy)
         },

         "Lab" => new NPCEncounterSystem.DialogueLine[]
         {
         new NPCEncounterSystem.DialogueLine("Ooh, there you are! Nice, nice, nice. I was hoping you'd come by.", NPCEncounterSystem.ExpressionType.Happy),
         new NPCEncounterSystem.DialogueLine("This is the Lab, home of breakthroughs, bad handwriting, and ideas brilliant enough to worry sensible people.", NPCEncounterSystem.ExpressionType.Special),
         new NPCEncounterSystem.DialogueLine("Research starts here, and every bit of progress opens the way to bigger systems, better options, and the really important projects.", NPCEncounterSystem.ExpressionType.Thinking),
         new NPCEncounterSystem.DialogueLine("If you want to unlock everything worth unlocking before the volcano gets too excited, you'll want to keep this place busy.", NPCEncounterSystem.ExpressionType.Happy),
         new NPCEncounterSystem.DialogueLine("And between you and me, a complete set of Lab progress helps turn 'escape plan' into 'actual submarine.' Pretty impressive, right?", NPCEncounterSystem.ExpressionType.Special),
         new NPCEncounterSystem.DialogueLine("Also, see that submarine flask button in the middle? That's where you can check what the final submarine still needs.", NPCEncounterSystem.ExpressionType.Thinking),
         new NPCEncounterSystem.DialogueLine("Anyway, try not to look too amazed. I need some mystery to maintain my reputation.", NPCEncounterSystem.ExpressionType.Happy)
         },

         "Ore Refinery" => new NPCEncounterSystem.DialogueLine[]
         {
         new NPCEncounterSystem.DialogueLine("...Oh... hello...", NPCEncounterSystem.ExpressionType.Neutral),
         new NPCEncounterSystem.DialogueLine("...This is the Refinery...", NPCEncounterSystem.ExpressionType.Thinking),
         new NPCEncounterSystem.DialogueLine("...Raw ore gets processed here... slowly, carefully... until it becomes something the rest of the settlement can actually use...", NPCEncounterSystem.ExpressionType.Happy),
         new NPCEncounterSystem.DialogueLine("...If production stalls here... other places feel it too... so it's better when the machines stay calm...", NPCEncounterSystem.ExpressionType.Special),
         new NPCEncounterSystem.DialogueLine("...Come by again... It's nicer when things are running softly...", NPCEncounterSystem.ExpressionType.Happy)
         },

         "Exploration Unit" => new NPCEncounterSystem.DialogueLine[]
         {
         new NPCEncounterSystem.DialogueLine("You made it here. Good.", NPCEncounterSystem.ExpressionType.Neutral),
         new NPCEncounterSystem.DialogueLine("This is the Exploration Unit.", NPCEncounterSystem.ExpressionType.Thinking),
         new NPCEncounterSystem.DialogueLine("You send expeditions from here when you need resources, discoveries, or a route to something farther out than your current reach.", NPCEncounterSystem.ExpressionType.Special),
         new NPCEncounterSystem.DialogueLine("Some progress only happens when someone is willing to go a little farther than is comfortable.", NPCEncounterSystem.ExpressionType.Thinking),
         new NPCEncounterSystem.DialogueLine("Prepare properly, then move. It's cheaper than regret.", NPCEncounterSystem.ExpressionType.Neutral)
         },

         "Forge" => new NPCEncounterSystem.DialogueLine[]
         {
         new NPCEncounterSystem.DialogueLine("Hey, welcome! Good timing too. I was just about to make something impressive.", NPCEncounterSystem.ExpressionType.Happy),
         new NPCEncounterSystem.DialogueLine("This is the Forge.", NPCEncounterSystem.ExpressionType.Special),
         new NPCEncounterSystem.DialogueLine("Raw materials become tools, equipment, and all the practical little miracles that keep the settlement working here.", NPCEncounterSystem.ExpressionType.Thinking),
         new NPCEncounterSystem.DialogueLine("If you want stronger production or the parts for bigger ambitions, this is where those ideas start taking shape.", NPCEncounterSystem.ExpressionType.Happy),
         new NPCEncounterSystem.DialogueLine("Come back anytime. Craftsmanship this good deserves repeat visits.", NPCEncounterSystem.ExpressionType.Special)
         },

         // Unknown building tags have no guide.
         _ => null
      };
   }
}