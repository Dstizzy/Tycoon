using TMPro;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MainUIManager : MonoBehaviour
{

   // Buttons on the main UI
   public Button MainMenuButton;
   public Button[] DropdownButtons;
   public Button InventoryButton;
   public Button victoryButton;
   public Button NextButton;

   // UI elements on main UI
   [SerializeField] private TextMeshProUGUI pearCountText;
   [SerializeField] private TextMeshProUGUI oreCountText;
   [SerializeField] private Transform erroPanel;
   [SerializeField] private GameObject victoryPanel;
   [SerializeField] private GameObject tutorial;

   // State variable to track dropdown visibility
   private bool isVisible = false;
   private int displayedPearlCount = 0;
   private int displayedOreCount = 0;

   public static MainUIManager mainUI;

   // Singleton instance
   private void Awake()
   {
      if (mainUI != null && mainUI != this)
         Destroy(this.gameObject);
      else
      {
         mainUI = this;
         DontDestroyOnLoad(this.gameObject);
      }

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
      MainMenuButton.onClick.AddListener(() => {
         AudioManager.Instance.PlayClick(); 
         ToggleMenu();
      });
      InventoryButton.onClick.AddListener(() => {
         //AudioManager.Instance.PlayClick(); 
         InventoryManager.Instance.ShowInventoryPanel();
      });
      victoryButton.onClick.AddListener(() => {
         AudioManager.Instance.PlayClick(); 
         ShowVictoryPanel();
      });

      if (NextButton != null)
      {
         NextButton.onClick.AddListener(() =>
         {
            if (AudioManager.Instance != null)
               AudioManager.Instance.PlayClick();
         });
      }

      if (victoryButton != null)
      {
         victoryPanel.transform.Find("ExitButton").GetComponent<Button>().onClick.AddListener(() =>
         {
            AudioManager.Instance.PlayClick();
            victoryPanel.SetActive(false);
            PopUpManager.Instance.EnablePlayerInput();
         });
      }

      ChangePearlCountText(InventoryManager.Instance.pearlCount);
      ChangeOreCountText(InventoryManager.Instance.oreCount);
      InventoryManager.Instance.OnOreCountChanged += ChangeOreCountText;
      InventoryManager.Instance.OnPearlCountChanged += ChangePearlCountText;
   }

   // Method to toggle the visibility of the dropdown buttons
   public void ToggleMenu()
   {
      isVisible = !isVisible;
      DropdownButtons[0].gameObject.SetActive(isVisible);
      DropdownButtons[1].gameObject.SetActive(isVisible);
      DropdownButtons[2].gameObject.SetActive(isVisible);
      //DropdownButtons[2].onClick.AddListener(() => ToggleTutorial());
   }

   public void ToggleTutorial()
   {
      if(tutorial.gameObject.activeSelf)
         tutorial.gameObject.SetActive(false);
      else 
         tutorial.gameObject.SetActive(true);
      //ChangeTutorial?.Invoke();
   }

   // Changes the Pearl count text on the main UI
   public void ChangePearlCountText(int newPearlCount)
   {
      if (pearCountText != null)
      {
         StartCoroutine(AnimateTopBarCounter(pearCountText, displayedPearlCount, newPearlCount, 0.5f, true));
      }
   }

   // Changes the Ore count text on the main UI
   public void ChangeOreCountText(int newOreCount)
   {
      if (oreCountText != null)
      {
         StartCoroutine(AnimateTopBarCounter(oreCountText, displayedOreCount, newOreCount, 0.5f, false));
      }
   }

   //Shows the victory panel and sets up the buttons for the submarine assembly
   public void ShowVictoryPanel()
   {
      bool isHeadReady,
           isBodyReady;

      if (victoryPanel != null)
      {
         victoryPanel.SetActive(true);
         PopUpManager.Instance.DisablePlayerInput();

         isHeadReady = LabManager.headUnlocked && InventoryManager.Instance.pearlCount >= 10000;
         isBodyReady = LabManager.bodyUnlocked && InventoryManager.Instance.engineCount >= 5
                                               && InventoryManager.Instance.pressureValveCount >= 5
                                               && InventoryManager.Instance.precisionLensCount >= 5;

         Transform skeleton = victoryPanel.transform.Find("SubmarineSkel");
         if (skeleton != null)
         {
            skeleton.Find("SubmarineHead").gameObject.SetActive(isHeadReady);
            skeleton.Find("SubmarineBody").gameObject.SetActive(isBodyReady);
            skeleton.Find("SubmarineTail").gameObject.SetActive(LabManager.tailUnlocked);
         }

         Transform qestionMark = victoryPanel.transform.Find("QuestionMark");
         if (qestionMark != null)
         {
            if (isHeadReady || isBodyReady || LabManager.tailUnlocked)
               qestionMark.gameObject.SetActive(false);
            else
               qestionMark.gameObject.SetActive(true);
         }

         if (isHeadReady && isBodyReady && LabManager.tailUnlocked)
         {
            ActivateFinalForm();
         }
      }
   }

   // Activates the head of the submarine
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

   // Activates the body of the submarine
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

   // Activates the tail of the submarine
   public void ActivateTail()
   {
      if (victoryPanel.transform.Find("QuestionMark").gameObject.activeSelf)
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

   // Activates the final form of the submarine when all parts are active
   public void ActivateFinalForm()
   {
      victoryPanel.transform.Find("SubmarineFull").gameObject.SetActive(true);
      victoryPanel.transform.Find("SubmarineBlackedOut").gameObject.SetActive(false);
      victoryPanel.transform.Find("SubmarineSkel").gameObject.SetActive(false);
   }

   public void GoToStartScreen()
   {
      if (SceneHistory.Instance != null) 
      {
         GameManager.RestartGame();
         SceneHistory.Instance.LoadPreviousScene();
      }
      else
         Debug.LogError("SceneHistory is missing from the scene!");
   }

   public void SetMainButtonsInteractable(bool interactable)
   {
      if (MainMenuButton != null)
         MainMenuButton.interactable = interactable;
      if (InventoryButton != null)
         InventoryButton.interactable = interactable;
      //if (victoryButton != null)
         //victoryButton.interactable = interactable;
      if (NextButton != null)
         NextButton.interactable = interactable;
      if (DropdownButtons != null)
         foreach (var btn in DropdownButtons)
            if (btn != null)
               btn.interactable = interactable;
   }

   private System.Collections.IEnumerator AnimateTopBarCounter(TextMeshProUGUI textElement, int startValue, int endValue, float duration, bool isPearl)
   {
      float elapsedTime = 0f;

      while (elapsedTime < duration)
      {
         elapsedTime += Time.deltaTime;
         float currentValue = Mathf.Lerp(startValue, endValue, elapsedTime / duration);

         if (textElement != null)
            textElement.text = Mathf.RoundToInt(currentValue).ToString();

         yield return null;
      }

      if (textElement != null)
         textElement.text = endValue.ToString();

      // Update our tracker variables
      if (isPearl) displayedPearlCount = endValue;
      else displayedOreCount = endValue;
   }
}