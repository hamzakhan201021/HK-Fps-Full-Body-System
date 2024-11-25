using UnityEngine;
using System;

namespace HKFps
{
    public class GunHolderAnimated : MonoBehaviour
    {

        #region General

        // Settings
        public HeadBobSettings HeadBobSettings;
        public MainBobSettings MainBobSettings;

        // Start Pos, Rot.
        Vector3 _startPos;
        Vector3 _startRot;

        // Bumping
        private Vector3 _bumpForce;
        private bool _isBumping = false;
        private float _bumpTimer = 0f;
        private float _bumpDuration = 0.2f;
        private AnimationCurve _defaultBumpCurve = new AnimationCurve(
            new Keyframe(0, 0), // start at 0.
            new Keyframe(0.5f, 1), // Mid point is 1.
            new Keyframe(1f, 0) // End at 0.
            );
        private AnimationCurve _bumpCurve;

        // CHANGES Added move input vector(
        //private Vector2 _moveInput;
        // CHANGES )

        // Start is called before the first frame update
        void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            _startPos = transform.localPosition;
            _startRot = transform.localRotation.eulerAngles;
        }

        //public void UpdateMoveInput(Vector2 moveInput)
        //{
        //    _moveInput = moveInput;
        //}

        //// Update is called once per frame
        //void Update()
        //{
        //    HeadBobCheck();
        //    SmoothTransformToTarget();
        //    HandleBumpEffect();
        //}

        public void UpdateGHAnimator(Vector2 moveInput, bool grounded, bool walking, bool sprinting, bool crouching)
        {
            HeadBobCheck(moveInput, grounded, walking, sprinting, crouching);
            SmoothTransformToTarget(sprinting);
            HandleBumpEffect(sprinting);
        }

        #endregion

        #region Functions

        /// <summary>
        /// Check whether we should be applying headbob.
        /// </summary>
        private void HeadBobCheck(Vector2 moveInput, bool grounded, bool walking, bool sprinting, bool crouching)
        {
            // CHANGES Look at the next code line, uses the newly created vector(
            //Vector2 moveInput = MainBobSettings.Controller.Input.Player.Move.ReadValue<Vector2>();
            // CHANGES )

            // CHANGES (
            //float inputMag = moveInput.magnitude;
            float inputMag = moveInput.magnitude;
            // CHANGES )

            if (inputMag > 0)
            {
                // Perform movement Effects.
                CreateHeadBobPositionEffect(grounded, walking, sprinting, crouching);
                MoveTilt(grounded, moveInput);
            }
        }

        /// <summary>
        /// Create the HeadBob Effect.
        /// </summary>
        /// <returns></returns>
        private Vector3 CreateHeadBobPositionEffect(bool grounded, bool walking, bool sprinting, bool crouching)
        {
            Vector3 pos = Vector3.zero;

            //// CHANGES BELOW <OLD ARE COMMENTED>

            //if (MainBobSettings.Controller.IsGrounded() == true)
            if (grounded)
            {
                //if (MainBobSettings.Controller.IsWalking() == true)
                if (walking)
                {
                    pos.x += Mathf.Lerp(pos.x, Mathf.Cos(Time.time * HeadBobSettings.WalkBobSpeed / 2f) *
                        HeadBobSettings.WalkBobAmount * HeadBobSettings.WalkXMulti, HeadBobSettings.Smoothing * Time.deltaTime);

                    pos.y += Mathf.Lerp(pos.y, Mathf.Sin(Time.time * HeadBobSettings.WalkBobSpeed) *
                        HeadBobSettings.WalkBobAmount * HeadBobSettings.WalkYMulti, HeadBobSettings.Smoothing * Time.deltaTime);
                }
                //else if (MainBobSettings.Controller.IsSprinting() == true)
                else if (sprinting)
                {
                    pos.x += Mathf.Lerp(pos.x, Mathf.Cos(Time.time * HeadBobSettings.SprintBobSpeed / 2f) *
                        HeadBobSettings.SprintBobAmount * HeadBobSettings.SprintXMulti, HeadBobSettings.Smoothing * Time.deltaTime);

                    pos.y += Mathf.Lerp(pos.y, Mathf.Sin(Time.time * HeadBobSettings.SprintBobSpeed) *
                        HeadBobSettings.SprintBobAmount * HeadBobSettings.SprintYMulti, HeadBobSettings.Smoothing * Time.deltaTime);

                    pos.z += Mathf.Lerp(pos.z, Mathf.Sin(Time.time * HeadBobSettings.SprintBobSpeed / 2f) *
                       HeadBobSettings.SprintBobAmount * HeadBobSettings.SprintZMulti, HeadBobSettings.Smoothing * Time.deltaTime);
                }
                //else if (MainBobSettings.Controller.IsCrouching() == true)
                else if (crouching)
                {
                    pos.x += Mathf.Lerp(pos.x, Mathf.Cos(Time.time * HeadBobSettings.CrouchBobSpeed / 2f) *
                        HeadBobSettings.CrouchBobAmount * HeadBobSettings.CrouchXMulti, HeadBobSettings.Smoothing * Time.deltaTime);

                    pos.y += Mathf.Lerp(pos.y, Mathf.Sin(Time.time * HeadBobSettings.CrouchBobSpeed) *
                        HeadBobSettings.CrouchBobAmount * HeadBobSettings.CrouchYMulti, HeadBobSettings.Smoothing * Time.deltaTime);
                }

                transform.localPosition += pos;
            }

            return pos;
        }

        /// <summary>
        /// Handle's The Movement tilt of the weapon.
        /// </summary>
        /// <returns></returns>
        private Vector3 MoveTilt(bool grounded, Vector2 moveInput)
        {
            Vector3 rot = Vector3.zero;

            // CHANGES.

            //if (MainBobSettings.Controller.IsGrounded() == true)
            if (grounded)
            {
                // CHANGES Use new vector instead of getting vector (
                //Vector2 moveInput = MainBobSettings.Controller.Input.Player.Move.ReadValue<Vector2>();

                //rot.z = moveInput.x * HeadBobSettings.XTiltMulti * Time.deltaTime;
                rot.z = moveInput.x * HeadBobSettings.XTiltMulti * Time.deltaTime;
                // CHANGES )

                transform.localRotation *= Quaternion.Euler(rot);
            }

            return rot;
        }

        /// <summary>
        /// Smooths the transform to the target.
        /// </summary>
        private void SmoothTransformToTarget(bool sprinting)
        {
            // CHANGES (
            // Set local position and rotation.
            //transform.SetLocalPositionAndRotation(Vector3.Lerp(transform.localPosition, _startPos + (MainBobSettings.Controller.IsSprinting() ?
            //    HeadBobSettings.SprintPositionOffset : Vector3.zero), HeadBobSettings.Smoothing * Time.deltaTime),
            //    Quaternion.Slerp(transform.localRotation, Quaternion.Euler(MainBobSettings.Controller.IsSprinting() ?
            //    HeadBobSettings.SprintRotation : _startRot), HeadBobSettings.Smoothing * Time.deltaTime));

            transform.SetLocalPositionAndRotation(Vector3.Lerp(transform.localPosition, _startPos + (sprinting ?
                HeadBobSettings.SprintPositionOffset : Vector3.zero), HeadBobSettings.Smoothing * Time.deltaTime),
                Quaternion.Slerp(transform.localRotation, Quaternion.Euler(sprinting ?
                HeadBobSettings.SprintRotation : _startRot), HeadBobSettings.Smoothing * Time.deltaTime));

            // CHANGES )
        }

        /// <summary>
        /// trigger's a bump using the force, duration & curve.
        /// </summary>
        /// <param name="force"></param>
        /// <param name="duration"></param>
        /// <param name="curve"></param>
        public void TriggerBump(Vector3 force, float duration, AnimationCurve curve = null)
        {
            _isBumping = true;
            _bumpTimer = 0f;
            _bumpForce = force;
            _bumpDuration = duration;
            _bumpCurve = curve != null ? curve : _defaultBumpCurve;
        }

        /// <summary>
        /// Handles the bumping effect.
        /// </summary>
        private void HandleBumpEffect(bool sprinting)
        {
            // Out of here if we aren't bumping.
            if (_isBumping == false) return;

            // Update Bump timer.
            _bumpTimer += Time.deltaTime;

            // Bump Progress.
            float bumpProgress = _bumpTimer / _bumpDuration;

            // Apply the bump effect using the curve.
            // CHANGES (
            //transform.localPosition = _startPos + (MainBobSettings.Controller.IsSprinting() ? HeadBobSettings.SprintPositionOffset :
            //    Vector3.zero) + _bumpForce * _bumpCurve.Evaluate(bumpProgress);

            transform.localPosition = _startPos + (sprinting ? HeadBobSettings.SprintPositionOffset :
                Vector3.zero) + _bumpForce * _bumpCurve.Evaluate(bumpProgress);

            // CHANGES )

            // Disable Bumping Once Bumping is Complete
            if (bumpProgress >= 1f) _isBumping = false;
        }

        #endregion

    }

    #region Settings

    [Serializable]
    public class HeadBobSettings
    {
        [Header("Bobbing Settings")]
        public float WalkBobAmount = 0.0035f;
        public float WalkBobSpeed = 14f;
        [Space(5)]
        public float SprintBobAmount = 0.015f;
        public float SprintBobSpeed = 20f;
        [Space(5)]
        public float CrouchBobAmount = 0.004f;
        public float CrouchBobSpeed = 12f;
        [Space(5)]
        public float Smoothing = 8f;
        public float XTiltMulti = 30f;
        [Space(5)]
        public float WalkXMulti = 1.6f;
        public float WalkYMulti = 1.4f;
        [Space(5)]
        public float SprintXMulti = 1.2f;
        public float SprintYMulti = 1.4f;
        public float SprintZMulti = 1.4f;
        [Space(5)]
        public float CrouchXMulti = 1.6f;
        public float CrouchYMulti = 1.4f;
        [Space(5)]
        public Vector3 SprintRotation = new Vector3(10, -10, 0);
        public Vector3 SprintPositionOffset = Vector3.zero;
    }

    [Serializable]
    public class MainBobSettings
    {
        //[Header("Controller")]
        // CHANGES (
        //public PlayerController Controller;
        //public HKPlayerLocomotionCC ControllerCC;
        // CHANGES )
    }

    #endregion
}