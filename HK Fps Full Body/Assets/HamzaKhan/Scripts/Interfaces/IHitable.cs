using UnityEngine;

namespace HKFps
{
    public interface IHitable
    {
        void Hit(GameObject hitObject, Vector3 hitPoint, Vector3 hitNormal, HealthDamageData healthDamageData);
    }
}