using UnityEngine;

public class Item {

   const int CRUDE_TOOL_SELL_VALUE = 5;
   const int REFINED_TOOL_SELL_VALUE = 10;
   const int ARTIFACT_TOOL_SELL_VALUE = 20;

   const int SWORD_PRICE = 5;

    public enum ItemType {
        CrudeTool,
        RefinedTool,
        Artifact,
        Sword
    }

    public static int GetItemValue(ItemType itemType) {
        switch (itemType) {
            case ItemType.CrudeTool:
                return CRUDE_TOOL_SELL_VALUE;
         case ItemType.RefinedTool:
                return REFINED_TOOL_SELL_VALUE;
            case ItemType.Artifact:
                return ARTIFACT_TOOL_SELL_VALUE;
            default:
                return 0;
        }
    }

   public static int GetItemPrice(ItemType itemType) {
      switch (itemType) {
         case ItemType.Sword:
            return SWORD_PRICE;
         default:
            return 0;
      }
   }

    public static Sprite GetItemSprite(ItemType itemType) {
        if (ItemSprites.itemSprites == null) {
            Debug.LogError("ItemSprites.Instance is NULL! Cannot retrieve sprites.");
            return null;
        }

        return ItemSprites.itemSprites.GetSprite(itemType);
    }
}
