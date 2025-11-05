using UnityEngine;

public class Resources
{
   const string PEARL_DESCRIPTION = 
      "A valuable gem found underwater. " +
      "Pearls can used to purchase different items.";
   const string CRYSTAL_DESCRIPTION = 
      "A rare resource found underwater. " +
      "Crystals can be used to unlock certain research branches";

   public enum ResourceType {
     Pearl,
     Crystal
   }

   public static string GetResourceDescription(ResourceType resourceType) {
      switch (resourceType) {
         case ResourceType.Pearl:
            return PEARL_DESCRIPTION;
         case ResourceType.Crystal:
            return CRYSTAL_DESCRIPTION;
         default:
            Debug.LogError("Unkown Description");
            return "";
      }
   }

   public static Sprite GetResourceSprite(ResourceType resourceType) {
      if (ResourceSprites.resourceSprites == null) {
         Debug.LogError("ResourceSprites.resourceSprites is NULL! Cannot retrieve sprites.");
         return null;
      }

      return ResourceSprites.resourceSprites.GetSprite(resourceType);
   }
}
