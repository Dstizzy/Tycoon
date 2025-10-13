using NUnit.Framework;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.UIElements;


public class ButtonsPopUpTest
{
    private PopUpManager popUpManager;
    private GameObject tradeHut;
    private Camera testCamera;
    private GameObject[] buttonPrefabs;

    // Change the field initializer to use a constant or assign in SetUp
    private Vector2 TEST_MOUSE_SCREEN_POS;
    [SetUp]
    public IEnumerator SetUp()
    {
        SceneManager.LoadScene("MainScene");
        yield return null;
       
        GameObject camObject = new GameObject("Main Camera");
        testCamera = camObject.AddComponent<Camera>();
        testCamera.orthographic = true; // Assuming 2D/orthographic
        testCamera.tag = "MainCamera";

        testCamera.transform.position = new Vector2(0, 0);

        GameObject managerObject = new GameObject("PopUpManager");
        popUpManager = managerObject.AddComponent<PopUpManager>();

        GameObject buttonPreFab = new GameObject("ButtonPreFab");
        buttonPreFab.AddComponent<BoxCollider2D>();
        buttonPreFab.AddComponent<ButtonsPopUp>();
        buttonPrefabs = new GameObject[] { buttonPreFab, buttonPreFab, buttonPreFab };
        popUpManager.buildingButtonsPreFab = buttonPrefabs;

        tradeHut = new GameObject("Trade Hut");
        tradeHut.tag = "Trade Hut";
        BoxCollider2D collider = tradeHut.AddComponent<BoxCollider2D>();

        Vector3 tradeHutPos = testCamera.ScreenToWorldPoint(TEST_MOUSE_SCREEN_POS);
        tradeHutPos.z = 0;
        tradeHut.transform.position = tradeHutPos;

        yield return null;
    }
    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(tradeHut);
        Object.DestroyImmediate(popUpManager.gameObject);
        Object.DestroyImmediate(testCamera.gameObject);
        foreach (var prefab in buttonPrefabs)
        {
            Object.DestroyImmediate(prefab);
        }

    }

    [Test]
    public void popUpsAreHiddenOnStart()
    {
        TEST_MOUSE_SCREEN_POS = new Vector2(0, 0);
        var mouse = InputSystem.AddDevice<Mouse>();

        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        InputSystem.QueueStateEvent(mouse, new MouseState { position = TEST_MOUSE_SCREEN_POS });

        // Simulate pressing the left mouse button
        InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 << (int)Unity.VisualScripting.MouseButton.Left });

        // Simulate releasing the left mouse button
        InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 0 });

        // You can then add assertions here to verify the expected behavior
        // For example, check if a UI element was clicked or a game object reacted
        // Assert.IsTrue(someExpectedCondition);

        // Remove the test device
        InputSystem.RemoveDevice(mouse);
        /*int initialPopUpCount = 0;
        Assert.AreEqual(0, initialPopUpCount, "Initial pop-up count must be zero.");
        InputSystem.
        mouse.Setup.WarpCursorPosition(TEST_MOUSE_SCREEN_POS);
        mouse.current.WarpCursorPosition(TEST_MOUSE_SCREEN_POS);
        InputSystem.QueueStateEvent(Mouse.current, new MouseState

        Mouse.current.WarpCursorPosition(TEST_MOUSE_SCREEN_POS);
        InputSystem.QueueStateEvent(Mouse.current, new MouseState { position = TEST_MOUSE_SCREEN_POS, buttons = 1 << (int)MouseButton.Left });
        InputSystem.Update();
        // Arrange
        GameObject buttonPrefab = new GameObject();
        buttonPrefab.AddComponent<ButtonsPopUp>();
        Camera cam = new GameObject().AddComponent<Camera>();
        PopUpManager popUpManager = new GameObject().AddComponent<PopUpManager>();
        popUpManager.cam = cam;
        popUpManager.buildingButtonsPreFab = new GameObject[] { buttonPrefab };
        // Act
        // Simulate building click
        // Note: In a real test, you would simulate the input action here
        // Assert
        // Check if buttons are created
        Assert.IsNotNull(popUpManager);
        // Further assertions would be needed to verify buttons are instantiated correctly*/

    }
}