using UnityEngine;

public static class TutorialFlowSettings
{
   private const string NARRATIVE_TUTORIAL_KEY = "NarrativeTutorialEnabled";

   // New tutorial toggle stored between sessions
   public static bool NarrativeTutorialEnabled
   {
      get => PlayerPrefs.GetInt(NARRATIVE_TUTORIAL_KEY, 1) == 1;
      set
      {
         PlayerPrefs.SetInt(NARRATIVE_TUTORIAL_KEY, value ? 1 : 0);
         PlayerPrefs.Save();
      }
   }

   // Keep the legacy tutorial script in the project, but disable it by default
   public static bool UseLegacyTutorial => false;
}