using UnityEngine;
using UnityEngine.Events;

public class HKPlayerInteraction : HKPlayerInteractionBase
{

    [Space]
    [Header("Weapon Manager")]
    [SerializeField] private HKPlayerWeaponManager _hKPlayerWeaponManager;

    [Space]
    [Header("Interactions")]
    [SerializeField] private Transform _interactPointer;
    [SerializeField] private LayerMask _interactableLayerMask;
    [SerializeField] private float _interactMaxDistance = 1;
    [SerializeField] private int detectInteractableInterval = 30;

    [Space]
    [Header("Events")]
    [Space]
    public UnityEvent OnSwapEvent;
    public UnityEvent<ItemBase, int> OnAddNewItemEvent;
    public UnityEvent<WeaponBase> OnPickNewWeaponEvent;
    public UnityEvent<int> OnAddAmmoEvent;
    public UnityEvent<bool, IInteractable> OnUpdateInteractUIEvent;

    private WeaponBase _interactedNewWeapon = null;
    private IInteractable _detectedInteractable;
    private bool _isInInteractionRange = false;

    public override void UpdatePlayerInteractions(bool reloading, bool interactTriggered)
    {
        if (Time.frameCount % detectInteractableInterval == 0) DetectInteractable(reloading);

        InteractInputCheck(interactTriggered);
    }

    private void DetectInteractable(bool reloading)
    {
        //if (!_currentWeapon._isReloading && Physics.Raycast(_interactPointer.position, _interactPointer.forward,
        //    out RaycastHit hit, _interactMaxDistance, _interactableLayerMask) &&
        //    hit.transform.TryGetComponent(out _detectedInteractable))
        if (!reloading && Physics.Raycast(_interactPointer.position, _interactPointer.forward,
            out RaycastHit hit, _interactMaxDistance, _interactableLayerMask) &&
            hit.transform.TryGetComponent(out _detectedInteractable))
        {
            if (!_detectedInteractable.CanInteract(this)) return;

            HandleInteractUIState(true, _detectedInteractable);
        }
        else
        {
            HandleInteractUIState(false);
        }
    }

    private void InteractInputCheck(bool interactTriggered)
    {
        //if (_isInInteractionRange && Input.Player.Interact.triggered)
        if (_isInInteractionRange && interactTriggered)
        {
            _detectedInteractable.Interact(this);
        }
    }
    private void HandleInteractUIState(bool state, IInteractable interactable = null)
    {
        OnUpdateInteractUIEvent.Invoke(state, interactable);

        _isInInteractionRange = state;
    }

    public override void SwapWeapon(WeaponBase newWeapon)
    {
        _interactedNewWeapon = newWeapon;
        OnSwapEvent.Invoke();
        //animToPlayID = _swapWeaponID;

        //UpdateSwitchingInputPerformed(true);
        //UpdateSwitchingState(true);
        //MoveWeaponToRightHandSlot(false);
        //PerformWeaponSwitchAnimation();
    }

    public override void AddNewItem(ItemBase item, int amount = 1)
    {
        OnAddNewItemEvent.Invoke(item, amount);
    }

    public override void PickNewWeapon()
    {
        OnPickNewWeaponEvent.Invoke(_interactedNewWeapon);
    }

    public override void AddAmmo(int ammoToAdd)
    {
        OnAddAmmoEvent.Invoke(ammoToAdd);
    }

    public override UnityEvent OnSwap()
    {
        return OnSwapEvent;
    }

    public override UnityEvent<ItemBase, int> OnAddNewItem()
    {
        return OnAddNewItemEvent;
    }

    public override UnityEvent<WeaponBase> OnPickNewWeapon()
    {
        return OnPickNewWeaponEvent;
    }

    public override UnityEvent<int> OnAddAmmo()
    {
        return OnAddAmmoEvent;
    }

    public override UnityEvent<bool, IInteractable> OnUpdateInteractUI()
    {
        return OnUpdateInteractUIEvent;
    }

    public override bool IsInInteractionRange()
    {
        return _isInInteractionRange;
    }

    public override WeaponBase CurrentWeapon()
    {
        return _hKPlayerWeaponManager.CurrentWeapon();
    }
}
