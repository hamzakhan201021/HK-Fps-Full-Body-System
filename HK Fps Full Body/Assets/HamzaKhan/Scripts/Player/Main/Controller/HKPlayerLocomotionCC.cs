using System.Collections;
using UnityEngine.Events;
using UnityEngine;

public class HKPlayerLocomotionCC : HKPlayerLocomotionCCBase
{

    #region Variables

    [Space]
    [Header("Movement")]
    [SerializeField] private float _smoothMoveInputSpeed = 0.1f;
    [Space]
    [SerializeField] private float _walkSpeed = 3;
    [SerializeField] private float _walkBackSpeed = 2;
    [Space]
    [SerializeField] private float _runSpeed = 7, _runBackSpeed = 5;
    [Space]
    [SerializeField] private float _crouchSpeed = 2, _crouchBackSpeed = 1;
    [SerializeField] private float _crouchHeight = 1.4f;
    [SerializeField] private float _crouchCenterY = 0.72f;
    [SerializeField] private float _crouchToStandCheckDistance = 2f;
    [Space]
    [SerializeField] private float _standingHeight = 1.8f;
    [SerializeField] private float _standingCenterY = 0.92f;
    [Space]
    [SerializeField] private float _controllerRadius = 0.5f;
    [SerializeField] private float _controllerValLerpSpeed = 10f;
    [Space(5)]
    [SerializeField] private GunHolderAnimated _gunHolderAnimator;
    [Space]
    [Header("Proning")]
    [SerializeField] private bool _enableProning = false;
    [SerializeField] private float _proneHeight = 1.1f;
    [SerializeField] private float _proneCenterY = 0.55f;
    [SerializeField] private float _proneSpeed = 0.5f, _proneBackSpeed = 0.5f;
    [Space]
    [Header("Gravity Push of ledge Settings")]
    [SerializeField] private float _radiusChangeTime = 0.4f;
    [SerializeField] private float _gSphereCheckRadius = 0.2f;
    [SerializeField] private float _gSphereCheckYOffset = -0.1f;
    [Space]
    [Header("Gravity")]
    [SerializeField] private float _gravityMultiplyer = 2f;
    [SerializeField] private float _groundDistance = 0.1f;
    [SerializeField] private float _groundCheckRadius = 0.5f;
    [SerializeField] private LayerMask _groundLayerMask;
    [Space]
    [Header("Jump")]
    [SerializeField] private float _jumpForce = 7f;
    [Space(5)]
    [SerializeField] private float _jumpBumpAmount = 0.025f;
    [SerializeField] private float _jumpBumpAmountDuration = 0.35f;
    [SerializeField] private AnimationCurve _jumpBumpCurve;
    [Space(5)]
    [SerializeField] private float _hitTheGroundbumpAmount;
    [SerializeField] private float _hitTheGroundbumpAmountDuration;
    [SerializeField] private AnimationCurve _hitTheGroundbumpCurve;
    [Space]
    [Header("Animator")]
    [SerializeField] private Animator _playerAnimator;
    [Header("Animator Parameter Names")]
    [SerializeField] private string _moveZParamName = "VelocityY";
    [SerializeField] private string _moveXParamName = "VelocityX";
    [SerializeField] private string _turnParamName = "Turn";
    [SerializeField] private float _turningAmountMulti = 0.0015f;
    [SerializeField] private float _turningAnimDampTime = 0.1f;
    [SerializeField] private float _animLerpingSpeed = 10f;
    [SerializeField] private string _inAirParamName = "InAir";
    [SerializeField] private string _standingValueParamName = "StandingValue";
    [SerializeField] private string _walkOrRunParamName = "WalkOrRun";
    [SerializeField] private string _weaponTypeParamName = "WeaponType";
    [SerializeField] private string _nearGroundParamName = "NearGround";
    [SerializeField] private string _jumpParamName = "Jump";
    [Space(5)]
    [SerializeField] private string _switchOrHolsterTriggerName = "PerformSwitchOrHolster";
    [SerializeField] private string _weaponSwitchTypeName = "WeaponSwitchType";
    [SerializeField] private string _upperBodyLayerName = "UpperBody";
    [SerializeField] private string _itemIDParamName = "ItemID";
    [SerializeField] private string _performItemParamName = "PerformItem";
    [SerializeField] private string _holdItemParamName = "HoldItem";
    [SerializeField] private string _cancelItemParamName = "CancelItem";
    [SerializeField] private string _playerDamageParamName = "PlayerDamage";
    [Space]
    [Header("Look")]
    [SerializeField] private Transform _centerSpinePos;
    [SerializeField] private Transform _clipProjectorPos;
    [SerializeField] private float _sensY = 50;
    [SerializeField] private float _sensX = 50;
    [SerializeField] private MaxLookAmounts _maxLookAmounts;
    [Space(3)]
    [SerializeField] private float _lookMaxChangeSpeed = 40f;
    [SerializeField] private float _rotatingSlerpSpeed = 20f;
    [Space]
    [Header("Camera")]
    [SerializeField] private Transform _cameraHolder;
    [SerializeField] private Transform _headTransform;
    [Space(3)]
    [SerializeField] private Vector3 _cameraHolderDefaultOffset = new Vector3(0, 0, 0.25f);
    [Space]
    [Header("Leaning / Peeking")]
    [SerializeField] private float _leaningInputSmoothingSpeed = 10f;
    [SerializeField] private float _leaningAngle = 25f;
    [Space]
    [Header("Events")]
    [Space]
    public UnityEvent OnJump;
    [Space]
    public UnityEvent<float> OnHitGround;

    private int _moveZParamNameHash;
    private int _moveXParamNameHash;
    private int _turnParamNameHash;
    private int _inAirParamNameHash;
    private int _standingValueParamNameHash;
    private int _walkOrRunParamNameHash;
    private int _weaponTypeParamNameHash;
    private int _nearGroundParamNameHash;
    private int _jumpParamNameHash;

    private int _switchOrHolsterTriggerNameHash;
    private int _weaponSwitchTypeNameHash;
    private int _upperBodyLayerIndex;

    private int _itemIDParamNameHash;
    private int _performItemParamNameHash;
    private int _holdItemParamNameHash;
    private int _cancelItemParamNameHash;

    private int _playerDamageParamNameHash;

    // private var
    // Movement
    private CharacterController _characterController;

    // Movement Input Smoothing Vectors...
    private Vector2 _currentInputVector;
    private Vector2 _currentMoveInputVector;
    private Vector2 _smoothInputVelocity;
    private Vector2 _smoothMoveInputVelocity;

    private Vector3 _gSphereCheckOffsetVec = Vector3.zero;
    private Vector3 _controllerCenter = Vector3.zero;
    private Vector3 _moveDirection;

    private Vector3 _groundCheckPosition;
    private Vector3 _bumpAmountVec = Vector3.zero;

    private Vector3 _spineTargetRotation = Vector3.zero;
    private Vector3 _spineTargetRotationZ = Vector3.zero;

    private Vector3 _cameraTargetPosition;
    private Quaternion _cameraTargetRotation;

    private float _currentSpeed = 3;
    private float _gravity = -9.81f;
    private float _velocityY;

    private float _smoothCamToTargetSpeed = 10f;
    private float _lookUpLimit;
    private float _lookDownLimit;
    private float _xRotation;
    private float _yRotation;
    private float _leaningInput;

    private LocomotionState _locomotionState;
    private AirState _airState = AirState.grounded;
    private AirState _previousAirState = AirState.grounded;

    private bool _hasFinishedChangingRadius = true;

    private Transform _thisTransform;

    private float _inAirValue = 0;
    private float _standingValue = 1;
    private float _walkOrRun = 0;
    private float _weaponTypeValue = 0;

    #endregion

    #region General

    // Start is called before the first frame update
    void Start()
    {
        // Init
        Initialize();
    }

    private void Initialize()
    {
        // Init Controller
        _thisTransform = transform;
        _characterController = GetComponent<CharacterController>();

        // Animator Hashing.
        _moveZParamNameHash = Animator.StringToHash(_moveZParamName);
        _moveXParamNameHash = Animator.StringToHash(_moveXParamName);
        _turnParamNameHash = Animator.StringToHash(_turnParamName);
        _inAirParamNameHash = Animator.StringToHash(_inAirParamName);
        _standingValueParamNameHash = Animator.StringToHash(_standingValueParamName);
        _walkOrRunParamNameHash = Animator.StringToHash(_walkOrRunParamName);
        _weaponTypeParamNameHash = Animator.StringToHash(_weaponTypeParamName);
        _nearGroundParamNameHash = Animator.StringToHash(_nearGroundParamName);
        _jumpParamNameHash = Animator.StringToHash(_jumpParamName);

        _switchOrHolsterTriggerNameHash = Animator.StringToHash(_switchOrHolsterTriggerName);
        _weaponSwitchTypeNameHash = Animator.StringToHash(_weaponSwitchTypeName);

        _upperBodyLayerIndex = _playerAnimator.GetLayerIndex(_upperBodyLayerName);

        _itemIDParamNameHash = Animator.StringToHash(_itemIDParamName);
        _performItemParamNameHash = Animator.StringToHash(_performItemParamName);
        _holdItemParamNameHash = Animator.StringToHash(_holdItemParamName);
        _cancelItemParamNameHash = Animator.StringToHash(_cancelItemParamName);

        _playerDamageParamNameHash = Animator.StringToHash(_playerDamageParamName);

        OnJump.AddListener(OnJumpAnim);
    }

    /// <summary>
    /// use this function to apply any impulse force on the weapon.
    /// </summary>
    /// <param name="force"></param>
    /// <param name="duration"></param>
    /// <param name="curve"></param>
    private void TriggerWeaponImpulse(Vector3 force, float duration, AnimationCurve curve = null)
    {
        // Tell the procedural animator system to trigger a bump.
        _gunHolderAnimator.TriggerBump(force, duration, curve);
    }

    private bool CrouchToStandRaycast()
    {
        return Physics.Raycast(_thisTransform.position, Vector3.up, _crouchToStandCheckDistance, _groundLayerMask);
    }

    private void OnDrawGizmosSelected()
    {
        // Set the gizmos color to red.
        Gizmos.color = Color.red;

        // Draw near ground check position gizmo.
        // In case set the ground check position.
        if (_groundCheckPosition == Vector3.zero)
        {
            _groundCheckPosition = transform.position;
            _groundCheckPosition.y += _groundDistance;
        }

        // Draw Wire Sphere.
        Gizmos.DrawWireSphere(_groundCheckPosition, _groundCheckRadius);

        // Draw sphere for radius changer.
        // In case set the _g Sphere Check Offset Vec.
        if (_gSphereCheckOffsetVec == Vector3.zero)
        {
            _gSphereCheckOffsetVec = transform.position;
            _gSphereCheckOffsetVec.y += _gSphereCheckYOffset;
        }

        // Draw Wire Sphere.
        Gizmos.DrawWireSphere(_gSphereCheckOffsetVec, _gSphereCheckRadius);
    }

    #endregion

    #region System Update Callbacks

    public override void MovePlayer(Vector2 moveInputVector, Vector2 lookInput, float targetUpperBodyLayerWeight,
        bool jumpPressed, bool sprintTriggered, bool hasStamina, bool crouchTriggered, bool proneTriggered, bool isCurrentWeaponRifle)
    {
        HandleInputAndMove(moveInputVector, jumpPressed);
        HandleSpeedsAndPlayerHeight(moveInputVector, sprintTriggered, hasStamina, crouchTriggered, proneTriggered);
        HandleAnim(lookInput, targetUpperBodyLayerWeight, isCurrentWeaponRifle);

        _gunHolderAnimator.UpdateGHAnimator(moveInputVector, IsGrounded(), IsWalking(), IsSprinting(), IsCrouching());
    }

    public override void UpdateRotation(Vector2 lookInput, Vector2 moveInput, float leanInput,
        bool isInInteractionRange, bool reloading, bool holstered, bool performingSwitch, bool clipped)
    {
        Look(lookInput, moveInput, leanInput, isInInteractionRange, reloading, holstered, performingSwitch, clipped);
    }

    public override void MoveCamera(Vector3 cameraClippedPosition, Vector3 cameraAimedPosition, Vector3 cameraNormalPosition, bool holstered, bool performingSwitch, bool clipped, bool aimed)
    {
        SetCameraPositionAndRotation(cameraClippedPosition, cameraAimedPosition, cameraNormalPosition, holstered, performingSwitch, clipped, aimed);
    }

    #endregion

    #region Player Movement

    private void HandleInputAndMove(Vector2 inputVector, bool jumpPressed)
    {
        // FALL OF EDGE SYSTEM.
        _gSphereCheckOffsetVec = transform.position;
        _gSphereCheckOffsetVec.y += _gSphereCheckYOffset;

        // Check if we should change the radius.
        if (_characterController.isGrounded && inputVector == Vector2.zero &&
            !Physics.CheckSphere(_gSphereCheckOffsetVec, _gSphereCheckRadius, _groundLayerMask))
        {
            // Check if we aren't already changing the radius.
            if (_hasFinishedChangingRadius)
            {
                // Start the radius change coroutine.
                StartCoroutine(ChangeRadiusCoroutine(_gSphereCheckRadius - 0.05f, _radiusChangeTime));
            }
        }

        // MOVEMENT SYSTEM

        // Input

        // Real input vector.
        _currentInputVector = Vector2.SmoothDamp(_currentInputVector, inputVector, ref _smoothInputVelocity, _smoothMoveInputSpeed);

        // Move input vector.
        _currentMoveInputVector = Vector2.SmoothDamp(_currentMoveInputVector, inputVector.normalized, ref _smoothMoveInputVelocity, _smoothMoveInputSpeed);

        // Gravity
        if (_characterController.isGrounded && _velocityY < 0.0f) _velocityY = -1f;
        else _velocityY += _gravity * Time.deltaTime * _gravityMultiplyer;

        // Air State

        // CheckSphere for ground check.
        _groundCheckPosition = _thisTransform.position;
        _groundCheckPosition.y += _groundDistance;

        // Set the air state.
        _airState = Physics.CheckSphere(_groundCheckPosition, _groundCheckRadius, _groundLayerMask) ? AirState.grounded : AirState.inAir;

        // Jump

        // Check if we should jump.
        if (jumpPressed && _characterController.isGrounded && !CrouchToStandRaycast())
        {
            // Set the in air value MAKE SURE PLAYER ANIMATOR SYSTEM DOES THIS.
            _inAirValue = 0;

            // Invoke On Jump instead.
            OnJump.Invoke();

            // Set the vel Y to the jump force.
            _velocityY = _jumpForce;

            // Trigger an impulse effect.
            _bumpAmountVec.y = _jumpBumpAmount;

            TriggerWeaponImpulse(_bumpAmountVec, _jumpBumpAmountDuration, _jumpBumpCurve);
        }

        // Check if the state has changed.
        if (_previousAirState != _airState)
        {
            // Check if the new state is grounded.
            if (_airState == AirState.grounded)
            {
                // Calculate & Trigger an impulse effect.
                _bumpAmountVec.y = _hitTheGroundbumpAmount * -_velocityY;
                TriggerWeaponImpulse(_bumpAmountVec, _hitTheGroundbumpAmountDuration, _hitTheGroundbumpCurve);

                OnHitGround.Invoke(_velocityY);
            }
        }

        // Update the previous state.
        _previousAirState = _airState;

        // Movement

        // Get move direction.
        _moveDirection = (_currentMoveInputVector.y * _thisTransform.forward) + (_currentMoveInputVector.x * _thisTransform.right);

        // Multiply move direction with speed.
        _moveDirection *= _currentSpeed;

        // Set Move Y.
        _moveDirection.y = _velocityY;

        // Controller : Move.
        if (_characterController.enabled) _characterController.Move(_moveDirection * Time.deltaTime);
    }

    private void HandleSpeedsAndPlayerHeight(Vector2 inputVector, bool sprintTriggered, bool hasStamina, bool crouchTriggered, bool proneTriggered)
    {
        // Sprint Check.
        //if (_hKPlayerInput.GetInputActions().Player.Sprint.triggered && !CrouchToStandRaycast())
        if (sprintTriggered && hasStamina &&!CrouchToStandRaycast())
        {
            switch (_locomotionState)
            {
                case LocomotionState.sprinting:
                    _locomotionState = LocomotionState.walking;
                    break;
                default:
                    _locomotionState = LocomotionState.sprinting;
                    break;
            }
        }

        // Crouch Check.
        //if (Input.Player.Crouch.triggered && !CrouchToStandRaycast())
        if (crouchTriggered && !CrouchToStandRaycast())
        {
            switch (_locomotionState)
            {
                case LocomotionState.crouching:
                    _locomotionState = LocomotionState.walking;
                    break;
                default:
                    _locomotionState = LocomotionState.crouching;
                    break;
            }
        }

        // Can prone?
        if (_enableProning)
        {
            // Prone Check.
            //if (Input.Player.Prone.triggered && !CrouchToStandRaycast())
            if (proneTriggered && !CrouchToStandRaycast())
            {
                switch (_locomotionState)
                {
                    case LocomotionState.proning:
                        _locomotionState = LocomotionState.walking;
                        break;
                    default:
                        _locomotionState = LocomotionState.proning;
                        break;
                }
            }
        }
        else
        {
            // Make sure we get out of the prone state if proning is disabled.
            if (IsProning()) _locomotionState = LocomotionState.walking;
        }

        // Auto turn off sprint state Check.
        if (_locomotionState == LocomotionState.sprinting)
        {
            if (inputVector == Vector2.zero || !hasStamina)
            {
                _locomotionState = LocomotionState.walking;
            }
        }

        // Set Default speeds.
        switch (_locomotionState)
        {
            case LocomotionState.walking:
                _currentSpeed = _walkSpeed;
                break;
            case LocomotionState.sprinting:
                _currentSpeed = _runSpeed;
                break;
            case LocomotionState.crouching:
                _currentSpeed = _crouchSpeed;
                break;
            case LocomotionState.proning:
                _currentSpeed = _proneSpeed;
                break;
        }

        // Change speed if required (Based on Input Vector).
        if (IsSprinting() && inputVector.y > 0.0f) _currentSpeed = _runSpeed;
        else if (IsSprinting() && inputVector.y < 0.0f) _currentSpeed = _runBackSpeed;
        if (IsWalking() && inputVector.y > 0.0f) _currentSpeed = _walkSpeed;
        else if (IsWalking() && inputVector.y < 0.0f) _currentSpeed = _walkBackSpeed;
        if (IsCrouching() && inputVector.y > 0.0f) _currentSpeed = _crouchSpeed;
        else if (IsCrouching() && inputVector.y < 0.0f) _currentSpeed = _crouchBackSpeed;
        if (IsProning() && inputVector.y > 0.0f) _currentSpeed = _proneSpeed;
        else if (IsProning() && inputVector.y < 0.0f) _currentSpeed = _proneBackSpeed;

        // Handle Height And Radius Size.
        switch (_locomotionState)
        {
            case LocomotionState.crouching:
                _characterController.height = Mathf.Lerp(_characterController.height, _crouchHeight, _controllerValLerpSpeed * Time.deltaTime);

                _controllerCenter.y = _crouchCenterY;
                _characterController.center = Vector3.Lerp(_characterController.center, _controllerCenter, _controllerValLerpSpeed * Time.deltaTime);
                break;
            case LocomotionState.proning:
                _characterController.height = Mathf.Lerp(_characterController.height, _proneHeight, _controllerValLerpSpeed * Time.deltaTime);

                _controllerCenter.y = _proneCenterY;
                _characterController.center = Vector3.Lerp(_characterController.center, _controllerCenter, _controllerValLerpSpeed * Time.deltaTime);
                break;
            default:
                _characterController.height = Mathf.Lerp(_characterController.height, _standingHeight, _controllerValLerpSpeed * Time.deltaTime);

                _controllerCenter.y = _standingCenterY;
                _characterController.center = Vector3.Lerp(_characterController.center, _controllerCenter, _controllerValLerpSpeed * Time.deltaTime);
                break;
        }
    }

    #endregion

    #region Player Rotation

    private void SetCameraPositionAndRotation(Vector3 cameraClippedPosition, Vector3 cameraAimedPosition, Vector3 cameraNormalPosition, bool holstered, bool performingSwitch, bool clipped, bool aimed)
    {
        // Check if clipped
        //if (!_holstered)
        if (!holstered)
        {
            //if (IsClipped()) _cameraTargetPosition = _currentWeapon.WeaponData.CameraClippedPosition;
            //else if (IsAiming()) _cameraTargetPosition = _currentWeapon.WeaponData.CameraAimingPosition;
            //else _cameraTargetPosition = _currentWeapon.WeaponData.CameraNormalPosition;
            if (clipped) _cameraTargetPosition = cameraClippedPosition;
            else if (aimed) _cameraTargetPosition = cameraAimedPosition;
            else _cameraTargetPosition = cameraNormalPosition;
        }
        else
        {
            _cameraTargetPosition = _cameraHolder.parent.InverseTransformPoint(_headTransform.position) + _cameraHolderDefaultOffset;
        }

        // Add z off to camera target rotation.
        //_cameraTargetRotation = Quaternion.Euler(_holstered ? _xRotation : 0f, 0f, -_centerSpinePos.localRotation.eulerAngles.z);
        _cameraTargetRotation = Quaternion.Euler((holstered && !performingSwitch) ? _xRotation : 0f, 0f, -_centerSpinePos.localRotation.eulerAngles.z);

        _cameraHolder.SetLocalPositionAndRotation(Vector3.Lerp(_cameraHolder.localPosition, _cameraTargetPosition, _smoothCamToTargetSpeed * Time.deltaTime), _cameraTargetRotation);
    }

    private void Look(Vector2 lookInput, Vector2 moveInput, float leanInput, bool isInInteractionRange, bool reloading, bool holstered, bool performingSwitch, bool clipped)
    {
        // get the lookVector and set it up
        // CHANGES
        //Vector2 lookVector = Input.Player.Look.ReadValue<Vector2>();
        //float mouseX = lookVector.x * Time.deltaTime * _sensX;
        //float mouseY = lookVector.y * Time.deltaTime * _sensY;
        float mouseX = lookInput.x * Time.deltaTime * _sensX;
        float mouseY = lookInput.y * Time.deltaTime * _sensY;
        _yRotation += mouseX;
        _xRotation -= mouseY;

        // Handle Max Look Amounts.
        //HandleMaxLookAmounts(IsProning(), Input.Player.Move.ReadValue<Vector2>() != Vector2.zero, _airState == AirState.inAir, ref _lookUpLimit, ref _lookDownLimit, _lookMaxChangeSpeed);
        HandleMaxLookAmounts(IsProning(), moveInput != Vector2.zero, _airState == AirState.inAir, ref _lookUpLimit, ref _lookDownLimit, _lookMaxChangeSpeed);

        // Clamp X Rot.
        _xRotation = Mathf.Clamp(_xRotation, -_lookUpLimit, _lookDownLimit);

        // setting the values to the objects
        _thisTransform.rotation = Quaternion.Slerp(_thisTransform.rotation, Quaternion.Euler(0, _yRotation, 0), _rotatingSlerpSpeed * Time.deltaTime);

        // LEANING

        // Set Input And Smooth.
        //if (_isInInteractionRange == false) _leaningInput = Mathf.Lerp(_leaningInput, Input.Player.Lean.ReadValue<float>(), _leaningInputSmoothingSpeed * Time.deltaTime);
        if (isInInteractionRange == false) _leaningInput = Mathf.Lerp(_leaningInput, leanInput, _leaningInputSmoothingSpeed * Time.deltaTime);
        else _leaningInput = Mathf.Lerp(_leaningInput, 0f, _leaningInputSmoothingSpeed * Time.deltaTime);

        // Leaning Amount.
        float leaningAmount = _leaningInput * _leaningAngle;

        // Spine Target.
        _spineTargetRotation.x = _xRotation;
        _spineTargetRotation.z = -leaningAmount;
        _spineTargetRotationZ.z = _spineTargetRotation.z;

        // Check if we are reloading.
        //if (_currentWeapon._isReloading || _holstered)
        if (reloading || (holstered && !performingSwitch))
        {
            // Set Spine Followers Rotation Accordingly.
            SetSpineFollowersRotation(_spineTargetRotationZ, _spineTargetRotation, _rotatingSlerpSpeed);
        }
        else if (clipped) // Check if is clipped is true
        {
            // Set Spine Followers Rotation _spineTargetRotation.
            SetSpineFollowersRotation(Vector3.zero, _spineTargetRotation, _rotatingSlerpSpeed);
        }
        else
        {
            // Set Spine Followers Rotation Accordingly.
            SetSpineFollowersRotation(_spineTargetRotation, _spineTargetRotation, _rotatingSlerpSpeed);
        }
    }

    /// <summary>
    /// Handles Max Look Amounts.
    /// </summary>
    /// <param name="isMoving"></param>
    /// <param name="isUnGround"></param>
    /// <param name="maxLookUp"></param>
    /// <param name="maxLookDown"></param>
    /// <param name="lookMaxChangeSpeed"></param>
    private void HandleMaxLookAmounts(bool isProning, bool isMoving, bool isUnGround, ref float maxLookUp, ref float maxLookDown, float lookMaxChangeSpeed)
    {
        if (isUnGround)
        {
            SetLookLimits(ref maxLookUp, ref maxLookDown, _maxLookAmounts.LookAmountUnGround.LookUpLimit, _maxLookAmounts.LookAmountUnGround.LookDownLimit, lookMaxChangeSpeed);
        }
        else if (isProning)
        {
            SetLookLimits(ref maxLookUp, ref maxLookDown, _maxLookAmounts.LookAmountProned.LookUpLimit, _maxLookAmounts.LookAmountProned.LookDownLimit, lookMaxChangeSpeed);
        }
        else if (isMoving)
        {
            SetLookLimits(ref maxLookUp, ref maxLookDown, _maxLookAmounts.LookAmountMoving.LookUpLimit, _maxLookAmounts.LookAmountMoving.LookDownLimit, lookMaxChangeSpeed);
        }
        else
        {
            SetLookLimits(ref maxLookUp, ref maxLookDown, _maxLookAmounts.LookAmountNormal.LookUpLimit, _maxLookAmounts.LookAmountNormal.LookDownLimit, lookMaxChangeSpeed);
        }
    }

    /// <summary>
    /// Sets Look Limits.
    /// </summary>
    /// <param name="maxLookUp"></param>
    /// <param name="maxLookDown"></param>
    /// <param name="targetLimitUp"></param>
    /// <param name="targetLimitDown"></param>
    /// <param name="lookMaxChangeSpeed"></param>
    private void SetLookLimits(ref float maxLookUp, ref float maxLookDown, float targetLimitUp, float targetLimitDown, float lookMaxChangeSpeed)
    {
        maxLookUp = Mathf.Lerp(maxLookUp, targetLimitUp, lookMaxChangeSpeed * Time.deltaTime);
        maxLookDown = Mathf.Lerp(maxLookDown, targetLimitDown, lookMaxChangeSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Helpful Function for setting spine followers target rotation X.
    /// </summary>
    /// <param name="centerSpineTarget"></param>
    /// <param name="clipProjTarget"></param>
    /// <param name="slerpingSpeed"></param>
    private void SetSpineFollowersRotation(Vector3 centerSpineTarget, Vector3 clipProjTarget, float slerpingSpeed)
    {
        //_clipProjectorPos.localRotation = Quaternion.Slerp(_clipProjectorPos.localRotation, Quaternion.Euler(clipProjTarget), slerpingSpeed * Time.deltaTime);
        _centerSpinePos.localRotation = Quaternion.Slerp(_centerSpinePos.localRotation, Quaternion.Euler(centerSpineTarget), slerpingSpeed * Time.deltaTime);
    }

    private IEnumerator ChangeRadiusCoroutine(float targetRadius, float time)
    {
        _hasFinishedChangingRadius = false;

        float elapsedTime = 0f;
        float initialRadius = _characterController.radius;

        // Smoothly set the radius to the target radius.
        while (elapsedTime < time)
        {
            _characterController.radius = Mathf.Lerp(initialRadius, targetRadius, elapsedTime / time);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _characterController.radius = targetRadius;

        // Smoothly revert the radius back to the original value
        elapsedTime = 0f;
        while (elapsedTime < time)
        {
            _characterController.radius = Mathf.Lerp(targetRadius, _controllerRadius, elapsedTime / time);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _characterController.radius = _controllerRadius;

        _hasFinishedChangingRadius = true;
    }

    #endregion

    #region Player Animator

    private void HandleAnim(Vector2 lookInput, float targetUpperBodyLayerWeight, bool isCurrentWeaponRifle)
    {
        // Movement Values.
        switch (_locomotionState)
        {
            case LocomotionState.crouching:
                _standingValue = Mathf.Lerp(_standingValue, 0, _animLerpingSpeed * Time.deltaTime);
                break;
            case LocomotionState.proning:
                _standingValue = Mathf.Lerp(_standingValue, -1, _animLerpingSpeed * Time.deltaTime);
                break;
            default:
                _standingValue = Mathf.Lerp(_standingValue, 1, _animLerpingSpeed * Time.deltaTime);
                break;
        }

        _walkOrRun = Mathf.Lerp(_walkOrRun, IsWalking() ? 0 : 1, _animLerpingSpeed * Time.deltaTime);
        _inAirValue = Mathf.Lerp(_inAirValue, 1, _animLerpingSpeed * Time.deltaTime);

        // Weapon Values.
        _weaponTypeValue = Mathf.Lerp(_weaponTypeValue, isCurrentWeaponRifle ? 0 : 1, _animLerpingSpeed * Time.deltaTime);

        // Near Ground.
        _playerAnimator.SetBool(_nearGroundParamNameHash, _airState == AirState.grounded);

        // Locomotion floats
        _playerAnimator.SetFloat(_moveZParamNameHash, _currentInputVector.y);
        _playerAnimator.SetFloat(_moveXParamNameHash, _currentInputVector.x);
        _playerAnimator.SetFloat(_standingValueParamNameHash, _standingValue);
        _playerAnimator.SetFloat(_walkOrRunParamNameHash, _walkOrRun);
        _playerAnimator.SetFloat(_inAirParamNameHash, _inAirValue);

        // Turn
        _playerAnimator.SetFloat(_turnParamNameHash, lookInput.x * _sensX * _turningAmountMulti, _turningAnimDampTime, Time.deltaTime);

        // Weapon floats
        _playerAnimator.SetFloat(_weaponTypeParamNameHash, _weaponTypeValue);

        // Upper Body Layer Weight
        _playerAnimator.SetLayerWeight(_upperBodyLayerIndex, Mathf.Lerp(_playerAnimator.GetLayerWeight(_upperBodyLayerIndex), targetUpperBodyLayerWeight, _animLerpingSpeed * Time.deltaTime));
    }

    #region Events

    private void OnJumpAnim()
    {
        _playerAnimator.SetTrigger(_jumpParamNameHash);
    }

    public override void OnPerformWeaponSwitchAnim(int animToPlayID)
    {
        _playerAnimator.SetFloat(_weaponSwitchTypeNameHash, animToPlayID);
        _playerAnimator.SetTrigger(_switchOrHolsterTriggerNameHash);
    }

    public override void OnPerformItemAnim(int itemID)
    {
        SetItemAnimID(itemID);

        _playerAnimator.SetTrigger(_performItemParamNameHash);
    }

    public override void OnHoldItemAnim(int itemID)
    {
        SetItemAnimID(itemID);

        _playerAnimator.SetTrigger(_holdItemParamNameHash);
    }

    public override void OnCancelItemAnim()
    {
        _playerAnimator.SetTrigger(_cancelItemParamNameHash);
    }

    public override void OnPlayerDamage(float prevHealth, float newHealth)
    {
        _playerAnimator.SetTrigger(_playerDamageParamNameHash);
    }

    public override void OnDeath()
    {
        _playerAnimator.enabled = false;
    }

    public override void OnRevive()
    {
        _playerAnimator.enabled = true;
    }

    private void SetItemAnimID(int itemID)
    {
        _playerAnimator.SetFloat(_itemIDParamNameHash, itemID);
    }

    #endregion

    #endregion

    #region External

    /// <summary>
    /// Returns true if the loco state is walking
    /// </summary>
    /// <returns></returns>
    public override bool IsWalking()
    {
        return _locomotionState == LocomotionState.walking;
    }

    /// <summary>
    /// Returns true if the loco state is sprinting
    /// </summary>
    /// <returns></returns>
    public override bool IsSprinting()
    {
        return _locomotionState == LocomotionState.sprinting;
    }

    /// <summary>
    /// Returns true if the loco state is crouching
    /// </summary>
    /// <returns></returns>
    public override bool IsCrouching()
    {
        return _locomotionState == LocomotionState.crouching;
    }

    /// <summary>
    /// Returns true if the loco state is proning
    /// </summary>
    /// <returns></returns>
    public override bool IsProning()
    {
        return _locomotionState == LocomotionState.proning;
    }

    /// <summary>
    /// Is air state grounded ?
    /// </summary>
    /// <returns></returns>
    public override bool IsGrounded()
    {
        return _airState == AirState.grounded;
    }

    public override float GetXRotation()
    {
        return _xRotation;
    }

    public override void SetXRotation(float xRot)
    {
        _xRotation = xRot;
    }

    public override float GetLookUpLimit()
    {
        return _lookUpLimit;
    }

    public override float GetLookDownLimit()
    {
        return _lookDownLimit;
    }

    public override UnityEvent OnJumpEvent()
    {
        return OnJump;
    }

    public override UnityEvent<float> OnHitGroundEvent()
    {
        return OnHitGround;
    }

    #endregion
}