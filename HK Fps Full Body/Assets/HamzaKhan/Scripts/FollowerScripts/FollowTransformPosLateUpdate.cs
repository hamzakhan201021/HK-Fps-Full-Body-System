using UnityEngine;

public class FollowTransformPosLateUpdate : MonoBehaviour
{

    public Transform Target;
    public Vector3 Offset;
    public PositionFollowType FollowType = PositionFollowType.world;

    public enum PositionFollowType
    {
        world,
        local,
    }

    // Update is called once per frame
    void LateUpdate()
    {
        switch (FollowType)
        {
            case PositionFollowType.world:
                transform.position = Target.position + Offset;
                break;
            case PositionFollowType.local:
                transform.localPosition = Target.localPosition + Offset;
                break;
        }
    }
}
