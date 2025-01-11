using UnityEngine;

namespace HKFps
{
    public interface IThrowable
    {
        void ThrowObj(HKPlayerItemSystem controller, Transform releasePosition, float force);
    }
}