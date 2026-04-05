using UnityEngine;

using static Item;

public class ItemSprites : MonoBehaviour {
    public static ItemSprites itemSprites { get; set; }
    
    [Header("Item Sprites")]
    public Sprite crudeTool;
    public Sprite harpoon;
    public Sprite patchKit;
    public Sprite pressureValve;
    public Sprite divingBells;
    public Sprite engine;
    public Sprite precisionLens;
    public Sprite rareOre;
    public Sprite industrialBluePrint;
    public Sprite clockworkBlueprint;
    public Sprite merceneryEngineer;
    public Sprite insurancePolicy;

    private void Awake() 
    {

      if (itemSprites != null && itemSprites != this) 
      {
         Destroy(gameObject);
         return;
      }

      itemSprites = this;
    }

    public Sprite GetSprite(ItemType itemType) 
    {
        switch (itemType) 
         {
            case ItemType.CrudeTool:
               return crudeTool;
            case ItemType.Harpoon:
               return harpoon;
            case ItemType.PatchKit:
               return patchKit;
            case ItemType.PressureValve:
               return pressureValve;
            case ItemType.DivingBell: 
               return divingBells;
            case ItemType.Engine:
               return engine;
            case ItemType.RawOreChunk:
               return rareOre;
            case ItemType.PrecisionLens:
               return precisionLens;
            case ItemType.IndustrialBlueprint:
               return industrialBluePrint;
            case ItemType.ClockworkBlueprint:
               return clockworkBlueprint;
            case ItemType.MercenaryEngineer:
               return merceneryEngineer;
            case ItemType.InsurancePolicy:
               return insurancePolicy;
            default:
               Debug.LogError("ItemSprites: GetSprite received unknown ItemType: " + itemType);
               return null;
        }
    }
}
