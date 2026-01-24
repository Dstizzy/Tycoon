using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
   public Item.ItemType itemType;

   [Header("Optional UI")]
   public TextMeshProUGUI itemName;
   public TextMeshProUGUI itemValue;
   public Image itemIcon;
}
