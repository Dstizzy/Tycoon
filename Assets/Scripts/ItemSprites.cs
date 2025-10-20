using UnityEngine;

public class ItemSprites : MonoBehaviour
{
    public static ItemSprites itemSprites { get; private set; }

    private void Awake() {
        if (itemSprites != null && itemSprites != this) {
            Destroy(gameObject);
        } else {
            itemSprites = this;
        }
    }

    [Header("Item Sprites")]
    public Sprite crudeTool;
    public Sprite refinedTool;
    public Sprite artifact;
}
