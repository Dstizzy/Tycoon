using UnityEngine;

public class WorldEvents {

   const int TURN_3 = 3,
             TURN_4 = 4,
             TURN_5 = 5;

  public const int WORLD_EVENT_RESET_PREVIEW_TURN = 0,
                   WORLD_EVENT_RESET_TURN         = 1,
                   WORLD_EVENT_PREVIEW_TURN       = 4,
                   WORLD_EVENT_ACTIVE_TURN        = 5;
   public enum WorldEventTypes {
      CrudeToolEvent,
      HarpoonEvent,
      DivingBellEvent,
      IndustrialGoldRushEvent,
      PressureValveEvent,
      DeepSeaWarEvent,
      PrecisionLensEvent,
      ClockworkEngineEvent,
      ScavengersHolidayEvent,
   }

   public static string GetCrudeToolTickerMessage(int shiftDriection) 
   {
      string tickerMessage = "";

      switch (TurnManager.Instance.eventCountdown) 
      {
         case TURN_3:
            if(shiftDriection <= 50)
               tickerMessage = "Water acidity levels rising. Strange red dust settling on outer hulls";
            else
               tickerMessage = "Scavengers spotted a massive wreck drifting down from the surface.";
            break;
         case TURN_4:
            if (shiftDriection <= 50)
               tickerMessage = "Corrosion accelerating! Maintenance crews are overwhelmed by the Red Algae";
            else
               tickerMessage = "The wreck was a cargo hauler! High-quality steel flooding the black market.";
            break;
         case TURN_5:
            if(shiftDriection <= 50)
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
            if (shiftDirection <= 50)
               tickerMessage = "Sonar detects massive biological signatures in the deep sector...";
            else
               tickerMessage = "Biological scanners are silent. The deep trenches feel unusually empty.";
            break;
         case TURN_4:
            if (shiftDirection <= 50)
               tickerMessage = "Forward scouts report shadow-shapes. Guild requests increased defense measures.";
            else
               tickerMessage = "City Guard reports zero attacks this week. Weapons ranges are quiet.";
            break;
         case TURN_5:
            if (shiftDirection <= 50)
               tickerMessage = "MIGRATION EVENT: Leviathans breaching! Harpoon prices Doubled!";
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
            if (shiftDirection <= 50)
               tickerMessage = "Geothermal vents are fluctuating. Minor tremors felt in the lower districts";
            else
               tickerMessage = "Seismic activity at an all-time low. The vents are dormant.";
            break;
         case TURN_4:
            if (shiftDirection <= 50)
               tickerMessage = "Seismic activity critical. Pipelines are bursting across the city";
            else
               tickerMessage = "Inspectors report 100% hull integrity. Maintenance backlog cleared.";
            break;
         case TURN_5:
            if (shiftDirection <= 50)
               tickerMessage = "SEISMIC EVENT: Pressure spikes detected! Valve demand Doubled!";
            else
               tickerMessage = "STABILITY EVENT: Zero pressure incidents. Valve market dead.";
            break;
         default:
            Debug.LogError("Unknown turn number");
            break;
      }

      return tickerMessage;
   }

   public static string GetDivingBellTickerMessage(int shiftDirection)
   {
      string tickerMessage = "";

      switch (TurnManager.Instance.eventCountdown)
      {
         case TURN_3:
            if (shiftDirection <= 50)
               tickerMessage = "New trench openings reported — survey crews request additional submersibles.";
            else
               tickerMessage = "Survey lanes are clear. No new deployment orders from exploration teams.";
            break;
         case TURN_4:
            if (shiftDirection <= 50)
               tickerMessage = "Deep caverns discovered: Exploration units mobilizing for salvage and mapping.";
            else
               tickerMessage = "Calm seas around survey sites. Explorers delay deployments until next window.";
            break;
         case TURN_5:
            if (shiftDirection <= 50)
               tickerMessage = "DEPLOYMENT EVENT: Exploration surge! Diving Bell demand Doubled!";
            else
               tickerMessage = "LULL EVENT: Survey work on hold. Diving Bell prices slump.";
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
            if (shiftDirection <= 50)
               tickerMessage = "Natural currents are slowing. Hydro-turbines losing efficiency.";
            else 
               tickerMessage = "Hydro-static sensors detect a warm current forming in the trade lane.";
            break;
         case TURN_4:
            if(shiftDirection <= 50)
               tickerMessage = "The currents have died. Trade ships are drifting and requesting tow";
            else
               tickerMessage = "Hydro-static sensors detect a warm current forming in the trade lane.";
            break;
         case TURN_5:
            if(shiftDirection <= 50)
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

   public static string GetPrecisionLensTickerMessage(int shiftDirection)
   {
      string tickerMessage = "";

      switch (TurnManager.Instance.eventCountdown)
      {
         case TURN_3:
            if (shiftDirection <= 50)
               tickerMessage = "Optics workshops report increased demand for fine components.";
            else
               tickerMessage = "Precision work quiet — instrument orders remain steady.";
            break;
         case TURN_4:
            if (shiftDirection <= 50)
               tickerMessage = "Research labs announce a high-precision initiative; parts requisitions rising.";
            else
               tickerMessage = "Calibration schedules cleared — no urgent optics orders.";
            break;
         case TURN_5:
            if (shiftDirection <= 50)
               tickerMessage = "BREAKTHROUGH EVENT: Precision Lens demand Doubled for scientific programs!";
            else
               tickerMessage = "OVERCAPACITY EVENT: Surplus optics available. Precision Lens prices fall.";
            break;
         default:
            Debug.LogError("Unknown turn number");
            break;
      }

      return tickerMessage;
   }

   public static string GetDeepSeaWarTickerMessage()
   {
      string tickerMessage = "";

      switch (TurnManager.Instance.eventCountdown)
      {
         case TURN_3:
             tickerMessage = "Tensions rising among the deep-sea factions. Whispers of a coming conflict.";
            break;
         case TURN_4:
             tickerMessage = "Military vessels are mobilizing. The noise is drawing unwanted attention from the deep.";
            break;
         case TURN_5:
            tickerMessage = "WAR EVENT: The Deep-Sea War begins! Harpoons jump 3x, Kraken Heat +5/turn!";
            break;
         default:
            Debug.LogError("Unknown turn number");
            break;
      }

      return tickerMessage;
   }

   public static string GetScavengersHolidayTickerMessage()
   {
      string tickerMessage = "";

      switch (TurnManager.Instance.eventCountdown)
      {
         case TURN_3:
            tickerMessage = "Currents are shifting favorably. Navigation routes are clearing up.";
            break;
         case TURN_4:
            tickerMessage = "A massive seasonal calm settles over the ocean. Explorers prepare for a frenzy.";
            break;
         case TURN_5:
            tickerMessage = "HOLIDAY EVENT: Scavenger's Holiday! Exploration costs 0 fuel, all prices drop 25%!";
            break;
         default:
            Debug.LogError("Unknown turn number");
            break;
      }

      return tickerMessage;
   }

   public static string GetIndustrialGoldRushTickerMessage()
   {
      string tickerMessage = "";

      switch (TurnManager.Instance.eventCountdown)
      {
         case TURN_3:
               tickerMessage = "Factories report a critical shortage of basic components.";
            break;
         case TURN_4:
               tickerMessage = "Production lines are desperate! Foremen are authorizing premium payouts for basic gear.";
            break;
         case TURN_5:
               tickerMessage = "GOLD RUSH EVENT: Industrial panic! Tier 1 items now sell for Tier 2 prices!";
            break;
         default:
            Debug.LogError("Unknown turn number");
            break;
      }

      return tickerMessage;
   }

   public static void WorldEventSideEffect(WorldEventTypes worldEvent) 
   {
      switch (worldEvent) 
      {
         case WorldEventTypes.ScavengersHolidayEvent:

            break;
         case WorldEventTypes.DeepSeaWarEvent:
            TurnManager.Instance.HandleHeat();
            TurnManager.Instance.HandleHeat();
            break;
      }
   }
}