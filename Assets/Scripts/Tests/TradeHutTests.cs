using NUnit.Framework;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using static Item;
using static WorldEvents;

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
   public IEnumerator TradePanelsTest() 
   {
      // 1. Load the scene and wait for it to be fully ready
      SceneManager.LoadScene("MainScene");

      float timeout = 2.0f;
      while (!SceneManager.GetActiveScene().name.Equals("MainScene") && timeout > 0) {
         timeout -= Time.deltaTime;
         yield return null;
      }

      yield return new WaitForEndOfFrame();

      Assert.IsNotNull(TradeHutManager.Instance, "TradeHutManager Instance is null. Is it in the scene?");

      // 2. Open the Trade Panel (Simulating the dynamic PopUp button click)
      TradeHutManager.Instance.RequestTradeHutPanel(TradeHutManager.TRADE_BUTTON);
      yield return new WaitForSeconds(2.0f); 

      // Retrieve the private TradePanels transform via reflection
      var tradePanelsField = typeof(TradeHutManager).GetField("TradePanels", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      Transform tradePanelsTransform = (Transform)tradePanelsField.GetValue(TradeHutManager.Instance);

      // 3. Verify Sell Panel is active
      Transform sellPanel = tradePanelsTransform.Find("SellPanel");
      Assert.IsNotNull(sellPanel, "Sell Panel not found within TradePanels.");
      Assert.IsTrue(sellPanel.gameObject.activeSelf, "Sell Panel is not visible.");

      // Test switching to Buy Panel
      TradeHutManager.Instance.ShowBuyPanel();
      yield return new WaitForSeconds(2.0f);
      
      Transform buyPanel = tradePanelsTransform.Find("BuyPanel");
      Assert.IsTrue(buyPanel.gameObject.activeSelf, "Buy Panel did not activate via ShowBuyPanel().");

      // 4. Close Trade Panel using the ACTUAL Exit Button
      Button tradeExitBtn = tradePanelsTransform.Find("ExitButton").GetComponent<Button>();
      Assert.IsNotNull(tradeExitBtn, "Could not find ExitButton on TradePanels.");
      
      tradeExitBtn.onClick.Invoke(); // Physically click the UI button
      yield return new WaitForSeconds(2.0f);
      
      Assert.IsFalse(tradePanelsTransform.gameObject.activeInHierarchy, "Trade Panels did not close after clicking Exit Button.");

      // 5. Open Info Panel
      TradeHutManager.Instance.RequestTradeHutPanel(TradeHutManager.INFO_BUTTON);
      yield return new WaitForSeconds(2.0f);
      
      // Retrieve the private InfoPanel transform via reflection
      var infoPanelField = typeof(TradeHutManager).GetField("InfoPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      Transform infoPanelTransform = (Transform)infoPanelField.GetValue(TradeHutManager.Instance);
      
      Assert.IsNotNull(infoPanelTransform, "Info Panel not found.");
      Assert.IsTrue(infoPanelTransform.gameObject.activeInHierarchy, "Info Panel did not open.");

      // 6. Close Info Panel using the ACTUAL Exit Button
      Button infoExitBtn = infoPanelTransform.Find("ExitButton").GetComponent<Button>();
      Assert.IsNotNull(infoExitBtn, "Could not find ExitButton on InfoPanel.");
      
      infoExitBtn.onClick.Invoke(); // Physically click the UI button
      yield return new WaitForSeconds(2.0f);
      
      Assert.IsFalse(infoPanelTransform.gameObject.activeInHierarchy, "Info Panel did not close after clicking Exit Button.");
   }

  [UnityTest]
   public IEnumerator SellAllItemsTransactionTest() 
   {
      SceneManager.LoadScene("MainScene");
      yield return new WaitForSeconds(1.0f);
      yield return new WaitForEndOfFrame();

      // Setup: Give player enough pearls to buy the blueprints
      InventoryManager.Instance.TryAddPearl(800);

      // Open the Trade Hut to the Buy Panel
      TradeHutManager.Instance.RequestTradeHutPanel(TradeHutManager.TRADE_BUTTON);
      TradeHutManager.Instance.ShowBuyPanel();
      yield return new WaitForSeconds(2.0f); // Opening Panel

      // Buy Tier 2 Blueprint to unlock Pressure Valve & Diving Bell UI
      var tier2UI = TradeHutManager.Instance.BuyItems.Find(item => item.CompareTag(TradeHutManager.INDUSTRIAL_BLUEPRINT_TAG));
      tier2UI.Find("ItemButton").GetComponent<Button>().onClick.Invoke();
      yield return new WaitForSeconds(2.0f);
      TradeHutManager.Instance.BuyItem();
      yield return new WaitForSeconds(2.0f);

      // Buy Tier 3 Blueprint to unlock Engine & Precision Lens UI
      var tier3UI = TradeHutManager.Instance.BuyItems.Find(item => item.CompareTag(TradeHutManager.CLOCKWORK_BLUEPRINT_TAG));
      tier3UI.Find("ItemButton").GetComponent<Button>().onClick.Invoke();
      yield return new WaitForSeconds(2.0f);
      TradeHutManager.Instance.BuyItem();
      yield return new WaitForSeconds(2.0f);

      // Switch to Sell Panel to begin selling tests
      TradeHutManager.Instance.ShowSellPanel();
      yield return new WaitForSeconds(2.0f); // Switching Panels

      var itemsToTest = new (string tag, ItemType type)[] {
        (InventoryManager.CRUDE_TOOL_TAG, ItemType.CrudeTool),
        (InventoryManager.HARPOON_TAG, ItemType.Harpoon),
        (InventoryManager.DIVING_BELL_TAG, ItemType.DivingBell),
        (InventoryManager.PRESSURE_VALVE_TAG, ItemType.PressureValve),
        (InventoryManager.PRECISION_LENS_TAG, ItemType.PrecisionLens),
        (InventoryManager.ENGINE_TAG, ItemType.Engine)
      };

      foreach (var item in itemsToTest) {
         // Give the player 10 of the item to sell
         switch (item.type) {
            case ItemType.CrudeTool: InventoryManager.Instance.TryAddCrudeTool(10); break;
            case ItemType.Harpoon: InventoryManager.Instance.TryAddHarpoon(10); break;
            case ItemType.DivingBell: InventoryManager.Instance.TryAddDivingBell(10); break;
            case ItemType.PressureValve: InventoryManager.Instance.TryAddPressureValve(10); break;
            case ItemType.PrecisionLens: InventoryManager.Instance.TryAddPrecisionLens(10); break;
            case ItemType.Engine: InventoryManager.Instance.TryAddEngine(10); break;
         }

         int startingPearls = InventoryManager.Instance.pearlCount;
         int itemValue = GetItemValue(item.type);

         var itemUI = TradeHutManager.Instance.SellItems.Find(i => i.CompareTag(item.tag));
         Assert.IsNotNull(itemUI, $"Could not find {item.tag} in SellItems list.");

         // Open the specific item's popup window
         itemUI.Find("ItemButton").GetComponent<Button>().onClick.Invoke();
         yield return new WaitForSeconds(2.0f); // Opening item window

         var field = typeof(TradeHutManager).GetField("currentSellItem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
         Transform activeWindow = (Transform)field.GetValue(TradeHutManager.Instance);
         
         Button sellIncreaseBtn = activeWindow.Find("QuantityButtons/IncreaseButton").GetComponent<Button>();
         Button sellDecreaseBtn = activeWindow.Find("QuantityButtons/DecreaseButton").GetComponent<Button>();

         // Press increase 6 times (Wait 0.8 seconds per press)
         for (int i = 0; i < 6; i++) {
            sellIncreaseBtn.onClick.Invoke();
            yield return new WaitForSeconds(0.8f);
         }
         
         // Press decrease 1 time (Wait 0.8 seconds)
         sellDecreaseBtn.onClick.Invoke();
         yield return new WaitForSeconds(0.8f);
         
         // Confirm the sale
         TradeHutManager.Instance.SellItem();
         yield return new WaitForSeconds(2.0f); // Confirming Sale

         // Verify the pearl payout was correct
         int expectedPearls = startingPearls + (5 * itemValue);
         Assert.AreEqual(expectedPearls, InventoryManager.Instance.pearlCount, $"Pearl count mismatch for {item.tag}");

         // Determine what the inventory count should be after the sale
         int currentInvCount = 0;
         switch (item.type) {
            case ItemType.CrudeTool: currentInvCount = InventoryManager.Instance.crudeToolCount; break;
            case ItemType.Harpoon: currentInvCount = InventoryManager.Instance.harpoonCount; break;
            case ItemType.DivingBell: currentInvCount = InventoryManager.Instance.divingBellCount; break;
            case ItemType.PressureValve: currentInvCount = InventoryManager.Instance.pressureValveCount; break;
            case ItemType.PrecisionLens: currentInvCount = InventoryManager.Instance.precisionLensCount; break;
            case ItemType.Engine: currentInvCount = InventoryManager.Instance.engineCount; break;
         }

         // Read the UI text string to confirm the shelf display matches the backend inventory
         string expectedUIString = " x" + currentInvCount;
         string actualUIString = itemUI.Find("ItemCount").GetComponent<TextMeshProUGUI>().text;
         Assert.AreEqual(expectedUIString, actualUIString, $"Trade Hut UI item count mismatch for {item.tag} after selling.");
      }
      
      // Close the Trade Hut after all items have been tested
      TradeHutManager.Instance.CloseTradeHutPanel(TradeHutManager.TRADE_BUTTON);
      yield return new WaitForSeconds(2.0f); // Closing Panel
   }

   [UnityTest]
   public IEnumerator BuyAllItemsTransactionTest() 
   {
      SceneManager.LoadScene("MainScene");
      yield return new WaitForSeconds(1.0f);
      yield return new WaitForEndOfFrame();
      
      InventoryManager.Instance.TryAddPearl(5000);
      
      TradeHutManager.Instance.RequestTradeHutPanel(TradeHutManager.TRADE_BUTTON);
      TradeHutManager.Instance.ShowBuyPanel();
      yield return new WaitForSeconds(2.0f);
      
      var currentBuyItemField = typeof(TradeHutManager).GetField("currentBuyItem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      
      // TEST 1: BUY TIER 2 BLUEPRINT
      var tier2UI = TradeHutManager.Instance.BuyItems.Find(item => item.CompareTag(TradeHutManager.INDUSTRIAL_BLUEPRINT_TAG));
      tier2UI.Find("ItemButton").GetComponent<Button>().onClick.Invoke();
      yield return new WaitForSeconds(2.0f);
      TradeHutManager.Instance.BuyItem();
      yield return new WaitForSeconds(2.0f);
      
      Assert.IsTrue(ForgeManager.Instance.hasTier2Blueprint, "ForgeManager flag did not update for Tier 2.");
      
      // TEST 2: BUY TIER 3 BLUEPRINT
      var tier3UI = TradeHutManager.Instance.BuyItems.Find(item => item.CompareTag(TradeHutManager.CLOCKWORK_BLUEPRINT_TAG));
      tier3UI.Find("ItemButton").GetComponent<Button>().onClick.Invoke();
      yield return new WaitForSeconds(2.0f);
      
      TradeHutManager.Instance.BuyItem();
      yield return new WaitForSeconds(2.0f);
      Assert.IsTrue(ForgeManager.Instance.hasTier3Blueprint, "ForgeManager flag did not update for Tier 3.");
      
      // TEST 3: BUY MERCENARY ENGINEERS
      int startingEngineers = InventoryManager.Instance.mercenaryEngineerCount;
      var mercUI = TradeHutManager.Instance.BuyItems.Find(item => item.CompareTag(InventoryManager.MERCENARY_ENGINEER_TAG));
      mercUI.Find("ItemButton").GetComponent<Button>().onClick.Invoke();
      yield return new WaitForSeconds(2.0f);
      
      Transform activeMercWindow = (Transform)currentBuyItemField.GetValue(TradeHutManager.Instance);
      Button mercIncreaseBtn = activeMercWindow.Find("QuantityButtons/IncreaseButton").GetComponent<Button>();
      Button mercDecreaseBtn = activeMercWindow.Find("QuantityButtons/DecreaseButton").GetComponent<Button>();

      // Press increase 3 times, decrease 1 time (Net target: 2)
      mercIncreaseBtn.onClick.Invoke();
      yield return new WaitForSeconds(1.0f);
      mercIncreaseBtn.onClick.Invoke();
      yield return new WaitForSeconds(1.0f);
      mercIncreaseBtn.onClick.Invoke();
      yield return new WaitForSeconds(1.0f);
      mercDecreaseBtn.onClick.Invoke();
      yield return new WaitForSeconds(1.0f);

      TradeHutManager.Instance.BuyItem();
      yield return new WaitForSeconds(2.0f);
      
      Assert.AreEqual(startingEngineers + 2, InventoryManager.Instance.mercenaryEngineerCount, "Failed to buy Mercenary Engineers.");
      
      // TEST 4: BUY RAW ORE CHUNKS
      int startingOre = InventoryManager.Instance.oreCount;
      var oreUI = TradeHutManager.Instance.BuyItems.Find(item => item.CompareTag(TradeHutManager.RAW_ORE_CHUNK_TAG));
      oreUI.Find("ItemButton").GetComponent<Button>().onClick.Invoke();
      yield return new WaitForSeconds(2.0f);
      
      Transform activeOreWindow = (Transform)currentBuyItemField.GetValue(TradeHutManager.Instance);
      Button oreIncreaseBtn = activeOreWindow.Find("QuantityButtons/IncreaseButton").GetComponent<Button>();
      Button oreDecreaseBtn = activeOreWindow.Find("QuantityButtons/DecreaseButton").GetComponent<Button>();

      // Press increase 6 times, decrease 1 time (Net target: 5)
      for(int i = 0; i < 6; i++) 
      {
         oreIncreaseBtn.onClick.Invoke();
         yield return new WaitForSeconds(1.0f);
      }
      oreDecreaseBtn.onClick.Invoke();
      yield return new WaitForSeconds(1.0f);

      TradeHutManager.Instance.BuyItem();
      yield return new WaitForSeconds(2.0f);
      Assert.AreEqual(startingOre + 5, InventoryManager.Instance.oreCount, "Failed to buy Raw Ore Chunks.");

      // TEST 5: BUY INSURANCE POLICY (NEW)
      var insUI = TradeHutManager.Instance.BuyItems.Find(item => item.CompareTag(TradeHutManager.INSURANCE_POLICY_TAG));
      insUI.Find("ItemButton").GetComponent<Button>().onClick.Invoke();
      yield return new WaitForSeconds(2.0f);
      TradeHutManager.Instance.BuyItem();
      yield return new WaitForSeconds(2.0f);
      Assert.IsTrue(TradeHutManager.Instance.isInsurancePolicyActive, "Insurance policy flag did not activate after purchase.");
   }

   // =========================================================
   // NEW TEST: INSURANCE POLICY CRASH PAYOUT
   // =========================================================
   [UnityTest]
   public IEnumerator InsurancePolicyPayoutTest() {
      SceneManager.LoadScene("MainScene");
      yield return new WaitForSeconds(1.0f);
      yield return new WaitForEndOfFrame();

      int startingPearls = InventoryManager.Instance.pearlCount;

      // 1. Manually activate the policy
      TradeHutManager.Instance.isInsurancePolicyActive = true;

      // 2. Force a market crash scenario
      // Set World Event to Crude Tool (Tier 1)
      var worldEventField = typeof(TradeHutManager).GetField("worldEvent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      worldEventField.SetValue(TradeHutManager.Instance, (int)WorldEventTypes.CrudeToolEvent);

      // Set shiftDirection to 99 (guarantees it exceeds the TIER_ONE_CHANCE of 50 causing a crash)
      var shiftProp = typeof(TradeHutManager).GetProperty("shiftDirection");
      shiftProp.SetValue(TradeHutManager.Instance, 99);

      // 3. Trigger the check
      TradeHutManager.Instance.InsurancePolicyCheck();
      yield return null;

      // 4. Verify payout and reset
      Assert.IsFalse(TradeHutManager.Instance.isInsurancePolicyActive, "Insurance policy should deactivate after a check.");
      Assert.AreEqual(startingPearls + TradeHutManager.INSURANCE_POLICY_PAYOUT, InventoryManager.Instance.pearlCount, "Insurance payout of 500 was not awarded correctly.");
   }

   // =========================================================
   // NEW TEST: WORLD EVENT MARKET SHIFT & RESET
   // =========================================================
   [UnityTest]
   public IEnumerator WorldEventMarketShiftTest() {
      SceneManager.LoadScene("MainScene");
      yield return new WaitForSeconds(1.0f);
      yield return new WaitForEndOfFrame();

      int baseValue = GetItemValue(ItemType.CrudeTool);

      // 1. Prime the event variables
      TradeHutManager.Instance.CraftMarketForesight();

      // 2. Force World Event to Crude Tool
      var worldEventField = typeof(TradeHutManager).GetField("worldEvent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      worldEventField.SetValue(TradeHutManager.Instance, (int)WorldEventTypes.CrudeToolEvent);

      // Sync the internal event tracking variable
      var crudeEventField = typeof(TradeHutManager).GetField("crudeToolEvent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      crudeEventField.SetValue(TradeHutManager.Instance, WorldEventTypes.CrudeToolEvent);

      // 3. Force TurnManager to Active Event Turn
      TurnManager.Instance.eventCountdown = WORLD_EVENT_ACTIVE_TURN;

      // 4. Force Shift Direction to cause a crash (99 is > 50)
      typeof(TradeHutManager).GetProperty("shiftDirection").SetValue(TradeHutManager.Instance, 99);

      // 5. Apply the Shift!
      TradeHutManager.Instance.MarketFluctuate();
      yield return null;

      int crashedValue = GetItemValue(ItemType.CrudeTool);
      Assert.Less(crashedValue, baseValue, "MarketFluctuate should have DECREASED the crude tool value during a crash.");

      // 6. Test the Reset Process
      TurnManager.Instance.eventCountdown = WORLD_EVENT_RESET_TURN;
      TradeHutManager.Instance.ResetWorldEventShifts();
      yield return null;

      Assert.AreEqual(baseValue, GetItemValue(ItemType.CrudeTool), "ResetWorldEventShifts did not revert the value back to base.");
   }

   [UnityTest]
   public IEnumerator CommerceTierOneForesightUnlockTest() {
      SceneManager.LoadScene("MainScene");
      yield return new WaitForSeconds(1.0f);
      yield return new WaitForEndOfFrame();

      InventoryManager.Instance.TryAddPearl(LabManager.T1_COMM_PEARL);

      var commerceTabField = typeof(LabManager).GetField("commerceTab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      GameObject commerceTab = (GameObject)commerceTabField.GetValue(LabManager.labManager);

      LabManager.labManager.ImplementTierOneInnovation(commerceTab);
      yield return new WaitForSeconds(0.5f);

      TradeHutManager.Instance.RequestTradeHutPanel(TradeHutManager.TRADE_BUTTON);
      yield return new WaitForSeconds(2.0f); // Opening Panel

      var crudeToolUI = TradeHutManager.Instance.SellItems.Find(item => item.CompareTag(InventoryManager.CRUDE_TOOL_TAG));
      GameObject nextValueObj = crudeToolUI.Find("NextValue").gameObject;
      TextMeshProUGUI nextValueText = nextValueObj.GetComponent<TextMeshProUGUI>();

      Assert.IsTrue(nextValueObj.activeSelf, "NextValue UI text was not activated by LabManager Innovation!");

      string lastValue = "";
      for (int i = 1; i <= 3; i++) {
         TradeHutManager.Instance.CraftMarketForesight();
         yield return new WaitForSeconds(0.5f);

         string currentValue = nextValueText.text;
         Assert.IsFalse(string.IsNullOrEmpty(currentValue), $"Iteration {i}: Next Value text is empty.");
         Assert.AreNotEqual("Next Value: 0", currentValue, $"Iteration {i}: Value failed to calculate (stayed at 0).");

         lastValue = currentValue;
      }
   }

   [UnityTest]
   public IEnumerator CommerceTierTwoRecycleUnlockAndExecuteTest() {
      SceneManager.LoadScene("MainScene");
      yield return new WaitForSeconds(1.0f);
      yield return new WaitForEndOfFrame();

      InventoryManager.Instance.TryAddOre(20);

      Assert.IsNotNull(TradeHutManager.Instance.RecycleButton, "RecycleButton reference is missing!");
      Assert.IsFalse(TradeHutManager.Instance.RecycleButton.gameObject.activeSelf, "Setup Error: Recycle button should be hidden before Tier 2 upgrade.");

      var commerceTabField = typeof(LabManager).GetField("commerceTab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      GameObject commerceTab = (GameObject)commerceTabField.GetValue(LabManager.labManager);

      LabManager.labManager.ImplementTierTwoInnovation(commerceTab);
      yield return new WaitForSeconds(1.0f);

      TradeHutManager.Instance.RequestTradeHutPanel(TradeHutManager.TRADE_BUTTON);
      yield return new WaitForSeconds(2.0f); // Opening Panel

      TradeHutManager.Instance.ShowBuyPanel();
      yield return new WaitForSeconds(2.0f); // Switching Panels

      Assert.IsTrue(TradeHutManager.Instance.RecycleButton.gameObject.activeSelf, "Recycle button was not activated by the Tier 2 Lab Upgrade!");
      Assert.AreEqual(3, TradeHutManager.Instance.marketShiftMin, "Market shift min did not update to 3.");
      Assert.AreEqual(5, TradeHutManager.Instance.marketShiftMax, "Market shift max did not update to 5.");

      int startingOre = InventoryManager.Instance.oreCount;
      int startingPearls = InventoryManager.Instance.pearlCount;

      // Simulate transaction
      TradeHutManager.Instance.RecycleOre();
      yield return new WaitForSeconds(2.0f); // Transaction Confirmed

      Assert.AreEqual(startingOre - TradeHutManager.ORE_EXCHANGE_COST, InventoryManager.Instance.oreCount, "Ore was not deducted after recycling.");
      Assert.Greater(InventoryManager.Instance.pearlCount, startingPearls, "Pearls were not awarded after recycling.");
   }

   [UnityTest]
   public IEnumerator CommerceTierThreeBuffTest() {
      SceneManager.LoadScene("MainScene");
      yield return new WaitForSeconds(1.0f);
      yield return new WaitForEndOfFrame();

      int baseCrudeTool = GetItemValue(ItemType.CrudeTool);
      int baseHarpoon = GetItemValue(ItemType.Harpoon);
      int basePressureValve = GetItemValue(ItemType.PressureValve);
      int baseEngine = GetItemValue(ItemType.Engine);

      int expectedCrudeTool = Mathf.CeilToInt(baseCrudeTool * 1.2f);
      int expectedHarpoon = Mathf.CeilToInt(baseHarpoon * 1.2f);
      int expectedPressureValve = Mathf.CeilToInt(basePressureValve * 1.2f);
      int expectedEngine = Mathf.CeilToInt(baseEngine * 1.2f);

      var commerceTabField = typeof(LabManager).GetField("commerceTab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      GameObject commerceTab = (GameObject)commerceTabField.GetValue(LabManager.labManager);

      LabManager.labManager.ImplementTierThreeInnovation(commerceTab);
      yield return new WaitForSeconds(1.0f);

      Assert.AreEqual(5, TradeHutManager.Instance.marketShiftMin, "Market shift min did not update to 5.");
      Assert.AreEqual(10, TradeHutManager.Instance.marketShiftMax, "Market shift max did not update to 10.");
      Assert.IsTrue(TradeHutManager.Instance.isTier3BuffACtive, "isTier3BuffACtive flag was not set to true!");

      Assert.AreEqual(expectedCrudeTool, GetItemValue(ItemType.CrudeTool), "Crude Tool base value did not increase by 20%.");
      Assert.AreEqual(expectedHarpoon, GetItemValue(ItemType.Harpoon), "Harpoon base value did not increase by 20%.");
      Assert.AreEqual(expectedPressureValve, GetItemValue(ItemType.PressureValve), "Pressure Valve base value did not increase by 20%.");
      Assert.AreEqual(expectedEngine, GetItemValue(ItemType.Engine), "Engine base value did not increase by 20%.");

      TradeHutManager.Instance.RequestTradeHutPanel(TradeHutManager.TRADE_BUTTON);
      yield return new WaitForSeconds(2.0f); // Opening Panel

      var crudeToolUI = TradeHutManager.Instance.SellItems.Find(item => item.CompareTag(InventoryManager.CRUDE_TOOL_TAG));
      string uiValue = crudeToolUI.Find("ItemValue").GetComponent<TextMeshProUGUI>().text;

      Assert.AreEqual(expectedCrudeTool.ToString(), uiValue, "UI did not update to reflect new Crude Tool value.");
   }
}