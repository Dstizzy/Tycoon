using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.UI;

public class Item {
    public enum ItemType {
        CrudeTool,
        RefinedTool,
        Artifact
    }

    public static int GetItemValue(ItemType itemType) {
        switch (itemType) {
            case ItemType.CrudeTool:
                return 5;
            case ItemType.RefinedTool:
                return 10;
            case ItemType.Artifact:
                return 20;
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
