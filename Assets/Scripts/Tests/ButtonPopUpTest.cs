using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

[TestFixture]
public class ButtonPopUpTest : InputTestFixture
{

    [SetUp]
    public void TestSetup()
    {
        // Ensure the default Mouse device is available
        if (Mouse.current == null)
        {
            InputSystem.AddDevice<Mouse>();
        }
    }

    [Test]
    public void IsMouseAdded()
    {
        Assert.IsNotNull(Mouse.current, "Setup Error: Missing Mouse device.");


    }

    [UnityTest]
    public IEnumerator TradeHutButtonsPopUpWhenBuildingIsClicked()
    {
        // Load the Main Scene
        SceneManager.LoadScene("MainScene");
        yield return new WaitForSeconds(0.1f);

        // Find a specific building in the scene (Trade Hut)
        GameObject TradeHut = GameObject.Find("TradeHut");
        Assert.IsNotNull(TradeHut, $"Could not find trade hut");

        // Set the main camera
        Camera mainCamera = Camera.main;
        Assert.IsNotNull(mainCamera, "Setup Error: Missing Main Camera.");

        // Find the position of the building in screen coordinates
        Vector3 worldPosition = TradeHut.transform.position;
        Vector2 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

        Assert.IsNotNull(Mouse.current, "Setup Error: Missing Mouse device.");

        // Set the mouse position to the building
        Set(Mouse.current.position, screenPosition);
        yield return new WaitForSeconds(0.01f);

        // Simulate a mouse click
        Press(Mouse.current.leftButton);
        yield return new WaitForSeconds(0.01f);

        // Release the mouse click
        Release(Mouse.current.leftButton);
        yield return new WaitForSeconds(0.01f);

        // Check for the presence of building buttons upon clicking the building
        GameObject[] myButtons = GameObject.FindGameObjectsWithTag("BuildingButtonPopUp");
        yield return new WaitForSeconds(0.1f);
        foreach(GameObject myButton in myButtons)
            Assert.IsNotNull(myButton, $"Could not find button.");

        yield return null;
    }

    [UnityTest]
    public IEnumerator ForgeButtonsPopUpWhenBuildingIsClicked()
    {
        // Load the Main Scene
        SceneManager.LoadScene("MainScene");
        yield return new WaitForSeconds(0.1f);

        // Find a specific building in the scene (Trade Hut)
        GameObject Forge = GameObject.Find("Forge");
        Assert.IsNotNull(Forge, $"Could not find trade hut");

        // Set the main camera
        Camera mainCamera = Camera.main;
        Assert.IsNotNull(mainCamera, "Setup Error: Missing Main Camera.");

        // Find the position of the building in screen coordinates
        Vector3 worldPosition = Forge.transform.position;
        Vector2 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

        Assert.IsNotNull(Mouse.current, "Setup Error: Missing Mouse device.");

        // Set the mouse position to the building
        Set(Mouse.current.position, screenPosition);
        yield return new WaitForSeconds(0.01f);

        // Simulate a mouse click
        Press(Mouse.current.leftButton);
        yield return new WaitForSeconds(0.01f);

        // Release the mouse click
        Release(Mouse.current.leftButton);
        yield return new WaitForSeconds(0.01f);

        // Check for the presence of building buttons upon clicking the building
        GameObject[] myButtons = GameObject.FindGameObjectsWithTag("BuildingButtonPopUp");
        yield return new WaitForSeconds(0.1f);
        foreach (GameObject myButton in myButtons)
            Assert.IsNotNull(myButton, $"Could not find button.");

        yield return null;
    }

    [UnityTest]
    public IEnumerator OreRefineryButtonsPopUpWhenBuildingIsClicked()
    {
        // Load the Main Scene
        SceneManager.LoadScene("MainScene");
        yield return new WaitForSeconds(0.1f);

        // Find a specific building in the scene (Trade Hut)
        GameObject OreRefinery = GameObject.Find("Ore Refinery");
        Assert.IsNotNull(OreRefinery, $"Could not find trade hut");

        // Set the main camera
        Camera mainCamera = Camera.main;
        Assert.IsNotNull(mainCamera, "Setup Error: Missing Main Camera.");

        // Find the position of the building in screen coordinates
        Vector3 worldPosition = OreRefinery.transform.position;
        Vector2 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

        Assert.IsNotNull(Mouse.current, "Setup Error: Missing Mouse device.");

        // Set the mouse position to the building
        Set(Mouse.current.position, screenPosition);
        yield return new WaitForSeconds(0.01f);

        // Simulate a mouse click
        Press(Mouse.current.leftButton);
        yield return new WaitForSeconds(0.01f);

        // Release the mouse click
        Release(Mouse.current.leftButton);
        yield return new WaitForSeconds(0.01f);

        // Check for the presence of building buttons upon clicking the building
        GameObject[] myButtons = GameObject.FindGameObjectsWithTag("BuildingButtonPopUp");
        yield return new WaitForSeconds(0.1f);
        foreach (GameObject myButton in myButtons)
            Assert.IsNotNull(myButton, $"Could not find button.");

        yield return null;
    }

    [UnityTest]
    public IEnumerator ExplorationUnitButtonsPopUpWhenBuildingIsClicked()
    {
        // Load the Main Scene
        SceneManager.LoadScene("MainScene");
        yield return new WaitForSeconds(0.1f);

        // Find a specific building in the scene (Trade Hut)
        GameObject ExplorationUnit = GameObject.Find("Exploration");
        Assert.IsNotNull(ExplorationUnit, $"Could not find trade hut");

        // Set the main camera
        Camera mainCamera = Camera.main;
        Assert.IsNotNull(mainCamera, "Setup Error: Missing Main Camera.");

        // Find the position of the building in screen coordinates
        Vector3 worldPosition = ExplorationUnit.transform.position;
        Vector2 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

        Assert.IsNotNull(Mouse.current, "Setup Error: Missing Mouse device.");

        // Set the mouse position to the building
        Set(Mouse.current.position, screenPosition);
        yield return new WaitForSeconds(0.01f);

        // Simulate a mouse click
        Press(Mouse.current.leftButton);
        yield return new WaitForSeconds(0.01f);

        // Release the mouse click
        Release(Mouse.current.leftButton);
        yield return new WaitForSeconds(0.01f);

        // Check for the presence of building buttons upon clicking the building
        GameObject[] myButtons = GameObject.FindGameObjectsWithTag("BuildingButtonPopUp");
        yield return new WaitForSeconds(0.1f);
        foreach (GameObject myButton in myButtons)
            Assert.IsNotNull(myButton, $"Could not find button.");

        yield return null;
    }

    [UnityTest]
    public IEnumerator LabButtonsPopUpWhenBuildingIsClicked()
    {
        // Load the Main Scene
        SceneManager.LoadScene("MainScene");
        yield return new WaitForSeconds(0.1f);

        // Find a specific building in the scene (Trade Hut)
        GameObject Lab = GameObject.Find("Lab");
        Assert.IsNotNull(Lab, $"Could not find lab");

        // Set the main camera
        Camera mainCamera = Camera.main;
        Assert.IsNotNull(mainCamera, "Setup Error: Missing Main Camera.");

        // Find the position of the building in screen coordinates
        Vector3 worldPosition = Lab.transform.position;
        Vector2 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

        Assert.IsNotNull(Mouse.current, "Setup Error: Missing Mouse device.");

        // Set the mouse position to the building
        Set(Mouse.current.position, screenPosition);
        yield return new WaitForSeconds(0.01f);

        // Simulate a mouse click
        Press(Mouse.current.leftButton);
        yield return new WaitForSeconds(0.01f);

        // Release the mouse click
        Release(Mouse.current.leftButton);
        yield return new WaitForSeconds(0.01f);

        // Check for the presence of building buttons upon clicking the building
        GameObject[] myButtons = GameObject.FindGameObjectsWithTag("BuildingButtonPopUp");
        yield return new WaitForSeconds(0.1f);
        foreach (GameObject myButton in myButtons)
            Assert.IsNotNull(myButton, $"Could not find button.");

        yield return null;
    }
}
