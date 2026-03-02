using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventUIController : MonoBehaviour
{
   [Header("UI References")]
   public TextMeshProUGUI scenarioText;
   public TextMeshProUGUI choiceAText;
   public TextMeshProUGUI choiceBText;
   public Button buttonA;
   public Button buttonB;

    public void SetEventPanel(ExploreEvents currentEvent)
   {
      scenarioText.text = currentEvent.description;
      choiceAText.text = currentEvent.choiceA.buttonText;
      choiceBText.text = currentEvent.choiceB.buttonText;
   }
}
