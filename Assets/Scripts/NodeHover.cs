using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NodeHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
   [SerializeField] private GameObject InfoPopUp;
   [SerializeField] private UIFade uiFade;
   [SerializeField] private Button associatedButton;

   public bool alwaysShow = true;
   public bool isTierLocked = false;

   [Header("Hover Speeds")]
   [SerializeField] private float fadeInSpeed = 0.15f;  
   [SerializeField] private float fadeOutSpeed = 0.1f;  

   private void Awake()
   {
      if (associatedButton == null)
         associatedButton = GetComponent<Button>();
   }

   public void OnPointerEnter(PointerEventData eventData)
   {
      if (associatedButton != null && associatedButton.interactable)
      {
         return;
      }

      if (InfoPopUp != null)
      {
         InfoPopUp.SetActive(true);
         if (uiFade != null) uiFade.Appear(fadeInSpeed);
      }
   }

   public void OnPointerExit(PointerEventData eventData)
   {
      if (InfoPopUp != null && InfoPopUp.activeInHierarchy)
      {
         if (uiFade != null)
         {
            uiFade.Disappear(fadeOutSpeed);
         }
         else
         {
            InfoPopUp.SetActive(false);
         }
      }
   }
}