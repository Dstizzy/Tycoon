using UnityEngine;

public class ButtonClickScript : MonoBehaviour
{
    public void OnFunctionButtonClick()
    {
        PopUpManager.Instance.ShowFunctionPanel();
    }
    public void OnInfoButtonClick()
    {
        PopUpManager.Instance.ShowInfoPanel();
    }

    public void OnUpgradeButtonClick()
    {
        PopUpManager.Instance.ShowUpgradePanel();
    }

}
