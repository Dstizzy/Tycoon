using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIItemBobbing : MonoBehaviour
{
    [Header("Position Settings")]
    [SerializeField] private float posAmplitude = 5f;  // Keep this small for UI!
    [SerializeField] private float posFrequency = 0.5f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotAmplitude = 2f;
    [SerializeField] private float rotFrequency = 0.3f;

    [Header("Randomization")]
    [Tooltip("If true, the item will pick a random starting point in the wave so all items don't bob in sync.")]
    [SerializeField] private bool randomizePhase = true;
    [SerializeField] private float manualPhaseOffset = 0f;

    private RectTransform _rectTransform;
    private Vector2 _startAnchoredPos;
    private Quaternion _startRot;
    private float _actualPhaseOffset;

    void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        
        // Cache the starting anchored position and rotation
        _startAnchoredPos = _rectTransform.anchoredPosition;
        _startRot = _rectTransform.localRotation;

        // Automatically desync the animations if enabled
        _actualPhaseOffset = randomizePhase ? Random.Range(0f, 100f) : manualPhaseOffset;
    }

    void Update()
    {
        // Use unscaledTime so the UI keeps bobbing even if the tycoon game is paused
        float time = Time.unscaledTime + _actualPhaseOffset;

        // 1. Vertical Bobbing
        float newY = _startAnchoredPos.y + Mathf.Sin(time * posFrequency * Mathf.PI * 2) * posAmplitude;
        _rectTransform.anchoredPosition = new Vector2(_startAnchoredPos.x, newY);

        // 2. Subtle Tilting
        float tiltZ = Mathf.Cos(time * rotFrequency * Mathf.PI * 2) * rotAmplitude;
        _rectTransform.localRotation = _startRot * Quaternion.Euler(0, 0, tiltZ);
    }
}