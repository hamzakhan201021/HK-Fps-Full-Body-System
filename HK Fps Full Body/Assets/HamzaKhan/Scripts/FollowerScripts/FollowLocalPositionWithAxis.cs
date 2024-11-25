using UnityEngine;

namespace HKFps
{
    public class FollowLocalPositionWithAxis : MonoBehaviour
    {

        [SerializeField] private Transform _target;

        [SerializeField] private Vector3 _positionOffset = Vector3.zero;

        [SerializeField] private bool _takeInitialPositionIntoAccount = true;
        [SerializeField] private bool _x = true;
        [SerializeField] private bool _y = true;
        [SerializeField] private bool _z = true;

        private Vector3 _initialPosition;
        private Vector3 _targetPosition;

        // Start is called before the first frame update
        void Start()
        {
            if (_takeInitialPositionIntoAccount)
            {
                _initialPosition = transform.localPosition;
            }
        }

        // Update is called once per frame
        void Update()
        {
            _targetPosition.x = _x ? _target.localPosition.x + _initialPosition.x : _initialPosition.x;
            _targetPosition.y = _y ? _target.localPosition.y + _initialPosition.y : _initialPosition.y;
            _targetPosition.z = _z ? _target.localPosition.z + _initialPosition.z : _initialPosition.z;

            _targetPosition += _positionOffset;

            transform.localPosition = _targetPosition;
        }
    }
}