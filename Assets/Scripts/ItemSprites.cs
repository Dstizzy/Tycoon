using UnityEngine;

using static Item;

public class ItemSprites : MonoBehaviour {
    public static ItemSprites itemSprites { get; private set; }
    
    [Header("Item Sprites")]
    public Sprite crudeTool;
    public Sprite harpoon;
    public Sprite pressureValve;
    public Sprite engine;
    public Sprite rareOre;
    public Sprite industrialBluePrint;
    public Sprite clockworkBlueprint;

    private void Awake() {
        if (itemSprites != null && itemSprites != this) {
            Destroy(gameObject);
        } else {
            itemSprites = this;
        }
    }

    public Sprite GetSprite(ItemType itemType) {
        switch (itemType) {
            case ItemType.CrudeTool:
               return crudeTool;
            case ItemType.Harpoon:
               return harpoon;
            case ItemType.PressureValve:
               return pressureValve;
            case ItemType.Engine:
               return engine;
            case ItemType.RareOre:
               return rareOre;
            case ItemType.IndustrialBluePrint:
               return industrialBluePrint;
            case ItemType.ClockworkBlueprint:
               return clockworkBlueprint;
         default:
               Debug.LogError("ItemSprites: GetSprite received unknown ItemType: " + itemType);
               return null;
        }
    }
}
