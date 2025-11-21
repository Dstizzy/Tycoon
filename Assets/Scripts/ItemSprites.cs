using UnityEngine;

using static Item;

public class ItemSprites : MonoBehaviour {
    public static ItemSprites itemSprites { get; private set; }
    
    [Header("Item Sprites")]
    public Sprite crudeTool;
    public Sprite refinedTool;
    public Sprite artifact;
    public Sprite sword;

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
                return refinedTool;
            case ItemType.Engine:
                return artifact;
            case ItemType.Sword:
                return sword;
            default:
                return null;
        }
    }
}
