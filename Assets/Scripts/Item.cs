using System;

using UnityEngine;

public class Item {

   /* Public static properties                                          */
   public static int base_crude_tool_value     = 30;
   public static int base_harpoon_value        = 60;
   public static int base_diving_bell_value    = 120;
   public static int base_pressure_valve_value = 250;
   public static int base_precision_lens_value = 600;
   public static int base_engine_value         = 900;

   private static int crudeToolSellValue     { get; set; } = base_crude_tool_value;
   private static int harpoonSellValue       { get; set; } = base_harpoon_value;
   private static int pressureValveSellValue { get; set; } = base_pressure_valve_value;
   private static int divingBellSellValue    { get; set; } = base_diving_bell_value;
   private static int precisionLensSellValue { get; set; } = base_precision_lens_value;
   private static int engineSellValue        { get; set; } = base_engine_value;
   private static int rawOrePrice            { get; set; } = 1;
   private static int mercenaryEngineerPrice { get; set; } = 100;
   private static int insurancePolicyPrice   { get; set; } = 100;
   private static int tier2BluePrintPrice    { get; set; } = 300;
   private static int tier3BluePrintPrice    { get; set; } = 500;

   public static int tierOneIncreaseFactor { get; private set; } = 2;

   public const int MIN_CRUDE_TOOL_VALUE           = 0;
   public const int MAX_CRUDE_TOOL_VALUE           = 250;
   public const int MIN_HARPOON_VALUE              = 0;
   public const int MAX_HARPOON_VALUE              = 250;
   public const int MIN_DIVING_BELL_VALUE          = 0;
   public const int MAX_DIVING_BELL_VALUE          = 250;
   public const int MIN_PRESSURE_VALVE_VALUE       = 0;
   public const int MAX_PRESSURE_VALVE_VALUE       = 540;
   public const int MIN_PRECISION_LENS_VALUE       = 0;
   public const int MAX_PRECISION_LENS_VALUE       = 1800;
   public const int MIN_ENGINE_VALUE               = 0;
   public const int MAX_ENGINE_VALUE               = 2700;

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
   const string INSURANCE_POLICY_DESCRIPTION =
      "Pays a 500 pearl payout if the an item in the sell market crashes within the next 5 turns.";

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
        MercenaryEngineer,
        InsurancePolicy
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
            return tier2BluePrintPrice;
         case ItemType.ClockworkBlueprint:
            return tier3BluePrintPrice;
         case ItemType.MercenaryEngineer:
            return mercenaryEngineerPrice;
         case ItemType.InsurancePolicy:
            return insurancePolicyPrice;
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
         case ItemType.InsurancePolicy:
            return INSURANCE_POLICY_DESCRIPTION;
         default:
            Debug.LogError("No description available.");
            return "No description available.";
      }
   }

    public static Sprite GetItemSprite(ItemType itemType) 
    {
      ItemSprites.itemSprites = UnityEngine.Object.FindFirstObjectByType<ItemSprites>();
        
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

    // --- Diving Bell increase/decrease methods ---
   public static void TryIncreaseDivingBellValue(int amount)
   {
      if (divingBellSellValue >= MAX_DIVING_BELL_VALUE)
      {
         Debug.LogError("Diving Bell Sell Value is already at maximum!");
         return;
      }

      if (divingBellSellValue + amount > MAX_DIVING_BELL_VALUE)
      {
         Debug.LogError($"Cannot increase by {amount}. Max value is {MAX_DIVING_BELL_VALUE}.");
         return;
      }

      divingBellSellValue += amount;
      OnItemValueChange?.Invoke(divingBellSellValue, ItemType.DivingBell);
   }

   public static void TryDecreaseDivingBellValue(int amount)
   {
      if (divingBellSellValue <= MIN_DIVING_BELL_VALUE)
      {
         Debug.LogError("Diving Bell Sell Value is already at minimum!");
         return;
      }

      if (divingBellSellValue - amount < MIN_DIVING_BELL_VALUE)
      {
         Debug.LogError($"Cannot decrease by {amount}. Min value is {MIN_DIVING_BELL_VALUE}.");
         return;
      }

      divingBellSellValue -= amount;
      OnItemValueChange?.Invoke(divingBellSellValue, ItemType.DivingBell);
   }

   

   public static void TryIncreasePrecisionLensValue(int amount)
   {
      if (precisionLensSellValue >= MAX_PRECISION_LENS_VALUE)
      {
         Debug.LogError("Precision Lens Sell Value is already at maximum!");
         return;
      }

      if (precisionLensSellValue + amount > MAX_PRECISION_LENS_VALUE)
      {
         Debug.LogError($"Cannot increase by {amount}. Max value is {MAX_PRECISION_LENS_VALUE}.");
         return;
      }

      precisionLensSellValue += amount;
      OnItemValueChange?.Invoke(precisionLensSellValue, ItemType.PrecisionLens);
   }

   public static void TryDecreasePrecisionLensValue(int amount)
   {
      if (precisionLensSellValue <= MIN_PRECISION_LENS_VALUE)
      {
         Debug.LogError("Precision Lens Sell Value is already at minimum!");
         return;
      }

      if (precisionLensSellValue - amount < MIN_PRECISION_LENS_VALUE)
      {
         Debug.LogError($"Cannot decrease by {amount}. Min value is {MIN_PRECISION_LENS_VALUE}.");
         return;
      }

      precisionLensSellValue -= amount;
      OnItemValueChange?.Invoke(precisionLensSellValue, ItemType.PrecisionLens);
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

   public static void TryDecreaseEngineSellValue(int amount) 
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
      if ( tier2BluePrintPrice >= MAX_ENGINE_VALUE) 
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
      tier2BluePrintPrice    -= (int)(tier2BluePrintPrice * percent);
      tier3BluePrintPrice    -= (int)(tier3BluePrintPrice * percent);
      mercenaryEngineerPrice -= (int)(mercenaryEngineerPrice * percent);
   }

   public static void AdjustSellItemsBaseValue(float percent) 
   {
      base_crude_tool_value     += (int) (base_crude_tool_value * percent);
      base_harpoon_value        += (int) (base_harpoon_value * percent);
      base_pressure_valve_value += (int) (base_pressure_valve_value * percent);
      base_diving_bell_value    += (int) (base_diving_bell_value * percent);
      base_precision_lens_value += (int) (base_precision_lens_value * percent);
      base_engine_value         += (int) (base_engine_value * percent);
   }

   // Resets all static prices and events back to default for a new game
   public static void ResetPrices() 
   {
      // Reset Sell Values
      crudeToolSellValue     = base_crude_tool_value;
      harpoonSellValue       = base_harpoon_value;
      pressureValveSellValue = base_pressure_valve_value;
      divingBellSellValue    = base_diving_bell_value;
      precisionLensSellValue = base_precision_lens_value;
      engineSellValue        = base_engine_value;

      // Reset Buy Prices (hardcoded defaults from your initializers)
      rawOrePrice            = 1;
      mercenaryEngineerPrice = 100;
      insurancePolicyPrice   = 100;
      tier2BluePrintPrice    = 500;
      tier3BluePrintPrice    = 2000;

      // Clear the static action delegate to prevent memory leaks/missing references
      OnItemValueChange = null;
   }
}
