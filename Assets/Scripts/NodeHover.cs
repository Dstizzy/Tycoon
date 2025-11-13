using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;


public class NodeHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    [SerializeField] private GameObject infoPopUp;

    public void OnPointerEnter(PointerEventData eventData)
    {
        infoPopUp.SetActive(true);

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        infoPopUp.SetActive(false);
    }
}
