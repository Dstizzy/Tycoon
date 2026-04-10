using UnityEngine.SceneManagement;

public static class GameEndingState
{
   public enum EndingResult
   {
      None,
      Success,
      Failure
   }

   private const string ENDING_SCENE_NAME = "EndingScene";

   public static EndingResult CurrentResult { get; private set; } = EndingResult.None;

   public static bool HasEndingTriggered => CurrentResult != EndingResult.None;

   public static void LoadSuccessEnding()
   {
      LoadEnding(EndingResult.Success);
   }

   public static void LoadFailureEnding()
   {
      if (CurrentResult == EndingResult.Success)
         return;

      LoadEnding(EndingResult.Failure);
   }

   public static void Reset()
   {
      CurrentResult = EndingResult.None;
   }

   private static void LoadEnding(EndingResult result)
   {
      if (CurrentResult != EndingResult.None)
         return;

      CurrentResult = result;

      if (NarrativeOverlayUI.Instance != null)
         NarrativeOverlayUI.Instance.DisposeOverlay();

      SceneManager.LoadScene(ENDING_SCENE_NAME);
   }
}