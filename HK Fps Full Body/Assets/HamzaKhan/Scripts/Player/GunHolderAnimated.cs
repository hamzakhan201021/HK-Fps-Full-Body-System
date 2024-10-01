using UnityEngine;
using System;

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

    // Update is called once per frame
    void Update()
    {
        HeadBobCheck();
        SmoothTransformToTarget();
        HandleBumpEffect();
    }

    #endregion

    #region Functions

    /// <summary>
    /// Check whether we should be applying headbob.
    /// </summary>
    private void HeadBobCheck()
    {
        Vector2 moveInput = MainBobSettings.Controller.Input.Player.Move.ReadValue<Vector2>();
        float inputMag = moveInput.magnitude;

        if (inputMag > 0)
        {
            // Perform movement Effects.
            CreateHeadBobPositionEffect();
            MoveTilt();
        }
    }

    /// <summary>
    /// Create the HeadBob Effect.
    /// </summary>
    /// <returns></returns>
    private Vector3 CreateHeadBobPositionEffect()
    {
        Vector3 pos = Vector3.zero;

        if (MainBobSettings.Controller.IsGrounded() == true)
        {
            if (MainBobSettings.Controller.IsWalking() == true)
            {
                pos.x += Mathf.Lerp(pos.x, Mathf.Cos(Time.time * HeadBobSettings.WalkBobSpeed / 2f) *
                    HeadBobSettings.WalkBobAmount * HeadBobSettings.WalkXMulti, HeadBobSettings.Smoothing * Time.deltaTime);

                pos.y += Mathf.Lerp(pos.y, Mathf.Sin(Time.time * HeadBobSettings.WalkBobSpeed) *
                    HeadBobSettings.WalkBobAmount * HeadBobSettings.WalkYMulti, HeadBobSettings.Smoothing * Time.deltaTime);
            }
            else if (MainBobSettings.Controller.IsSprinting() == true)
            {
                pos.x += Mathf.Lerp(pos.x, Mathf.Cos(Time.time * HeadBobSettings.SprintBobSpeed / 2f) *
                    HeadBobSettings.SprintBobAmount * HeadBobSettings.SprintXMulti, HeadBobSettings.Smoothing * Time.deltaTime);

                pos.y += Mathf.Lerp(pos.y, Mathf.Sin(Time.time * HeadBobSettings.SprintBobSpeed) *
                    HeadBobSettings.SprintBobAmount * HeadBobSettings.SprintYMulti, HeadBobSettings.Smoothing * Time.deltaTime);

                pos.z += Mathf.Lerp(pos.z, Mathf.Sin(Time.time * HeadBobSettings.SprintBobSpeed / 2f) *
                   HeadBobSettings.SprintBobAmount * HeadBobSettings.SprintZMulti, HeadBobSettings.Smoothing * Time.deltaTime);
            }
            else if (MainBobSettings.Controller.IsCrouching() == true)
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
    private Vector3 MoveTilt()
    {
        Vector3 rot = Vector3.zero;

        if (MainBobSettings.Controller.IsGrounded() == true)
        {
            Vector2 moveInput = MainBobSettings.Controller.Input.Player.Move.ReadValue<Vector2>();

            rot.z = moveInput.x * HeadBobSettings.XTiltMulti * Time.deltaTime;

            transform.localRotation *= Quaternion.Euler(rot);
        }

        return rot;
    }

    /// <summary>
    /// Smooths the transform to the target.
    /// </summary>
    private void SmoothTransformToTarget()
    {
        // Set local position and rotation.
        transform.SetLocalPositionAndRotation(Vector3.Lerp(transform.localPosition, _startPos + (MainBobSettings.Controller.IsSprinting() ?
            HeadBobSettings.SprintPositionOffset : Vector3.zero), HeadBobSettings.Smoothing * Time.deltaTime),
            Quaternion.Slerp(transform.localRotation, Quaternion.Euler(MainBobSettings.Controller.IsSprinting() ?
            HeadBobSettings.SprintRotation : _startRot), HeadBobSettings.Smoothing * Time.deltaTime));
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
    private void HandleBumpEffect()
    {
        // Out of here if we aren't bumping.
        if (_isBumping == false) return;

        // Update Bump timer.
        _bumpTimer += Time.deltaTime;

        // Bump Progress.
        float bumpProgress = _bumpTimer / _bumpDuration;

        // Apply the bump effect using the curve.
        transform.localPosition = _startPos + (MainBobSettings.Controller.IsSprinting() ? HeadBobSettings.SprintPositionOffset :
            Vector3.zero) + _bumpForce * _bumpCurve.Evaluate(bumpProgress);

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
    [Header("Controller")]
    public PlayerController Controller;
}

#endregion