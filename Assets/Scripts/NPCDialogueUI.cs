using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Builds and manages the NPC dialogue panel entirely at runtime.
// Creates its own Screen-Space Overlay canvas (sorting order 999)
// so the dialogue always appears on top of every other element.
//
// Flow:
//   1. ShowDialogue() → displays opening lines one by one
//   2. Player clicks "Next ▼" to advance; clicking while typing finishes the line instantly
//   3. After all opening lines, choice buttons appear
//   4. Player picks a choice → result lines play out
//   5. "Close" button appears → closes the panel and re-enables input
//
// Lines marked with isAction = true are rendered in italics.
public class NPCDialogueUI : MonoBehaviour
{
    public static NPCDialogueUI Instance { get; private set; }

    // ── Layout constants ─────────────────────────────────────────────────
    private const float DEFAULT_TEXT_SPEED  = 0.03f;
    private const int   MAX_CHOICE_BUTTONS  = 5;

    private const float PANEL_WIDTH         = 1400f;
    private const float PANEL_HEIGHT        = 700f;
    private const float PORTRAIT_SIZE       = 350f;
    private const float BUTTON_HEIGHT       = 90f;
    private const float PADDING             = 40f;

    private const float NAME_FONT_SIZE      = 56f;
    private const float DIALOGUE_FONT_SIZE  = 42f;
    private const float BUTTON_FONT_SIZE    = 38f;
    private const float NAV_BUTTON_FONT_SIZE = 36f;

    // ── Inspector settings ───────────────────────────────────────────────
    [Header("Settings")]
    [SerializeField] private float textSpeed = DEFAULT_TEXT_SPEED;
    [SerializeField] private bool  useTypewriterEffect = true;

    [Header("UI Sprites (Optional)")]
    [SerializeField] private Sprite panelBackgroundSprite;
    [SerializeField] private Sprite buttonSprite;
    [SerializeField] private Sprite portraitFrameSprite;

    // ── Runtime UI references (created in GenerateUI) ────────────────────
    private GameObject       canvasObject;
    private GameObject       dialoguePanel;
    private Image            portraitImage;
    private TextMeshProUGUI  npcNameText;
    private TextMeshProUGUI  dialogueText;
    private Button[]         choiceButtons;
    private TextMeshProUGUI[] choiceTexts;
    private Button           nextButton;
    private Button           closeButton;
    private MonoBehaviour    coroutineRunner; // Lives on the canvas so coroutines work even if this GO is inactive

    // ── Dialogue state ───────────────────────────────────────────────────
    private NPCEncounterSystem.NPCProfile         currentNPC;
    private NPCEncounterSystem.EncounterScenario   currentScenario;
    private NPCEncounterSystem.DialogueLine[]      currentDialogueLines;
    private int      currentLineIndex = 0;
    private bool     isTyping         = false;
    private bool     hasInteracted    = false;
    private Coroutine typingCoroutine;
    private bool     isUIGenerated    = false;
    private bool     isDialogueActive = false;


    // ─────────────────────────────────────────────────────────────────────
    //  Unity lifecycle
    // ─────────────────────────────────────────────────────────────────────

    // Enforces Singleton and keeps the object active
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        gameObject.SetActive(true);
    }

    // Pre-builds the UI so it is ready when the first NPC is clicked
    private void Start()
    {
        EnsureUIGenerated();
    }


    // ─────────────────────────────────────────────────────────────────────
    //  UI generation
    //  Everything is created via code so no manual prefab setup is needed.
    // ─────────────────────────────────────────────────────────────────────

    // Called once before the first dialogue. Does nothing if already built.
    private void EnsureUIGenerated()
    {
        if (isUIGenerated) return;
        GenerateUI(); 
    }

    // Builds the full dialogue panel inside a new overlay canvas
    private void GenerateUI()
    {
        if (isUIGenerated) return;

        // ── Canvas ────────────────────────────────────────────────────────
        canvasObject = new GameObject("NPCDialogueCanvas");
        DontDestroyOnLoad(canvasObject);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight  = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
        coroutineRunner = canvasObject.AddComponent<CoroutineRunner>();

        // ── Main panel ────────────────────────────────────────────────────
        dialoguePanel = CreatePanel("NPCDialoguePanel", canvasObject.transform);
        RectTransform panelRect = dialoguePanel.GetComponent<RectTransform>();
        panelRect.anchorMin        = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax        = new Vector2(0.5f, 0.5f);
        panelRect.pivot            = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta        = new Vector2(PANEL_WIDTH, PANEL_HEIGHT);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.localScale       = Vector3.one;

        Image panelBg = dialoguePanel.GetComponent<Image>();
        panelBg.color = new Color(0.08f, 0.12f, 0.18f, 0.98f);
        if (panelBackgroundSprite != null) panelBg.sprite = panelBackgroundSprite;

        Outline outline = dialoguePanel.AddComponent<Outline>();
        outline.effectColor    = new Color(0.4f, 0.6f, 0.8f, 0.8f);
        outline.effectDistance = new Vector2(2, 2);

        // ── Portrait (left side) ──────────────────────────────────────────
        GameObject portraitContainer = CreatePanel("PortraitContainer", dialoguePanel.transform);
        RectTransform pcRect = portraitContainer.GetComponent<RectTransform>();
        pcRect.anchorMin = new Vector2(0, 0);
        pcRect.anchorMax = new Vector2(0, 1);
        pcRect.pivot     = new Vector2(0, 0.5f);
        pcRect.offsetMin = new Vector2(PADDING, PADDING);
        pcRect.offsetMax = new Vector2(PORTRAIT_SIZE + PADDING, -PADDING);
        pcRect.sizeDelta = new Vector2(PORTRAIT_SIZE + PADDING, 0);
        portraitContainer.GetComponent<Image>().color = new Color(0.15f, 0.2f, 0.28f, 1f);

        GameObject portraitObj = new GameObject("PortraitImage");
        portraitObj.transform.SetParent(portraitContainer.transform, false);
        portraitImage = portraitObj.AddComponent<Image>();
        portraitImage.preserveAspect = true;
        RectTransform prRect = portraitObj.GetComponent<RectTransform>();
        prRect.anchorMin        = new Vector2(0.5f, 0.5f);
        prRect.anchorMax        = new Vector2(0.5f, 0.5f);
        prRect.pivot            = new Vector2(0.5f, 0.5f);
        prRect.sizeDelta        = new Vector2(PORTRAIT_SIZE - 20, PORTRAIT_SIZE - 20);
        prRect.anchoredPosition = Vector2.zero;
        prRect.localScale       = Vector3.one;

        // ── Dialogue area (right side) ────────────────────────────────────
        GameObject dialogueContainer = CreatePanel("DialogueContainer", dialoguePanel.transform);
        RectTransform dcRect = dialogueContainer.GetComponent<RectTransform>();
        dcRect.anchorMin = Vector2.zero;
        dcRect.anchorMax = Vector2.one;
        dcRect.offsetMin = new Vector2(PORTRAIT_SIZE + PADDING * 2, PADDING);
        dcRect.offsetMax = new Vector2(-PADDING, -PADDING);
        dialogueContainer.GetComponent<Image>().color = new Color(0.12f, 0.16f, 0.22f, 0.9f);

        // NPC name label
        GameObject nameObj = new GameObject("NPCNameText");
        nameObj.transform.SetParent(dialogueContainer.transform, false);
        npcNameText           = nameObj.AddComponent<TextMeshProUGUI>();
        npcNameText.fontSize  = NAME_FONT_SIZE;
        npcNameText.fontStyle = FontStyles.Bold;
        npcNameText.color     = new Color(0.95f, 0.85f, 0.5f, 1f);
        npcNameText.alignment = TextAlignmentOptions.TopLeft;
        RectTransform nmRect = nameObj.GetComponent<RectTransform>();
        nmRect.anchorMin        = new Vector2(0, 1);
        nmRect.anchorMax        = new Vector2(1, 1);
        nmRect.pivot            = new Vector2(0, 1);
        nmRect.sizeDelta        = new Vector2(-PADDING * 2, 70);
        nmRect.anchoredPosition = new Vector2(PADDING, -PADDING);
        nmRect.localScale       = Vector3.one;

        // Dialogue text area
        GameObject dlgObj = new GameObject("DialogueText");
        dlgObj.transform.SetParent(dialogueContainer.transform, false);
        dialogueText                   = dlgObj.AddComponent<TextMeshProUGUI>();
        dialogueText.fontSize          = DIALOGUE_FONT_SIZE;
        dialogueText.color             = new Color(0.9f, 0.9f, 0.9f, 1f);
        dialogueText.alignment         = TextAlignmentOptions.TopLeft;
        dialogueText.enableWordWrapping = true;
        dialogueText.richText          = true;
        RectTransform dlRect = dlgObj.GetComponent<RectTransform>();
        dlRect.anchorMin  = new Vector2(0, 0.4f);
        dlRect.anchorMax  = new Vector2(1, 1);
        dlRect.offsetMin  = new Vector2(PADDING, 10);
        dlRect.offsetMax  = new Vector2(-PADDING, -PADDING - 70 - 10);
        dlRect.localScale = Vector3.one;

        // ── Choice buttons ────────────────────────────────────────────────
        GameObject choiceContainer = new GameObject("ChoiceContainer");
        choiceContainer.transform.SetParent(dialogueContainer.transform, false);
        RectTransform ccRect = choiceContainer.AddComponent<RectTransform>();
        ccRect.anchorMin  = new Vector2(0, 0);
        ccRect.anchorMax  = new Vector2(1, 0.4f);
        ccRect.offsetMin  = new Vector2(PADDING, 10);
        ccRect.offsetMax  = new Vector2(-PADDING - 210, -5);
        ccRect.localScale = Vector3.one;

        VerticalLayoutGroup vlg = choiceContainer.AddComponent<VerticalLayoutGroup>();
        vlg.spacing                = 8;
        vlg.childAlignment         = TextAnchor.UpperLeft;
        vlg.childControlWidth      = true;
        vlg.childControlHeight     = true;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.padding                = new RectOffset(0, 0, 5, 5);

        choiceButtons = new Button[MAX_CHOICE_BUTTONS];
        choiceTexts   = new TextMeshProUGUI[MAX_CHOICE_BUTTONS];
        for (int i = 0; i < MAX_CHOICE_BUTTONS; i++)
        {
            GameObject btn = CreateChoiceButton($"ChoiceButton_{i}", choiceContainer.transform, "");
            choiceButtons[i] = btn.GetComponent<Button>();
            choiceTexts[i]   = btn.GetComponentInChildren<TextMeshProUGUI>();
            btn.SetActive(false);
        }

        // ── Navigation buttons ────────────────────────────────────────────
        GameObject nextObj = CreateNavButton("NextButton", dialogueContainer.transform, "Next >");
        nextButton = nextObj.GetComponent<Button>();
        nextButton.onClick.AddListener(OnNextClicked);
        RectTransform nxRect = nextObj.GetComponent<RectTransform>();
        nxRect.anchorMin        = new Vector2(1, 0);
        nxRect.anchorMax        = new Vector2(1, 0);
        nxRect.pivot            = new Vector2(1, 0);
        nxRect.sizeDelta        = new Vector2(200, 80);
        nxRect.anchoredPosition = new Vector2(-10, 10);
        nextObj.SetActive(false);

        GameObject closeObj = CreateNavButton("CloseButton", dialogueContainer.transform, "Close");
        closeButton = closeObj.GetComponent<Button>();
        closeButton.onClick.AddListener(CloseDialogue);
        RectTransform clRect = closeObj.GetComponent<RectTransform>();
        clRect.anchorMin        = new Vector2(1, 0);
        clRect.anchorMax        = new Vector2(1, 0);
        clRect.pivot            = new Vector2(1, 0);
        clRect.sizeDelta        = new Vector2(200, 80);
        clRect.anchoredPosition = new Vector2(-10, 10);
        closeObj.SetActive(false);

        dialoguePanel.SetActive(false);
        isUIGenerated = true;
        Debug.Log("[NPCDialogueUI] UI generated successfully!");
    }

    // ── Helper: creates a GameObject with RectTransform + Image ──────────
    private GameObject CreatePanel(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>().localScale = Vector3.one;
        go.AddComponent<Image>();
        return go;
    }

    // ── Helper: creates a choice button with left-aligned text ───────────
    private GameObject CreateChoiceButton(string name, Transform parent, string text)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>().localScale = Vector3.one;

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.3f, 0.4f, 0.95f);

        Button btn = go.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor      = new Color(0.2f, 0.3f, 0.4f, 0.95f);
        cb.highlightedColor = new Color(0.3f, 0.45f, 0.6f, 1f);
        cb.pressedColor     = new Color(0.15f, 0.22f, 0.3f, 1f);
        cb.selectedColor    = new Color(0.25f, 0.35f, 0.5f, 1f);
        btn.colors = cb;

        LayoutElement le = go.AddComponent<LayoutElement>();
        le.minHeight       = BUTTON_HEIGHT;
        le.preferredHeight = BUTTON_HEIGHT;

        GameObject tObj = new GameObject("Text");
        tObj.transform.SetParent(go.transform, false);
        TextMeshProUGUI tmp = tObj.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = BUTTON_FONT_SIZE;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Left;
        RectTransform tRect = tObj.GetComponent<RectTransform>();
        tRect.anchorMin  = Vector2.zero;
        tRect.anchorMax  = Vector2.one;
        tRect.offsetMin  = new Vector2(20, 8);
        tRect.offsetMax  = new Vector2(-20, -8);
        tRect.localScale = Vector3.one;

        return go;
    }

    // ── Helper: creates a navigation button (Next / Close) ───────────────
    private GameObject CreateNavButton(string name, Transform parent, string text)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>().localScale = Vector3.one;

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.25f, 0.4f, 0.55f, 1f);

        Button btn = go.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor      = new Color(0.25f, 0.4f, 0.55f, 1f);
        cb.highlightedColor = new Color(0.35f, 0.55f, 0.7f, 1f);
        cb.pressedColor     = new Color(0.18f, 0.3f, 0.42f, 1f);
        cb.selectedColor    = new Color(0.28f, 0.45f, 0.6f, 1f);
        btn.colors = cb;

        GameObject tObj = new GameObject("Text");
        tObj.transform.SetParent(go.transform, false);
        TextMeshProUGUI tmp = tObj.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = NAV_BUTTON_FONT_SIZE;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        RectTransform tRect = tObj.GetComponent<RectTransform>();
        tRect.anchorMin  = Vector2.zero;
        tRect.anchorMax  = Vector2.one;
        tRect.offsetMin  = new Vector2(5, 2);
        tRect.offsetMax  = new Vector2(-5, -2);
        tRect.localScale = Vector3.one;

        return go;
    }


    // ─────────────────────────────────────────────────────────────────────
    //  Public API
    // ─────────────────────────────────────────────────────────────────────

    // Opens the dialogue panel and begins playing the scenario's opening lines
    public void ShowDialogue(
        NPCEncounterSystem.NPCProfile npc,
        NPCEncounterSystem.EncounterScenario scenario)
    {
        // Prevent re-entry if dialogue is already active
        if (isDialogueActive) return;

        EnsureUIGenerated();
        if (!isUIGenerated || npc == null || scenario == null) return;
        if (scenario.openingDialogues == null || scenario.openingDialogues.Length == 0) return;

        currentNPC           = npc;
        currentScenario      = scenario;
        currentDialogueLines = scenario.openingDialogues;
        currentLineIndex     = 0;
        hasInteracted        = false;
        isDialogueActive     = true;

        npcNameText.text = npc.npcName ?? "Unknown";
        HideAllButtons();
        nextButton.gameObject.SetActive(true);
        dialoguePanel.SetActive(true);

        PopUpManager.Instance?.DisablePlayerInput();
        DisplayCurrentLine();
    }

    // True while the dialogue panel is open
    public bool IsDialogueActive() => isDialogueActive;

    // Immediately closes the dialogue (used when the turn ends, etc.)
    public void ForceClose()
    {
        if (isDialogueActive) CloseDialogue();
    }


    // ─────────────────────────────────────────────────────────────────────
    //  Dialogue display
    // ─────────────────────────────────────────────────────────────────────

    // Shows the current line (or ends the sequence if there are no more)
    private void DisplayCurrentLine()
    {
        // Guard: no lines to display
        if (currentDialogueLines == null || currentDialogueLines.Length == 0)
        {
            OnDialogueSequenceComplete();
            return;
        }

        // Guard: past the end of the array
        if (currentLineIndex >= currentDialogueLines.Length)
        {
            OnDialogueSequenceComplete();
            return;
        }

        var line = currentDialogueLines[currentLineIndex];

        // Skip null lines with a safety limit to prevent infinite recursion
        if (line == null)
        {
            currentLineIndex++;
            // Safety: if we've skipped past the end, stop
            if (currentLineIndex >= currentDialogueLines.Length)
            {
                OnDialogueSequenceComplete();
                return;
            }
            DisplayCurrentLine();
            return;
        }

        UpdatePortrait(line.expression);

        string text = line.text ?? "";

        if (useTypewriterEffect && coroutineRunner != null)
        {
            if (typingCoroutine != null) coroutineRunner.StopCoroutine(typingCoroutine);
            typingCoroutine = coroutineRunner.StartCoroutine(TypeText(text, line.isAction));
        }
        else
        {
            dialogueText.text = line.isAction ? $"<i>{text}</i>" : text;
        }
    }

    // Swaps the portrait sprite to match the current expression
    private void UpdatePortrait(NPCEncounterSystem.ExpressionType expression)
    {
        if (currentNPC == null || NPCEncounterSystem.Instance == null || portraitImage == null) return;

        Sprite sprite = NPCEncounterSystem.Instance.GetPortraitForExpression(currentNPC, expression);
        if (sprite != null) { portraitImage.sprite = sprite; portraitImage.color = Color.white; }
        else                { portraitImage.sprite = null;   portraitImage.color = new Color(0.4f, 0.4f, 0.5f, 1f); }
    }

    // Typewriter effect. Action lines are wrapped in <i> tags for italics.
    private IEnumerator TypeText(string text, bool isAction = false)
    {
        isTyping = true;
        dialogueText.text = "";

        if (isAction)
        {
            for (int i = 0; i < text.Length; i++)
            {
                dialogueText.text = $"<i>{text.Substring(0, i + 1)}</i>";
                yield return new WaitForSeconds(textSpeed);
            }
        }
        else
        {
            foreach (char c in text)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(textSpeed);
            }
        }

        isTyping = false;
    }

    // "Next" button handler. If typing is in progress, completes the line instantly.
    private void OnNextClicked()
    {
        if (isTyping)
        {
            if (typingCoroutine != null && coroutineRunner != null)
                coroutineRunner.StopCoroutine(typingCoroutine);
            isTyping = false;

            if (currentDialogueLines != null && currentLineIndex < currentDialogueLines.Length)
            {
                var line = currentDialogueLines[currentLineIndex];
                string t = line?.text ?? "";
                dialogueText.text = (line != null && line.isAction) ? $"<i>{t}</i>" : t;
            }
            return;
        }

        currentLineIndex++;
        DisplayCurrentLine();
    }

    // Called when the last line of a sequence has been displayed
    private void OnDialogueSequenceComplete()
    {
        nextButton.gameObject.SetActive(false);

        if (hasInteracted)
            closeButton.gameObject.SetActive(true);   // Result done → let player close
        else
            ShowChoiceButtons();                       // Opening done → show choices
    }


    // ─────────────────────────────────────────────────────────────────────
    //  Choice handling
    // ─────────────────────────────────────────────────────────────────────

    // Activates the choice buttons that match the current scenario
    private void ShowChoiceButtons()
    {
        if (currentScenario?.choices == null || currentScenario.choices.Length == 0)
        { ShowCloseButtonAsFallback(); return; }

        var choices = currentScenario.choices;
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (choiceButtons[i] == null) continue;

            if (i < choices.Length && choices[i] != null)
            {
                choiceButtons[i].gameObject.SetActive(true);
                if (choiceTexts[i] != null)
                    choiceTexts[i].text = choices[i].choiceText ?? $"Choice {i + 1}";

                int index = i;
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(index));
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }

        closeButton.gameObject.SetActive(true); // Allow closing without choosing
    }

    // Hides every interactive button
    private void HideAllButtons()
    {
        if (choiceButtons != null)
            foreach (var b in choiceButtons) if (b != null) b.gameObject.SetActive(false);
        if (nextButton  != null) nextButton.gameObject.SetActive(false);
        if (closeButton != null) closeButton.gameObject.SetActive(false);
    }

    // Fallback: if there are no valid choices, just show the close button
    private void ShowCloseButtonAsFallback()
    {
        if (choiceButtons != null)
            foreach (var b in choiceButtons) if (b != null) b.gameObject.SetActive(false);
        if (closeButton != null) closeButton.gameObject.SetActive(true);
    }

   // Processes the selected choice, then plays the result dialogue
   private void OnChoiceSelected(int choiceIndex)
   {
      if (hasInteracted) return;

      if (NPCEncounterSystem.Instance == null) { ShowCloseButtonAsFallback(); return; }

      var outcome = NPCEncounterSystem.Instance.ProcessChoice(choiceIndex);

      // null means payment failed — re-show choices so the player can pick again
      if (outcome == null)
      {
         Debug.Log("[NPCDialogueUI] Choice failed (insufficient resources). Re-showing choices.");
         return;
      }

      // Lock in the interaction so player can't pick again
      hasInteracted = true;
      HideAllButtons();

      if (outcome.resultDialogues != null && outcome.resultDialogues.Length > 0)
      {
         currentDialogueLines = outcome.resultDialogues;
         currentLineIndex = 0;
         nextButton.gameObject.SetActive(true);
         DisplayCurrentLine();
         if (coroutineRunner != null)
            coroutineRunner.StartCoroutine(AppendRewardInfo(outcome));
      }
      else
      {
         closeButton.gameObject.SetActive(true);
      }
   }

   // Waits until the last result line finishes, then applies the reward
   // and shows a colored summary line (+50 Pearl / -30 Pearl).
   private IEnumerator AppendRewardInfo(NPCEncounterSystem.EncounterOutcome outcome)
   {
      if (outcome == null) yield break;

      // Wait until we reach the last dialogue line
      while (currentDialogueLines != null && currentLineIndex < currentDialogueLines.Length - 1)
         yield return null;

      // Wait for typing to finish
      while (isTyping)
         yield return null;

      // Small pause for dramatic effect
      yield return new WaitForSeconds(0.5f);

      // ── Apply the reward/penalty NOW ─────────────────────────────────
      NPCEncounterSystem.Instance?.ApplyOutcomeReward(outcome);

      // ── Show result text ─────────────────────────────────────────────
      if (outcome.pearlChange != 0 && dialogueText != null)
      {
         string hex = outcome.pearlChange > 0 ? "#00FF00" : "#FF6666";
         string sign = outcome.pearlChange > 0 ? "+" : "";
         string label = outcome.pearlChange > 0 ? "Gained" : "Lost";
         int display = Mathf.Abs(outcome.pearlChange);
         dialogueText.text += $"\n\n<color={hex}><b>{label} {display} Pearl ({sign}{outcome.pearlChange})</b></color>";
      }
   }

   // ─────────────────────────────────────────────────────────────────────
   //  Close / cleanup
   // ─────────────────────────────────────────────────────────────────────

   // Resets all state and hides the panel
   private void CloseDialogue()
    {
        if (typingCoroutine != null && coroutineRunner != null)
        { coroutineRunner.StopCoroutine(typingCoroutine); typingCoroutine = null; }

        isTyping         = false;
        isDialogueActive = false;
        hasInteracted    = false;
        currentNPC       = null;
        currentScenario  = null;
        currentDialogueLines = null;
        currentLineIndex = 0;

        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        NPCEncounterSystem.Instance?.OnDialogueEnded();
        PopUpManager.Instance?.EnablePlayerInput();
    }

    // Destroys the canvas we created when this component is destroyed
    private void OnDestroy()
    {
        if (canvasObject != null) Destroy(canvasObject);
    }
}

// Empty MonoBehaviour attached to the generated canvas so we can
// run coroutines on an object that is always active.
public class CoroutineRunner : MonoBehaviour { }