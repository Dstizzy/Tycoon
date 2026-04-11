using UnityEngine;
using TMPro;
using System.ComponentModel;

public class EventUIController : MonoBehaviour
{
   [Header("UI References")]
   public TextMeshProUGUI scenarioText; // Text block where scenario is displayed
   public TextMeshProUGUI choiceAText; // Text block inside first choice button
   public TextMeshProUGUI choiceBText; // Text block inside second choice button

   [HideInInspector] public ExploreEvents currentEvent;

   // Populates the decision panel's text fields with data from the randomly pulled event
   public void SetEventPanel(ExploreEvents newEvent)
   {
      currentEvent = newEvent;
      scenarioText.text = currentEvent.description;
      choiceAText.text = currentEvent.choiceA.buttonText;
      choiceBText.text = currentEvent.choiceB.buttonText;
   }
}