using System;

using UnityEngine;

public class Item {

   public static int crudeToolSellValue    { get; private set; } = 15;
   public static int harpoonSellValue      { get; private set; } = 20;
   public static int pressureValveValue    { get; private set; } = 60;
   public static int engineSellValue       { get; private set; } = 200;
   public static int swordPrice            { get; private set; } = 5;
   public static int tierOneIncreaseFactor { get; private set; } = 2;

   private const int MIN_CRUDE_TOOL_VALUE = 10;
   private const int MAX_CRUDE_TOOL_VALUE = 25;
   private const int MIN_HARPOON_VALUE = 12;
   private const int MAX_HARPOON_VALUE = 40;
   private const int MIN_PRESSURE_VALVE_VALUE = 35;
   private const int MAX_PRESSURE_VALVE_VALUE = 100;
   private const int MIN_ENGINE_VALUE = 80;
   private const int MAX_ENGINE_VALUE = 450;

   const string CRUDE_TOOL_DESCRIPTION   = 
      "A basic tool made from rudimentary materials. " +
      "Useful for simple tasks but lacks durability.";
   const string HARPOON_DESCRIPTION =
      "A well-crafted tool made from high-quality materials. " +
      "Offers better performance and durability for various tasks.";
   const string PRESSURE_VALVE_DESCRIPTION =
      "A well-crafted tool made from high-quality materials. " +
      "Offers better performance and durability for various tasks.";
   const string ENGINE_DESCRIPTION     = 
      "An ancient artifact recovered from the depths. " +
      "Artifacts can be sold for a high price or used in special research.";

   public static Action<int, ItemType> OnItemValueChange;

   public enum ItemType {
        CrudeTool,
        Harpoon,
        PressureValve,
        Engine,
        Sword
    }

    public static int GetItemValue(ItemType itemType) {
        switch (itemType) {
            case ItemType.CrudeTool:
                return crudeToolSellValue;
            case ItemType.Harpoon:
                return harpoonSellValue;
            case ItemType.PressureValve:
               return pressureValveValue;
            case ItemType.Engine:
                return engineSellValue;
            case ItemType.Sword:
                return swordPrice;
            default:
                Debug.LogError("Unkown Item");
            return 0;
        }
    }

   public static int GetItemPrice(ItemType itemType) {
      switch (itemType) {
         case ItemType.Sword:
            return swordPrice;
         default:
            return 0;
      }
   }

   public static string GetItemDescription(ItemType itemType) {
      switch (itemType) {
         case ItemType.CrudeTool:
            return CRUDE_TOOL_DESCRIPTION;
         case ItemType.Harpoon:
            return HARPOON_DESCRIPTION;
         case ItemType.Engine:
            return ENGINE_DESCRIPTION;
         default:
            return "No description available.";
      }
   }
    public static Sprite GetItemSprite(ItemType itemType) {
        if (ItemSprites.itemSprites == null) {
            Debug.LogError("ItemSprites.Instance is NULL! Cannot retrieve sprites.");
            return null;
        }

        return ItemSprites.itemSprites.GetSprite(itemType);
    }

   public static void TryIncreaseCrudeToolSellValue(int amount) {
      // 1. Check if adding the amount would exceed the MAX_VALUE
      if (crudeToolSellValue >= MAX_CRUDE_TOOL_VALUE) {
         Debug.LogError("Crude Tool Sell Value is already at maximum!");
      }

      // 2. Check if the *new* value would exceed the maximum.
      // We use Math.Max to see what the new value will be if clamped, and compare it.
      if (crudeToolSellValue + amount > MAX_CRUDE_TOOL_VALUE) {
         // Log the error if the amount is too large
         Debug.LogError($"Cannot increase by {amount}. Max value is {MAX_CRUDE_TOOL_VALUE}.");
      }

      // 3. If checks pass, perform the increase. The setter enforces the clamp just in case.
      crudeToolSellValue += amount;

      OnItemValueChange?.Invoke(crudeToolSellValue, ItemType.CrudeTool);

      return;
   }

   public static void TryDecreaseCrudeToolSellValue(int amount) {
      // 1. Check if the value is already at the MIN_VALUE
      if (crudeToolSellValue <= MIN_CRUDE_TOOL_VALUE) {
         Debug.LogError("Crude Tool Sell Value is already at minimum!");
      }

      // 2. Check if subtracting the amount would drop below the minimum.
      if (crudeToolSellValue - amount < MIN_CRUDE_TOOL_VALUE) {
         // Log the error if the amount is too large
         Debug.LogError($"Cannot decrease by {amount}. Min value is {MIN_CRUDE_TOOL_VALUE}.");
      }

      // 3. If checks pass, perform the decrease. The setter enforces the clamp just in case.
      crudeToolSellValue -= amount;
      OnItemValueChange?.Invoke(crudeToolSellValue, ItemType.CrudeTool);

      return;
   }

   public static void TryIncreasePressureValveValue(int amount) {
      // 1. Check if adding the amount would exceed the MAX_VALUE
      if (pressureValveValue >= MAX_PRESSURE_VALVE_VALUE) {
         Debug.LogError("Crude Tool Sell Value is already at maximum!");
      }

      // 2. Check if the *new* value would exceed the maximum.
      // We use Math.Max to see what the new value will be if clamped, and compare it.
      if (pressureValveValue + amount > MAX_PRESSURE_VALVE_VALUE) {
         // Log the error if the amount is too large
         Debug.LogError($"Cannot increase by {amount}. Max value is {MAX_PRESSURE_VALVE_VALUE}.");
      }

      // 3. If checks pass, perform the increase. The setter enforces the clamp just in case.
      pressureValveValue += amount;

      OnItemValueChange?.Invoke(pressureValveValue, ItemType.PressureValve);

      return;
   }

   public static void TryDecreasePressureValveValue(int amount) {
      // 1. Check if the value is already at the MIN_VALUE
      if (pressureValveValue <= MIN_PRESSURE_VALVE_VALUE) {
         Debug.LogError("Crude Tool Sell Value is already at minimum!");
      }

      // 2. Check if subtracting the amount would drop below the minimum.
      if (pressureValveValue - amount < MIN_PRESSURE_VALVE_VALUE) {
         // Log the error if the amount is too large
         Debug.LogError($"Cannot decrease by {amount}. Min value is {MIN_PRESSURE_VALVE_VALUE}.");

      }

      // 3. If checks pass, perform the decrease. The setter enforces the clamp just in case.
      pressureValveValue -= amount;

      OnItemValueChange?.Invoke(pressureValveValue, ItemType.PressureValve);

      return;
   }

   public static void TryIncreaseWeaponsSellValue(int amount) {
      // 1. Check if adding the amount would exceed the MAX_VALUE
      if (harpoonSellValue >= MAX_HARPOON_VALUE) {
         Debug.LogError("Crude Tool Sell Value is already at maximum!");
      }

      // 2. Check if the *new* value would exceed the maximum.
      // We use Math.Max to see what the new value will be if clamped, and compare it.
      if (harpoonSellValue + amount > MAX_HARPOON_VALUE) {
         // Log the error if the amount is too large
         Debug.LogError($"Cannot increase by {amount}. Max value is {MAX_CRUDE_TOOL_VALUE}.");
      }

      // 3. If checks pass, perform the increase. The setter enforces the clamp just in case.
      harpoonSellValue += amount;

      OnItemValueChange?.Invoke(harpoonSellValue, ItemType.Harpoon);

      return;
   }

   public static void TryDecreaseWeaponsSellValue(int amount) {
      // 1. Check if the value is already at the MIN_VALUE
      if (harpoonSellValue <= MIN_HARPOON_VALUE) {
         Debug.LogError("Crude Tool Sell Value is already at minimum!");
         return;
      }

      // 2. Check if subtracting the amount would drop below the minimum.
      if (harpoonSellValue - amount < MIN_HARPOON_VALUE) {
         // Log the error if the amount is too large
         Debug.LogError($"Cannot decrease by {amount}. Min value is {MIN_HARPOON_VALUE}.");
         return;
         // OPTION: Set value to min instead of rejecting
         // crudeToolSellValue = MIN_VALUE; 
         // return true;

      }

      // 3. If checks pass, perform the decrease. The setter enforces the clamp just in case.
      harpoonSellValue -= amount;

      OnItemValueChange?.Invoke(harpoonSellValue, ItemType.Harpoon);

      return;
   }
   public static void TryIncreaseEnginesSellValue(int amount) {
      // 1. Check if adding the amount would exceed the MAX_VALUE
      if (engineSellValue >= MAX_ENGINE_VALUE) {
         Debug.LogError("Crude Tool Sell Value is already at maximum!");
      }

      // 2. Check if the *new* value would exceed the maximum.
      // We use Math.Max to see what the new value will be if clamped, and compare it.
      if (engineSellValue + amount > MAX_ENGINE_VALUE) {
         // Log the error if the amount is too large
         Debug.LogError($"Cannot increase by {amount}. Max value is {MAX_ENGINE_VALUE}.");
      }

      // 3. If checks pass, perform the increase. The setter enforces the clamp just in case.
      engineSellValue += amount;

      OnItemValueChange?.Invoke(engineSellValue, ItemType.Engine);
      return;
   }

   public static void TryDecreaseEnginesSellValue(int amount) {
      // 1. Check if the value is already at the MIN_VALUE
      if (engineSellValue <= MIN_ENGINE_VALUE)
         Debug.LogError("Crude Tool Sell Value is already at minimum!");

      // 2. Check if subtracting the amount would drop below the minimum.
      if (engineSellValue - amount < MIN_ENGINE_VALUE)
         Debug.LogError($"Cannot decrease by {amount}. Min value is {MIN_ENGINE_VALUE}.");

      // 3. If checks pass, perform the decrease. The setter enforces the clamp just in case.
      engineSellValue -= amount;

      OnItemValueChange?.Invoke(engineSellValue, ItemType.Engine);

      return;
   }
}
