using UnityEngine;

using static Item;

public class CraftItemSprites : MonoBehaviour
{
   public static CraftItemSprites itemSprites { get; private set; }

   [Header("Item Sprites")]
   public Sprite crudeTool;
   public Sprite harpoonGun;
   public Sprite patchKit;
   public Sprite pressureValve;
   public Sprite divingBell;
   public Sprite clockworkEngine;
   public Sprite precisionLens;



   private void Awake()
   {
      if (itemSprites != null && itemSprites != this)
      {
         Destroy(gameObject);
      }
      else
      {
         itemSprites = this;
      }
   }

   public Sprite GetSprite(ItemType itemType)
   {
      switch (itemType)
      {
         case ItemType.CrudeTool:
            return crudeTool;
         case ItemType.Harpoon:
            return harpoonGun;
         case ItemType.PatchKit:
            return patchKit;
         case ItemType.PressureValve:
            return pressureValve;
         case ItemType.DivingBell:
            return divingBell;
         case ItemType.Engine:
            return clockworkEngine;
         case ItemType.PrecisionLens:
            return precisionLens;
         default:
            return null;
      }
   }
}