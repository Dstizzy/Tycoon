using TMPro;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MainUIManager : MonoBehaviour
{

   // Buttons on the main UI
   public Button MainMenuButton;
   public Button[] DropdownButtons;
   public Button InventoryButton;
   public Button NextButton;

   // UI elements on main UI
   [SerializeField] private TextMeshProUGUI pearCountText;
   [SerializeField] private TextMeshProUGUI oreCountText;
   [SerializeField] private Transform erroPanel;
   [SerializeField] private GameObject tutorial;

   // State variable to track dropdown visibility
   private bool isVisible = false;
   private int displayedPearlCount = 0;
   private int displayedOreCount = 0;

   public static MainUIManager mainUI;

   public static MainUIManager Instance {get; private set; }

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

      if (NextButton != null)
      {
         NextButton.onClick.AddListener(() =>
         {
            if (AudioManager.Instance != null)
               AudioManager.Instance.PlayClick();
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
      foreach(var btn in DropdownButtons)
      {
         if (btn != null)
         {
            btn.gameObject.SetActive(isVisible);
            btn.onClick.RemoveAllListeners();
         }
      }
      DropdownButtons[0].onClick.AddListener(() => GoToStartScreen());
      DropdownButtons[1].onClick.AddListener(() => StartWalkthrough());
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

   public void GoToStartScreen()
   {
      if (SceneHistory.Instance != null) 
      {
         GameManager.RestartGame();
         SceneHistory.Instance.LoadScene("StartScreen");
      }
      else
         Debug.LogError("SceneHistory is missing from the scene!");
   }

   public void StartWalkthrough()
   {
      SceneManager.LoadScene("WalkthroughScene");
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