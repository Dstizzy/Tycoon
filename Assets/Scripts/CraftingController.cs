using UnityEngine;
using UnityEngine.UI;

public class CraftingController : MonoBehaviour
{
    [Header("Craft Buttons")]
    public Button craftTool1Button; // Crude Tool
    public Button craftTool2Button; // Refined Tool
    public Button craftTool3Button; // Artifact

    [Header("Craft Confirmation Panels")]
    public GameObject crudeConfirmation;
    public GameObject refinedConfirmation;
    public GameObject artifactConfirmation;

    [Header("Craft Panel Root")]
    public GameObject craftPanel;

    [Header("Craft Costs (in Ore)")]
    public int tool1OreCost = 15;   // Crude Ore Tool
    public int tool2OreCost = 25;   // Refined Tool
    public int tool3OreCost = 50;   // Artifact

    private void Start()
    {
        // Hide all confirmation popups at start
        crudeConfirmation.SetActive(false);
        refinedConfirmation.SetActive(false);
        artifactConfirmation.SetActive(false);

        // Hook up crafting buttons to show confirmation popups
        craftTool1Button.onClick.AddListener(() => ShowConfirmation(crudeConfirmation));
        craftTool2Button.onClick.AddListener(() => ShowConfirmation(refinedConfirmation));
        craftTool3Button.onClick.AddListener(() => ShowConfirmation(artifactConfirmation));
    }

    // Show the corresponding confirmation panel
    private void ShowConfirmation(GameObject confirmationPanel)
    {
        if (craftPanel != null)
            craftPanel.SetActive(false);

        crudeConfirmation.SetActive(false);
        refinedConfirmation.SetActive(false);
        artifactConfirmation.SetActive(false);
        confirmationPanel.SetActive(true);
    }

    // ---- CONFIRM CRAFTING ----
    public void ConfirmCraftCrude()
    {
        CraftItem(tool1OreCost, "Crude Tool");
        crudeConfirmation.SetActive(false);
        if (craftPanel != null)
            craftPanel.SetActive(true);
    }

    public void ConfirmCraftRefined()
    {
        CraftItem(tool2OreCost, "Refined Tool");
        refinedConfirmation.SetActive(false);
        if (craftPanel != null)
            craftPanel.SetActive(true);
    }

    public void ConfirmCraftArtifact()
    {
        CraftItem(tool3OreCost, "Artifact");
        artifactConfirmation.SetActive(false);
        if (craftPanel != null)
            craftPanel.SetActive(true);
    }

    // ---- CANCEL CRAFTING ----
    public void CancelCraftCrude()
    {
        crudeConfirmation.SetActive(false);
        if (craftPanel != null)
            craftPanel.SetActive(true);
    }

    public void CancelCraftRefined()
    {
        refinedConfirmation.SetActive(false);
        if (craftPanel != null)
            craftPanel.SetActive(true);
    }

    public void CancelCraftArtifact()
    {
        artifactConfirmation.SetActive(false);
        if (craftPanel != null)
            craftPanel.SetActive(true);
    }

    // ---- ACTUAL CRAFT LOGIC ----
    private void CraftItem(int oreCost, string toolName)
    {
        var inv = InventoryManager.Instance;

        if (inv.oreCount >= oreCost)
        {
            inv.oreCount -= oreCost;
            Debug.Log($"{toolName} crafted successfully! Used {oreCost} ore.");
            inv.OnOreCountChanged?.Invoke(inv.oreCount);
        }
        else
        {
            Debug.Log($"Not enough ore to craft {toolName}. Need {oreCost}, have {inv.oreCount}.");
        }
    }
}