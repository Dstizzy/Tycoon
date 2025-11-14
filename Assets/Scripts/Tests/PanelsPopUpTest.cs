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
public class PanelsPopUpTest : InputTestFixture
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

    [UnityTearDown] // Runs after *each* [UnityTest]
    public IEnumerator CleanupTestObjects()
    {
        // Find all the pop-up buttons that were likely left behind.
        GameObject[] popUpButtons = GameObject.FindGameObjectsWithTag("BuildingButtonPopUp");

        foreach (GameObject go in popUpButtons)
        {
            // Destroy them to ensure they aren't blocking the next test's input.
            UnityEngine.Object.Destroy(go);
        }

        // Allow one frame for the destruction to take effect
        yield return null;
    }

    [Test]
    public void IsMouseAdded()
    {
        Assert.IsNotNull(Mouse.current, "Setup Error: Missing Mouse device.");
    }

    [UnityTest]
    public IEnumerator AllPanelsPopUpUponClickingBuildingButton()
    {
        Button ExitButton = null;
        GameObject myPanel = null;
        Vector3 worldPosition = new Vector3(0, 0, 0);
        Vector2 screenPosition = new Vector2(0, 0);

        // Load the Main Scene
        SceneManager.LoadScene("MainScene");
        yield return new WaitForSeconds(0.1f);

        // Find a specific building in the scene (Trade Hut)
        GameObject myBuilding = GameObject.Find("TradeHut");
        Assert.IsNotNull(myBuilding, $"Could not find trade hut");

        // Set the main camera
        Camera mainCamera = Camera.main;
        Assert.IsNotNull(mainCamera, "Setup Error: Missing Main Camera.");

        // Loop through each of the three buttons: Trade, Upgrade, Info
        for (int buttonIndex = 0; buttonIndex < 3; buttonIndex++)
        {
            // Click a building
            worldPosition = myBuilding.transform.position;
            screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
            Set(Mouse.current.position, screenPosition);
            yield return null;
            Press(Mouse.current.leftButton);
            yield return null;
            Release(Mouse.current.leftButton);
            yield return null;

            // Check for the presence of building buttons upon clicking the building
            GameObject[] popUpButtons = GameObject.FindGameObjectsWithTag("BuildingButtonPopUp");
            yield return new WaitForSeconds(0.1f);
            Assert.IsNotEmpty(popUpButtons, "No building buttons found.");

            // Check if the panels open up for each button
            Button myButton = popUpButtons[buttonIndex].GetComponentInChildren<Button>();
            Assert.IsNotNull(myButton, $"Could not find button.");

            // Get the text of the button
            TextMeshProUGUI buttonTextComponent = myButton.GetComponentInChildren<TextMeshProUGUI>();
            Assert.IsNotNull(buttonTextComponent, "Could not find TextMeshProUGUI component on button.");
            Debug.Log($"Clicking button with text: {buttonTextComponent.text}");

            // Click a building button pop up
            worldPosition = myButton.transform.position;
            screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
            Set(Mouse.current.position, screenPosition);
            yield return null;
            Press(Mouse.current.leftButton);
            yield return null;
            Release(Mouse.current.leftButton);
            yield return null;

            // Verify the corresponding panel pops up
            switch (buttonTextComponent.text)
            {
                case "Trade":
                    myPanel = GameObject.Find("Building Panel Canvas/UI_TradeHut/TradePanel");
                    Assert.IsNotNull(myPanel, $"Could not find the Trade Panel.");
                    Assert.IsTrue(myPanel.activeSelf, "Trade Panel did not pop up.");
                    ExitButton = GameObject.Find($"Building Panel Canvas/UI_TradeHut/TradePanel/ExitButton").GetComponent<Button>();
                    Assert.IsNotNull(ExitButton, "Could not find Exit Button.");
                    break;
                case "Upgrade":
                    myPanel = GameObject.Find("Building Panel Canvas/UI_TradeHut/TradeUpgradePanel");
                    Assert.IsNotNull(myPanel, $"Could not find the Upgrade Panel.");
                    Assert.IsTrue(myPanel.activeSelf, "Upgrade Panel did not pop up.");
                    ExitButton = GameObject.Find($"Building Panel Canvas/UI_TradeHut/TradeUpgradePanel/CancelButton").GetComponent<Button>();
                    Assert.IsNotNull(ExitButton, "Could not find Exit Button.");
                    break;
                case "Info":
                    myPanel = GameObject.Find("Building Panel Canvas/UI_TradeHut/TradeInfoPanel");
                    Assert.IsNotNull(myPanel, $"Could not find the Info Panel.");
                    Assert.IsTrue(myPanel.activeSelf, "Info Panel did not pop up.");
                    ExitButton = GameObject.Find($"Building Panel Canvas/UI_TradeHut/TradeInfoPanel/ExitButton").GetComponent<Button>();
                    Assert.IsNotNull(ExitButton, "Could not find Exit Button.");
                    break;
                /* This occurs when there is a fourth button                                     */
                default:
                    Assert.Fail();
                    break;
            }
            ;

            yield return null;

            // Close the panel by clicking the exit button
            ExitButton.onClick.Invoke();

            // Verify the panel is closed
            Assert.IsFalse(myPanel.activeSelf, "Panel did not close after clicking Exit button.");
        }

        // Find a specific building in the scene (Trade Hut)
        myBuilding = GameObject.Find("Forge");
        Assert.IsNotNull(myBuilding, $"Could not find forge");

        // Loop through each of the three buttons: Trade, Upgrade, Info
        for (int buttonIndex = 0; buttonIndex < 3; buttonIndex++)
        {
            // Click a building
            worldPosition = myBuilding.transform.position;
            screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
            Set(Mouse.current.position, screenPosition);
            yield return null;
            Press(Mouse.current.leftButton);
            yield return null;
            Release(Mouse.current.leftButton);
            yield return null;

            // Check for the presence of building buttons upon clicking the building
            GameObject[] popUpButtons = GameObject.FindGameObjectsWithTag("BuildingButtonPopUp");
            yield return null;
            Assert.IsNotEmpty(popUpButtons, "No building buttons found.");

            // Check if the panels open up for each button
            Button myButton = popUpButtons[buttonIndex].GetComponentInChildren<Button>();
            Assert.IsNotNull(myButton, $"Could not find button.");

            // Get the text of the button
            TextMeshProUGUI buttonTextComponent = myButton.GetComponentInChildren<TextMeshProUGUI>();
            Assert.IsNotNull(buttonTextComponent, "Could not find TextMeshProUGUI component on button.");
            Debug.Log($"Clicking button with text: {buttonTextComponent.text}");

            // Click a building button pop up
            worldPosition = myButton.transform.position;
            screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
            Set(Mouse.current.position, screenPosition);
            yield return null;
            Press(Mouse.current.leftButton);
            yield return null;
            Release(Mouse.current.leftButton);
            yield return null;

            // Verify the corresponding panel pops up
            switch (buttonTextComponent.text)
            {
                case "Craft":
                    myPanel = GameObject.Find("Building Panel Canvas/UI_Forge/CraftPanel");
                    Assert.IsNotNull(myPanel, $"Could not find the Forge Panel.");
                    Assert.IsTrue(myPanel.activeSelf, "Craft Panel did not pop up.");
                    ExitButton = GameObject.Find($"Building Panel Canvas/UI_Forge/CraftPanel/ExitButton").GetComponent<Button>();
                    Assert.IsNotNull(ExitButton, "Could not find Exit Button.");
                    break;
                case "Upgrade":
                    myPanel = GameObject.Find("Building Panel Canvas/UI_Forge/ForgeUpgradePanel");
                    Assert.IsNotNull(myPanel, $"Could not find the Upgrade Panel.");
                    Assert.IsTrue(myPanel.activeSelf, "Upgrade Panel did not pop up.");
                    ExitButton = GameObject.Find($"Building Panel Canvas/UI_Forge/ForgeUpgradePanel/CancelButton").GetComponent<Button>();
                    Assert.IsNotNull(ExitButton, "Could not find Exit Button.");
                    break;
                case "Info":
                    myPanel = GameObject.Find("Building Panel Canvas/UI_Forge/ForgeInfoPanel");
                    Assert.IsNotNull(myPanel, $"Could not find the Info Panel.");
                    Assert.IsTrue(myPanel.activeSelf, "Info Panel did not pop up.");
                    ExitButton = GameObject.Find($"Building Panel Canvas/UI_Forge/ForgeInfoPanel/ExitButton").GetComponent<Button>();
                    Assert.IsNotNull(ExitButton, "Could not find Exit Button.");
                    break;
                default:
                    Assert.Fail();
                    break;
            }
            ;

            yield return null;

            // Close the panel by clicking the exit button
            ExitButton.onClick.Invoke();

            // Verify the panel is closed
            Assert.IsFalse(myPanel.activeSelf, "Panel did not close after clicking Exit button.");
        }

        // Find a specific building in the scene (Trade Hut)
        myBuilding = GameObject.Find("Ore Refinery");
        Assert.IsNotNull(myBuilding, $"Could not find ore refinery");

        // Loop through each of the three buttons: Trade, Upgrade, Info
        for (int buttonIndex = 0; buttonIndex < 3; buttonIndex++)
        {
            // Click a building
            worldPosition = myBuilding.transform.position;
            screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
            Set(Mouse.current.position, screenPosition);
            yield return null;
            Press(Mouse.current.leftButton);
            yield return null;
            Release(Mouse.current.leftButton);
            yield return null;

            // Check for the presence of building buttons upon clicking the building
            GameObject[] popUpButtons = GameObject.FindGameObjectsWithTag("BuildingButtonPopUp");
            yield return new WaitForSeconds(0.1f);
            Assert.IsNotEmpty(popUpButtons, "No building buttons found.");

            // Check if the panels open up for each button
            Button myButton = popUpButtons[buttonIndex].GetComponentInChildren<Button>();
            Assert.IsNotNull(myButton, $"Could not find button.");

            // Get the text of the button
            TextMeshProUGUI buttonTextComponent = myButton.GetComponentInChildren<TextMeshProUGUI>();
            Assert.IsNotNull(buttonTextComponent, "Could not find TextMeshProUGUI component on button.");
            Debug.Log($"Clicking button with text: {buttonTextComponent.text}");

            // Click a building button pop up
            worldPosition = myButton.transform.position;
            screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
            Set(Mouse.current.position, screenPosition);
            yield return null;
            Press(Mouse.current.leftButton);
            yield return null;
            Release(Mouse.current.leftButton);
            yield return null;

            // Verify the corresponding panel pops up
            switch (buttonTextComponent.text)
            {
                case "Upgrade":
                    myPanel = GameObject.Find("Building Panel Canvas/UI_OreRefinery/OreUpgradePanel");
                    Assert.IsNotNull(myPanel, $"Could not find the Upgrade Panel.");
                    Assert.IsTrue(myPanel.activeSelf, "Upgrade Panel did not pop up.");
                    ExitButton = GameObject.Find($"Building Panel Canvas/UI_OreRefinery/OreUpgradePanel/CancelButton").GetComponent<Button>();
                    Assert.IsNotNull(ExitButton, "Could not find Exit Button.");
                    break;
                case "Info":
                    myPanel = GameObject.Find("Building Panel Canvas/UI_OreRefinery/OreInfoPanel");
                    Assert.IsNotNull(myPanel, $"Could not find the Info Panel.");
                    Assert.IsTrue(myPanel.activeSelf, "Info Panel did not pop up.");
                    ExitButton = GameObject.Find($"Building Panel Canvas/UI_OreRefinery/OreInfoPanel/ExitButton").GetComponent<Button>();
                    Assert.IsNotNull(ExitButton, "Could not find Exit Button.");
                    break;
                default:
                    Debug.Log("Ore Refinery does not need a refine button");
                    break;
            }
            ;
            yield return null;

            // Close the panel by clicking the exit button
            if (ExitButton != null)
                ExitButton.onClick.Invoke();

            // Verify the panel is closed
            if (myPanel != null)
                Assert.IsFalse(myPanel.activeSelf, "Panel did not close after clicking Exit button.");
        }

        // Find a specific building in the scene (Trade Hut)
        myBuilding = GameObject.Find("Exploration");
        Assert.IsNotNull(myBuilding, $"Could not find exploration unit");

        // Loop through each of the three buttons: Trade, Upgrade, Info
        for (int buttonIndex = 0; buttonIndex < 3; buttonIndex++)
        {
            // Click a building
            worldPosition = myBuilding.transform.position;
            screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
            Set(Mouse.current.position, screenPosition);
            yield return null;
            Press(Mouse.current.leftButton);
            yield return null;
            Release(Mouse.current.leftButton);
            yield return null;

            // Check for the presence of building buttons upon clicking the building
            GameObject[] popUpButtons = GameObject.FindGameObjectsWithTag("BuildingButtonPopUp");
            yield return new WaitForSeconds(0.1f);
            Assert.IsNotEmpty(popUpButtons, "No building buttons found.");

            // Check if the panels open up for each button
            Button myButton = popUpButtons[buttonIndex].GetComponentInChildren<Button>();
            Assert.IsNotNull(myButton, $"Could not find button.");

            // Get the text of the button
            TextMeshProUGUI buttonTextComponent = myButton.GetComponentInChildren<TextMeshProUGUI>();
            Assert.IsNotNull(buttonTextComponent, "Could not find TextMeshProUGUI component on button.");
            Debug.Log($"Clicking button with text: {buttonTextComponent.text}");

            // Click a building button pop up
            worldPosition = myButton.transform.position;
            screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
            Set(Mouse.current.position, screenPosition);
            yield return null;
            Press(Mouse.current.leftButton);
            yield return null;
            Release(Mouse.current.leftButton);
            yield return null;

            // Verify the corresponding panel pops up
            switch (buttonTextComponent.text)
            {
                case "Explore":
                    myPanel = GameObject.Find("Building Panel Canvas/UI_ExplorationUnit/ExplorePanel");
                    Assert.IsNotNull(myPanel, $"Could not find the Trade Panel.");
                    Assert.IsTrue(myPanel.activeSelf, "Trade Panel did not pop up.");
                    ExitButton = GameObject.Find($"Building Panel Canvas/UI_ExplorationUnit/ExplorePanel/ExitButton").GetComponent<Button>();
                    Assert.IsNotNull(ExitButton, "Could not find Exit Button.");
                    break;
                case "Upgrade":
                    myPanel = GameObject.Find("Building Panel Canvas/UI_ExplorationUnit/ExploreUpgradePanel");
                    Assert.IsNotNull(myPanel, $"Could not find the Upgrade Panel.");
                    Assert.IsTrue(myPanel.activeSelf, "Upgrade Panel did not pop up.");
                    ExitButton = GameObject.Find($"Building Panel Canvas/UI_ExplorationUnit/ExploreUpgradePanel/CancelButton").GetComponent<Button>();
                    Assert.IsNotNull(ExitButton, "Could not find Exit Button.");
                    break;
                case "Info":
                    myPanel = GameObject.Find("Building Panel Canvas/UI_ExplorationUnit/ExploreInfoPanel");
                    Assert.IsNotNull(myPanel, $"Could not find the Info Panel.");
                    Assert.IsTrue(myPanel.activeSelf, "Info Panel did not pop up.");
                    ExitButton = GameObject.Find($"Building Panel Canvas/UI_ExplorationUnit/ExploreInfoPanel/ExitButton").GetComponent<Button>();
                    Assert.IsNotNull(ExitButton, "Could not find Exit Button.");
                    break;
                default:
                    Assert.Fail();
                    break;
            }
            ;

            yield return null;

            // Close the panel by clicking the exit button
            ExitButton.onClick.Invoke();

            // Verify the panel is closed
            Assert.IsFalse(myPanel.activeSelf, "Panel did not close after clicking Exit button.");
        }

        // Find a specific building in the scene (Trade Hut)
        myBuilding = GameObject.Find("Lab");
        Assert.IsNotNull(myBuilding, $"Could not find lab");

        // Loop through each of the three buttons: Trade, Upgrade, Info
        for (int buttonIndex = 0; buttonIndex < 2; buttonIndex++)
        {
            // Click a building
            worldPosition = myBuilding.transform.position;
            screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
            Set(Mouse.current.position, screenPosition);
            yield return null;
            Press(Mouse.current.leftButton);
            yield return null;
            Release(Mouse.current.leftButton);
            yield return null;

            // Check for the presence of building buttons upon clicking the building
            GameObject[] popUpButtons = GameObject.FindGameObjectsWithTag("BuildingButtonPopUp");
            yield return new WaitForSeconds(0.1f);
            Assert.IsNotEmpty(popUpButtons, "No building buttons found.");

            // Check if the panels open up for each button
            Button myButton = popUpButtons[buttonIndex].GetComponentInChildren<Button>();
            Assert.IsNotNull(myButton, $"Could not find button.");

            // Get the text of the button
            TextMeshProUGUI buttonTextComponent = myButton.GetComponentInChildren<TextMeshProUGUI>();
            Assert.IsNotNull(buttonTextComponent, "Could not find TextMeshProUGUI component on button.");
            Debug.Log($"Clicking button with text: {buttonTextComponent.text}");

            // Click a building button pop up
            worldPosition = myButton.transform.position;
            screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
            Set(Mouse.current.position, screenPosition);
            yield return null;
            Press(Mouse.current.leftButton);
            yield return null;
            Release(Mouse.current.leftButton);
            yield return null;

            // Verify the corresponding panel pops up
            switch (buttonTextComponent.text)
            {
                case "Research":
                    myPanel = GameObject.Find("Building Panel Canvas/UI_Lab/InnovatePanel");
                    Assert.IsNotNull(myPanel, $"Could not find the Innovate Panel.");
                    Assert.IsTrue(myPanel.activeSelf, "Innovate panel did not pop up.");
                    ExitButton = GameObject.Find($"Building Panel Canvas/UI_Lab/InnovatePanel/ExitButton").GetComponent<Button>();
                    Assert.IsNotNull(ExitButton, "Could not find Exit Button.");
                    break;
                case "Info":
                    myPanel = GameObject.Find("Building Panel Canvas/UI_Lab/LabInfoPanel");
                    Assert.IsNotNull(myPanel, $"Could not find the Info Panel.");
                    Assert.IsTrue(myPanel.activeSelf, "Info Panel did not pop up.");
                    ExitButton = GameObject.Find($"Building Panel Canvas/UI_Lab/LabInfoPanel/ExitButton").GetComponent<Button>();
                    Assert.IsNotNull(ExitButton, "Could not find Exit Button.");
                    break;
                default:
                    Assert.Fail();
                    break;
            }
            ;

            yield return null;

            // Close the panel by clicking the exit button
            ExitButton.onClick.Invoke();

            // Verify the panel is closed
            Assert.IsFalse(myPanel.activeSelf, "Panel did not close after clicking Exit button.");
        }

        yield return null;
    }
}