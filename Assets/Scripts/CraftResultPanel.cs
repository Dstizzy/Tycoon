using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftResultPanel : MonoBehaviour
{
   [SerializeField] private TextMeshProUGUI messageText;
   [SerializeField] private Button          closeButton;

   private void Start()
   {
      if (closeButton != null)
         closeButton.onClick.AddListener(() => gameObject.SetActive(false));
   }

   public void ShowSuccess(int amount, string toolName)
   {
      gameObject.SetActive(true);
      messageText.text  = $"Congratulations.\nYou've crafted {amount} {toolName}!";
      messageText.color = Color.darkGreen;
   }

   public void ShowFailure()
   {
      gameObject.SetActive(true);
      messageText.text  = "Sorry.\nNot enough ores for crafting.";
      messageText.color = Color.darkRed;
   }
}
