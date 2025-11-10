using UnityEngine;
using UnityEngine.EventSystems;

public class NodeHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    [SerializeField] private GameObject tierInfoPopUp;

    private void OnPointerEnter(PointerEventData EventData)
    {
        tierInfoPopUp.SetActive(true);
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        OnPointerEnter(eventData);
    }

    private void OnPointerExit(PointerEventData EventData)
    {
        tierInfoPopUp.SetActive(false);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        OnPointerExit(eventData);
    }
}




