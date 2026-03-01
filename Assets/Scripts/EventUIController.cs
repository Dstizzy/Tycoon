using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventUIController : MonoBehaviour
{
   [Header("UI References")]
   public TextMeshProUGUI scenarioText; // Text block where scenario is displayed
   public TextMeshProUGUI choiceAText; // Text block inside first choice button
   public TextMeshProUGUI choiceBText; // Text block inside second choice button

   // Populates the decision panel's text fields with data from the randomly pulled event
    public void SetEventPanel(ExploreEvents currentEvent)
   {
      scenarioText.text = currentEvent.description;
      choiceAText.text = currentEvent.choiceA.buttonText;
      choiceBText.text = currentEvent.choiceB.buttonText;
   }
}
