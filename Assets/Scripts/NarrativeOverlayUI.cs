using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Creates and manages the full-screen overlay flow UI used for:
// 1. Intro dialogue
// 2. Tutorial toggle prompt
// 3. Tutorial dialogue steps
//
// This UI is separate from NPCEncounter dialogue on purpose.
// Story dialogue uses fixed or preset layouts and blocks gameplay while active.
public class NarrativeOverlayUI : MonoBehaviour
{
   public enum DialogueLayoutMode
   {
      StoryBottom,
      Top
   }

   public static NarrativeOverlayUI Instance { get; private set; }

   [SerializeField] private float textSpeed = 0.02f;

   private Coroutine typingCoroutine;
   private bool isTyping;
   private string currentFormattedLine = string.Empty;

   private const float PANEL_WIDTH = 1200f;
   private const float PANEL_HEIGHT = 360f;
   private const float PORTRAIT_SIZE = 220f;
   private const float PADDING = 24f;

   private GameObject canvasObject;
   private Canvas overlayCanvas;
   private GraphicRaycaster raycaster;

   private Image blockInputImage;
   private Image fadeImage;

   private GameObject dialoguePanel;
   private Image portraitImage;
   private TextMeshProUGUI nameText;
   private TextMeshProUGUI dialogueText;
   private Button nextButton;
   private Button skipButton;

   private GameObject togglePanel;
   private Button tutorialOnButton;
   private Button tutorialOffButton;
   private TextMeshProUGUI toggleTitleText;
   private TextMeshProUGUI toggleBodyText;

   private NPCEncounterSystem.NPCProfile currentProfile;
   private NPCEncounterSystem.DialogueLine[] currentLines;
   private int currentLineIndex;
   private Action currentCompleteAction;
   private Action<bool> currentToggleAction;

   private bool isSequencePlaying;

   private void Awake()
   {
      if (Instance != null && Instance != this)
      {
         Destroy(gameObject);
         return;
      }

      Instance = this;
      BuildUI();
      HideAll();
   }

   private void BuildUI()
   {
      canvasObject = new GameObject("NarrativeOverlayCanvas");
      DontDestroyOnLoad(canvasObject);

      overlayCanvas = canvasObject.AddComponent<Canvas>();
      overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
      overlayCanvas.sortingOrder = 1200;

      CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
      scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
      scaler.referenceResolution = new Vector2(1920, 1080);
      scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
      scaler.matchWidthOrHeight = 0.5f;

      raycaster = canvasObject.AddComponent<GraphicRaycaster>();

      BuildBlockInput();
      BuildFadeOverlay();
      BuildDialoguePanel();
      BuildTutorialTogglePanel();
   }

   private void BuildBlockInput()
   {
      GameObject blockObject = CreateUIObject("BlockInputPanel", canvasObject.transform);
      RectTransform rect = blockObject.GetComponent<RectTransform>();
      rect.anchorMin = Vector2.zero;
      rect.anchorMax = Vector2.one;
      rect.offsetMin = Vector2.zero;
      rect.offsetMax = Vector2.zero;

      blockInputImage = blockObject.AddComponent<Image>();
      blockInputImage.color = new Color(0f, 0f, 0f, 0f);
      blockObject.SetActive(false);
   }

   private void BuildFadeOverlay()
   {
      GameObject fadeObject = CreateUIObject("FadeOverlay", canvasObject.transform);
      RectTransform rect = fadeObject.GetComponent<RectTransform>();
      rect.anchorMin = Vector2.zero;
      rect.anchorMax = Vector2.one;
      rect.offsetMin = Vector2.zero;
      rect.offsetMax = Vector2.zero;

      fadeImage = fadeObject.AddComponent<Image>();
      fadeImage.color = new Color(0f, 0f, 0f, 1f);
      fadeObject.SetActive(false);
   }

   private void BuildDialoguePanel()
   {
      dialoguePanel = CreateUIObject("NarrativeDialoguePanel", canvasObject.transform);
      RectTransform panelRect = dialoguePanel.GetComponent<RectTransform>();
      panelRect.sizeDelta = new Vector2(PANEL_WIDTH, PANEL_HEIGHT);

      Image panelImage = dialoguePanel.AddComponent<Image>();
      panelImage.color = new Color(0.08f, 0.12f, 0.18f, 0.96f);

      Outline panelOutline = dialoguePanel.AddComponent<Outline>();
      panelOutline.effectColor = new Color(0.35f, 0.55f, 0.8f, 0.9f);
      panelOutline.effectDistance = new Vector2(2f, 2f);

      GameObject portraitFrame = CreateUIObject("PortraitFrame", dialoguePanel.transform);
      RectTransform portraitFrameRect = portraitFrame.GetComponent<RectTransform>();
      portraitFrameRect.anchorMin = new Vector2(0f, 0f);
      portraitFrameRect.anchorMax = new Vector2(0f, 1f);
      portraitFrameRect.offsetMin = new Vector2(PADDING, PADDING);
      portraitFrameRect.offsetMax = new Vector2(PORTRAIT_SIZE + PADDING, -PADDING);

      Image portraitFrameImage = portraitFrame.AddComponent<Image>();
      portraitFrameImage.color = new Color(0.14f, 0.18f, 0.24f, 1f);

      GameObject portraitObject = CreateUIObject("PortraitImage", portraitFrame.transform);
      RectTransform portraitRect = portraitObject.GetComponent<RectTransform>();
      portraitRect.anchorMin = new Vector2(0.5f, 0.5f);
      portraitRect.anchorMax = new Vector2(0.5f, 0.5f);
      portraitRect.pivot = new Vector2(0.5f, 0.5f);
      portraitRect.sizeDelta = new Vector2(PORTRAIT_SIZE - 24f, PORTRAIT_SIZE - 24f);
      portraitRect.anchoredPosition = Vector2.zero;

      portraitImage = portraitObject.AddComponent<Image>();
      portraitImage.preserveAspect = true;

      GameObject nameObject = CreateUIObject("NameText", dialoguePanel.transform);
      RectTransform nameRect = nameObject.GetComponent<RectTransform>();
      nameRect.anchorMin = new Vector2(0f, 1f);
      nameRect.anchorMax = new Vector2(1f, 1f);
      nameRect.offsetMin = new Vector2(PORTRAIT_SIZE + PADDING * 2f, -88f);
      nameRect.offsetMax = new Vector2(-PADDING, -PADDING);

      nameText = nameObject.AddComponent<TextMeshProUGUI>();
      nameText.fontSize = 38f;
      nameText.fontStyle = FontStyles.Bold;
      nameText.color = new Color(0.95f, 0.85f, 0.55f, 1f);
      nameText.alignment = TextAlignmentOptions.TopLeft;

      GameObject dialogueObject = CreateUIObject("DialogueText", dialoguePanel.transform);
      RectTransform dialogueRect = dialogueObject.GetComponent<RectTransform>();
      dialogueRect.anchorMin = new Vector2(0f, 0f);
      dialogueRect.anchorMax = new Vector2(1f, 1f);
      dialogueRect.offsetMin = new Vector2(PORTRAIT_SIZE + PADDING * 2f, 96f);
      dialogueRect.offsetMax = new Vector2(-PADDING, -96f);

      dialogueText = dialogueObject.AddComponent<TextMeshProUGUI>();
      dialogueText.fontSize = 30f;
      dialogueText.color = Color.white;
      dialogueText.alignment = TextAlignmentOptions.TopLeft;
      dialogueText.enableWordWrapping = true;
      dialogueText.richText = true;

      nextButton = CreateButton("NextButton", dialoguePanel.transform, "Next >", out TextMeshProUGUI _);
      RectTransform nextRect = nextButton.GetComponent<RectTransform>();
      nextRect.anchorMin = new Vector2(1f, 0f);
      nextRect.anchorMax = new Vector2(1f, 0f);
      nextRect.pivot = new Vector2(1f, 0f);
      nextRect.sizeDelta = new Vector2(180f, 64f);
      nextRect.anchoredPosition = new Vector2(-PADDING, PADDING);
      nextButton.onClick.AddListener(HandleNextClicked);

      skipButton = CreateButton("SkipButton", dialoguePanel.transform, "Skip", out TextMeshProUGUI _);
      RectTransform skipRect = skipButton.GetComponent<RectTransform>();
      skipRect.anchorMin = new Vector2(1f, 0f);
      skipRect.anchorMax = new Vector2(1f, 0f);
      skipRect.pivot = new Vector2(1f, 0f);
      skipRect.sizeDelta = new Vector2(180f, 64f);
      skipRect.anchoredPosition = new Vector2(-PADDING - 196f, PADDING);
      skipButton.onClick.AddListener(HandleSkipClicked);

      dialoguePanel.SetActive(false);
   }

   private void BuildTutorialTogglePanel()
   {
      togglePanel = CreateUIObject("TutorialTogglePanel", canvasObject.transform);
      RectTransform panelRect = togglePanel.GetComponent<RectTransform>();
      panelRect.anchorMin = new Vector2(0.5f, 0.5f);
      panelRect.anchorMax = new Vector2(0.5f, 0.5f);
      panelRect.pivot = new Vector2(0.5f, 0.5f);
      panelRect.sizeDelta = new Vector2(920f, 420f);
      panelRect.anchoredPosition = Vector2.zero;

      Image panelImage = togglePanel.AddComponent<Image>();
      panelImage.color = new Color(0.08f, 0.12f, 0.18f, 0.96f);

      Outline outline = togglePanel.AddComponent<Outline>();
      outline.effectColor = new Color(0.35f, 0.55f, 0.8f, 0.9f);
      outline.effectDistance = new Vector2(2f, 2f);

      GameObject titleObject = CreateUIObject("TitleText", togglePanel.transform);
      RectTransform titleRect = titleObject.GetComponent<RectTransform>();
      titleRect.anchorMin = new Vector2(0f, 1f);
      titleRect.anchorMax = new Vector2(1f, 1f);
      titleRect.offsetMin = new Vector2(PADDING, -96f);
      titleRect.offsetMax = new Vector2(-PADDING, -PADDING);

      toggleTitleText = titleObject.AddComponent<TextMeshProUGUI>();
      toggleTitleText.fontSize = 44f;
      toggleTitleText.fontStyle = FontStyles.Bold;
      toggleTitleText.color = new Color(0.95f, 0.85f, 0.55f, 1f);
      toggleTitleText.alignment = TextAlignmentOptions.Top;

      GameObject bodyObject = CreateUIObject("BodyText", togglePanel.transform);
      RectTransform bodyRect = bodyObject.GetComponent<RectTransform>();
      bodyRect.anchorMin = new Vector2(0f, 0f);
      bodyRect.anchorMax = new Vector2(1f, 1f);
      bodyRect.offsetMin = new Vector2(PADDING, 140f);
      bodyRect.offsetMax = new Vector2(-PADDING, -120f);

      toggleBodyText = bodyObject.AddComponent<TextMeshProUGUI>();
      toggleBodyText.fontSize = 28f;
      toggleBodyText.color = Color.white;
      toggleBodyText.alignment = TextAlignmentOptions.Midline;
      toggleBodyText.enableWordWrapping = true;

      tutorialOnButton = CreateButton("TutorialOnButton", togglePanel.transform, "Play Tutorial", out TextMeshProUGUI _);
      RectTransform onRect = tutorialOnButton.GetComponent<RectTransform>();
      onRect.anchorMin = new Vector2(0.5f, 0f);
      onRect.anchorMax = new Vector2(0.5f, 0f);
      onRect.pivot = new Vector2(1f, 0f);
      onRect.sizeDelta = new Vector2(260f, 72f);
      onRect.anchoredPosition = new Vector2(-16f, 28f);
      tutorialOnButton.onClick.AddListener(() => HandleTutorialToggle(true));

      tutorialOffButton = CreateButton("TutorialOffButton", togglePanel.transform, "Skip Tutorial", out TextMeshProUGUI _);
      RectTransform offRect = tutorialOffButton.GetComponent<RectTransform>();
      offRect.anchorMin = new Vector2(0.5f, 0f);
      offRect.anchorMax = new Vector2(0.5f, 0f);
      offRect.pivot = new Vector2(0f, 0f);
      offRect.sizeDelta = new Vector2(260f, 72f);
      offRect.anchoredPosition = new Vector2(16f, 28f);
      tutorialOffButton.onClick.AddListener(() => HandleTutorialToggle(false));

      togglePanel.SetActive(false);
   }

   private GameObject CreateUIObject(string objectName, Transform parent)
   {
      GameObject newObject = new GameObject(objectName);
      newObject.transform.SetParent(parent, false);
      newObject.AddComponent<RectTransform>();
      return newObject;
   }

   private Button CreateButton(string objectName, Transform parent, string buttonText, out TextMeshProUGUI textComponent)
   {
      GameObject buttonObject = CreateUIObject(objectName, parent);
      Image image = buttonObject.AddComponent<Image>();
      image.color = new Color(0.2f, 0.3f, 0.4f, 0.96f);

      Button button = buttonObject.AddComponent<Button>();
      ColorBlock colors = button.colors;
      colors.normalColor = new Color(0.2f, 0.3f, 0.4f, 0.96f);
      colors.highlightedColor = new Color(0.32f, 0.46f, 0.62f, 1f);
      colors.pressedColor = new Color(0.15f, 0.22f, 0.3f, 1f);
      button.colors = colors;

      GameObject textObject = CreateUIObject("Text", buttonObject.transform);
      RectTransform textRect = textObject.GetComponent<RectTransform>();
      textRect.anchorMin = Vector2.zero;
      textRect.anchorMax = Vector2.one;
      textRect.offsetMin = new Vector2(8f, 4f);
      textRect.offsetMax = new Vector2(-8f, -4f);

      textComponent = textObject.AddComponent<TextMeshProUGUI>();
      textComponent.text = buttonText;
      textComponent.fontSize = 28f;
      textComponent.fontStyle = FontStyles.Bold;
      textComponent.color = Color.white;
      textComponent.alignment = TextAlignmentOptions.Center;

      return button;
   }

   public void HideAll()
   {
      if (dialoguePanel != null)
         dialoguePanel.SetActive(false);

      if (togglePanel != null)
         togglePanel.SetActive(false);

      if (fadeImage != null)
         fadeImage.gameObject.SetActive(false);

      if (blockInputImage != null)
         blockInputImage.gameObject.SetActive(false);

      isSequencePlaying = false;
      currentCompleteAction = null;
      currentToggleAction = null;
   }

   // Enables or disables gameplay interaction while overlay dialogue is active.
   //
   // This blocks normal player actions and is used to ensure scripted onboarding
   // or tutorial dialogue is not interrupted by unrelated input.
   public void SetGameplayBlocked(bool isBlocked)
   {
      if (blockInputImage != null)
         blockInputImage.gameObject.SetActive(isBlocked);

      if (TurnManager.Instance != null && TurnManager.Instance.endTurnButton != null)
         TurnManager.Instance.endTurnButton.interactable = !isBlocked;

      if (PopUpManager.Instance != null)
      {
         if (isBlocked)
            PopUpManager.Instance.DisablePlayerInput();
         else
            PopUpManager.Instance.EnablePlayerInput();
      }
   }

   public void SetFadeImmediate(float alpha)
   {
      fadeImage.gameObject.SetActive(true);
      Color currentColor = fadeImage.color;
      currentColor.a = alpha;
      fadeImage.color = currentColor;
   }

   public Coroutine FadeTo(float targetAlpha, float duration, Action onComplete = null)
   {
      return StartCoroutine(FadeRoutine(targetAlpha, duration, onComplete));
   }

   private IEnumerator FadeRoutine(float targetAlpha, float duration, Action onComplete)
   {
      fadeImage.gameObject.SetActive(true);

      float elapsed = 0f;
      float startAlpha = fadeImage.color.a;

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

      if (targetAlpha <= 0f)
         fadeImage.gameObject.SetActive(false);

      onComplete?.Invoke();
   }

   private IEnumerator TypeLine(string rawText, bool isAction)
   {
      isTyping = true;
      dialogueText.text = string.Empty;

      if (isAction)
      {
         for (int index = 0; index < rawText.Length; index++)
         {
            dialogueText.text = $"<i>{rawText.Substring(0, index + 1)}</i>";
            yield return new WaitForSeconds(textSpeed);
         }
      }
      else
      {
         for (int index = 0; index < rawText.Length; index++)
         {
            dialogueText.text = rawText.Substring(0, index + 1);
            yield return new WaitForSeconds(textSpeed);
         }
      }

      dialogueText.text = currentFormattedLine;
      isTyping = false;
      typingCoroutine = null;
   }

   // Starts a dialogue sequence on the overlay UI.
   //
   // This method is used by:
   // - NarrativeFlowManager (scene intro)
   // - NarrativeTutorialManager (step-based tutorial)
   // - BuildingTutorialGuideManager (one-time building guides)
   // - event-based tutorial prompts (enemy attack, refinery jam, etc.)
   //
   // Important:
   // - This method only handles dialogue display.
   // - It does not wait for arbitrary gameplay conditions by itself.
   // - If a tutorial step should continue after a specific button click,
   //   that waiting logic should usually live in the calling manager.
   public void PlaySequence(
      NPCEncounterSystem.NPCProfile profile,
      NPCEncounterSystem.DialogueLine[] dialogueLines,
      DialogueLayoutMode layoutMode,
      bool canSkip,
      Action onComplete)
   {
      if (profile == null || dialogueLines == null || dialogueLines.Length == 0)
         return;

      currentProfile = profile;
      currentLines = dialogueLines;
      currentLineIndex = 0;
      currentCompleteAction = onComplete;
      isSequencePlaying = true;

      ApplyDialogueLayout(layoutMode);

      skipButton.gameObject.SetActive(canSkip);
      nextButton.gameObject.SetActive(true);
      dialoguePanel.SetActive(true);

      ShowCurrentLine();
   }

   public void ShowTutorialToggle(Action<bool> onChoice)
   {
      currentToggleAction = onChoice;
      toggleTitleText.text = "Tutorial";
      toggleBodyText.text = "Would you like to play the new story tutorial?\n\nRecommended for first-time players.";
      togglePanel.SetActive(true);
   }

   private void HandleTutorialToggle(bool isTutorialEnabled)
   {
      TutorialFlowSettings.NarrativeTutorialEnabled = isTutorialEnabled;
      togglePanel.SetActive(false);
      currentToggleAction?.Invoke(isTutorialEnabled);
      currentToggleAction = null;
   }

   private void HandleNextClicked()
   {
      if (!isSequencePlaying)
         return;

      if (isTyping)
      {
         if (typingCoroutine != null)
         {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
         }

         isTyping = false;
         dialogueText.text = currentFormattedLine;
         return;
      }

      currentLineIndex++;

      if (currentLines == null || currentLineIndex >= currentLines.Length)
      {
         CompleteCurrentSequence();
         return;
      }

      ShowCurrentLine();
   }

   private void HandleSkipClicked()
   {
      if (!isSequencePlaying)
         return;

      CompleteCurrentSequence();
   }

   private void ShowCurrentLine()
   {
      if (currentLines == null || currentLineIndex >= currentLines.Length)
         return;

      NPCEncounterSystem.DialogueLine currentLine = currentLines[currentLineIndex];

      if (currentProfile != null && NPCEncounterSystem.Instance != null)
      {
         Sprite portrait = NPCEncounterSystem.Instance.GetPortraitForExpression(currentProfile, currentLine.expression);
         portraitImage.sprite = portrait;
         portraitImage.color = portrait != null ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
         nameText.text = currentProfile.npcName;
      }

      currentFormattedLine = currentLine.isAction
      ? $"<i>{currentLine.text}</i>"
      : currentLine.text;

      if (typingCoroutine != null)
         StopCoroutine(typingCoroutine);

      typingCoroutine = StartCoroutine(TypeLine(currentLine.text ?? string.Empty, currentLine.isAction));
   }

   private void CompleteCurrentSequence()
   {
      if (typingCoroutine != null)
      {
         StopCoroutine(typingCoroutine);
         typingCoroutine = null;
      }

      isTyping = false;
      isSequencePlaying = false;
      dialoguePanel.SetActive(false);

      Action cachedAction = currentCompleteAction;
      currentCompleteAction = null;
      cachedAction?.Invoke();
   }

   private void ApplyDialogueLayout(DialogueLayoutMode layoutMode)
   {
      RectTransform panelRect = dialoguePanel.GetComponent<RectTransform>();

      panelRect.sizeDelta = new Vector2(PANEL_WIDTH, PANEL_HEIGHT);

      switch (layoutMode)
      {
         case DialogueLayoutMode.Top:
            panelRect.anchorMin = new Vector2(0.5f, 1f);
            panelRect.anchorMax = new Vector2(0.5f, 1f);
            panelRect.pivot = new Vector2(0.5f, 1f);
            panelRect.anchoredPosition = new Vector2(0f, -48f);
            break;

         case DialogueLayoutMode.StoryBottom:
         default:
            panelRect.anchorMin = new Vector2(0.5f, 0f);
            panelRect.anchorMax = new Vector2(0.5f, 0f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.anchoredPosition = new Vector2(0f, 48f);
            break;
      }
   }

   public bool IsBusy()
   {
      return isSequencePlaying ||
             (dialoguePanel != null && dialoguePanel.activeSelf) ||
             (togglePanel != null && togglePanel.activeSelf);
   }

   public void DisposeOverlay()
   {
      HideAll();
      SetGameplayBlocked(false);

      if (canvasObject != null)
      {
         Destroy(canvasObject);
         canvasObject = null;
      }

      if (Instance == this)
         Instance = null;
   }
}