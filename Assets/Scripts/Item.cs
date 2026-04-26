using System;

using UnityEngine;

public class Item {

   /* Public static properties                                          */
   public static int base_diving_bell_value    = 25;
   public static int base_crude_tool_value     = 40;
   public static int base_harpoon_value        = 80;
   public static int base_pressure_valve_value = 250;
   public static int base_precision_lens_value = 800;
   public static int base_engine_value         = 1200;

   private static int crudeToolSellValue     { get; set; } = base_crude_tool_value;
   private static int harpoonSellValue       { get; set; } = base_harpoon_value;
   private static int pressureValveSellValue { get; set; } = base_pressure_valve_value;
   private static int divingBellSellValue    { get; set; } = base_diving_bell_value;
   private static int precisionLensSellValue { get; set; } = base_precision_lens_value;
   private static int engineSellValue        { get; set; } = base_engine_value;
   private static int rawOrePrice            { get; set; } = 1;
   private static int mercenaryEngineerPrice { get; set; } = 100;
   private static int insurancePolicyPrice   { get; set; } = 100;
   private static int tier2BluePrintPrice    { get; set; } = 150;
   private static int tier3BluePrintPrice    { get; set; } = 350;

   public static int tierOneIncreaseFactor { get; private set; } = 2;

   public const int MIN_CRUDE_TOOL_VALUE           = 0;
   public const int MAX_CRUDE_TOOL_VALUE           = 150;
   public const int MIN_HARPOON_VALUE              = 0;
   public const int MAX_HARPOON_VALUE              = 300;
   public const int MIN_DIVING_BELL_VALUE          = 0;
   public const int MAX_DIVING_BELL_VALUE          = 120;
   public const int MIN_PRESSURE_VALVE_VALUE       = 0;
   public const int MAX_PRESSURE_VALVE_VALUE       = 800;
   public const int MIN_PRECISION_LENS_VALUE       = 0;
   public const int MAX_PRECISION_LENS_VALUE       = 3000;
   public const int MIN_ENGINE_VALUE               = 0;
   public const int MAX_ENGINE_VALUE               = 5000;

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
      "Exchanges pearls for ore at a 1:1 ratio (Shift + click to increase by 10).";
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
      "Pays a pearl payout if a market crash occurs within the next 5 turns. Active 1st 2 turns of every 5 turn cycle";

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

// ==========================================
   // CRUDE TOOL
   // ==========================================
   public static void TryIncreaseCrudeToolSellValue(int amount) 
   {
      if (crudeToolSellValue >= MAX_CRUDE_TOOL_VALUE) return;
      
      crudeToolSellValue = Mathf.Min(crudeToolSellValue + amount, MAX_CRUDE_TOOL_VALUE);
      OnItemValueChange?.Invoke(crudeToolSellValue, ItemType.CrudeTool);
   }

   public static void TryDecreaseCrudeToolSellValue(int amount) 
   {
      if (crudeToolSellValue <= MIN_CRUDE_TOOL_VALUE) return;
      
      crudeToolSellValue = Mathf.Max(crudeToolSellValue - amount, MIN_CRUDE_TOOL_VALUE);
      OnItemValueChange?.Invoke(crudeToolSellValue, ItemType.CrudeTool);
   }

   public static void TryIncreaseHarpoonSellValue(int amount) 
   {
      if (harpoonSellValue >= MAX_HARPOON_VALUE) return;
      
      // Use Mathf.Min to safely add the amount, capping it perfectly at the Maximum!
      harpoonSellValue = Mathf.Min(harpoonSellValue + amount, MAX_HARPOON_VALUE);
   
      OnItemValueChange?.Invoke(harpoonSellValue, ItemType.Harpoon);
   }

   public static void TryDecreaseHarpoonSellValue(int amount) 
   {
      if (harpoonSellValue <= MIN_HARPOON_VALUE) return;
   
      // Use Mathf.Max to safely subtract, stopping perfectly at the Minimum!
      harpoonSellValue = Mathf.Max(harpoonSellValue - amount, MIN_HARPOON_VALUE);
   
      OnItemValueChange?.Invoke(harpoonSellValue, ItemType.Harpoon);
   }

   // ==========================================
   // DIVING BELL
   // ==========================================
   public static void TryIncreaseDivingBellValue(int amount)
   {
      if (divingBellSellValue >= MAX_DIVING_BELL_VALUE) return;

      divingBellSellValue = Mathf.Min(divingBellSellValue + amount, MAX_DIVING_BELL_VALUE);
      OnItemValueChange?.Invoke(divingBellSellValue, ItemType.DivingBell);
   }

   public static void TryDecreaseDivingBellValue(int amount)
   {
      if (divingBellSellValue <= MIN_DIVING_BELL_VALUE) return;

      divingBellSellValue = Mathf.Max(divingBellSellValue - amount, MIN_DIVING_BELL_VALUE);
      OnItemValueChange?.Invoke(divingBellSellValue, ItemType.DivingBell);
   }

   // ==========================================
   // PRESSURE VALVE
   // ==========================================
   public static void TryIncreasePressureValveValue(int amount) 
   {
      if (pressureValveSellValue >= MAX_PRESSURE_VALVE_VALUE) return;
      
      pressureValveSellValue = Mathf.Min(pressureValveSellValue + amount, MAX_PRESSURE_VALVE_VALUE);
      OnItemValueChange?.Invoke(pressureValveSellValue, ItemType.PressureValve);
   }

   public static void TryDecreasePressureValveValue(int amount) 
   {
      if (pressureValveSellValue <= MIN_PRESSURE_VALVE_VALUE) return;

      pressureValveSellValue = Mathf.Max(pressureValveSellValue - amount, MIN_PRESSURE_VALVE_VALUE);
      OnItemValueChange?.Invoke(pressureValveSellValue, ItemType.PressureValve);
   }

   // ==========================================
   // PRECISION LENS
   // ==========================================
   public static void TryIncreasePrecisionLensValue(int amount)
   {
      if (precisionLensSellValue >= MAX_PRECISION_LENS_VALUE) return;

      precisionLensSellValue = Mathf.Min(precisionLensSellValue + amount, MAX_PRECISION_LENS_VALUE);
      OnItemValueChange?.Invoke(precisionLensSellValue, ItemType.PrecisionLens);
   }

   public static void TryDecreasePrecisionLensValue(int amount)
   {
      if (precisionLensSellValue <= MIN_PRECISION_LENS_VALUE) return;

      precisionLensSellValue = Mathf.Max(precisionLensSellValue - amount, MIN_PRECISION_LENS_VALUE);
      OnItemValueChange?.Invoke(precisionLensSellValue, ItemType.PrecisionLens);
   }

   // ==========================================
   // ENGINE
   // ==========================================
   public static void TryIncreaseEngineSellValue(int amount) 
   {
      if (engineSellValue >= MAX_ENGINE_VALUE) return;

      engineSellValue = Mathf.Min(engineSellValue + amount, MAX_ENGINE_VALUE);
      OnItemValueChange?.Invoke(engineSellValue, ItemType.Engine);
   }

   public static void TryDecreaseEngineSellValue(int amount) 
   {
      if (engineSellValue <= MIN_ENGINE_VALUE) return;

      engineSellValue = Mathf.Max(engineSellValue - amount, MIN_ENGINE_VALUE);
      OnItemValueChange?.Invoke(engineSellValue, ItemType.Engine);
   }

   public static void TryIncreaseTier2BlueprintPrice(int amount) 
   {
      tier2BluePrintPrice += amount;
      OnItemValueChange?.Invoke(tier2BluePrintPrice, ItemType.IndustrialBlueprint);
   }

   public static void TryDecreaseTier2BlueprintPrice(int amount) 
   {
      // Prevent the blueprint from dropping below 0 (or some minimum)
      tier2BluePrintPrice = Mathf.Max(tier2BluePrintPrice - amount, 0); 
      OnItemValueChange?.Invoke(tier2BluePrintPrice, ItemType.IndustrialBlueprint);
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

   public static void RevertItemToBaseSellValue(ItemType item) 
   {
      switch(item) 
      { 
         case ItemType.CrudeTool:
            crudeToolSellValue = base_crude_tool_value;
            OnItemValueChange?.Invoke(crudeToolSellValue, ItemType.CrudeTool);
            break;
         case ItemType.Harpoon:
            harpoonSellValue = base_harpoon_value;
            OnItemValueChange?.Invoke(harpoonSellValue, ItemType.Harpoon);
            break;
         case ItemType.DivingBell:
            divingBellSellValue = base_diving_bell_value;
            OnItemValueChange?.Invoke(divingBellSellValue, ItemType.DivingBell);
            break;

         case ItemType.PressureValve:
            pressureValveSellValue = base_pressure_valve_value;
            OnItemValueChange?.Invoke(pressureValveSellValue, ItemType.PressureValve);
            break;

         case ItemType.PrecisionLens:
            precisionLensSellValue = base_precision_lens_value;
            OnItemValueChange?.Invoke(precisionLensSellValue, ItemType.PrecisionLens);
            break;
         case ItemType.Engine:
            engineSellValue = base_engine_value;
            OnItemValueChange?.Invoke(engineSellValue, ItemType.Engine);
            break;
      }

      return;
   }

   // Instantly snaps all market values back to their base line and updates the UI
   public static void RevertMarketToBase()
   {
      crudeToolSellValue     = base_crude_tool_value;
      harpoonSellValue       = base_harpoon_value;
      pressureValveSellValue = base_pressure_valve_value;
      divingBellSellValue    = base_diving_bell_value;
      precisionLensSellValue = base_precision_lens_value;
      engineSellValue        = base_engine_value;

      OnItemValueChange?.Invoke(crudeToolSellValue, ItemType.CrudeTool);
      OnItemValueChange?.Invoke(harpoonSellValue, ItemType.Harpoon);
      OnItemValueChange?.Invoke(pressureValveSellValue, ItemType.PressureValve);
      OnItemValueChange?.Invoke(divingBellSellValue, ItemType.DivingBell);
      OnItemValueChange?.Invoke(precisionLensSellValue, ItemType.PrecisionLens);
      OnItemValueChange?.Invoke(engineSellValue, ItemType.Engine);
   }

   // Resets all static prices and events back to default for a new game
   public static void ResetItems() 
   {
      // 1. Reset Base Values FIRST
      base_diving_bell_value    = 25;
      base_crude_tool_value     = 40;
      base_harpoon_value        = 80;
      base_pressure_valve_value = 250;
      base_precision_lens_value = 800;
      base_engine_value         = 1200;
      
      // 2. THEN Reset Current Sell Values
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
      tier2BluePrintPrice    = 150;
      tier3BluePrintPrice    = 350;

      // Clear the static action delegate to prevent memory leaks/missing references
      OnItemValueChange = null;
   }
}
