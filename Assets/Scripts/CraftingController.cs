/* Libraries and references                                                                      */
using UnityEngine;
using UnityEngine.UI;

public class CraftingController : MonoBehaviour
{
   /* Holds reference to the singleton instance of this class                                    */
   public static CraftingController Instance { get; private set; }

   [Header("Craft Buttons")] 
   public Button craftTool1Button; /* Button for Crude tool   */
   public Button craftTool2Button; /* Button for Refined tool */
   public Button craftTool3Button; /* Button for Artfact tool */

   [Header("Craft Confirmation Panels")]
   public GameObject crudeConfirmation;    /* Popup panel for confirming crude tool crating      */
   public GameObject refinedConfirmation;  /* Popup panel for confirming refined tool crating    */
   public GameObject artifactConfirmation; /* Popup panel for confirming artifact  tool crating  */

   [Header("Craft Panel Root")]
   public GameObject craftPanel; /* Main crafting UI panel */

   [Header("Craft Costs (in Ore)")]
   public int tool1OreCost = 15; /* Ore cost for crude tool    */
   public int tool2OreCost = 25; /* Ore cost for refined tool  */
   public int tool3OreCost = 50; /* Ore cost for artifact tool */

   [Header("Unlock Settings (in Pearls)")]
   public int refinedUnlockCost  = 100; /* Pearl cost for unlocking refined tool  */
   public int artifactUnlockCost = 200; /* Pearl cost for unlocking artifact tool */

   [Header("Lock Overlays")]
   public GameObject refinedLockOverlay;    /* Lock overlay for refined tool    */
   public GameObject artifactLockOverlay;   /* Lock overlay for artifact tool   */

   [Header("Unlock Panels (Popups)")]
   public GameObject refinedUnlockPanel;    /* Unlock panel for refined tool    */
   public GameObject artifactUnlockPanel;   /* Unlock panel for artifact tool   */
   public Text       refinedUnlockMessage;  /* Text for refined unlock message  */
   public Text       artifactUnlockMessage; /* Text for artifact unlock message */

   private bool refinedUnlocked;
   private bool artifactUnlocked;

   /* Sets up the singleton instance and initializes the crafting panel state                   */
   private void Awake()
   {
      Instance = this;

      refinedUnlocked  = PlayerPrefs.GetInt("Unlocked_Refined", 0)  == 1;
      artifactUnlocked = PlayerPrefs.GetInt("Unlocked_Artifact", 0) == 1;
   }


   private void Start()
   {
      /* Resets refined and artifact to be locked                                               */
      PlayerPrefs.SetInt("Unlocked_Refined", 0);
      PlayerPrefs.SetInt("Unlocked_Artifact", 0);
      PlayerPrefs.Save();

      refinedUnlocked = false;
      artifactUnlocked = false;

      /* Set all craft confirmation panels to inactive at start                                 */
      crudeConfirmation.   SetActive(false);
      refinedConfirmation. SetActive(false);
      artifactConfirmation.SetActive(false);

      craftTool1Button.onClick.AddListener(() => ShowConfirmation(crudeConfirmation));
      craftTool2Button.onClick.AddListener(() => ShowConfirmation(refinedConfirmation));
      craftTool3Button.onClick.AddListener(() => ShowConfirmation(artifactConfirmation));

      ApplyLockStateToUI();

      if (refinedUnlockPanel)
         refinedUnlockPanel.SetActive(false);
      if (artifactUnlockPanel)
         artifactUnlockPanel.SetActive(false);

      Debug.Log("[Crafting] Reset: both tools locked on play start");
   }

   /* Update tool button interactivity and lock overlay visibility                              */
   public void ApplyLockStateToUI()
   {
      if (refinedLockOverlay)
         refinedLockOverlay.SetActive(!refinedUnlocked);

      if (craftTool2Button)
         craftTool2Button.interactable = refinedUnlocked;

      if (artifactLockOverlay)
         artifactLockOverlay.SetActive(!artifactUnlocked);

      if (craftTool3Button)
         craftTool3Button.interactable = artifactUnlocked;

      Debug.Log($"[Crafting UI] Updated lock states: Refined={refinedUnlocked}, Artifact={artifactUnlocked}");
   }

   /* Open Unlock panel for refined tool when lock is clicked                                    */
   public void OpenUnlockRefined()
   {
      if (refinedUnlocked)
         return; 

      if (refinedUnlockMessage)
         refinedUnlockMessage.text = $"Unlock Refined Tool for {refinedUnlockCost} pearls?";

      if (refinedUnlockPanel)
         refinedUnlockPanel.SetActive(true);
   }
    
   /* Open Unlock panel for artifact tool when lock is clicked                                   */
   public void OpenUnlockArtifact()
    {
      if (artifactUnlocked)
         return;

      if (artifactUnlockMessage)
         artifactUnlockMessage.text = $"Unlock Artifact for {artifactUnlockCost} pearls?";

      if (artifactUnlockPanel)
         artifactUnlockPanel.SetActive(true);
   }

   /* Confirm to unlock refined tool using pearls                                                */
   public void ConfirmUnlockRefined()
   {
      if (TrySpendPearls(refinedUnlockCost))
      {
         refinedUnlocked = true;
         PlayerPrefs.SetInt("Unlocked_Refined", 1);
         PlayerPrefs.Save();
         ApplyLockStateToUI();
         Debug.Log("Refined Tool unlocked!");
      }
      else
      {
         Debug.Log($"Not enough pearls to unlock Refined Tool (need {refinedUnlockCost}).");
      }

      if (refinedUnlockPanel)
         refinedUnlockPanel.SetActive(false);
   }

   /* Confirm to unlock artiafct using pearls                                                    */
   public void ConfirmUnlockArtifact()
   {
      if (TrySpendPearls(artifactUnlockCost))
      {
         artifactUnlocked = true;
         PlayerPrefs.SetInt("Unlocked_Artifact", 1);
         PlayerPrefs.Save();
         ApplyLockStateToUI();
         Debug.Log("Artifact unlocked!");
      }
      else
      {
         Debug.Log($"Not enough pearls to unlock Artifact (need {artifactUnlockCost}).");
      }

      if (artifactUnlockPanel) artifactUnlockPanel.SetActive(false);
   }

   /* Close unlock pop up panel                                                                 */
   public void CancelUnlockRefined()
   {
      if (refinedUnlockPanel) refinedUnlockPanel.SetActive(false);
   }

   public void CancelUnlockArtifact()
   {
      if (artifactUnlockPanel) artifactUnlockPanel.SetActive(false);
   }

   /* Display craft confirmation when a tool is selected                                         */
   private void ShowConfirmation(GameObject confirmationPanel)
   {
      if (craftPanel != null)
         craftPanel.SetActive(false);

      crudeConfirmation.   SetActive(false);
      refinedConfirmation. SetActive(false);
      artifactConfirmation.SetActive(false);
      confirmationPanel.   SetActive(true);
   }

   /* Show crafting confirmation for crude tool                                                  */
   public void ConfirmCraftCrude()
   {
      CraftItem(tool1OreCost, "Crude Tool");
      crudeConfirmation.SetActive(false);

      if (craftPanel != null)
         craftPanel.SetActive(true);
   }

   /* Show crafting confirmation for refined tool                                                */
   public void ConfirmCraftRefined()
   {
      if (!refinedUnlocked)
      {
         Debug.Log("Refined Tool is locked!");
         return;
      }

      CraftItem(tool2OreCost, "Refined Tool");
      refinedConfirmation.SetActive(false);

      if (craftPanel != null)
         craftPanel.SetActive(true);
   }

   /* Show crafting confirmation for artifact tool                                               */
   public void ConfirmCraftArtifact()
   {
      if (!artifactUnlocked)
      {
         Debug.Log("Artifact is locked!");
         return;
      }

      CraftItem(tool3OreCost, "Artifact");
      artifactConfirmation.SetActive(false);

      if (craftPanel != null)
         craftPanel.SetActive(true);
   }

   /* Cancel crafting crude tool                                                                 */
   public void CancelCraftCrude()
   {
      crudeConfirmation.SetActive(false);
      if (craftPanel != null)
         craftPanel.SetActive(true);
   }

   /* Cancel crafting refined tool                                                               */
   public void CancelCraftRefined()
   {
      refinedConfirmation.SetActive(false);
      if (craftPanel != null)
         craftPanel.SetActive(true);
   }

   /* Cancel crafting artifact tool                                                              */
   public void CancelCraftArtifact()
   {
      artifactConfirmation.SetActive(false);
      if (craftPanel != null)
      craftPanel.SetActive(true);
   }

   /* Unlock refined tool from Lab tier 2 production path                                        */
   public void UnlockRefinedToolFromLab()
   {
      refinedUnlocked = true;
      PlayerPrefs.SetInt("Unlocked_Refined", 1);
      PlayerPrefs.Save();
      ApplyLockStateToUI();
      Debug.Log("Refined Tool unlocked by Lab Tier 2!");
   }

   /* Unlock artifact tool from Lab tier 3 production path                                       */
   public void UnlockArtifactToolFromLab()
   {
      artifactUnlocked = true;
      PlayerPrefs.SetInt("Unlocked_Artifact", 1);
      PlayerPrefs.Save();
      ApplyLockStateToUI();
      Debug.Log("Artifact Tool unlocked by Lab Tier 3!");
   }

   /* Craft the selected item and updated the corresponding count in inventory                   */
   private void CraftItem(int oreCost, string toolName)
   {
      var inv = InventoryManager.Instance;

        if (inv.oreCount >= oreCost)
        {
           //inv.oreCount -= oreCost;
           //Debug.Log($"{toolName} crafted successfully! Used {oreCost} ore.");
           //inv.OnOreCountChanged?.Invoke(inv.oreCount);

           // code added for OreRefinery_Manager.cs scripts by Juyoung
           inv.TrySpendOre(oreCost);
           Debug.Log($"{toolName} crafted successfully! Used {oreCost} ore.");
           TransactionMsgManager.Instance.ShowSuccess($"{toolName} crafted successfully! (-{oreCost} ore)");
         }
         else
         {
            Debug.Log($"Not enough ore to craft {toolName}. Need {oreCost}, have {inv.oreCount}.");
            TransactionMsgManager.Instance.ShowFailure($"Not enough ore to craft {toolName}. Need {oreCost}, have {inv.oreCount}.");
         }
   }

   /* Spend pearl if there is enoguh pearl in the inventory                                      */
   private bool TrySpendPearls(int pearls)
   {
      var inv = InventoryManager.Instance;

      if (inv == null)
      {
         Debug.LogError("InventoryManager instance not found!");
         return false;
      }

      if (inv.pearlCount < pearls)
      { 
         Debug.Log($"Not enough pearls to unlock. Need {pearls}, have {inv.pearlCount}.");
         return false;
      }

      inv.TrySpendPearl(pearls);
      return true;
   }
}