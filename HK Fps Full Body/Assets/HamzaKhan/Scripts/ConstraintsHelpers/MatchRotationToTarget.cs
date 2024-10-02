using UnityEngine;

public class MatchRotationToTarget : MonoBehaviour, ICustomConstraints
{

    // Settings.
    [Space]
    [Header("Settings")]
    [Tooltip("The Weight Of The Matching")]
    [Range(0, 1)]
    [SerializeField] private float _weight = 1.0f;
    [Tooltip("The Smoothing Speed, Used For Smooth Results")]
    [SerializeField] private float _smoothingSpeed = 15f;

    [Space(5)]
    [Tooltip("The Matching Type Local or World.")]
    [SerializeField] private MatchingType _matchingType;

    [Space(5)]
    [Tooltip("The Object Which Will Be Matched.")]
    [SerializeField] private Transform _constrained;

    [Tooltip("The Object To Match")]
    [SerializeField] private Transform _toMatch;

    // Holds the last constrained objects rotation, Used For Smoothing.
    private Quaternion _lastConstrainedRotation = Quaternion.identity;

    void LateUpdate()
    {
        // Check if the matching type if local.
        if (_matchingType == MatchingType.local)
        {
            // Calculate Target Rotation.
            Quaternion targetRotation = Quaternion.Slerp(_lastConstrainedRotation,
                Quaternion.Lerp(_constrained.localRotation, _toMatch.localRotation, _weight), _smoothingSpeed * Time.deltaTime);

            // Set Constrained Local Rotation.
            _constrained.localRotation = targetRotation;

            // Set Last Constrained Rotation.
            _lastConstrainedRotation = targetRotation;
        }
        else if (_matchingType == MatchingType.world)
        {
            // Calculate Target Rotation.
            Quaternion targetRotation = Quaternion.Slerp(_lastConstrainedRotation,
                Quaternion.Lerp(_constrained.rotation, _toMatch.rotation, _weight), _smoothingSpeed * Time.deltaTime);

            // Set Constrained Rotation.
            _constrained.rotation = targetRotation;

            // Set Last Constrained Rotation.
            _lastConstrainedRotation = targetRotation;
        }
    }

    /// <summary>
    /// Returns the weight
    /// </summary>
    /// <returns></returns>
    public float GetWeight()
    {
        return _weight;
    }

    /// <summary>
    /// Use for setting the weight.
    /// </summary>
    /// <param name="weight"></param>
    public void SetWeight(float weight)
    {
        _weight = weight;
    }
}
