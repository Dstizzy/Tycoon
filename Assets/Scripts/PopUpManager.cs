using NUnit.Framework;

using System.Collections.Generic;
using TMPro;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PopUpManager : MonoBehaviour
{
    public GameObject[] buildingButtonsPreFab;
    [SerializeField] private Camera cam;

    private List<GameObject> popUps;
    private PlayerActions playerActions;
    private Transform lastClickedBuilding;

    private TradeHutManager tradeHutManager;
    public static Transform buildingTransform;
    public TextMeshProUGUI TradeHutLevel;

    private void Awake()
    {
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
            lastClickedBuilding = hit.collider.transform;
            Transform buildingTransform = lastClickedBuilding;
            Vector3 offset = new Vector3(-6.0f, 3.0f, 0f);
            Vector3 fixedPopUpPosition = buildingTransform.position + offset;

            if (popUps != null)
            {
                foreach (GameObject currentPopUp in popUps)
                {
                    if (currentPopUp != null)
                        Destroy(currentPopUp);
                }
                popUps = null;
                return;
            }

            popUps = new List<GameObject>();
            float buttonSpacing = 2.0f;

            int buttonCount = (buildingTransform.CompareTag("Lab")) ? 2 : 3;

            for (int i = 0; i < buttonCount && i < buildingButtonsPreFab.Length; i++)
            {
                GameObject buttonPreFab = buildingButtonsPreFab[i];
                GameObject newButton = Instantiate(buttonPreFab, fixedPopUpPosition, Quaternion.identity);
                popUps.Add(newButton);

                string buttonText = i switch
                {
                    0 => "Trade",
                    1 => "Info",
                    2 => "Upgrade",
                    _ => "Button"
                };

                ButtonsPopUp buttonComponent = newButton.GetComponentInChildren<ButtonsPopUp>();
                buttonComponent.SetText(buttonText);

                int capturedId = i;
                Button uiButton = newButton.GetComponentInChildren<Button>();
                if (uiButton != null)
                {
                    uiButton.onClick.RemoveAllListeners();
                    uiButton.onClick.AddListener(() => OnBuildingButtonClick(capturedId));
                }
                else
                    Debug.LogWarning("button was null");
                fixedPopUpPosition.y -= buttonSpacing;
            }
        }

        if (TradeHutLevel.gameObject.activeInHierarchy)
        {
            TradeHutLevel.text = "Hello";
            TradeHutLevel.text = $"lvl. {tradeHutManager.GetTradeHutLevel()}";
            TradeHutLevel.gameObject.SetActive(false);
        }
        else
        {
            TradeHutLevel.gameObject.SetActive(true);
        }
    }

    public void OnBuildingButtonClick(int buttonId)
    {
        if(lastClickedBuilding == null)
        {
            Debug.LogWarning("No building was selected before clicking a button");
            return;
        }

        string buildingTag = lastClickedBuilding.tag;
        switch (buildingTag)
        {
            case "Trade Hut":
                tradeHutManager = TradeHutManager.Instance;
                tradeHutManager.TradeHutButtonClick(buttonId);
                break;
            default:
                Debug.LogWarning($"Unknown building tag: {buildingTag}");
                break;
        }
    }
}
