using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles the NPC dialogue UI with multi-line dialogue and expression changes.
/// Can auto-generate UI if references are not assigned.
/// </summary>
public class NPCDialogueUI : MonoBehaviour
{
    #region Constants
    private const float DEFAULT_TEXT_SPEED = 0.03f;
    private const int MAX_CHOICE_BUTTONS = 5;
    
    // UI Layout Constants
    private const float PANEL_WIDTH = 800f;
    private const float PANEL_HEIGHT = 400f;
    private const float PORTRAIT_SIZE = 150f;
    private const float BUTTON_HEIGHT = 40f;
    private const float PADDING = 20f;
    #endregion

    #region Inspector References
    [Header("Panel")]
    [SerializeField] private GameObject dialoguePanel;

    [Header("NPC Display")]
    [SerializeField] private Image portraitImage;
    [SerializeField] private TextMeshProUGUI npcNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Choice Buttons")]
    [SerializeField] private Button[] choiceButtons = new Button[MAX_CHOICE_BUTTONS];
    [SerializeField] private TextMeshProUGUI[] choiceTexts = new TextMeshProUGUI[MAX_CHOICE_BUTTONS];

    [Header("Navigation")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button closeButton;

    [Header("Settings")]
    [SerializeField] private float textSpeed = DEFAULT_TEXT_SPEED;
    [SerializeField] private bool useTypewriterEffect = true;
    [SerializeField] private bool autoGenerateUI = true;

    [Header("UI Sprites (Optional)")]
    [SerializeField] private Sprite panelBackgroundSprite;
    [SerializeField] private Sprite buttonSprite;
    [SerializeField] private Sprite portraitFrameSprite;
    #endregion

    #region Private Variables
    private NPCEncounterSystem.NPCProfile currentNPC;
    private NPCEncounterSystem.EncounterScenario currentScenario;
    private NPCEncounterSystem.DialogueLine[] currentDialogueLines;
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private bool hasInteracted = false;
    private Coroutine typingCoroutine;
    private Canvas parentCanvas;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        // Find or create canvas
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            parentCanvas = FindObjectOfType<Canvas>();
        }

        // Auto-generate UI if not assigned
        if (autoGenerateUI && dialoguePanel == null)
        {
            GenerateUI();
        }

        // Initial state
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        // Setup button listeners
        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextClicked);
        
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseDialogue);
    }
    #endregion

    #region UI Generation
    /// <summary>
    /// Generates the complete dialogue UI at runtime
    /// </summary>
    private void GenerateUI()
    {
        if (parentCanvas == null)
        {
            Debug.LogError("[NPCDialogueUI] No Canvas found. Cannot generate UI.");
            return;
        }

        // === Main Dialogue Panel ===
        dialoguePanel = CreatePanel("NPCDialoguePanel", parentCanvas.transform);
        RectTransform panelRect = dialoguePanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(PANEL_WIDTH, PANEL_HEIGHT);
        panelRect.anchoredPosition = Vector2.zero;

        // Panel background
        Image panelBg = dialoguePanel.GetComponent<Image>();
        panelBg.color = new Color(0.1f, 0.15f, 0.2f, 0.95f);
        if (panelBackgroundSprite != null)
            panelBg.sprite = panelBackgroundSprite;

        // === Portrait Section (Left Side) ===
        GameObject portraitContainer = CreatePanel("PortraitContainer", dialoguePanel.transform);
        RectTransform portraitContainerRect = portraitContainer.GetComponent<RectTransform>();
        portraitContainerRect.anchorMin = new Vector2(0, 0.5f);
        portraitContainerRect.anchorMax = new Vector2(0, 0.5f);
        portraitContainerRect.pivot = new Vector2(0, 0.5f);
        portraitContainerRect.sizeDelta = new Vector2(PORTRAIT_SIZE + PADDING * 2, PORTRAIT_SIZE + PADDING * 2);
        portraitContainerRect.anchoredPosition = new Vector2(PADDING, 0);

        // Portrait frame background
        Image portraitContainerBg = portraitContainer.GetComponent<Image>();
        portraitContainerBg.color = new Color(0.2f, 0.25f, 0.3f, 1f);
        if (portraitFrameSprite != null)
            portraitContainerBg.sprite = portraitFrameSprite;

        // Portrait image
        GameObject portraitObj = new GameObject("PortraitImage");
        portraitObj.transform.SetParent(portraitContainer.transform);
        portraitImage = portraitObj.AddComponent<Image>();
        portraitImage.color = Color.white;
        RectTransform portraitRect = portraitObj.GetComponent<RectTransform>();
        portraitRect.anchorMin = new Vector2(0.5f, 0.5f);
        portraitRect.anchorMax = new Vector2(0.5f, 0.5f);
        portraitRect.sizeDelta = new Vector2(PORTRAIT_SIZE, PORTRAIT_SIZE);
        portraitRect.anchoredPosition = Vector2.zero;
        portraitRect.localScale = Vector3.one;

        // === Dialogue Section (Right Side) ===
        GameObject dialogueContainer = CreatePanel("DialogueContainer", dialoguePanel.transform);
        RectTransform dialogueContainerRect = dialogueContainer.GetComponent<RectTransform>();
        dialogueContainerRect.anchorMin = new Vector2(0, 0);
        dialogueContainerRect.anchorMax = new Vector2(1, 1);
        dialogueContainerRect.offsetMin = new Vector2(PORTRAIT_SIZE + PADDING * 3, PADDING);
        dialogueContainerRect.offsetMax = new Vector2(-PADDING, -PADDING);

        Image dialogueContainerBg = dialogueContainer.GetComponent<Image>();
        dialogueContainerBg.color = new Color(0.15f, 0.2f, 0.25f, 0.8f);

        // NPC Name
        GameObject nameObj = new GameObject("NPCNameText");
        nameObj.transform.SetParent(dialogueContainer.transform);
        npcNameText = nameObj.AddComponent<TextMeshProUGUI>();
        npcNameText.text = "NPC Name";
        npcNameText.fontSize = 24;
        npcNameText.fontStyle = FontStyles.Bold;
        npcNameText.color = new Color(0.9f, 0.8f, 0.5f, 1f);
        npcNameText.alignment = TextAlignmentOptions.TopLeft;
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 1);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.pivot = new Vector2(0, 1);
        nameRect.sizeDelta = new Vector2(0, 30);
        nameRect.anchoredPosition = new Vector2(PADDING, -PADDING);
        nameRect.localScale = Vector3.one;

        // Dialogue Text
        GameObject dialogueObj = new GameObject("DialogueText");
        dialogueObj.transform.SetParent(dialogueContainer.transform);
        dialogueText = dialogueObj.AddComponent<TextMeshProUGUI>();
        dialogueText.text = "Dialogue text goes here...";
        dialogueText.fontSize = 18;
        dialogueText.color = Color.white;
        dialogueText.alignment = TextAlignmentOptions.TopLeft;
        dialogueText.enableWordWrapping = true;
        RectTransform dialogueRect = dialogueObj.GetComponent<RectTransform>();
        dialogueRect.anchorMin = new Vector2(0, 0.4f);
        dialogueRect.anchorMax = new Vector2(1, 1);
        dialogueRect.offsetMin = new Vector2(PADDING, 0);
        dialogueRect.offsetMax = new Vector2(-PADDING, -50);
        dialogueRect.localScale = Vector3.one;

        // === Choice Buttons Container ===
        GameObject choiceContainer = new GameObject("ChoiceContainer");
        choiceContainer.transform.SetParent(dialogueContainer.transform);
        RectTransform choiceContainerRect = choiceContainer.AddComponent<RectTransform>();
        choiceContainerRect.anchorMin = new Vector2(0, 0);
        choiceContainerRect.anchorMax = new Vector2(1, 0.4f);
        choiceContainerRect.offsetMin = new Vector2(PADDING, PADDING);
        choiceContainerRect.offsetMax = new Vector2(-PADDING, 0);
        choiceContainerRect.localScale = Vector3.one;

        VerticalLayoutGroup vlg = choiceContainer.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 5;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // Create choice buttons
        choiceButtons = new Button[MAX_CHOICE_BUTTONS];
        choiceTexts = new TextMeshProUGUI[MAX_CHOICE_BUTTONS];

        for (int i = 0; i < MAX_CHOICE_BUTTONS; i++)
        {
            GameObject buttonObj = CreateButton($"ChoiceButton_{i}", choiceContainer.transform, $"Choice {i + 1}");
            choiceButtons[i] = buttonObj.GetComponent<Button>();
            choiceTexts[i] = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            
            LayoutElement le = buttonObj.AddComponent<LayoutElement>();
            le.preferredHeight = BUTTON_HEIGHT;
            le.flexibleWidth = 1;
        }

        // === Next Button ===
        GameObject nextObj = CreateButton("NextButton", dialoguePanel.transform, "▼");
        nextButton = nextObj.GetComponent<Button>();
        RectTransform nextRect = nextObj.GetComponent<RectTransform>();
        nextRect.anchorMin = new Vector2(1, 0);
        nextRect.anchorMax = new Vector2(1, 0);
        nextRect.pivot = new Vector2(1, 0);
        nextRect.sizeDelta = new Vector2(60, 40);
        nextRect.anchoredPosition = new Vector2(-PADDING, PADDING);

        // === Close Button ===
        GameObject closeObj = CreateButton("CloseButton", dialoguePanel.transform, "X");
        closeButton = closeObj.GetComponent<Button>();
        RectTransform closeRect = closeObj.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1, 1);
        closeRect.anchorMax = new Vector2(1, 1);
        closeRect.pivot = new Vector2(1, 1);
        closeRect.sizeDelta = new Vector2(40, 40);
        closeRect.anchoredPosition = new Vector2(-5, -5);

        // Setup button listeners
        nextButton.onClick.AddListener(OnNextClicked);
        closeButton.onClick.AddListener(CloseDialogue);

        Debug.Log("[NPCDialogueUI] UI generated successfully.");
    }

    /// <summary>
    /// Creates a panel with Image component
    /// </summary>
    private GameObject CreatePanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent);
        panel.AddComponent<RectTransform>();
        panel.AddComponent<Image>();
        panel.transform.localScale = Vector3.one;
        return panel;
    }

    /// <summary>
    /// Creates a button with TextMeshProUGUI
    /// </summary>
    private GameObject CreateButton(string name, Transform parent, string text)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent);
        
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.localScale = Vector3.one;

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.3f, 0.4f, 0.5f, 1f);
        if (buttonSprite != null)
            buttonImage.sprite = buttonSprite;

        Button button = buttonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.3f, 0.4f, 0.5f, 1f);
        colors.highlightedColor = new Color(0.4f, 0.5f, 0.6f, 1f);
        colors.pressedColor = new Color(0.2f, 0.3f, 0.4f, 1f);
        button.colors = colors;

        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
        tmpText.text = text;
        tmpText.fontSize = 16;
        tmpText.color = Color.white;
        tmpText.alignment = TextAlignmentOptions.Center;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        textRect.localScale = Vector3.one;

        return buttonObj;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Shows the dialogue panel with opening lines
    /// </summary>
    public void ShowDialogue(
        NPCEncounterSystem.NPCProfile npc, 
        NPCEncounterSystem.EncounterScenario scenario)
    {
        currentNPC = npc;
        currentScenario = scenario;
        currentDialogueLines = scenario.openingDialogues;
        currentLineIndex = 0;
        hasInteracted = false;

        // Setup UI
        npcNameText.text = npc.npcName;
        HideChoiceButtons();
        
        if (closeButton != null)
            closeButton.gameObject.SetActive(false);
        
        if (nextButton != null)
            nextButton.gameObject.SetActive(true);

        dialoguePanel.SetActive(true);
        PopUpManager.Instance?.DisablePlayerInput();

        // Start first line
        DisplayCurrentLine();
    }
    #endregion

    #region Dialogue Display
    /// <summary>
    /// Displays the current dialogue line with expression
    /// </summary>
    private void DisplayCurrentLine()
    {
        if (currentDialogueLines == null || currentLineIndex >= currentDialogueLines.Length)
        {
            OnDialogueSequenceComplete();
            return;
        }

        var line = currentDialogueLines[currentLineIndex];
        
        // Update portrait expression
        UpdatePortrait(line.expression);

        // Display text
        if (useTypewriterEffect)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeText(line.text));
        }
        else
        {
            dialogueText.text = line.text;
        }
    }

    /// <summary>
    /// Updates the portrait image based on expression
    /// </summary>
    private void UpdatePortrait(NPCEncounterSystem.ExpressionType expression)
    {
        if (currentNPC == null || NPCEncounterSystem.Instance == null) return;

        Sprite portrait = NPCEncounterSystem.Instance.GetPortraitForExpression(currentNPC, expression);
        if (portrait != null)
            portraitImage.sprite = portrait;
    }

    /// <summary>
    /// Typewriter effect coroutine
    /// </summary>
    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
    }

    /// <summary>
    /// Called when user clicks next/screen
    /// </summary>
    private void OnNextClicked()
    {
        // If still typing, complete immediately
        if (isTyping)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
            
            isTyping = false;
            
            if (currentDialogueLines != null && currentLineIndex < currentDialogueLines.Length)
                dialogueText.text = currentDialogueLines[currentLineIndex].text;
            
            return;
        }

        // Move to next line
        currentLineIndex++;
        DisplayCurrentLine();
    }

    /// <summary>
    /// Called when all opening lines are complete
    /// </summary>
    private void OnDialogueSequenceComplete()
    {
        if (nextButton != null)
            nextButton.gameObject.SetActive(false);

        // If this was result dialogue, show close button
        if (hasInteracted)
        {
            if (closeButton != null)
                closeButton.gameObject.SetActive(true);
        }
        else
        {
            // Show choice buttons
            ShowChoiceButtons();
        }
    }
    #endregion

    #region Choice Handling
    /// <summary>
    /// Shows the choice buttons for player selection
    /// </summary>
    private void ShowChoiceButtons()
    {
        if (currentScenario == null) return;

        var choices = currentScenario.choices;

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < choices.Length)
            {
                choiceButtons[i].gameObject.SetActive(true);
                choiceTexts[i].text = choices[i].choiceText;

                int index = i;
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(index));
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Hides all choice buttons
    /// </summary>
    private void HideChoiceButtons()
    {
        if (choiceButtons == null) return;
        
        foreach (var btn in choiceButtons)
        {
            if (btn != null)
                btn.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Handles player's choice selection
    /// </summary>
    private void OnChoiceSelected(int choiceIndex)
    {
        if (hasInteracted) return;
        hasInteracted = true;

        HideChoiceButtons();

        // Process choice and get result
        var outcome = NPCEncounterSystem.Instance.ProcessChoice(choiceIndex);

        if (outcome != null && outcome.resultDialogues != null && outcome.resultDialogues.Length > 0)
        {
            // Setup result dialogue sequence
            currentDialogueLines = outcome.resultDialogues;
            currentLineIndex = 0;

            if (nextButton != null)
                nextButton.gameObject.SetActive(true);

            // Display first result line
            DisplayCurrentLine();

            // Append reward info to last line
            StartCoroutine(AppendRewardInfo(outcome));
        }
        else
        {
            // No result dialogue, just close
            if (closeButton != null)
                closeButton.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Appends reward/penalty info after result dialogue
    /// </summary>
    private IEnumerator AppendRewardInfo(NPCEncounterSystem.EncounterOutcome outcome)
    {
        // Wait until dialogue is complete
        while (currentLineIndex < currentDialogueLines.Length - 1 || isTyping)
            yield return null;

        // Wait a moment then append reward text
        yield return new WaitForSeconds(0.3f);

        if (outcome.pearlChange != 0)
        {
            string color = outcome.isPositive ? "#00FF00" : "#FF0000";
            string sign = outcome.pearlChange > 0 ? "+" : "";
            dialogueText.text += $"\n\n<color={color}>{sign}{outcome.pearlChange} Pearl</color>";
        }
    }
    #endregion

    #region Close Dialogue
    /// <summary>
    /// Closes the dialogue panel
    /// </summary>
    private void CloseDialogue()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        dialoguePanel.SetActive(false);
        NPCEncounterSystem.Instance?.OnDialogueEnded();
        PopUpManager.Instance?.EnablePlayerInput();
    }
    #endregion
}