using UnityEngine;

public class FollowTransformSmooth : MonoBehaviour
{

    [Space]
    [Header("Settings")]
    public Transform Target;
    [SerializeField] private float _smoothingSpeed = 15f;

    public Vector3 PositionOffset;
    public Vector3 RotationOffset;

    // Update is called once per frame
    void Update()
    {
        if (Target != null)
            transform.SetPositionAndRotation(Vector3.Lerp(transform.position, Target.position + PositionOffset, _smoothingSpeed * Time.deltaTime),
                Quaternion.Slerp(transform.rotation, Target.rotation * Quaternion.Euler(RotationOffset), _smoothingSpeed * Time.deltaTime));
    }
}
