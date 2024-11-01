using UnityEngine;

[RequireComponent(typeof(HKPlayerInputBase))]
public class HKPlayerManager : MonoBehaviour
{
    #region Settings

    // Manager Settings
    [Space]
    [Header("Manager System")]
    [SerializeField] private HKPlayerInputBase _hKPlayerInput;
    [SerializeField] private HKPlayerLocomotionCCBase _hKPlayerLocomotion;
    [SerializeField] private HKPlayerInteractionBase _hKPlayerInteraction;
    [SerializeField] private HKPlayerWeaponManager _hKPlayerWeapon;
    [SerializeField] private HKPlayerInventory _hKPlayerInventory;
    [SerializeField] private HKPlayerItemSystem _hKPlayerItemSystem;
    [SerializeField] private HKPlayerHealthSystem _hKPlayerHealthSystem;
    [SerializeField] private HKPlayerAudioEffects _hKPlayerAudioEffects;
    [Space]
    [Header("Anim Event Model")]
    [SerializeField] private HKPlayerAnimEventModel _hKPlayerAnimEventModel;

    #endregion

    #region Main

    void Start()
    {
        InitSystem();
        InitEvents();
    }

    private void InitSystem()
    {
        // Hide And Lock Cursor.
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void InitEvents()
    {
        _hKPlayerAnimEventModel.OnFootStepSound.AddListener(OnPlayerFootLand);
        _hKPlayerAnimEventModel.OnWeaponSwitchComplete.AddListener(_hKPlayerWeapon.OnWeaponSwitchComplete);
        _hKPlayerAnimEventModel.OnHolster.AddListener(_hKPlayerWeapon.Holster);
        _hKPlayerAnimEventModel.OnSwitchWeapon.AddListener(_hKPlayerWeapon.SwitchWeapon);
        _hKPlayerAnimEventModel.OnSnapCurrentWeaponToGunSlot.AddListener(_hKPlayerWeapon.SnapCurrentWeaponToGunSlot);
        _hKPlayerAnimEventModel.OnSnapCurrentWeaponToRightHandAndAssignIKTargets.AddListener(_hKPlayerWeapon.SnapCurrentWeaponToRightHandAndAssignIKTargets);
        _hKPlayerAnimEventModel.OnDropCurrentWeapon.AddListener(_hKPlayerWeapon.DropCurrentWeapon);
        _hKPlayerAnimEventModel.OnPickNewWeapon.AddListener(_hKPlayerInteraction.PickNewWeapon);
        _hKPlayerAnimEventModel.OnThrowItem.AddListener(_hKPlayerItemSystem.OnThrowableThrow);
        _hKPlayerAnimEventModel.OnItemThrowComplete.AddListener(OnItemThrowComplete);

        _hKPlayerLocomotion.OnHitGroundEvent().AddListener(OnHitGround);

        _hKPlayerInteraction.OnSwap().AddListener(_hKPlayerWeapon.OnSwap);
        _hKPlayerInteraction.OnPickNewWeapon().AddListener(_hKPlayerWeapon.OnPickNewWeapon);
        _hKPlayerInteraction.OnAddNewItem().AddListener(_hKPlayerInventory.OnAddNewItem);

        _hKPlayerWeapon.OnPerformWeaponSwitchAnimation.AddListener(_hKPlayerLocomotion.OnPerformWeaponSwitchAnim);
        _hKPlayerWeapon.OnShoot.AddListener(AddXRotation);

        _hKPlayerItemSystem.OnItemStartUse.AddListener(OnCurrentItemStartUse);
        _hKPlayerItemSystem.OnItemReleaseUse.AddListener(OnCurrentItemReleaseUse);
        _hKPlayerItemSystem.OnItemUseCanceled.AddListener(OnCurrentItemCancelUse);
        _hKPlayerItemSystem.OnItemUseComplete.AddListener(OnCurrentItemUseComplete);
        _hKPlayerItemSystem.OnAddHealth.AddListener(_hKPlayerHealthSystem.AddHealth);

        _hKPlayerHealthSystem.OnDeductHealth.AddListener(_hKPlayerLocomotion.OnPlayerDamage);
        _hKPlayerHealthSystem.OnDeath.AddListener(OnPlayerDeath);
        _hKPlayerHealthSystem.OnRevive.AddListener(OnPlayerRevive);

        // Audio Callbacks
        _hKPlayerAnimEventModel.OnThrowItem.AddListener(_hKPlayerAudioEffects.OnPlayerThrowObj);

        _hKPlayerLocomotion.OnJumpEvent().AddListener(_hKPlayerAudioEffects.OnPlayerJump);
        _hKPlayerLocomotion.OnHitGroundEvent().AddListener(_hKPlayerAudioEffects.OnPlayerLand);

        _hKPlayerHealthSystem.OnDeductHealth.AddListener(_hKPlayerAudioEffects.OnPlayerDamage);
        _hKPlayerHealthSystem.OnDeath.AddListener(_hKPlayerAudioEffects.OnPlayerDeath);
        _hKPlayerHealthSystem.OnDeath.AddListener(_hKPlayerAudioEffects.StopAllAudio);
        _hKPlayerWeapon.OnWeaponSet.AddListener(_hKPlayerAudioEffects.OnPlayerSwitchWeapon);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSystems();
    }

    private void UpdateSystems()
    {
        #region System Temp

        #region Classes

        PlayerInputActions inputActions = _hKPlayerInput.GetInputActions();
        InventoryItem currentInventoryItem = _hKPlayerInventory.CurrentInventoryItem();

        #endregion

        #region Input

        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        Vector2 lookInput = inputActions.Player.Look.ReadValue<Vector2>();

        float leanInput = inputActions.Player.Lean.ReadValue<float>();

        bool jumpPressed = inputActions.Player.Jump.IsPressed();
        bool shootPressed = inputActions.Player.Shoot.IsPressed();

        bool sprintTriggered = inputActions.Player.Sprint.triggered;
        bool hasStamina = _hKPlayerHealthSystem.HasStamina();
        bool crouchTriggered = inputActions.Player.Crouch.triggered;
        bool proneTriggered = inputActions.Player.Prone.triggered;
        bool interactTriggered = inputActions.Player.Interact.triggered;
        bool aimTriggered = inputActions.Player.Aim.triggered;

        bool switchToRifleTriggered = inputActions.Player.SwitchToRifle.triggered;
        bool switchToPistolTriggered = inputActions.Player.SwitchToPistol.triggered;
        bool switchToKnifeTriggered = inputActions.Player.SwitchToKnife.triggered;

        bool holsterTriggered = inputActions.Player.Holster.triggered;
        bool reloadTriggered = inputActions.Player.Reload.triggered;

        bool switchInputPerformed = _hKPlayerWeapon.SwitchInputPerformed();

        bool useItemTriggered = inputActions.Player.UseItem.triggered;
        bool useItemReleased = inputActions.Player.UseItem.WasReleasedThisFrame();
        bool useItemPressed = inputActions.Player.UseItem.IsPressed();

        bool openItemWheelTriggered = inputActions.Player.OpenItemWheel.triggered;
        bool openItemWheelReleased = inputActions.Player.OpenItemWheel.WasReleasedThisFrame();
        bool cancelUseItemTriggered = inputActions.Player.CancelUseItem.triggered;

        bool detonateTriggered = inputActions.Player.Detonate.triggered;
        bool reviveTriggered = inputActions.Player.RevivePlayer.triggered;

        #endregion

        #region System State

        float lookUpLimit = _hKPlayerLocomotion.GetLookUpLimit();
        float lookDownLimit = _hKPlayerLocomotion.GetLookDownLimit();
        float xRotation = _hKPlayerLocomotion.GetXRotation();
        float targetUpperBodyLayerWeight = _hKPlayerWeapon.GetUpperBodyWeight();

        bool interactionRange = _hKPlayerInteraction.IsInInteractionRange();
        bool reloading = _hKPlayerWeapon.CurrentWeapon()._isReloading;
        bool holstered = _hKPlayerWeapon.Holstered();
        bool clipped = _hKPlayerWeapon.IsClipped();
        bool sprinting = _hKPlayerLocomotion.IsSprinting();
        bool isCurrentWeaponRifle = _hKPlayerWeapon.CurrentWeapon().WeaponData.WeaponType == WeaponType.rifle;
        bool performingSwitch = _hKPlayerWeapon.IsPerformingSwitch();

        #endregion

        #endregion

        #region Update Systems

        if (_hKPlayerHealthSystem.GetHealthState() == HKPlayerHealthSystem.HealthState.Alive)
        {
            _hKPlayerLocomotion.MovePlayer(moveInput, lookInput, targetUpperBodyLayerWeight, jumpPressed, sprintTriggered,
                                           hasStamina, crouchTriggered, proneTriggered, isCurrentWeaponRifle);

            _hKPlayerLocomotion.UpdateRotation(lookInput, moveInput, leanInput, interactionRange, reloading, holstered, performingSwitch, clipped);

            _hKPlayerWeapon.UpdateWeaponManager(lookInput, lookUpLimit, lookDownLimit, xRotation,
                                                _hKPlayerInteraction.IsInInteractionRange(), shootPressed,
                                                aimTriggered, sprinting, switchToRifleTriggered, switchToPistolTriggered,
                                                switchToKnifeTriggered, holsterTriggered, reloadTriggered);

            _hKPlayerInteraction.UpdatePlayerInteractions(reloading, interactTriggered);

            _hKPlayerItemSystem.UpdateItemSystem(switchInputPerformed, performingSwitch, reloading, currentInventoryItem, useItemTriggered,
                useItemReleased, useItemPressed, openItemWheelTriggered, openItemWheelReleased, cancelUseItemTriggered,
                detonateTriggered);

            _hKPlayerAudioEffects.UpdateAudioSystem(sprinting);
        }

        _hKPlayerHealthSystem.UpdateHealthSystem(sprinting, reviveTriggered);
        #endregion
    }

    #region Item CallBacks

    private void OnCurrentItemStartUse(ItemBase item)
    {
        UpdateSwitchingStateTrue();

        switch (_hKPlayerWeapon.CurrentWeapon().ItemUseBehaviour)
        {
            case ItemStartUseBehaviour.MoveToLeftHand:
                MoveWeaponToLeftHandSlot();
                break;
            case ItemStartUseBehaviour.DisableWeapon:
                DisableCurrentWeapon();
                break;
        }

        _hKPlayerLocomotion.OnHoldItemAnim(item.ItemID);
    }

    private void OnCurrentItemReleaseUse(ItemBase item)
    {
        UpdateSwitchingInputStateTrue();

        _hKPlayerLocomotion.OnPerformItemAnim(item.ItemID);
    }

    private void OnCurrentItemCancelUse(ItemBase item)
    {
        _hKPlayerLocomotion.OnCancelItemAnim();

        _hKPlayerWeapon.UpdateSwitchingState(false);

        if (!_hKPlayerWeapon.Holstered())
        {
            //_hKPlayerWeapon.SnapCurrentWeaponToGunSlot();
            switch (_hKPlayerWeapon.CurrentWeapon().ItemUseBehaviour)
            {
                case ItemStartUseBehaviour.MoveToLeftHand:
                    _hKPlayerWeapon.SnapCurrentWeaponToGunSlot();
                    break;
                case ItemStartUseBehaviour.DisableWeapon:
                    EnableCurrentWeapon();
                    break;
            }
        }
    }

    private void OnCurrentItemUseComplete(ItemBase item)
    {
        UpdateSwitchingInputStateFalse();
    }

    #endregion

    #endregion

    #region Event Callbacks

    private void AddXRotation(float xRotToAdd)
    {
        // We are using - (negative) because thats what makes the rotation look up..
        _hKPlayerLocomotion.SetXRotation(_hKPlayerLocomotion.GetXRotation() - xRotToAdd);
    }

    private void UpdateSwitchingInputStateTrue()
    {
        _hKPlayerWeapon.UpdateSwitchingInputPerformed(true);
    }

    private void UpdateSwitchingInputStateFalse()
    {
        _hKPlayerWeapon.UpdateSwitchingInputPerformed(false);
    }

    private void UpdateSwitchingStateTrue()
    {
        _hKPlayerWeapon.UpdateSwitchingState(true);
    }

    private void OnItemThrowComplete()
    {
        _hKPlayerWeapon.OnItemUseComplete(_hKPlayerInventory.CurrentInventoryItem().Item.ItemType);
    }

    private void MoveWeaponToLeftHandSlot()
    {
        if (!_hKPlayerWeapon.Holstered()) _hKPlayerWeapon.MoveWeaponToLeftHandSlot();
    }

    private void DisableCurrentWeapon()
    {
        if (!_hKPlayerWeapon.Holstered()) _hKPlayerWeapon.CurrentWeapon().gameObject.SetActive(false);
    }

    private void EnableCurrentWeapon()
    {
        _hKPlayerWeapon.CurrentWeapon().gameObject.SetActive(true);
    }

    private void OnHitGround(float velocity)
    {
        _hKPlayerHealthSystem.OnHitTheGround(velocity);
    }

    private void OnPlayerFootLand()
    {
        if (_hKPlayerInput.GetInputActions().Player.Move.ReadValue<Vector2>() != Vector2.zero)
        {
            _hKPlayerAudioEffects.OnPlayerFootLand();
        }
    }

    private void OnPlayerDeath()
    {
        // Player just died
        _hKPlayerItemSystem.OnCloseItemWheel.Invoke();
        _hKPlayerLocomotion.OnDeath();
    }

    private void OnPlayerRevive()
    {
        // Player just revived
        _hKPlayerLocomotion.OnRevive();
    }

    #endregion

    #region player Camera

    private void LateUpdate()
    {
        _hKPlayerLocomotion.MoveCamera(_hKPlayerWeapon.CurrentWeapon().WeaponData.CameraClippedPosition,
            _hKPlayerWeapon.CurrentWeapon().WeaponData.CameraAimingPosition,
            _hKPlayerWeapon.CurrentWeapon().WeaponData.CameraNormalPosition,
            _hKPlayerWeapon.Holstered(), _hKPlayerWeapon.IsPerformingSwitch(), _hKPlayerWeapon.IsClipped(),
            _hKPlayerWeapon.IsAiming());
    }

    #endregion
}