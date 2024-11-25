using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

namespace HKFps
{
    public class HKPlayerWeaponManager : MonoBehaviour
    {

        #region General

        [Space]
        [Header("Weapon")]
        [SerializeField] private Transform _gunHolder;
        [Space]
        [Header("Hands Transform Constraints")]
        [SerializeField] private ConstraintsWeightModifier _iKBasedFingersWeightModifier;
        [SerializeField] private ConstraintsWeightModifier _rotationConstraitBasedFingersWeightModifier;

        [SerializeField] private HandsIKTransform _handsIKFollowers;
        [SerializeField] private HandsRotationConstraintTransforms _handsConstraintsFollowers;
        [Space(5)]
        [SerializeField] private List<MultiRotationConstraintData> _multiRotationConstraints;

        // aiming
        private AimState _aimState = AimState.normal;

        // weapon
        private WeaponBase _currentWeapon;

        // Clip prevention
        [Space]
        [Header("Clip Prevention Settings")]
        [SerializeField] private bool _useClassicClipPrevention = true;
        [SerializeField] private int _detectWeaponClipInterval = 15;
        [Space]
        [SerializeField] private Transform _clipProjector;
        [SerializeField] private Transform _clipVisual;

        [SerializeField] private float _spineOffsetChangeSpeed = 20f;
        [SerializeField] private float _lerpPosChangeSpeed = 4f;
        [SerializeField] private LayerMask _clipCheckMask;

        private WeaponClippedState _weaponClippedState = WeaponClippedState.normal;
        private Vector3 _gunHolderInitialPosition;

        private float _lerpPos;
        private bool _detectionClipped = false;

        [Space]
        [Header("Weapon Switching")]
        [SerializeField] private WeaponBase _primaryWeapon;
        [SerializeField] private WeaponBase _secondaryWeapon;
        [SerializeField] private WeaponBase _sidearmWeapon;
        [SerializeField] private WeaponBase _meleeWeapon;
        [Space(5)]
        [SerializeField] private Slot _rifleSlotOne;
        [SerializeField] private Slot _rifleSlotTwo;
        [SerializeField] private Slot _pistolSlot;
        [SerializeField] private Slot _meleeSlot;
        [Space(2)]
        [SerializeField] private Slot _gunHolderSlot;
        [Space(2)]
        [SerializeField] private FollowTransformPosAndRot _rightHandFollower;
        [Space(2)]
        [SerializeField] private FollowTransformPosAndRot _leftHandFollower;
        [Space(2)]
        [SerializeField] private ConstraintsWeightModifier _handsIKWeightModifier;
        [Space(5)]
        [SerializeField] private float _switchToRifleInputDelay = 0.8f;
        [SerializeField] private float _switchToPistolInputDelay = 0.4f;
        [Space(5)]
        [SerializeField] private int _holsterPistolOrKnifeID = 0;
        [SerializeField] private int _holsterRifleID = 1;
        [SerializeField] private int _switchBetweenPistolOrKnifeID = 2;
        [SerializeField] private int _switchBetweenRifleID = 3;
        [SerializeField] private int _switchFromKnifeOrPistolToRifleID = 4;
        [SerializeField] private int _switchFromRifleToKnifeID = 5;
        [SerializeField] private int _switchFromRifleToPistolID = 6;
        [SerializeField] private int _unHolsterPistolOrKnifeID = 7;
        [SerializeField] private int _unHolsterRifleID = 8;
        [SerializeField] private int _swapWeaponID = 9;
        [Space(5)]
        [SerializeField] private bool canHolster = true;
        [Space(5)]
        [Header("Swapping")]
        [SerializeField] private float _dropWeaponForce = 6;
        [SerializeField] private float _pickNewWeaponSmoothTime = 0.05f;
        [SerializeField] private float delayBeforeResetSwitchInputPerformed = 0.25f;
        [Space]
        public UnityEvent<int> OnPerformWeaponSwitchAnimation;
        public UnityEvent<float> OnShoot;
        public UnityEvent<WeaponBase> OnWeaponSet;
        public UnityEvent<bool> OnAimInput;

        private WaitForSeconds _switchToRifleInputWaitForSeconds;
        private WaitForSeconds _switchToPistolInputWaitForSeconds;

        private Vector3 _weaponOffsetPosition = Vector3.zero;

        private float _targetUpperBodyLayerWeight = 1f;

        private bool _isPerformingSwitch = false;
        private bool _switchInputPerformed = false;
        private bool _holstered = false;

        void Awake()
        {
            // CurrentWeaponRef
            _currentWeapon = _primaryWeapon;

            // Set the isCurrentWeapon bool.
            _currentWeapon.SetWeaponData(this, true);
        }

        // Start is called before the first frame update
        void Start()
        {
            OnWeaponSet.Invoke(_currentWeapon);

            _gunHolderInitialPosition = _gunHolder.localPosition;

            // Set the current weapons Hand IK Targets, So we don't have to set them even manually...
            SetAllIKFollowersTargetToTargets(_currentWeapon);

            _switchToRifleInputWaitForSeconds = new WaitForSeconds(_switchToRifleInputDelay);
            _switchToPistolInputWaitForSeconds = new WaitForSeconds(_switchToPistolInputDelay);
        }

        public void UpdateWeaponManager(Vector2 lookInput, float upLimit, float downLimit, float xRotation,
            bool isInInteractionRange, bool shootPressed, bool aimTriggered, bool sprinting, bool switchToRifleTriggered,
            bool switchToPistolTriggered, bool switchToKnifeTriggered, bool holsterTriggered, bool reloadTriggered)
        {
            _currentWeapon.UpdateWeapon(lookInput, upLimit, downLimit, xRotation);

            HandleShooting(shootPressed);
            HandleAiming(aimTriggered, sprinting);
            HandleWeaponSwitching(switchToRifleTriggered, switchToPistolTriggered, switchToKnifeTriggered, holsterTriggered);

            HandleReloadingAndLeftHandIK(reloadTriggered, shootPressed, isInInteractionRange);

            if (Time.frameCount % _detectWeaponClipInterval == 0 && _useClassicClipPrevention) DetectWeaponClip();

            UpdateClippedValues(xRotation);
        }

        public void OnSwap()
        {
            UpdateSwitchingInputPerformed(true);
            UpdateSwitchingState(true);
            MoveWeaponToRightHandSlot(false);

            OnPerformWeaponSwitchAnimation.Invoke(_swapWeaponID);
        }

        public void OnPickNewWeapon(WeaponBase interactedWeapon)
        {
            _currentWeapon = interactedWeapon;
            _currentWeapon.SetWeaponData(this, true);

            SetAllIKFollowersTargetToTargets(_currentWeapon);

            UpdateSwitchingInputPerformed(false);
            UpdateSwitchingState(false);

            SnapCurrentWeaponToGunSlot(_pickNewWeaponSmoothTime);

            OnWeaponSet.Invoke(_currentWeapon);
        }

        #endregion

        #region Weapon Management

        // Handle shooting
        private void HandleShooting(bool shootPressed)
        {
            //if (Input.Player.Shoot.IsPressed() && CanShoot())
            if (shootPressed && CanShoot())
            {
                // Try Shooting.
                Shoot();
            }
        }

        private bool CanShoot()
        {
            //return !IsClipped() && !_switchInputPerformed && !_holstered;
            return !IsClipped() && !_switchInputPerformed && !_isPerformingSwitch && !_holstered;
        }

        // Shoot!
        private void Shoot()
        {
            //bool didShoot = _currentWeapon.Shoot();

            //if (didShoot == false) return;

            ShootResult shootResult = _currentWeapon.Shoot();

            if (shootResult.ShootActionSuccess == false) return;

            // Bump Rotation.
            //_xRotation -= _currentWeapon.WeaponData.ShotBumpRotationAmount;
            if (shootResult.BumpRotationEffect) OnShoot.Invoke(_currentWeapon.WeaponData.ShotBumpRotationAmount);
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
        private void HandleAiming(bool aimTriggered, bool isSprinting)
        {
            // check for input aim.
            //if (Input.Player.Aim.triggered)
            if (aimTriggered && !_holstered)
            {
                // toggle is aim.
                ToggleAimState();
            }

            //if (IsSprinting() && IsAiming()) _aimState = AimState.normal;
            if (isSprinting && IsAiming()) _aimState = AimState.normal;
        }

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

            OnAimInput.Invoke(IsAiming());
        }

        // HandleReloading
        private void HandleReloadingAndLeftHandIK(bool reloadTriggered, bool shootPressed, bool isInInteractionRange)
        {
            // Check if reload input.
            //if (Input.Player.Reload.triggered)
            if (reloadTriggered)
            {
                // Try Reload.
                TryReload(isInInteractionRange);
            }
            //else if (Input.Player.Shoot.IsPressed() && _currentWeapon.CurrentAmmo == 0)
            else if (shootPressed && _currentWeapon.CurrentAmmo == 0)
            {
                // Try Reload.
                TryReload(isInInteractionRange);
            }
        }

        /// <summary>
        /// Call this function when you want to try to reload.
        /// Checks if we can reload, And Relaods if so.
        /// </summary>
        private void TryReload(bool isInInteractionRange)
        {
            if (CanReload(isInInteractionRange))
            {
                _currentWeapon.StartReloading();
            }
        }

        private bool CanReload(bool isInInteractionRange)
        {
            //return !IsClipped() && !_isInInteractionRange && !_switchInputPerformed && !_holstered;
            //return !IsClipped() && !isInInteractionRange && !_switchInputPerformed && !_holstered;
            return !IsClipped() && !isInInteractionRange && !_switchInputPerformed && !_isPerformingSwitch && !_holstered;
        }

        private void DetectWeaponClip()
        {
            _clipProjector.localPosition = _currentWeapon.WeaponData.ClipProjectorPosition;

            _clipVisual.localScale = _weaponClippedState == WeaponClippedState.clipped
                ? _currentWeapon.WeaponData.BoxCastClippedSize
                : _currentWeapon.WeaponData.BoxCastSize;

            _detectionClipped = IsWeaponClipped();

            _weaponClippedState = _detectionClipped ? WeaponClippedState.clipped : WeaponClippedState.normal;
        }

        private void UpdateClippedValues(float xRotation)
        {
            SetSpineConstraintsOffset(_detectionClipped, xRotation);
            UpdateConstraintsWeights();
            UpdateWeaponTransform();
        }

        private bool IsWeaponClipped()
        {
            if (_holstered) return false;

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

        private void SetSpineConstraintsOffset(bool clipped, float xRotation)
        {
            _lerpPos = Mathf.Lerp(_lerpPos, clipped ? 1f : 0f, _lerpPosChangeSpeed * Time.deltaTime);

            float targetOffsetY = clipped ? 0f : _currentWeapon.WeaponData.SpineConstraintOffsetY;

            targetOffsetY = (_holstered || _isPerformingSwitch) ? 0f : targetOffsetY;

            float targetWeight = (_holstered && !_isPerformingSwitch) ? 0 : 1;

            for (int i = 0; i < _multiRotationConstraints.Count; i++)
            {
                Vector3 targetConstraintOffset = Vector3.zero;
                targetConstraintOffset.y = Mathf.Lerp(_multiRotationConstraints[i].MultiRotationConstraint.data.offset.y, targetOffsetY, _spineOffsetChangeSpeed * Time.deltaTime);

                _multiRotationConstraints[i].MultiRotationConstraint.data.offset = _multiRotationConstraints[i].ApplyWeaponRotationOffsetY ? targetConstraintOffset : Vector3.zero;

                float realWeight = Mathf.Lerp(_multiRotationConstraints[i].MultiRotationConstraint.weight, targetWeight * _multiRotationConstraints[i].WeightScale, _spineOffsetChangeSpeed * Time.deltaTime);

                _multiRotationConstraints[i].MultiRotationConstraint.weight = realWeight;

                // Handle Hierarchy Bone if available
                if (_multiRotationConstraints[i].HRotationBone != null)
                {
                    _multiRotationConstraints[i].HRotationBone.rotation = Quaternion.Euler(xRotation * _multiRotationConstraints[i].WeightScale, transform.eulerAngles.y, 0f);
                }
            }
        }

        private void UpdateConstraintsWeights()
        {
            float ikWeight = _currentWeapon.HandsConstraintType == HandsConstraintType.IKBasedFingers ? 1f : 0f;
            float rotationWeight = 1f - ikWeight;
            float ikLerpSpeed = 20f;

            float handsIKWeight;

            if (_isPerformingSwitch || _holstered)
            {
                ikWeight = 0;
                rotationWeight = 0;
                handsIKWeight = 0;
            }
            else
            {
                handsIKWeight = 1;
            }

            _iKBasedFingersWeightModifier.SetWeight(Mathf.Lerp(_iKBasedFingersWeightModifier.GetWeight(), ikWeight, ikLerpSpeed * Time.deltaTime));
            _rotationConstraitBasedFingersWeightModifier.SetWeight(Mathf.Lerp(_rotationConstraitBasedFingersWeightModifier.GetWeight(),
                rotationWeight, ikLerpSpeed * Time.deltaTime));

            _handsIKWeightModifier.SetWeight(Mathf.Lerp(_handsIKWeightModifier.GetWeight(), handsIKWeight, ikLerpSpeed * Time.deltaTime));

            _targetUpperBodyLayerWeight = (_holstered && !_isPerformingSwitch) ? 0 : 1;
        }

        private void UpdateWeaponTransform()
        {
            float offsetPositioningSpeed = 20;

            Vector3 gunHolderLerpPosOffsetPosition = Vector3.Lerp(_gunHolderInitialPosition, _currentWeapon.WeaponData.NewPosition, _lerpPos);
            Vector3 weaponPositionOffset = _currentWeapon.WeaponData.WeaponPositionOffset;

            _weaponOffsetPosition = Vector3.Lerp(_weaponOffsetPosition, weaponPositionOffset, offsetPositioningSpeed * Time.deltaTime);

            _gunHolder.SetLocalPositionAndRotation(gunHolderLerpPosOffsetPosition + weaponPositionOffset,
                Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(_currentWeapon.WeaponData.NewRotation), _lerpPos));
        }

        #endregion

        #region Weapon Switching

        #region General

        private void HandleWeaponSwitching(bool switchToRifleTriggered, bool switchToPistolTriggered, bool switchToKnifeTriggered, bool holsterTriggered)
        {
            // Check Input And Perform Weapon Switching/Holstering.
            if (CanPerformSwitching())
            {
                //if (Input.Player.SwitchToRifle.triggered) StartCoroutine(TrySwitchFromCurrentToRifleDelay());

                //if (Input.Player.SwitchToPistol.triggered) StartCoroutine(TrySwitchFromCurrentToPistolDelay());

                //if (Input.Player.SwitchToKnife.triggered && _currentWeapon.WeaponData.WeaponType != WeaponType.knife) TrySwitchFromCurrentToKnife();

                if (switchToRifleTriggered) StartCoroutine(TrySwitchFromCurrentToRifleDelay());

                if (switchToPistolTriggered) StartCoroutine(TrySwitchFromCurrentToPistolDelay());

                if (switchToKnifeTriggered && _currentWeapon.WeaponData.WeaponType != WeaponType.knife) TrySwitchFromCurrentToKnife();
            }

            if (CanPerformHolstering())
            {
                if (holsterTriggered) TryHolsterOrUnHolsterCurrentWeapon();
            }
        }

        private bool CanPerformSwitching()
        {
            return !_isPerformingSwitch && !_currentWeapon._isReloading && _currentWeapon._gunShotTimer <= 0 && !_holstered;
        }

        private bool CanPerformHolstering()
        {
            return !_switchInputPerformed && !_currentWeapon._isReloading && _currentWeapon._gunShotTimer <= 0 && canHolster;
        }

        #endregion

        #region Weapon Switch Trigger Functions

        private void TrySwitchFromCurrentToKnife()
        {
            UpdateSwitchingState(true);
            UpdateSwitchingInputPerformed(true);

            _currentWeapon.SetWeaponData(this, false);

            MoveWeaponToRightHandSlot(false);

            int animToPlayID = 0;

            switch (_currentWeapon.WeaponData.WeaponType)
            {
                case WeaponType.rifle:
                    animToPlayID = _switchFromRifleToKnifeID;
                    break;
                case WeaponType.pistol:
                    animToPlayID = _switchBetweenPistolOrKnifeID;
                    break;
            }

            PerformWeaponSwitchAnimation(animToPlayID);
        }
        private void TryHolsterOrUnHolsterCurrentWeapon()
        {
            UpdateSwitchingState(true);
            UpdateSwitchingInputPerformed(true);

            if (!_holstered)
            {
                _currentWeapon.SetWeaponData(this, false);

                MoveWeaponToRightHandSlot(false);
            }

            int animToPlayID = 0;

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

            PerformWeaponSwitchAnimation(animToPlayID);
        }

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

            _currentWeapon.SetWeaponData(this, false);

            int animToPlayID = 0;

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

            PerformWeaponSwitchAnimation(animToPlayID);
        }

        private void SwitchFromCurrentToPistol()
        {
            UpdateSwitchingState(true);
            MoveWeaponToRightHandSlot();

            _currentWeapon.SetWeaponData(this, false);

            int animToPlayID = 0;

            switch (_currentWeapon.WeaponData.WeaponType)
            {
                case WeaponType.rifle:
                    animToPlayID = _switchFromRifleToPistolID;
                    break;
                case WeaponType.knife:
                    animToPlayID = _switchBetweenPistolOrKnifeID;
                    break;
            }

            PerformWeaponSwitchAnimation(animToPlayID);
        }

        private IEnumerator SmoothTransformLocalToTarget(Transform localTransform, bool smooth = true,
            Vector3 targetLPosition = default, Vector3 targetLRotation = default, float smoothTime = 0.1f)
        {
            Vector3 localPosition = localTransform.localPosition;
            Quaternion localRotation = localTransform.localRotation;

            float elapsedTime = 0f;

            if (smooth)
            {
                while (elapsedTime < smoothTime)
                {
                    localTransform.localPosition = Vector3.Lerp(localPosition, targetLPosition, elapsedTime / smoothTime);
                    localTransform.localRotation = Quaternion.Slerp(localRotation, Quaternion.Euler(targetLRotation), elapsedTime / smoothTime);

                    elapsedTime += Time.deltaTime;

                    yield return null;
                }
            }

            localTransform.localPosition = targetLPosition;
            localTransform.localRotation = Quaternion.Euler(targetLRotation);
        }

        #endregion

        #region Reusable Functions

        public void UpdateSwitchingState(bool state)
        {
            _isPerformingSwitch = state;
        }

        public void UpdateSwitchingInputPerformed(bool performed)
        {
            _switchInputPerformed = performed;
        }

        private void PerformWeaponSwitchAnimation(int id)
        {
            OnPerformWeaponSwitchAnimation.Invoke(id);
        }

        #endregion

        #region Weapon Child to Hand Slots

        public void MoveWeaponToRightHandSlot(bool smooth = true)
        {
            Quaternion previousWeaponRotation = _currentWeapon.transform.rotation;
            _rightHandFollower.transform.rotation = Quaternion.identity;
            _currentWeapon.transform.rotation = Quaternion.identity;

            Vector3 offset = (_currentWeapon.HandsConstraintType == HandsConstraintType.IKBasedFingers ?
                _currentWeapon.HandsIKTargets.RightHandIKTransform.position :
                _currentWeapon.HandsRotationConstraintTransforms.RightHandIKTransform.position) -
                _currentWeapon.transform.position;

            _currentWeapon.transform.rotation = previousWeaponRotation;

            Quaternion rotation = Quaternion.Inverse(_currentWeapon.transform.rotation) *
                (_currentWeapon.HandsConstraintType == HandsConstraintType.IKBasedFingers ?
                _currentWeapon.HandsIKTargets.RightHandIKTransform.rotation :
                _currentWeapon.HandsRotationConstraintTransforms.RightHandIKTransform.rotation);

            _rightHandFollower.RotationOffset = Quaternion.Inverse(rotation).eulerAngles;

            _rightHandFollower.CalcAndApply();

            _currentWeapon.transform.parent = _rightHandFollower.transform;
            StartCoroutine(SmoothTransformLocalToTarget(_currentWeapon.transform, smooth, -offset, Vector3.zero));
        }

        public void MoveWeaponToLeftHandSlot(bool smooth = true)
        {
            Quaternion previousWeaponRotation = _currentWeapon.transform.rotation;
            _leftHandFollower.transform.rotation = Quaternion.identity;
            _currentWeapon.transform.rotation = Quaternion.identity;

            Vector3 offset = (_currentWeapon.HandsConstraintType == HandsConstraintType.IKBasedFingers ?
                _currentWeapon.HandsIKTargets.LeftHandIKTransform.position :
                _currentWeapon.HandsRotationConstraintTransforms.LeftHandIKTransform.position) -
                _currentWeapon.transform.position;

            _currentWeapon.transform.rotation = previousWeaponRotation;

            Quaternion rotation = Quaternion.Inverse(_currentWeapon.transform.rotation) *
                (_currentWeapon.HandsConstraintType == HandsConstraintType.IKBasedFingers ?
                _currentWeapon.HandsIKTargets.LeftHandIKTransform.rotation :
                _currentWeapon.HandsRotationConstraintTransforms.LeftHandIKTransform.rotation);

            _leftHandFollower.RotationOffset = Quaternion.Inverse(rotation).eulerAngles;

            _leftHandFollower.CalcAndApply();

            _currentWeapon.transform.parent = _leftHandFollower.transform;
            StartCoroutine(SmoothTransformLocalToTarget(_currentWeapon.transform, smooth, -offset, Vector3.zero));
        }

        #endregion

        #endregion

        #region Events

        public void OnWeaponSwitchComplete()
        {
            UpdateSwitchingState(false);
            UpdateSwitchingInputPerformed(false);
            _currentWeapon.SetWeaponData(this, true);
        }

        public void OnWeaponSwitchComplete(float delayBeforeRestInputPerformed)
        {
            if (delayBeforeRestInputPerformed > 0.02f)
            {
                UpdateSwitchingState(false);
                StartCoroutine(RestInputPerformedWithDelay(delayBeforeRestInputPerformed));

                _currentWeapon.SetWeaponData(this, true);
            }
            else
            {
                OnWeaponSwitchComplete();
            }
        }

        public void OnItemUseComplete(ItemType itemType)
        {
            if (itemType != ItemType.RemoteGrenade)
            {
                StartCoroutine(DelayedSwitchInputUpdate(delayBeforeResetSwitchInputPerformed, false));
            }

            UpdateSwitchingState(false);

            if (!_holstered)
            {
                _currentWeapon.SetWeaponData(this, true);

                switch (_currentWeapon.ItemUseBehaviour)
                {
                    case ItemStartUseBehaviour.MoveToLeftHand:
                        SnapCurrentWeaponToGunSlot();
                        break;
                    case ItemStartUseBehaviour.DisableWeapon:
                        _currentWeapon.gameObject.SetActive(true);
                        break;
                }
            }
        }

        private IEnumerator DelayedSwitchInputUpdate(float delay, bool state)
        {
            yield return new WaitForSeconds(delay);

            UpdateSwitchingInputPerformed(state);
        }

        private IEnumerator RestInputPerformedWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            UpdateSwitchingInputPerformed(false);
        }

        public void SnapCurrentWeaponToGunSlot(float smoothTime = 0.2f)
        {
            _gunHolderSlot.SmoothTime = smoothTime;
            _gunHolderSlot.SnapToSocket(_currentWeapon.transform);
        }

        public void SnapCurrentWeaponToRightHandAndAssignIKTargets()
        {
            MoveWeaponToRightHandSlot();
            SetAllIKFollowersTargetToTargets(_currentWeapon);
        }

        public void SwitchWeapon(WeaponSwitchType weaponSwitchType)
        {
            _currentWeapon.SetWeaponData(this, false);

            switch (weaponSwitchType)
            {
                case WeaponSwitchType.SwitchBetweenRifle:
                    SwitchBetweenSlots(_rifleSlotOne, _rifleSlotTwo);
                    break;
                case WeaponSwitchType.SwitchBetweenPistolOrKnife:
                    SwitchBetweenSlots(_pistolSlot, _meleeSlot);
                    break;
                case WeaponSwitchType.SwitchFromPistolOrKnifeToRifle:
                    SnapWeaponToSlotBasedOnPistolOrMeleeType(_currentWeapon.WeaponData.WeaponType, _pistolSlot, _meleeSlot);
                    _currentWeapon = GetWeaponFromSlot(_rifleSlotOne);
                    break;
                case WeaponSwitchType.SwitchFromRifleToPistol:
                    SnapWeaponToAvailableSlotOfTwo(_rifleSlotOne, _rifleSlotTwo);
                    _currentWeapon = GetWeaponFromSlot(_pistolSlot);
                    break;
                case WeaponSwitchType.SwitchFromRifleToKnife:
                    SnapWeaponToAvailableSlotOfTwo(_rifleSlotOne, _rifleSlotTwo);
                    _currentWeapon = GetWeaponFromSlot(_meleeSlot);
                    break;
            }

            //_currentWeapon.SetWeaponData(this, true);

            OnWeaponSet.Invoke(_currentWeapon);
        }

        public void Holster()
        {
            _holstered = true;
            _aimState = AimState.normal;
            _currentWeapon.SetWeaponData(this, false);

            switch (_currentWeapon.WeaponData.WeaponType)
            {
                case WeaponType.rifle:
                    SnapWeaponToAvailableSlotOfTwo(_rifleSlotOne, _rifleSlotTwo);
                    break;
                case WeaponType.pistol:
                case WeaponType.knife:
                    SnapWeaponToSlotBasedOnPistolOrMeleeType(_currentWeapon.WeaponData.WeaponType, _pistolSlot, _meleeSlot);
                    break;
            }
        }

        public void DropCurrentWeapon()
        {
            _currentWeapon.SetWeaponData(this, false);
            _currentWeapon.transform.SetParent(null);

            _currentWeapon.DropWeapon();
            _currentWeapon.AddForceToWeapon(_currentWeapon.transform.forward * _dropWeaponForce, ForceMode.Impulse);
        }

        private void SwitchBetweenSlots(Slot slotOne, Slot slotTwo)
        {
            if (!slotOne.HasObjectInSlot().hasObject)
            {
                slotOne.SnapToSocket(_currentWeapon.transform);
                _currentWeapon = GetWeaponFromSlot(slotTwo);
            }
            else if (!slotTwo.HasObjectInSlot().hasObject)
            {
                slotTwo.SnapToSocket(_currentWeapon.transform);
                _currentWeapon = GetWeaponFromSlot(slotOne);
            }
        }

        private void SnapWeaponToSlotBasedOnPistolOrMeleeType(WeaponType weaponType, Slot pistolSlot, Slot meleeSlot)
        {
            switch (weaponType)
            {
                case WeaponType.pistol:
                    pistolSlot.SnapToSocket(_currentWeapon.transform);
                    break;
                case WeaponType.knife:
                    meleeSlot.SnapToSocket(_currentWeapon.transform);
                    break;
            }
        }

        private void SnapWeaponToAvailableSlotOfTwo(Slot slotOne, Slot slotTwo)
        {
            if (!slotOne.HasObjectInSlot().hasObject) slotOne.SnapToSocket(_currentWeapon.transform);
            else if (!slotTwo.HasObjectInSlot().hasObject) slotTwo.SnapToSocket(_currentWeapon.transform);
        }

        private WeaponBase GetWeaponFromSlot(Slot slot)
        {
            return slot.HasObjectInSlot().objectTransforms[0].GetComponent<WeaponBase>();
        }

        #endregion

        #region Unity Events

        #endregion

        #region Extracts

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

        #endregion

        #region External

        public WeaponBase CurrentWeapon()
        {
            return _currentWeapon;
        }

        public bool IsAiming()
        {
            return _aimState == AimState.aiming;
        }

        public bool Holstered()
        {
            return _holstered;
        }

        public bool IsClipped()
        {
            return _weaponClippedState == WeaponClippedState.clipped;
        }

        public bool SwitchInputPerformed()
        {
            return _switchInputPerformed;
        }

        public bool IsPerformingSwitch()
        {
            return _isPerformingSwitch;
        }

        public float GetUpperBodyWeight() { return _targetUpperBodyLayerWeight; }

        public void OnAddAmmo(int ammoToAdd)
        {
            _currentWeapon.TotalAmmo += ammoToAdd;
        }

        #endregion
    }
}