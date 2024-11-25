using UnityEngine;

namespace HKFps
{
    public abstract class HKPlayerInputBase : MonoBehaviour
    {
        public abstract PlayerInputActions GetInputActions();
    }
}