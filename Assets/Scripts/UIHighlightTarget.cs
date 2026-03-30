using System;
using System.Reflection;
using UnityEngine;

// Controls UIEffect highlight effects used during the narrative tutorial.
// Assign the original UI GameObjects. Only the attached Coffee.UIEffects.UIEffect
// components are toggled, while the original UI visuals remain untouched.
public class UIHighlightTarget : MonoBehaviour
{
   [SerializeField] private GameObject[] highlightTargets;

   private static Type cachedUIEffectType;
   private static bool didSearchUIEffectType;
   private static bool hasLoggedMissingUIEffectType;

   private void Awake()
   {
      HideHighlight();
   }

   public void ShowHighlight()
   {
      SetHighlightState(true);
   }

   public void HideHighlight()
   {
      SetHighlightState(false);
   }

   private void SetHighlightState(bool isVisible)
   {
      Type uiEffectType = GetUIEffectType();
      if (uiEffectType == null)
      {
         if (!hasLoggedMissingUIEffectType)
         {
            Debug.LogWarning("[UIHighlightTarget] Coffee.UIEffects.UIEffect type was not found.");
            hasLoggedMissingUIEffectType = true;
         }

         return;
      }

      if (highlightTargets == null)
         return;

      foreach (GameObject currentTarget in highlightTargets)
      {
         if (currentTarget == null)
            continue;

         Component[] currentEffects = currentTarget.GetComponents(uiEffectType);
         foreach (Component currentEffect in currentEffects)
         {
            if (currentEffect is Behaviour currentBehaviour)
               currentBehaviour.enabled = isVisible;
         }
      }
   }

   private static Type GetUIEffectType()
   {
      if (didSearchUIEffectType)
         return cachedUIEffectType;

      didSearchUIEffectType = true;

      Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
      foreach (Assembly currentAssembly in assemblies)
      {
         Type currentType = currentAssembly.GetType("Coffee.UIEffects.UIEffect");
         if (currentType != null)
         {
            cachedUIEffectType = currentType;
            return cachedUIEffectType;
         }
      }

      return null;
   }
}