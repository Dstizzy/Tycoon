using UnityEngine;

public class Item {

   const int CRUDE_TOOL_SELL_VALUE    = 5;
   const int REFINED_TOOL_SELL_VALUE  = 10;
   const int ARTIFACT_TOOL_SELL_VALUE = 20;
   const string CRUDE_TOOL_DESCRIPTION = 
      "A basic tool made from rudimentary materials. " +
      "Useful for simple tasks but lacks durability.";
   const string REFINED_TOOL_DESCRIPTION =
      "A well-crafted tool made from high-quality materials. " +
      "Offers better performance and durability for various tasks.";
   const string ARTIFACT_DESCRIPTION = 
      "An ancient artifact recovered from the depths. " +
      "Artifacts can be sold for a high price or used in special research.";


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
            case ItemType.Sword:
                return SWORD_PRICE;
            default:
                Debug.LogError("Unkown Item");
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

   public static string GetItemDescription(ItemType itemType) {
      switch (itemType) {
         case ItemType.CrudeTool:
            return CRUDE_TOOL_DESCRIPTION;
         case ItemType.RefinedTool:
            return REFINED_TOOL_DESCRIPTION;
         case ItemType.Artifact:
            return ARTIFACT_DESCRIPTION;
         default:
            return "No description available.";
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
