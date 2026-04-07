using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Controls the ending scene flow for both success and failure routes.
// All ending dialogue uses the same speaker + DialogueLine format as the
// NPC encounter system so portraits, expressions and action lines remain consistent.
public class EndingSequenceManager : MonoBehaviour
{
   // ─────────────────────────────────────────────────────────────────────
   //  Data classes
   // ─────────────────────────────────────────────────────────────────────

   [Serializable]
   public class EndingDialogueEntry
   {
      public NPCEncounterSystem.NPCPersonality speaker;
      public NPCEncounterSystem.DialogueLine dialogue;

      public EndingDialogueEntry() { }

      public EndingDialogueEntry(
         NPCEncounterSystem.NPCPersonality speaker,
         string text,
         NPCEncounterSystem.ExpressionType expression = NPCEncounterSystem.ExpressionType.Neutral,
         bool isAction = false)
      {
         this.speaker = speaker;
         dialogue = new NPCEncounterSystem.DialogueLine(text, expression, isAction);
      }
   }

   [Serializable]
   public class EndingSpeakerProfile
   {
      public NPCEncounterSystem.NPCPersonality personality;
      public string speakerName;
      public Sprite portraitNeutral;
      public Sprite portraitHappy;
      public Sprite portraitAngry;
      public Sprite portraitSurprised;
      public Sprite portraitThinking;
      public Sprite portraitSpecial;
   }


   // ─────────────────────────────────────────────────────────────────────
   //  Inspector fields
   // ─────────────────────────────────────────────────────────────────────

   [Header("Scene")]
   [SerializeField] private string startSceneName = "StartScreenScene";

   [Header("Roots")]
   [SerializeField] private GameObject successRoot;
   [SerializeField] private GameObject failureRoot;

   [Header("UI")]
   [SerializeField] private TextMeshProUGUI titleText;
   [SerializeField] private TextMeshProUGUI bodyText;
   [SerializeField] private TextMeshProUGUI speakerNameText;
   [SerializeField] private Image portraitImage;
   [SerializeField] private Image fadeImage;

   [Header("Speaker Profiles")]
   [SerializeField] private EndingSpeakerProfile[] speakerProfiles;

   [Header("Success")]
   [SerializeField] private Transform[] successBuildings;
   [SerializeField] private RectTransform submarineTransform;
   [SerializeField] private float successDuration = 5f;
   [SerializeField] private float successBounceAmount = 12f;
   [SerializeField] private float successBounceSpeed = 2.5f;
   [SerializeField] private float submarineRiseDistance = 260f;
   [SerializeField] private float submarineRiseDuration = 3.5f;
   [SerializeField] private float submarineFloatDuration = 2.5f;
   [SerializeField] private float submarineFloatAmount = 18f;
   [SerializeField] private float submarineFloatSpeed = 1.8f;
   [SerializeField] private float successFinalHoldDuration = 1.8f;
   [SerializeField] private float successLaunchHoldDuration = 1.5f;
   [SerializeField] private float successPostLaunchHoldDuration = 1.5f;

   [SerializeField]
   private EndingDialogueEntry[] successLines = new EndingDialogueEntry[]
   {
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Turtle, "WAIT WAIT WAIT— it worked?!", NPCEncounterSystem.ExpressionType.Surprised),
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Turtle, "Nearly trips over the lab console.", NPCEncounterSystem.ExpressionType.Surprised, isAction: true),
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Turtle, "It actually worked! The submarine is complete! COMPLETE complete!", NPCEncounterSystem.ExpressionType.Special),
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.HermitCrab, "Good heavens.", NPCEncounterSystem.ExpressionType.Surprised),
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.HermitCrab, "At last, a plan with the rare quality of being finished.", NPCEncounterSystem.ExpressionType.Thinking),
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Jellyfish, "...Shiny... big shiny...", NPCEncounterSystem.ExpressionType.Happy),
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Dolphin, "Hold on, hold on, nobody touch anything dramatic for five seconds.", NPCEncounterSystem.ExpressionType.Surprised),
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Dolphin, "I just want to enjoy one successful moment in this cursed workplace.", NPCEncounterSystem.ExpressionType.Thinking),
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Octopus, "HA! I knew all that last-minute crafting would pay off!", NPCEncounterSystem.ExpressionType.Happy),
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Octopus, "Look at that finish! Gorgeous! Practically heroic!", NPCEncounterSystem.ExpressionType.Special),
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Seahorse, "Route's clear.", NPCEncounterSystem.ExpressionType.Neutral),
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Seahorse, "If we're leaving, then we leave now.", NPCEncounterSystem.ExpressionType.Thinking),
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Turtle, "We're really doing this!", NPCEncounterSystem.ExpressionType.Happy),
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Turtle, "No more backup plans! No more backup-backup plans! No more maybe-we-survive-if-the-volcano-gets-bored plans!", NPCEncounterSystem.ExpressionType.Special),
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.HermitCrab, "A pity. I was just beginning to appreciate the local panic economy.", NPCEncounterSystem.ExpressionType.Happy),
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Dolphin, "Don't joke like that right now!", NPCEncounterSystem.ExpressionType.Angry),
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Dolphin, "...Actually, no, keep joking. If I stop moving, I might cry.", NPCEncounterSystem.ExpressionType.Happy),
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Jellyfish, "...Happy...", NPCEncounterSystem.ExpressionType.Happy),
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Jellyfish, "...Everyone... happy...", NPCEncounterSystem.ExpressionType.Special),
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Seahorse, "Engines are responding.", NPCEncounterSystem.ExpressionType.Neutral),
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Seahorse, "That's our window.", NPCEncounterSystem.ExpressionType.Thinking),
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Turtle, "Then what are we waiting for?!", NPCEncounterSystem.ExpressionType.Surprised),
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Turtle, "Everybody cheer! Somebody wave! Somebody do something cinematic!", NPCEncounterSystem.ExpressionType.Special),
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Dolphin, "The launch platform rattles with cheers, laughter, and far too many people talking at once.", NPCEncounterSystem.ExpressionType.Neutral, isAction: true)
   };

   [SerializeField]
   private EndingDialogueEntry[] successLaunchLines = new EndingDialogueEntry[]
   {
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Dolphin, "Hey—!", NPCEncounterSystem.ExpressionType.Surprised),
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Dolphin, "We're actually rising. We're actually rising!", NPCEncounterSystem.ExpressionType.Happy),
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Turtle, "The engines are stable!", NPCEncounterSystem.ExpressionType.Surprised),
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Turtle, "They're actually stable!", NPCEncounterSystem.ExpressionType.Special),
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Seahorse, "Then stop sounding surprised and keep us moving.", NPCEncounterSystem.ExpressionType.Neutral)
   };

   [SerializeField]
   private EndingDialogueEntry[] successPostLaunchLines = new EndingDialogueEntry[]
   {
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Turtle, "WOO! We are officially in the dramatic departure part!", NPCEncounterSystem.ExpressionType.Special),
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Seahorse, "Maintain course.", NPCEncounterSystem.ExpressionType.Neutral),
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Jellyfish, "...Pretty...", NPCEncounterSystem.ExpressionType.Happy),
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Dolphin, "You know what?", NPCEncounterSystem.ExpressionType.Thinking),
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Dolphin, "For once, this ridiculous plan actually worked.", NPCEncounterSystem.ExpressionType.Happy)
   };

   [Header("Failure")]
   [SerializeField] private Transform[] failureBuildings;
   [SerializeField] private Transform cameraShakeTarget;
   [SerializeField] private float failureDuration = 4.5f;
   [SerializeField] private float failureBounceAmount = 12f;
   [SerializeField] private float failureBounceSpeed = 6f;
   [SerializeField] private float cameraShakeStrength = 16f;
   [SerializeField] private int failureShakeStartLineIndex = 1;
   [SerializeField] private float failureFinalHoldDuration = 1.1f;

   [SerializeField]
   private EndingDialogueEntry[] failureLines = new EndingDialogueEntry[]
   {
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Dolphin, "Wait. No. No no no—", NPCEncounterSystem.ExpressionType.Surprised),
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Turtle, "That sound is BAD! That's a very bad sound!", NPCEncounterSystem.ExpressionType.Surprised),
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Dolphin, "Everybody run!", NPCEncounterSystem.ExpressionType.Special),
      new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Dolphin, "The entire outpost lurches as the volcano finally erupts.", NPCEncounterSystem.ExpressionType.Angry, isAction: true)
   };

   [Header("Timing")]
   [SerializeField] private float lineInterval = 1.8f;
   [SerializeField] private float fadeDuration = 1.5f;


   // ─────────────────────────────────────────────────────────────────────
   //  Runtime state
   // ─────────────────────────────────────────────────────────────────────

   private float activeSequenceDuration;
   private bool hasStartedFailureShake;
   private bool isDialoguePlaying;
   private bool isCurrentDialogueSkippable;
   private bool skipCurrentDialogueRequested;


   // ─────────────────────────────────────────────────────────────────────
   //  Unity lifecycle
   // ─────────────────────────────────────────────────────────────────────

   // Initializes the ending UI before playback begins.
   private void Awake()
   {
      if (successRoot != null)
         successRoot.SetActive(false);

      if (failureRoot != null)
         failureRoot.SetActive(false);

      if (bodyText != null)
         bodyText.text = string.Empty;

      ClearSpeakerUI();

      if (fadeImage != null)
      {
         Color currentColor = fadeImage.color;
         currentColor.a = 1f;
         fadeImage.color = currentColor;
         fadeImage.gameObject.SetActive(true);
      }
   }

   // Checks for skip input while a skippable dialogue block is playing.
   private void Update()
   {
      if (!isDialoguePlaying || !isCurrentDialogueSkippable)
         return;

      if (WasSkipPressed())
         skipCurrentDialogueRequested = true;
   }

   // Starts the correct ending sequence as soon as the scene loads.
   private void Start()
   {
      StartCoroutine(PlayEndingSequence());
   }


   // ─────────────────────────────────────────────────────────────────────
   //  Main sequence flow
   // ─────────────────────────────────────────────────────────────────────

   // Chooses the success or failure route, then fades out and returns to the start screen.
   private IEnumerator PlayEndingSequence()
   {
      bool isSuccess = GameEndingState.CurrentResult == GameEndingState.EndingResult.Success;

      if (titleText != null)
         titleText.text = isSuccess ? "Escape Successful" : "Escape Failed";

      if (successRoot != null)
         successRoot.SetActive(isSuccess);

      if (failureRoot != null)
         failureRoot.SetActive(!isSuccess);

      yield return FadeTo(0f, fadeDuration);

      if (isSuccess)
         yield return PlaySuccessSequence();
      else
         yield return PlayFailureSequence();

      yield return FadeTo(1f, fadeDuration);

      CleanupPersistentObjects();
      GameEndingState.Reset();
      SceneManager.LoadScene(startSceneName);
   }

   // Success flow:
   //   1. Skippable pre-launch dialogue
   //   2. Mandatory submarine rise
   //   3. Skippable launch dialogue
   //   4. Mandatory floating
   //   5. Skippable post-launch dialogue
   private IEnumerator PlaySuccessSequence()
   {
      float introDuration = GetDialogueDuration(successLines, successFinalHoldDuration);
      float launchDialogueDuration = GetDialogueDuration(successLaunchLines, successLaunchHoldDuration);
      float postLaunchDuration = GetDialogueDuration(successPostLaunchLines, successPostLaunchHoldDuration);
      float mandatoryDuration = submarineRiseDuration + submarineFloatDuration;

      activeSequenceDuration = Mathf.Max(
         successDuration,
         introDuration + launchDialogueDuration + postLaunchDuration + mandatoryDuration);

      if (successBuildings != null && successBuildings.Length > 0)
      {
         StartCoroutine(AnimateBuildings(
            successBuildings,
            activeSequenceDuration,
            successBounceAmount,
            successBounceSpeed));
      }

      yield return PlayDialogueEntries(successLines, successFinalHoldDuration, true);
      yield return AnimateSubmarineRise();
      yield return PlayDialogueEntries(successLaunchLines, successLaunchHoldDuration, true);
      yield return AnimateSubmarineFloat();
      yield return PlayDialogueEntries(successPostLaunchLines, successPostLaunchHoldDuration, true);

      float consumedDuration = introDuration + launchDialogueDuration + postLaunchDuration + mandatoryDuration;
      float remainingDuration = Mathf.Max(0f, activeSequenceDuration - consumedDuration);

      if (remainingDuration > 0f)
         yield return new WaitForSeconds(remainingDuration);
   }

   // Failure flow:
   //   1. Dialogue
   //   2. Building + camera shake
   //   3. Fade out
   private IEnumerator PlayFailureSequence()
   {
      float dialogueDuration = GetDialogueDuration(failureLines, failureFinalHoldDuration);
      activeSequenceDuration = Mathf.Max(failureDuration, dialogueDuration);
      hasStartedFailureShake = false;

      if (failureBuildings != null && failureBuildings.Length > 0)
      {
         StartCoroutine(AnimateBuildings(
            failureBuildings,
            activeSequenceDuration,
            failureBounceAmount,
            failureBounceSpeed));
      }

      yield return PlayDialogueEntries(failureLines, failureFinalHoldDuration, false, HandleFailureLineStarted);

      if (!hasStartedFailureShake && cameraShakeTarget != null)
      {
         hasStartedFailureShake = true;
         StartCoroutine(ShakeTransform(cameraShakeTarget, activeSequenceDuration, cameraShakeStrength));
      }

      float remainingDuration = Mathf.Max(0f, activeSequenceDuration - dialogueDuration);
      if (remainingDuration > 0f)
         yield return new WaitForSeconds(remainingDuration);
   }


   // ─────────────────────────────────────────────────────────────────────
   //  Dialogue helpers
   // ─────────────────────────────────────────────────────────────────────

   // Plays one dialogue block and optionally allows it to be skipped.
   private IEnumerator PlayDialogueEntries(
      EndingDialogueEntry[] entries,
      float finalHoldDuration,
      bool allowSkip,
      Action<int> onLineStarted = null)
   {
      if (entries == null || entries.Length == 0)
      {
         if (finalHoldDuration > 0f)
            yield return new WaitForSeconds(finalHoldDuration);
         yield break;
      }

      isDialoguePlaying = true;
      isCurrentDialogueSkippable = allowSkip;
      skipCurrentDialogueRequested = false;

      for (int index = 0; index < entries.Length; index++)
      {
         if (allowSkip && skipCurrentDialogueRequested)
            break;

         onLineStarted?.Invoke(index);
         DisplayDialogueEntry(entries[index]);

         bool isLastLine = index >= entries.Length - 1;
         float waitDuration = isLastLine ? finalHoldDuration : lineInterval;

         if (allowSkip)
            yield return WaitForSecondsOrSkip(waitDuration);
         else if (waitDuration > 0f)
            yield return new WaitForSeconds(waitDuration);
      }

      isDialoguePlaying = false;
      isCurrentDialogueSkippable = false;
      skipCurrentDialogueRequested = false;
   }

   // Displays one dialogue entry, including speaker name and portrait.
   private void DisplayDialogueEntry(EndingDialogueEntry entry)
   {
      if (entry == null || entry.dialogue == null)
      {
         if (bodyText != null)
            bodyText.text = string.Empty;

         ClearSpeakerUI();
         return;
      }

      NPCEncounterSystem.DialogueLine line = entry.dialogue;
      string text = line.text ?? string.Empty;

      if (bodyText != null)
         bodyText.text = line.isAction ? $"<i>{text}</i>" : text;

      ApplySpeakerUI(entry.speaker, line.expression);
   }

   // Updates the portrait and speaker name using the ending speaker profile data.
   private void ApplySpeakerUI(
      NPCEncounterSystem.NPCPersonality speaker,
      NPCEncounterSystem.ExpressionType expression)
   {
      EndingSpeakerProfile profile = GetSpeakerProfile(speaker);

      if (speakerNameText != null)
         speakerNameText.text = profile != null && string.IsNullOrEmpty(profile.speakerName) == false
            ? profile.speakerName
            : speaker.ToString();

      if (portraitImage == null)
         return;

      if (profile == null)
      {
         portraitImage.sprite = null;
         portraitImage.color = new Color(1f, 1f, 1f, 0f);
         return;
      }

      Sprite portrait = GetPortraitForExpression(profile, expression);
      portraitImage.sprite = portrait;
      portraitImage.color = portrait != null ? Color.white : new Color(1f, 1f, 1f, 0f);
   }

   // Clears the current speaker name and portrait UI.
   private void ClearSpeakerUI()
   {
      if (speakerNameText != null)
         speakerNameText.text = string.Empty;

      if (portraitImage != null)
      {
         portraitImage.sprite = null;
         portraitImage.color = new Color(1f, 1f, 1f, 0f);
      }
   }

   // Returns the total playback time of a dialogue block.
   private float GetDialogueDuration(EndingDialogueEntry[] entries, float finalHoldDuration)
   {
      if (entries == null || entries.Length == 0)
         return 0f;

      return ((entries.Length - 1) * lineInterval) + finalHoldDuration;
   }

   // Detects skip input for skippable dialogue blocks.
   private bool WasSkipPressed()
   {
      return Input.GetMouseButtonDown(0)
         || Input.GetKeyDown(KeyCode.Space)
         || Input.GetKeyDown(KeyCode.Return)
         || Input.GetKeyDown(KeyCode.Escape);
   }

   // Waits for the given duration unless the current dialogue block is skipped.
   private IEnumerator WaitForSecondsOrSkip(float duration)
   {
      if (duration <= 0f)
         yield break;

      float elapsed = 0f;

      while (elapsed < duration)
      {
         if (skipCurrentDialogueRequested)
            yield break;

         elapsed += Time.deltaTime;
         yield return null;
      }
   }

   // Starts the failure shake once the sequence reaches the requested line index.
   private void HandleFailureLineStarted(int lineIndex)
   {
      if (hasStartedFailureShake)
         return;

      if (cameraShakeTarget == null)
         return;

      if (lineIndex < Mathf.Max(0, failureShakeStartLineIndex))
         return;

      hasStartedFailureShake = true;
      StartCoroutine(ShakeTransform(cameraShakeTarget, activeSequenceDuration, cameraShakeStrength));
   }

   // Returns the ending speaker profile for the requested personality.
   private EndingSpeakerProfile GetSpeakerProfile(NPCEncounterSystem.NPCPersonality speaker)
   {
      if (speakerProfiles == null)
         return null;

      foreach (EndingSpeakerProfile currentProfile in speakerProfiles)
      {
         if (currentProfile != null && currentProfile.personality == speaker)
            return currentProfile;
      }

      return null;
   }

   // Returns the correct portrait sprite for the requested expression.
   private Sprite GetPortraitForExpression(
      EndingSpeakerProfile profile,
      NPCEncounterSystem.ExpressionType expression)
   {
      return expression switch
      {
         NPCEncounterSystem.ExpressionType.Happy => profile.portraitHappy ?? profile.portraitNeutral,
         NPCEncounterSystem.ExpressionType.Angry => profile.portraitAngry ?? profile.portraitNeutral,
         NPCEncounterSystem.ExpressionType.Surprised => profile.portraitSurprised ?? profile.portraitNeutral,
         NPCEncounterSystem.ExpressionType.Thinking => profile.portraitThinking ?? profile.portraitNeutral,
         NPCEncounterSystem.ExpressionType.Special => profile.portraitSpecial ?? profile.portraitNeutral,
         _ => profile.portraitNeutral
      };
   }


   // ─────────────────────────────────────────────────────────────────────
   //  Animation helpers
   // ─────────────────────────────────────────────────────────────────────

   // Loops a bounce/rotation animation on the supplied transforms.
   private IEnumerator AnimateBuildings(Transform[] targets, float duration, float amount, float speed)
   {
      Vector3[] originalPositions = new Vector3[targets.Length];
      Vector3[] originalRotations = new Vector3[targets.Length];

      for (int index = 0; index < targets.Length; index++)
      {
         if (targets[index] == null)
            continue;

         originalPositions[index] = targets[index].localPosition;
         originalRotations[index] = targets[index].localEulerAngles;
      }

      float elapsed = 0f;

      while (elapsed < duration)
      {
         elapsed += Time.deltaTime;

         for (int index = 0; index < targets.Length; index++)
         {
            Transform currentTarget = targets[index];
            if (currentTarget == null)
               continue;

            float phase = index * 0.45f;
            float wave = Mathf.Sin((elapsed * speed) + phase);
            float rotationWave = Mathf.Sin((elapsed * speed * 0.8f) + phase);

            currentTarget.localPosition = originalPositions[index] + Vector3.up * (wave * amount);
            currentTarget.localEulerAngles = originalRotations[index] + new Vector3(0f, 0f, rotationWave * (amount * 0.35f));
         }

         yield return null;
      }

      for (int index = 0; index < targets.Length; index++)
      {
         if (targets[index] == null)
            continue;

         targets[index].localPosition = originalPositions[index];
         targets[index].localEulerAngles = originalRotations[index];
      }
   }

   // Raises the submarine upward with a slight side-to-side drift.
   private IEnumerator AnimateSubmarineRise()
   {
      if (submarineTransform == null)
         yield break;

      Vector2 startPosition = submarineTransform.anchoredPosition;
      Vector2 endPosition = startPosition + Vector2.up * submarineRiseDistance;

      float elapsed = 0f;

      while (elapsed < submarineRiseDuration)
      {
         elapsed += Time.deltaTime;
         float t = Mathf.Clamp01(elapsed / submarineRiseDuration);
         float easedT = Mathf.SmoothStep(0f, 1f, t);

         Vector2 currentPosition = Vector2.Lerp(startPosition, endPosition, easedT);
         currentPosition.x += Mathf.Sin(elapsed * 3f) * 10f;
         submarineTransform.anchoredPosition = currentPosition;

         yield return null;
      }

      submarineTransform.anchoredPosition = endPosition;
   }

   // Plays the floating motion after launch.
   private IEnumerator AnimateSubmarineFloat()
   {
      if (submarineTransform == null)
         yield break;

      Vector2 basePosition = submarineTransform.anchoredPosition;
      float elapsed = 0f;

      while (elapsed < submarineFloatDuration)
      {
         elapsed += Time.deltaTime;

         float yOffset = Mathf.Sin(elapsed * submarineFloatSpeed) * submarineFloatAmount;
         float xOffset = Mathf.Sin(elapsed * submarineFloatSpeed * 0.55f) * (submarineFloatAmount * 0.35f);

         submarineTransform.anchoredPosition = basePosition + new Vector2(xOffset, yOffset);
         yield return null;
      }

      submarineTransform.anchoredPosition = basePosition;
   }

   // Shakes the supplied transform for the failure route.
   private IEnumerator ShakeTransform(Transform target, float duration, float strength)
   {
      if (target == null)
         yield break;

      Vector3 originalPosition = target.localPosition;
      float elapsed = 0f;

      while (elapsed < duration)
      {
         elapsed += Time.deltaTime;

         float offsetX = UnityEngine.Random.Range(-strength, strength);
         float offsetY = UnityEngine.Random.Range(-strength, strength);

         target.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);
         yield return null;
      }

      target.localPosition = originalPosition;
   }

   // Fades the overlay image to the requested alpha.
   private IEnumerator FadeTo(float targetAlpha, float duration)
   {
      if (fadeImage == null)
         yield break;

      fadeImage.gameObject.SetActive(true);

      float startAlpha = fadeImage.color.a;
      float elapsed = 0f;

      while (elapsed < duration)
      {
         elapsed += Time.deltaTime;
         float t = Mathf.Clamp01(elapsed / duration);

         Color currentColor = fadeImage.color;
         currentColor.a = Mathf.Lerp(startAlpha, targetAlpha, t);
         fadeImage.color = currentColor;

         yield return null;
      }

      Color endColor = fadeImage.color;
      endColor.a = targetAlpha;
      fadeImage.color = endColor;
   }


   // ─────────────────────────────────────────────────────────────────────
   //  Cleanup
   // ─────────────────────────────────────────────────────────────────────

   // Destroys persistent singleton objects before returning to the start scene.
   private void CleanupPersistentObjects()
   {
      if (NarrativeOverlayUI.Instance != null)
         NarrativeOverlayUI.Instance.DisposeOverlay();

      DestroySingleton(BuildingTutorialGuideManager.Instance);
      DestroySingleton(NPCEncounterSystem.Instance);
      DestroySingleton(InventoryManager.Instance);
      DestroySingleton(TradeHutManager.Instance);
      DestroySingleton(LabManager.labManager);
   }

   // Safely destroys a singleton object if it exists.
   private void DestroySingleton(MonoBehaviour currentSingleton)
   {
      if (currentSingleton != null)
         Destroy(currentSingleton.gameObject);
   }
}