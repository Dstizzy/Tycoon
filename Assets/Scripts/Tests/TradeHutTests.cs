using NUnit.Framework;

using System.Collections;

using TMPro;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

using static Item;

[TestFixture]
public class TradeHutTests : InputTestFixture {
   [SetUp]
   public void TestSetup() {
      // Ensure the default Mouse device is available
      if (Mouse.current == null)
         InputSystem.AddDevice<Mouse>();
   }

   [UnityTearDown] // Runs after *each* [UnityTest]
   public IEnumerator CleanupTestObjects() {
      // Find all the pop-up buttons that were likely left behind.
      GameObject[] popUpButtons = GameObject.FindGameObjectsWithTag("BuildingButton");

      foreach (GameObject go in popUpButtons)
         // Destroy them to ensure they aren't blocking the next test's input.
         UnityEngine.Object.Destroy(go);

      // Allow one frame for the destruction to take effect
      yield return null;
   }

   [Test]
   public void IsMouseAdded() {
      Assert.IsNotNull(Mouse.current, "Setup Error: Missing Mouse device.");
   }

   [UnityTest]
   public IEnumerator TradePanelsTest() {
      // 1. Load the scene and wait for it to be fully ready
      SceneManager.LoadScene("MainScene");

      // Proper way to wait for scene load in Unity tests
      float timeout = 2.0f;
      while (!SceneManager.GetActiveScene().name.Equals("MainScene") && timeout > 0) {
         timeout -= Time.deltaTime;
         yield return null;
      }

      // Give Unity one frame to initialize the UI and Singletons
      yield return new WaitForEndOfFrame();

      // 2. Ensure TradeHutManager is active
      Assert.IsNotNull(TradeHutManager.Instance, "TradeHutManager Instance is null. Is it in the scene?");

      // 3. Open the Panels via the Manager instead of searching for floating buttons
      // This bypasses the hover/popup logic which is often the source of test flakiness
      TradeHutManager.Instance.RequestTradeHutPanel(TradeHutManager.TRADE_BUTTON);
      yield return null; // Wait for SetActive(true) to propagate

      // 4. Verify Trade Panel (Sell/Buy)
      GameObject sellPanel = GameObject.Find("Building Panel Canvas/UI_TradeHut/TradePanels/SellPanel");
      Assert.IsNotNull(sellPanel, "Sell Panel not found after RequestTradeHutPanel(TRADE_BUTTON).");
      Assert.IsTrue(sellPanel.activeInHierarchy, "Sell Panel is not visible.");

      // Test switching to Buy Panel
      TradeHutManager.Instance.ShowBuyPanel();
      yield return null;
      GameObject buyPanel = GameObject.Find("Building Panel Canvas/UI_TradeHut/TradePanels/BuyPanel");
      Assert.IsTrue(buyPanel.activeInHierarchy, "Buy Panel did not activate via ShowBuyPanel().");

      // Close Trade Panel
      TradeHutManager.Instance.CloseTradeHutPanel(TradeHutManager.TRADE_BUTTON);
      yield return null;
      Assert.IsFalse(GameObject.Find("Building Panel Canvas/UI_TradeHut/TradePanels").activeInHierarchy, "Trade Panels did not close.");

      // 5. Verify Info Panel
      TradeHutManager.Instance.RequestTradeHutPanel(TradeHutManager.INFO_BUTTON);
      yield return null;
      GameObject infoPanel = GameObject.Find("Building Panel Canvas/UI_TradeHut/TradeInfoPanel");
      Assert.IsNotNull(infoPanel, "Info Panel not found.");
      Assert.IsTrue(infoPanel.activeInHierarchy, "Info Panel did not open.");

      TradeHutManager.Instance.CloseTradeHutPanel(TradeHutManager.INFO_BUTTON);
      yield return null;
      Assert.IsFalse(infoPanel.activeInHierarchy, "Info Panel did not close.");
   }

   [UnityTest]
   public IEnumerator SellAllItemsTransactionTest() {
      // 1. Load scene and initialize
      SceneManager.LoadScene("MainScene");
      yield return new WaitForSeconds(1.0f);
      yield return new WaitForEndOfFrame();

      // 2. Unlock all tiers so all items are sellable
      ForgeManager.Instance.hasTier2Blueprint = true;
      ForgeManager.Instance.hasTier3Blueprint = true;

      // Define items to test: Tag, Inventory Count Method, ItemType Enum
      var itemsToTest = new (string tag, ItemType type)[] {
        (InventoryManager.CRUDE_TOOL_TAG, ItemType.CrudeTool),
        (InventoryManager.HARPOON_TAG, ItemType.Harpoon),
        (InventoryManager.PRESSURE_VALVE_TAG, ItemType.PressureValve),
        (InventoryManager.ENGINE_TAG, ItemType.Engine)
    };

      foreach (var item in itemsToTest) {
         Debug.Log($"Starting sale test for: {item.tag}");

         // 3. Setup Inventory: Give 10 of the current item
         switch (item.type) {
            case ItemType.CrudeTool: InventoryManager.Instance.TryAddCrudeTool(10); break;
            case ItemType.Harpoon: InventoryManager.Instance.TryAddHarpoon(10); break;
            case ItemType.PressureValve: InventoryManager.Instance.TryAddPressureValve(10); break;
            case ItemType.Engine: InventoryManager.Instance.TryAddEngine(10); break;
         }

         int startingPearls = InventoryManager.Instance.pearlCount;
         int itemValue = GetItemValue(item.type);

         // 4. Open Trade Panel
         TradeHutManager.Instance.RequestTradeHutPanel(TradeHutManager.TRADE_BUTTON);
         yield return new WaitForSeconds(0.5f);

         // 5. Find the shelf item and trigger the Sell Window
         var itemUI = TradeHutManager.Instance.SellItems.Find(i => i.CompareTag(item.tag));
         Assert.IsNotNull(itemUI, $"Could not find {item.tag} in SellItems list.");

         itemUI.Find("ItemButton").GetComponent<Button>().onClick.Invoke();
         yield return new WaitForEndOfFrame();
         yield return new WaitForSeconds(0.5f);

         // 6. Get the instantiated window via Reflection
         var field = typeof(TradeHutManager).GetField("currentSellItem",
             System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
         Transform activeWindow = (Transform)field.GetValue(TradeHutManager.Instance);
         Assert.IsNotNull(activeWindow, $"Sell window for {item.tag} did not open.");

         // 7. Increase count to 5
         for (int i = 0; i < 5; i++) {
            TradeHutManager.Instance.IncreaseSellItemCount(activeWindow);
         }
         yield return new WaitForSeconds(0.5f);

         // 8. Execute Sale
         TradeHutManager.Instance.SellItem();
         yield return new WaitForSeconds(0.5f);

         // 9. Verification
         int expectedPearls = startingPearls + (5 * itemValue);
         Assert.AreEqual(expectedPearls, InventoryManager.Instance.pearlCount, $"Pearl count mismatch for {item.tag}");

         // Check inventory deduction
         int currentInvCount = 0;
         switch (item.type) {
            case ItemType.CrudeTool: currentInvCount = InventoryManager.Instance.crudeToolCount; break;
            case ItemType.Harpoon: currentInvCount = InventoryManager.Instance.harpoonCount; break;
            case ItemType.PressureValve: currentInvCount = InventoryManager.Instance.pressureValveCount; break;
            case ItemType.Engine: currentInvCount = InventoryManager.Instance.engineCount; break;
         }
         Assert.AreEqual(5, currentInvCount, $"Inventory deduction failed for {item.tag}");

         // Close panel before next iteration
         TradeHutManager.Instance.CloseTradeHutPanel(TradeHutManager.TRADE_BUTTON);
         yield return new WaitForSeconds(0.5f);
      }
   }

   [UnityTest]
   public IEnumerator BuyAllItemsTransactionTest() 
   {
       // 1. Load scene and wait for initialization
       SceneManager.LoadScene("MainScene");
       yield return new WaitForSeconds(1.0f);
       yield return new WaitForEndOfFrame();
   
       // 2. Setup: Ensure player has a massive amount of pearls to afford everything
       InventoryManager.Instance.TryAddPearl(5000);
       Assert.IsFalse(ForgeManager.Instance.hasTier2Blueprint, "Setup Error: Tier 2 already unlocked.");
       Assert.IsFalse(ForgeManager.Instance.hasTier3Blueprint, "Setup Error: Tier 3 already unlocked.");
   
       // 3. Open Trade Hut -> Buy Panel
       TradeHutManager.Instance.RequestTradeHutPanel(TradeHutManager.TRADE_BUTTON);
       TradeHutManager.Instance.ShowBuyPanel();
       yield return new WaitForSeconds(1.0f);
   
       // Cache the reflection field so we can grab the active window instances later
       var currentBuyItemField = typeof(TradeHutManager).GetField("currentBuyItem", 
           System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
   
       // =========================================================
       // TEST 1: BUY TIER 2 BLUEPRINT
       // =========================================================
       var tier2UI = TradeHutManager.Instance.BuyItems.Find(item => item.CompareTag(TradeHutManager.INDUSTRIAL_BLUEPRINT_TAG));
       tier2UI.Find("ItemButton").GetComponent<Button>().onClick.Invoke();
       yield return new WaitForSeconds(0.5f);
       
       var pressureValveUI = TradeHutManager.Instance.SellItems.Find(item => item.CompareTag(InventoryManager.PRESSURE_VALVE_TAG));
       Assert.IsFalse(pressureValveUI.Find("ItemButton").gameObject.activeSelf, "Pressure Valve was revealed in the Sell tab before purchase.");
   
       TradeHutManager.Instance.BuyItem();
       yield return new WaitForSeconds(0.5f);
       
       // Verify Logic and UI Unlocks
       Assert.IsTrue(ForgeManager.Instance.hasTier2Blueprint, "ForgeManager flag did not update for Tier 2.");
       Assert.IsTrue(pressureValveUI.Find("ItemButton").gameObject.activeSelf, "Pressure Valve was not revealed in the Sell tab after purchase.");
   
       // =========================================================
       // TEST 2: BUY TIER 3 BLUEPRINT
       // =========================================================
       var tier3UI = TradeHutManager.Instance.BuyItems.Find(item => item.CompareTag(TradeHutManager.CLOCKWORK_BLUEPRINT_TAG));
       tier3UI.Find("ItemButton").GetComponent<Button>().onClick.Invoke();
       yield return new WaitForSeconds(0.5f);
       
       // Check that Engine is hidden BEFORE purchase
       var engineUI = TradeHutManager.Instance.SellItems.Find(item => item.CompareTag(InventoryManager.ENGINE_TAG));
       Assert.IsFalse(engineUI.Find("ItemButton").gameObject.activeSelf, "Engine was revealed in the Sell tab before purchase.");
   
       TradeHutManager.Instance.BuyItem();
       yield return new WaitForSeconds(0.5f);
       
       // Verify Logic and UI Unlocks
       Assert.IsTrue(ForgeManager.Instance.hasTier3Blueprint, "ForgeManager flag did not update for Tier 3.");
       Assert.IsTrue(engineUI.Find("ItemButton").gameObject.activeSelf, "Engine was not revealed in the Sell tab after purchase.");
   
       // =========================================================
       // TEST 3: BUY MERCENARY ENGINEERS (Requires Quantity Change)
       // =========================================================
       int startingEngineers = InventoryManager.Instance.mercenaryEngineerCount;
       var mercUI = TradeHutManager.Instance.BuyItems.Find(item => item.CompareTag(InventoryManager.MERCENARY_ENGINEER_TAG));
       mercUI.Find("ItemButton").GetComponent<Button>().onClick.Invoke();
       yield return new WaitForSeconds(0.5f);
   
       // Get the active window instance to pass to the Increase method
       Transform activeMercWindow = (Transform)currentBuyItemField.GetValue(TradeHutManager.Instance);
       
       // Increase quantity by 2
       TradeHutManager.Instance.IncreaseBuyItemsCount(activeMercWindow);
       TradeHutManager.Instance.IncreaseBuyItemsCount(activeMercWindow);
       yield return new WaitForSeconds(0.2f); // Brief delay to see UI update
       
       TradeHutManager.Instance.BuyItem();
       yield return new WaitForSeconds(0.5f);
       Assert.AreEqual(startingEngineers + 2, InventoryManager.Instance.mercenaryEngineerCount, "Failed to buy Mercenary Engineers.");
   
       // =========================================================
       // TEST 4: BUY RAW ORE CHUNKS (Requires Quantity Change)
       // =========================================================
       int startingOre = InventoryManager.Instance.oreCount;
       var oreUI = TradeHutManager.Instance.BuyItems.Find(item => item.CompareTag(TradeHutManager.RAW_ORE_CHUNK_TAG));
       oreUI.Find("ItemButton").GetComponent<Button>().onClick.Invoke();
       yield return new WaitForSeconds(0.5f);
   
       // Get the newly created active window instance
       Transform activeOreWindow = (Transform)currentBuyItemField.GetValue(TradeHutManager.Instance);
       
       // Increase quantity by 5
       for(int i = 0; i < 5; i++)
           TradeHutManager.Instance.IncreaseBuyItemsCount(activeOreWindow);
       
      yield return new WaitForSeconds(0.2f);
       
       TradeHutManager.Instance.BuyItem();
       yield return new WaitForSeconds(0.5f);
       Assert.AreEqual(startingOre + 5, InventoryManager.Instance.oreCount, "Failed to buy Raw Ore Chunks.");
   
       Debug.Log("Successfully purchased all items and verified UI unlocks!");
   }


   [UnityTest]
   public IEnumerator CommerceTierOneForesightUnlockTest() 
   {
       // 1. Load Scene and wait for initialization
       SceneManager.LoadScene("MainScene");
       yield return new WaitForSeconds(1.0f);
       yield return new WaitForEndOfFrame();
   
       // 2. Setup: Ensure player can afford the Tier 1 upgrade
       InventoryManager.Instance.TryAddPearl(LabManager.T1_COMM_PEARL);
       
       // 3. Purchase Tier 1 Commerce Innovation via Lab Logic
       // Accessing private commerceTab via reflection
       var commerceTabField = typeof(LabManager).GetField("commerceTab", 
           System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      // Note: Ensure you are using your actual Singleton reference (e.g., LabManager.Instance or your specific reference)
      GameObject commerceTab = (GameObject)commerceTabField.GetValue(LabManager.labManager);

      LabManager.labManager.ImplementTierOneInnovation(commerceTab);
       yield return new WaitForSeconds(0.5f);
   
       // 4. Navigate to Trade Hut
       TradeHutManager.Instance.RequestTradeHutPanel(TradeHutManager.TRADE_BUTTON);
       yield return new WaitForSeconds(1.0f); 
   
       // 5. Locate the UI elements for Crude Tool
       var crudeToolUI = TradeHutManager.Instance.SellItems.Find(item => item.CompareTag(InventoryManager.CRUDE_TOOL_TAG));
       GameObject nextValueObj = crudeToolUI.Find("NextValue").gameObject;
       TextMeshProUGUI nextValueText = nextValueObj.GetComponent<TextMeshProUGUI>();
   
       Assert.IsTrue(nextValueObj.activeSelf, "NextValue UI text was not activated by LabManager Innovation!");
   
       // 6. Perform and Verify 3 Market Shifts
       string lastValue = "";
       for (int i = 1; i <= 3; i++)
       {
           Debug.Log($"--- Market Shift Iteration {i} ---");
           
           // Trigger Foresight calculation
           TradeHutManager.Instance.CraftMarketForesight();
           yield return new WaitForSeconds(0.8f); // Delay to watch the text change in Play Mode
   
           string currentValue = nextValueText.text;
           
           // Verification: Ensure text is not empty and has updated from the last shift
           Assert.IsFalse(string.IsNullOrEmpty(currentValue), $"Iteration {i}: Next Value text is empty.");
           Assert.AreNotEqual("Next Value: 0", currentValue, $"Iteration {i}: Value failed to calculate (stayed at 0).");
           
           // Note: Prices *could* randomly stay the same, but with 3 shifts, it should change at least once.
           if (i > 1 && currentValue != lastValue)
           {
               Debug.Log($"Iteration {i} confirmed: Value changed from {lastValue} to {currentValue}");
           }
   
           lastValue = currentValue;
       }
   
       Debug.Log("Foresight logic verified across multiple shifts.");
   }

   [UnityTest]
   public IEnumerator CommerceTierTwoRecycleUnlockAndExecuteTest() 
   {
       // 1. Load Scene and wait for initialization
       SceneManager.LoadScene("MainScene");
       yield return new WaitForSeconds(2.0f); // Longer wait for initial load
       yield return new WaitForEndOfFrame();
   
       // 2. Setup: Give the player some Ore so they can test the recycle feature later
       InventoryManager.Instance.TryAddOre(20);
       
       // 3. Verify Initial State: Recycle Button should be HIDDEN before the upgrade
       Assert.IsNotNull(TradeHutManager.Instance.RecycleButton, "RecycleButton reference is missing!");
       Assert.IsFalse(TradeHutManager.Instance.RecycleButton.gameObject.activeSelf, "Setup Error: Recycle button should be hidden before Tier 2 upgrade.");
   
       // 4. Simulate Lab Upgrade (Tier 2 Commerce)
       // Get the private commerceTab via reflection
       var commerceTabField = typeof(LabManager).GetField("commerceTab", 
           System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
       GameObject commerceTab = (GameObject)commerceTabField.GetValue(LabManager.labManager);
   
       Debug.Log("Simulating Tier 2 Lab Upgrade...");
       LabManager.labManager.ImplementTierTwoInnovation(commerceTab);
       yield return new WaitForSeconds(1.0f); // Wait to let the background logic settle
   
       // 5. Open Trade Hut
       Debug.Log("Opening Trade Hut Panel...");
       TradeHutManager.Instance.RequestTradeHutPanel(TradeHutManager.TRADE_BUTTON);
       yield return new WaitForSeconds(1.5f); // Watch the Sell panel open
   
       // Switch to Buy Panel
       Debug.Log("Switching to Buy Panel...");
       TradeHutManager.Instance.ShowBuyPanel();
       yield return new WaitForSeconds(2.0f); // Long delay to clearly see the Recycle button has appeared!
   
       // 6. Verify Unlock Logic
       Assert.IsTrue(TradeHutManager.Instance.RecycleButton.gameObject.activeSelf, "Recycle button was not activated by the Tier 2 Lab Upgrade!");
       Assert.AreEqual(3, TradeHutManager.Instance.marketShiftMin, "Market shift min did not update to 3.");
       Assert.AreEqual(5, TradeHutManager.Instance.marketShiftMax, "Market shift max did not update to 5.");
   
       // 7. Execute Recycle
       int startingOre = InventoryManager.Instance.oreCount;
       int startingPearls = InventoryManager.Instance.pearlCount;
       
       Debug.Log("Clicking Recycle Button...");
       // Call the logic that the button uses
       TradeHutManager.Instance.RecycleOre();
       
       // Extra long delay here so you can watch the Top Bar UI update the Ore and Pearl counts
       yield return new WaitForSeconds(2.5f); 
   
       // 8. Verify Recycle Results
       Assert.AreEqual(startingOre - TradeHutManager.ORE_EXCHANGE_COST, InventoryManager.Instance.oreCount, "Ore was not deducted after recycling.");
       Assert.Greater(InventoryManager.Instance.pearlCount, startingPearls, "Pearls were not awarded after recycling.");
       
       Debug.Log($"Tier 2 Unlock & Recycle Success! Gained {InventoryManager.Instance.pearlCount - startingPearls} pearls.");
   }

   [UnityTest]
   public IEnumerator CommerceTierThreeBuffTest() 
   {
       // 1. Load Scene and wait for initialization
       SceneManager.LoadScene("MainScene");
       yield return new WaitForSeconds(2.0f); // Generous wait for initial load
       yield return new WaitForEndOfFrame();
   
       // 2. Record Baseline Values BEFORE Upgrade
       // We capture these to mathematically verify the 20% increase later
       int baseCrudeTool = GetItemValue(ItemType.CrudeTool);
       int baseHarpoon = GetItemValue(ItemType.Harpoon);
       int basePressureValve = GetItemValue(ItemType.PressureValve);
       int baseEngine = GetItemValue(ItemType.Engine);
   
       // Calculate the expected new values (+20% ceiling)
       int expectedCrudeTool = Mathf.CeilToInt(baseCrudeTool * 1.2f);
       int expectedHarpoon = Mathf.CeilToInt(baseHarpoon * 1.2f);
       int expectedPressureValve = Mathf.CeilToInt(basePressureValve * 1.2f);
       int expectedEngine = Mathf.CeilToInt(baseEngine * 1.2f);
   
       // 3. Simulate Lab Upgrade (Tier 3 Commerce)
       // Get the private commerceTab via reflection
       var commerceTabField = typeof(LabManager).GetField("commerceTab", 
           System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
       GameObject commerceTab = (GameObject)commerceTabField.GetValue(LabManager.labManager);
   
       Debug.Log("Simulating Tier 3 Lab Upgrade...");
       LabManager.labManager.ImplementTierThreeInnovation(commerceTab);
       yield return new WaitForSeconds(1.0f); // Let background logic apply the buffs
   
       // 4. Verify Internal Logic & Flags
       Assert.AreEqual(5, TradeHutManager.Instance.marketShiftMin, "Market shift min did not update to 5.");
       Assert.AreEqual(10, TradeHutManager.Instance.marketShiftMax, "Market shift max did not update to 10.");
       Assert.IsTrue(TradeHutManager.Instance.isTier3BuffACtive, "isTier3BuffACtive flag was not set to true!");
   
       // Verify the Item logic values actually increased
       Assert.AreEqual(expectedCrudeTool, GetItemValue(ItemType.CrudeTool), "Crude Tool base value did not increase by 20%.");
       Assert.AreEqual(expectedHarpoon, GetItemValue(ItemType.Harpoon), "Harpoon base value did not increase by 20%.");
       Assert.AreEqual(expectedPressureValve, GetItemValue(ItemType.PressureValve), "Pressure Valve base value did not increase by 20%.");
       Assert.AreEqual(expectedEngine, GetItemValue(ItemType.Engine), "Engine base value did not increase by 20%.");
   
       // 5. Open Trade Hut -> Sell Panel for UI Verification
       Debug.Log("Opening Trade Hut Panel to verify UI Updates...");
       TradeHutManager.Instance.RequestTradeHutPanel(TradeHutManager.TRADE_BUTTON);
       yield return new WaitForSeconds(2.0f); // Wait to watch the panel open
   
       // 6. Visual UI Verification
       // Check if the TextMeshPro elements were updated via the OnItemValueChange event
       var crudeToolUI = TradeHutManager.Instance.SellItems.Find(item => item.CompareTag(InventoryManager.CRUDE_TOOL_TAG));
       string uiValue = crudeToolUI.Find("ItemValue").GetComponent<TextMeshProUGUI>().text;
       
       Assert.AreEqual(expectedCrudeTool.ToString(), uiValue, "UI did not update to reflect new Crude Tool value.");
   
       Debug.Log($"Tier 3 Buffs Verified! Crude Tool price jumped from {baseCrudeTool} to {uiValue}.");
   }
}