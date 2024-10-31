using UnityEngine;
using UnityEngine.Events;

public class HKPlayerAnimEventModel : MonoBehaviour
{

    //[Space]
    //[Header("Main Settings")]
    //[SerializeField] private AudioClip _footAudioClip;
    //[Space]
    //public HKPlayerInput _playerInput;
    //[Space]
    [Space]
    [Header("Events")]
    [Space]
    public UnityEvent OnFootStepSound;
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

    // Animation Events
    #region Events

    /// <summary>
    /// Plays foot step sound.
    /// </summary>
    private void PlayFootSound()
    {
        OnFootStepSound.Invoke();

        //// check if there is some movement input.
        //if (_playerInput.GetInputActions().Player.Move.ReadValue<Vector2>() != Vector2.zero)
        //{
        //    // play the foot audio clip
        //    AudioSource.PlayClipAtPoint(_footAudioClip, transform.position, 1f);
        //}
    }

    private void WeaponSwitchComplete() { OnWeaponSwitchComplete.Invoke(); }

    private void Holster() { OnHolster.Invoke(); }

    private void SwitchWeapon(WeaponSwitchType weaponSwitchType) { OnSwitchWeapon.Invoke(weaponSwitchType); }

    private void SnapCurrentWeaponToGunSlot() { OnSnapCurrentWeaponToGunSlot.Invoke(0.2f); }

    private void SnapCurrentWeaponToRightHandAndAssignIKTargets() { OnSnapCurrentWeaponToRightHandAndAssignIKTargets.Invoke(); }

    private void DropCurrentWeapon() { OnDropCurrentWeapon.Invoke(); }

    private void PickNewWeapon() { OnPickNewWeapon.Invoke(); }

    #endregion

    public void ThrowGrenadeAndExplode()
    {
        //_playerController.ThrowGrenadeAndExplode();
    }

    public void EndGrenadeThrow() { OnItemThrowComplete.Invoke(); }

    public void ThrowItem() { OnThrowItem?.Invoke(); }
}