using UnityEngine;

public class FollowTransformRot : MonoBehaviour
{

    public Transform Target;
    public Vector3 Offset;

    // Update is called once per frame
    void Update()
    {
        if (Target != null)
        {
            transform.rotation = Target.rotation * Quaternion.Euler(Offset);
        }
    }
}
