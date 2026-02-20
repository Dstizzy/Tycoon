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
   public IEnumerator SellItemsTransactionTest() 
   {
      // 1. Load the scene and wait for initialization
      SceneManager.LoadScene("MainScene");

      float timeout = 2.0f;
      while (!SceneManager.GetActiveScene().name.Equals("MainScene") && timeout > 0) {
         timeout -= Time.deltaTime;
         yield return null;
      }

      // Wait for Start() and UI to stabilize
      yield return new WaitForEndOfFrame();
      yield return new WaitForSeconds(1.0f); // Delay to see the initial state

      // 2. Setup Inventory state
      InventoryManager.Instance.TryAddCrudeTool(10);
      int startingPearls = InventoryManager.Instance.pearlCount;
      int itemValue = GetItemValue(ItemType.CrudeTool);

      // 3. Open the main Trade Panel
      TradeHutManager.Instance.RequestTradeHutPanel(TradeHutManager.TRADE_BUTTON);
      yield return new WaitForSeconds(1.0f); // Delay to see the Trade Panel open

      // 4. Find the template item in the list
      var itemUI = TradeHutManager.Instance.SellItems.Find(item => item.CompareTag(InventoryManager.CRUDE_TOOL_TAG));
      Assert.IsNotNull(itemUI, "Could not find Crude Tool in the SellItems list!");

      // 5. Trigger 'CreateSellWindow' by clicking the item button
      // This instantiates the window that contains "ItemCount" and "currencyGained"
      Button itemBtn = itemUI.Find("ItemButton").GetComponent<Button>();
      itemBtn.onClick.Invoke();
      yield return new WaitForSeconds(1.0f); // Delay to see the Sell Transaction Window appear

      // 6. Grab the reference directly from the Manager using Reflection
      var field = typeof(TradeHutManager).GetField("currentSellItem",
          System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      Transform currentSellItemTransform = (Transform)field.GetValue(TradeHutManager.Instance);

      Assert.IsNotNull(currentSellItemTransform, "TradeHutManager.currentSellItem is null!");
      GameObject activeWindow = currentSellItemTransform.gameObject;

      // 7. Increment count and observe UI updates
      for (int i = 0; i < 5; i++) {
         TradeHutManager.Instance.IncreaseSellItemCount(activeWindow.transform);
         yield return new WaitForSeconds(0.3f); // Brief delay to see the number incrementing
      }
      yield return new WaitForSeconds(1.0f); // Pause to see the final count before selling

      // 8. Execute Sale and Verify
      TradeHutManager.Instance.SellItem();
      yield return new WaitForSeconds(1.0f); // Delay to see the window close and pearls update

      // Verification
      Assert.AreEqual(startingPearls + (5 * itemValue), InventoryManager.Instance.pearlCount, "Pearls were not added correctly.");
      Assert.AreEqual(5, InventoryManager.Instance.crudeToolCount, "Items were not deducted from inventory correctly.");
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

      // 4. Find the Blueprint UI button and INVOKE the click
      // This call triggers CreateBuyWindow() which sets the 'currentBuyItem' reference
      var blueprintUI = TradeHutManager.Instance.BuyItems.Find(item => item.CompareTag(TradeHutManager.TIER_2_BLUEPRINT));
      Assert.IsNotNull(blueprintUI, "Blueprint UI not found in list.");

      Button buyButton = blueprintUI.Find("ItemButton").GetComponent<Button>();
      buyButton.onClick.Invoke();

      // 5. CRITICAL: Wait for the next frame so the Manager can finish instantiating the window
      yield return new WaitForEndOfFrame();
      yield return new WaitForSeconds(0.5f);

      // 6. Verify the reference is set before calling BuyItem
      var field = typeof(TradeHutManager).GetField("currentBuyItem",
          System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      Assert.IsNotNull(field.GetValue(TradeHutManager.Instance), "Manager failed to set 'currentBuyItem' reference!");

      // 7. Execute Purchase
      TradeHutManager.Instance.BuyItem();
      yield return new WaitForSeconds(1.0f);

      // 8. Verify logic results
      Assert.IsTrue(ForgeManager.Instance.hasTier2Blueprint, "ForgeManager flag did not update to TRUE.");
   }
}