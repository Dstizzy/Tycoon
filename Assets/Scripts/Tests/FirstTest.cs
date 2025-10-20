using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Search;
using UnityEditor.TestRunner;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestRunner;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using static UnityEngine.InputSystem.InputSystem;

[TestFixture]
public class FirstTest : InputTestFixture
{

[Test]
    public void IsMouseAdded()
    {
        Assert.IsNotNull(Mouse.current, "Setup Error: Missing Mouse device.");


    }
    [UnityTest]
    public IEnumerator FirstTestWithEnumeratorPasses()
    {
        SceneManager.LoadScene("MainScene");
        yield return new WaitForSeconds(0.1f);

        GameObject TradeHut = GameObject.Find("TradeHut");
        Assert.IsNotNull(TradeHut, $"Could not find trade hut");

        Camera mainCamera = Camera.main;
        Assert.IsNotNull(mainCamera, "Setup Error: Missing Main Camera.");

        Vector3 worldPosition = TradeHut.transform.position;
        Vector2 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

        Assert.IsNotNull(Mouse.current, "Setup Error: Missing Mouse device.");
        Set(Mouse.current.position, screenPosition);
        yield return new WaitForSeconds(0.1f);

        Press(Mouse.current.leftButton);
        yield return new WaitForSeconds(0.1f);

        Release(Mouse.current.leftButton);
        yield return new WaitForSeconds(0.1f);
        
        GameObject Button = GameObject.Find("ButtonCanvas(clone)");
        Assert.IsNotNull(Button, $"Could not find button.");
        /*Mouse mouse = Mouse.current;
        Assert.IsNotNull(mouse);
        Vector2 targetPosition = new Vector2(0, 0);
        Set(mouse.position, targetPosition);
        yield return null;
        Press(mouse.leftButton);
        yield return new WaitForSeconds(0.05f);
        Release(mouse.leftButton);*/
        yield return null;

    }
}
