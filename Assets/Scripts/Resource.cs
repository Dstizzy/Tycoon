using UnityEngine;

public class Resource {
    public enum ResourceType {
        Pearl,
        Crystal,
    }

    public static Sprite GetResourceSprite(ResourceType resourceType) {
        if (ResourceSrpites.resourceSrpites == null) {
            Debug.LogError("resourceSrpites is NULL! Cannot retrieve sprites.");
            return null;
        }

        return ResourceSrpites.resourceSrpites.GetSprite(resourceType);
    }
}
