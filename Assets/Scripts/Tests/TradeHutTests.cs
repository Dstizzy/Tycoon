using NUnit.Framework;

using System.Collections;

using TMPro;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

[TestFixture]
public class TradeHutTests : InputTestFixture {
   [SetUp]
   public void TestSetup() {
      // Ensure the default Mouse device is available
      if (Mouse.current == null) {
         InputSystem.AddDevice<Mouse>();
      }
   }

   [UnityTearDown] // Runs after *each* [UnityTest]
   public IEnumerator CleanupTestObjects() {
      // Find all the pop-up buttons that were likely left behind.
      GameObject[] popUpButtons = GameObject.FindGameObjectsWithTag("BuildingButton");

      foreach (GameObject go in popUpButtons) {
         // Destroy them to ensure they aren't blocking the next test's input.
         UnityEngine.Object.Destroy(go);
      }

      // Allow one frame for the destruction to take effect
      yield return null;
   }

   [Test]
   public void IsMouseAdded() {
      Assert.IsNotNull(Mouse.current, "Setup Error: Missing Mouse device.");
   }

   [UnityTest]
   public IEnumerator TradePanelsTest() {

      Button     ExitButton     = null;
      Button     BuyTab         = null;
      GameObject BuyPanel       = null;
      GameObject TradePanels    = null;
      GameObject SellPanel      = null;
      GameObject InfoPanel      = null;
      GameObject UpgradePanel   = null;
      Vector3    worldPosition  = new Vector3(0, 0, 0);
      Vector2    screenPosition = new Vector2(0, 0);

      SceneManager.LoadScene("MainScene");
      yield return new WaitForSeconds(0.1f);
      
      // Find a specific building in the scene (Trade Hut)
      GameObject tradeHut = GameObject.Find("TradeHut");
      Assert.IsNotNull(tradeHut, $"Could not find trade hut");
      
      // Set the main camera
      Camera mainCamera = Camera.main;
      Assert.IsNotNull(mainCamera, "Setup Error: Missing Main Camera.");

      // 1.Find the PopUpManager's GameObject by its unique name
      GameObject popUpManagerGO = GameObject.Find("PopUp Manager");
      Assert.IsNotNull(popUpManagerGO, "Setup Error: PopUpManager GameObject not found in scene by name.");
      Component popUpManagerComponent = popUpManagerGO.GetComponent("PopUpManager");
      Assert.IsNotNull(popUpManagerComponent, "Setup Error: PopUpManager component not found on the GameObject.");

      // Loop through each of the three buttons: Trade, Upgrade, Info
      for (int buttonIndex = 0; buttonIndex < 3; buttonIndex++) {
         // ... (input queuing logic remains the same) ...

         Set(Mouse.current.position, screenPosition);
         
         yield return new WaitForSeconds(0.1f);

         // 4. NOW, find the containers (they are frozen and won't be destroyed)
         GameObject[] popUpContainers = GameObject.FindGameObjectsWithTag("BuildingButton");

         Assert.IsNotEmpty(popUpContainers, "No building buttons (containers) found after input was frozen.");
         // We assert that the containers were created.
         // Check if the panels open up for each button
         Button myButton = popUpContainers[buttonIndex].GetComponentInChildren<Button>();
         Assert.IsNotNull(myButton, $"Could not find button.");
         Debug.Log($"{buttonIndex}");

         // Get the text of the button
         TextMeshProUGUI buttonTextComponent = myButton.GetComponentInChildren<TextMeshProUGUI>();
         Assert.IsNotNull(buttonTextComponent, "Could not find TextMeshProUGUI component on button.");
         Debug.Log($"Clicking button with text: {buttonTextComponent.text}");
         
         // Click a building button pop up
         //worldPosition = myButton.transform.position;
         //screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
         //Set(Mouse.current.position, screenPosition);
         //yield return null;
         //Press(Mouse.current.leftButton);
         //yield return null;
         //Release(Mouse.current.leftButton);
         //yield return null;
         
         myButton.onClick.Invoke();
         // Verify the corresponding panel pops up
         switch (buttonTextComponent.text) 
         {
            case "Trade":
               TradePanels = GameObject.Find("Building Panel Canvas/UI_TradeHut/TradePanels");

               SellPanel = GameObject.Find("Building Panel Canvas/UI_TradeHut/TradePanels/SellPanel");
               Assert.IsNotNull(SellPanel, $"Could not find the Trade Panel.");
               Assert.IsTrue(SellPanel.activeSelf, "Sell Panel did not pop up.");

               BuyTab = GameObject.Find($"Building Panel Canvas/UI_TradeHut/TradePanels/BuyTab").GetComponent<Button>();
               BuyTab.onClick.Invoke();

               BuyPanel = GameObject.Find("Building Panel Canvas/UI_TradeHut/TradePanels/BuyPanel");
               Assert.IsNotNull(BuyPanel, $"Could not find the Trade Panel.");
               Assert.IsTrue(BuyPanel.activeSelf, "Trade Panel did not pop up.");

               ExitButton = GameObject.Find($"Building Panel Canvas/UI_TradeHut/TradePanels/ExitButton").GetComponent<Button>();
               Assert.IsNotNull(ExitButton, "Could not find Exit Button.");
               ExitButton.onClick.Invoke();

               Assert.IsFalse(TradePanels.activeSelf, "TradePanels are still active");
               break;
            case "Upgrade":
               UpgradePanel = GameObject.Find("Building Panel Canvas/UI_TradeHut/TradeUpgradePanel");
               Assert.IsNotNull(UpgradePanel, $"Could not find the Upgrade Panel.");
               Assert.IsTrue(UpgradePanel.activeSelf, "Upgrade Panel did not pop up.");

               ExitButton = GameObject.Find($"Building Panel Canvas/UI_TradeHut/TradeUpgradePanel/CancelButton").GetComponent<Button>();
               Assert.IsNotNull(ExitButton, "Could not find Exit Button.");

               ExitButton.onClick.Invoke();
               Assert.IsFalse(UpgradePanel.activeSelf, "Upgrade Panel is still active");
               break;
            case "Info":
               InfoPanel = GameObject.Find("Building Panel Canvas/UI_TradeHut/TradeInfoPanel");
               Assert.IsNotNull(InfoPanel, $"Could not find the Info Panel.");
               Assert.IsTrue(InfoPanel.activeSelf, "Info Panel did not pop up.");

               ExitButton = GameObject.Find($"Building Panel Canvas/UI_TradeHut/TradeInfoPanel/ExitButton").GetComponent<Button>();
               Assert.IsNotNull(ExitButton, "Could not find Exit Button.");
               ExitButton.onClick.Invoke();
               Assert.IsFalse(InfoPanel.activeSelf, "Info Panel is still atcive");
               break;
            /* This occurs when there is a fourth button                                     */
            default:
               Assert.Fail();
               break;
         }
         
         yield return null;
      } 
   }
}