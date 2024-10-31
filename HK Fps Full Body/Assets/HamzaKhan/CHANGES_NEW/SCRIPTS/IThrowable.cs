using UnityEngine.Events;
using UnityEngine;

public interface IThrowable
{
    void ThrowObj(HKPlayerItemSystem controller, Transform releasePosition, float force);
}