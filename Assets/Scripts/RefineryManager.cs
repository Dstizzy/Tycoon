using TMPro;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;

public class RefineryManager : MonoBehaviour
{
    public Transform refineryPanel;
    public Transform infoPanel;
    public Transform upgradePanel;

    public static RefineryManager Instance { get; private set; }

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

    public void RefineryButtonClick(int buttonId)
    {
        refineryPanel.gameObject.SetActive(false); 
        infoPanel.gameObject.SetActive(false);
        upgradePanel.gameObject.SetActive(false);

        switch (buttonId)
        {
            case 0:
                refineryPanel.gameObject.SetActive(true);
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
