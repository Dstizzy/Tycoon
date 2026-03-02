using UnityEngine;
using System.Collections.Generic;

public class EventDatabase : MonoBehaviour
{
   [Header("Regions")]
   public List<ExploreEvents> shallowsEvents; // List of all events that could occur in depth level 1
   public List<ExploreEvents> middleEvents; // List of all events that could occur in depth level 2
   public List<ExploreEvents> deepEvents; // List of all events that could occur in depth level 3

   // Pulls a random node event from the proper list based on the ship's current depth
   public ExploreEvents GetRandomEvent(int region)
   {
      List<ExploreEvents> selectedPool = null; // Temporary list to hold pool of events to be pulled from

      // Check requested region and assign correct list to the temporary list
      switch(region)
      {
         case 1:
            selectedPool = shallowsEvents;
            break;
         case 2:
            selectedPool = middleEvents;
            break;
         case 3:
            selectedPool = deepEvents;
            break;
      }

      // Verify that valid, non-empty list was found
      if(selectedPool != null && selectedPool.Count > 0)
      {
         // Pick a random number from 0 to the total number of events in the list, and return that event
         int randomIndex = Random.Range(0, selectedPool.Count);
         return selectedPool[randomIndex];
      }

      Debug.LogWarning("No events were found for region");
      return null;
   }
}
