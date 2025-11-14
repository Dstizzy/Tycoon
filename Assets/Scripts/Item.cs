using UnityEngine;

public class Item {

   static int crudeToolSellValue   = 5;
   static int refinedToolSellValue = 10;
   static int artifactSellValue    = 20;
   static int swordPrice           = 5;
   const string CRUDE_TOOL_DESCRIPTION   = 
      "A basic tool made from rudimentary materials. " +
      "Useful for simple tasks but lacks durability.";
   const string REFINED_TOOL_DESCRIPTION =
      "A well-crafted tool made from high-quality materials. " +
      "Offers better performance and durability for various tasks.";
   const string ARTIFACT_DESCRIPTION = 
      "An ancient artifact recovered from the depths. " +
      "Artifacts can be sold for a high price or used in special research.";

    public enum ItemType {
        CrudeTool,
        RefinedTool,
        Artifact,
        Sword
    }

    public static int GetItemValue(ItemType itemType) {
        switch (itemType) {
            case ItemType.CrudeTool:
                return crudeToolSellValue;
            case ItemType.RefinedTool:
                return refinedToolSellValue;
            case ItemType.Artifact:
                return artifactSellValue;
            case ItemType.Sword:
                return swordPrice;
            default:
                Debug.LogError("Unkown Item");
            return 0;
        }
    }

   public static int GetItemPrice(ItemType itemType) {
      switch (itemType) {
         case ItemType.Sword:
            return swordPrice;
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
