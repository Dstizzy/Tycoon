using NUnit.Framework;

using System.Collections;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

using static Item;

[TestFixture]
public class TradeHutTests : InputTestFixture {
   [SetUp]
   public void TestSetup()
   {
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
   public void IsMouseAdded() 
   {
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
   public IEnumerator BuyBlueprintTransactionTest() {
      // 1. Load scene and wait for initialization
      SceneManager.LoadScene("MainScene");
      yield return new WaitForSeconds(1.0f);
      yield return new WaitForEndOfFrame();

      // 2. Setup: Ensure player has enough pearls
      InventoryManager.Instance.TryAddPearl(5000);
      Assert.IsFalse(ForgeManager.Instance.hasTier2Blueprint, "Setup Error: Tier 2 already unlocked.");

      // 3. Open Panels
      TradeHutManager.Instance.RequestTradeHutPanel(TradeHutManager.TRADE_BUTTON);
      TradeHutManager.Instance.ShowBuyPanel();
      yield return new WaitForSeconds(1.0f);

      // 4. Find the Blueprint UI button and CLICK it
      var blueprintUI = TradeHutManager.Instance.BuyItems.Find(item => item.CompareTag(TradeHutManager.TIER_2_BLUEPRINT));
      Assert.IsNotNull(blueprintUI, "Tier 2 Blueprint UI button not found in Buy Panel.");

      Button buyButton = blueprintUI.Find("ItemButton").GetComponent<Button>();
      buyButton.onClick.Invoke();

      // 5. CRITICAL FIX: Wait for the Manager to finish CreateBuyWindow() 
      // Calling onClick.Invoke() is asynchronous; the Manager needs a frame to set currentBuyItem
      yield return new WaitForEndOfFrame();
      yield return new WaitForSeconds(0.5f);

      // 6. Verify the reference is set via Reflection before calling BuyItem
      var field = typeof(TradeHutManager).GetField("currentBuyItem",
          System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      var currentBuyRef = field.GetValue(TradeHutManager.Instance);

      Assert.IsNotNull(currentBuyRef, "TradeHutManager.currentBuyItem is still null! Manager did not set the reference after the click.");

      // 7. Execute Purchase
      TradeHutManager.Instance.BuyItem();
      yield return new WaitForSeconds(1.0f);

      // 8. Verify logic results
      Assert.IsTrue(ForgeManager.Instance.hasTier2Blueprint, "ForgeManager flag did not update to TRUE.");
   }
}