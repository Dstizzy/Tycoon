using UnityEngine;
using UnityEngine.UI;

public class CraftingController : MonoBehaviour
{
    [Header("Craft Buttons")]
    public Button craftTool1Button;
    public Button craftTool2Button;
    public Button craftTool3Button;

    [Header("Craft Costs (in Ore)")]
    public int tool1OreCost = 15;   // Crude Ore Tool
    public int tool2OreCost = 25;   // Reinforced Component
    public int tool3OreCost = 50;   // Artifact

    private void Start()
    {
        craftTool1Button.onClick.AddListener(() => CraftItem(tool1OreCost, "Crude Ore Tool"));
        craftTool2Button.onClick.AddListener(() => CraftItem(tool2OreCost, "Reinforced Component"));
        craftTool3Button.onClick.AddListener(() => CraftItem(tool3OreCost, "Artifact"));
    }

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
            Debug.Log($"Not enough ore to craft {toolName}. Need {oreCost}, have {inv.oreCount}.");
    }
}
