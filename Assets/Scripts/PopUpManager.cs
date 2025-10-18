using NUnit.Framework;

using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PopUpManager : MonoBehaviour
{
    public GameObject[] buildingButtonsPreFab;
    [SerializeField] private Camera cam;

    private List<GameObject> popUps;
    private PlayerActions playerActions;

    private TradeHutManager tradeHutManager;
    public static Transform buildingTransform;

    public static PopUpManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        playerActions = new PlayerActions();
        playerActions.PlayerInput.Enable();
        playerActions.PlayerInput.OnBuildingClick.performed += OnBuildingClick;
    }

    private void OnDestroy()
    {
        if (playerActions != null)
        {
            playerActions.PlayerInput.OnBuildingClick.performed -= OnBuildingClick;
            playerActions.PlayerInput.Disable();
            playerActions.Dispose();
        }
    }

    private void OnBuildingClick(InputAction.CallbackContext context)
    {

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

        Vector3 worldPos = cam.ScreenToWorldPoint(mouseScreenPos);

        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        worldPos.z = 0;

        if (hit.collider != null)
        {
            buildingTransform = hit.collider.transform;
            Vector3 offset = new Vector3(-6.0f, 3.0f, 0f);
            Vector3 fixedPopUpPosition = buildingTransform.position + offset;

            if (popUps == null)
            {
                popUps = new List<GameObject>();

                float buttonSpacing = 2.0f;

                foreach (GameObject button in buildingButtonsPreFab)
                {
                    if (buildingTransform.tag != "Lab" || popUps.Count < 2)
                    {
                        GameObject newButton = Instantiate(button, fixedPopUpPosition, Quaternion.identity);

                        popUps.Add(newButton);
                        switch (buildingTransform.tag)
                        {
                            case "Trade Hut":
                                if (popUps.Count == 1)
                                    newButton.GetComponentInChildren<ButtonsPopUp>().SetText("Trade");
                                else if (popUps.Count == 2)
                                    newButton.GetComponentInChildren<ButtonsPopUp>().SetText("Info");
                                else
                                    newButton.GetComponentInChildren<ButtonsPopUp>().SetText("Upgrade");
                                newButton.tag = "Trade Hut";
                                break;
                            default:
                                newButton.GetComponentInChildren<ButtonsPopUp>().SetText("Test");
                                break;
                        }
                        fixedPopUpPosition.y -= buttonSpacing;
                    }
                }
            }
            else
            {
                foreach (GameObject currentPopUp in popUps)
                {
                    if (currentPopUp != null)
                        Destroy(currentPopUp);
                }
                popUps = null;
            }
        }
        else
        {
            if(hit.collider != popUps[0] || hit.collider != popUps[1] || hit.collider != popUps[2])
            {

                foreach (GameObject currentPopUp in popUps)
                {
                    if (currentPopUp != null)
                        Destroy(currentPopUp);
                }
                popUps = null;
            }
        }

    }
    public void ShowFunctionPanel()
    {
        TradeHutManager.Instance.ShowTradePanel();
        switch (buildingTransform.tag)
        {
            case "Trade Hut":
                TradeHutManager.Instance.ShowTradePanel();
                TradeHutManager myManager = new TradeHutManager();
                myManager.ShowTradePanel();
                break;
            default:
                Debug.Log("No function assigned to this building.");
                break;
        }
    }
    public void ShowInfoPanel()
    {
    }

    public void ShowUpgradePanel()
    {
    }
}
