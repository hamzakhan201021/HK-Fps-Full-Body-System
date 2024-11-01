using UnityEngine;

public class FollowTransformPosAndRot : MonoBehaviour
{

    public Transform Target;

    public Vector3 PositionOffset;
    public Vector3 RotationOffset;

    public bool MaintainOffset = false;

    private Quaternion _rotationOffset = Quaternion.identity;

    void Start()
    {
        if (MaintainOffset && Target != null) _rotationOffset = Quaternion.Inverse(Target.rotation) * transform.rotation;
    }

    private void LateUpdate()
    {
        CalcAndApply();
    }

    public void CalcAndApply()
    {
        SetPositionAndRotation();
    }

    private void SetPositionAndRotation()
    {
        if (Target != null) transform.SetPositionAndRotation(Target.position + PositionOffset,
            Target.rotation * _rotationOffset * Quaternion.Euler(RotationOffset));
    }
}
