using UnityEngine;
using UnityEngine.UI;

public class LabManager : MonoBehaviour
{
    [SerializeField] private Transform innovatePanel;
    [SerializeField] private Transform infoPanel;

    const int INNOVATE_BUTTON = 1;
    const int INFO_BUTTON = 2;
    const int UPGRADE_BUTTON = 3;

    private void Awake() {
        if (infoPanel == null) {
            Debug.LogError("Info Panel is not assigned in the Inspector!");
        } else {
            infoPanel.gameObject.SetActive(false);
        }

        if(innovatePanel == null) {
            Debug.LogError("Innovate Panel is not assigned");
        } else {
            innovatePanel.gameObject.SetActive(false);
        }
    }

    public void RequestLabPanel(int buttonID) {
        switch (buttonID) {
            case INNOVATE_BUTTON:
                ShowInnovatePanel();
                innovatePanel.transform.Find("ExitButton").GetComponent<Button>().onClick.AddListener(() => CloseLabPanel(INNOVATE_BUTTON));
                break;
            case INFO_BUTTON:
                ShowInfoPanel();
                infoPanel.transform.Find("ExitButton").GetComponent<Button>().onClick.AddListener(() => CloseLabPanel(INFO_BUTTON));
                break;
            case UPGRADE_BUTTON:
                Debug.Log("Building Panel: Info requested.");
                break;
            default:
                Debug.Log("Building Panel: Unknown button ID.");
                break;
        }
    }

    public void CloseLabPanel(int buttonID) {
        switch (buttonID) {
            case INNOVATE_BUTTON:
                CloseInnovatePanel();
                break;
            case INFO_BUTTON:
                CloseInfoPanel();
                break;
            case UPGRADE_BUTTON:
                Debug.Log("Building Panel: Info requested.");
                break;
            default:
                Debug.Log("Building Panel: Unknown button ID.");
                break;
        }
        PopUpManager.Instance.EnablePlayerInput();
    }
    private void ShowInnovatePanel() {
        innovatePanel.gameObject.SetActive(true);
    }
    private void ShowInfoPanel() {
        infoPanel.gameObject.SetActive(true);
    }
    
    private void CloseInnovatePanel() {
        innovatePanel.gameObject.SetActive(false);
    }
    private void CloseInfoPanel() {
        infoPanel.gameObject.SetActive(false);
    }
   
}
