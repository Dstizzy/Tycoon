using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class EventDatabase : MonoBehaviour
{
   [Header("Regions")]
   public List<ExploreEvents> shallowsEvents;
   public List<ExploreEvents> middleEvents;
   public List<ExploreEvents> deepEvents;

   public ExploreEvents GetRandomEvent(int region)
   {
      List<ExploreEvents> selectedPool = null;

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

      if(selectedPool != null && selectedPool.Count > 0)
      {
         int randomIndex = Random.Range(0, selectedPool.Count);
         return selectedPool[randomIndex];
      }

      Debug.LogWarning("No events were found for region");
      return null;
   }
}
