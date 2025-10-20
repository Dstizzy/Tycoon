using TMPro;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;

public class ExplorationUnitManager : MonoBehaviour
{
    public Transform explorePanel;
    public Transform infoPanel;
    public Transform upgradePanel;

    public static ExplorationUnitManager Instance { get; private set; }

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
    }

    private void Start()
    {

    }

    public void ExplorationButtonClick(int buttonId)
    {
        explorePanel.gameObject.SetActive(false);
        infoPanel.gameObject.SetActive(false);
        upgradePanel.gameObject.SetActive(false);

        switch (buttonId)
        {
            case 0:
                explorePanel.gameObject.SetActive(true);
                break;
            case 1:
                infoPanel.gameObject.SetActive(true);
                break;
            case 2:
                upgradePanel.gameObject.SetActive(true);
                break;
            default:
                Debug.LogWarning("Invalid button ID received");
                break;
        }
    }
}
