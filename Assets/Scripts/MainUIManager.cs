using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class MainUIManager : MonoBehaviour
{
   public Button   MainMenuButton;
   public Button[] DropdownButtons;
   public Button   InventoryButton;
   public Button   victoryButton;

   [SerializeField] private TextMeshProUGUI pearCountText;
   [SerializeField] private TextMeshProUGUI oreCountText;
   [SerializeField] private Transform       erroPanel;
   [SerializeField] private GameObject      victoryPanel;

   private bool isVisible = false;

   private void Awake()
   {

      if (InventoryManager.Instance == null)
      {
         // This is primarily for safety, though the script execution order should help.
         // You might need to check your Unity Project Settings -> Script Execution Order 
         // to ensure InventoryManager runs before MainUIManager.
         Debug.LogError("InventoryManager instance is not yet available in MainUIManager Awake. Delaying subscriptions.");
         // If it's not ready, we cannot subscribe yet and must defer to Start().
         return;
      }

      // Initially hide dropdown buttons
      foreach (Button btn in DropdownButtons)
      {
         btn.gameObject.SetActive(false);
      }
      // Add listener to main menu button
      MainMenuButton.onClick.AddListener(ToggleMenu);
      InventoryButton.onClick.AddListener(() => {
         InventoryManager.Instance.ShowInventoryPanel();
      });
      victoryButton.onClick.AddListener(() => {
         ShowVictoryPanel();
      });

      ChangePearlCountText(InventoryManager.Instance.pearlCount);
      ChangeOreCountText(InventoryManager.Instance.oreCount);
      InventoryManager.Instance.OnOreCountChanged += ChangeOreCountText;
      InventoryManager.Instance.OnPearlCountChanged += ChangePearlCountText;
   }
   public void ToggleMenu()
   {
      isVisible = !isVisible;
      DropdownButtons[0].gameObject.SetActive(isVisible);
      DropdownButtons[1].gameObject.SetActive(isVisible);
   }

   public void ChangePearlCountText(int newPearlCount)
   {
      if (pearCountText != null)
         pearCountText.text = newPearlCount.ToString();
   }

   public void ChangeOreCountText(int newOreCount)
   {
      if (oreCountText != null)
         oreCountText.text = newOreCount.ToString();
   }

   public void ShowVictoryPanel()
   {
      if (victoryPanel != null)
      {
         victoryPanel.SetActive(true);
         victoryPanel.transform.Find("ExitButton").GetComponent<Button>().onClick.AddListener(() => {
            victoryPanel.SetActive(false);
            PopUpManager.Instance.EnablePlayerInput();
         });

         PopUpManager.Instance.DisablePlayerInput();

         // First Part
         victoryPanel.transform.Find("SubmarineSkel/HeadButton").GetComponent<Button>().onClick.AddListener(() => ActivateHead());

         // Second Part
         victoryPanel.transform.Find("SubmarineSkel/BodyButton").GetComponent<Button>().onClick.AddListener(() => ActivateBody());

         // Third Part
         victoryPanel.transform.Find("SubmarineSkel/TailButton").GetComponent<Button>().onClick.AddListener(() => ActivateTail());
      }
   }

   public void ActivateHead()
   {
      if (victoryPanel.transform.Find("QuestionMark").gameObject.activeSelf)
      {
         victoryPanel.transform.Find("QuestionMark").gameObject.SetActive(false);
      }

      if (victoryPanel.transform.Find("SubmarineSkel/SubmarineBody").gameObject.activeSelf && victoryPanel.transform.Find("SubmarineSkel/SubmarineTail").gameObject.activeSelf)
      {
         ActivateFinalForm();
      }
      else
      {
         if (victoryPanel.transform.Find("SubmarineSkel/SubmarineHead").gameObject.activeSelf)
            victoryPanel.transform.Find("SubmarineSkel/SubmarineHead").gameObject.SetActive(false);
         else
            victoryPanel.transform.Find("SubmarineSkel/SubmarineHead").gameObject.SetActive(true);
      }
   }

   public void ActivateBody() 
   {
      if (victoryPanel.transform.Find("QuestionMark").gameObject.activeSelf)
      {
         victoryPanel.transform.Find("QuestionMark").gameObject.SetActive(false);
      }

      if (victoryPanel.transform.Find("SubmarineSkel/SubmarineHead").gameObject.activeSelf && victoryPanel.transform.Find("SubmarineSkel/SubmarineTail").gameObject.activeSelf)
      {
         ActivateFinalForm();
      }
      else
      {
         if (victoryPanel.transform.Find("SubmarineSkel/SubmarineBody").gameObject.activeSelf)
            victoryPanel.transform.Find("SubmarineSkel/SubmarineBody").gameObject.SetActive(false);
         else
            victoryPanel.transform.Find("SubmarineSkel/SubmarineBody").gameObject.SetActive(true);
      }
   }

   public void ActivateTail() 
   {
      if(victoryPanel.transform.Find("QuestionMark").gameObject.activeSelf)
      {
         victoryPanel.transform.Find("QuestionMark").gameObject.SetActive(false);
      }

      if (victoryPanel.transform.Find("SubmarineSkel/SubmarineBody").gameObject.activeSelf && victoryPanel.transform.Find("SubmarineSkel/SubmarineHead").gameObject.activeSelf)
      {
         ActivateFinalForm();
      }
      else
      {
         if (victoryPanel.transform.Find("SubmarineSkel/SubmarineTail").gameObject.activeSelf)
            victoryPanel.transform.Find("SubmarineSkel/SubmarineTail").gameObject.SetActive(false);
         else
            victoryPanel.transform.Find("SubmarineSkel/SubmarineTail").gameObject.SetActive(true);
      }
   }

   public void ActivateFinalForm() 
   {
      victoryPanel.transform.Find("SubmarineFull").gameObject.SetActive(true);
      victoryPanel.transform.Find("SubmarineBlackedOut").gameObject.SetActive(false);
      victoryPanel.transform.Find("SubmarineSkel").gameObject.SetActive(false);
   }
}