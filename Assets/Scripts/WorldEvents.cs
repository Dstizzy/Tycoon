using Unity.VisualScripting;

using UnityEngine;

public class WorldEvents {

   const int TURN_3 = 3,
             TURN_4 = 4,
             TURN_5 = 5;

   public enum WorldEventTypes {
      CrudeToolEvent,
      HarpoonEvent,
      PressureValveEvent,
      ClockworkEngineEvent
   }

   public static string GetCrudeToolTickerMessage() 
   {
      string tickerMessage = "";

      switch (TurnManager.Instance.eventCountdown) 
      {
         case TURN_3:
            tickerMessage = "Water acidity levels rising. Strange red dust settling on outer hulls";
            break;
         case TURN_4:
            tickerMessage = "Corrosion accelerating! Maintenance crews are overwhelmed by the Red Algae";
            break;
         case TURN_5:
            tickerMessage = "PLAGUE EVENT: The Rust is here! Crude Tools needed immediately!";
            break;
         default:
            Debug.LogError("Unknown turn number");
            break;
      }

      return tickerMessage;
   }

   public static string GetHarpoonTickerMessage() 
   {
      string tickerMessage = "";

      switch (TurnManager.Instance.currentTurn) 
      {
         case TURN_3:
            tickerMessage = "Sonar detects massive biological signatures in the deep sector...";
            break;
         case TURN_4:
            tickerMessage = "Forward scouts report shadow-shapes. Guild requests increased defense measures.";
            break;
         case TURN_5:
            tickerMessage = "MIGRATION EVENT: Leviathans breaching! Harpoon prices +300%!";
            break;
         default:
            Debug.LogError("Unknown turn number");
            break;
      }

      return tickerMessage;
   }
   public static string GetPressureValveMessage() 
   {
      string tickerMessage = "";

      switch (TurnManager.Instance.currentTurn) 
      {
         case TURN_3:
            tickerMessage = "Geothermal vents are fluctuating. Minor tremors felt in the lower districts";
            break;
         case TURN_4:
            tickerMessage = "Seismic activity critical. Pipelines are bursting across the city";
            break;
         case TURN_5:
            tickerMessage = "SEISMIC EVENT: Pressure spikes detected! Valve demand Tripled!";
            break;
         default:
            Debug.LogError("Unknown turn number");
            break;
      }

      return tickerMessage;
   }
   public static string GetClockWorkEngineMessage() 
   {
      string tickerMessage = "";

      switch (TurnManager.Instance.currentTurn) 
      {
         case TURN_3:
            tickerMessage = "Natural currents are slowing. Hydro-turbines losing efficiency.";
            break;
         case TURN_4:
            tickerMessage = "The currents have died. Trade ships are drifting and requesting tow";
            break;
         case TURN_5:
            tickerMessage = "STAGNATION EVENT: Dead calm waters. Engine prices Doubled!";
            break;
         default:
            Debug.LogError("Unknown turn number");
            break;
      }

      return tickerMessage;
   }
}