﻿using UnityEngine.Animations.Rigging;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class PlayerControllerBackup : MonoBehaviour
{

    // main
    #region
    [Space]
    [Header("Main")]
    [SerializeField] private int detectWeaponClipInterval = 15;
    [SerializeField] private int detectInteractableInterval = 30;
    #endregion

    // input
    #region
    [Space]
    [Header("Input")]
    [SerializeField] private float _smoothInputSpeed = 0.1f;

    // private var
    private Vector2 _currentInputVector;
    private Vector2 _currentMoveInputVector;
    private Vector2 _smoothInputVelocity;
    private Vector2 _smoothMoveInputVelocity;
    [HideInInspector] public PlayerInputActions Input;
    #endregion

    // movement
    #region
    [Space]
    [Header("Movement")]
    [SerializeField] private float _walkSpeed = 3;
    [SerializeField] private float _walkBackSpeed = 2;
    [SerializeField] private float _runSpeed = 7, _runBackSpeed = 5;
    [SerializeField] private float _crouchSpeed = 2, _crouchBackSpeed = 1;
    [SerializeField] private float _crouchHeight = 1.4f;
    [SerializeField] private float _crouchCenterY = 0.72f;
    [SerializeField] private float _crouchToStandCheckDistance = 2f;
    [SerializeField] private float _standingHeight = 1.8f;
    [SerializeField] private float _standingCenterY = 0.92f;

    [SerializeField] private float _controllerRadius = 0.5f;
    [SerializeField] private float _controllerValLerpSpeed = 10f;

    [Header("Proning")]
    [SerializeField] private bool _enableProning = false;
    [SerializeField] private float _proneHeight = 1.1f;
    [SerializeField] private float _proneCenterY = 0.55f;
    [SerializeField] private float _proneSpeed = 0.5f, _proneBackSpeed = 0.5f;

    [Header("Gravity Push of ledge Settings")]
    [SerializeField] private float _radiusChangeTime = 0.4f;
    [SerializeField] private float _gSphereCheckRadius = 0.2f;
    [SerializeField] private float _gSphereCheckYOffset = -0.1f;

    // private var
    // Movement
    private CharacterController _characterController;

    private Vector3 _gSphereCheckOffsetVec = Vector3.zero;
    private Vector3 _controllerCenter = Vector3.zero;
    private Vector3 _moveDirection;

    private float _currentSpeed = 3;

    private LocomotionState _locomotionState;

    public enum LocomotionState
    {
        walking,
        sprinting,
        crouching,
        proning,
    }

    private bool _hasFinishedChangingRadius = true;

    private Transform _thisTransform;
    #endregion

    // animator
    #region
    [Space]
    [Header("Animator")]
    [SerializeField] private Animator _playerAnimator;
    [Header("Animator Parameter Names")]
    [SerializeField] private string _moveZParamName = "VelocityY";
    [SerializeField] private string _moveXParamName = "VelocityX";
    [SerializeField] private string _turnParamName = "Turn";

    [SerializeField] private float _turningAmountMulti = 0.0015f;

    [SerializeField] private string _inAirParamName = "InAir";
    [SerializeField] private string _standingValueParamName = "StandingValue";
    [SerializeField] private string _walkOrRunParamName = "WalkOrRun";
    [SerializeField] private string _weaponTypeParamName = "WeaponType";
    [SerializeField] private string _nearGroundParamName = "NearGround";
    [SerializeField] private string _jumpParamName = "Jump";
    [SerializeField] private float _animLerpingSpeed = 10f;

    private int _moveZParamNameHash;
    private int _moveXParamNameHash;
    private int _turnParamNameHash;
    private int _inAirParamNameHash;
    private int _standingValueParamNameHash;
    private int _walkOrRunParamNameHash;
    private int _weaponTypeParamNameHash;
    private int _nearGroundParamNameHash;
    private int _jumpParamNameHash;

    private float _inAirValue = 0;
    private float _standingValue = 1;
    private float _walkOrRun = 0;
    private float _weaponTypeValue = 0;
    #endregion

    // gravity
    #region
    [Space]
    [Header("Gravity")]
    [SerializeField] private float _gravityMultiplyer = 2f;

    private Vector3 _groundCheckPosition;

    // private var
    private float _gravity = -9.81f;
    private float _velocityY;
    #endregion

    // jump
    #region
    [Space]
    [Header("Jump")]
    [SerializeField] private float _jumpForce = 7f;

    [SerializeField] private float _groundDistance = 0.1f;
    [SerializeField] private float _groundCheckRadius = 0.5f;
    [SerializeField] private LayerMask _groundLayerMask;
    [Space(5)]
    [SerializeField] private GunHolderAnimated _gunHolderAnimator;
    [Space(5)]
    [SerializeField] private float _jumpBumpAmount = 0.025f;
    [SerializeField] private float _jumpBumpAmountDuration = 0.35f;
    [SerializeField] private AnimationCurve _jumpBumpCurve;
    [Space(5)]
    [SerializeField] private float _hitTheGroundbumpAmount;
    [SerializeField] private float _hitTheGroundbumpAmountDuration;
    [SerializeField] private AnimationCurve _hitTheGroundbumpCurve;

    private Vector3 _bumpAmountVec = Vector3.zero;

    private AirState _airState = AirState.grounded;
    private AirState _previousAirState = AirState.grounded;

    public enum AirState
    {
        grounded,
        inAir,
    }

    #endregion

    // look
    #region
    [Space]
    [Header("Look")]
    [SerializeField] private Transform _centerSpinePos;

    [SerializeField] private float _sensY = 50;
    [SerializeField] private float _sensX = 50;

    [SerializeField] private MaxLookAmounts _maxLookAmounts;
    [Space(3)]

    [SerializeField] private float _lookMaxChangeSpeed = 40f;
    [SerializeField] private float _rotatingSlerpSpeed = 20f;

    private Vector3 _spineTargetRotation = Vector3.zero;
    private Vector3 _spineTargetRotationZ = Vector3.zero;
    private Vector3 _spineConstraintOffset = Vector3.zero;

    private float _lookUpLimit;
    private float _lookDownLimit;

    // private var
    private float _xRotation;
    private float _yRotation;

    #endregion

    // camera
    #region
    [Space]
    [Header("Camera")]
    [SerializeField] private Transform _cameraHolder;

    // private var
    private Vector3 _cameraTargetPosition;
    private Quaternion _cameraTargetRotation;

    private float _smoothCamToTargetSpeed = 10f;

    #endregion

    // Weapon
    #region
    [Space]
    [Header("Weapon")]
    [SerializeField] private Transform _rifleHolder;
    [SerializeField] private Transform _pistolHolder;
    // CHANGED
    //[SerializeField] private float _leftHandIKSmoothWeightSpeed = 20f;

    [Header("Hands Transform Constraints")]
    [SerializeField] private ConstraintsWeightModifier _iKBasedFingersWeightModifier;
    [SerializeField] private ConstraintsWeightModifier _rotationConstraitBasedFingersWeightModifier;

    [SerializeField] private HandsIKTransform _handsIKFollowers;
    [SerializeField] private HandsRotationConstraintTransforms _handsConstraintsFollowers;

    // aiming
    private AimState _aimState = AimState.normal;

    public enum AimState
    {
        normal,
        aiming,
    }

    // CHANGED
    //// reloading
    //[SerializeField] private TwoBoneIKConstraint _leftHandIK;

    // weapon
    private WeaponBase _currentWeapon;

    // Clip prevention
    [Space]
    [Header("Clip Prevention Settings")]
    [SerializeField] private Transform _gunHolder;
    [SerializeField] private Transform _clipProjectorPos;
    [SerializeField] private Transform _clipProjector;
    [SerializeField] private Transform _clipVisual;

    [SerializeField] private float _spineOffsetChangeSpeed = 20f;
    [SerializeField] private float _lerpPosChangeSpeed = 4f;

    [SerializeField] private LayerMask _clipCheckMask;

    [SerializeField] private List<MultiRotationConstraint> _spineConstraints;

    private Vector3 _spineRotationOffset = Vector3.zero;
    private Vector3 _initialPosition;

    private WeaponClippedState _weaponClippedState = WeaponClippedState.normal;

    public enum WeaponClippedState
    {
        clipped,
        normal,
    }

    private float _lerpPos;

    private bool _detectionClipped = false;

    #endregion

    // CHANGED

    // Weapon Switching
    #region
    [Space]
    [Header("Weapon Switching")]
    [SerializeField] private WeaponBase _primaryWeapon;
    [SerializeField] private WeaponBase _secondaryWeapon;
    [SerializeField] private WeaponBase _sidearmWeapon;
    [SerializeField] private WeaponBase _meleeWeapon;
    [Space]
    //[SerializeField] private Slot _rifleSlotOne;
    //[SerializeField] private Slot _rifleSlotTwo;
    //[SerializeField] private Slot _pistolSlot;
    //[SerializeField] private Slot _meleeSlot;
    //[Space]
    //[SerializeField] private Slot _gunHolderSlot;
    [SerializeField] private FollowTransformPosAndRot _rightHandFollower;
    [Space]
    [SerializeField] private ConstraintsWeightModifier _handsIKWeightModifier;
    [Space]
    /// use hash id instead... in vid
    [SerializeField] private string _switchOrHolsterTriggerName = "PerformSwitchOrHolster";
    [SerializeField] private string _weaponSwitchTypeName = "WeaponSwitchType";

    // CHANGED
    [SerializeField] private string _upperBodyLayerName = "UpperBody";
    #endregion

    // CHANGED

    // Leaning/Peeking
    #region
    [Space]
    [Header("Leaning / Peeking")]
    [SerializeField] private float _leaningInputSmoothingSpeed = 10f;
    [SerializeField] private float _leaningAngle = 25f;

    private float _leaningInput;
    #endregion

    // interactions
    #region
    [Space]
    [Header("Interactions")]
    [SerializeField] private Transform _interactPointer;

    [SerializeField] private float _interactMaxDistance = 1;

    [SerializeField] private LayerMask _interactableLayerMask;

    private IInteractable _detectedInteractable;

    private bool _isInInteractionRange = false;
    #endregion

    // Player UI
    #region
    [Header("UI")]
    [SerializeField] private PlayerUI _playerUI;
    #endregion

    #region General

    // initialization...
    void Awake()
    {
        // Create an instance of the player input actions.
        Input = new PlayerInputActions();

        // Initialize transform.
        _thisTransform = transform;

        // CurrentWeaponRef
        // CHANGED
        //_currentWeapon = GetComponentInChildren<WeaponBase>();
        _currentWeapon = _primaryWeapon;

        // Set the isCurrentWeapon bool.
        //_currentWeapon.SetCurrentWeapon(true);
        //_currentWeapon.SetControllerReference(this);
        //_currentWeapon.SetWeaponData(this, true);
    }

    void Start()
    {
        // Hide And Lock Cursor.
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Get the controller.
        _characterController = GetComponent<CharacterController>();

        // Get the initial position.
        _initialPosition = _gunHolder.localPosition;

        // Set the current weapons Hand IK Targets, So we don't have to set them even manually...
        SetAllIKFollowersTargetToTargets(_currentWeapon);

        // Init Cross Hair.
        _playerUI.SetCrossHairShow(_aimState != AimState.aiming);

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




        // CHANGED
        _switchToRifleInputWaitForSeconds = new WaitForSeconds(_switchToRifleInputDelay);
        _switchToPistolInputWaitForSeconds = new WaitForSeconds(_switchToPistolInputDelay);
        // CHANGED
    }

    // Update is called once per frame
    void Update()
    {
        // Call all methods
        HandleSpeedsAndPlayerHeight();
        HandleInputAndMove();
        HandleAnim();
        Look();
        HandleShooting();
        HandleAiming();

        if (Time.frameCount % detectInteractableInterval == 0) DetectInteractable();

        InteractInputCheck();
        HandleReloadingAndLeftHandIK();

        if (Time.frameCount % detectWeaponClipInterval == 0) DetectWeaponClip();

        UpdateClippedValues();



        // CHANGED
        HandleWeaponSwitching();
        // CHANGED



        HandlePlayerUI();
    }

    private void HandlePlayerUI()
    {
        // Update Weapon UI.
        _playerUI.SetCurrentWeaponUI(_currentWeapon);
    }

    #endregion

    #region player Camera

    private void LateUpdate()
    {
        SetCameraPositionAndRotation();
    }

    private void SetCameraPositionAndRotation()
    {
        // Check if clipped
        if (IsClipped()) _cameraTargetPosition = _currentWeapon.WeaponData.CameraClippedPosition;
        else if (IsAiming()) _cameraTargetPosition = _currentWeapon.WeaponData.CameraAimingPosition;
        else _cameraTargetPosition = _currentWeapon.WeaponData.CameraNormalPosition;

        // Add z off to camera target rotation.
        _cameraTargetRotation = Quaternion.Euler(0f, 0f, -_centerSpinePos.localRotation.eulerAngles.z);

        _cameraHolder.SetLocalPositionAndRotation(Vector3.Lerp(_cameraHolder.localPosition, _cameraTargetPosition, _smoothCamToTargetSpeed * Time.deltaTime), _cameraTargetRotation);
    }

    #endregion

    #region player Movement

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

    private void HandleSpeedsAndPlayerHeight()
    {
        // get the inputVector
        Vector2 inputVector = Input.Player.Move.ReadValue<Vector2>();

        // Sprint Check.
        if (Input.Player.Sprint.triggered && !CrouchToStandRaycast())
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
        if (Input.Player.Crouch.triggered && !CrouchToStandRaycast())
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
            if (Input.Player.Prone.triggered && !CrouchToStandRaycast())
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
        if (inputVector == Vector2.zero && _locomotionState == LocomotionState.sprinting)
        {
            _locomotionState = LocomotionState.walking;
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

    private bool CrouchToStandRaycast()
    {
        return Physics.Raycast(_thisTransform.position, Vector3.up, _crouchToStandCheckDistance, _groundLayerMask);
    }

    private void HandleInputAndMove()
    {
        // Input Vector
        Vector2 inputVector = Input.Player.Move.ReadValue<Vector2>();

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
        _currentInputVector = Vector2.SmoothDamp(_currentInputVector, inputVector, ref _smoothInputVelocity, _smoothInputSpeed);

        // Move input vector.
        _currentMoveInputVector = Vector2.SmoothDamp(_currentMoveInputVector, inputVector.normalized, ref _smoothMoveInputVelocity, _smoothInputSpeed);

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
        if (Input.Player.Jump.IsPressed() && _characterController.isGrounded && !CrouchToStandRaycast())
        {
            // Set the in air value
            _inAirValue = 0;

            // play jump Anim.
            _playerAnimator.SetTrigger(_jumpParamNameHash);

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
        _characterController.Move(_moveDirection * Time.deltaTime);
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

    private void HandleAnim()
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
        _weaponTypeValue = Mathf.Lerp(_weaponTypeValue, _currentWeapon.WeaponData.WeaponType == WeaponType.rifle ? 0 : 1, _animLerpingSpeed * Time.deltaTime);

        // Near Ground.
        _playerAnimator.SetBool(_nearGroundParamNameHash, _airState == AirState.grounded);

        // Locomotion floats
        _playerAnimator.SetFloat(_moveZParamNameHash, _currentInputVector.y);
        _playerAnimator.SetFloat(_moveXParamNameHash, _currentInputVector.x);
        _playerAnimator.SetFloat(_standingValueParamNameHash, _standingValue);
        _playerAnimator.SetFloat(_walkOrRunParamNameHash, _walkOrRun);
        _playerAnimator.SetFloat(_inAirParamNameHash, _inAirValue);

        // Turn
        _playerAnimator.SetFloat(_turnParamNameHash, Input.Player.Look.ReadValue<Vector2>().x * _sensX * _turningAmountMulti, 0.1f, Time.deltaTime);

        // Weapon floats
        _playerAnimator.SetFloat(_weaponTypeParamNameHash, _weaponTypeValue);
    }

    private void Look()
    {
        // get the lookVector and set it up
        Vector2 lookVector = Input.Player.Look.ReadValue<Vector2>();
        float mouseX = lookVector.x * Time.deltaTime * _sensX;
        float mouseY = lookVector.y * Time.deltaTime * _sensY;
        _yRotation += mouseX;
        _xRotation -= mouseY;

        // Handle Max Look Amounts.
        HandleMaxLookAmounts(IsProning(), Input.Player.Move.ReadValue<Vector2>() != Vector2.zero, _airState == AirState.inAir, ref _lookUpLimit, ref _lookDownLimit, _lookMaxChangeSpeed);

        // Clamp X Rot.
        _xRotation = Mathf.Clamp(_xRotation, -_lookUpLimit, _lookDownLimit);

        // setting the values to the objects
        _thisTransform.rotation = Quaternion.Slerp(_thisTransform.rotation, Quaternion.Euler(0, _yRotation, 0), _rotatingSlerpSpeed * Time.deltaTime);

        // LEANING

        // Set Input And Smooth.
        if (_isInInteractionRange == false) _leaningInput = Mathf.Lerp(_leaningInput, Input.Player.Lean.ReadValue<float>(), _leaningInputSmoothingSpeed * Time.deltaTime);
        else _leaningInput = Mathf.Lerp(_leaningInput, 0f, _leaningInputSmoothingSpeed * Time.deltaTime);

        // Leaning Amount.
        float leaningAmount = _leaningInput * _leaningAngle;

        // Spine Target.
        _spineTargetRotation.x = _xRotation;
        _spineTargetRotation.z = -leaningAmount;
        _spineTargetRotationZ.z = _spineTargetRotation.z;

        // Check if we are reloading.
        if (_currentWeapon._isReloading)
        {
            // Set Spine Followers Rotation Accordingly.
            SetSpineFollowersRotation(_spineTargetRotationZ, _spineTargetRotation, _rotatingSlerpSpeed);
        }
        else if (IsClipped()) // Check if is clipped is true
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
        _clipProjectorPos.localRotation = Quaternion.Slerp(_clipProjectorPos.localRotation, Quaternion.Euler(clipProjTarget), slerpingSpeed * Time.deltaTime);
        _centerSpinePos.localRotation = Quaternion.Slerp(_centerSpinePos.localRotation, Quaternion.Euler(centerSpineTarget), slerpingSpeed * Time.deltaTime);
    }

    #endregion

    #region player Weapon

    // Handle shooting
    private void HandleShooting()
    {
        // CHANGED Added Simpler Bool FUNC
        //if (Input.Player.Shoot.IsPressed() && !IsClipped())
        //{
        //    // Try Shooting.
        //    Shoot();
        //}
        if (Input.Player.Shoot.IsPressed() && CanShoot())
        {
            // Try Shooting.
            Shoot();
        }
    }

    private bool CanShoot()
    {
        return !IsClipped() && !_switchingInputPerformed && !_holstered;
    }

    // Shoot!
    private void Shoot()
    {
        bool didShoot = _currentWeapon.Shoot();

        if (didShoot == false) return;

        // Bump Rotation.
        _xRotation -= _currentWeapon.WeaponData.ShotBumpRotationAmount;
    }

    /// <summary>
    /// Takes in the direction, and max angle, randomizes the direction a bit and returns it.
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="maxAngle"></param>
    /// <returns></returns>
    Vector3 RandomizeDirection(Vector3 direction, float maxAngle)
    {
        // Get Random Angles.
        float angleY = Random.Range(-maxAngle, maxAngle);
        float angleX = Random.Range(-maxAngle, maxAngle);

        // Convert to rotations.
        Quaternion rotationY = Quaternion.AngleAxis(angleY, Vector3.up);
        Quaternion rotationX = Quaternion.AngleAxis(angleX, Vector3.right);

        // Final Rotation.
        Quaternion rotation = rotationY * rotationX;

        // Multiply with direction.
        return rotation * direction;
    }

    // aiming
    private void HandleAiming()
    {
        // check for input aim.
        // CHANGED !SPRINT CHECK.
        if (Input.Player.Aim.triggered && !IsSprinting())
        {
            // CHANGED
            //// toggle is aiming.
            //switch (_aimState)
            //{
            //    case AimState.aiming:
            //        // CHANGED
            //        //_aimState = AimState.normal;

            //        //_currentWeapon.InvokeOnAimExit();
            //        ChangeAimState(true);

            //        break;
            //    case AimState.normal:
            //        // CHANGED
            //        //_aimState = AimState.aiming;

            //        //_currentWeapon.InvokeOnAimEnter();
            //        ChangeAimState(false);

            //        break;
            //}

            ToggleAimState();

            //_playerUI.SetCrossHairShow(_aimState != AimState.aiming);
        }

        // CHANGED
        if (IsSprinting() && IsAiming()) _aimState = AimState.normal;
    }

    // CHANGED
    private void ToggleAimState()
    {
        _aimState = _aimState == AimState.normal ? AimState.aiming : AimState.normal;

        switch (_aimState)
        {
            case AimState.aiming:
                _currentWeapon.InvokeOnAimEnter();
                break;
            case AimState.normal:
                _currentWeapon.InvokeOnAimExit();
                break;
        }

        _playerUI.SetCrossHairShow(_aimState == AimState.normal);
    }

    // HandleReloading
    private void HandleReloadingAndLeftHandIK()
    {
        // Check if reload input.
        if (Input.Player.Reload.triggered)
        {
            // Try Reload.
            TryReload();
        }
        else if (Input.Player.Shoot.IsPressed() && _currentWeapon.CurrentAmmo == 0)
        {
            // Try Reload.
            TryReload();
        }

        // Left Hand IK
        // CHANGED
        //_leftHandIK.weight = Mathf.Lerp(_leftHandIK.weight, 1f, Time.deltaTime * _leftHandIKSmoothWeightSpeed);
    }

    /// <summary>
    /// Call this function when you want to try to reload.
    /// Checks if we can reload, And Relaods if so.
    /// </summary>
    private void TryReload()
    {
        // CHANGED Added Simpler Bool FUNC
        //if (!IsClipped() && !_isInInteractionRange)
        //{
        //    _currentWeapon.StartReloading();
        //}
        if (CanReload())
        {
            _currentWeapon.StartReloading();
        }
    }

    private bool CanReload()
    {
        return !IsClipped() && !_isInInteractionRange && !_switchingInputPerformed && !_holstered;
    }

    private void DetectWeaponClip()
    {
        _clipProjector.localPosition = _currentWeapon.WeaponData.ClipProjectorPosition;

        _clipVisual.localScale = _weaponClippedState == WeaponClippedState.clipped
            ? _currentWeapon.WeaponData.BoxCastClippedSize
            : _currentWeapon.WeaponData.BoxCastSize;

        _detectionClipped = IsWeaponClipped();

        _weaponClippedState = _detectionClipped ? WeaponClippedState.clipped : WeaponClippedState.normal;

        // CHANGED
        //_currentWeapon.transform.localPosition = _currentWeapon.WeaponData.WeaponPositionOffset;
    }

    private void UpdateClippedValues()
    {
        SetSpineConstraintsOffset(_detectionClipped);
        UpdateConstraintsWeights();
        UpdateWeaponTransform();
    }

    private bool IsWeaponClipped()
    {
        bool isClipped = Physics.CheckBox(_clipProjector.position,
                                          _currentWeapon.WeaponData.BoxCastSize / 2f,
                                          _clipProjector.rotation,
                                          _clipCheckMask);

        if (!isClipped && IsClipped())
        {
            isClipped = Physics.CheckBox(_clipProjector.position,
                                          _currentWeapon.WeaponData.BoxCastClippedSize / 2f,
                                          _clipProjector.rotation,
                                          _clipCheckMask);
        }

        return isClipped;
    }

    private void SetSpineConstraintsOffset(bool clipped)
    {
        _lerpPos = Mathf.Lerp(_lerpPos, clipped ? 1f : 0f, _lerpPosChangeSpeed * Time.deltaTime);

        float targetOffsetY = clipped ? 0f : _currentWeapon.WeaponData.SpineConstraintOffsetY;

        _spineRotationOffset.y = Mathf.Lerp(_spineConstraints[0].data.offset.y, targetOffsetY, _spineOffsetChangeSpeed * Time.deltaTime);

        for (int i = 0; i < _spineConstraints.Count; i++)
        {
            _spineConstraints[i].data.offset = _spineRotationOffset;
        }
    }

    private void UpdateConstraintsWeights()
    {
        // CHANGED
        //float ikWeight = _currentWeapon.HandsConstraintType == HandsConstraintType.IKBasedFingers ? 1f : 0f;

        //float rotationWeight = 1f - ikWeight;
        //float ikLerpSpeed = 10f;

        //_iKBasedFingersWeightModifier.SetWeight(Mathf.Lerp(_iKBasedFingersWeightModifier.GetWeight(), ikWeight, ikLerpSpeed * Time.deltaTime));
        //_rotationConstraitBasedFingersWeightModifier.SetWeight(Mathf.Lerp(_rotationConstraitBasedFingersWeightModifier.GetWeight(),
        //    rotationWeight, ikLerpSpeed * Time.deltaTime));

        float ikWeight = _currentWeapon.HandsConstraintType == HandsConstraintType.IKBasedFingers ? 1f : 0f;

        float rotationWeight = 1f - ikWeight;
        float ikLerpSpeed = 20f;

        if (_isPerformingSwitch || _holstered)
        {
            _iKBasedFingersWeightModifier.SetWeight(Mathf.Lerp(_iKBasedFingersWeightModifier.GetWeight(), 0, ikLerpSpeed * Time.deltaTime));
            _rotationConstraitBasedFingersWeightModifier.SetWeight(Mathf.Lerp(_rotationConstraitBasedFingersWeightModifier.GetWeight(),
                0, ikLerpSpeed * Time.deltaTime));

            float targetWeight = Mathf.Lerp(_handsIKWeightModifier.GetWeight(), 0, ikLerpSpeed * Time.deltaTime);

            _handsIKWeightModifier.SetWeight(targetWeight);
        }
        else
        {
            _iKBasedFingersWeightModifier.SetWeight(Mathf.Lerp(_iKBasedFingersWeightModifier.GetWeight(), ikWeight, ikLerpSpeed * Time.deltaTime));
            _rotationConstraitBasedFingersWeightModifier.SetWeight(Mathf.Lerp(_rotationConstraitBasedFingersWeightModifier.GetWeight(),
                rotationWeight, ikLerpSpeed * Time.deltaTime));

            float targetWeight = Mathf.Lerp(_handsIKWeightModifier.GetWeight(), 1, ikLerpSpeed * Time.deltaTime);

            _handsIKWeightModifier.SetWeight(targetWeight);
        }

        // CHANGED
        int upperBodyLayer = _playerAnimator.GetLayerIndex(_upperBodyLayerName);

        float upperBodyWeight = Mathf.Lerp(_playerAnimator.GetLayerWeight(upperBodyLayer), _holstered ? 0 : 1, ikLerpSpeed * Time.deltaTime);

        _playerAnimator.SetLayerWeight(upperBodyLayer, upperBodyWeight);
        // CHANGED
    }

    private void UpdateWeaponTransform()
    {
        _gunHolder.SetLocalPositionAndRotation(Vector3.Lerp(_initialPosition, _currentWeapon.WeaponData.NewPosition, _lerpPos),
            Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(_currentWeapon.WeaponData.NewRotation), _lerpPos));

        // CHANGED
        float offsetPositioningSpeed = 20;
        if (!_isPerformingSwitch) _currentWeapon.transform.localPosition = Vector3.Lerp(_currentWeapon.transform.localPosition,
            _currentWeapon.WeaponData.WeaponPositionOffset, Time.deltaTime * offsetPositioningSpeed);
    }

    // CHANGED

    #region Var

    private WaitForSeconds _switchToRifleInputWaitForSeconds;
    private WaitForSeconds _switchToPistolInputWaitForSeconds;

    [Space]
    [SerializeField] private float _switchToRifleInputDelay = 0.6f;
    [SerializeField] private float _switchToPistolInputDelay = 0.3f;
    [Space]
    [SerializeField] private int _holsterPistolOrKnifeID = 0;
    [SerializeField] private int _holsterRifleID = 1;
    [SerializeField] private int _switchBetweenPistolOrKnifeID = 2;
    [SerializeField] private int _switchBetweenRifleID = 3;
    [SerializeField] private int _switchFromKnifeOrPistolToRifleID = 4;
    [SerializeField] private int _switchFromRifleToKnifeID = 5;
    [SerializeField] private int _switchFromRifleToPistolID = 6;
    [SerializeField] private int _unHolsterPistolOrKnifeID = 7;
    [SerializeField] private int _unHolsterRifleID = 8;

    private bool _isPerformingSwitch = false;
    private bool _switchingInputPerformed = false;
    private bool _holstered = false;

    private int animToPlayID = 0;

    #endregion

    #region General

    private void HandleWeaponSwitching()
    {
        // Check Input And Perform Weapon Switching/Holstering.
        if (CanPerformSwitching())
        {
            if (Input.Player.SwitchToRifle.triggered) StartCoroutine(TrySwitchFromCurrentToRifleDelay());

            if (Input.Player.SwitchToPistol.triggered) StartCoroutine(TrySwitchFromCurrentToPistolDelay());

            if (Input.Player.SwitchToKnife.triggered) TrySwitchFromCurrentToKnife();
        }

        if (CanPerformHolstering())
        {
            if (Input.Player.Holster.triggered) TryHolsterOrUnHolsterCurrentWeapon();
        }
    }

    private bool CanPerformSwitching()
    {
        return !_isPerformingSwitch && !_currentWeapon._isReloading;
    }

    private bool CanPerformHolstering()
    {
        return !_switchingInputPerformed && !_currentWeapon._isReloading;
    }

    #endregion

    #region Try Switch
    private void TrySwitchFromCurrentToKnife()
    {
        UpdateSwitchingState(true);
        UpdateSwitchingInputPerformed(true);

        //_currentWeapon.SetWeaponData(this, false);

        MoveWeaponToRightHandSlot(false);

        switch (_currentWeapon.WeaponData.WeaponType)
        {
            case WeaponType.rifle:
                animToPlayID = _switchFromRifleToKnifeID;
                break;
            case WeaponType.pistol:
                animToPlayID = _switchBetweenPistolOrKnifeID;
                break;
            case WeaponType.knife:
                UpdateSwitchingState(false);
                return;
        }

        PerformWeaponSwitchAnimation();
    }
    private void TryHolsterOrUnHolsterCurrentWeapon()
    {
        // Try to holster the current weapon.
        UpdateSwitchingInputPerformed(true);
        UpdateSwitchingState(true);

        if (!_holstered)
        {
            //_currentWeapon.SetWeaponData(this, false);

            MoveWeaponToRightHandSlot(false);
        }

        switch (_currentWeapon.WeaponData.WeaponType)
        {
            case WeaponType.rifle:
                if (!_holstered) animToPlayID = _holsterRifleID;
                else animToPlayID = _unHolsterRifleID;
                break;
            case WeaponType.pistol:
                if (!_holstered) animToPlayID = _holsterPistolOrKnifeID;
                else animToPlayID = _unHolsterPistolOrKnifeID;
                break;
            case WeaponType.knife:
                if (!_holstered) animToPlayID = _holsterPistolOrKnifeID;
                else animToPlayID = _unHolsterPistolOrKnifeID;
                break;
        }

        if (_holstered) _holstered = false;

        PerformWeaponSwitchAnimation();
    }
    #endregion

    #region Switch Delayed Functions
    private IEnumerator TrySwitchFromCurrentToRifleDelay()
    {
        UpdateSwitchingInputPerformed(true);
        yield return _switchToRifleInputWaitForSeconds;

        if (!_isPerformingSwitch) SwitchFromCurrentToRifle();
    }
    private IEnumerator TrySwitchFromCurrentToPistolDelay()
    {
        UpdateSwitchingInputPerformed(true);
        yield return _switchToPistolInputWaitForSeconds;

        if (!_isPerformingSwitch && _currentWeapon.WeaponData.WeaponType != WeaponType.pistol) SwitchFromCurrentToPistol();
    }

    private void SwitchFromCurrentToRifle()
    {
        UpdateSwitchingState(true);
        MoveWeaponToRightHandSlot();

        //_currentWeapon.SetWeaponData(this, false);

        switch (_currentWeapon.WeaponData.WeaponType)
        {
            case WeaponType.rifle:
                animToPlayID = _switchBetweenRifleID;
                break;
            case WeaponType.pistol:
                animToPlayID = _switchFromKnifeOrPistolToRifleID;
                break;
            case WeaponType.knife:
                animToPlayID = _switchFromKnifeOrPistolToRifleID;
                break;
        }

        PerformWeaponSwitchAnimation();
    }
    private void SwitchFromCurrentToPistol()
    {
        UpdateSwitchingState(true);
        MoveWeaponToRightHandSlot();

        //_currentWeapon.SetWeaponData(this, false);

        switch (_currentWeapon.WeaponData.WeaponType)
        {
            case WeaponType.rifle:
                animToPlayID = _switchFromRifleToPistolID;
                break;
            case WeaponType.pistol:
                UpdateSwitchingState(false);
                return;
            case WeaponType.knife:
                animToPlayID = _switchBetweenPistolOrKnifeID;
                break;
        }

        PerformWeaponSwitchAnimation();
    }
    #endregion

    #region Animation Events
    public void OnWeaponSwitchComplete()
    {
        UpdateSwitchingState(false);
        UpdateSwitchingInputPerformed(false);
        //_currentWeapon.SetWeaponData(this, true);
    }
    public void SwitchBetweenRifle(int step)
    {
        switch (step)
        {
            case 1:
                //_currentWeapon.SetWeaponData(this, false);

                //if (!_rifleSlotOne.HasObjectInSlot().hasObject)
                //{
                //    _rifleSlotOne.SnapToSocket(_currentWeapon.transform);
                //    _currentWeapon = _rifleSlotTwo.HasObjectInSlot().objectTransforms[0].GetComponent<WeaponBase>();
                //}
                //else if (!_rifleSlotTwo.HasObjectInSlot().hasObject)
                //{
                //    _rifleSlotTwo.SnapToSocket(_currentWeapon.transform);
                //    _currentWeapon = _rifleSlotOne.HasObjectInSlot().objectTransforms[0].GetComponent<WeaponBase>();
                //}

                //_currentWeapon.SetWeaponData(this, true);

                break;
            case 2:

                MoveWeaponToRightHandSlot();
                SetAllIKFollowersTargetToTargets(_currentWeapon);
                break;
            case 3:
                //_gunHolderSlot.SnapToSocket(_currentWeapon.transform);
                break;
        }
    }
    public void SwitchBetweenPistolOrKnife(int step)
    {
        switch (step)
        {
            case 1:

                //_currentWeapon.SetWeaponData(this, false);

                //if (!_pistolSlot.HasObjectInSlot().hasObject)
                //{
                //    _pistolSlot.SnapToSocket(_currentWeapon.transform);
                //    _currentWeapon = _meleeSlot.HasObjectInSlot().objectTransforms[0].GetComponent<WeaponBase>();
                //}
                //else if (!_meleeSlot.HasObjectInSlot().hasObject)
                //{
                //    _meleeSlot.SnapToSocket(_currentWeapon.transform);
                //    _currentWeapon = _pistolSlot.HasObjectInSlot().objectTransforms[0].GetComponent<WeaponBase>();
                //}

                //_currentWeapon.SetWeaponData(this, true);

                break;
            case 2:

                MoveWeaponToRightHandSlot();
                SetAllIKFollowersTargetToTargets(_currentWeapon);

                break;
            case 3:

                //_gunHolderSlot.SnapToSocket(_currentWeapon.transform);

                break;
        }
    }
    public void SwitchFromPistolOrKnifeToRifle(int step)
    {
        switch (step)
        {
            case 1:

                //_currentWeapon.SetWeaponData(this, false);

                switch (_currentWeapon.WeaponData.WeaponType)
                {
                    case WeaponType.pistol:
                        //_pistolSlot.SnapToSocket(_currentWeapon.transform);
                        break;
                    case WeaponType.knife:
                        //_meleeSlot.SnapToSocket(_currentWeapon.transform);
                        break;
                }

                //_currentWeapon = _rifleSlotTwo.HasObjectInSlot().objectTransforms[0].GetComponent<WeaponBase>();

                //_currentWeapon.SetWeaponData(this, true);

                break;
            case 2:

                MoveWeaponToRightHandSlot();
                SetAllIKFollowersTargetToTargets(_currentWeapon);

                break;
            case 3:

                //_gunHolderSlot.SnapToSocket(_currentWeapon.transform);

                break;
        }
    }
    public void SwitchFromRifleToPistol(int step)
    {
        switch (step)
        {
            case 1:
                //_currentWeapon.SetWeaponData(this, false);

                //if (!_rifleSlotOne.HasObjectInSlot().hasObject) _rifleSlotOne.SnapToSocket(_currentWeapon.transform);
                //else if (!_rifleSlotTwo.HasObjectInSlot().hasObject) _rifleSlotTwo.SnapToSocket(_currentWeapon.transform);

                //_currentWeapon = _pistolSlot.HasObjectInSlot().objectTransforms[0].GetComponent<WeaponBase>();

                //_currentWeapon.SetWeaponData(this, true);

                break;
            case 2:

                MoveWeaponToRightHandSlot();
                SetAllIKFollowersTargetToTargets(_currentWeapon);

                break;
            case 3:
                //_gunHolderSlot.SnapToSocket(_currentWeapon.transform);
                break;
        }
    }
    public void SwitchFromRifleToKnife(int step)
    {
        switch (step)
        {
            case 1:
                //_currentWeapon.SetWeaponData(this, false);

                //if (!_rifleSlotOne.HasObjectInSlot().hasObject) _rifleSlotOne.SnapToSocket(_currentWeapon.transform);
                //else if (!_rifleSlotTwo.HasObjectInSlot().hasObject) _rifleSlotTwo.SnapToSocket(_currentWeapon.transform);

                //_currentWeapon = _meleeSlot.HasObjectInSlot().objectTransforms[0].GetComponent<WeaponBase>();

                //_currentWeapon.SetWeaponData(this, true);

                break;
            case 2:

                MoveWeaponToRightHandSlot();
                SetAllIKFollowersTargetToTargets(_currentWeapon);

                break;
            case 3:
                //_gunHolderSlot.SnapToSocket(_currentWeapon.transform);
                break;
        }
    }
    public void Holster()
    {
        _holstered = true;
        //_currentWeapon.SetWeaponData(this, false);

        switch (_currentWeapon.WeaponData.WeaponType)
        {
            case WeaponType.rifle:
                //if (!_rifleSlotOne.HasObjectInSlot().hasObject) _rifleSlotOne.SnapToSocket(_currentWeapon.transform);
                //else if (!_rifleSlotTwo.HasObjectInSlot().hasObject) _rifleSlotTwo.SnapToSocket(_currentWeapon.transform);
                break;
            case WeaponType.pistol:
                //_pistolSlot.SnapToSocket(_currentWeapon.transform);
                break;
            case WeaponType.knife:
                //_meleeSlot.SnapToSocket(_currentWeapon.transform);
                break;
        }
    }
    public void UnHolster(int step)
    {
        switch (step)
        {
            case 1:
                MoveWeaponToRightHandSlot();
                break;
            case 2:
                //_gunHolderSlot.SnapToSocket(_currentWeapon.transform);
                break;
        }
    }
    #endregion

    // //
    #region Useful Functions
    private void MoveWeaponToRightHandSlot(bool smooth = true)
    {
        Quaternion prevWeaponRotation = _currentWeapon.transform.rotation;
        _rightHandFollower.transform.rotation = Quaternion.identity;
        _currentWeapon.transform.rotation = Quaternion.identity;

        Vector3 _offset = (_currentWeapon.HandsConstraintType == HandsConstraintType.IKBasedFingers ?
            _currentWeapon.HandsIKTargets.RightHandIKTransform.position :
            _currentWeapon.HandsRotationConstraintTransforms.RightHandIKTransform.position) -
            _currentWeapon.transform.position;

        _currentWeapon.transform.rotation = prevWeaponRotation;

        if (_currentWeapon.HandsConstraintType == HandsConstraintType.IKBasedFingers)
        {
            // IK Based Fingers.
            Quaternion rotation = Quaternion.Inverse(_currentWeapon.transform.rotation) *
                _currentWeapon.HandsIKTargets.RightHandIKTransform.rotation;

            _rightHandFollower.RotationOffset = Quaternion.Inverse(rotation).eulerAngles;
        }
        else
        {
            // Rotation Based Fingers.
            Quaternion rotation = Quaternion.Inverse(_currentWeapon.transform.rotation) *
                _currentWeapon.HandsRotationConstraintTransforms.RightHandIKTransform.rotation;

            _rightHandFollower.RotationOffset = Quaternion.Inverse(rotation).eulerAngles;
        }

        //_rightHandFollower.CalcAndApply();

        _currentWeapon.transform.parent = _rightHandFollower.transform;
        StartCoroutine(SmoothTransformLocalToTarget(_currentWeapon.transform, smooth, -_offset, Vector3.zero));
    }

    private IEnumerator SmoothTransformLocalToTarget(Transform transformM, bool smooth = true,
        Vector3 targetLPosition = default, Vector3 targetLRotation = default, float smoothTime = 0.1f)
    {
        // Keep track of initial position and rotation
        Vector3 localPosition = transformM.localPosition;
        Quaternion localRotation = transformM.localRotation;

        // Start smooth transition
        float elapsedTime = 0f;

        if (smooth)
        {
            while (elapsedTime < smoothTime)
            {
                transformM.localPosition = Vector3.Lerp(localPosition, targetLPosition, elapsedTime / smoothTime);
                transformM.localRotation = Quaternion.Lerp(localRotation, Quaternion.Euler(targetLRotation), elapsedTime / smoothTime);

                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }

        transformM.localPosition = targetLPosition;
        transformM.localRotation = Quaternion.Euler(targetLRotation);
    }

    private void UpdateSwitchingState(bool state)
    {
        _isPerformingSwitch = state;
    }

    private void UpdateSwitchingInputPerformed(bool performed)
    {
        _switchingInputPerformed = performed;
    }

    private void PerformWeaponSwitchAnimation()
    {
        _playerAnimator.SetFloat(_weaponSwitchTypeName, animToPlayID);
        _playerAnimator.SetTrigger(_switchOrHolsterTriggerName);
    }
    #endregion

    // CHANGED

    #endregion

    #region player Interactions

    private void DetectInteractable()
    {
        if (!_currentWeapon._isReloading && Physics.Raycast(_interactPointer.position, _interactPointer.forward,
            out RaycastHit hit, _interactMaxDistance, _interactableLayerMask) &&
            hit.transform.TryGetComponent(out _detectedInteractable))
        {
            //if (!_detectedInteractable.CanInteract(this)) return;

            HandleInteractUIState(true, _detectedInteractable);
        }
        else
        {
            HandleInteractUIState(false);
        }
    }

    private void InteractInputCheck()
    {
        if (_isInInteractionRange && Input.Player.Interact.triggered)
        {
            //_detectedInteractable.Interact(this);
        }
    }
    private void HandleInteractUIState(bool state, IInteractable interactable = null)
    {
        _playerUI.SetInteractUIShow(state, interactable);
        _isInInteractionRange = state;
    }

    #endregion

    #region Exposed Methods

    // Functions
    public void SwapWeapon(WeaponBase newWeapon)
    {
        Vector3 newWeaponPickupPosition = newWeapon.transform.position;
        Quaternion newWeaponPickupRotation = newWeapon.transform.rotation;

        _currentWeapon.transform.SetParent(newWeapon.transform.parent);
        _currentWeapon.transform.SetPositionAndRotation(newWeaponPickupPosition, newWeaponPickupRotation);
        // CHANGED
        //_currentWeapon.SetCurrentWeapon(false);
        //_currentWeapon.SetWeaponData(this, false);

        if (newWeapon.WeaponData.WeaponType == WeaponType.rifle)
        {
            newWeapon.transform.SetParent(_rifleHolder);
            newWeapon.transform.SetPositionAndRotation(_rifleHolder.position, _rifleHolder.rotation);
        }
        else if (newWeapon.WeaponData.WeaponType == WeaponType.pistol)
        {
            newWeapon.transform.SetParent(_pistolHolder);
            newWeapon.transform.SetPositionAndRotation(_pistolHolder.position, _pistolHolder.rotation);
        }

        // Local Position Offsetting.
        newWeapon.transform.localPosition = newWeapon.WeaponData.WeaponPositionOffset;

        // Set Current Weapon Is used everywhere instead of iscurrentweapon as we made that private.
        // CHANGED
        //newWeapon.SetCurrentWeapon(true);
        //newWeapon.SetControllerReference(this);
        //newWeapon.SetWeaponData(this, true);

        // Set all ik targets.
        SetAllIKFollowersTargetToTargets(newWeapon);

        // Set the new weapon to the current.
        _currentWeapon = newWeapon;
    }

    private void SetAllIKFollowersTargetToTargets(WeaponBase weapon)
    {
        if (weapon.HandsConstraintType == HandsConstraintType.IKBasedFingers)
        {
            // Left Hand IK.
            //Hand
            SetIKFollowersTargetToTarget(_handsIKFollowers.LeftHandIKTransform, weapon.HandsIKTargets.LeftHandIKTransform);

            //Fingers
            SetIKFollowersTargetToTarget(_handsIKFollowers.LeftHandIndexIKTransform, weapon.HandsIKTargets.LeftHandIndexIKTransform);
            SetIKFollowersTargetToTarget(_handsIKFollowers.LeftHandMiddleIKTransform, weapon.HandsIKTargets.LeftHandMiddleIKTransform);
            SetIKFollowersTargetToTarget(_handsIKFollowers.LeftHandPinkyIKTransform, weapon.HandsIKTargets.LeftHandPinkyIKTransform);
            SetIKFollowersTargetToTarget(_handsIKFollowers.LeftHandRingIKTransform, weapon.HandsIKTargets.LeftHandRingIKTransform);
            SetIKFollowersTargetToTarget(_handsIKFollowers.LeftHandThumbIKTransform, weapon.HandsIKTargets.LeftHandThumbIKTransform);

            // Right Hand IK.
            //Hand
            SetIKFollowersTargetToTarget(_handsIKFollowers.RightHandIKTransform, weapon.HandsIKTargets.RightHandIKTransform);

            //Fingers
            SetIKFollowersTargetToTarget(_handsIKFollowers.RightHandIndexIKTransform, weapon.HandsIKTargets.RightHandIndexIKTransform);
            SetIKFollowersTargetToTarget(_handsIKFollowers.RightHandMiddleIKTransform, weapon.HandsIKTargets.RightHandMiddleIKTransform);
            SetIKFollowersTargetToTarget(_handsIKFollowers.RightHandPinkyIKTransform, weapon.HandsIKTargets.RightHandPinkyIKTransform);
            SetIKFollowersTargetToTarget(_handsIKFollowers.RightHandRingIKTransform, weapon.HandsIKTargets.RightHandRingIKTransform);
            SetIKFollowersTargetToTarget(_handsIKFollowers.RightHandThumbIKTransform, weapon.HandsIKTargets.RightHandThumbIKTransform);
        }
        else
        {
            // Left Hand IK.
            //Hand
            SetIKFollowersTargetToTarget(_handsConstraintsFollowers.LeftHandIKTransform, weapon.HandsRotationConstraintTransforms.LeftHandIKTransform);

            //Fingers
            SetConstraintFollowersTargetToTarget(_handsConstraintsFollowers.LeftHandIndex1ConstraintTransform, weapon.HandsRotationConstraintTransforms.LeftHandIndex1ConstraintTransform);
            SetConstraintFollowersTargetToTarget(_handsConstraintsFollowers.LeftHandIndex2ConstraintTransform, weapon.HandsRotationConstraintTransforms.LeftHandIndex2ConstraintTransform);
            SetConstraintFollowersTargetToTarget(_handsConstraintsFollowers.LeftHandIndex3ConstraintTransform, weapon.HandsRotationConstraintTransforms.LeftHandIndex3ConstraintTransform);

            SetConstraintFollowersTargetToTarget(_handsConstraintsFollowers.LeftHandMiddle1ConstraintTransform, weapon.HandsRotationConstraintTransforms.LeftHandMiddle1ConstraintTransform);
            SetConstraintFollowersTargetToTarget(_handsConstraintsFollowers.LeftHandMiddle2ConstraintTransform, weapon.HandsRotationConstraintTransforms.LeftHandMiddle2ConstraintTransform);
            SetConstraintFollowersTargetToTarget(_handsConstraintsFollowers.LeftHandMiddle3ConstraintTransform, weapon.HandsRotationConstraintTransforms.LeftHandMiddle3ConstraintTransform);

            SetConstraintFollowersTargetToTarget(_handsConstraintsFollowers.LeftHandPinky1ConstraintTransform, weapon.HandsRotationConstraintTransforms.LeftHandPinky1ConstraintTransform);
            SetConstraintFollowersTargetToTarget(_handsConstraintsFollowers.LeftHandPinky2ConstraintTransform, weapon.HandsRotationConstraintTransforms.LeftHandPinky2ConstraintTransform);
            SetConstraintFollowersTargetToTarget(_handsConstraintsFollowers.LeftHandPinky3ConstraintTransform, weapon.HandsRotationConstraintTransforms.LeftHandPinky3ConstraintTransform);

            SetConstraintFollowersTargetToTarget(_handsConstraintsFollowers.LeftHandRing1ConstraintTransform, weapon.HandsRotationConstraintTransforms.LeftHandRing1ConstraintTransform);
            SetConstraintFollowersTargetToTarget(_handsConstraintsFollowers.LeftHandRing2ConstraintTransform, weapon.HandsRotationConstraintTransforms.LeftHandRing2ConstraintTransform);
            SetConstraintFollowersTargetToTarget(_handsConstraintsFollowers.LeftHandRing3ConstraintTransform, weapon.HandsRotationConstraintTransforms.LeftHandRing3ConstraintTransform);

            SetConstraintFollowersTargetToTarget(_handsConstraintsFollowers.LeftHandThumb1ConstraintTransform, weapon.HandsRotationConstraintTransforms.LeftHandThumb1ConstraintTransform);
            SetConstraintFollowersTargetToTarget(_handsConstraintsFollowers.LeftHandThumb2ConstraintTransform, weapon.HandsRotationConstraintTransforms.LeftHandThumb2ConstraintTransform);
            SetConstraintFollowersTargetToTarget(_handsConstraintsFollowers.LeftHandThumb3ConstraintTransform, weapon.HandsRotationConstraintTransforms.LeftHandThumb3ConstraintTransform);

            // Right Hand IK.
            //Hand
            SetIKFollowersTargetToTarget(_handsConstraintsFollowers.RightHandIKTransform, weapon.HandsRotationConstraintTransforms.RightHandIKTransform);

            //Fingers
            SetConstraintFollowersTargetToTarget(_handsConstraintsFollowers.RightHandIndex1ConstraintTransform, weapon.HandsRotationConstraintTransforms.RightHandIndex1ConstraintTransform);
            SetConstraintFollowersTargetToTarget(_handsConstraintsFollowers.RightHandIndex2ConstraintTransform, weapon.HandsRotationConstraintTransforms.RightHandIndex2ConstraintTransform);
            SetConstraintFollowersTargetToTarget(_handsConstraintsFollowers.RightHandIndex3ConstraintTransform, weapon.HandsRotationConstraintTransforms.RightHandIndex3ConstraintTransform);

            SetConstraintFollowersTargetToTarget(_handsConstraintsFollowers.RightHandMiddle1ConstraintTransform, weapon.HandsRotationConstraintTransforms.RightHandMiddle1ConstraintTransform);
            SetConstraintFollowersTargetToTarget(_handsConstraintsFollowers.RightHandMiddle2ConstraintTransform, weapon.HandsRotationConstraintTransforms.RightHandMiddle2ConstraintTransform);
            SetConstraintFollowersTargetToTarget(_handsConstraintsFollowers.RightHandMiddle3ConstraintTransform, weapon.HandsRotationConstraintTransforms.RightHandMiddle3ConstraintTransform);

            SetConstraintFollowersTargetToTarget(_handsConstraintsFollowers.RightHandPinky1ConstraintTransform, weapon.HandsRotationConstraintTransforms.RightHandPinky1ConstraintTransform);
            SetConstraintFollowersTargetToTarget(_handsConstraintsFollowers.RightHandPinky2ConstraintTransform, weapon.HandsRotationConstraintTransforms.RightHandPinky2ConstraintTransform);
            SetConstraintFollowersTargetToTarget(_handsConstraintsFollowers.RightHandPinky3ConstraintTransform, weapon.HandsRotationConstraintTransforms.RightHandPinky3ConstraintTransform);

            SetConstraintFollowersTargetToTarget(_handsConstraintsFollowers.RightHandRing1ConstraintTransform, weapon.HandsRotationConstraintTransforms.RightHandRing1ConstraintTransform);
            SetConstraintFollowersTargetToTarget(_handsConstraintsFollowers.RightHandRing2ConstraintTransform, weapon.HandsRotationConstraintTransforms.RightHandRing2ConstraintTransform);
            SetConstraintFollowersTargetToTarget(_handsConstraintsFollowers.RightHandRing3ConstraintTransform, weapon.HandsRotationConstraintTransforms.RightHandRing3ConstraintTransform);

            SetConstraintFollowersTargetToTarget(_handsConstraintsFollowers.RightHandThumb1ConstraintTransform, weapon.HandsRotationConstraintTransforms.RightHandThumb1ConstraintTransform);
            SetConstraintFollowersTargetToTarget(_handsConstraintsFollowers.RightHandThumb2ConstraintTransform, weapon.HandsRotationConstraintTransforms.RightHandThumb2ConstraintTransform);
            SetConstraintFollowersTargetToTarget(_handsConstraintsFollowers.RightHandThumb3ConstraintTransform, weapon.HandsRotationConstraintTransforms.RightHandThumb3ConstraintTransform);
        }
    }

    private void SetIKFollowersTargetToTarget(Transform follower, Transform target)
    {
        follower.GetComponent<FollowTransformPosAndRot>().Target = target;
    }

    private void SetConstraintFollowersTargetToTarget(Transform follower, Transform target)
    {
        follower.GetComponent<FollowTransformRot>().Target = target;
    }

    // Variable Returners
    public WeaponBase CurrentWeapon()
    {
        return _currentWeapon;
    }

    /// <summary>
    /// Returns true if the loco state is walking
    /// </summary>
    /// <returns></returns>
    public bool IsWalking()
    {
        return _locomotionState == LocomotionState.walking;
    }

    /// <summary>
    /// Returns true if the loco state is sprinting
    /// </summary>
    /// <returns></returns>
    public bool IsSprinting()
    {
        return _locomotionState == LocomotionState.sprinting;
    }

    /// <summary>
    /// Returns true if the loco state is crouching
    /// </summary>
    /// <returns></returns>
    public bool IsCrouching()
    {
        return _locomotionState == LocomotionState.crouching;
    }

    /// <summary>
    /// Returns true if the loco state is proning
    /// </summary>
    /// <returns></returns>
    public bool IsProning()
    {
        return _locomotionState == LocomotionState.proning;
    }

    /// <summary>
    /// Returns aimState = aiming
    /// </summary>
    /// <returns></returns>
    public bool IsAiming()
    {
        return _aimState == AimState.aiming;
    }

    public bool IsGrounded()
    {
        return _airState == AirState.grounded;
    }

    public float GetXRotation()
    {
        return _xRotation;
    }

    public float GetLookUpLimit()
    {
        return _lookUpLimit;
    }

    public float GetLookDownLimit()
    {
        return _lookDownLimit;
    }

    public bool IsClipped()
    {
        return _weaponClippedState == WeaponClippedState.clipped;
    }

    #endregion

    #region Input Enabling & Disabling Methods
    private void OnEnable()
    {
        Input.Enable();
    }
    private void OnDisable()
    {
        Input.Disable();
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmos()
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
}
// Great 1872 lines of code already...