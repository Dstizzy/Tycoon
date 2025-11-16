using NUnit.Framework;

using System.Collections;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

[TestFixture]
public class InventoryTest : InputTestFixture 
{
   [SetUp]
   public void TestSetup() 
   {
      // Ensure the default Mouse device is available
      if (Mouse.current == null)
         InputSystem.AddDevice<Mouse>();
   }

   [UnityTearDown] // Runs after *each* [UnityTest]
   public IEnumerator CleanupTestObjects() 
   {
      // Find all the pop-up buttons that were likely left behind.
      GameObject[] popUpButtons = GameObject.FindGameObjectsWithTag("BuildingButtonPopUp");

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
   public IEnumerator PopupTest() 
   {
      Button     InventoryButton;
      Button     ExitButton;
      Button     CraftTab;
      Button     ResourceTab;
      GameObject inventoryPanel;
      GameObject CraftsPanel;
      GameObject ResourcePanel;


      SceneManager.LoadScene("MainScene");
      yield return null;

      // Set the main camera
      Camera mainCamera = Camera.main;
      Assert.IsNotNull(mainCamera, "Setup Error: Missing Main Camera.");

      inventoryPanel  = GameObject.Find("Inventory/InventoryUI/InventoryPanel");
      CraftsPanel     = GameObject.Find("Inventory/InventoryUI/InventoryPanel/CraftsPanel");
      ResourcePanel   = GameObject.Find("Inventory/InventoryUI/InventoryPanel/ResourcePanel");
      InventoryButton = GameObject.Find($"Main Canvas/Main UI/InventoryButton").GetComponent<Button>();
      CraftTab        = GameObject.Find("Inventory/InventoryUI/InventoryPanel/CraftsTab").GetComponent<Button>();
      ResourceTab     = GameObject.Find("Inventory/InventoryUI/InventoryPanel/ResourceTab").GetComponent<Button>();
      ExitButton      = GameObject.Find("Inventory/InventoryUI/InventoryPanel/ExitButton").GetComponent<Button>();

      Assert.IsNotNull(InventoryButton, "Inventory Button not found in the scene.");
      Assert.IsNotNull(CraftTab, "Craft Tab Button not found in the Inventory Panel.");
      Assert.IsNotNull(ResourceTab, "Resource Tab Button not found in the Inventory Panel.");

      InventoryButton.onClick.Invoke();
      Assert.IsTrue(ResourcePanel.activeSelf, "Resource Panel did not pop up");
      yield return new WaitForSeconds(.2f);

      CraftTab.onClick.Invoke();
      Assert.IsFalse(ResourcePanel.activeSelf, "Resource Panel did not close");
      Assert.IsTrue(CraftsPanel.activeSelf, "Craft Panel did not pop up");
      yield return new WaitForSeconds(.2f);


      ResourceTab.onClick.Invoke();
      Assert.IsFalse(CraftsPanel.activeSelf, "Craft Panel did not close");
      Assert.IsTrue(ResourcePanel.activeSelf, "Resource Panel did not pop up");
      yield return new WaitForSeconds(.2f);


      Assert.IsNotNull(ExitButton, "Exit Button not found in the Inventory Panel.");
      yield return new WaitForSeconds(.2f);

      ExitButton.onClick.Invoke();
      Assert.IsFalse(inventoryPanel.activeSelf, "Inventory Panel did not close");

      yield return null;
   }

   [UnityTest]
   public IEnumerator ItemWindowsTest() 
   {
      Button     InventoryButton;
      Button     ExitButton;
      Button     CraftTab;
      Button     ResourceTab;
      GameObject inventoryPanel;
      GameObject CraftsPanel;
      GameObject ResourcePanel;
      GameObject ResourceWindow;
      GameObject CraftWindow;

      SceneManager.LoadScene("MainScene");
      yield return null;

      // Set the main camera
      Camera mainCamera = Camera.main;
      Assert.IsNotNull(mainCamera, "Setup Error: Missing Main Camera.");

      inventoryPanel  = GameObject.Find("Inventory/InventoryUI/InventoryPanel");
      CraftsPanel     = GameObject.Find("Inventory/InventoryUI/InventoryPanel/CraftsPanel");
      ResourcePanel   = GameObject.Find("Inventory/InventoryUI/InventoryPanel/ResourcePanel");
      InventoryButton = GameObject.Find($"Main Canvas/Main UI/InventoryButton").GetComponent<Button>();
      CraftTab        = GameObject.Find("Inventory/InventoryUI/InventoryPanel/CraftsTab").GetComponent<Button>();
      ResourceTab     = GameObject.Find("Inventory/InventoryUI/InventoryPanel/ResourceTab").GetComponent<Button>();
      ExitButton      = GameObject.Find("Inventory/InventoryUI/InventoryPanel/ExitButton").GetComponent<Button>();
      CraftWindow     = GameObject.Find("Inventory/InventoryUI/InventoryPanel/CraftsPanel/CraftWindow");
      ResourceWindow  = GameObject.Find("Inventory/InventoryUI/InventoryPanel/ResourcePanel/ResourceWindow");

      Assert.IsNotNull(InventoryButton, "Inventory Button not found in the scene.");
      Assert.IsNotNull(CraftTab, "Craft Tab Button not found in the Inventory Panel.");
      Assert.IsNotNull(ResourceTab, "Resource Tab Button not found in the Inventory Panel.");

      InventoryButton.onClick.Invoke();
      yield return new WaitForSeconds(.2f);

      CraftTab.onClick.Invoke();
      yield return new WaitForSeconds(.2f);

      ResourceTab.onClick.Invoke();
      yield return new WaitForSeconds(.2f);

      yield return new WaitForSeconds(.2f);


      // If ResourcePanel is a GameObject:
      Transform ResourceContainer = ResourcePanel.transform.Find("ResourceContainer");

      Assert.IsNotNull(ResourceContainer, "Could not find ResourceContainer object.");

      // Find a resource button (ResourceButton) under the ResourcePanel
      Button[] resourceButtons = ResourceContainer.GetComponentsInChildren<Button>(); // 'false' is default, only finds active children

      Assert.IsNotEmpty(resourceButtons, "No active resource buttons found in ResourceContainer.");
      Button resourceButton = null;

      // The CreateResource method places the actual button component on a child named "ResourceButton".
      // We assume resourceButtons[0] will be the first instantiated button component.
      resourceButton = resourceButtons[0];

      Assert.IsNotNull(resourceButton, "Could not find an active resource button to click.");

      // Click the resource button to open the resource info window
      resourceButton.onClick.Invoke();
      yield return new WaitForSeconds(.2f);

      Assert.IsNotNull(ResourceWindow, "Resource window object not present in scene.");
      Assert.IsTrue(ResourceWindow.activeSelf, "Resource window did not open after clicking resource button.");

      CraftTab.onClick.Invoke();
      yield return new WaitForSeconds(.2f);

      Transform CraftContainer = CraftsPanel.transform.Find("CraftContainer");

      Assert.IsNotNull(CraftContainer, "Could not find CraftContainer object.");

      // Find a resource button (ResourceButton) under the ResourcePanel
      Button[] craftButtons = CraftContainer.GetComponentsInChildren<Button>(); // 'false' is default, only finds active children

      Assert.IsNotEmpty(craftButtons, "No active craft buttons found in CraftContainer.");
      Button craftButton = null;

      // The CreateResource method places the actual button component on a child named "ResourceButton".
      // We assume resourceButtons[0] will be the first instantiated button component.
      craftButton = craftButtons[0];
      Debug.Log($"Found craft button: {craftButton.name}");

      Assert.IsNotNull(craftButton, "Could not find an active craft button to click.");

      // Click the resource button to open the resource info window
      craftButton.onClick.Invoke();
      yield return new WaitForSeconds(.2f);

      Assert.IsNotNull(CraftWindow, "Craft window object not present in scene.");
      Assert.IsTrue(CraftWindow.activeSelf, "Craft window did not open after clicking craft button.");

      // Cleanup: close windows and inventory
      ExitButton.onClick.Invoke();
      Assert.IsFalse(inventoryPanel.activeSelf, "Inventory Panel did not close");

      yield return null;
   }
}