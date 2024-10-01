using UnityEngine;

public class FollowTransformPosLateUpdate : MonoBehaviour
{

    public Transform Target;
    public Vector3 Offset;

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = Target.position + Offset;
    }
}
