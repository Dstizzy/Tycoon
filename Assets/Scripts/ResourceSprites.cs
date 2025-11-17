using UnityEngine;

using static Resources;

public class ResourceSprites : MonoBehaviour {
   public static ResourceSprites resourceSprites { get; private set; }

   [Header("Resource Sprites")]
   [SerializeField] private Sprite Pearl;
   [SerializeField] private Sprite Crystal;
   [SerializeField] private Sprite Ore;


   private void Awake() {
      if (resourceSprites != null && resourceSprites != this) {
         Destroy(gameObject);
      } else {
         resourceSprites = this;
      }
   }

   public Sprite GetSprite(ResourceType resourceType) {
      switch (resourceType) {
         case ResourceType.Pearl:
            return Pearl;
         case ResourceType.Crystal:
            return Crystal;
         case ResourceType.Ore:
            return Ore;
         default:
            return null;
      }
   }
}
