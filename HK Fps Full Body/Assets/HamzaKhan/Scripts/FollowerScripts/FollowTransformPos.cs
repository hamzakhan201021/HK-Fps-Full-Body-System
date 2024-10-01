using UnityEngine;

public class FollowTransformPos : MonoBehaviour
{

    public Transform Target;
    public Vector3 Offset;

    // Update is called once per frame
    void Update()
    {
        transform.position = Target.position + Offset;
    }
}
