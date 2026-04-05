using UnityEngine;

public class RealTimeGear : MonoBehaviour
{
   [Range(-500f, 500f)]
   [Tooltip("Degrees per second. 30f is a slow, steady crawl.")]
   public float degreesPerSecond = 30f;

   void Update()
   {
      // Time.deltaTime is the time in seconds it took to complete the last frame.
      // Using this ensures the gear rotates at the same speed regardless of your FPS.
      float rotationAmount = degreesPerSecond * Time.deltaTime;

      // Rotates around the Z-axis (standard for 2D sprites/UI)
      transform.Rotate(0, 0, rotationAmount);
   }
}