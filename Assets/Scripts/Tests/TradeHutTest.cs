using NUnit.Framework;

using System;
using System.Collections;

using TMPro;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

[TestFixture]
public class InventoryTest : InputTestFixture {
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
      GameObject[] popUpButtons = GameObject.FindGameObjectsWithTag("BuildingButtonPopUp");

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
   public IEnumerator PopupTest() 
   {
      Button InventoryButton;
      Button ExitButton;
      GameObject inventoryPanel;
      Vector3 worldPosition = new Vector3(0, 0, 0);
      Vector2 screenPosition = new Vector2(0, 0);

      SceneManager.LoadScene("MainScene");
      yield return null;

      // Set the main camera
      Camera mainCamera = Camera.main;
      Assert.IsNotNull(mainCamera, "Setup Error: Missing Main Camera.");

      inventoryPanel = GameObject.Find("Inventory/InventoryUI/InventoryPanel");
      InventoryButton = GameObject.Find($"Main Canvas/Main UI/InventoryButton").GetComponent<Button>();
      ExitButton = GameObject.Find("Inventory/InventoryUI/InventoryPanel/ExitButton").GetComponent<Button>();

      Assert.IsNotNull(InventoryButton, "Inventory Button not found in the scene.");

      //yield return new WaitForSeconds(0.1f);

      worldPosition = InventoryButton.transform.position;
      screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
      Set(Mouse.current.position, screenPosition);
      yield return null;
      Press(Mouse.current.leftButton);
      yield return null;
      Release(Mouse.current.leftButton);
      yield return null;

      //InventoryButton.onClick.Invoke();
      Assert.IsTrue(inventoryPanel.activeSelf, "Inventory Panel did not pop up");
      Assert.IsNotNull(ExitButton, "Exit Button not found in the Inventory Panel.");

      ExitButton.onClick.Invoke();
      Assert.IsFalse(inventoryPanel.activeSelf, "Inventory Panel did not close");

      yield return null;
   }
}