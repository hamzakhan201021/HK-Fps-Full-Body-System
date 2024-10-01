using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slot : MonoBehaviour
{
    public float SmoothTime = 0.3f;
    public Vector3 RotationOffset;

    private List<Transform> _objectTransforms = new List<Transform>();

    public void SnapToSocket(Transform targetTransform, Vector3 positionOffset = default,
        Vector3 rotationOffset = default, bool keepRealPosition = false, bool keepRealRotation = false)
    {
        StartCoroutine(SmoothSnapCoroutine(targetTransform, positionOffset, rotationOffset, keepRealPosition, keepRealRotation));
    }

    private IEnumerator SmoothSnapCoroutine(Transform targetTransform, Vector3 positionOffset,
        Vector3 rotationOffset, bool keepRealPosition = false, bool keepRealRotation = false)
    {
        targetTransform.SetParent(transform);

        Vector3 localPosition = targetTransform.localPosition;
        Quaternion localRotation = targetTransform.localRotation;

        float elapsedTime = 0;

        while (elapsedTime < SmoothTime)
        {
            if (!keepRealPosition)
            {
                targetTransform.localPosition = Vector3.Lerp(localPosition, positionOffset, elapsedTime / SmoothTime);
            }

            if (!keepRealRotation)
            {
                targetTransform.localRotation = Quaternion.Slerp(localRotation, Quaternion.Euler(rotationOffset), elapsedTime / SmoothTime);
            }

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        if (!keepRealPosition) targetTransform.localPosition = positionOffset;
        if (!keepRealRotation) targetTransform.localRotation = Quaternion.Euler(rotationOffset);
    }

    public (bool hasObject, List<Transform> objectTransforms) HasObjectInSlot()
    {
        bool hasObject = transform.childCount > 0;

        _objectTransforms.Clear();

        if (hasObject)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                _objectTransforms.Add(transform.GetChild(i));
            }
        }

        return (hasObject, _objectTransforms);
    }
}
