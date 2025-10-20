using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.UI;

public class Item {
    public enum ItemType {
        CrudeTool,
        RefinedTool,
        Articfatct
    }

    public static int GetItemValue(ItemType itemType) {
        switch(itemType) {
            case ItemType.CrudeTool: 
                return 5;
            case ItemType.RefinedTool: 
                return 10;
            case ItemType.Articfatct: 
                return 20;
            default:
                return 0;
        }
    }

    public static Sprite GetItemSprite(ItemType itemType) {
        switch(itemType) {
            case ItemType.CrudeTool: 
                return ItemSprites.itemSprites.crudeTool;
            case ItemType.RefinedTool:
                return ItemSprites.itemSprites.refinedTool;
            case ItemType.Articfatct: 
                return ItemSprites.itemSprites.artifact;
            default:
                return null;
        }
    }

}
