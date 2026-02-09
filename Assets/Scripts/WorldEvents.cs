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

   public static string GetCrudeToolTickerMessage(int shiftDriection) 
   {
      string tickerMessage = "";

      switch (TurnManager.Instance.eventCountdown) 
      {
         case TURN_3:
            if(shiftDriection < 50)
               tickerMessage = "Water acidity levels rising. Strange red dust settling on outer hulls";
            else
               tickerMessage = "Scavengers spotted a massive wreck drifting down from the surface.";
            break;
         case TURN_4:
            if (shiftDriection < 50)
               tickerMessage = "Corrosion accelerating! Maintenance crews are overwhelmed by the Red Algae";
            else
               tickerMessage = "The wreck was a cargo hauler! High-quality steel flooding the black market.";
            break;
         case TURN_5:
            if(shiftDriection < 50)
               tickerMessage = "PLAGUE EVENT: The Rust is here! Crude Tools needed immediately!";
            else
               tickerMessage = "SURPLUS EVENT: Market flooded with salvage! Crude Tools worthless.";
            break;
         default:
            Debug.LogError("Unknown turn number");
            break;
      }

      return tickerMessage;
   }

   public static string GetHarpoonTickerMessage(int shiftDirection) 
   {
      string tickerMessage = "";

      switch (TurnManager.Instance.eventCountdown) 
      {
         case TURN_3:
            if (shiftDirection < 50)
               tickerMessage = "Sonar detects massive biological signatures in the deep sector...";
            else
               tickerMessage = "Biological scanners are silent. The deep trenches feel unusually empty.";
            break;
         case TURN_4:
            if (shiftDirection < 50)
               tickerMessage = "Forward scouts report shadow-shapes. Guild requests increased defense measures.";
            else
               tickerMessage = "City Guard reports zero attacks this week. Weapons ranges are quiet.";
            break;
         case TURN_5:
            if (shiftDirection < 50)
            tickerMessage = "MIGRATION EVENT: Leviathans breaching! Harpoon prices +300%!";
            else
               tickerMessage = "PEACE EVENT: The beasts are gone. Harpoon value crashed!";
            break;
         default:
            Debug.LogError("Unknown turn number");
            break;
      }

      return tickerMessage;
   }

   public static string GetPressureValveMessage(int shiftDirection) 
   {
      string tickerMessage = "";

      switch (TurnManager.Instance.eventCountdown) 
      {
         case TURN_3:
            if (shiftDirection < 50)
               tickerMessage = "Geothermal vents are fluctuating. Minor tremors felt in the lower districts";
           else
               tickerMessage = "Seismic activity at an all-time low. The vents are dormant.";
            break;
         case TURN_4:
            if (shiftDirection < 50)
               tickerMessage = "Seismic activity critical. Pipelines are bursting across the city";
            else
               tickerMessage = "Inspectors report 100% hull integrity. Maintenance backlog cleared.";
            break;
         case TURN_5:
            if (shiftDirection < 50)
               tickerMessage = "SEISMIC EVENT: Pressure spikes detected! Valve demand Tripled!";
            else
               tickerMessage = "STABILITY EVENT: Zero pressure incidents. Valve market dead.";
            break;
         default:
            Debug.LogError("Unknown turn number");
            break;
      }

      return tickerMessage;
   }

   public static string GetClockWorkEngineMessage(int shiftDirection) 
   {
      string tickerMessage = "";

      switch (TurnManager.Instance.eventCountdown) 
      {
         case TURN_3:
            if (shiftDirection < 50)
               tickerMessage = "Natural currents are slowing. Hydro-turbines losing efficiency.";
            else 
               tickerMessage = "Hydro-static sensors detect a warm current forming in the trade lane.";
            break;
         case TURN_4:
            if(shiftDirection < 50)
               tickerMessage = "The currents have died. Trade ships are drifting and requesting tow";
            else
               tickerMessage = "Hydro-static sensors detect a warm current forming in the trade lane.";
            break;
         case TURN_5:
            if(shiftDirection < 50)
               tickerMessage = "STAGNATION EVENT: Dead calm waters. Engine prices Doubled!";
            else
               tickerMessage = "FLOW EVENT: Free travel currents active. Engine demand plummeted.";
            break;
         default:
            Debug.LogError("Unknown turn number");
            break;
      }

      return tickerMessage;
   }
}