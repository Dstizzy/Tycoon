using System;

using TMPro;

using UnityEngine;
using static TradeHutManager;

public class Item {

   /* Public static properties                                          */
   private static int crudeToolSellValue     { get; set; } = BASE_CRUDE_TOOL_SELL_VALUE;
   private static int harpoonSellValue       { get; set; } = 60;
   private static int pressureValveSellValue { get; set; } = 180;
   private static int divingBellSellValue    { get; set; } = 250;
   private static int precisionLensSellValue { get; set; } = 600;
   private static int engineSellValue        { get; set; } = 900;
   private static int rawOrePrice            { get; set; } = 1;
   private static int mercenaryEngineerPrice { get; set; } = 100;
   private static int Tier2BluePrintPrice    { get; set; } = 500;
   private static int Tier3BluePrintPrice    { get; set; } = 2000;

   public static int tierOneIncreaseFactor { get; private set; } = 2;

   public const int BASE_CRUDE_TOOL_SELL_VALUE     = 30;
   public const int BASE_HARPON_SELL_VALUE         = 60;
   public const int BASE_PRESSURE_VALVE_SELL_VALUE = 180;
   public const int BASE_ENGINE_VALUE              = 900;
   public const int MIN_CRUDE_TOOL_VALUE           = 0;
   public const int MAX_CRUDE_TOOL_VALUE           = 60;
   public const int MIN_HARPOON_VALUE              = 0;
   public const int MAX_HARPOON_VALUE              = 120;
   public const int MIN_PRESSURE_VALVE_VALUE       = 0;
   public const int MAX_PRESSURE_VALVE_VALUE       = 360;
   public const int MIN_ENGINE_VALUE               = 0;
   public const int MAX_ENGINE_VALUE               = 1800;

   const string CRUDE_TOOL_DESCRIPTION           = 
      "A basic tool made from rudimentary materials. " +
      "Useful for simple tasks but lacks durability.";
   const string HARPOON_DESCRIPTION              =
     "A reinforced harpoon crafted for hunting and defense — effective against leviathans like the kraken. " +
      "More durable than a crude tool, and sells for a higher price.";
   const string PRESSURE_VALVE_DESCRIPTION       =
      "A well-crafted tool made from high-quality materials. " +
      "Offers better performance and durability for various tasks.";
   const string ENGINE_DESCRIPTION               = 
      "An ancient artifact recovered from the depths. " +
      "Artifacts can be sold for a high price or used in special research.";
   const string RAW_ORE_CHUNK_DESCRIPTION        =
      "Exchanges at the Trade Hut for pearls at a 1:1 ratio.";
   const string PATCH_KIT_DESCRIPTION            =
     "A compact repair kit containing patches, resin and basic tools. " +
     "Used to repair equipment or as a component in crafting.";
   const string MERCENARY_ENGINEER_DESCRIPTION   =
      "A specialist who can immediately craft a single item.  " +
      "Consumed on use — ideal when you need an item instantly.";
   const string PRECISION_LENS_DESCRIPTION       =
     "A small optical component used to focus delicate mechanisms. " +
     "Required for precision assemblies; consumed during crafting.";
   const string DIVING_BELL_DESCRIPTION          =
      "A reinforced submersible chamber that enables the Exploration Unit. " +
      "Possessing a Diving Bell allows deployment of the unit for scouting and resource miss";
   const string INDUSTRIAL_BLUEPRINT_DESCRIPTION =
      "Unlocks tier 2 items recipes at the Forge.";
   const string CLOCKWORK_BLUEPRINT_DESCRIPTION  =
      "Unlocks tier 3 recipes at the Forge.";

   public static Action<int, ItemType> OnItemValueChange;

   public enum ItemType {
        CrudeTool,
        Harpoon,
        PatchKit,
        PressureValve,
        DivingBell,
        Engine,
        PrecisionLens,
        RawOreChunk,
        IndustrialBlueprint,
        ClockworkBlueprint,
        MercenaryEngineer
   }

    public static int GetItemValue(ItemType itemType) 
    {
        switch (itemType) 
        {
           case ItemType.CrudeTool:
               return crudeToolSellValue;
           case ItemType.Harpoon:
               return harpoonSellValue;
           case ItemType.PressureValve:
              return pressureValveSellValue;
           case ItemType.DivingBell:
              return divingBellSellValue;
           case ItemType.PrecisionLens:
              return precisionLensSellValue;
           case ItemType.Engine:
               return engineSellValue;
           default:
               Debug.LogError("Unkown Item");
               return 0;
        }
    }

   public static int GetItemPrice(ItemType itemType) 
   {
      switch (itemType) 
      {
         case ItemType.RawOreChunk:
            return rawOrePrice;
         case ItemType.IndustrialBlueprint:
            return Tier2BluePrintPrice;
         case ItemType.ClockworkBlueprint:
            return Tier3BluePrintPrice;
         case ItemType.MercenaryEngineer:
            return mercenaryEngineerPrice;
         default:
            Debug.LogError($"Unknown Item: `{itemType}`");
            return 0;
      }
   }

   public static string GetItemDescription(ItemType itemType) 
   {
      switch (itemType) 
      {
         case ItemType.CrudeTool:
            return CRUDE_TOOL_DESCRIPTION;
         case ItemType.Harpoon:
            return HARPOON_DESCRIPTION;
         case ItemType.PatchKit:
            return PATCH_KIT_DESCRIPTION;
         case ItemType.PressureValve:
            return PRESSURE_VALVE_DESCRIPTION;
         case ItemType.Engine:
            return ENGINE_DESCRIPTION;
         case ItemType.RawOreChunk:
            return RAW_ORE_CHUNK_DESCRIPTION;
         case ItemType.IndustrialBlueprint:
            return INDUSTRIAL_BLUEPRINT_DESCRIPTION;
         case ItemType.ClockworkBlueprint:
            return CLOCKWORK_BLUEPRINT_DESCRIPTION;
         case ItemType.MercenaryEngineer:
            return MERCENARY_ENGINEER_DESCRIPTION;
         case ItemType.PrecisionLens:
            return PRECISION_LENS_DESCRIPTION;
         case ItemType.DivingBell:
            return DIVING_BELL_DESCRIPTION;
         default:
            Debug.LogError("No description available.");
            return "No description available.";
      }
   }

    public static Sprite GetItemSprite(ItemType itemType) 
    {
        if (ItemSprites.itemSprites == null) 
        {
            Debug.LogError("ItemSprites.Instance is NULL! Cannot retrieve sprites.");
            return null;
        }

        return ItemSprites.itemSprites.GetSprite(itemType);
    }

   public static void TryIncreaseCrudeToolSellValue(int amount) 
   {
      // 1. Check if adding the amount would exceed the MAX_VALUE
      if (crudeToolSellValue >= MAX_CRUDE_TOOL_VALUE) 
      {
         Debug.LogError("Crude Tool Sell Value is already at maximum!");
         return;
      }

      // 2. Check if the *new* value would exceed the maximum.
      // We use Math.Max to see what the new value will be if clamped, and compare it.
      if (crudeToolSellValue + amount > MAX_CRUDE_TOOL_VALUE) 
      {
         Debug.LogError($"Cannot increase by {amount}. Max value is {MAX_CRUDE_TOOL_VALUE}.");
         return;
      }

      // 3. If checks pass, perform the increase. The setter enforces the clamp just in case.
      crudeToolSellValue += amount;

      OnItemValueChange?.Invoke(crudeToolSellValue, ItemType.CrudeTool);

      return;
   }

   public static void TryDecreaseCrudeToolSellValue(int amount) 
   {
      // 1. Check if the value is already at the MIN_VALUE
      if (crudeToolSellValue <= MIN_CRUDE_TOOL_VALUE) 
      {
         Debug.LogError("Crude Tool Sell Value is already at minimum!");
         return;
      }

      // 2. Check if subtracting the amount would drop below the minimum.
      if (crudeToolSellValue - amount < MIN_CRUDE_TOOL_VALUE) 
      {
         Debug.LogError($"Cannot decrease by {amount}. Min value is {MIN_CRUDE_TOOL_VALUE}.");
         return;
      }

      // 3. If checks pass, perform the decrease. The setter enforces the clamp just in case.
      crudeToolSellValue -= amount;
      OnItemValueChange?.Invoke(crudeToolSellValue, ItemType.CrudeTool);

      return;
   }

   public static void TryIncreasePressureValveValue(int amount) 
   {
      // 1. Check if adding the amount would exceed the MAX_VALUE
      if (pressureValveSellValue >= MAX_PRESSURE_VALVE_VALUE) 
      {
         Debug.LogError("Crude Tool Sell Value is already at maximum!");
         return;
      }

      // 2. Check if the *new* value would exceed the maximum.
      // We use Math.Max to see what the new value will be if clamped, and compare it.
      if (pressureValveSellValue + amount > MAX_PRESSURE_VALVE_VALUE) 
      {
         Debug.LogError($"Cannot increase by {amount}. Max value is {MAX_PRESSURE_VALVE_VALUE}.");
         return;
      }

      // 3. If checks pass, perform the increase. The setter enforces the clamp just in case.
      pressureValveSellValue += amount;

      OnItemValueChange?.Invoke(pressureValveSellValue, ItemType.PressureValve);

      return;
   }

   public static void TryDecreasePressureValveValue(int amount) 
   {
      // 1. Check if the value is already at the MIN_VALUE
      if (pressureValveSellValue <= MIN_PRESSURE_VALVE_VALUE) 
      { 
         Debug.LogError("Crude Tool Sell Value is already at minimum!");
         return;
      }

      // 2. Check if subtracting the amount would drop below the minimum.
      if (pressureValveSellValue - amount < MIN_PRESSURE_VALVE_VALUE) 
      {
         Debug.LogError($"Cannot decrease by {amount}. Min value is {MIN_PRESSURE_VALVE_VALUE}.");
         return;
      }

      // 3. If checks pass, perform the decrease. The setter enforces the clamp just in case.
      pressureValveSellValue -= amount;

      OnItemValueChange?.Invoke(pressureValveSellValue, ItemType.PressureValve);

      return;
   }

   public static void TryIncreaseHarpoonSellValue(int amount) 
   {
      // 1. Check if adding the amount would exceed the MAX_VALUE
      if (harpoonSellValue >= MAX_HARPOON_VALUE) 
      {
         Debug.LogError("Harpoon Sell Value is already at maximum!");
         return;
      }

      // 2. Check if the *new* value would exceed the maximum.
      // We use Math.Max to see what the new value will be if clamped, and compare it.
      if (harpoonSellValue + amount > MAX_HARPOON_VALUE) 
      {
         Debug.LogError($"Cannot increase by {amount}. Max value is {MAX_HARPOON_VALUE}.");
         return;
      }

      // 3. If checks pass, perform the increase. The setter enforces the clamp just in case.
      harpoonSellValue += amount;

      OnItemValueChange?.Invoke(harpoonSellValue, ItemType.Harpoon);

      return;
   }

   public static void TryDecreaseHarpoonSellValue(int amount) 
   {
      // 1. Check if the value is already at the MIN_VALUE
      if (harpoonSellValue <= MIN_HARPOON_VALUE)
      {
         Debug.LogError("Crude Tool Sell Value is already at minimum!");
         return;
      }

      // 2. Check if subtracting the amount would drop below the minimum.
      if (harpoonSellValue - amount < MIN_HARPOON_VALUE) 
      {
         Debug.LogError($"Cannot decrease by {amount}. Min value is {MIN_HARPOON_VALUE}.");
         return;
      }

      // 3. If checks pass, perform the decrease. The setter enforces the clamp just in case.
      harpoonSellValue -= amount;

      OnItemValueChange?.Invoke(harpoonSellValue, ItemType.Harpoon);

      return;
   }

   public static void TryIncreaseEngineSellValue(int amount) 
   {
      // 1. Check if adding the amount would exceed the MAX_VALUE
      if (engineSellValue >= MAX_ENGINE_VALUE) 
      {
         Debug.LogError("Crude Tool Sell Value is already at maximum!");
         return;
      }

      // 2. Check if the *new* value would exceed the maximum.
      // We use Math.Max to see what the new value will be if clamped, and compare it.
      if (engineSellValue + amount > MAX_ENGINE_VALUE) 
      {
         Debug.LogError($"Cannot increase by {amount}. Max value is {MAX_ENGINE_VALUE}.");
         return;
      }

      // 3. If checks pass, perform the increase. The setter enforces the clamp just in case.
      engineSellValue += amount;

      OnItemValueChange?.Invoke(engineSellValue, ItemType.Engine);
      return;
   }

   public static void TryDecreaseEnginesSellValue(int amount) 
   {
      // 1. Check if the value is already at the MIN_VALUE
      if (engineSellValue <= MIN_ENGINE_VALUE) 
      {
         Debug.LogError("Crude Tool Sell Value is already at minimum!");
         return;
      }

      // 2. Check if subtracting the amount would drop below the minimum.
      if (engineSellValue - amount < MIN_ENGINE_VALUE) 
      {
         Debug.LogError($"Cannot decrease by {amount}. Min value is {MIN_ENGINE_VALUE}.");
         return;
      }

      // 3. If checks pass, perform the decrease. The setter enforces the clamp just in case.
      engineSellValue -= amount;

      OnItemValueChange?.Invoke(engineSellValue, ItemType.Engine);

      return;
   }
   public static void TryIncreaseTier2BlueprintPrice(int amount) 
   {
      // 1. Check if adding the amount would exceed the MAX_VALUE
      if ( Tier2BluePrintPrice >= MAX_ENGINE_VALUE) 
      {
         Debug.LogError("Crude Tool Sell Value is already at maximum!");
         return;
      }

      // 2. Check if the *new* value would exceed the maximum.
      // We use Math.Max to see what the new value will be if clamped, and compare it.
      if (engineSellValue + amount > MAX_ENGINE_VALUE) 
      {
         Debug.LogError($"Cannot increase by {amount}. Max value is {MAX_ENGINE_VALUE}.");
         return;
      }

      // 3. If checks pass, perform the increase. The setter enforces the clamp just in case.
      engineSellValue += amount;

      OnItemValueChange?.Invoke(engineSellValue, ItemType.Engine);
      return;
   }

   public static void TryDecreaseTier2BlueprintPrice(int amount) 
   {
      // 1. Check if the value is already at the MIN_VALUE
      if (engineSellValue <= MIN_ENGINE_VALUE) 
      {
         Debug.LogError("Crude Tool Sell Value is already at minimum!");
         return;
      }

      // 2. Check if subtracting the amount would drop below the minimum.
      if (engineSellValue - amount < MIN_ENGINE_VALUE) 
      {
         Debug.LogError($"Cannot decrease by {amount}. Min value is {MIN_ENGINE_VALUE}.");
         return;
      }

      // 3. If checks pass, perform the decrease. The setter enforces the clamp just in case.
      engineSellValue -= amount;

      OnItemValueChange?.Invoke(engineSellValue, ItemType.Engine);

      return;
   }

   public static void ApplyDiscountToBuyItems(float percent) 
   {
      TextMeshProUGUI rareOrePriceText           = new();
      TextMeshProUGUI Tier2BluePrintPriceText    = new();
      TextMeshProUGUI Tier3BluePrintPriceText    = new();
      TextMeshProUGUI mercenaryEngineerPriceText = new();

      rawOrePrice          -= (int)(rawOrePrice * percent);
      rareOrePriceText      = Instance.BuyItems.Find(item => item.CompareTag(RAW_ORE_CHUNK_TAG)).Find("ItemValue").GetComponent<TextMeshProUGUI>();
      rareOrePriceText.text = rawOrePrice.ToString();

      Tier2BluePrintPrice         -= (int)(Tier2BluePrintPrice * percent);
      Tier2BluePrintPriceText      = Instance.BuyItems.Find(item => item.CompareTag(CLOCKWORK_BLUEPRINT_TAG)).Find("ItemValue").GetComponent<TextMeshProUGUI>();
      Tier2BluePrintPriceText.text = Tier2BluePrintPrice.ToString();

      Tier3BluePrintPrice         -= (int)(Tier3BluePrintPrice * percent);
      Tier3BluePrintPriceText      = Instance.BuyItems.Find(item => item.CompareTag(INDUSTRIAL_BLUEPRINT_TAG)).Find("ItemValue").GetComponent<TextMeshProUGUI>();
      Tier3BluePrintPriceText.text = Tier3BluePrintPrice.ToString();

      mercenaryEngineerPrice         -= (int)(mercenaryEngineerPrice * percent);
      mercenaryEngineerPriceText      = Instance.BuyItems.Find(item => item.CompareTag(InventoryManager.MERCENARY_ENGINEER_TAG)).Find("ItemValue").GetComponent<TextMeshProUGUI>();
      mercenaryEngineerPriceText.text = mercenaryEngineerPrice.ToString();
   }

}
