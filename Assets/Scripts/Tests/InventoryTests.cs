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
   public IEnumerator PopupTest() {
      // 1. Load the scene and wait
      SceneManager.LoadScene("MainScene");

      // Wait until the scene is actually loaded
      float timeout = 2f;
      while (!SceneManager.GetActiveScene().name.Equals("MainScene") && timeout > 0) {
         timeout -= Time.deltaTime;
         yield return null;
      }

      // Give Unity one extra frame to run Awake/Start on UI elements
      yield return new WaitForEndOfFrame();

      // 2. Locate UI Elements with careful null checking
      GameObject inventoryPanel = GameObject.Find("Inventory/InventoryUI/InventoryPanel");
      GameObject craftsPanel = GameObject.Find("Inventory/InventoryUI/InventoryPanel/CraftsPanel");
      GameObject resourcePanel = GameObject.Find("Inventory/InventoryUI/InventoryPanel/ResourcePanel");

      // Ensure we find the button on the Main Canvas
      GameObject invBtnObj = GameObject.Find("Main Canvas/Main UI/InventoryButton");
      Assert.IsNotNull(invBtnObj, "Inventory Button GameObject not found.");
      Button inventoryButton = invBtnObj.GetComponent<Button>();

      Button craftTab = GameObject.Find("Inventory/InventoryUI/InventoryPanel/CraftsTab")?.GetComponent<Button>();
      Button resourceTab = GameObject.Find("Inventory/InventoryUI/InventoryPanel/ResourceTab")?.GetComponent<Button>();
      Button exitButton = GameObject.Find("Inventory/InventoryUI/InventoryPanel/ExitButton")?.GetComponent<Button>();

      // 3. Perform assertions
      Assert.IsNotNull(inventoryButton, "Inventory Button component missing.");
      Assert.IsNotNull(craftTab, "Craft Tab Button missing.");

      // 4. Simulate Clicks (Method Invocation)
      inventoryButton.onClick.Invoke();
      yield return null; // Wait one frame for UI toggle logic
      Assert.IsTrue(resourcePanel.activeSelf, "Resource Panel should be active after opening inventory.");

      craftTab.onClick.Invoke();
      yield return null;
      Assert.IsFalse(resourcePanel.activeSelf, "Resource Panel should close when Craft Tab is clicked.");
      Assert.IsTrue(craftsPanel.activeSelf, "Crafts Panel should open when Craft Tab is clicked.");

      exitButton.onClick.Invoke();
      yield return null;
      Assert.IsFalse(inventoryPanel.activeSelf, "Inventory Panel should be inactive after clicking exit.");
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