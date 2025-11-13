/* Libraries                                                                                     */
using UnityEngine;
using UnityEngine.EventSystems;


public class NodeHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /* Inspector Variables                                                                       */
    [SerializeField] private GameObject tierInfoPopUp; /* Panel that gets activated upon hover   */

    /* Implements interface function for entering the game object with mouse                     */
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        OnPointerEnter(eventData);
    }

    /* Implements interface fucntion for exiting the game object with mouse                      */
    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        OnPointerExit(eventData);
    }

    /* Sets the info panel active upon hovering over this object                                 */
    private void OnPointerEnter(PointerEventData EventData)
    {
        tierInfoPopUp.SetActive(true);
    }

    /* Sets the info panel inactive upon exiting this object                                     */
    private void OnPointerExit(PointerEventData EventData)
    {
        tierInfoPopUp.SetActive(false);
    }

    
}




