using TMPro;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;

public class LabManager : MonoBehaviour
{
    public Transform LabPanel;
    public Transform infoPanel;

    public static LabManager Instance { get; private set; }

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

    public void LabButtonClick(int buttonId)
    {
        LabPanel.gameObject.SetActive(false);
        infoPanel.gameObject.SetActive(false);

        switch (buttonId)
        {
            case 0:
                LabPanel.gameObject.SetActive(true);
                break;
            case 1:
                infoPanel.gameObject.SetActive(true);
                break;
            default:
                Debug.LogWarning("Invalid button ID received");
                break;
        }
    }
}
