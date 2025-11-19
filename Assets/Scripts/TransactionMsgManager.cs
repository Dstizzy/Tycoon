using UnityEngine;
using UnityEngine.UI;

public class TransactionMsgManager : MonoBehaviour
{
   public static TransactionMsgManager Instance;

   [Header("Panels")]
   public GameObject successPanel;
   public GameObject failPanel;

   [Header("Messages")]
   public Text successText;
   public Text failText;

   private void Awake()
   {
      Instance = this;

      gameObject.  SetActive(false);
      successPanel.SetActive(false);
      failPanel.   SetActive(false);
    }

   public void ShowSuccess(string msg)
   {
      gameObject.  SetActive(true);
      successPanel.SetActive(true);
      failPanel.   SetActive(false);

      successText.text = msg;
   }

   public void ShowFailure(string msg)
   {
      gameObject.  SetActive(true);
      failPanel.   SetActive(true);
      successPanel.SetActive(false);

      failText.text = msg;
   }

   public void CloseAll()
   {
      successPanel.SetActive(false);
      failPanel.   SetActive(false);
      gameObject.  SetActive(false);
   }
}
