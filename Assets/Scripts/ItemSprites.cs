using UnityEngine;

using static Item;

public class ItemSprites : MonoBehaviour {
    public static ItemSprites itemSprites { get; private set; }
    
    [Header("Item Sprites")]
    public Sprite crudeTool;
    public Sprite refinedTool;
    public Sprite artifact;

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
            case ItemType.RefinedTool:
                return refinedTool;
            case ItemType.Artifact:
                return artifact;
            default:
                return null;
        }
    }
}
