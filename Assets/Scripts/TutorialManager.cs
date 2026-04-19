using System;
using System.Collections;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
   [System.Serializable]
   public class TutorialStep
   {
      public NPCEncounterSystem.NPCPersonality speaker;

      // Internal note for designers / developers.
      // Useful in the Inspector to identify the purpose of the step quickly.
      [TextArea] public string note;

      // Controls whether the dialogue appears at the bottom or top of the screen.
      public DialogueLayoutMode layoutMode = DialogueLayoutMode.StoryBottom;

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
   [SerializeField] private TutorialStep[] tutorialSteps;   // Array to hold all tutorial steps for easy management
   [SerializeField] private GameObject[] tutorialSections;  // Array to hold all tutorial sections for easy management
   [SerializeField] private GameObject   oreRefineryCanvas; // building canvas of the ore refinery
   [SerializeField] private GameObject   forgeCanvas;       // building canvas of the forge
   [SerializeField] private GameObject   tradeHutCanvas;    // building canvas of the trade hut
   [SerializeField] private GameObject   explorationCanvas; // building canvas of the exploration
   [SerializeField] private GameObject   labCanvas;         // building canvas of the lab
   [SerializeField] private GameObject   turnButton;        // Reference to the button that must be clicked to proceed

   [Header("Custom Fonts")]
   [SerializeField] private TMP_FontAsset chubbyFont;


   public int  tutorialIndex   = 0;           // To track the current tutorial section
   private static int  sectionIndex    = 0;           // To track the current section within a tutorial
   public         bool requiredButtonClicked = true;  // Flag to check if the required button has been clicked
   public         bool oreRefineryUpgrade    = false; // Flag to check if the ore refinery upgrade has been completed
   public         bool forgeFunction         = false; // Checks if the forge function has been explained
   public         bool tradeHutFunction      = false; // Checks if the trade hut function has been explained
   public         bool tradeHutFunctionTwo   = false; // Checks if the second trade hut function has been explained
   public         bool explorationFunction   = false; // Checks if the exploration function has been explained
   public         bool labFunction           = false; // Checks if the lab function has been explained
   public         bool enemyFunction         = false; // Checks if the enemy function has been explained
   public         bool victoryFunction       = false; // Checks if the victory function has been explained
   public         bool oreUpgradeButton      = false; // Flag to check if the ore refinery upgrade button has been clicked
   public         bool isWalkthroughGoing    = false;  // Flag to check if the tutorial is still going
   public static  int  stepIndex             = 0;      // To track the current part within a section
   public bool         isRequiredButtonClicked = true;

   public static TutorialManager Instance { get; private set; }

   private bool isWalkthroughRunning;

   public enum DialogueLayoutMode
   {
      StoryBottom,
      Top
   }

   private bool isWaitingForCustomAction;
   private GameObject currentCustomObjective;

   // Add a reference for the Next Button's container if you want to hide it specifically
   private GameObject nextButtonObject => nextButton.gameObject;

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

   private TutorialStep currentStep;

   private void Awake()
   {
      currentStep = tutorialSteps[stepIndex];
      if (Instance != null && Instance != this)
      {
         Destroy(gameObject);
         return;
      }

      Instance = this;
      BuildUI();
      HideAll();
   }

   private void OnEnable()
   {
      PopUpManager.OnHoverTagChanged += HandleGlobalHover;
      ForgeManager.HandleTutorial += HandleNextStep;
      TradeHutManager.HandleTutorial += HandleNextStep;
   }

   private void OnDisable()
   {
      PopUpManager.OnHoverTagChanged -= HandleGlobalHover;
      ForgeManager.HandleTutorial -= HandleNextStep;
      TradeHutManager.HandleTutorial -= HandleNextStep;
   }

   // Store the reference to the current part so we can toggle arrows from the event
   public GameObject currentActivePart = null;
   public GameObject currentActiveSection = null;

   // Event to start tutorial parts that can only be handled in other managers
   public static event Action HandleForgeTutorial;
   public static event Action<int> HandleTradeHutTutorial;

   // Start is called before the first frame update
   /*private void Start()
   {
      sectionIndex = 1;
      tutorialSections[tutorialIndex].SetActive(true);
      GoThroughSection(tutorialSections[tutorialIndex], sectionIndex);

   }*/

   public void BeginWalkthrough(Action onTutorialFinished)
   {
      sectionIndex = 1;
      GoThroughSection(tutorialSections[tutorialIndex], sectionIndex);
      if (isWalkthroughRunning)
         return;

      if (tutorialSteps == null || tutorialSteps.Length == 0)
      {
         Debug.LogWarning("[NarrativeTutorialManager] No tutorial steps found. Completing immediately.");
         onTutorialFinished?.Invoke();
         return;
      }

      Debug.Log($"[NarrativeTutorialManager] Starting tutorial with {tutorialSteps.Length} steps.");
      StartCoroutine(ShowDialogue(onTutorialFinished));
   }

   public void StartWalkthrough()
   {
      isWalkthroughGoing = true;
   }
   // Method to manage the flow of the tutorial sections and their subsections
   private void GoThroughSection(GameObject tutorialSection, int sectionIndex)
   {

      switch(sectionIndex)
      {
         case 1:
            tutorialSection.transform.Find("FirstPart").gameObject.SetActive(true);
            DoAllChecks(tutorialSection.transform.Find("FirstPart").gameObject);
            break;
         case 2:
            if (tutorialSection.transform.Find("SecondPart") != null)
            {
               tutorialSection.transform.Find("FirstPart").gameObject.SetActive(false);
               tutorialSection.transform.Find("SecondPart").gameObject.SetActive(true);
               DoAllChecks(tutorialSection.transform.Find("SecondPart").gameObject);
            }
            else
            {
               GoToNext();
            }
            break;
         case 3:
            if (tutorialSection.transform.Find("ThirdPart") != null)
            {
               tutorialSection.transform.Find("FirstPart").gameObject.SetActive(false);
               tutorialSection.transform.Find("SecondPart").gameObject.SetActive(false);
               tutorialSection.transform.Find("ThirdPart").gameObject.SetActive(true);
               DoAllChecks(tutorialSection.transform.Find("ThirdPart").gameObject);
            }
            else
            {
               GoToNext();
            }
            break;
         case 4:
            if (tutorialSection.transform.Find("FourthPart") != null)
            {
               tutorialSection.transform.Find("ThirdPart").gameObject.SetActive(false);
               tutorialSection.transform.Find("FourthPart").gameObject.SetActive(true);
               DoAllChecks(tutorialSection.transform.Find("FourthPart").gameObject);
            }
            else
            {
               GoToNext();
            }
            break;
         case 5:
            if (tutorialSection.transform.Find("FifthPart") != null)
            {
               tutorialSection.transform.Find("FourthPart").gameObject.SetActive(false);
               tutorialSection.transform.Find("FifthPart").gameObject.SetActive(true);
               DoAllChecks(tutorialSection.transform.Find("FifthPart").gameObject);
            }
            else
            {
               GoToNext();
            }
            break;
         case 6:
            if (tutorialSection.transform.Find("SixthPart") != null)
            {
               tutorialSection.transform.Find("FifthPart").gameObject.SetActive(false);
               tutorialSection.transform.Find("SixthPart").gameObject.SetActive(true);
               DoAllChecks(tutorialSection.transform.Find("SixthPart").gameObject);
            }
            else
            {
               GoToNext();
            }
            break;
         case 7:
            if (tutorialSection.transform.Find("SeventhPart") != null)
            {
               tutorialSection.transform.Find("SixthPart").gameObject.SetActive(false);
               tutorialSection.transform.Find("SeventhPart").gameObject.SetActive(true);
               //DoAllChecks(tutorialSection.transform.Find("SeventhPart").gameObject);
            }
            else
            {
               GoToNext();
            }
            break;
         case 8:
            if (tutorialSection.transform.Find("EighthPart") != null)
            {
               tutorialSection.transform.Find("SeventhPart").gameObject.SetActive(false);
               tutorialSection.transform.Find("EighthPart").gameObject.SetActive(true);
               //DoAllChecks(tutorialSection.transform.Find("EighthPart").gameObject);
            }
            else
            {
               GoToNext();
            }
            break;
         case 9:
            if (tutorialSection.transform.Find("NinthPart") != null)
            {
               tutorialSection.transform.Find("EighthPart").gameObject.SetActive(false);
               tutorialSection.transform.Find("NinthPart").gameObject.SetActive(true);
               //DoAllChecks(tutorialSection.transform.Find("NinthPart").gameObject);
            }
            else
            {
               GoToNext();
            }
            break;
         case 10:
            if (tutorialSection.transform.Find("TenthPart") != null)
            {
               tutorialSection.transform.Find("NinthPart").gameObject.SetActive(false);
               tutorialSection.transform.Find("TenthPart").gameObject.SetActive(true);
               //DoAllChecks(tutorialSection.transform.Find("TenthPart").gameObject);
            }
            else
            {
               GoToNext();
            }
            break;
         case 11:
            if (tutorialSection.transform.Find("EleventhPart") != null)
            {
               tutorialSection.transform.Find("TenthPart").gameObject.SetActive(false);
               tutorialSection.transform.Find("EleventhPart").gameObject.SetActive(true);
               //DoAllChecks(tutorialSection.transform.Find("EleventhPart").gameObject);
            }
            else
            {
               GoToNext();
            }
            break;
         case 12:
            if (tutorialSection.transform.Find("TwelvthPart") != null)
            {
               tutorialSection.transform.Find("EleventhPart").gameObject.SetActive(false);
               tutorialSection.transform.Find("TwelvthPart").gameObject.SetActive(true);
               //DoAllChecks(tutorialSection.transform.Find("TwelvthPart").gameObject);
            }
            else
            {
               GoToNext();
            }
            break;
         case 13:
            if (tutorialSection.transform.Find("ThirteenthPart") != null)
            {
               tutorialSection.transform.Find("TwelvthPart").gameObject.SetActive(false);
               tutorialSection.transform.Find("ThirteenthPart").gameObject.SetActive(true);
               //DoAllChecks(tutorialSection.transform.Find("ThirteenthPart").gameObject);
            }
            else
            {
               GoToNext();
            }
            break;
         case 14:
            if (tutorialSection.transform.Find("FourteenthPart") != null)
            {
               tutorialSection.transform.Find("ThirteenthPart").gameObject.SetActive(false);
               tutorialSection.transform.Find("FourteenthPart").gameObject.SetActive(true);
               //DoAllChecks(tutorialSection.transform.Find("FourteenthPart").gameObject);
            }
            else
            {
               GoToNext();
            }
            break;
      }
   }

   public IEnumerator ShowDialogue(Action onWalkthroughFinished)
   {
      isWalkthroughRunning = true;

      for (int stepIndex = 0; stepIndex < tutorialSteps.Length; stepIndex++)
      {
         currentStep = tutorialSteps[stepIndex];
         Debug.Log($"[NarrativeTutorialManager] Playing tutorial step {stepIndex}: {currentStep.note}");

         // Resolve the speaker portrait/profile from the NPC encounter database.
         NPCEncounterSystem.NPCProfile speakerProfile = null;
         if (NPCEncounterSystem.Instance != null)
            speakerProfile = NPCEncounterSystem.Instance.GetProfileByPersonality(currentStep.speaker);

         // Turn on any requested highlights before the dialogue starts.
         //ShowHighlights(currentStep.highlightTargets);
         //ShowUIHighlights(currentStep.uiHighlightTargets);

         bool dialogueFinished = false;

         // While tutorial dialogue is playing, gameplay input should remain blocked.
         //NarrativeOverlayUI.Instance.SetGameplayBlocked(true);
         PlaySequence(
            speakerProfile,
            currentStep.lines,
            currentStep.layoutMode,
            false,
            () => dialogueFinished = true);

         // Wait until the overlay reports that the dialogue has finished.
         yield return new WaitUntil(() => dialogueFinished);


         // Clean up highlights before moving to the next tutorial step.
         //HideHighlights(currentStep.highlightTargets);
         //HideUIHighlights(currentStep.uiHighlightTargets);
      }
      isWalkthroughRunning = false;
      Debug.Log("[NarrativeTutorialManager] Walkthrough finished.");
      onWalkthroughFinished?.Invoke();
      
   }

   // Method to transition to the next tutorial section
   public void GoToNext()
   {
      if (tutorialIndex < tutorialSections.Length - 1)
      {
         tutorialSections[tutorialIndex].SetActive(false);
         tutorialIndex++;
         tutorialSections[tutorialIndex].SetActive(true);
         sectionIndex = 1;
         GoThroughSection(tutorialSections[tutorialIndex], sectionIndex);
      }
      else
      {
         tutorialSections[tutorialIndex].SetActive(false);
      }
   }

   // Method to perform all necessary checks for the current tutorial part
   private void DoAllChecks(GameObject myPart)
   {
      if(myPart.transform.Find("Turn") != null)
      {
         nextButton.gameObject.SetActive(false);
         SetGameplayBlocked(false);
         ShowUIHighlights(currentStep.uiHighlightTargets);
         forgeCanvas.transform.Find("turnScreens").gameObject.SetActive(true);
         if(!turnButton.gameObject.activeSelf)
            turnButton.gameObject.SetActive(true);
         turnButton.GetComponent<Button>().onClick.AddListener(() => HandleCustomClick());
      }
      else if(myPart.transform.Find("ForgeExample") != null)
      {
         SetGameplayBlocked(false);
         HandleForgeTutorial?.Invoke();
         ShowHighlights(currentStep.highlightTargets);
         forgeFunction = true;
         turnButton.gameObject.SetActive(false);
         forgeCanvas.transform.Find("Screens").gameObject.SetActive(true);
         HideNextButton();
      }
      else if(myPart.transform.Find("TradeHutExample") != null)
      {
         SetGameplayBlocked(false);
         ShowHighlights(currentStep.highlightTargets);
         tradeHutFunction = true;
         turnButton.gameObject.SetActive(false);
         HandleTradeHutTutorial?.Invoke(1);
         tradeHutCanvas.transform.Find("Screens").gameObject.SetActive(true);
         HideNextButton();
      }
      else
      {
         isRequiredButtonClicked = true;
      }
   }   

   public void HideNextButton()
   {
      nextButton.gameObject.SetActive(false);
      SetGameplayBlocked(false);
   }
   public void HandleCustomClick()
   {
      //InventoryManager.Instance.TryAddOre(10);
      requiredButtonClicked = true;
      sectionIndex += 1;
      isRequiredButtonClicked = true;
      sectionIndex -= 1;
      GoThroughSection(tutorialSections[tutorialIndex], sectionIndex);

      Debug.Log("Turn button clicked, proceeding to next part.");
      CompleteCurrentSequence();
      currentLineIndex = 0;
      HandleNextClicked();
      turnButton.GetComponent<Button>().onClick.RemoveListener(HandleCustomClick);
      HideUIHighlights(currentStep.uiHighlightTargets);
      forgeCanvas.transform.Find("turnScreens").gameObject.SetActive(false);
   }

   public void HandleTurn()
   {
      Debug.Log("hello");
      requiredButtonClicked = true;
      currentActivePart = null;
      turnButton.GetComponent<Button>().onClick.RemoveListener(HandleTurn);
      GoThroughSection(tutorialSections[tutorialIndex], sectionIndex++);
   }

   // Method to handle global hover events and toggle arrows based on the current tutorial part
   public void HandleGlobalHover(string tag)
   {
      if(forgeFunction == true)
      {
         if(tag == "Forge")
         {
            forgeCanvas.transform.Find("Arrow3").gameObject.SetActive(true);
         }
         else
         {
            forgeCanvas.transform.Find("Arrow3").gameObject.SetActive(false);
         }
      }

      if(tradeHutFunction == true)
      {
         if(tag == "Trade Hut")
         {
            tradeHutCanvas.transform.Find("Arrow2").gameObject.SetActive(true);
         }
         else
         {
            tradeHutCanvas.transform.Find("Arrow2").gameObject.SetActive(false);
         }
      }
   }

   // Method to handle the next step in the tutorial when the required button is clicked
   public void HandleNextStep(int step)
   {
      if(forgeFunction == true && step == 2)
      {
         nextButton.gameObject.SetActive(true);
         forgeFunction = false;
         forgeCanvas.transform.Find("Arrow3").gameObject.SetActive(false);
         forgeCanvas.transform.Find("Screens").gameObject.SetActive(false);
      }

      requiredButtonClicked = false;

      if (tradeHutFunction == true && step == 2)
      {
         turnButton.gameObject.SetActive(true);
         tradeHutFunction = false;
         tradeHutCanvas.transform.Find("Arrow2").gameObject.SetActive(false);
         tradeHutCanvas.transform.Find("Screens").gameObject.SetActive(false);
         requiredButtonClicked = true;
      }

      CompleteCurrentSequence();
      currentLineIndex = 0;
      HandleNextClicked();
      ShowUIHighlights(currentStep.uiHighlightTargets);

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
      if (chubbyFont != null)
         nameText.font = chubbyFont;
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
      if (chubbyFont != null)
         dialogueText.font = chubbyFont;
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
      nextButton.onClick.AddListener(() => StartCoroutine(HandleNextClicked()));

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
      if (chubbyFont != null)
         toggleTitleText.font = chubbyFont;
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
      if (chubbyFont != null)
         toggleBodyText.font = chubbyFont;
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
      if (chubbyFont != null)
         textComponent.font = chubbyFont;
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
      currentCompleteAction = onComplete;
      isSequencePlaying = true;

      ApplyDialogueLayout(layoutMode);

      if(requiredButtonClicked == true)
      {
         skipButton.gameObject.SetActive(canSkip);
         nextButton.gameObject.SetActive(true);
         SetGameplayBlocked(true);
      }

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

   public IEnumerator HandleNextClicked()
   {
      if(isRequiredButtonClicked == true)
      {

         if (!isSequencePlaying)
            yield break;

         if (isTyping)
         {
            if (typingCoroutine != null)
            {
               StopCoroutine(typingCoroutine);
               typingCoroutine = null;
            }

            isTyping = false;
            dialogueText.text = currentFormattedLine;
            yield break;
         }

         currentLineIndex++;
         sectionIndex += 1;

         if (currentLines == null || currentLineIndex >= currentLines.Length)
         {
            CompleteCurrentSequence();
            currentLineIndex = 0;
            yield break;
         }

         ShowCurrentLine();
         GoThroughSection(tutorialSections[tutorialIndex], sectionIndex);
      }
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
      //SetGameplayBlocked(false);

      if (canvasObject != null)
      {
         Destroy(canvasObject);
         canvasObject = null;
      }

      if (Instance == this)
         Instance = null;
   }

   public void SetCustomObjective(GameObject objectivePrefab, Vector2 position)
   {
      // 1. Hide the standard next button
      nextButtonObject.SetActive(false);

      // 2. Instantiate or show your custom object
      isWaitingForCustomAction = true;

      // Example: Spawning the object on the UI or in-world
      currentCustomObjective = Instantiate(objectivePrefab, canvasObject.transform);
      currentCustomObjective.GetComponent<RectTransform>().anchoredPosition = position;

      // 3. You'll need a way for the custom object to tell this script it's done
      // You could use a delegate, a simple button listener, or a custom script
      if (currentCustomObjective.TryGetComponent<Button>(out var btn))
      {
         btn.onClick.AddListener(OnCustomObjectiveCompleted);
      }
   }

   private void OnCustomObjectiveCompleted()
   {
      isWaitingForCustomAction = false;
      if (currentCustomObjective != null) Destroy(currentCustomObjective);

      // Show the next button again or automatically advance
      nextButtonObject.SetActive(true);
      currentLineIndex++;
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