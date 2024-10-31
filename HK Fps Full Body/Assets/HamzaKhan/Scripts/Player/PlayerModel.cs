using UnityEngine;
using UnityEngine.Events;

// CHANGES ARE IN !REGIONS
public class PlayerModel : MonoBehaviour
{

    [Space]
    [Header("Main Settings")]
    [SerializeField] private AudioClip _footAudioClip;

    // CHANGES (
    [Space]
    public HKPlayerInput _playerInput;
    [Space]
    public UnityEvent OnWeaponSwitchComplete;
    [Space]
    public UnityEvent OnHolster;
    [Space]
    public UnityEvent<WeaponSwitchType> OnSwitchWeapon;
    [Space]
    public UnityEvent<float> OnSnapCurrentWeaponToGunSlot;
    [Space]
    public UnityEvent OnSnapCurrentWeaponToRightHandAndAssignIKTargets;
    [Space]
    public UnityEvent OnDropCurrentWeapon;
    [Space]
    public UnityEvent OnPickNewWeapon;
    [Space]
    public UnityEvent OnThrowItem;
    [Space]
    public UnityEvent OnItemThrowComplete;
    // CHANGES )

    // Animation Events
    #region Events

    /// <summary>
    /// Plays foot step sound.
    /// </summary>
    private void PlayFootSound()
    {
        // check if there is some movement input.
        if (_playerInput.GetInputActions().Player.Move.ReadValue<Vector2>() != Vector2.zero)
        {
            // play the foot audio clip
            AudioSource.PlayClipAtPoint(_footAudioClip, transform.position, 1f);
        }
    }

    private void WeaponSwitchComplete()
    {
        // CHANGES (
        //_playerController.OnWeaponSwitchComplete();

        OnWeaponSwitchComplete.Invoke();
        // CHANGES )
    }

    private void Holster()
    {
        // CHANGES (
        //_playerController.Holster();

        OnHolster.Invoke();
        // CHANGES )
    }

    private void SwitchWeapon(WeaponSwitchType weaponSwitchType)
    {
        // CHANGES (
        //_playerController.SwitchWeapon(weaponSwitchType);

        OnSwitchWeapon.Invoke(weaponSwitchType);
        // CHANGES )
    }

    private void SnapCurrentWeaponToGunSlot()
    {
        // CHANGES (
        //_playerController.SnapCurrentWeaponToGunSlot();

        OnSnapCurrentWeaponToGunSlot.Invoke(0.2f);
        // CHANGES )
    }

    private void SnapCurrentWeaponToRightHandAndAssignIKTargets()
    {
        // CHANGES (
        //_playerController.SnapCurrentWeaponToRightHandAndAssignIKTargets();

        OnSnapCurrentWeaponToRightHandAndAssignIKTargets.Invoke();
        // CHANGES )
    }

    private void DropCurrentWeapon()
    {
        // CHANGES (
        //_playerController.DropCurrentWeapon();

        OnDropCurrentWeapon.Invoke();
        // CHANGES )
    }

    private void PickNewWeapon()
    {
        // CHANGES (
        //_playerController.PickNewWeapon();

        OnPickNewWeapon.Invoke();
        // CHANGES )
    }

    #endregion

    public void ThrowGrenadeAndExplode()
    {
        //_playerController.ThrowGrenadeAndExplode();
    }

    public void EndGrenadeThrow()
    {
        //_playerController.EndThrowGrenade();
        OnItemThrowComplete.Invoke();
    }

    public void ThrowItem()
    {
        //_playerController.ThrowGrenade();
        OnThrowItem?.Invoke();
    }
}