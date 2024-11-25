using Unity.Cinemachine;
using UnityEngine;

namespace HKFps
{
    [RequireComponent(typeof(CinemachineBrain))]
    public class CinemachineBrainInstance : MonoBehaviour
    {

        // Instance
        public static CinemachineBrainInstance Instance;

        private void Awake()
        {
            // Set the Instance.
            Instance = this;
        }

        /// <summary>
        /// When using this, make sure to cache reference, because it uses Get Component
        /// </summary>
        /// <returns></returns>
        public CinemachineBrain GetCinemachineBrain()
        {
            return GetComponent<CinemachineBrain>();
        }
    }
}