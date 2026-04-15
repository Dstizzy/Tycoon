using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Controls the ending scene flow using the same click-to-advance dialogue style
// as the NPC dialogue UI. The ending UI is generated entirely at runtime.
public class EndingSequenceManager : MonoBehaviour
{
   // ─────────────────────────────────────────────────────────────────────
   //  Data classes
   // ─────────────────────────────────────────────────────────────────────

   public enum EndingActionType
   {
      None,
      LaunchSubmarine,
      FloatSubmarine,
      StartFailureShake
   }

   [Serializable]
   public class EndingDialogueEntry
   {
      public NPCEncounterSystem.NPCPersonality speaker;
      public NPCEncounterSystem.DialogueLine dialogue;
      public EndingActionType actionType;

      public EndingDialogueEntry() { }

      public EndingDialogueEntry(
         NPCEncounterSystem.NPCPersonality speaker,
         string text,
         NPCEncounterSystem.ExpressionType expression = NPCEncounterSystem.ExpressionType.Neutral,
         bool isAction = false,
         EndingActionType actionType = EndingActionType.None)
      {
         this.speaker = speaker;
         dialogue = new NPCEncounterSystem.DialogueLine(text, expression, isAction);
         this.actionType = actionType;
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
   //  Layout constants
   // ─────────────────────────────────────────────────────────────────────

   private const float DEFAULT_TEXT_SPEED = 0.03f;

   private const float PANEL_WIDTH = 1200f;
   private const float PANEL_HEIGHT = 360f;
   private const float PORTRAIT_SIZE = 220f;
   private const float PADDING = 24f;

   private const float NAME_FONT_SIZE = 38f;
   private const float DIALOGUE_FONT_SIZE = 30f;
   private const float NAV_BUTTON_FONT_SIZE = 28f;

   private const float RESULT_TITLE_WIDTH = 720f;
   private const float RESULT_TITLE_HEIGHT = 180f;
   private const float RESULT_TITLE_TOP_OFFSET = -36f;

   private const float TRAVEL_LAYER_WIDTH = 2880f;
   private const float TRAVEL_LAYER_HEIGHT = 1620f;
   private const float TRAVEL_SUBMARINE_WIDTH = 720f;
   private const float TRAVEL_SUBMARINE_HEIGHT = 360f;
   private const float TRAVEL_SUBMARINE_START_Y = -380f;
   private const float TRAVEL_SUBMARINE_TARGET_Y = 0f;


   // ─────────────────────────────────────────────────────────────────────
   //  Inspector fields
   // ─────────────────────────────────────────────────────────────────────

   [Header("Scene")]
   [SerializeField] private string startSceneName = "StartScreenScene";

   [Header("Optional Visual Roots")]
   [SerializeField] private GameObject successRoot;
   [SerializeField] private GameObject failureRoot;

   [Header("Speaker Profiles")]
   [SerializeField] private EndingSpeakerProfile[] speakerProfiles;

   [Header("Success Visuals")]
   [SerializeField] private Transform[] successBuildings;
   [SerializeField] private Transform submarineTransform;
   [SerializeField] private float successBounceAmount = 12f;
   [SerializeField] private float successBounceSpeed = 2.5f;

   [Header("Success Travel Overlay")]
   [SerializeField] private Sprite successTravelBackgroundSprite;
   [SerializeField] private Sprite successTravelSubmarineSprite;
   [SerializeField] private float successTravelBackgroundScrollSpeed = 120f;
   [SerializeField] private float successTravelSubmarineRiseDuration = 1.8f;
   [SerializeField] private float successTravelSubmarineFloatAmount = 18f;
   [SerializeField] private float successTravelSubmarineFloatSpeed = 1.4f;

   [Header("Failure Visuals")]
   [SerializeField] private Transform[] failureBuildings;
   [SerializeField] private Transform cameraShakeTarget;
   [SerializeField] private float failureBounceAmount = 12f;
   [SerializeField] private float failureBounceSpeed = 6f;
   [SerializeField] private float cameraShakeStrength = 16f;
   [SerializeField] private int failureShakeStartLineIndex = 1;

   [Header("Dialogue Settings")]
   [SerializeField] private float textSpeed = DEFAULT_TEXT_SPEED;
   [SerializeField] private bool useTypewriterEffect = true;
   [SerializeField] private float fadeDuration = 1.5f;

   [Header("Title Images")]
   [SerializeField] private Sprite successTitleSprite;
   [SerializeField] private Sprite failureTitleSprite;

   [Header("Title Float Animation")]
   [SerializeField] private float successTitleFloatAmount = 12f;
   [SerializeField] private float successTitleFloatSpeed = 1f;

   [Header("Success Dialogue")]
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
      new EndingDialogueEntry(
         NPCEncounterSystem.NPCPersonality.Dolphin,
         "The launch platform rattles with cheers, laughter, and far too many people talking at once.",
         NPCEncounterSystem.ExpressionType.Neutral,
         isAction: true,
         actionType: EndingActionType.LaunchSubmarine)
   };

   [SerializeField]
   private EndingDialogueEntry[] successLaunchLines = new EndingDialogueEntry[]
{
   new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Dolphin, "Hey—!", NPCEncounterSystem.ExpressionType.Surprised),
   new EndingDialogueEntry(
      NPCEncounterSystem.NPCPersonality.Dolphin,
      "We're actually rising. We're actually rising!",
      NPCEncounterSystem.ExpressionType.Happy,
      actionType: EndingActionType.FloatSubmarine),
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

   [Header("Failure Dialogue")]
   [SerializeField]
   private EndingDialogueEntry[] failureLines = new EndingDialogueEntry[]
{
   new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Dolphin, "Wait. No. No no no—", NPCEncounterSystem.ExpressionType.Surprised),
   new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Turtle, "That sound is BAD! That's a very bad sound!", NPCEncounterSystem.ExpressionType.Surprised,
      actionType: EndingActionType.StartFailureShake),
   new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Dolphin, "Everybody run!", NPCEncounterSystem.ExpressionType.Special),
   new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Octopus, "What did you DO?! Why is everything shaking?!", NPCEncounterSystem.ExpressionType.Surprised),
   new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.HermitCrab, "This is deeply, profoundly bad for business.", NPCEncounterSystem.ExpressionType.Angry),
   new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Jellyfish, "...Too loud... too bright...", NPCEncounterSystem.ExpressionType.Angry),
   new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Seahorse, "Move! If you freeze here, you die here!", NPCEncounterSystem.ExpressionType.Special),
   new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Dolphin, "The entire outpost lurches as the volcano finally erupts.", NPCEncounterSystem.ExpressionType.Angry, isAction: true),
   new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Dolphin, "If we ever got another chance...", NPCEncounterSystem.ExpressionType.Thinking),
   new EndingDialogueEntry(NPCEncounterSystem.NPCPersonality.Dolphin, "I think... we could have done better.", NPCEncounterSystem.ExpressionType.Neutral)
};


   // ─────────────────────────────────────────────────────────────────────
   //  Runtime UI references
   // ─────────────────────────────────────────────────────────────────────

   private GameObject canvasObject;
   private GameObject dialoguePanel;
   private Image portraitImage;
   private TextMeshProUGUI speakerNameText;
   private TextMeshProUGUI bodyText;
   private Image fadeImage;
   private Button nextButton;
   private Button closeButton;
   private Image resultTitleImage;
   private GameObject successTravelLayer;
   private Image successTravelBackgroundImageA;
   private Image successTravelBackgroundImageB;
   private Image successTravelSubmarineImage;
   private RectTransform successTravelBackgroundRectA;
   private RectTransform successTravelBackgroundRectB;
   private RectTransform successTravelSubmarineRect;


   // ─────────────────────────────────────────────────────────────────────
   //  Runtime state
   // ─────────────────────────────────────────────────────────────────────

   private EndingDialogueEntry[] currentSequence;
   private int currentLineIndex;
   private bool isTyping;
   private bool isEndingActive;
   private bool isActionInProgress;
   private string currentFormattedLine;
   private Coroutine typingCoroutine;
   private Coroutine successBuildingsCoroutine;
   private Coroutine failureBuildingsCoroutine;
   private Coroutine failureShakeCoroutine;
   private Coroutine resultTitleFloatCoroutine;
   private bool isUIGenerated; 
   private Coroutine successTravelBackgroundCoroutine;
   private Coroutine successTravelSubmarineFloatCoroutine;


   // ─────────────────────────────────────────────────────────────────────
   //  Unity lifecycle
   // ─────────────────────────────────────────────────────────────────────

   // Initializes the ending scene roots.
   private void Awake()
   {
      if (successRoot != null)
         successRoot.SetActive(false);

      if (failureRoot != null)
         failureRoot.SetActive(false);
   }

   // Builds the runtime UI and starts the selected ending route.
   private void Start()
   {
      EnsureUIGenerated();
      StartCoroutine(BeginEndingFlow());
   }

   private void OnDestroy()
   {
      if (canvasObject != null)
         Destroy(canvasObject);
   }

   // ─────────────────────────────────────────────────────────────────────
   //  UI generation
   // ─────────────────────────────────────────────────────────────────────

   // Ensures the ending UI is created only once.
   private void EnsureUIGenerated()
   {
      if (isUIGenerated)
         return;

      GenerateUI();
   }

   // Builds the full ending dialogue canvas at runtime.
   private void GenerateUI()
   {
      canvasObject = new GameObject("EndingDialogueCanvas");

      Canvas canvas = canvasObject.AddComponent<Canvas>();
      canvas.renderMode = RenderMode.ScreenSpaceOverlay;
      canvas.sortingOrder = 999;

      CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
      scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
      scaler.referenceResolution = new Vector2(1920, 1080);
      scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
      scaler.matchWidthOrHeight = 0.5f;

      canvasObject.AddComponent<GraphicRaycaster>();

      // Fade image
      GameObject fadeObject = CreatePanel("FadeImage", canvasObject.transform);
      fadeImage = fadeObject.GetComponent<Image>();
      fadeImage.color = new Color(0f, 0f, 0f, 1f);

      RectTransform fadeRect = fadeObject.GetComponent<RectTransform>();
      fadeRect.anchorMin = Vector2.zero;
      fadeRect.anchorMax = Vector2.one;
      fadeRect.offsetMin = Vector2.zero;
      fadeRect.offsetMax = Vector2.zero;

      GameObject resultTitleObject = CreateUIObject("ResultTitleImage", canvasObject.transform);
      RectTransform resultTitleRect = resultTitleObject.GetComponent<RectTransform>();
      resultTitleRect.anchorMin = new Vector2(0.5f, 1f);
      resultTitleRect.anchorMax = new Vector2(0.5f, 1f);
      resultTitleRect.pivot = new Vector2(0.5f, 1f);
      resultTitleRect.sizeDelta = new Vector2(RESULT_TITLE_WIDTH, RESULT_TITLE_HEIGHT);
      resultTitleRect.anchoredPosition = new Vector2(0f, RESULT_TITLE_TOP_OFFSET);

      resultTitleImage = resultTitleObject.AddComponent<Image>();
      resultTitleImage.preserveAspect = true;
      resultTitleImage.color = Color.white;
      resultTitleImage.gameObject.SetActive(false);

      BuildSuccessTravelOverlay();

      // Main panel
      dialoguePanel = CreatePanel("EndingDialoguePanel", canvasObject.transform);
      RectTransform panelRect = dialoguePanel.GetComponent<RectTransform>();
      panelRect.anchorMin = new Vector2(0.5f, 0f);
      panelRect.anchorMax = new Vector2(0.5f, 0f);
      panelRect.pivot = new Vector2(0.5f, 0f);
      panelRect.sizeDelta = new Vector2(PANEL_WIDTH, PANEL_HEIGHT);
      panelRect.anchoredPosition = new Vector2(0f, 48f);
      panelRect.localScale = Vector3.one;

      Image panelBg = dialoguePanel.GetComponent<Image>();
      panelBg.color = new Color(0.08f, 0.12f, 0.18f, 0.96f);

      Outline outline = dialoguePanel.AddComponent<Outline>();
      outline.effectColor = new Color(0.35f, 0.55f, 0.8f, 0.9f);
      outline.effectDistance = new Vector2(2f, 2f);

      // Portrait frame
      GameObject portraitFrame = CreatePanel("PortraitFrame", dialoguePanel.transform);
      RectTransform portraitFrameRect = portraitFrame.GetComponent<RectTransform>();
      portraitFrameRect.anchorMin = new Vector2(0f, 0f);
      portraitFrameRect.anchorMax = new Vector2(0f, 1f);
      portraitFrameRect.offsetMin = new Vector2(PADDING, PADDING);
      portraitFrameRect.offsetMax = new Vector2(PORTRAIT_SIZE + PADDING, -PADDING);

      Image portraitFrameImage = portraitFrame.GetComponent<Image>();
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
      portraitImage.color = new Color(1f, 1f, 1f, 0f);

      // Speaker name
      GameObject speakerObject = CreateUIObject("SpeakerNameText", dialoguePanel.transform);
      RectTransform speakerRect = speakerObject.GetComponent<RectTransform>();
      speakerRect.anchorMin = new Vector2(0f, 1f);
      speakerRect.anchorMax = new Vector2(1f, 1f);
      speakerRect.offsetMin = new Vector2(PORTRAIT_SIZE + PADDING * 2f, -88f);
      speakerRect.offsetMax = new Vector2(-PADDING, -PADDING);

      speakerNameText = speakerObject.AddComponent<TextMeshProUGUI>();
      speakerNameText.fontSize = NAME_FONT_SIZE;
      speakerNameText.fontStyle = FontStyles.Bold;
      speakerNameText.color = new Color(0.95f, 0.85f, 0.55f, 1f);
      speakerNameText.alignment = TextAlignmentOptions.TopLeft;

      // Dialogue text
      GameObject bodyObject = CreateUIObject("BodyText", dialoguePanel.transform);
      RectTransform bodyRect = bodyObject.GetComponent<RectTransform>();
      bodyRect.anchorMin = new Vector2(0f, 0f);
      bodyRect.anchorMax = new Vector2(1f, 1f);
      bodyRect.offsetMin = new Vector2(PORTRAIT_SIZE + PADDING * 2f, 96f);
      bodyRect.offsetMax = new Vector2(-PADDING, -96f);

      bodyText = bodyObject.AddComponent<TextMeshProUGUI>();
      bodyText.fontSize = DIALOGUE_FONT_SIZE;
      bodyText.color = Color.white;
      bodyText.alignment = TextAlignmentOptions.TopLeft;
      bodyText.enableWordWrapping = true;
      bodyText.richText = true;

      // Next button
      nextButton = CreateButton("NextButton", dialoguePanel.transform, "Next >", out TextMeshProUGUI _);
      RectTransform nextRect = nextButton.GetComponent<RectTransform>();
      nextRect.anchorMin = new Vector2(1f, 0f);
      nextRect.anchorMax = new Vector2(1f, 0f);
      nextRect.pivot = new Vector2(1f, 0f);
      nextRect.sizeDelta = new Vector2(180f, 64f);
      nextRect.anchoredPosition = new Vector2(-PADDING, PADDING);
      nextButton.onClick.AddListener(OnNextClicked);

      // Close button
      closeButton = CreateButton("CloseButton", dialoguePanel.transform, "Return", out TextMeshProUGUI _);
      RectTransform closeRect = closeButton.GetComponent<RectTransform>();
      closeRect.anchorMin = new Vector2(1f, 0f);
      closeRect.anchorMax = new Vector2(1f, 0f);
      closeRect.pivot = new Vector2(1f, 0f);
      closeRect.sizeDelta = new Vector2(180f, 64f);
      closeRect.anchoredPosition = new Vector2(-PADDING, PADDING);
      closeButton.onClick.AddListener(OnCloseClicked);

      closeButton.gameObject.SetActive(false);
      dialoguePanel.SetActive(false);
      isUIGenerated = true;
   }

   private GameObject CreatePanel(string name, Transform parent)
   {
      GameObject go = new GameObject(name);
      go.transform.SetParent(parent, false);
      go.AddComponent<RectTransform>().localScale = Vector3.one;
      go.AddComponent<Image>();
      return go;
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
      textComponent.fontSize = NAV_BUTTON_FONT_SIZE;
      textComponent.fontStyle = FontStyles.Bold;
      textComponent.color = Color.white;
      textComponent.alignment = TextAlignmentOptions.Center;

      return button;
   }

   private void BuildSuccessTravelOverlay()
   {
      successTravelLayer = CreateUIObject("SuccessTravelLayer", canvasObject.transform);
      RectTransform layerRect = successTravelLayer.GetComponent<RectTransform>();
      layerRect.anchorMin = Vector2.zero;
      layerRect.anchorMax = Vector2.one;
      layerRect.offsetMin = Vector2.zero;
      layerRect.offsetMax = Vector2.zero;

      successTravelBackgroundImageA = CreateTravelImage("SuccessTravelBackgroundA", successTravelLayer.transform, out successTravelBackgroundRectA);
      successTravelBackgroundRectA.anchorMin = new Vector2(0.5f, 0.5f);
      successTravelBackgroundRectA.anchorMax = new Vector2(0.5f, 0.5f);
      successTravelBackgroundRectA.pivot = new Vector2(0.5f, 0.5f);
      successTravelBackgroundRectA.sizeDelta = new Vector2(TRAVEL_LAYER_WIDTH, TRAVEL_LAYER_HEIGHT);
      successTravelBackgroundRectA.anchoredPosition = Vector2.zero;

      successTravelBackgroundImageB = CreateTravelImage("SuccessTravelBackgroundB", successTravelLayer.transform, out successTravelBackgroundRectB);
      successTravelBackgroundRectB.anchorMin = new Vector2(0.5f, 0.5f);
      successTravelBackgroundRectB.anchorMax = new Vector2(0.5f, 0.5f);
      successTravelBackgroundRectB.pivot = new Vector2(0.5f, 0.5f);
      successTravelBackgroundRectB.sizeDelta = new Vector2(TRAVEL_LAYER_WIDTH, TRAVEL_LAYER_HEIGHT);
      successTravelBackgroundRectB.anchoredPosition = new Vector2(-TRAVEL_LAYER_WIDTH, 0f);

      successTravelSubmarineImage = CreateTravelImage("SuccessTravelSubmarine", successTravelLayer.transform, out successTravelSubmarineRect);
      successTravelSubmarineRect.anchorMin = new Vector2(0.5f, 0.5f);
      successTravelSubmarineRect.anchorMax = new Vector2(0.5f, 0.5f);
      successTravelSubmarineRect.pivot = new Vector2(0.5f, 0.5f);
      successTravelSubmarineRect.sizeDelta = new Vector2(TRAVEL_SUBMARINE_WIDTH, TRAVEL_SUBMARINE_HEIGHT);
      successTravelSubmarineRect.anchoredPosition = new Vector2(0f, TRAVEL_SUBMARINE_START_Y);

      successTravelLayer.SetActive(false);
   }

   private Image CreateTravelImage(string objectName, Transform parent, out RectTransform rectTransform)
   {
      GameObject imageObject = CreateUIObject(objectName, parent);
      rectTransform = imageObject.GetComponent<RectTransform>();

      Image image = imageObject.AddComponent<Image>();
      image.preserveAspect = false;
      image.color = Color.white;
      return image;
   }

   // ─────────────────────────────────────────────────────────────────────
   //  Main flow
   // ─────────────────────────────────────────────────────────────────────

   private IEnumerator BeginEndingFlow()
   {
      dialoguePanel.SetActive(true);
      SetFadeImmediate(1f);

      if (GameEndingState.CurrentResult == GameEndingState.EndingResult.None)
      {
         speakerNameText.text = string.Empty;
         bodyText.text = "No ending state was set before loading this scene.";
         nextButton.gameObject.SetActive(false);
         closeButton.gameObject.SetActive(true);
         yield return FadeTo(0f, fadeDuration);
         yield break;
      }

      bool isSuccess = GameEndingState.CurrentResult == GameEndingState.EndingResult.Success;

      if (successRoot != null)
         successRoot.SetActive(isSuccess);

      if (failureRoot != null)
         failureRoot.SetActive(!isSuccess);

      isEndingActive = true;

      if (isSuccess)
      {
         if (successBuildings != null && successBuildings.Length > 0)
            successBuildingsCoroutine = StartCoroutine(AnimateBuildingsLoop(successBuildings, successBounceAmount, successBounceSpeed));

         ResetSuccessTravelOverlay();
         currentSequence = BuildSuccessSequence();
      }
      else
      {
         if (failureBuildings != null && failureBuildings.Length > 0)
            failureBuildingsCoroutine = StartCoroutine(AnimateBuildingsLoop(failureBuildings, failureBounceAmount, failureBounceSpeed));

         currentSequence = failureLines;
      }

      Sprite resultSprite = isSuccess ? successTitleSprite : failureTitleSprite;

      if (resultTitleImage != null)
      {
         resultTitleImage.sprite = resultSprite;
         resultTitleImage.gameObject.SetActive(resultSprite != null);

         if (resultTitleFloatCoroutine != null)
            StopCoroutine(resultTitleFloatCoroutine);

         if (isSuccess && resultSprite != null)
            resultTitleFloatCoroutine = StartCoroutine(AnimateResultTitleFloatLoop());
      }

      currentLineIndex = 0;
      closeButton.gameObject.SetActive(false);
      nextButton.gameObject.SetActive(true);

      yield return FadeTo(0f, fadeDuration);
      ShowCurrentLine();
   }

   private EndingDialogueEntry[] BuildSuccessSequence()
   {
      List<EndingDialogueEntry> sequence = new List<EndingDialogueEntry>();

      AddEntries(sequence, successLines);
      AddEntries(sequence, successLaunchLines);
      AddEntries(sequence, successPostLaunchLines);

      return sequence.ToArray();
   }

   private void AddEntries(List<EndingDialogueEntry> target, EndingDialogueEntry[] entries)
   {
      if (entries == null || entries.Length == 0)
         return;

      foreach (EndingDialogueEntry entry in entries)
      {
         if (entry != null)
            target.Add(entry);
      }
   }

   private void ResetSuccessTravelOverlay()
   {
      if (successTravelLayer != null)
         successTravelLayer.SetActive(false);

      if (successTravelBackgroundImageA != null)
         successTravelBackgroundImageA.sprite = successTravelBackgroundSprite;

      if (successTravelBackgroundImageB != null)
         successTravelBackgroundImageB.sprite = successTravelBackgroundSprite;

      if (successTravelSubmarineImage != null)
         successTravelSubmarineImage.sprite = successTravelSubmarineSprite;

      if (successTravelBackgroundRectA != null)
         successTravelBackgroundRectA.anchoredPosition = Vector2.zero;

      if (successTravelBackgroundRectB != null)
         successTravelBackgroundRectB.anchoredPosition = new Vector2(-TRAVEL_LAYER_WIDTH, 0f);

      if (successTravelSubmarineRect != null)
         successTravelSubmarineRect.anchoredPosition = new Vector2(0f, TRAVEL_SUBMARINE_START_Y);

      if (successTravelBackgroundCoroutine != null)
      {
         StopCoroutine(successTravelBackgroundCoroutine);
         successTravelBackgroundCoroutine = null;
      }

      if (successTravelSubmarineFloatCoroutine != null)
      {
         StopCoroutine(successTravelSubmarineFloatCoroutine);
         successTravelSubmarineFloatCoroutine = null;
      }

      if (submarineTransform != null)
         submarineTransform.gameObject.SetActive(true);
   }

   // ─────────────────────────────────────────────────────────────────────
   //  Dialogue display
   // ─────────────────────────────────────────────────────────────────────

   private void ShowCurrentLine()
   {
      if (currentSequence == null || currentLineIndex >= currentSequence.Length)
      {
         CompleteSequence();
         return;
      }

      EndingDialogueEntry currentEntry = currentSequence[currentLineIndex];
      if (currentEntry == null || currentEntry.dialogue == null)
      {
         currentLineIndex++;
         ShowCurrentLine();
         return;
      }

      ApplySpeakerUI(currentEntry.speaker, currentEntry.dialogue.expression);

      string text = currentEntry.dialogue.text ?? string.Empty;
      currentFormattedLine = currentEntry.dialogue.isAction ? $"<i>{text}</i>" : text;

      if (typingCoroutine != null)
         StopCoroutine(typingCoroutine);

      if (useTypewriterEffect)
         typingCoroutine = StartCoroutine(TypeLine(text, currentEntry.dialogue.isAction));
      else
      {
         bodyText.text = currentFormattedLine;
         isTyping = false;
         typingCoroutine = null;
      }

      HandleFailureLineStart();
   }

   private IEnumerator TypeLine(string rawText, bool isAction)
   {
      isTyping = true;
      bodyText.text = string.Empty;

      if (isAction)
      {
         for (int index = 0; index < rawText.Length; index++)
         {
            bodyText.text = $"<i>{rawText.Substring(0, index + 1)}</i>";
            yield return new WaitForSeconds(textSpeed);
         }
      }
      else
      {
         for (int index = 0; index < rawText.Length; index++)
         {
            bodyText.text = rawText.Substring(0, index + 1);
            yield return new WaitForSeconds(textSpeed);
         }
      }

      bodyText.text = currentFormattedLine;
      isTyping = false;
      typingCoroutine = null;
   }

   private void OnNextClicked()
   {
      if (isActionInProgress)
         return;

      if (isTyping)
      {
         if (typingCoroutine != null)
         {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
         }

         isTyping = false;
         bodyText.text = currentFormattedLine;
         return;
      }

      if (currentSequence == null || currentLineIndex >= currentSequence.Length)
      {
         CompleteSequence();
         return;
      }

      EndingDialogueEntry currentEntry = currentSequence[currentLineIndex];
      if (currentEntry != null && currentEntry.actionType != EndingActionType.None)
      {
         StartCoroutine(ExecuteEntryAction(currentEntry.actionType));
         return;
      }

      AdvanceLine();
   }

   private void AdvanceLine()
   {
      currentLineIndex++;
      ShowCurrentLine();
   }

   private void CompleteSequence()
   {
      nextButton.gameObject.SetActive(false);
      closeButton.gameObject.SetActive(true);
   }

   private void OnCloseClicked()
   {
      StartCoroutine(CloseAndReturnToStartScene());
   }


   // ─────────────────────────────────────────────────────────────────────
   //  Speaker UI helpers
   // ─────────────────────────────────────────────────────────────────────

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
   //  Action helpers
   // ─────────────────────────────────────────────────────────────────────

   private IEnumerator ExecuteEntryAction(EndingActionType actionType)
   {
      isActionInProgress = true;
      nextButton.interactable = false;

      switch (actionType)
      {
         case EndingActionType.LaunchSubmarine:
            yield return PlaySuccessTravelEntranceSequence();
            break;

         case EndingActionType.FloatSubmarine:
            StartSuccessTravelFloating();
            break;

         case EndingActionType.StartFailureShake:
            if (failureShakeCoroutine == null && cameraShakeTarget != null)
               failureShakeCoroutine = StartCoroutine(ShakeTransformLoop(cameraShakeTarget, cameraShakeStrength));
            break;
      }

      nextButton.interactable = true;
      isActionInProgress = false;
      AdvanceLine();
   }

   private void HandleFailureLineStart()
   {
      if (GameEndingState.CurrentResult != GameEndingState.EndingResult.Failure)
         return;

      if (failureShakeCoroutine != null)
         return;

      if (currentLineIndex < Mathf.Max(0, failureShakeStartLineIndex))
         return;

      if (cameraShakeTarget == null)
         return;

      failureShakeCoroutine = StartCoroutine(ShakeTransformLoop(cameraShakeTarget, cameraShakeStrength));
   }


   // ─────────────────────────────────────────────────────────────────────
   //  Animation helpers
   // ─────────────────────────────────────────────────────────────────────

   private IEnumerator AnimateBuildingsLoop(Transform[] targets, float amount, float speed)
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

      while (isEndingActive)
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

   private IEnumerator AnimateResultTitleFloatLoop()
   {
      if (resultTitleImage == null)
         yield break;

      RectTransform titleRect = resultTitleImage.rectTransform;
      Vector2 basePosition = titleRect.anchoredPosition;
      float elapsed = 0f;

      while (isEndingActive && resultTitleImage.gameObject.activeSelf)
      {
         elapsed += Time.deltaTime;

         float yOffset = Mathf.Sin(elapsed * successTitleFloatSpeed * Mathf.PI * 2f) * successTitleFloatAmount;
         titleRect.anchoredPosition = basePosition + new Vector2(0f, yOffset);

         yield return null;
      }

      titleRect.anchoredPosition = basePosition;
   }

private IEnumerator ShakeTransformLoop(Transform target, float strength)
   {
      if (target == null)
         yield break;

      Vector3 originalPosition = target.localPosition;

      while (isEndingActive)
      {
         float offsetX = UnityEngine.Random.Range(-strength, strength);
         float offsetY = UnityEngine.Random.Range(-strength, strength);

         target.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);
         yield return null;
      }

      target.localPosition = originalPosition;
   }

   private IEnumerator PlaySuccessTravelOverlaySequence()
   {
      if (successTravelLayer == null || successTravelSubmarineRect == null)
         yield break;

      successTravelLayer.SetActive(true);

      if (submarineTransform != null)
         submarineTransform.gameObject.SetActive(false);

      successTravelSubmarineRect.anchoredPosition = new Vector2(0f, TRAVEL_SUBMARINE_START_Y);

      float elapsed = 0f;

      while (elapsed < successTravelSubmarineRiseDuration)
      {
         elapsed += Time.deltaTime;
         float t = Mathf.Clamp01(elapsed / successTravelSubmarineRiseDuration);
         float easedT = Mathf.SmoothStep(0f, 1f, t);

         float currentY = Mathf.Lerp(TRAVEL_SUBMARINE_START_Y, TRAVEL_SUBMARINE_TARGET_Y, easedT);
         successTravelSubmarineRect.anchoredPosition = new Vector2(0f, currentY);

         yield return null;
      }

      successTravelSubmarineRect.anchoredPosition = new Vector2(0f, TRAVEL_SUBMARINE_TARGET_Y);

      if (successTravelBackgroundCoroutine == null)
         successTravelBackgroundCoroutine = StartCoroutine(AnimateSuccessTravelBackgroundScrollLoop());

      if (successTravelSubmarineFloatCoroutine == null)
         successTravelSubmarineFloatCoroutine = StartCoroutine(AnimateSuccessTravelSubmarineFloatLoop());
   }

   private IEnumerator AnimateSuccessTravelBackgroundScrollLoop()
   {
      if (successTravelBackgroundRectA == null || successTravelBackgroundRectB == null)
         yield break;

      while (isEndingActive && successTravelLayer != null && successTravelLayer.activeSelf)
      {
         float delta = successTravelBackgroundScrollSpeed * Time.deltaTime;

         successTravelBackgroundRectA.anchoredPosition += Vector2.right * delta;
         successTravelBackgroundRectB.anchoredPosition += Vector2.right * delta;

         if (successTravelBackgroundRectA.anchoredPosition.x >= TRAVEL_LAYER_WIDTH)
            successTravelBackgroundRectA.anchoredPosition = new Vector2(successTravelBackgroundRectB.anchoredPosition.x - TRAVEL_LAYER_WIDTH, 0f);

         if (successTravelBackgroundRectB.anchoredPosition.x >= TRAVEL_LAYER_WIDTH)
            successTravelBackgroundRectB.anchoredPosition = new Vector2(successTravelBackgroundRectA.anchoredPosition.x - TRAVEL_LAYER_WIDTH, 0f);

         yield return null;
      }
   }

   private IEnumerator AnimateSuccessTravelSubmarineFloatLoop()
   {
      if (successTravelSubmarineRect == null)
         yield break;

      float elapsed = 0f;
      Vector2 basePosition = new Vector2(0f, TRAVEL_SUBMARINE_TARGET_Y);

      while (isEndingActive && successTravelLayer != null && successTravelLayer.activeSelf)
      {
         elapsed += Time.deltaTime;

         float yOffset = Mathf.Sin(elapsed * successTravelSubmarineFloatSpeed) * successTravelSubmarineFloatAmount;
         float xOffset = Mathf.Sin(elapsed * successTravelSubmarineFloatSpeed * 0.55f) * (successTravelSubmarineFloatAmount * 0.35f);

         successTravelSubmarineRect.anchoredPosition = basePosition + new Vector2(xOffset, yOffset);
         yield return null;
      }

      successTravelSubmarineRect.anchoredPosition = basePosition;
   }

   private IEnumerator PlaySuccessTravelEntranceSequence()
   {
      if (successTravelLayer == null || successTravelSubmarineRect == null)
         yield break;

      successTravelLayer.SetActive(true);

      if (submarineTransform != null)
         submarineTransform.gameObject.SetActive(false);

      successTravelSubmarineRect.anchoredPosition = new Vector2(0f, TRAVEL_SUBMARINE_START_Y);

      float elapsed = 0f;

      while (elapsed < successTravelSubmarineRiseDuration)
      {
         elapsed += Time.deltaTime;
         float t = Mathf.Clamp01(elapsed / successTravelSubmarineRiseDuration);
         float easedT = Mathf.SmoothStep(0f, 1f, t);

         float currentY = Mathf.Lerp(TRAVEL_SUBMARINE_START_Y, TRAVEL_SUBMARINE_TARGET_Y, easedT);
         successTravelSubmarineRect.anchoredPosition = new Vector2(0f, currentY);

         yield return null;
      }

      successTravelSubmarineRect.anchoredPosition = new Vector2(0f, TRAVEL_SUBMARINE_TARGET_Y);
   }

   private void StartSuccessTravelFloating()
   {
      if (successTravelBackgroundCoroutine == null)
         successTravelBackgroundCoroutine = StartCoroutine(AnimateSuccessTravelBackgroundScrollLoop());

      if (successTravelSubmarineFloatCoroutine == null)
         successTravelSubmarineFloatCoroutine = StartCoroutine(AnimateSuccessTravelSubmarineFloatLoop());
   }

   // ─────────────────────────────────────────────────────────────────────
   //  Fade / cleanup
   // ─────────────────────────────────────────────────────────────────────

   private void SetFadeImmediate(float alpha)
   {
      if (fadeImage == null)
         return;

      fadeImage.gameObject.SetActive(true);

      Color currentColor = fadeImage.color;
      currentColor.a = alpha;
      fadeImage.color = currentColor;
   }

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

      if (targetAlpha <= 0f)
         fadeImage.gameObject.SetActive(false);
   }

   private IEnumerator CloseAndReturnToStartScene()
   {
      if (successTravelBackgroundCoroutine != null)
      {
         StopCoroutine(successTravelBackgroundCoroutine);
         successTravelBackgroundCoroutine = null;
      }

      if (successTravelSubmarineFloatCoroutine != null)
      {
         StopCoroutine(successTravelSubmarineFloatCoroutine);
         successTravelSubmarineFloatCoroutine = null;
      }

      if (resultTitleFloatCoroutine != null)
      {
         StopCoroutine(resultTitleFloatCoroutine);
         resultTitleFloatCoroutine = null;
      }

      nextButton.interactable = false;
      closeButton.interactable = false;
      isEndingActive = false;

      yield return FadeTo(1f, fadeDuration);

      ForgeManager.forgeLevel = 1;

      CleanupPersistentObjects();
      GameEndingState.Reset();
      SceneManager.LoadScene(startSceneName);
   }

   private void CleanupPersistentObjects()
   {
      if (NarrativeOverlayUI.Instance != null)
         NarrativeOverlayUI.Instance.DisposeOverlay();

      DestroySingleton(BuildingTutorialGuideManager.Instance);
      DestroySingleton(NPCEncounterSystem.Instance);
      DestroySingleton(InventoryManager.Instance);
      DestroySingleton(TradeHutManager.Instance);
      DestroySingleton(LabManager.labManager);
      DestroySingleton(ForgeManager.Instance);
      DestroySingleton(OreRefinery_Manager.Instance);
      DestroySingleton(ExplorationUnitManager.Instance);
      DestroySingleton(PopUpManager.Instance);
      DestroySingleton(HoverScript.Instance);
      DestroySingleton(TickerSystem.Instance);
   }

   private void DestroySingleton(MonoBehaviour currentSingleton)
   {
      if (currentSingleton != null)
         Destroy(currentSingleton.gameObject);
   }
}