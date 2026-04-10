// Libraries                                                                                     
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class NodeHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
   // Inspector Variables                                                                       
   [SerializeField] private GameObject InfoPopUp;
   [SerializeField] private UIFade uiFade;


   // Implements interface function for entering the game object with mouse                     
   void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
   {
       OnPointerEnter(eventData);
   }

   // Implements interface fucntion for exiting the game object with mouse                      
   void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
   {
       OnPointerExit(eventData);
   }

   // Sets the info panel active upon hovering over this object                                 
   private void OnPointerEnter(PointerEventData EventData)
   {
      //yield return new WaitForSeconds(.3f);
      InfoPopUp.SetActive(true);
      uiFade.Appear(1f);
   }

   // Sets the info panel inactive upon exiting this object                                     
   private void OnPointerExit(PointerEventData EventData)
   {
      uiFade.Disappear(.5f);
      InfoPopUp.SetActive(false);
   }
}




