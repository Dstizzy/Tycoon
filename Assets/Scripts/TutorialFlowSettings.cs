using UnityEngine;

// Stores global tutorial-related state.
//
// Responsibility:
// - Save whether the player wants the narrative tutorial enabled.
// - Expose whether the legacy tutorial system should be ignored.
// - Track whether the new narrative tutorial is currently running.
//
// Other tutorial-related systems use this class as a shared source of truth.
public static class TutorialFlowSettings
{
   // PlayerPrefs key used to persist the narrative tutorial toggle between sessions.
   private const string NARRATIVE_TUTORIAL_KEY = "NarrativeTutorialEnabled";

   // True if the player wants the narrative tutorial enabled.
   // Default value is true for first-time players.
   public static bool NarrativeTutorialEnabled
   {
      get => PlayerPrefs.GetInt(NARRATIVE_TUTORIAL_KEY, 1) == 1;
      set
      {
         PlayerPrefs.SetInt(NARRATIVE_TUTORIAL_KEY, value ? 1 : 0);
         PlayerPrefs.Save();
      }
   }

   // The old tutorial system still exists in the project, but is intentionally disabled.
   public static bool UseLegacyTutorial => false;

   // True only while the new narrative tutorial sequence is actively running.
   // Used by other systems to avoid stacking extra tutorial popups during onboarding.
   public static bool IsNarrativeTutorialRunning { get; set; }
}