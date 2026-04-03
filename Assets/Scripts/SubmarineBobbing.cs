using UnityEngine;

public class SubmarineBobbing : MonoBehaviour
{
   [Header("Position Settings")]
   [SerializeField] private float posAmplitude = 10f; // Distance of the bob
   [SerializeField] private float posFrequency = 0.5f;  // Speed of the bob

   [Header("Rotation Settings")]
   [SerializeField] private float rotAmplitude = 2f;  // Max tilt angle
   [SerializeField] private float rotFrequency = 0.3f;  // Speed of the tilt

   [Header("Randomization")]
   [SerializeField] private float phaseOffset = 0f;    // Use this to desync the two halves

   private Vector3 _startPos;
   private Quaternion _startRot;

   void Start()
   {
      _startPos = transform.localPosition;
      _startRot = transform.localRotation;
   }

   void Update()
   {
      // Calculate a shared time value with the offset
      float time = Time.time + phaseOffset;

      // 1. Vertical Bobbing (Sine Wave)
      float newY = _startPos.y + Mathf.Sin(time * posFrequency * Mathf.PI * 2) * posAmplitude;
      transform.localPosition = new Vector3(_startPos.x, newY, _startPos.z);

      // 2. Subtle Tilting (Cos Wave so it's not perfectly in sync with the bob)
      float tiltZ = Mathf.Cos(time * rotFrequency * Mathf.PI * 2) * rotAmplitude;
      transform.localRotation = _startRot * Quaternion.Euler(0, 0, tiltZ);
   }
}