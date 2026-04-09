using UnityEngine;

public class WorldEventSprites : MonoBehaviour
{

   public static WorldEventSprites worldEventSprites { get; set; }

   [Header("World Event Sprites")]
   public Sprite industrialGodlRushEventIcon;

   private void Awake() {

      if (worldEventSprites != null && worldEventSprites != this) 
      {
         Destroy(gameObject);
         return;
      }

      worldEventSprites = this;
   }

   public Sprite GetWorldEventSprite(WorldEvents.WorldEventTypes worldEventType) 
   {
      switch(worldEventType) 
      { 
         case WorldEvents.WorldEventTypes.IndustrialGoldRushEvent:
            return industrialGodlRushEventIcon;
         default:
            Debug.LogError("WorldEventSprite: GetSprite received unknown WorldEventType: " + worldEventType);
            return null;
      }
   }
}
