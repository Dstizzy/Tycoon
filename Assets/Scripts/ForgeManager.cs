using TMPro;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;

public class ForgeManager : MonoBehaviour
{
    public Transform forgePanel;
    public Transform infoPanel;
    public Transform upgradePanel;

    public static ForgeManager Instance { get; private set; }

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

    public void ForgeButtonClick(int buttonId)
    {
        forgePanel.gameObject.SetActive(false);
        infoPanel.gameObject.SetActive(false);
        upgradePanel.gameObject.SetActive(false);

        switch (buttonId)
        {
            case 0:
                forgePanel.gameObject.SetActive(true);
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
