
using UnityEditor.Experimental;

using UnityEngine;

using static Resource;

public class ResourceSrpites : MonoBehaviour
{
    public static ResourceSrpites resourceSrpites { get; private set; }

    [Header("Resource Sprites")]
    [SerializeField] private Sprite Pearl;
    [SerializeField] private Sprite Crystal;


    private void Awake() {
        if (resourceSrpites != null && resourceSrpites != this) {
            Destroy(gameObject);
        } else {
            resourceSrpites = this;
        }
    }

    public Sprite GetSprite(ResourceType resourceType) {
        switch (resourceType) {
            case ResourceType.Pearl:
                return Pearl;
            case ResourceType.Crystal:
                return Crystal;
            default:
                return null;
        }
    }
}
